using UnityEngine;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Controls panel switching via dropdown (replaces tab buttons).
    /// CRITICAL: Only toggles visibility - does NOT rebuild or refresh UI.
    /// Data persistence is handled by WorkingScenarioConfig pattern.
    /// </summary>
    public class DockModeController : MonoBehaviour
    {
        [Header("Mode Dropdown")]
        [SerializeField] private TMP_Dropdown modeDropdown;

        [Header("Panels")]
        [SerializeField] private GameObject setupPanel;
        [SerializeField] private GameObject inspectPanel;
        [SerializeField] private GameObject exportPanel;

        private void Start()
        {
            if (modeDropdown == null)
            {
                Debug.LogError("[DockModeController] Mode dropdown not assigned!");
                return;
            }

            // Populate dropdown options
            modeDropdown.ClearOptions();
            modeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Setup",
                "Inspect",
                "Export"
            });

            // Wire value changed event
            modeDropdown.onValueChanged.AddListener(OnModeChanged);

            // Show Setup panel by default
            OnModeChanged(0);
        }

        /// <summary>
        /// Handle mode dropdown change.
        /// CRITICAL: Only changes panel visibility, does NOT touch data!
        /// </summary>
        private void OnModeChanged(int modeIndex)
        {
            // Hide all panels
            if (setupPanel != null) setupPanel.SetActive(false);
            if (inspectPanel != null) inspectPanel.SetActive(false);
            if (exportPanel != null) exportPanel.SetActive(false);

            // Show selected panel
            switch (modeIndex)
            {
                case 0: // Setup
                    if (setupPanel != null)
                    {
                        setupPanel.SetActive(true);
                        Debug.Log("[DockModeController] Switched to Setup panel");
                    }
                    break;

                case 1: // Inspect
                    if (inspectPanel != null)
                    {
                        inspectPanel.SetActive(true);
                        Debug.Log("[DockModeController] Switched to Inspect panel");
                    }
                    break;

                case 2: // Export
                    if (exportPanel != null)
                    {
                        exportPanel.SetActive(true);
                        Debug.Log("[DockModeController] Switched to Export panel");
                    }
                    break;

                default:
                    Debug.LogWarning($"[DockModeController] Invalid mode index: {modeIndex}");
                    break;
            }
        }

        /// <summary>
        /// Programmatically switch to a specific panel.
        /// </summary>
        public void SwitchToSetup() => modeDropdown.value = 0;
        public void SwitchToInspect() => modeDropdown.value = 1;
        public void SwitchToExport() => modeDropdown.value = 2;
    }
}
