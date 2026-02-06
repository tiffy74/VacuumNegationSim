using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Viable.Core.Unity.Configuration;
using Viable.Core.Unity.Controllers;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Mechanisms section - always visible in Setup tab.
    /// Contains dropdowns for all mechanism modes.
    /// Updates mechanism summary text in parent RightDockUI.
    /// </summary>
    public class MechanismsSection : CollapsibleSection
    {
        [Header("Mechanism Dropdowns")]
        [SerializeField] private TMP_Dropdown gridTopologyDropdown; // NEW: Grid tessellation (Rect/Tri/Hex)
        [SerializeField] private TMP_Dropdown domainModeDropdown;   // RENAMED: Was topologyDropdown
        [SerializeField] private TMP_Dropdown boundaryDropdown;
        [SerializeField] private TMP_Dropdown inflowDropdown;
        [SerializeField] private TMP_Dropdown diffusionDropdown;
        [SerializeField] private TMP_Dropdown viabilityDropdown;
        [SerializeField] private TMP_Dropdown phaseSetDropdown;

        [Header("External References")]
        [SerializeField] private TextMeshProUGUI mechanismSummaryText;

        [Header("Orchestrator")]
        [SerializeField] private SimulationUIOrchestrator orchestrator;

        [Header("Events")]
        [Tooltip("Invoked when any mechanism dropdown changes - use to refresh detail sections")]
        public UnityEvent OnMechanismChanged = new UnityEvent();

        private Configuration.WorkingScenarioConfig currentConfig;

        protected override void Start()
        {
            base.Start();

            Debug.Log("[MechanismsSection] Start() called - populating dropdowns");

            // Find orchestrator if not assigned
            if (orchestrator == null)
            {
                orchestrator = FindFirstObjectByType<SimulationUIOrchestrator>();
                if (orchestrator != null)
                {
                    Debug.Log("[MechanismsSection] Found orchestrator");
                }
                else
                {
                    Debug.LogWarning("[MechanismsSection] Orchestrator not found!");
                }
            }

            // Populate dropdowns
            PopulateAllDropdowns();

            Debug.Log("[MechanismsSection] Dropdowns populated");

            // Wire change listeners
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

        /// <summary>
        /// Refresh UI controls from WorkingScenarioConfig.
        /// Called by orchestrator after preset load.
        /// </summary>
        public void Refresh(WorkingScenarioConfig cfg)
        {
            // Check if we're in a refresh cycle to avoid loops
            if (orchestrator != null && orchestrator.IsRefreshing())
                return;

            // Set dropdown values WITHOUT triggering OnValueChanged events
            if (gridTopologyDropdown != null)
                gridTopologyDropdown.SetValueWithoutNotify((int)cfg.GridTopology); // NEW: Set grid tessellation

            if (domainModeDropdown != null)
                domainModeDropdown.SetValueWithoutNotify((int)cfg.Domain); // RENAMED: Set domain masking

            if (boundaryDropdown != null)
                boundaryDropdown.SetValueWithoutNotify((int)cfg.Boundary);

            if (inflowDropdown != null)
                inflowDropdown.SetValueWithoutNotify((int)cfg.Inflow);

            if (diffusionDropdown != null)
                diffusionDropdown.SetValueWithoutNotify((int)cfg.Diffusion);

            if (viabilityDropdown != null)
                viabilityDropdown.SetValueWithoutNotify((int)cfg.ViabilityRule);
        }
        private void OnEnable()
        {
            // Populate dropdowns when section becomes visible
            // This handles the case where Start() hasn't been called yet because GameObject was inactive
            Debug.Log("[MechanismsSection] OnEnable() called");
            PopulateAllDropdowns();
        }

        private void PopulateAllDropdowns()
        {
            PopulateGridTopologyDropdown(); // NEW: Populate grid tessellation dropdown
            PopulateDomainModeDropdown();   // RENAMED: Was PopulateTopologyDropdown
            PopulateBoundaryDropdown();
            PopulateInflowDropdown();
            PopulateDiffusionDropdown();
            PopulateViabilityDropdown();
            PopulatePhaseSetDropdown();
        }

        #region Populate Dropdowns

        private void PopulateGridTopologyDropdown()
        {
            if (gridTopologyDropdown == null)
            {
                Debug.LogError("[MechanismsSection] gridTopologyDropdown is NULL!");
                return;
            }
            
            Debug.Log($"[MechanismsSection] Populating GridTopology dropdown (name: {gridTopologyDropdown.name})");
            gridTopologyDropdown.ClearOptions();
            gridTopologyDropdown.AddOptions(new List<string>
            {
                "Rectangular Grid (4-neighbor)",
                "Triangular Grid (3 or 6-neighbor)",
                "Hexagonal Grid (6-neighbor)"
            });
            Debug.Log($"[MechanismsSection] GridTopology dropdown populated with {gridTopologyDropdown.options.Count} options");
        }

        private void PopulateDomainModeDropdown()
        {
            if (domainModeDropdown == null)
            {
                Debug.LogError("[MechanismsSection] domainModeDropdown is NULL!");
                return;
            }
            
            Debug.Log($"[MechanismsSection] Populating DomainMode dropdown (name: {domainModeDropdown.name})");
            domainModeDropdown.ClearOptions();
            domainModeDropdown.AddOptions(new List<string>
            {
                "Full Domain",
                "Masked Domain"
            });
            Debug.Log($"[MechanismsSection] DomainMode dropdown populated with {domainModeDropdown.options.Count} options");
        }

        private void PopulateBoundaryDropdown()
        {
            if (boundaryDropdown == null)
            {
                Debug.LogError("[MechanismsSection] boundaryDropdown is NULL!");
                return;
            }
            
            Debug.Log($"[MechanismsSection] Populating Boundary dropdown (name: {boundaryDropdown.name}, current options: {boundaryDropdown.options.Count})");
            boundaryDropdown.ClearOptions();
            boundaryDropdown.AddOptions(new List<string>
            {
                "Closed (Reflective)",
                "Open (Absorbing)",
                "Wrap (Periodic)"
            });
            Debug.Log($"[MechanismsSection] Boundary dropdown populated with {boundaryDropdown.options.Count} options:");
            foreach (var opt in boundaryDropdown.options)
            {
                Debug.Log($"  - {opt.text}");

            }
        }

        private void PopulateInflowDropdown()
        {
            if (inflowDropdown == null)
            {
                Debug.LogError("[MechanismsSection] inflowDropdown is NULL!");
                return;
            }
            
            Debug.Log($"[MechanismsSection] Populating Inflow dropdown (name: {inflowDropdown.name}, current options: {inflowDropdown.options.Count})");
            inflowDropdown.ClearOptions();
            inflowDropdown.AddOptions(new List<string>
            {
                "Uniform Field",
                "Point Sources",
                "Edge Sources"
            });
            Debug.Log($"[MechanismsSection] Inflow dropdown populated with {inflowDropdown.options.Count} options:");
            foreach (var opt in inflowDropdown.options)
            {
                Debug.Log($"  - {opt.text}");
            }
        }

        private void PopulateDiffusionDropdown()
        {
            if (diffusionDropdown == null)
            {
                Debug.LogError("[MechanismsSection] diffusionDropdown is NULL!");
                return;
            }
            
            Debug.Log($"[MechanismsSection] Populating Diffusion dropdown (name: {diffusionDropdown.name}, current options: {diffusionDropdown.options.Count})");
            diffusionDropdown.ClearOptions();
            diffusionDropdown.AddOptions(new List<string>
            {
                "Von Neumann (4-neighbor)",
                "Moore (8-neighbor)",
                "Anisotropic"
            });
            Debug.Log($"[MechanismsSection] Diffusion dropdown populated with {diffusionDropdown.options.Count} options:");
            foreach (var opt in diffusionDropdown.options)
            {
                Debug.Log($"  - {opt.text}");
            }
        }

        private void PopulateViabilityDropdown()
        {
            if (viabilityDropdown == null)
            {
                Debug.LogError("[MechanismsSection] viabilityDropdown is NULL!");
                return;
            }
            
            Debug.Log($"[MechanismsSection] Populating Viability dropdown (name: {viabilityDropdown.name}, current options: {viabilityDropdown.options.Count})");
            viabilityDropdown.ClearOptions();
            viabilityDropdown.AddOptions(new List<string>
            {
                "Simple Threshold",
                "Hysteresis"
            });
            Debug.Log($"[MechanismsSection] Viability dropdown populated with {viabilityDropdown.options.Count} options:");
            foreach (var opt in viabilityDropdown.options)
            {
                Debug.Log($"  - {opt.text}");
            }
        }

        private void PopulatePhaseSetDropdown()
        {
            if (phaseSetDropdown == null)
            {
                Debug.LogError("[MechanismsSection] phaseSetDropdown is NULL!");
                return;
            }
            
            Debug.Log($"[MechanismsSection] Populating PhaseSet dropdown (name: {phaseSetDropdown.name}, current options: {phaseSetDropdown.options.Count})");
            phaseSetDropdown.ClearOptions();
            phaseSetDropdown.AddOptions(new List<string>
            {
                "Standard",
                "Custom (Advanced)"
            });
            Debug.Log($"[MechanismsSection] PhaseSet dropdown populated with {phaseSetDropdown.options.Count} options:");
            foreach (var opt in phaseSetDropdown.options)
            {
                Debug.Log($"  - {opt.text}");
            }
        }

        #endregion

        #region IConfigSection Implementation

        public override void Bind(Configuration.WorkingScenarioConfig config)
        {
            currentConfig = config;

            // Set dropdown values from config
            if (gridTopologyDropdown != null)
                gridTopologyDropdown.value = (int)config.GridTopology; // NEW: Set grid tessellation

            if (domainModeDropdown != null)
                domainModeDropdown.value = (int)config.Domain; // RENAMED: Set domain masking

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
            // Mechanisms section is always visible
            gameObject.SetActive(true);
        }

        public override void ApplyEdits(Configuration.WorkingScenarioConfig config)
        {
            // Apply dropdown selections to config
            if (gridTopologyDropdown != null)
            {
                var selectedTopology = (Contracts.TopologyMode)gridTopologyDropdown.value;
                Debug.Log($"[MechanismsSection] ApplyEdits: GridTopology dropdown value={gridTopologyDropdown.value} ? {selectedTopology}");
                config.GridTopology = selectedTopology;
            }

            if (domainModeDropdown != null)
                config.Domain = (Configuration.DomainMode)domainModeDropdown.value; // RENAMED: Apply domain masking

            if (boundaryDropdown != null)
                config.Boundary = (Configuration.BoundaryMode)boundaryDropdown.value;

            if (inflowDropdown != null)
                config.Inflow = (Configuration.InflowMode)inflowDropdown.value;

            if (diffusionDropdown != null)
                config.Diffusion = (Configuration.DiffusionMode)diffusionDropdown.value;

            if (viabilityDropdown != null)
                config.ViabilityRule = (Configuration.ViabilityRuleMode)viabilityDropdown.value;

            // PhaseSetId is string, handle separately
            if (phaseSetDropdown != null && phaseSetDropdown.value == 0)
                config.PhaseSetId = "Standard";
            else
                config.PhaseSetId = "Custom";
        }

        #endregion

        private void OnDropdownChanged()
        {
            // Apply changes to orchestrator's WorkingConfig (THE single source of truth)
            if (orchestrator != null && orchestrator.WorkingConfig != null)
            {
                // Apply changes immediately to orchestrator's working config
                ApplyEdits(orchestrator.WorkingConfig);
                UpdateMechanismSummary();

                // Notify listeners (to refresh detail sections)
                OnMechanismChanged?.Invoke();
                
                Debug.Log($"[MechanismsSection] Updated orchestrator WorkingConfig: GridTopology={orchestrator.WorkingConfig.GridTopology}");
            }
            else if (currentConfig != null)
            {
                // Fallback to local config if orchestrator not available
                ApplyEdits(currentConfig);
                UpdateMechanismSummary();
                OnMechanismChanged?.Invoke();
                
                Debug.LogWarning("[MechanismsSection] Orchestrator not found, using local config (changes may not persist)");
            }
        }

        private void UpdateMechanismSummary()
        {
            // Use orchestrator's WorkingConfig (the single source of truth)
            var config = (orchestrator != null && orchestrator.WorkingConfig != null) 
                ? orchestrator.WorkingConfig 
                : currentConfig;
                
            if (mechanismSummaryText != null && config != null)
            {
                // Use "•" bullet separator per original design
                mechanismSummaryText.text = 
                    $"Grid: {GetFriendlyName(config.GridTopology)} • " + // NEW: Show grid tessellation
                    $"Domain: {GetFriendlyName(config.Domain)} • " +
                    $"Boundary: {GetFriendlyName(config.Boundary)} • " +
                    $"Inflow: {GetFriendlyName(config.Inflow)} • " +
                    $"Diffusion: {GetFriendlyName(config.Diffusion)} • " +
                    $"Viability: {GetFriendlyName(config.ViabilityRule)}";
            }
        }

        private string GetFriendlyName(Contracts.TopologyMode mode) // NEW: Grid tessellation names
        {
            switch (mode)
            {
                case Contracts.TopologyMode.RectGrid: return "Rectangular";
                case Contracts.TopologyMode.TriGrid: return "Triangular";
                case Contracts.TopologyMode.HexGrid: return "Hexagonal";
                default: return mode.ToString();
            }
        }

        private string GetFriendlyName(Configuration.DomainMode mode) // Domain masking names
        {
            return mode == Configuration.DomainMode.FullDomain ? "FullDomain" : "MaskedDomain";
        }

        private string GetFriendlyName(Configuration.BoundaryMode mode)
        {
            switch (mode)
            {
                case Configuration.BoundaryMode.Closed: return "Closed";
                case Configuration.BoundaryMode.Open: return "Open";
                case Configuration.BoundaryMode.Wrap: return "Wrap";
                default: return mode.ToString();
            }
        }

        private string GetFriendlyName(Configuration.InflowMode mode)
        {
            switch (mode)
            {
                case Configuration.InflowMode.UniformField: return "UniformField";
                case Configuration.InflowMode.PointSources: return "PointSources";
                case Configuration.InflowMode.EdgeSources: return "EdgeSources";
                default: return mode.ToString();
            }
        }

        private string GetFriendlyName(Configuration.DiffusionMode mode)
        {
            switch (mode)
            {
                case Configuration.DiffusionMode.VonNeumann4: return "VonNeumann4";
                case Configuration.DiffusionMode.Moore8: return "Moore8";
                case Configuration.DiffusionMode.Anisotropic: return "Anisotropic";
                default: return mode.ToString();
            }
        }

        private string GetFriendlyName(Configuration.ViabilityRuleMode mode)
        {
            return mode == Configuration.ViabilityRuleMode.Simple ? "Simple" : "Hysteresis";
        }
    }
}
