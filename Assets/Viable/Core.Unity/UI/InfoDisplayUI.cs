using UnityEngine;
using TMPro;

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
        [SerializeField] private int updateFrequency = 10; // Update every N frames

        private Controllers.SimulationController simulationController;
        private int frameCounter = 0;

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;
        }

        public void UpdateDisplay()
        {
            if (simulationController == null) return;

            // Update every N frames to reduce overhead
            frameCounter++;
            if (frameCounter < updateFrequency) return;
            frameCounter = 0;

            // Get metrics from SimulationController
            // Note: This requires adding public getters to SimulationController
            UpdateMetrics();
        }

        private void UpdateMetrics()
        {
            // TODO: Get actual metrics from SimulationController
            // For now, show placeholders

            if (tickText != null)
            {
                tickText.text = $"Tick: [N/A]";
                // TODO: tickText.text = $"Tick: {simulationController.GetCurrentTick()}";
            }

            if (viableCountText != null)
            {
                viableCountText.text = $"Viable: [N/A]";
                // TODO: Get from result.SummaryMetrics["viableCount"]
            }

            if (activeCountText != null)
            {
                activeCountText.text = $"Active: [N/A]";
                // TODO: Get from result.SummaryMetrics["activeCount"]
            }

            if (sinkCountText != null)
            {
                sinkCountText.text = $"Sinks: [N/A]";
                // TODO: Get from result.SummaryMetrics["sinkCount"]
            }

            if (resourceGlobalText != null)
            {
                resourceGlobalText.text = $"Resource: [N/A]";
                // TODO: Get from context.ResourceGlobal
            }

            // Note: SimulationController needs to expose:
            // public int GetCurrentTick() => context.Tick;
            // public GridState GetCurrentState() => state;
            // public StepContext GetCurrentContext() => context;
        }
    }
}
