using System;

namespace Assets.Scripts.Events
{
    /// <summary>
    /// Pass 3: Global Energy Pool Replenishment
    /// 
    /// Manages the global energy reservoir that supplies activation costs
    /// and maintains overall energy balance in the system.
    /// Acts as a "cosmological constant" or vacuum energy source.
    /// </summary>
    public static class Pass3
    {
        // ============================================================================
        // PUBLIC API: GLOBAL ENERGY MANAGEMENT
        // ============================================================================

        /// <summary>
        /// Replenishes the global energy pool at a constant rate, capped at maximum.
        /// This represents continuous energy inflow from the vacuum or external source.
        /// 
        /// Physical analogy: Dark energy or vacuum energy density replenishment.
        /// </summary>
        /// <param name="NGlobal">Current global energy (modified in place)</param>
        /// <param name="NGlobalMax">Maximum capacity of global energy pool</param>
        /// <param name="GlobalReplenishPerTick">Amount of energy added per tick</param>
        public static void GlobalRecharge(ref float NGlobal, float NGlobalMax, float GlobalReplenishPerTick)
        {
            ReplenishGlobalEnergyPool(ref NGlobal, NGlobalMax, GlobalReplenishPerTick);
        }

        // ============================================================================
        // PRIVATE: ENERGY REPLENISHMENT
        // ============================================================================

        /// <summary>
        /// Adds replenishment energy to the global pool, respecting maximum capacity.
        /// Energy cannot exceed NGlobalMax (conservation + capacity constraint).
        /// </summary>
        private static void ReplenishGlobalEnergyPool(ref float NGlobal, float NGlobalMax, float GlobalReplenishPerTick)
        {
            float newGlobal = NGlobal + GlobalReplenishPerTick;
            NGlobal = Math.Min(NGlobalMax, newGlobal);
        }
    }
}

