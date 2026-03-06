using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Viable.Core.Unity.Configuration;
using Viable.Core.Unity.Controllers;
using Viable.Engine.ExpansionModels;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Mechanisms section - always visible in Setup tab.
    /// Contains dropdowns for all mechanism modes.
    /// Updates mechanism summary text in parent RightDockUI.
    /// Stage 14: Added ExpansionModel dropdown.
    /// </summary>
    public class MechanismsSection : CollapsibleSection
    {
        [Header("Expansion Model")]
        [SerializeField] private TMP_Dropdown expansionModelDropdown;

        [Header("Preset Controls")]
        [SerializeField] private TMP_Dropdown presetDropdown;

        [Header("Mechanism Dropdowns")]
        [SerializeField] private TMP_Dropdown gridTopologyDropdown;
        [SerializeField] private TMP_Dropdown domainModeDropdown;
        [SerializeField] private TMP_Dropdown boundaryDropdown;
        [SerializeField] private TMP_Dropdown inflowDropdown;
        [SerializeField] private TMP_Dropdown diffusionDropdown;
        [SerializeField] private TMP_Dropdown viabilityDropdown;
        [SerializeField] private TMP_Dropdown phaseSetDropdown;

        [Header("External References")]
        [SerializeField] private TextMeshProUGUI mechanismSummaryText;
        [SerializeField] private NotificationUI notificationUI;

        [Header("Orchestrator")]
        [SerializeField] private SimulationUIOrchestrator orchestrator;

        [Header("Events")]
        [Tooltip("Invoked when any mechanism dropdown changes - use to refresh detail sections")]
        public UnityEvent OnMechanismChanged = new UnityEvent();

        private Configuration.WorkingScenarioConfig currentConfig;
        private List<ModelMetadata> _expansionModels;

        protected override void Start()
        {
            base.Start();

            Debug.Log("[MechanismsSection] Start() called - populating dropdowns");

            if (orchestrator == null)
            {
                orchestrator = FindFirstObjectByType<SimulationUIOrchestrator>();
                if (orchestrator != null)
                    Debug.Log("[MechanismsSection] Found orchestrator");
                else
                    Debug.LogWarning("[MechanismsSection] Orchestrator not found!");
            }

            if (notificationUI == null)
                notificationUI = FindFirstObjectByType<NotificationUI>();

            PopulateAllDropdowns();

            Debug.Log("[MechanismsSection] Dropdowns populated");

            // Wire change listeners
            if (expansionModelDropdown != null)
                expansionModelDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());

            if (presetDropdown != null)
            {
                presetDropdown.onValueChanged.RemoveAllListeners();
                presetDropdown.onValueChanged.AddListener(OnPresetDropdownChanged);
            }

            if (gridTopologyDropdown != null)
                gridTopologyDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());

            if (domainModeDropdown != null)
                domainModeDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());

            if (boundaryDropdown != null)
                boundaryDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());

            if (inflowDropdown != null)
                inflowDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());

            if (diffusionDropdown != null)
                diffusionDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());

            if (viabilityDropdown != null)
                viabilityDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());

            if (phaseSetDropdown != null)
                phaseSetDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());
        }

        public void Refresh(WorkingScenarioConfig cfg)
        {
            if (orchestrator != null && orchestrator.IsRefreshing())
                return;

            if (expansionModelDropdown != null && _expansionModels != null)
            {
                int index = _expansionModels.FindIndex(m => (int)m.ModelType == cfg.ExpansionModelTypeInt);
                if (index >= 0)
                    expansionModelDropdown.SetValueWithoutNotify(index);
            }

            if (gridTopologyDropdown != null)
                gridTopologyDropdown.SetValueWithoutNotify(MapTopologyAdjacencyToIndex(cfg.GridTopology, cfg.Adjacency));

            if (domainModeDropdown != null)
                domainModeDropdown.SetValueWithoutNotify((int)cfg.Domain);

            if (boundaryDropdown != null)
                boundaryDropdown.SetValueWithoutNotify((int)cfg.Boundary);

            if (inflowDropdown != null)
                inflowDropdown.SetValueWithoutNotify((int)cfg.Inflow);

            if (diffusionDropdown != null)
                diffusionDropdown.SetValueWithoutNotify((int)cfg.Diffusion);

            if (viabilityDropdown != null)
                viabilityDropdown.SetValueWithoutNotify((int)cfg.ViabilityRule);

            UpdateMechanismSummary();
        }

        private void OnEnable()
        {
            Debug.Log("[MechanismsSection] OnEnable() called");
            PopulateAllDropdowns();
        }

        private void PopulateAllDropdowns()
        {
            PopulateExpansionModelDropdown();
            PopulatePresetDropdown();
            PopulateGridTopologyDropdown();
            PopulateDomainModeDropdown();
            PopulateBoundaryDropdown();
            PopulateInflowDropdown();
            PopulateDiffusionDropdown();
            PopulateViabilityDropdown();
            PopulatePhaseSetDropdown();
        }

        #region Populate Dropdowns

        private void PopulateExpansionModelDropdown()
        {
            if (expansionModelDropdown == null)
            {
                Debug.LogWarning("[MechanismsSection] expansionModelDropdown is NULL - skipping");
                return;
            }

            expansionModelDropdown.ClearOptions();
            _expansionModels = ExpansionModelFactory.GetModelMetadata().ToList();

            if (_expansionModels.Count == 0)
            {
                expansionModelDropdown.AddOptions(new List<string> { "No Models Available" });
                return;
            }

            var options = new List<string>();
            string lastCategory = null;

            foreach (var model in _expansionModels)
            {
                string displayText = model.DisplayName;
                if (model.Category != lastCategory)
                {
                    displayText = $"[{model.Category}] {model.DisplayName}";
                    lastCategory = model.Category;
                }
                options.Add(displayText);
            }

            expansionModelDropdown.AddOptions(options);
        }

        private void PopulatePresetDropdown()
        {
            if (presetDropdown == null)
                return;

            presetDropdown.ClearOptions();
            var presetNames = new List<string>();

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScenarioPreset", new[] { "Assets/Viable/Core.Unity/Presets/Examples" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var preset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScenarioPreset>(path);
                if (preset != null)
                    presetNames.Add(preset.PresetName);
            }
#else
            var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
            presetNames.AddRange(presets.Select(p => p.PresetName));

            if (presetNames.Count == 0)
            {
                presets = Resources.LoadAll<ScenarioPreset>("Presets");
                presetNames.AddRange(presets.Select(p => p.PresetName));
            }
#endif

            if (presetNames.Count == 0)
            {
                presetNames.Add("Default");
            }
            else
            {
                presetNames.Sort();
                int defaultIndex = presetNames.FindIndex(p => 
                    p.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
                    p.StartsWith("00_", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Default"));

                if (defaultIndex > 0)
                {
                    string defaultPreset = presetNames[defaultIndex];
                    presetNames.RemoveAt(defaultIndex);
                    presetNames.Insert(0, defaultPreset);
                }
            }

            presetDropdown.AddOptions(presetNames);
            presetDropdown.value = 0;
            presetDropdown.RefreshShownValue();
        }

        private void OnPresetDropdownChanged(int index)
        {
            StartCoroutine(CloseDropdownDelayed(presetDropdown));
        }

        private System.Collections.IEnumerator CloseDropdownDelayed(TMP_Dropdown dropdown)
        {
            yield return null;
            if (dropdown != null)
            {
                dropdown.Hide();
                var template = dropdown.template;
                if (template != null && template.gameObject.activeSelf)
                    template.gameObject.SetActive(false);
            }
        }

        public void SelectPreset(ScenarioPreset preset)
        {
            if (presetDropdown == null || preset == null) return;

            int index = presetDropdown.options.FindIndex(opt => opt.text == preset.PresetName);
            if (index >= 0)
            {
                presetDropdown.SetValueWithoutNotify(index);
                presetDropdown.RefreshShownValue();
            }
        }

        private void PopulateGridTopologyDropdown()
        {
            if (gridTopologyDropdown == null) return;
            gridTopologyDropdown.ClearOptions();
            gridTopologyDropdown.AddOptions(new List<string>
            {
                "Rectangular (edges)",
                "Rectangular (edges+vertices)",
                "Triangular (edges)",
                "Triangular (edges+vertices)",
                "Hexagonal (edges)",
                "Hexagonal (edges+vertices)"
            });
        }

        private void PopulateDomainModeDropdown()
        {
            if (domainModeDropdown == null) return;
            domainModeDropdown.ClearOptions();
            domainModeDropdown.AddOptions(new List<string> { "Full Domain", "Masked Domain" });
        }

        private void PopulateBoundaryDropdown()
        {
            if (boundaryDropdown == null) return;
            boundaryDropdown.ClearOptions();
            boundaryDropdown.AddOptions(new List<string> { "Closed (Reflective)", "Open (Absorbing)", "Wrap (Periodic)" });
        }

        private void PopulateInflowDropdown()
        {
            if (inflowDropdown == null) return;
            inflowDropdown.ClearOptions();
            inflowDropdown.AddOptions(new List<string> { "Uniform Field", "Point Sources", "Edge Sources" });
        }

        private void PopulateDiffusionDropdown()
        {
            if (diffusionDropdown == null) return;
            diffusionDropdown.ClearOptions();
            diffusionDropdown.AddOptions(new List<string> { "Von Neumann (4-neighbor)", "Moore (8-neighbor)", "Anisotropic" });
        }

        private void PopulateViabilityDropdown()
        {
            if (viabilityDropdown == null) return;
            viabilityDropdown.ClearOptions();
            viabilityDropdown.AddOptions(new List<string> { "Simple Threshold", "Hysteresis" });
        }

        private void PopulatePhaseSetDropdown()
        {
            if (phaseSetDropdown == null) return;
            phaseSetDropdown.ClearOptions();
            phaseSetDropdown.AddOptions(new List<string> { "Standard", "Custom (Advanced)" });
        }

        #endregion

        #region IConfigSection Implementation

        public override void Bind(Configuration.WorkingScenarioConfig config)
        {
            currentConfig = config;

            if (expansionModelDropdown != null && _expansionModels != null)
            {
                int index = _expansionModels.FindIndex(m => (int)m.ModelType == config.ExpansionModelTypeInt);
                if (index >= 0)
                    expansionModelDropdown.value = index;
            }

            if (gridTopologyDropdown != null)
                gridTopologyDropdown.value = MapTopologyAdjacencyToIndex(config.GridTopology, config.Adjacency);

            if (domainModeDropdown != null)
                domainModeDropdown.value = (int)config.Domain;

            if (boundaryDropdown != null)
                boundaryDropdown.value = (int)config.Boundary;

            if (inflowDropdown != null)
                inflowDropdown.value = (int)config.Inflow;

            if (diffusionDropdown != null)
                diffusionDropdown.value = (int)config.Diffusion;

            if (viabilityDropdown != null)
                viabilityDropdown.value = (int)config.ViabilityRule;

            UpdateMechanismSummary();
        }

        public override void RefreshVisibility(Configuration.WorkingScenarioConfig config)
        {
            gameObject.SetActive(true);
        }

        public override void ApplyEdits(Configuration.WorkingScenarioConfig config)
        {
            if (expansionModelDropdown != null && _expansionModels != null && expansionModelDropdown.value < _expansionModels.Count)
            {
                var selectedModel = _expansionModels[expansionModelDropdown.value];
                config.ExpansionModelTypeInt = (int)selectedModel.ModelType;
            }

            if (gridTopologyDropdown != null)
            {
                MapIndexToTopologyAdjacency(gridTopologyDropdown.value, out var topo, out var adj);
                config.GridTopology = topo;
                config.Adjacency = adj;
            }

            if (domainModeDropdown != null)
                config.Domain = (Configuration.DomainMode)domainModeDropdown.value;

            if (boundaryDropdown != null)
                config.Boundary = (Configuration.BoundaryMode)boundaryDropdown.value;

            if (inflowDropdown != null)
                config.Inflow = (Configuration.InflowMode)inflowDropdown.value;

            if (diffusionDropdown != null)
                config.Diffusion = (Configuration.DiffusionMode)diffusionDropdown.value;

            if (viabilityDropdown != null)
                config.ViabilityRule = (Configuration.ViabilityRuleMode)viabilityDropdown.value;

            if (phaseSetDropdown != null)
                config.PhaseSetId = phaseSetDropdown.value == 0 ? "Standard" : "Custom";
        }

        #endregion

        private void OnDropdownChanged()
        {
            if (orchestrator != null && orchestrator.WorkingConfig != null)
            {
                ApplyEdits(orchestrator.WorkingConfig);
                UpdateMechanismSummary();
                OnMechanismChanged?.Invoke();
            }
            else if (currentConfig != null)
            {
                ApplyEdits(currentConfig);
                UpdateMechanismSummary();
                OnMechanismChanged?.Invoke();
            }
        }

        private void UpdateMechanismSummary()
        {
            var config = (orchestrator != null && orchestrator.WorkingConfig != null) 
                ? orchestrator.WorkingConfig 
                : currentConfig;
                
            if (mechanismSummaryText != null && config != null)
            {
                mechanismSummaryText.text = 
                    $"Expansion: {GetFriendlyName(config.ExpansionModelType)} • " +
                    $"Grid: {GetFriendlyName(config.GridTopology)} • " +
                    $"Domain: {GetFriendlyName(config.Domain)} • " +
                    $"Boundary: {GetFriendlyName(config.Boundary)} • " +
                    $"Inflow: {GetFriendlyName(config.Inflow)} • " +
                    $"Diffusion: {GetFriendlyName(config.Diffusion)} • " +
                    $"Viability: {GetFriendlyName(config.ViabilityRule)}";
            }
        }

        private string GetFriendlyName(Contracts.ExpansionModel model)
        {
            // Get the display name from metadata, or use enum name as fallback
            var displayName = _expansionModels?.Find(m => m.ModelType == model)?.DisplayName ?? model.ToString();

            // Clean up the display name for summary (remove "Default: " prefix if present)
            if (displayName.StartsWith("Default: "))
                displayName = displayName.Substring(9);

            // Shorten "Viability Boundary Pressure" to "Viability Boundary" for summary
            if (displayName.Contains("Viability Boundary Pressure"))
                displayName = "Viability Boundary";

            return displayName;
        }

        private string GetFriendlyName(Contracts.TopologyMode mode) => mode switch
        {
            Contracts.TopologyMode.RectGrid => "Rectangular",
            Contracts.TopologyMode.TriGrid => "Triangular",
            Contracts.TopologyMode.HexGrid => "Hexagonal",
            _ => mode.ToString()
        };

        private string GetFriendlyName(Configuration.DomainMode mode) =>
            mode == Configuration.DomainMode.FullDomain ? "FullDomain" : "MaskedDomain";

        private string GetFriendlyName(Configuration.BoundaryMode mode) => mode switch
        {
            Configuration.BoundaryMode.Closed => "Closed",
            Configuration.BoundaryMode.Open => "Open",
            Configuration.BoundaryMode.Wrap => "Wrap",
            _ => mode.ToString()
        };

        private string GetFriendlyName(Configuration.InflowMode mode) => mode switch
        {
            Configuration.InflowMode.UniformField => "UniformField",
            Configuration.InflowMode.PointSources => "PointSources",
            Configuration.InflowMode.EdgeSources => "EdgeSources",
            _ => mode.ToString()
        };

        private string GetFriendlyName(Configuration.DiffusionMode mode) => mode switch
        {
            Configuration.DiffusionMode.VonNeumann4 => "VonNeumann4",
            Configuration.DiffusionMode.Moore8 => "Moore8",
            Configuration.DiffusionMode.Anisotropic => "Anisotropic",
            _ => mode.ToString()
        };

        private string GetFriendlyName(Configuration.ViabilityRuleMode mode) =>
            mode == Configuration.ViabilityRuleMode.Simple ? "Simple" : "Hysteresis";

        private int MapTopologyAdjacencyToIndex(Contracts.TopologyMode topo, Contracts.AdjacencyMode adj) => (topo, adj) switch
        {
            (Contracts.TopologyMode.RectGrid, Contracts.AdjacencyMode.EdgeOnly) => 0,
            (Contracts.TopologyMode.RectGrid, Contracts.AdjacencyMode.EdgeAndVertex) => 1,
            (Contracts.TopologyMode.TriGrid, Contracts.AdjacencyMode.EdgeOnly) => 2,
            (Contracts.TopologyMode.TriGrid, Contracts.AdjacencyMode.EdgeAndVertex) => 3,
            (Contracts.TopologyMode.HexGrid, Contracts.AdjacencyMode.EdgeOnly) => 4,
            (Contracts.TopologyMode.HexGrid, Contracts.AdjacencyMode.EdgeAndVertex) => 5,
            _ => 0
        };

        private void MapIndexToTopologyAdjacency(int index, out Contracts.TopologyMode topo, out Contracts.AdjacencyMode adj)
        {
            (topo, adj) = index switch
            {
                0 => (Contracts.TopologyMode.RectGrid, Contracts.AdjacencyMode.EdgeOnly),
                1 => (Contracts.TopologyMode.RectGrid, Contracts.AdjacencyMode.EdgeAndVertex),
                2 => (Contracts.TopologyMode.TriGrid, Contracts.AdjacencyMode.EdgeOnly),
                3 => (Contracts.TopologyMode.TriGrid, Contracts.AdjacencyMode.EdgeAndVertex),
                4 => (Contracts.TopologyMode.HexGrid, Contracts.AdjacencyMode.EdgeOnly),
                5 => (Contracts.TopologyMode.HexGrid, Contracts.AdjacencyMode.EdgeAndVertex),
                _ => (Contracts.TopologyMode.RectGrid, Contracts.AdjacencyMode.EdgeOnly)
            };
        }
    }
}

