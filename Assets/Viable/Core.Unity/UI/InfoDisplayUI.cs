using TMPro;
using UnityEngine;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel displaying live simulation metrics.
    /// Shows tick, viable cells, active cells, sinks, global resource.
    /// Stage 12: Essential UI component.
    /// </summary>
    public class InfoDisplayUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI tickText;
        [SerializeField] private TextMeshProUGUI viableCountText;
        [SerializeField] private TextMeshProUGUI activeCountText;
        [SerializeField] private TextMeshProUGUI sinkCountText;
        [SerializeField] private TextMeshProUGUI resourceGlobalText;

        [Header("Settings")]
        [SerializeField] private int updateFrequency = 1; // Update every frame for now

        private Controllers.SimulationController simulationController;
        private int frameCounter = 0;

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;
            Debug.Log("[InfoDisplayUI] Initialized with SimulationController");
        }

        public void UpdateDisplay()
        {
            if (simulationController == null)
            {
                Debug.LogWarning("[InfoDisplayUI] SimulationController is null!");
                return;
            }

            // Update every N frames to reduce overhead
            frameCounter++;
            if (frameCounter < updateFrequency) return;
            frameCounter = 0;

            // Get metrics from SimulationController
            UpdateMetrics();
        }

        private void UpdateMetrics()
        {
            try
            {
                // Get current metrics from SimulationController
                var metrics = simulationController.GetCurrentMetrics();
                var context = simulationController.GetCurrentContext();

                if (tickText != null)
                {
                    int tick = simulationController.GetCurrentTick();
                    tickText.text = $"Tick: {tick}";
                }

                if (viableCountText != null && metrics != null && metrics.ContainsKey("viableCount"))
                {
                    viableCountText.text = $"Viable: {metrics["viableCount"]:F0}";
                }

                if (activeCountText != null && metrics != null && metrics.ContainsKey("activeCount"))
                {
                    activeCountText.text = $"Active: {metrics["activeCount"]:F0}";
                }

                if (sinkCountText != null && metrics != null && metrics.ContainsKey("sinkCount"))
                {
                    sinkCountText.text = $"Sinks: {metrics["sinkCount"]:F0}";
                }

                if (resourceGlobalText != null && context != null)
                {
                    resourceGlobalText.text = $"Resource: {context.ResourceGlobal:F0}";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InfoDisplayUI] Error updating metrics: {ex.Message}");
            }
        }
    }
}
