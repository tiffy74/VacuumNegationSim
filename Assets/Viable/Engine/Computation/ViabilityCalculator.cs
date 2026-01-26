using System;
using Viable.Contracts;

namespace Viable.Engine.Computation
{
    /// <summary>
    /// Core viability calculation logic extracted from SimulationController.
    /// Computes persistence viability based on incoming flow, resource, and complexity.
    /// Stage 13.7: Adds hysteresis activation logic.
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

        /// <summary>
        /// Determine active state based on viability rule and current state.
        /// Stage 13.7: Implements hysteresis (separate on/off thresholds).
        /// </summary>
        /// <param name="viability">Current viability score</param>
        /// <param name="currentActive">Current activation state (0=inactive, 1=active)</param>
        /// <param name="resource">Current resource level</param>
        /// <param name="minBudgetToPropagate">Minimum resource required for propagation</param>
        /// <param name="viabilityRule">Activation rule (HardThreshold or Hysteresis)</param>
        /// <param name="onThreshold">Hysteresis: threshold to turn ON (ignored for HardThreshold)</param>
        /// <param name="offThreshold">Hysteresis: threshold to turn OFF (ignored for HardThreshold)</param>
        /// <returns>New active state (0=inactive, 1=active)</returns>
        public static byte DetermineActiveState(
            float viability,
            byte currentActive,
            float resource,
            float minBudgetToPropagate,
            ViabilityRule viabilityRule,
            double onThreshold,
            double offThreshold)
        {
            // Resource gate: must have minimum resource to be active
            if (resource <= minBudgetToPropagate)
                return 0;

            switch (viabilityRule)
            {
                case ViabilityRule.HardThreshold:
                    // Default: Simple threshold (viability > 0)
                    return (byte)(viability > 0f ? 1 : 0);

                case ViabilityRule.Hysteresis:
                    // Stage 13.7: Separate thresholds for activation and deactivation
                    if (currentActive == 0)
                    {
                        // Currently inactive: need to exceed ON threshold to activate
                        return (byte)(viability >= onThreshold ? 1 : 0);
                    }
                    else
                    {
                        // Currently active: need to drop below OFF threshold to deactivate
                        return (byte)(viability > offThreshold ? 1 : 0);
                    }

                default:
                    // Unknown rule: fallback to HardThreshold
                    return (byte)(viability > 0f ? 1 : 0);
            }
        }
    }
}
