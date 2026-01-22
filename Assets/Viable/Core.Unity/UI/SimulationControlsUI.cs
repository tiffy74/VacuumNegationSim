using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for controlling simulation playback.
    /// Play, Pause, Stop, Speed control, and tick display.
    /// Stage 12: Essential UI component.
    /// </summary>
    public class SimulationControlsUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI tickText;

        [Header("Button Icons (Optional)")]
        [SerializeField] private TextMeshProUGUI playButtonText;
        [SerializeField] private TextMeshProUGUI pauseButtonText;
        [SerializeField] private TextMeshProUGUI stopButtonText;

        [Header("Settings")]
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private float maxSpeed = 10f;

        private Controllers.SimulationController simulationController;
        private bool isPlaying = false;

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;
            SetupUI();
        }

        private void SetupUI()
        {
            // Setup button listeners
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (stopButton != null)
                stopButton.onClick.AddListener(OnStopClicked);

            // Setup speed slider
            if (speedSlider != null)
            {
                speedSlider.minValue = minSpeed;
                speedSlider.maxValue = maxSpeed;
                speedSlider.value = 1f;
                speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            }

            // Set button text/icons
            if (playButtonText != null)
                playButtonText.text = "?";

            if (pauseButtonText != null)
                pauseButtonText.text = "?";

            if (stopButtonText != null)
                stopButtonText.text = "?";

            UpdateSpeedDisplay(1f);
            UpdateButtonStates();
        }

        public void UpdateDisplay()
        {
            if (simulationController == null) return;

            // Update tick display
            // Note: Requires adding GetCurrentTick() to SimulationController
            // For now, show placeholder
            if (tickText != null)
            {
                // TODO: Get actual tick from SimulationController
                tickText.text = $"Tick: [N/A]";
            }
        }

        private void OnPlayClicked()
        {
            if (simulationController == null) return;

            simulationController.Play();
            isPlaying = true;
            UpdateButtonStates();

            Debug.Log("[SimulationControlsUI] Play clicked");
        }

        private void OnPauseClicked()
        {
            if (simulationController == null) return;

            simulationController.Pause();
            isPlaying = false;
            UpdateButtonStates();

            Debug.Log("[SimulationControlsUI] Pause clicked");
        }

        private void OnStopClicked()
        {
            if (simulationController == null) return;

            simulationController.Pause();
            simulationController.RestartSimulation();
            isPlaying = false;
            UpdateButtonStates();

            Debug.Log("[SimulationControlsUI] Stop clicked");
        }

        private void OnSpeedChanged(float value)
        {
            // Update time scale
            Time.timeScale = value;
            UpdateSpeedDisplay(value);

            Debug.Log($"[SimulationControlsUI] Speed changed to {value}x");
        }

        private void UpdateSpeedDisplay(float speed)
        {
            if (speedText != null)
            {
                speedText.text = $"{speed:F1}x";
            }
        }

        private void UpdateButtonStates()
        {
            // Enable/disable buttons based on state
            if (playButton != null)
                playButton.interactable = !isPlaying;

            if (pauseButton != null)
                pauseButton.interactable = isPlaying;

            // Stop always enabled
        }
    }
}
