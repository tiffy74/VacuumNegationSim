using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Assets.Scripts.Core;

/// <summary>
/// Simulation UI Controller - User Interface Management
/// 
/// Manages UI buttons and controls for simulation playback (Play/Pause/Restart/Exit).
/// Acts as mediator between Unity UI system and SimulationController.
/// 
/// Design Philosophy:
/// - Thin Controller: Minimal logic, just routes UI events to SimulationController
/// - Single Responsibility: Only handles UI interaction, no simulation logic
/// - Inspector-Driven: UI elements wired up in Unity Inspector
/// - Event-Based: Uses Unity Button.onClick events
/// 
/// UI Features:
/// - Play/Pause/Restart playback controls
/// - Parameter overlay panel (legacy - currently disabled)
/// - Exit button (Editor: stops play, Build: quits application)
/// 
/// Migration Status:
/// - Parameter Inputs: Disabled (moved to Inspector on SimulationController)
/// - Statistics Display: Disabled (consider re-enabling for runtime monitoring)
/// - TODO: Clean up disabled code or re-enable statistics
/// </summary>
public class SimulationUIController : MonoBehaviour
{
    // ============================================================================
    // INSPECTOR REFERENCES: CORE COMPONENTS
    // ============================================================================

    /// <summary>
    /// Reference to main simulation controller.
    /// Used to invoke Play/Pause/Restart commands.
    /// Must be assigned in Unity Inspector.
    /// </summary>
    [Header("Simulation Controller")]
    [Tooltip("Reference to SimulationController component")]
    public SimulationController simController;

    // ============================================================================
    // INSPECTOR REFERENCES: UI PANELS
    // ============================================================================

    /// <summary>
    /// Overlay panel containing parameter inputs (currently disabled).
    /// Can be toggled on/off with PanelButton.
    /// TODO: Re-enable for runtime parameter display or remove entirely
    /// </summary>
    [Header("UI Panels")]
    [Tooltip("Parameter overlay panel (currently disabled)")]
    public GameObject OverlayPanel;

    // ============================================================================
    // INSPECTOR REFERENCES: PARAMETER INPUTS (DISABLED)
    // ============================================================================
    // Note: These are kept for potential future re-enablement of runtime parameter editing
    // Currently simulation parameters are edited via Inspector on SimulationController

    [Header("Parameter Inputs (Disabled)")]
    public TMP_InputField NGlobalMaxInput;
    public TMP_InputField NGlobalInput;
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

    // ============================================================================
    // INSPECTOR REFERENCES: STATISTICS DISPLAY (DISABLED)
    // ============================================================================

    [Header("Statistics Display (Disabled)")]
    public TMP_Text CellCountText;
    public TMP_Text ViableCellCountText;
    public TMP_Text ActiveCellCountText;
    public TMP_Text AvgEnergyText;
    public TMP_Text AvgEntropyText;
    public TMP_Text NGlobalText;
    public TMP_Text TickText;

    // ============================================================================
    // INSPECTOR REFERENCES: CONTROL BUTTONS
    // ============================================================================

    [Header("Control Buttons")]
    [Tooltip("Play button - starts/resumes simulation")]
    public Button PlayButton;

    [Tooltip("Pause button - halts simulation")]
    public Button PauseButton;

    [Tooltip("Restart button - resets simulation to initial state")]
    public Button RestartButton;

    [Tooltip("Panel toggle button - shows/hides overlay panel")]
    public Button PanelButton;

    [Tooltip("Type toggle button (currently unused)")]
    public Button ToggleTypeButton;

    [Tooltip("Exit button - quits application")]
    public Button ExitButton;

    // ============================================================================
    // UNITY LIFECYCLE: INITIALIZATION
    // ============================================================================

    /// <summary>
    /// Unity Start - Wires up button event handlers.
    /// Called once before first frame update.
    /// </summary>
    void Start()
    {
        ValidateReferences();
        RegisterButtonHandlers();
        InitializeUI();
    }

    // ============================================================================
    // UNITY LIFECYCLE: UPDATE (DISABLED)
    // ============================================================================

    /// <summary>
    /// Unity Update - Currently disabled.
    /// Previously updated statistics display each frame.
    /// TODO: Re-enable if runtime statistics monitoring desired.
    /// </summary>
    void Update()
    {
        // Statistics display currently disabled
        // Uncomment UpdateStatisticsDisplay() to re-enable

        // UpdateStatisticsDisplay();
    }

    // ============================================================================
    // PRIVATE METHODS: INITIALIZATION
    // ============================================================================

    /// <summary>
    /// Validates that required references are assigned.
    /// Logs errors for missing critical references.
    /// </summary>
    private void ValidateReferences()
    {
        if (simController == null)
        {
            Debug.LogError("[SimulationUIController] SimulationController reference not assigned!");
        }

        if (PlayButton == null || PauseButton == null || RestartButton == null || ExitButton == null)
        {
            Debug.LogError("[SimulationUIController] One or more button references not assigned!");
        }
    }

    /// <summary>
    /// Registers event handlers for all UI buttons.
    /// </summary>
    private void RegisterButtonHandlers()
    {
        if (PlayButton != null)
            PlayButton.onClick.AddListener(OnPlayButtonPressed);

        if (PauseButton != null)
            PauseButton.onClick.AddListener(OnPauseButtonPressed);

        if (RestartButton != null)
            RestartButton.onClick.AddListener(OnRestartButtonPressed);

        if (PanelButton != null)
            PanelButton.onClick.AddListener(OnPanelButtonPressed);

        if (ExitButton != null)
            ExitButton.onClick.AddListener(OnExitButtonPressed);

        // ToggleTypeButton currently unused
    }

    /// <summary>
    /// Initializes UI to default state.
    /// </summary>
    private void InitializeUI()
    {
        if (OverlayPanel != null)
        {
            OverlayPanel.SetActive(false); // Start hidden by default
        }
    }

    // ============================================================================
    // BUTTON EVENT HANDLERS
    // ============================================================================

    /// <summary>
    /// Handles Play button press - starts/resumes simulation.
    /// </summary>
    private void OnPlayButtonPressed()
    {
        if (simController != null)
        {
            simController.Play();
            Debug.Log("[SimulationUI] Play button pressed - simulation started");
        }
    }

    /// <summary>
    /// Handles Pause button press - halts simulation.
    /// </summary>
    private void OnPauseButtonPressed()
    {
        if (simController != null)
        {
            simController.Pause();
            Debug.Log("[SimulationUI] Pause button pressed - simulation paused");
        }
    }

    /// <summary>
    /// Handles Restart button press - resets simulation and immediately resumes.
    /// Uses combined RestartAndResume() for convenience.
    /// </summary>
    private void OnRestartButtonPressed()
    {
        RestartAndResume();
    }

    /// <summary>
    /// Handles Panel button press - toggles overlay panel visibility.
    /// </summary>
    private void OnPanelButtonPressed()
    {
        TogglePanel();
    }

    /// <summary>
    /// Handles Exit button press - quits application.
    /// Editor: Stops play mode
    /// Build: Calls Application.Quit()
    /// </summary>
    private void OnExitButtonPressed()
    {
        ExitApplication();
    }

    // ============================================================================
    // UI ACTIONS
    // ============================================================================

    /// <summary>
    /// Toggles overlay panel visibility.
    /// </summary>
    private void TogglePanel()
    {
        if (OverlayPanel != null)
        {
            bool newState = !OverlayPanel.activeSelf;
            OverlayPanel.SetActive(newState);
            Debug.Log($"[SimulationUI] Panel toggled: {(newState ? "Visible" : "Hidden")}");
        }
    }

    /// <summary>
    /// Restarts simulation and immediately resumes playback.
    /// Convenience method combining two operations.
    /// </summary>
    private void RestartAndResume()
    {
        if (simController != null)
        {
            simController.RestartSimulation();
            simController.Play(); // Immediately resume after restart
            Debug.Log("[SimulationUI] Simulation restarted and resumed");
        }
    }

    /// <summary>
    /// Exits the application.
    /// Platform-specific behavior:
    /// - Unity Editor: Stops play mode
    /// - Standalone Build: Quits application
    /// </summary>
    private void ExitApplication()
    {
        Debug.Log("[SimulationUI] Exiting application...");

#if UNITY_EDITOR
        // Stop play mode in Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Quit the application in standalone builds
            Application.Quit();
#endif
    }

    // ============================================================================
    // STATISTICS DISPLAY (DISABLED - Re-enable if needed)
    // ============================================================================

    /// <summary>
    /// Updates real-time statistics display (currently disabled).
    /// 
    /// To Re-Enable:
    /// 1. Uncomment this method call in Update()
    /// 2. Ensure text references are assigned in Inspector
    /// 3. Verify GridState access (may need refactoring)
    /// 
    /// Statistics Shown:
    /// - Total cell count
    /// - Viable cell count (Nlocal > MinEnergyForPersistence)
    /// - Active cell count (Active[i] == 1)
    /// - Average energy
    /// - Average entropy
    /// - Global energy pool
    /// - Current tick number
    /// </summary>
    private void UpdateStatisticsDisplay()
    {
        // Disabled - requires refactoring to use GridState instead of Cell[]

        /*
        if (OverlayPanel == null || !OverlayPanel.activeSelf)
            return;

        if (simController == null || simController.state == null)
            return;

        var state = simController.state;
        int cellCount = state.Len;
        int viableCount = 0;
        int activeCount = 0;
        double totalEnergy = 0;
        double totalEntropy = 0;

        for (int i = 0; i < state.Len; i++)
        {
            if (state.Nlocal[i] > simController.MinEnergyForPersistence)
                viableCount++;
            if (state.Active[i] == 1)
                activeCount++;
            totalEnergy += state.Nlocal[i];
            totalEntropy += state.Entropy[i];
        }

        double avgEnergy = totalEnergy / cellCount;
        double avgEntropy = totalEntropy / cellCount;

        if (CellCountText != null)
            CellCountText.text = $"Total Cells: {cellCount}";
        if (ViableCellCountText != null)
            ViableCellCountText.text = $"Viable Cells: {viableCount}";
        if (ActiveCellCountText != null)
            ActiveCellCountText.text = $"Active Cells: {activeCount}";
        if (AvgEnergyText != null)
            AvgEnergyText.text = $"Avg Energy: {avgEnergy:F2}";
        if (AvgEntropyText != null)
            AvgEntropyText.text = $"Avg Entropy: {avgEntropy:F2}";
        if (NGlobalText != null)
            NGlobalText.text = $"Global Energy: {simController.ctx.NGlobal:E2}";
        if (TickText != null)
            TickText.text = $"Tick: {simController.ctx.Tick}";
        */
    }
}
