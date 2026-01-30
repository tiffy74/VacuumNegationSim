using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Diffusion details section - shows when Diffusion = Anisotropic.
    /// Allows configuration of anisotropic direction and bias.
    /// </summary>
    public class DiffusionDetailsSection : CollapsibleSection
    {
        [Header("Anisotropic Configuration")]
        [SerializeField] private TMP_Dropdown directionDropdown;
        [SerializeField] private Slider biasSlider;
        [SerializeField] private TextMeshProUGUI biasValueText;

        private Configuration.WorkingScenarioConfig currentConfig;

        protected override void Start()
        {
            base.Start();

            // Populate direction dropdown
            if (directionDropdown != null)
            {
                directionDropdown.ClearOptions();
                directionDropdown.AddOptions(new List<string>
                {
                    "North (?)",
                    "East (?)",
                    "South (?)",
                    "West (?)"
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

        public override void RefreshVisibility(Configuration.WorkingScenarioConfig config)
        {
            // Show only when Diffusion = Anisotropic
            bool relevant = (config.Diffusion == Configuration.DiffusionMode.Anisotropic);
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
            // Apply direction
            if (directionDropdown != null)
                config.AnisotropicDirection = (Configuration.DiffusionDirection)directionDropdown.value;

            // Apply bias
            if (biasSlider != null)
                config.AnisotropicBias = biasSlider.value;
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
    }
}
