using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for exporting simulation runs.
    /// Provides visible export button and shows export path/feedback.
    /// Stage 12: Essential UI component - makes export discoverable.
    /// </summary>
    public class ExportUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button exportButton;
        [SerializeField] private TextMeshProUGUI exportPathText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button openFolderButton; // Optional

        [Header("Button Text")]
        [SerializeField] private TextMeshProUGUI exportButtonText;

        private Controllers.SimulationController simulationController;
        private string lastExportPath = "";

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;
            SetupUI();
        }

        private void SetupUI()
        {
            // Setup export button
            if (exportButton != null)
                exportButton.onClick.AddListener(OnExportClicked);

            // Setup open folder button (optional)
            if (openFolderButton != null)
            {
                openFolderButton.onClick.AddListener(OnOpenFolderClicked);
                openFolderButton.gameObject.SetActive(false); // Hidden until export
            }

            // Set button text
            if (exportButtonText != null)
                exportButtonText.text = "Export Run";

            // Clear initial text
            if (exportPathText != null)
                exportPathText.text = "No exports yet";

            if (feedbackText != null)
                feedbackText.text = "";
        }

        private void OnExportClicked()
        {
            if (simulationController == null)
            {
                ShowFeedback("SimulationController not found!", Color.red);
                return;
            }

            try
            {
                // Call export method
                simulationController.ExportLastRun();

                // Get export path (requires SimulationController to return path)
                // For now, show success message
                ShowFeedback("Export successful!", Color.green);

                // Update path display
                if (exportPathText != null)
                {
                    exportPathText.text = "Check Console for export path";
                    // TODO: Get actual path from SimulationController
                    // exportPathText.text = $"Exported to: {exportPath}";
                }

                // Show open folder button
                if (openFolderButton != null)
                    openFolderButton.gameObject.SetActive(true);

                Debug.Log("[ExportUI] Export triggered successfully");
            }
            catch (System.Exception ex)
            {
                ShowFeedback($"Export failed: {ex.Message}", Color.red);
                Debug.LogError($"[ExportUI] Export failed: {ex.Message}");
            }
        }

        private void OnOpenFolderClicked()
        {
            if (string.IsNullOrEmpty(lastExportPath))
            {
                ShowFeedback("No export path available", Color.yellow);
                return;
            }

            try
            {
                // Open folder in file explorer
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{lastExportPath}\"");
                Debug.Log($"[ExportUI] Opened folder: {lastExportPath}");
            }
            catch (System.Exception ex)
            {
                ShowFeedback($"Failed to open folder: {ex.Message}", Color.red);
                Debug.LogError($"[ExportUI] Failed to open folder: {ex.Message}");
            }
        }

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;

                // Auto-clear after 5 seconds
                Invoke(nameof(ClearFeedback), 5f);
            }
        }

        private void ClearFeedback()
        {
            if (feedbackText != null)
            {
                feedbackText.text = "";
            }
        }

        /// <summary>
        /// Update export path display.
        /// Call this from UIManager when export completes.
        /// </summary>
        public void SetLastExportPath(string path)
        {
            lastExportPath = path;

            if (exportPathText != null)
            {
                exportPathText.text = $"Last export: {System.IO.Path.GetFileName(path)}";
            }

            if (openFolderButton != null)
                openFolderButton.gameObject.SetActive(true);
        }
    }
}
