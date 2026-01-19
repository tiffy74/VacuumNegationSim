using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SimulationUIController : MonoBehaviour
{
    public SimulationController simController;
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
        // Note: SimulationController no longer exposes arrays directly
        // You'll need to access them through simController.state (StateGrid)
        
        // Example implementation:
        /*
        if (OverlayPanel.activeSelf && simController.state != null)
        {
            var state = simController.state;
            var ctx = simController.ctx;
            
            int cellCount = state.Len;
            int viableCount = 0;
            int activeCount = 0;
            double totalResource = 0;
            double totalComplexity = 0;

            for (int i = 0; i < state.Len; i++)
            {
                if (state.ResourceLocal[i] > simController.MinResourceForPersistence)
                    viableCount++;
                if (state.Active[i] == 1)
                    activeCount++;
                totalResource += state.ResourceLocal[i];
                totalComplexity += state.ComplexityMetric[i];
            }

            double avgResource = totalResource / cellCount;
            double avgComplexity = totalComplexity / cellCount;

            CellCountText.text = $"Total Cells: {cellCount}";
            ViableCellCountText.text = $"Viable Cells: {viableCount}";
            ActiveCellCountText.text = $"Active Cells: {activeCount}";
            AvgResourceText.text = $"Avg Resource: {avgResource:F2}";
            AvgComplexityText.text = $"Avg Complexity: {avgComplexity:F2}";
            ResourceGlobalText.text = $"Global Resource: {ctx.ResourceGlobal:E2}";
            TickText.text = $"Tick: {ctx.Tick}";
        }
        */
    }
}