using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public static class Pass3
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

