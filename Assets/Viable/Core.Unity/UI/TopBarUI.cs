using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private Controllers.SimulationController simulationController;
        private Configuration.WorkingScenarioConfig workingConfig;
        private bool isPlaying = false;

        public void Initialize(Controllers.SimulationController controller, Configuration.WorkingScenarioConfig config)
        {
            simulationController = controller;
            workingConfig = config;

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

            // Populate speed dropdown
            PopulateSpeedDropdown();

            // Load preset list
            PopulatePresetDropdown();

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
            if (presetDropdown == null) return;

            // TODO: Load from PresetLibrary
            presetDropdown.ClearOptions();
            presetDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Default",
                "Circle Domain",
                "Point Sources",
                "Hysteresis Test"
            });
            presetDropdown.value = 0;
        }

        private void OnLoadPreset()
        {
            Debug.Log($"[TopBarUI] Load Preset: {presetDropdown.options[presetDropdown.value].text}");
            // TODO: Load preset into workingConfig
            // UIController.LoadPreset(presetDropdown.value);
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
            Debug.Log("[TopBarUI] Export");
            // TODO: Trigger export via UIController
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
        }

        void Update()
        {
            // Update seed display if it changed externally
            if (seedInput != null && !seedInput.isFocused)
            {
                seedInput.text = workingConfig.Seed.ToString();
            }
        }
    }
}
