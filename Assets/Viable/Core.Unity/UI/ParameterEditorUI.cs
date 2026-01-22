using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Optional UI panel for editing simulation parameters at runtime.
    /// Advanced feature for power users.
    /// Stage 12: Optional component.
    /// </summary>
    public class ParameterEditorUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private GameObject parametersPanel;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;

        [Header("Parameter Fields")]
        // Add TMP_InputField references for each parameter
        // For now, placeholder implementation

        private Controllers.SimulationController simulationController;
        private bool isPanelVisible = false;

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;
            SetupUI();
        }

        private void SetupUI()
        {
            // Setup toggle button
            if (toggleButton != null)
                toggleButton.onClick.AddListener(OnToggleClicked);

            // Setup apply/reset buttons
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClicked);

            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);

            // Start hidden
            if (parametersPanel != null)
                parametersPanel.SetActive(false);
        }

        private void OnToggleClicked()
        {
            isPanelVisible = !isPanelVisible;

            if (parametersPanel != null)
                parametersPanel.SetActive(isPanelVisible);

            Debug.Log($"[ParameterEditorUI] Panel toggled: {isPanelVisible}");
        }

        private void OnApplyClicked()
        {
            // TODO: Read values from input fields and apply to simulation
            Debug.LogWarning("[ParameterEditorUI] Parameter editing not yet implemented");
            Debug.LogWarning("[ParameterEditorUI] Requires SimulationController.UpdateParameters() method");
        }

        private void OnResetClicked()
        {
            // TODO: Reset to preset values
            Debug.LogWarning("[ParameterEditorUI] Parameter reset not yet implemented");
        }

        // TODO: Add methods to:
        // - Populate fields from current preset
        // - Read values from fields
        // - Apply to SimulationConfiguration
        // - Validate input values
    }
}
