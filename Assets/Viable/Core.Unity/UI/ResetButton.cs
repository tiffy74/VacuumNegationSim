using UnityEngine;
using UnityEngine.UI;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Reset Button - Clears all configuration back to defaults and resets simulation.
    /// Located next to Apply button at bottom of Right Dock Setup Panel.
    /// Stage 14: Added for quick reset to default state.
    /// </summary>
    public class ResetButton : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button resetButton;

        [Header("References")]
        private Controllers.SimulationUIOrchestrator orchestrator;
        private Controllers.SimulationController simulationController;
        private NotificationUI notificationUI;

        [Header("Default Preset")]
        [Tooltip("The default preset to load when resetting. If null, uses first preset found.")]
        [SerializeField] private ScenarioPreset defaultPreset;

        private void Start()
        {
            // Find references
            orchestrator = FindFirstObjectByType<Controllers.SimulationUIOrchestrator>();
            simulationController = FindFirstObjectByType<Controllers.SimulationController>();
            notificationUI = FindFirstObjectByType<NotificationUI>();

            // Wire button
            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(OnReset);
            }

            // Find default preset if not assigned
            if (defaultPreset == null)
            {
                LoadDefaultPreset();
            }

            Debug.Log("[ResetButton] Initialized in Right Dock");
        }

        private void LoadDefaultPreset()
        {
#if UNITY_EDITOR
            // Editor: Load from AssetDatabase
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScenarioPreset", new[] { "Assets/Viable/Core.Unity/Presets/Examples" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var preset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScenarioPreset>(path);
                if (preset != null && (preset.PresetName.Contains("Default") || preset.PresetName.StartsWith("00_")))
                {
                    defaultPreset = preset;
                    Debug.Log($"[ResetButton] Found default preset: {preset.PresetName}");
                    return;
                }
            }
            
            // Fallback to first preset
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                defaultPreset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScenarioPreset>(path);
                Debug.Log($"[ResetButton] Using first preset as default: {defaultPreset?.PresetName}");
            }
#else
            // Runtime: Load from Resources
            var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
            
            // Try to find "Default" preset
            defaultPreset = System.Array.Find(presets, p => 
                p.PresetName.Contains("Default") || p.PresetName.StartsWith("00_"));
            
            // Fallback to first preset
            if (defaultPreset == null && presets.Length > 0)
            {
                defaultPreset = presets[0];
            }
            
            Debug.Log($"[ResetButton] Default preset: {defaultPreset?.PresetName ?? "None"}");
#endif
        }

        private void OnReset()
        {
            if (orchestrator == null)
            {
                Debug.LogError("[ResetButton] Orchestrator not found!");
                ShowNotification("Error: UI Orchestrator not found", NotificationUI.NotificationType.Error);
                return;
            }

            if (simulationController == null)
            {
                Debug.LogError("[ResetButton] SimulationController not found!");
                ShowNotification("Error: Simulation Controller not found", NotificationUI.NotificationType.Error);
                return;
            }

            Debug.Log("[ResetButton] Resetting to defaults...");
            
            try
            {
                // Step 1: Load default preset into orchestrator
                if (defaultPreset != null)
                {
                    Debug.Log($"[ResetButton] Loading default preset: {defaultPreset.PresetName}");
                    Debug.Log($"[ResetButton] Default preset topology: {defaultPreset.MechanismConfig.GridTopology}");
                    
                    orchestrator.LoadPreset(defaultPreset);
                    Debug.Log($"[ResetButton] ? Loaded default preset into UI: {defaultPreset.PresetName}");
                }
                else
                {
                    Debug.LogWarning("[ResetButton] No default preset assigned");
                }

                // Step 2: Force clear old visuals before restart
                Debug.Log("[ResetButton] Clearing old visual cells...");
                if (simulationController.Grid != null)
                {
                    simulationController.Grid.ClearAllCells();
                }

                // Step 3: Restart simulation with default config (resets tick to 0)
                Debug.Log("[ResetButton] Restarting simulation...");
                orchestrator.ApplyAndRestart();
                
                string presetName = defaultPreset != null ? defaultPreset.PresetName : "Default";
                Debug.Log($"[ResetButton] ? Reset complete - Tick 0, Preset: {presetName}");
                ShowNotification($"? Reset to {presetName} (Tick 0)", NotificationUI.NotificationType.Success);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ResetButton] Reset failed: {ex.Message}");
                ShowNotification($"Error: {ex.Message}", NotificationUI.NotificationType.Error);
            }
        }

        /// <summary>
        /// Show notification in bottom-right corner.
        /// </summary>
        private void ShowNotification(string message, NotificationUI.NotificationType type)
        {
            if (notificationUI != null)
            {
                notificationUI.ShowNotification(message, type);
            }
            else
            {
                Debug.LogWarning($"[ResetButton] NotificationUI not found, message: {message}");
            }
        }

        /// <summary>
        /// Set the default preset programmatically.
        /// </summary>
        public void SetDefaultPreset(ScenarioPreset preset)
        {
            defaultPreset = preset;
            Debug.Log($"[ResetButton] Default preset set to: {preset?.PresetName}");
        }
    }
}
