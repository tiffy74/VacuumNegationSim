using System;

namespace Viable.Engine.Computation
{
    /// <summary>
    /// Core viability calculation logic extracted from SimulationController.
    /// Computes persistence viability based on incoming flow, resource, and complexity.
    /// </summary>
    public static class ViabilityCalculator
    {
        private const float MinViabilityEpsilon = 1e-6f; // Prevent division by zero

        /// <summary>
        /// Compute viability score for a cell.
        /// Viability = (incomingFlow * complexityGain - decayLoss) / effectiveThreshold
        /// </summary>
        /// <param name="incomingFlow">Resource flow into the cell</param>
        /// <param name="resource">Current resource level (unused in current formula but available for future)</param>
        /// <param name="complexity">Structural complexity metric [0..1]</param>
        /// <param name="complexityGainA">Amplitude of complexity gain</param>
        /// <param name="complexityGainK">Decay rate of complexity gain</param>
        /// <param name="decayLoss">Baseline resource decay per tick</param>
        /// <param name="thresholdEffective">Effective viability threshold (may vary with global state)</param>
        /// <returns>Viability score (positive = viable, negative = non-viable)</returns>
        public static float Compute(
            float incomingFlow,
            float resource,
            float complexity,
            float complexityGainA,
            float complexityGainK,
            float decayLoss,
            float thresholdEffective)
        {
            // Complexity provides multiplicative gain to incoming flow
            // gain = 1 + A * (1 - exp(-K * complexity))
            // This creates diminishing returns: more complexity ? higher gain, but saturates
            float gain = 1f + complexityGainA * (1f - MathF.Exp(-complexityGainK * Math.Max(0f, complexity)));
            
            // Viability = (effective inflow - decay) / threshold
            // Positive viability ? cell can persist
            // Negative viability ? cell will decay
            return (incomingFlow * gain - decayLoss) / Math.Max(MinViabilityEpsilon, thresholdEffective);
        }

        /// <summary>
        /// Compute effective viability threshold based on global resource scarcity.
        /// Threshold increases as global resources become scarce.
        /// </summary>
        /// <param name="resourceGlobal">Current global resource pool</param>
        /// <param name="resourceGlobalMax">Maximum global resource pool</param>
        /// <param name="thresholdBase">Base threshold value</param>
        /// <param name="scarcityK">Scarcity sensitivity parameter</param>
        /// <returns>Effective threshold (higher under scarcity)</returns>
        public static float ComputeEffectiveThreshold(
            float resourceGlobal,
            float resourceGlobalMax,
            float thresholdBase,
            float scarcityK)
        {
            // Scarcity = 1 - (current / max)
            // As resources deplete, scarcity ? 1, threshold increases
            float scarcity = 1f - (resourceGlobal / Math.Max(1f, resourceGlobalMax));
            return thresholdBase * (1f + scarcityK * scarcity);
        }
    }
}
