using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Viable.Core.Unity.Configuration;
using Viable.Core.Unity.Controllers;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Topology details section - shows when Topology = MaskedDomain.
    /// Allows configuration of mask shape and parameters.
    /// </summary>
    public class TopologyDetailsSection : CollapsibleSection
    {
        [Header("Mask Configuration")]
        [SerializeField] private TMP_Dropdown maskShapeDropdown;
        [SerializeField] private TMP_InputField radiusOuterInput;
        [SerializeField] private TMP_InputField radiusInnerInput;
        [SerializeField] private TMP_InputField corridorWidthInput;
        [SerializeField] private TMP_InputField percolationProbInput;

        [Header("Field Visibility GameObjects")]
        [SerializeField] private GameObject radiusOuterRow;
        [SerializeField] private GameObject radiusInnerRow;
        [SerializeField] private GameObject corridorWidthRow;
        [SerializeField] private GameObject percolationProbRow;

        [Header("Orchestrator")]
        [SerializeField] private SimulationUIOrchestrator orchestrator;

        private Configuration.WorkingScenarioConfig currentConfig;

        protected override void Start()
        {
            base.Start();

            // Populate mask shape dropdown
            if (maskShapeDropdown != null)
            {
                maskShapeDropdown.ClearOptions();
                maskShapeDropdown.AddOptions(new List<string>
                {
                    "Rectangle",
                    "Circle",
                    "Ring",
                    "Corridor",
                    "Percolation Holes"
                });
                maskShapeDropdown.onValueChanged.AddListener(OnMaskShapeChanged);
            }

            // Wire input field listeners
            if (radiusOuterInput != null)
                radiusOuterInput.onEndEdit.AddListener(OnRadiusOuterChanged);

            if (radiusInnerInput != null)
                radiusInnerInput.onEndEdit.AddListener(OnRadiusInnerChanged);

            if (corridorWidthInput != null)
                corridorWidthInput.onEndEdit.AddListener(OnCorridorWidthChanged);

            if (percolationProbInput != null)
                percolationProbInput.onEndEdit.AddListener(OnPercolationProbChanged);
        }

        /// <summary>
        /// Refresh UI controls from WorkingScenarioConfig.
        /// </summary>
        public void Refresh(WorkingScenarioConfig cfg)
        {
            if (orchestrator != null && orchestrator.IsRefreshing())
                return;

            // Set topology/mask controls
            if (maskShapeDropdown != null)
                maskShapeDropdown.SetValueWithoutNotify((int)cfg.MaskType);

            if (radiusOuterInput != null)
                radiusOuterInput.SetTextWithoutNotify(cfg.MaskRadiusOuter.ToString("F1"));

            if (radiusInnerInput != null)
                radiusInnerInput.SetTextWithoutNotify(cfg.MaskRadiusInner.ToString("F1"));

            if (corridorWidthInput != null)
                corridorWidthInput.SetTextWithoutNotify(cfg.MaskCorridorWidth.ToString("F1"));

            if (percolationProbInput != null)
                percolationProbInput.SetTextWithoutNotify(cfg.MaskPercolationProbability.ToString("F2"));
        }

        public override void Bind(Configuration.WorkingScenarioConfig config)
        {
            currentConfig = config;

            // Set dropdown
            if (maskShapeDropdown != null)
                maskShapeDropdown.value = (int)config.MaskType;

            // Set input fields
            if (radiusOuterInput != null)
                radiusOuterInput.text = config.MaskRadiusOuter.ToString("F1");

            if (radiusInnerInput != null)
                radiusInnerInput.text = config.MaskRadiusInner.ToString("F1");

            if (corridorWidthInput != null)
                corridorWidthInput.text = config.MaskCorridorWidth.ToString("F1");

            if (percolationProbInput != null)
                percolationProbInput.text = config.MaskPercolationProbability.ToString("F2");

            // Update field visibility based on mask shape
            UpdateFieldVisibility(config.MaskType);
        }

        public override void RefreshVisibility(Configuration.WorkingScenarioConfig config)
        {
            // Show only when Domain = MaskedDomain (renamed from Topology)
            bool relevant = (config.Domain == Configuration.DomainMode.MaskedDomain);
            gameObject.SetActive(relevant);
        }

        /// <summary>
        /// Unity-event compatible wrapper for RefreshVisibility.
        /// Uses cached currentConfig.
        /// </summary>
        public void RefreshVisibility()
        {
            if (currentConfig != null)
            {
                RefreshVisibility(currentConfig);
            }
            else
            {
                // If no config bound yet, hide by default
                gameObject.SetActive(false);
            }
        }

        public override void ApplyEdits(Configuration.WorkingScenarioConfig config)
        {
            // Apply mask shape
            if (maskShapeDropdown != null)
                config.MaskType = (Configuration.MaskShape)maskShapeDropdown.value;

            // Apply numeric parameters
            if (radiusOuterInput != null && float.TryParse(radiusOuterInput.text, out float ro))
                config.MaskRadiusOuter = ro;

            if (radiusInnerInput != null && float.TryParse(radiusInnerInput.text, out float ri))
                config.MaskRadiusInner = ri;

            if (corridorWidthInput != null && float.TryParse(corridorWidthInput.text, out float cw))
                config.MaskCorridorWidth = cw;

            if (percolationProbInput != null && float.TryParse(percolationProbInput.text, out float pp))
                config.MaskPercolationProbability = pp;
        }

        private void OnMaskShapeChanged(int index)
        {
            var maskType = (Configuration.MaskShape)index;
            UpdateFieldVisibility(maskType);

            if (currentConfig != null)
            {
                currentConfig.MaskType = maskType;
            }
        }

        private void UpdateFieldVisibility(Configuration.MaskShape maskType)
        {
            // Hide all by default
            if (radiusOuterRow != null) radiusOuterRow.SetActive(false);
            if (radiusInnerRow != null) radiusInnerRow.SetActive(false);
            if (corridorWidthRow != null) corridorWidthRow.SetActive(false);
            if (percolationProbRow != null) percolationProbRow.SetActive(false);

            // Show fields based on mask type
            switch (maskType)
            {
                case Configuration.MaskShape.Rectangle:
                    // No parameters needed
                    break;

                case Configuration.MaskShape.Circle:
                    if (radiusOuterRow != null) radiusOuterRow.SetActive(true);
                    break;

                case Configuration.MaskShape.Ring:
                    if (radiusOuterRow != null) radiusOuterRow.SetActive(true);
                    if (radiusInnerRow != null) radiusInnerRow.SetActive(true);
                    break;

                case Configuration.MaskShape.Corridor:
                    if (corridorWidthRow != null) corridorWidthRow.SetActive(true);
                    break;

                case Configuration.MaskShape.PercolationHoles:
                    if (percolationProbRow != null) percolationProbRow.SetActive(true);
                    break;
            }
        }

        private void OnRadiusOuterChanged(string value)
        {
            if (currentConfig != null && float.TryParse(value, out float val))
            {
                currentConfig.MaskRadiusOuter = val;
            }
        }

        private void OnRadiusInnerChanged(string value)
        {
            if (currentConfig != null && float.TryParse(value, out float val))
            {
                currentConfig.MaskRadiusInner = val;
            }
        }

        private void OnCorridorWidthChanged(string value)
        {
            if (currentConfig != null && float.TryParse(value, out float val))
            {
                currentConfig.MaskCorridorWidth = val;
            }
        }

        private void OnPercolationProbChanged(string value)
        {
            if (currentConfig != null && float.TryParse(value, out float val))
            {
                currentConfig.MaskPercolationProbability = val;
            }
        }
    }
}
