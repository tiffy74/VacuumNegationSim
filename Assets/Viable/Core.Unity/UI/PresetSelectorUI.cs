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

        [Header("Orchestrator")]
        [SerializeField] private Controllers.SimulationUIOrchestrator orchestrator;

        private Controllers.SimulationController simulationController;
        private List<ScenarioPreset> availablePresets;
        private ScenarioPreset selectedPreset;

        void Awake()
        {
            // Auto-wire orchestrator if not set
            if (orchestrator == null)
                orchestrator = FindFirstObjectByType<Controllers.SimulationUIOrchestrator>();
        }

        void Start()
        {
            // Initialize UI after orchestrator has loaded default preset
            LoadAvailablePresets();
            SetupUI();
            
            // If orchestrator has a current preset, select it in dropdown
            if (orchestrator != null && orchestrator.CurrentPreset != null)
            {
                SelectPreset(orchestrator.CurrentPreset);
            }
        }

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;
            
            // If Initialize is called manually, do setup
            if (availablePresets == null || availablePresets.Count == 0)
            {
                LoadAvailablePresets();
                SetupUI();
            }
        }

        /// <summary>
        /// Load all available presets from Resources or specified folder.
        /// </summary>
        private void LoadAvailablePresets()
        {
            availablePresets = new List<ScenarioPreset>();

#if UNITY_EDITOR
            // Editor: Load from anywhere using AssetDatabase
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScenarioPreset", new[] { "Assets/Viable/Core.Unity/Presets/Examples" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var preset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScenarioPreset>(path);
                if (preset != null)
                {
                    availablePresets.Add(preset);
                }
            }
#else
            // Runtime: Load from Resources folder
            var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
            availablePresets.AddRange(presets);

            if (availablePresets.Count == 0)
            {
                presets = Resources.LoadAll<ScenarioPreset>("Presets");
                availablePresets.AddRange(presets);
            }
#endif

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

            // DON'T select a preset here - let orchestrator do it in Start()
            // This ensures the default preset from orchestrator is respected
            Debug.Log($"[PresetSelectorUI] Dropdown populated with {availablePresets.Count} presets. Waiting for orchestrator to set default.");
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
            
            // Load preset into orchestrator (doesn't apply until user clicks Apply button)
            if (orchestrator != null)
            {
                orchestrator.LoadPreset(selectedPreset);
            }
        }

        /// <summary>
        /// Programmatically select a preset by reference.
        /// Used by orchestrator to sync dropdown on startup.
        /// </summary>
        public void SelectPreset(ScenarioPreset preset)
        {
            if (preset == null || availablePresets == null) return;

            int index = availablePresets.IndexOf(preset);
            if (index >= 0)
            {
                presetDropdown.SetValueWithoutNotify(index);
                OnPresetSelected(index);
            }
        }

        /// <summary>
        /// Called when user clicks Load button.
        /// NOTE: This is deprecated - use TopBar Apply button instead.
        /// Kept for backward compatibility but now just shows message.
        /// </summary>
        private void OnLoadButtonClicked()
        {
            if (selectedPreset == null)
            {
                ShowFeedback("No preset selected!", Color.red);
                return;
            }

            // Show message that changes are pending Apply
            ShowFeedback($"Selected: {selectedPreset.PresetName}. Click Apply in TopBar to restart simulation.", Color.yellow);
            Debug.Log($"[PresetSelectorUI] Preset selected: {selectedPreset.PresetName} (pending Apply)");
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
