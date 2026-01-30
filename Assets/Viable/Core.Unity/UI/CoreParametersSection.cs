using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Core parameters section - always visible in Setup tab.
    /// Contains curated simulation parameters that users typically adjust.
    /// </summary>
    public class CoreParametersSection : CollapsibleSection
    {
        [Header("Core Parameter Input Fields")]
        [SerializeField] private TMP_InputField resourceGlobalMaxInput;
        [SerializeField] private TMP_InputField resourceRechargeRateInput;
        [SerializeField] private TMP_InputField decayLossInput;
        [SerializeField] private TMP_InputField maintCostInput;
        [SerializeField] private TMP_InputField activationCostInput;
        [SerializeField] private TMP_InputField expansionProbabilityInput;
        [SerializeField] private TMP_InputField inflowPerCellInput;
        [SerializeField] private TMP_InputField diffusionRateInput;

        [Header("Advanced Parameters Button")]
        [SerializeField] private Button advancedParamsButton;

        private Configuration.WorkingScenarioConfig currentConfig;

        protected override void Start()
        {
            base.Start();

            // Wire input field listeners
            if (resourceGlobalMaxInput != null)
                resourceGlobalMaxInput.onEndEdit.AddListener(OnResourceGlobalMaxChanged);

            if (resourceRechargeRateInput != null)
                resourceRechargeRateInput.onEndEdit.AddListener(OnResourceRechargeRateChanged);

            if (decayLossInput != null)
                decayLossInput.onEndEdit.AddListener(OnDecayLossChanged);

            if (maintCostInput != null)
                maintCostInput.onEndEdit.AddListener(OnMaintCostChanged);

            if (activationCostInput != null)
                activationCostInput.onEndEdit.AddListener(OnActivationCostChanged);

            if (expansionProbabilityInput != null)
                expansionProbabilityInput.onEndEdit.AddListener(OnExpansionProbabilityChanged);

            if (inflowPerCellInput != null)
                inflowPerCellInput.onEndEdit.AddListener(OnInflowPerCellChanged);

            if (diffusionRateInput != null)
                diffusionRateInput.onEndEdit.AddListener(OnDiffusionRateChanged);

            // Wire advanced params button
            if (advancedParamsButton != null)
            {
                advancedParamsButton.onClick.AddListener(OnAdvancedParamsClicked);
            }
        }

        public override void Bind(Configuration.WorkingScenarioConfig config)
        {
            currentConfig = config;

            // Populate input fields with current values
            if (resourceGlobalMaxInput != null)
                resourceGlobalMaxInput.text = FormatScientific(config.ResourceGlobalMax);

            if (resourceRechargeRateInput != null)
                resourceRechargeRateInput.text = FormatScientific(config.ResourceRechargeRate);

            if (decayLossInput != null)
                decayLossInput.text = config.DecayLoss.ToString("F4");

            if (maintCostInput != null)
                maintCostInput.text = config.MaintCost.ToString("F2");

            if (activationCostInput != null)
                activationCostInput.text = config.ActivationCost.ToString("F2");

            if (expansionProbabilityInput != null)
                expansionProbabilityInput.text = config.ExpansionProbability.ToString("F4");

            if (inflowPerCellInput != null)
                inflowPerCellInput.text = FormatScientific(config.InflowPerCell);

            if (diffusionRateInput != null)
                diffusionRateInput.text = config.DiffusionRate.ToString("F3");
        }

        public override void RefreshVisibility(Configuration.WorkingScenarioConfig config)
        {
            // Core parameters are always visible
            gameObject.SetActive(true);
        }

        public override void ApplyEdits(Configuration.WorkingScenarioConfig config)
        {
            // Parse and apply all input fields
            if (resourceGlobalMaxInput != null && TryParseDouble(resourceGlobalMaxInput.text, out double rgm))
                config.ResourceGlobalMax = rgm;

            if (resourceRechargeRateInput != null && TryParseDouble(resourceRechargeRateInput.text, out double rrr))
                config.ResourceRechargeRate = rrr;

            if (decayLossInput != null && double.TryParse(decayLossInput.text, out double dl))
                config.DecayLoss = dl;

            if (maintCostInput != null && double.TryParse(maintCostInput.text, out double mc))
                config.MaintCost = mc;

            if (activationCostInput != null && double.TryParse(activationCostInput.text, out double ac))
                config.ActivationCost = ac;

            if (expansionProbabilityInput != null && double.TryParse(expansionProbabilityInput.text, out double ep))
                config.ExpansionProbability = ep;

            if (inflowPerCellInput != null && TryParseDouble(inflowPerCellInput.text, out double ipc))
                config.InflowPerCell = ipc;

            if (diffusionRateInput != null && double.TryParse(diffusionRateInput.text, out double dr))
                config.DiffusionRate = dr;
        }

        #region Input Field Change Handlers

        private void OnResourceGlobalMaxChanged(string value)
        {
            if (currentConfig != null && TryParseDouble(value, out double val))
            {
                currentConfig.ResourceGlobalMax = val;
            }
        }

        private void OnResourceRechargeRateChanged(string value)
        {
            if (currentConfig != null && TryParseDouble(value, out double val))
            {
                currentConfig.ResourceRechargeRate = val;
            }
        }

        private void OnDecayLossChanged(string value)
        {
            if (currentConfig != null && double.TryParse(value, out double val))
            {
                currentConfig.DecayLoss = val;
            }
        }

        private void OnMaintCostChanged(string value)
        {
            if (currentConfig != null && double.TryParse(value, out double val))
            {
                currentConfig.MaintCost = val;
            }
        }

        private void OnActivationCostChanged(string value)
        {
            if (currentConfig != null && double.TryParse(value, out double val))
            {
                currentConfig.ActivationCost = val;
            }
        }

        private void OnExpansionProbabilityChanged(string value)
        {
            if (currentConfig != null && double.TryParse(value, out double val))
            {
                currentConfig.ExpansionProbability = val;
            }
        }

        private void OnInflowPerCellChanged(string value)
        {
            if (currentConfig != null && TryParseDouble(value, out double val))
            {
                currentConfig.InflowPerCell = val;
            }
        }

        private void OnDiffusionRateChanged(string value)
        {
            if (currentConfig != null && double.TryParse(value, out double val))
            {
                currentConfig.DiffusionRate = val;
            }
        }

        #endregion

        private void OnAdvancedParamsClicked()
        {
            Debug.Log("[CoreParametersSection] Advanced Parameters button clicked");
            // TODO: Open AdvancedParametersModal (Step 11)
        }

        #region Utility Methods

        /// <summary>
        /// Format large numbers in scientific notation (e.g., 5e7).
        /// </summary>
        private string FormatScientific(double value)
        {
            if (value >= 1e6 || value <= -1e6 || (value != 0 && System.Math.Abs(value) < 0.001))
            {
                return value.ToString("E2"); // Scientific notation
            }
            else
            {
                return value.ToString("F2"); // Standard notation
            }
        }

        /// <summary>
        /// Try to parse a double, supporting scientific notation (e.g., "5e7").
        /// </summary>
        private bool TryParseDouble(string text, out double result)
        {
            return double.TryParse(text, System.Globalization.NumberStyles.Any, 
                                  System.Globalization.CultureInfo.InvariantCulture, 
                                  out result);
        }

        #endregion
    }
}
