using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Viability details section - shows when ViabilityRule = Hysteresis.
    /// Allows configuration of hysteresis on/off thresholds.
    /// </summary>
    public class ViabilityDetailsSection : CollapsibleSection
    {
        [Header("Hysteresis Configuration")]
        [SerializeField] private TMP_InputField onThresholdInput;
        [SerializeField] private TMP_InputField offThresholdInput;
        [SerializeField] private TextMeshProUGUI explanationText;

        private Configuration.WorkingScenarioConfig currentConfig;

        protected override void Start()
        {
            base.Start();

            // Wire input field listeners
            if (onThresholdInput != null)
                onThresholdInput.onEndEdit.AddListener(OnOnThresholdChanged);

            if (offThresholdInput != null)
                offThresholdInput.onEndEdit.AddListener(OnOffThresholdChanged);

            // Set explanation text
            if (explanationText != null)
            {
                explanationText.text = "Hysteresis: Cells turn ON when viability > ON threshold, " +
                                      "and turn OFF when viability < OFF threshold. " +
                                      "ON threshold should be > OFF threshold to prevent flickering.";
            }
        }

        public override void Bind(Configuration.WorkingScenarioConfig config)
        {
            currentConfig = config;

            // Set input fields
            if (onThresholdInput != null)
                onThresholdInput.text = config.HysteresisOnThreshold.ToString("F2");

            if (offThresholdInput != null)
                offThresholdInput.text = config.HysteresisOffThreshold.ToString("F2");
        }

        public override void RefreshVisibility(Configuration.WorkingScenarioConfig config)
        {
            // Show only when ViabilityRule = Hysteresis
            bool relevant = (config.ViabilityRule == Configuration.ViabilityRuleMode.Hysteresis);
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
            // Apply on threshold
            if (onThresholdInput != null && float.TryParse(onThresholdInput.text, out float onVal))
                config.HysteresisOnThreshold = onVal;

            // Apply off threshold
            if (offThresholdInput != null && float.TryParse(offThresholdInput.text, out float offVal))
                config.HysteresisOffThreshold = offVal;
        }

        private void OnOnThresholdChanged(string value)
        {
            if (currentConfig != null && float.TryParse(value, out float val))
            {
                currentConfig.HysteresisOnThreshold = val;
            }
        }

        private void OnOffThresholdChanged(string value)
        {
            if (currentConfig != null && float.TryParse(value, out float val))
            {
                currentConfig.HysteresisOffThreshold = val;
            }
        }
    }
}
