using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Central UI coordinator for Viable Engine simulation interface.
    /// Manages all UI panels and coordinates with SimulationController.
    /// Stage 12: Essential UI for non-programmer users.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Controllers.SimulationController simulationController;

        [Header("UI Panels")]
        [SerializeField] private PresetSelectorUI presetSelector;
        [SerializeField] private SimulationControlsUI simulationControls;
        [SerializeField] private InfoDisplayUI infoDisplay;
        [SerializeField] private ExportUI exportUI;
        [SerializeField] private ParameterEditorUI parameterEditor; // Optional

        [Header("UI Settings")]
        [SerializeField] private bool showParameterEditor = false;

        void Start()
        {
            InitializeUI();
        }

        void Update()
        {
            UpdateUI();
        }

        /// <summary>
        /// Initialize all UI components and wire to SimulationController.
        /// </summary>
        private void InitializeUI()
        {
            // Find SimulationController if not assigned
            if (simulationController == null)
            {
                simulationController = FindObjectOfType<Controllers.SimulationController>();
                if (simulationController == null)
                {
                    Debug.LogError("[UIManager] SimulationController not found!");
                    return;
                }
            }

            // Initialize panels
            if (presetSelector != null)
                presetSelector.Initialize(simulationController);

            if (simulationControls != null)
                simulationControls.Initialize(simulationController);

            if (infoDisplay != null)
                infoDisplay.Initialize(simulationController);

            if (exportUI != null)
                exportUI.Initialize(simulationController);

            if (parameterEditor != null && showParameterEditor)
                parameterEditor.Initialize(simulationController);
            else if (parameterEditor != null)
                parameterEditor.gameObject.SetActive(false);

            Debug.Log("[UIManager] UI initialized successfully");
        }

        /// <summary>
        /// Update UI panels each frame.
        /// </summary>
        private void UpdateUI()
        {
            if (simulationController == null) return;

            // Update panels that need per-frame updates
            if (infoDisplay != null)
                infoDisplay.UpdateDisplay();

            if (simulationControls != null)
                simulationControls.UpdateDisplay();
        }

        /// <summary>
        /// Toggle parameter editor visibility.
        /// </summary>
        public void ToggleParameterEditor()
        {
            if (parameterEditor != null)
            {
                showParameterEditor = !showParameterEditor;
                parameterEditor.gameObject.SetActive(showParameterEditor);
            }
        }
    }
}
