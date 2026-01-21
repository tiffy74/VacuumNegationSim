using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Viable.Core.Unity.Controllers; // ADDED: Reference to new namespace

public class SimulationUIController : MonoBehaviour
{
    public SimulationController simController; // Now finds the Core.Unity version
    public GameObject OverlayPanel;
    public TMP_InputField ResourceGlobalMaxInput;
    public TMP_InputField ResourceGlobalInput;

    public TMP_InputField MinResourceForPersistenceInput;

    public TMP_InputField EthreshBaseInput;
    public TMP_InputField GlobalScarcityKInput;
    public TMP_InputField ComplexityPenaltyInput;
    public TMP_InputField DecayLossInput;

    public TMP_InputField MinBudgetToPropagateInput;
    public TMP_InputField ActivationCostInput;
    public TMP_InputField PropagateFracInput;

    public TMP_InputField ComplexityGainPerUseInput;
    public TMP_InputField ComplexityDiffusionRateInput;
    public TMP_InputField ComplexityDecayInput;

    public TMP_InputField ResourceLocalMaxInput;
    public TMP_InputField PerturbationProbabilityInput;
    public TMP_InputField PerturbationComplexityInput;

    public TMP_Text CellCountText;
    public TMP_Text ViableCellCountText;
    public TMP_Text ActiveCellCountText;
    public TMP_Text AvgResourceText;
    public TMP_Text AvgComplexityText;
    public TMP_Text ResourceGlobalText;
    public TMP_Text TickText;

    public Button PauseButton;
    public Button PlayButton;
    public Button RestartButton;
    public Button PanelButton;
    public Button ToggleTypeButton;
    public Button ExitButton;

    void Start()
    {
        // Controls
        PlayButton.onClick.AddListener(() => simController.Play());
        RestartButton.onClick.AddListener(() => RestartAndResume());
        PauseButton.onClick.AddListener(() => simController.Pause());
        ExitButton.onClick.AddListener(() => ExitApplication());
        PanelButton.onClick.AddListener(TogglePanel);

        OverlayPanel.SetActive(true);
    }

    void TogglePanel()
    {
        OverlayPanel.SetActive(!OverlayPanel.activeSelf);
    }

    void RestartAndResume()
    {
        simController.RestartSimulation();
        simController.Play();
    }

    void ExitApplication()
    {
        Debug.Log("Exiting application...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        // Uncomment and update this section when you want to display live stats
        // Note: SimulationController state is now private
        // For Stage 12 (UI), we'll add public accessors for stats
        
        // Example implementation (will be enabled in Stage 12):
        /*
        if (OverlayPanel.activeSelf && simController != null)
        {
            // TODO Stage 12: Add GetSimulationStats() method to SimulationController
            // var stats = simController.GetSimulationStats();
            
            // CellCountText.text = $"Total Cells: {stats.TotalCells}";
            // ViableCellCountText.text = $"Viable Cells: {stats.ViableCount}";
            // ActiveCellCountText.text = $"Active Cells: {stats.ActiveCount}";
            // AvgResourceText.text = $"Avg Resource: {stats.AvgResource:F2}";
            // AvgComplexityText.text = $"Avg Complexity: {stats.AvgComplexity:F2}";
            // ResourceGlobalText.text = $"Global Resource: {stats.ResourceGlobal:E2}";
            // TickText.text = $"Tick: {stats.Tick}";
        }
        */
    }
}