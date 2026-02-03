using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Top bar UI controller - always visible, single row.
    /// Contains: Preset dropdown, Load button, Run controls, Speed, Seed, Export.
    /// </summary>
    public class TopBarUI : MonoBehaviour
    {
        [Header("Preset Controls")]
        [SerializeField] private TMP_Dropdown presetDropdown;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button applyRestartButton;

        [Header("Run Controls")]
        [SerializeField] private Button playPauseButton;
        [SerializeField] private TextMeshProUGUI playPauseButtonText;
        [SerializeField] private Button stepButton;
        [SerializeField] private Button restartButton;

        [Header("Configuration")]
        [SerializeField] private TMP_Dropdown speedDropdown;
        [SerializeField] private TMP_InputField seedInput;

        [Header("Export")]
        [SerializeField] private Button exportButton;
        
        [Header("Button Feedback")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private float buttonFeedbackDuration = 0.1f;
        [SerializeField] [Range(0.5f, 1.5f)] private float hoverBrightnessMultiplier = 1.2f;
        [SerializeField] [Range(0.3f, 0.9f)] private float pressedDarknessMultiplier = 0.7f;
        
        private AudioSource audioSource;

        private Controllers.SimulationController simulationController;
        private Configuration.WorkingScenarioConfig workingConfig;
        private bool isPlaying = false;

        private void Start()
        {
            // Find SimulationController if not assigned
            if (simulationController == null)
            {
                simulationController = FindFirstObjectByType<Controllers.SimulationController>();
            }

            // Setup audio source for button feedback
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null && buttonClickSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            // Populate dropdowns on start
            PopulateSpeedDropdown();
            PopulatePresetDropdown();

            // Wire buttons with feedback
            if (loadButton != null)
            {
                loadButton.onClick.RemoveAllListeners(); // Clear any existing
                loadButton.onClick.AddListener(() => OnButtonClick(OnLoadPreset));
                SetupButtonVisualFeedback(loadButton);
            }

            if (applyRestartButton != null)
            {
                applyRestartButton.onClick.RemoveAllListeners();
                applyRestartButton.onClick.AddListener(() => OnButtonClick(OnApplyAndRestart));
                SetupButtonVisualFeedback(applyRestartButton);
            }

            if (playPauseButton != null)
            {
                playPauseButton.onClick.RemoveAllListeners();
                playPauseButton.onClick.AddListener(() => OnButtonClick(OnTogglePlayPause));
                SetupButtonVisualFeedback(playPauseButton);
            }

            if (stepButton != null)
            {
                stepButton.onClick.RemoveAllListeners();
                stepButton.onClick.AddListener(() => OnButtonClick(OnStep));
                SetupButtonVisualFeedback(stepButton);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => OnButtonClick(OnRestart));
                SetupButtonVisualFeedback(restartButton);
            }

            if (exportButton != null)
            {
                exportButton.onClick.RemoveAllListeners();
                exportButton.onClick.AddListener(() => OnButtonClick(OnExport));
                SetupButtonVisualFeedback(exportButton);
            }

            // Wire seed input
            if (seedInput != null)
            {
                seedInput.onEndEdit.AddListener(OnSeedChanged);
            }

            UpdatePlayPauseButton();

            Debug.Log("[TopBarUI] Initialized with button feedback");
        }

        public void Initialize(Controllers.SimulationController controller, Configuration.WorkingScenarioConfig config)
        {
            simulationController = controller;
            workingConfig = config;

            // Populate dropdowns
            PopulateSpeedDropdown();
            PopulatePresetDropdown();

            // Wire buttons
            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadPreset);

            if (applyRestartButton != null)
                applyRestartButton.onClick.AddListener(OnApplyAndRestart);

            if (playPauseButton != null)
                playPauseButton.onClick.AddListener(OnTogglePlayPause);

            if (stepButton != null)
                stepButton.onClick.AddListener(OnStep);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestart);

            if (exportButton != null)
                exportButton.onClick.AddListener(OnExport);

            // Wire seed input
            if (seedInput != null)
            {
                seedInput.onEndEdit.AddListener(OnSeedChanged);
            }

            UpdatePlayPauseButton();
        }

        private void PopulateSpeedDropdown()
        {
            if (speedDropdown == null) return;

            speedDropdown.ClearOptions();
            speedDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "1 step/frame",
                "5 steps/frame",
                "10 steps/frame",
                "50 steps/frame",
                "100 steps/frame"
            });
            speedDropdown.value = 0;
            speedDropdown.onValueChanged.AddListener(OnSpeedChanged);
        }

        private void PopulatePresetDropdown()
        {
            if (presetDropdown == null)
            {
                Debug.LogWarning("[TopBarUI] presetDropdown is null");
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
            // Runtime: Load from Resources
            var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
            presetNames.AddRange(presets.Select(p => p.PresetName));

            if (presetNames.Count == 0)
            {
                presets = Resources.LoadAll<ScenarioPreset>("Presets");
                presetNames.AddRange(presets.Select(p => p.PresetName));
            }
#endif

            // Fallback to dummy data if no presets found
            if (presetNames.Count == 0)
            {
                Debug.LogWarning("[TopBarUI] No presets found, using fallback list");
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
                Debug.Log($"[TopBarUI] After sort, presets: {string.Join(", ", presetNames)}");
                
                // Find "Default" preset (prioritize exact match, then partial match)
                int defaultIndex = presetNames.FindIndex(p => p.Equals("Default", System.StringComparison.OrdinalIgnoreCase));
                Debug.Log($"[TopBarUI] Found 'Default' at index: {defaultIndex}");
                
                // If no exact "Default", look for presets starting with "00_" or containing "Default"
                if (defaultIndex < 0)
                {
                    defaultIndex = presetNames.FindIndex(p => 
                        p.StartsWith("00_", System.StringComparison.OrdinalIgnoreCase) || 
                        p.Contains("Default"));
                    Debug.Log($"[TopBarUI] Fallback search found preset at index: {defaultIndex}");
                }
                
                // Move to front if found (and not already at front)
                if (defaultIndex > 0)
                {
                    string defaultPreset = presetNames[defaultIndex];
                    presetNames.RemoveAt(defaultIndex);
                    presetNames.Insert(0, defaultPreset);
                    Debug.Log($"[TopBarUI] ? Moved '{defaultPreset}' to front of preset list");
                }
                else if (defaultIndex == 0)
                {
                    Debug.Log($"[TopBarUI] ? '{presetNames[0]}' already at front of preset list");
                }
                else
                {
                    Debug.LogWarning($"[TopBarUI] ?? Could not find 'Default' preset. First preset: '{presetNames[0]}'");
                }
                
                Debug.Log($"[TopBarUI] Final preset order: {string.Join(", ", presetNames)}");
            }

            presetDropdown.AddOptions(presetNames);
            presetDropdown.value = 0; // Select first item (Default)
            presetDropdown.RefreshShownValue(); // Force UI update
        }

        private void OnLoadPreset()
        {
            if (presetDropdown == null || presetDropdown.options.Count == 0)
            {
                Debug.LogError("[TopBarUI] No presets available!");
                return;
            }

            if (simulationController == null)
            {
                Debug.LogError("[TopBarUI] SimulationController not found!");
                return;
            }

            int selectedIndex = presetDropdown.value;
            string presetName = presetDropdown.options[selectedIndex].text;
            Debug.Log($"[TopBarUI] Load Preset: {presetName}");

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
                presets = Resources.LoadAll<ScenarioPreset>("Presets");
                selectedPreset = System.Array.Find(presets, p => p.PresetName == presetName);
            }
#endif

            if (selectedPreset == null)
            {
                Debug.LogError($"[TopBarUI] Could not find preset: {presetName}");
                return;
            }

            // Load preset into SimulationController
            simulationController.LoadPreset(selectedPreset);
            Debug.Log($"[TopBarUI] ? Loaded preset: {presetName}");
        }

        private void OnApplyAndRestart()
        {
            Debug.Log("[TopBarUI] Apply & Restart");
            // TODO: Convert workingConfig ? ScenarioDefinition/RunRequest
            // simulationController.ResetAndRun(...)
        }

        private void OnTogglePlayPause()
        {
            isPlaying = !isPlaying;
            
            if (isPlaying)
            {
                simulationController?.Play();
            }
            else
            {
                simulationController?.Pause();
            }

            UpdatePlayPauseButton();
        }

        private void OnStep()
        {
            simulationController?.Step();
        }

        private void OnRestart()
        {
            Debug.Log("[TopBarUI] Restart");
            simulationController?.RestartSimulation();
        }

        private void OnExport()
        {
            if (simulationController == null)
            {
                Debug.LogError("[TopBarUI] SimulationController not found!");
                return;
            }

            Debug.Log("[TopBarUI] Exporting current run...");
            
            try
            {
                string exportPath = simulationController.ExportLastRunWithPath();
                if (!string.IsNullOrEmpty(exportPath))
                {
                    Debug.Log($"[TopBarUI] ? Export complete: {exportPath}");
                }
                else
                {
                    Debug.LogWarning("[TopBarUI] Export returned empty path");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TopBarUI] Export failed: {ex.Message}");
            }
        }

        private void OnSeedChanged(string value)
        {
            if (int.TryParse(value, out int seed))
            {
                workingConfig.Seed = seed;
                Debug.Log($"[TopBarUI] Seed changed to {seed}");
            }
        }

        private void OnSpeedChanged(int index)
        {
            int[] speeds = { 1, 5, 10, 50, 100 };
            if (index >= 0 && index < speeds.Length)
            {
                // TODO: Set simulation speed
                Debug.Log($"[TopBarUI] Speed changed to {speeds[index]} steps/frame");
            }
        }

        private void UpdatePlayPauseButton()
        {
            if (playPauseButtonText != null)
            {
                playPauseButtonText.text = isPlaying ? "Pause" : "Play";
            }
            
            // Add visual pulse when state changes
            if (playPauseButton != null)
            {
                StartCoroutine(PulseButton(playPauseButton));
            }
        }
        
        /// <summary>
        /// Pulse effect for button to show state change.
        /// </summary>
        private System.Collections.IEnumerator PulseButton(Button button)
        {
            if (button == null) yield break;
            
            var colors = button.colors;
            var originalColor = colors.normalColor;
            
            // Brighten
            colors.normalColor = new Color(
                Mathf.Min(originalColor.r * 1.3f, 1f),
                Mathf.Min(originalColor.g * 1.3f, 1f),
                Mathf.Min(originalColor.b * 1.3f, 1f),
                originalColor.a
            );
            button.colors = colors;
            
            yield return new WaitForSeconds(0.15f);
            
            // Return to normal
            colors.normalColor = originalColor;
            button.colors = colors;
        }

        void Update()
        {
            // Update seed display if it changed externally
            if (workingConfig != null && seedInput != null && !seedInput.isFocused)
            {
                seedInput.text = workingConfig.Seed.ToString();
            }
        }

        // ===== Button Feedback System =====

        /// <summary>
        /// Wrapper for button clicks that adds visual and audio feedback.
        /// </summary>
        private void OnButtonClick(System.Action buttonAction)
        {
            // Play click sound if available
            if (audioSource != null && buttonClickSound != null)
            {
                audioSource.PlayOneShot(buttonClickSound);
            }

            // Execute the actual button action
            buttonAction?.Invoke();
        }

        /// <summary>
        /// Setup visual feedback for button (color transition on click and hover).
        /// Makes hover state more prominent and pressed state darker.
        /// </summary>
        private void SetupButtonVisualFeedback(Button button)
        {
            if (button == null) return;

            var colors = button.colors;
            
            // Ensure visual feedback is enabled
            colors.colorMultiplier = 1f;
            colors.fadeDuration = buttonFeedbackDuration;
            
            // Always set pressed color (darker)
            colors.pressedColor = new Color(
                colors.normalColor.r * pressedDarknessMultiplier,
                colors.normalColor.g * pressedDarknessMultiplier,
                colors.normalColor.b * pressedDarknessMultiplier,
                colors.normalColor.a
            );
            
            // Always set highlighted/hover color (brighter)
            colors.highlightedColor = new Color(
                Mathf.Min(colors.normalColor.r * hoverBrightnessMultiplier, 1f),
                Mathf.Min(colors.normalColor.g * hoverBrightnessMultiplier, 1f),
                Mathf.Min(colors.normalColor.b * hoverBrightnessMultiplier, 1f),
                colors.normalColor.a
            );
            
            // Selected color (for toggle-like buttons) - slightly brighter than normal
            colors.selectedColor = new Color(
                Mathf.Min(colors.normalColor.r * 1.05f, 1f),
                Mathf.Min(colors.normalColor.g * 1.05f, 1f),
                Mathf.Min(colors.normalColor.b * 1.05f, 1f),
                colors.normalColor.a
            );
            
            // Disabled color (semi-transparent)
            colors.disabledColor = new Color(
                colors.normalColor.r * 0.5f,
                colors.normalColor.g * 0.5f,
                colors.normalColor.b * 0.5f,
                colors.normalColor.a * 0.5f
            );

            button.colors = colors;
            
            // Log the setup for debugging
            Debug.Log($"[TopBarUI] Setup feedback for button '{button.gameObject.name}': " +
                     $"Hover={hoverBrightnessMultiplier}x, Press={pressedDarknessMultiplier}x");
        }
    }
}
