using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SimulationUIController : MonoBehaviour
{
    public SimulationController simController;
    public GameObject OverlayPanel;
    public TMP_InputField NGlobalMaxInput;
    public TMP_InputField NGlobalInput;
    
    // public TMP_InputField GlobalReplenishPerTickInput;
    public TMP_InputField MinEnergyForPersistenceInput;

    public TMP_InputField EthreshBaseInput;
    public TMP_InputField GlobalScarcityKInput;
    public TMP_InputField EntropyPenaltyInput;
    public TMP_InputField DecayLossInput;

    public TMP_InputField MinBudgetToPropagateInput;
    public TMP_InputField ActivationCostInput;
    public TMP_InputField PropagateFracInput;

    public TMP_InputField EntropyGainPerUseInput;
    public TMP_InputField EntropyDiffuseRateInput;
    
    public TMP_InputField EntropyDecayInput;

    public TMP_InputField NlocalMaxInput;
    public TMP_InputField VacuumEventProbabilityInput;
    public TMP_InputField VacuumEventEntropyInput;

    public TMP_Text CellCountText;
    public TMP_Text ViableCellCountText;
    public TMP_Text ActiveCellCountText;
    public TMP_Text AvgEnergyText;
    public TMP_Text AvgEntropyText;
    public TMP_Text NGlobalText;
    public TMP_Text TickText;

    //public TMP_InputField NlocalMaxInput;

    public Button PauseButton;
    public Button PlayButton;
    public Button RestartButton;
    public Button PanelButton;

    void Start()
    {
        // Helper to add float input listeners
        void AddFloatInputListener(TMP_InputField input, Action<float> setter, string format = "G", float min = 0.0001f)
        {
            input.onEndEdit.AddListener(val =>
            {
                if (float.TryParse(val, out float result))
                {
                    float clamped = Mathf.Max(min, result);
                    setter(clamped);
                    input.text = clamped.ToString(format);
                }
            });
        }

        // Controls

        // Play Button
        PlayButton.onClick.AddListener(() => simController.Play());

        RestartButton.onClick.AddListener(RestartAndResume);

        // Pause Button
        PauseButton.onClick.AddListener(() => simController.Pause());

        // Restart Button
        RestartButton.onClick.AddListener(() => simController.RestartSimulation());

        PanelButton.onClick.AddListener(TogglePanel);
        OverlayPanel.SetActive(true);

        //// **************************************************************************
        //// Global
        //Debug.Log($"NGlobalMax: {simController.NGlobalMax}");
        //NGlobalMaxInput.text = simController.NGlobalMax.ToString("G");
        //Debug.Log($"NGlobal: {simController.NGlobal}");
        //NGlobalInput.text = simController.NGlobal.ToString("G"); ;
        ////GlobalReplenishPerTickInput.text = simController.GlobalReplenishPerTick.ToString("G");
        //MinEnergyForPersistenceInput.text = simController.MinEnergyForPersistence.ToString("G");

        //AddFloatInputListener(NGlobalMaxInput, v => simController.NGlobalMax = v);
        //AddFloatInputListener(NGlobalInput, v => simController.NGlobal = v);
        ////AddFloatInputListener(GlobalReplenishPerTickInput, v => simController.GlobalReplenishPerTick = v);
        //AddFloatInputListener(MinEnergyForPersistenceInput, v => simController.MinEnergyForPersistence = v);

        //// **************************************************************************
        //// Viability / Threshold
        //EthreshBaseInput.text = simController.EthreshBase.ToString("G");
        //GlobalScarcityKInput.text = simController.GlobalScarcityK.ToString("G");
        //EntropyPenaltyInput.text = simController.EntropyPenalty.ToString("G");
        //DecayLossInput.text = simController.DecayLoss.ToString("G");

        //// Listeners
        //AddFloatInputListener(EthreshBaseInput, v => simController.EthreshBase = v);
        //AddFloatInputListener(GlobalScarcityKInput, v => simController.GlobalScarcityK = v);
        //AddFloatInputListener(EntropyPenaltyInput, v => simController.EntropyPenalty = v);
        //AddFloatInputListener(DecayLossInput, v => simController.DecayLoss = v);

        //// **************************************************************************
        //// Propagation
        //PropagateFracInput.text = simController.PropagateFrac.ToString("G");
        //MinBudgetToPropagateInput.text = simController.MinBudgetToPropagate.ToString("G");
        //ActivationCostInput.text = simController.ActivationCost.ToString("G");

        //// Listeners
        //AddFloatInputListener(PropagateFracInput, v => simController.PropagateFrac = v);
        //AddFloatInputListener(MinBudgetToPropagateInput, v => simController.MinBudgetToPropagate = v);
        //AddFloatInputListener(ActivationCostInput, v => simController.ActivationCost = v);

        //// **************************************************************************
        //// Entropy Dynamics
        //EntropyGainPerUseInput.text = simController.EntropyGainPerUse.ToString("G");
        //EntropyDiffuseRateInput.text = simController.EntropyDiffuseRate.ToString("G");
        //EntropyDecayInput.text = simController.EntropyDecay.ToString("G");

        //// Listeners
        //AddFloatInputListener(EntropyGainPerUseInput, v => simController.EntropyGainPerUse = v);
        //AddFloatInputListener(EntropyDiffuseRateInput, v => simController.EntropyDiffuseRate = v);
        //AddFloatInputListener(EntropyDecayInput, v => simController.EntropyDecay = v);


        //// **************************************************************************
        //// ***** Local Limits
        //NlocalMaxInput.text = simController.NlocalMax.ToString("G");
        //VacuumEventProbabilityInput.text = simController.VacuumEventProbability.ToString("G");
        //VacuumEventEntropyInput.text = simController.VacuumEventEntropy.ToString("G");
        //// Listeners
        //AddFloatInputListener(NlocalMaxInput, v => simController.NlocalMax = v);
        //AddFloatInputListener(VacuumEventProbabilityInput, v => simController.VacuumEventProbability = v);
        //AddFloatInputListener(VacuumEventProbabilityInput, v => simController.VacuumEventProbability = v);

        //// **************************************************************************
    }
    void Update()
    {
        if (OverlayPanel.activeSelf)
        {
            int cellCount = simController.Grid.Width * simController.Grid.Height;
            int viableCount = 0;
            int activeCount = 0;
            double totalEnergy = 0;
            double totalEntropy = 0;

            for (int i = 0; i < simController.Nlocal.Length; i++)
            {
                if (simController.Nlocal[i] > simController.MinEnergyForPersistence)
                    viableCount++;
                if (simController.Active[i] == 1)
                    activeCount++;
                totalEnergy += simController.Nlocal[i];
                totalEntropy += simController.Entropy[i];
            }

            double avgEnergy = totalEnergy / cellCount;
            double avgEntropy = totalEntropy / cellCount;

            CellCountText.text = $"Total Cells: {cellCount}";
            ViableCellCountText.text = $"Viable Cells: {viableCount}";
            ActiveCellCountText.text = $"Active Cells: {activeCount}";
            AvgEnergyText.text = $"Avg Energy: {avgEnergy:F2}";
            AvgEntropyText.text = $"Avg Entropy: {avgEntropy:F2}";
            NGlobalText.text = $"Global Energy: {simController.NGlobal:E2}";
            TickText.text = $"Tick: {simController.tick}";
        }
    }

    void TogglePanel()
    {
        OverlayPanel.SetActive(!OverlayPanel.activeSelf);
    }
    void RestartAndResume()
    {
        simController.RestartSimulation();
        simController.Play(); // Immediately resume simulation after restart
    }
    public void ExitApplication()
    {
        Debug.Log("Exiting application...");
        #if UNITY_EDITOR
                // Stop play mode in the Unity Editor
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Quit the application in standalone builds
            Application.Quit();
        #endif
    }
}