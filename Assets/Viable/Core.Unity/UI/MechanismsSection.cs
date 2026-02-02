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
        [SerializeField] private TMP_Dropdown topologyDropdown;
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

            // Populate dropdowns
            PopulateAllDropdowns();

            Debug.Log("[MechanismsSection] Dropdowns populated");

            // Wire change listeners
            if (topologyDropdown != null)
                topologyDropdown.onValueChanged.AddListener(_ => OnDropdownChanged());

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
            // Assuming you have these dropdowns (adjust names to match your actual fields):
            if (topologyDropdown != null)
                topologyDropdown.SetValueWithoutNotify((int)cfg.Topology);

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
            PopulateTopologyDropdown();
            PopulateBoundaryDropdown();
            PopulateInflowDropdown();
            PopulateDiffusionDropdown();
            PopulateViabilityDropdown();
            PopulatePhaseSetDropdown();
        }

        #region Populate Dropdowns

        private void PopulateTopologyDropdown()
        {
            if (topologyDropdown == null)
            {
                Debug.LogError("[MechanismsSection] topologyDropdown is NULL!");
                return;
            }
            
            Debug.Log($"[MechanismsSection] Populating Topology dropdown (name: {topologyDropdown.name}, current options: {topologyDropdown.options.Count})");
            topologyDropdown.ClearOptions();
            topologyDropdown.AddOptions(new List<string>
            {
                "Full Domain",
                "Masked Domain"
            });
            Debug.Log($"[MechanismsSection] Topology dropdown populated with {topologyDropdown.options.Count} options:");
            foreach (var opt in topologyDropdown.options)
            {
                Debug.Log($"  - {opt.text}");
            }
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
            if (topologyDropdown != null)
                topologyDropdown.value = (int)config.Topology;

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
            if (topologyDropdown != null)
                config.Topology = (Configuration.TopologyMode)topologyDropdown.value;

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
            if (currentConfig != null)
            {
                // Apply changes immediately to working config
                ApplyEdits(currentConfig);
                UpdateMechanismSummary();

                // Notify listeners (to refresh detail sections)
                OnMechanismChanged?.Invoke();
            }
        }

        private void UpdateMechanismSummary()
        {
            if (mechanismSummaryText != null && currentConfig != null)
            {
                // Use "•" bullet separator per original design
                mechanismSummaryText.text = 
                    $"Topology: {GetFriendlyName(currentConfig.Topology)} • " +
                    $"Boundary: {GetFriendlyName(currentConfig.Boundary)} • " +
                    $"Inflow: {GetFriendlyName(currentConfig.Inflow)} • " +
                    $"Diffusion: {GetFriendlyName(currentConfig.Diffusion)} • " +
                    $"Viability: {GetFriendlyName(currentConfig.ViabilityRule)}";
            }
        }

        private string GetFriendlyName(Configuration.TopologyMode mode)
        {
            return mode == Configuration.TopologyMode.FullDomain ? "FullDomain" : "MaskedDomain";
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
