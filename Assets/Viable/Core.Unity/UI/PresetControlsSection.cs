using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Preset Controls Section - Located in Right Dock Setup Panel.
    /// Contains: Preset dropdown and Load button.
    /// Stage 14: Moved from TopBar to RightDock for cleaner layout.
    /// Load button only loads into UI (doesn't restart sim).
    /// Apply button is separate at bottom of Setup Panel.
    /// </summary>
    public class PresetControlsSection : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown presetDropdown;
        [SerializeField] private Button loadPresetButton;

        [Header("References")]
        private Controllers.SimulationUIOrchestrator orchestrator;
        private NotificationUI notificationUI; // NEW: For feedback

        private void Start()
        {
            // Find references
            orchestrator = FindFirstObjectByType<Controllers.SimulationUIOrchestrator>();
            notificationUI = FindFirstObjectByType<NotificationUI>();

            // Populate and wire
            PopulatePresetDropdown();
            WireButtons();

            Debug.Log("[PresetControlsSection] Initialized in Right Dock");
        }

        private void WireButtons()
        {
            if (loadPresetButton != null)
            {
                loadPresetButton.onClick.RemoveAllListeners();
                loadPresetButton.onClick.AddListener(OnLoadPreset);
            }

            if (presetDropdown != null)
            {
                presetDropdown.onValueChanged.RemoveAllListeners();
                presetDropdown.onValueChanged.AddListener(OnPresetDropdownChanged);
            }
        }

        private void PopulatePresetDropdown()
        {
            if (presetDropdown == null)
            {
                Debug.LogWarning("[PresetControlsSection] presetDropdown is null");
                return;
            }

            presetDropdown.ClearOptions();

            var presetNames = new System.Collections.Generic.List<string>();

#if UNITY_EDITOR
            // Editor: Load from AssetDatabase
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScenarioPreset", new[] { "Assets/Viable/Core.Unity/Presets/Examples" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var preset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScenarioPreset>(path);
                if (preset != null)
                {
                    presetNames.Add(preset.PresetName);
                }
            }
#else
            // Runtime: Load from Resources folder
            var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
            Debug.Log($"[PresetControlsSection] Loaded {presets.Length} presets from Resources/Presets/Examples");
            
            presetNames.AddRange(presets.Select(p => p.PresetName));

            if (presetNames.Count == 0)
            {
                Debug.LogWarning("[PresetControlsSection] No presets found in Resources/Presets/Examples, trying Resources/Presets");
                presets = Resources.LoadAll<ScenarioPreset>("Presets");
                presetNames.AddRange(presets.Select(p => p.PresetName));
            }
#endif

            // Fallback to dummy data if no presets found
            if (presetNames.Count == 0)
            {
                Debug.LogWarning("[PresetControlsSection] No presets found, using fallback list");
                presetNames.AddRange(new System.Collections.Generic.List<string>
                {
                    "Default",
                    "No Presets Found"
                });
            }
            else
            {
                // Sort alphabetically first
                presetNames.Sort();
                
                // Find "Default" preset (prioritize exact match, then partial match)
                int defaultIndex = presetNames.FindIndex(p => p.Equals("Default", System.StringComparison.OrdinalIgnoreCase));
                
                // If no exact "Default", look for presets starting with "00_" or containing "Default"
                if (defaultIndex < 0)
                {
                    defaultIndex = presetNames.FindIndex(p => 
                        p.StartsWith("00_", System.StringComparison.OrdinalIgnoreCase) || 
                        p.Contains("Default"));
                }
                
                // Move to front if found (and not already at front)
                if (defaultIndex > 0)
                {
                    string defaultPreset = presetNames[defaultIndex];
                    presetNames.RemoveAt(defaultIndex);
                    presetNames.Insert(0, defaultPreset);
                    Debug.Log($"[PresetControlsSection] Moved '{defaultPreset}' to front of preset list");
                }
            }

            presetDropdown.AddOptions(presetNames);
            presetDropdown.value = 0; // Select first item (Default)
            presetDropdown.RefreshShownValue();
            
            Debug.Log($"[PresetControlsSection] Dropdown populated with {presetNames.Count} presets. First preset: '{presetNames[0]}'");
        }

        private void OnPresetDropdownChanged(int index)
        {
            Debug.Log($"[PresetControlsSection] Preset dropdown changed to index {index}: {presetDropdown.options[index].text}");
            
            // Force close the dropdown immediately
            StartCoroutine(CloseDropdownDelayed(presetDropdown));
        }

        private System.Collections.IEnumerator CloseDropdownDelayed(TMP_Dropdown dropdown)
        {
            yield return null;
            
            if (dropdown != null)
            {
                dropdown.Hide();
                
                var template = dropdown.template;
                if (template != null && template.gameObject.activeSelf)
                {
                    template.gameObject.SetActive(false);
                }
            }
        }

        private void OnLoadPreset()
        {
            if (presetDropdown == null || presetDropdown.options.Count == 0)
            {
                Debug.LogError("[PresetControlsSection] No presets available!");
                ShowNotification("Error: No presets available", NotificationUI.NotificationType.Error);
                return;
            }

            int selectedIndex = presetDropdown.value;
            string presetName = presetDropdown.options[selectedIndex].text;
            Debug.Log($"[PresetControlsSection] Load Preset: {presetName}");

            // Find the actual preset asset
            ScenarioPreset selectedPreset = null;

#if UNITY_EDITOR
            // Editor: Load from AssetDatabase
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScenarioPreset", new[] { "Assets/Viable/Core.Unity/Presets/Examples" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var preset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScenarioPreset>(path);
                if (preset != null && preset.PresetName == presetName)
                {
                    selectedPreset = preset;
                    break;
                }
            }
#else
            // Runtime: Load from Resources
            var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
            selectedPreset = System.Array.Find(presets, p => p.PresetName == presetName);

            if (selectedPreset == null)
            {
                Debug.LogWarning($"[PresetControlsSection] Preset '{presetName}' not found in Presets/Examples, trying Presets");
                presets = Resources.LoadAll<ScenarioPreset>("Presets");
                selectedPreset = System.Array.Find(presets, p => p.PresetName == presetName);
            }
#endif

            if (selectedPreset == null)
            {
                Debug.LogError($"[PresetControlsSection] Could not find preset: {presetName}");
                ShowNotification($"Error: Preset '{presetName}' not found", NotificationUI.NotificationType.Error);
                return;
            }

            // Load preset into UI ONLY (doesn't restart simulation)
            if (orchestrator != null)
            {
                orchestrator.LoadPreset(selectedPreset);
                Debug.Log($"[PresetControlsSection] ? Loaded preset into UI: {presetName}");
                ShowNotification($"? Preset loaded: {presetName}", NotificationUI.NotificationType.Success);
            }
            else
            {
                Debug.LogError("[PresetControlsSection] Orchestrator not found!");
                ShowNotification("Error: UI Orchestrator not found", NotificationUI.NotificationType.Error);
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
                Debug.LogWarning($"[PresetControlsSection] NotificationUI not found, message: {message}");
            }
        }

        /// <summary>
        /// Programmatically select a preset by name (called by orchestrator on load).
        /// </summary>
        public void SelectPreset(ScenarioPreset preset)
        {
            if (presetDropdown == null || preset == null) return;

            int index = presetDropdown.options.FindIndex(opt => opt.text == preset.PresetName);
            if (index >= 0)
            {
                presetDropdown.value = index;
                presetDropdown.RefreshShownValue();
                Debug.Log($"[PresetControlsSection] Selected preset: {preset.PresetName}");
            }
        }
    }
}
