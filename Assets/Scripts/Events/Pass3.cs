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
        /// Recharges the global energy pool, capping at the maximum.
        /// </summary>
        /// <param name="NGlobal">Current global energy (ref)</param>
        /// <param name="NGlobalMax">Maximum global energy</param>
        /// <param name="GlobalReplenishPerTick">Amount to replenish per tick</param>
        public static void GlobalRecharge(ref float NGlobal, float NGlobalMax, float GlobalReplenishPerTick)
        {
            NGlobal = Math.Min(NGlobalMax, NGlobal + GlobalReplenishPerTick);
        }
    }
}

