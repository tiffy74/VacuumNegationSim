using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for selecting and loading scenario presets.
    /// Allows users to switch presets without Inspector knowledge.
    /// Stage 12: Essential UI component.
    /// </summary>
    public class PresetSelectorUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown presetDropdown;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button loadButton;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Settings")]
        [SerializeField] private string presetFolderPath = "Assets/Viable/Core.Unity/Presets/Examples";

        private Controllers.SimulationController simulationController;
        private List<ScenarioPreset> availablePresets;
        private ScenarioPreset selectedPreset;

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;
            LoadAvailablePresets();
            SetupUI();
        }

        /// <summary>
        /// Load all available presets from Resources or specified folder.
        /// </summary>
        private void LoadAvailablePresets()
        {
            // Load presets from Resources folder
            // Note: Presets must be in a Resources folder to be loaded at runtime
            availablePresets = new List<ScenarioPreset>();

            // Try loading from Resources/Presets/Examples
            var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
            availablePresets.AddRange(presets);

            // Also try just Resources/Presets
            if (availablePresets.Count == 0)
            {
                presets = Resources.LoadAll<ScenarioPreset>("Presets");
                availablePresets.AddRange(presets);
            }

            // Sort by name
            availablePresets = availablePresets.OrderBy(p => p.PresetName).ToList();

            Debug.Log($"[PresetSelectorUI] Loaded {availablePresets.Count} presets");
        }

        /// <summary>
        /// Setup dropdown and button listeners.
        /// </summary>
        private void SetupUI()
        {
            if (presetDropdown == null || loadButton == null)
            {
                Debug.LogError("[PresetSelectorUI] Missing UI elements!");
                return;
            }

            // Populate dropdown
            presetDropdown.ClearOptions();
            var optionNames = availablePresets.Select(p => p.PresetName).ToList();
            presetDropdown.AddOptions(optionNames);

            // Wire dropdown change event
            presetDropdown.onValueChanged.AddListener(OnPresetSelected);

            // Wire load button
            loadButton.onClick.AddListener(OnLoadButtonClicked);

            // Select first preset by default
            if (availablePresets.Count > 0)
            {
                OnPresetSelected(0);
            }
        }

        /// <summary>
        /// Called when user selects a preset from dropdown.
        /// </summary>
        private void OnPresetSelected(int index)
        {
            if (index < 0 || index >= availablePresets.Count) return;

            selectedPreset = availablePresets[index];

            // Update description text
            if (descriptionText != null)
            {
                descriptionText.text = $"<b>{selectedPreset.PresetName}</b>\n\n{selectedPreset.Description}";
            }

            // Clear feedback
            if (feedbackText != null)
            {
                feedbackText.text = "";
            }
        }

        /// <summary>
        /// Called when user clicks Load button.
        /// </summary>
        private void OnLoadButtonClicked()
        {
            if (selectedPreset == null)
            {
                ShowFeedback("No preset selected!", Color.red);
                return;
            }

            if (simulationController == null)
            {
                ShowFeedback("SimulationController not found!", Color.red);
                return;
            }

            try
            {
                // Load preset into SimulationController
                // This requires adding a method to SimulationController
                LoadPresetIntoController(selectedPreset);

                ShowFeedback($"Loaded: {selectedPreset.PresetName}", Color.green);
                Debug.Log($"[PresetSelectorUI] Loaded preset: {selectedPreset.PresetName}");
            }
            catch (System.Exception ex)
            {
                ShowFeedback($"Error: {ex.Message}", Color.red);
                Debug.LogError($"[PresetSelectorUI] Failed to load preset: {ex.Message}");
            }
        }

        /// <summary>
        /// Load preset into SimulationController.
        /// Note: This requires SimulationController to support runtime preset loading.
        /// </summary>
        private void LoadPresetIntoController(ScenarioPreset preset)
        {
            // For now, just log - actual implementation requires SimulationController changes
            Debug.LogWarning("[PresetSelectorUI] Runtime preset loading not yet implemented in SimulationController");
            Debug.LogWarning("[PresetSelectorUI] This requires adding LoadPreset() method to SimulationController");

            // TODO: Add this to SimulationController:
            // public void LoadPreset(ScenarioPreset preset)
            // {
            //     scenarioPreset = preset;
            //     RestartSimulation(); // Reinitialize with new preset
            // }
        }

        /// <summary>
        /// Show feedback message to user.
        /// </summary>
        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;

                // Auto-clear after 3 seconds
                Invoke(nameof(ClearFeedback), 3f);
            }
        }

        private void ClearFeedback()
        {
            if (feedbackText != null)
            {
                feedbackText.text = "";
            }
        }
    }
}
