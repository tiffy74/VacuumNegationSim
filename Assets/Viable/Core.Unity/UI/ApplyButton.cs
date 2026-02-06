using UnityEngine;
using UnityEngine.UI;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Apply Button - Located at bottom of Right Dock Setup Panel.
    /// Applies all changes (preset + mechanism tweaks) and restarts simulation.
    /// Stage 14: Separated from Load Preset button for clearer UX.
    /// </summary>
    public class ApplyButton : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button applyButton;

        [Header("References")]
        private Controllers.SimulationUIOrchestrator orchestrator;
        private NotificationUI notificationUI;

        private void Start()
        {
            // Find references
            orchestrator = FindFirstObjectByType<Controllers.SimulationUIOrchestrator>();
            notificationUI = FindFirstObjectByType<NotificationUI>();

            // Wire button
            if (applyButton != null)
            {
                applyButton.onClick.RemoveAllListeners();
                applyButton.onClick.AddListener(OnApply);
            }

            Debug.Log("[ApplyButton] Initialized in Right Dock");
        }

        private void OnApply()
        {
            if (orchestrator == null)
            {
                Debug.LogError("[ApplyButton] Orchestrator not found!");
                ShowNotification("Error: UI Orchestrator not found", NotificationUI.NotificationType.Error);
                return;
            }

            Debug.Log("[ApplyButton] Applying changes and restarting simulation...");
            
            try
            {
                orchestrator.ApplyAndRestart();
                ShowNotification("? Simulation restarted", NotificationUI.NotificationType.Info); // Blue - informational
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ApplyButton] Apply failed: {ex.Message}");
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
                Debug.LogWarning($"[ApplyButton] NotificationUI not found, message: {message}");
            }
        }
    }
}
