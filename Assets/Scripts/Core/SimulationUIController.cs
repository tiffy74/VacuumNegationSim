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
    public Button ToggleTypeButton;

    void Start()
    {
             
        // Controls

        // Play Button
        PlayButton.onClick.AddListener(() => simController.Play());

        // Fix: Use lambda to call RestartAndResume, which is defined in Update (should be moved to class scope)
        RestartButton.onClick.AddListener(() => RestartAndResume());

        // Pause Button
        PauseButton.onClick.AddListener(() => simController.Pause());

        // Restart Button
        RestartButton.onClick.AddListener(() => simController.RestartSimulation());


        PanelButton.onClick.AddListener(TogglePanel);
        
        


        OverlayPanel.SetActive(true);
    }

    // Move these methods to class scope so they are accessible
    void TogglePanel()
    {
        OverlayPanel.SetActive(!OverlayPanel.activeSelf);
    }

    void RestartAndResume()
    {
        simController.RestartSimulation();
        simController.Play(); // Immediately resume simulation after restart
    }

    void ExitApplication()
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

    void Update()
    {
        //    if (OverlayPanel.activeSelf)
        //    {
        //        int cellCount = simController.Grid.Width * simController.Grid.Height;
        //        int viableCount = 0;
        //        int activeCount = 0;
        //        double totalEnergy = 0;
        //        double totalEntropy = 0;

        //        for (int i = 0; i < simController.Nlocal.Length; i++)
        //        {
        //            if (simController.Nlocal[i] > simController.MinEnergyForPersistence)
        //                viableCount++;
        //            if (simController.Active[i] == 1)
        //                activeCount++;
        //            totalEnergy += simController.Nlocal[i];
        //            totalEntropy += simController.Entropy[i];
        //        }

        //        double avgEnergy = totalEnergy / cellCount;
        //        double avgEntropy = totalEntropy / cellCount;

        //        CellCountText.text = $"Total Cells: {cellCount}";
        //        ViableCellCountText.text = $"Viable Cells: {viableCount}";
        //        ActiveCellCountText.text = $"Active Cells: {activeCount}";
        //        AvgEnergyText.text = $"Avg Energy: {avgEnergy:F2}";
        //        AvgEntropyText.text = $"Avg Entropy: {avgEntropy:F2}";
        //        NGlobalText.text = $"Global Energy: {simController.NGlobal:E2}";
        //        TickText.text = $"Tick: {simController.tick}";
        //    }
        //}
    }
}