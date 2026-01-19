using System;

namespace Viable.Engine.Steps
{
    /// <summary>
    /// Phase 3: Recharge the global resource pool.
    /// </summary>
    public static class RechargePhase
    {
        /// <summary>
        /// Recharges the global resource pool, capping at the maximum.
        /// </summary>
        /// <param name="ResourceGlobal">Current global resource (ref)</param>
        /// <param name="ResourceGlobalMax">Maximum global resource</param>
        /// <param name="GlobalReplenishPerTick">Amount to replenish per tick</param>
        public static void GlobalRecharge(ref float ResourceGlobal, float ResourceGlobalMax, float GlobalReplenishPerTick)
        {
            ResourceGlobal = Math.Min(ResourceGlobalMax, ResourceGlobal + GlobalReplenishPerTick);
        }
    }
}
