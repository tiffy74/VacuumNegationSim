using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;
using System.Collections.Generic;

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

        [Header("Events")]
        [Tooltip("Invoked when any mechanism dropdown changes - use to refresh detail sections")]
        public UnityEvent OnMechanismChanged = new UnityEvent();

        private Configuration.WorkingScenarioConfig currentConfig;

        protected override void Start()
        {
            base.Start();

            // Populate dropdowns
            PopulateTopologyDropdown();
            PopulateBoundaryDropdown();
            PopulateInflowDropdown();
            PopulateDiffusionDropdown();
            PopulateViabilityDropdown();
            PopulatePhaseSetDropdown();

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

        #region Populate Dropdowns

        private void PopulateTopologyDropdown()
        {
            if (topologyDropdown == null) return;
            topologyDropdown.ClearOptions();
            topologyDropdown.AddOptions(new List<string>
            {
                "Full Domain",
                "Masked Domain"
            });
        }

        private void PopulateBoundaryDropdown()
        {
            if (boundaryDropdown == null) return;
            boundaryDropdown.ClearOptions();
            boundaryDropdown.AddOptions(new List<string>
            {
                "Closed (Reflective)",
                "Open (Absorbing)",
                "Wrap (Periodic)"
            });
        }

        private void PopulateInflowDropdown()
        {
            if (inflowDropdown == null) return;
            inflowDropdown.ClearOptions();
            inflowDropdown.AddOptions(new List<string>
            {
                "Uniform Field",
                "Point Sources",
                "Edge Sources"
            });
        }

        private void PopulateDiffusionDropdown()
        {
            if (diffusionDropdown == null) return;
            diffusionDropdown.ClearOptions();
            diffusionDropdown.AddOptions(new List<string>
            {
                "Von Neumann (4-neighbor)",
                "Moore (8-neighbor)",
                "Anisotropic"
            });
        }

        private void PopulateViabilityDropdown()
        {
            if (viabilityDropdown == null) return;
            viabilityDropdown.ClearOptions();
            viabilityDropdown.AddOptions(new List<string>
            {
                "Simple Threshold",
                "Hysteresis"
            });
        }

        private void PopulatePhaseSetDropdown()
        {
            if (phaseSetDropdown == null) return;
            phaseSetDropdown.ClearOptions();
            phaseSetDropdown.AddOptions(new List<string>
            {
                "Standard",
                "Custom (Advanced)"
            });
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
