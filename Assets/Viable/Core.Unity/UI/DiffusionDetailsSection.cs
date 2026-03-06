using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Viable.Core.Unity.Configuration;
using Viable.Core.Unity.Controllers;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Diffusion details section - shows when Diffusion = Anisotropic.
    /// Allows configuration of anisotropic direction, bias, and sink controls.
    /// </summary>
    public class DiffusionDetailsSection : CollapsibleSection
    {
        [Header("Anisotropic Configuration")]
        [SerializeField] private TMP_Dropdown directionDropdown;
        [SerializeField] private Slider biasSlider;
        [SerializeField] private TextMeshProUGUI biasValueText;

        [Header("Sink Controls")]
        [SerializeField] private TMP_InputField sinkCountInput;
        [SerializeField] private TMP_InputField sinkSpacingInput;
        [SerializeField] private TMP_InputField sinkRandomnessInput;
        [SerializeField] private TMP_InputField sinkThresholdInput;

        [Header("Orchestrator")]
        [SerializeField] private SimulationUIOrchestrator orchestrator;

        private Configuration.WorkingScenarioConfig currentConfig;

        protected override void Start()
        {
            base.Start();

            // Find orchestrator if not assigned
            if (orchestrator == null)
            {
                orchestrator = FindFirstObjectByType<SimulationUIOrchestrator>();
            }

            // Populate direction dropdown
            if (directionDropdown != null)
            {
                directionDropdown.ClearOptions();
                directionDropdown.AddOptions(new List<string>
                {
                    "North (↑)",
                    "East (→)",
                    "South (↓)",
                    "West (←)"
                });
                directionDropdown.onValueChanged.AddListener(OnDirectionChanged);
            }

            // Wire bias slider
            if (biasSlider != null)
            {
                biasSlider.minValue = 0f;
                biasSlider.maxValue = 1f;
                biasSlider.value = 0.5f;
                biasSlider.onValueChanged.AddListener(OnBiasChanged);
            }

           
        }

        public override void Bind(Configuration.WorkingScenarioConfig config)
        {
            currentConfig = config;

            // Set direction dropdown
            if (directionDropdown != null)
                directionDropdown.value = (int)config.AnisotropicDirection;

            // Set bias slider
            if (biasSlider != null)
                biasSlider.value = config.AnisotropicBias;

            UpdateBiasValueText(config.AnisotropicBias);


        }

        /// <summary>
        /// Refresh UI controls from WorkingScenarioConfig.
        /// </summary>
        public void Refresh(WorkingScenarioConfig cfg)
        {
            if (orchestrator != null && orchestrator.IsRefreshing())
                return;

            // Set anisotropic diffusion controls
            if (directionDropdown != null)
                directionDropdown.SetValueWithoutNotify((int)cfg.AnisotropicDirection);

            if (biasSlider != null)
                biasSlider.SetValueWithoutNotify(cfg.AnisotropicBias);

            if (biasValueText != null)
                biasValueText.text = cfg.AnisotropicBias.ToString("F2");


        }

        public override void RefreshVisibility(Configuration.WorkingScenarioConfig config)
        {
            // Always visible now since sink controls are here
            // Or show only when Diffusion = Anisotropic if you prefer
            gameObject.SetActive(true);
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
                gameObject.SetActive(true);
            }
        }

        public override void ApplyEdits(Configuration.WorkingScenarioConfig config)
        {
            // Apply direction
            if (directionDropdown != null)
                config.AnisotropicDirection = (Configuration.DiffusionDirection)directionDropdown.value;

            // Apply bias
            if (biasSlider != null)
                config.AnisotropicBias = biasSlider.value;

            // Apply sink controls
            if (sinkCountInput != null && int.TryParse(sinkCountInput.text, out var sinkCount))
                config.InitialSinkCount = Mathf.Max(0, sinkCount);

            if (sinkSpacingInput != null && float.TryParse(sinkSpacingInput.text, out var sinkSpacing))
                config.SinkSpacing = Mathf.Max(0f, sinkSpacing);

            if (sinkRandomnessInput != null && float.TryParse(sinkRandomnessInput.text, out var sinkRand))
                config.SinkRandomness = Mathf.Clamp01(sinkRand);

            if (sinkThresholdInput != null && float.TryParse(sinkThresholdInput.text, out var sinkThresh))
                config.SinkFormationThreshold = sinkThresh;
        }

        private void OnDirectionChanged(int index)
        {
            if (currentConfig != null)
            {
                currentConfig.AnisotropicDirection = (Configuration.DiffusionDirection)index;
            }
        }

        private void OnBiasChanged(float value)
        {
            if (currentConfig != null)
            {
                currentConfig.AnisotropicBias = value;
            }

            UpdateBiasValueText(value);
        }

        private void UpdateBiasValueText(float bias)
        {
            if (biasValueText != null)
            {
                biasValueText.text = $"Bias: {bias:F2}";
            }
        }

        /// <summary>
        /// Called when any sink control input changes.
        /// Applies edits to orchestrator's WorkingConfig.
        /// </summary>
        private void OnSinkControlChanged()
        {
            if (orchestrator != null && orchestrator.WorkingConfig != null)
            {
                ApplyEdits(orchestrator.WorkingConfig);
                Debug.Log($"[DiffusionDetailsSection] Sink controls updated: " +
                          $"Count={orchestrator.WorkingConfig.InitialSinkCount}, " +
                          $"Threshold={orchestrator.WorkingConfig.SinkFormationThreshold}");
            }
            else if (currentConfig != null)
            {
                ApplyEdits(currentConfig);
                Debug.LogWarning("[DiffusionDetailsSection] Orchestrator not found, using local config");
            }
        }
    }
}
