using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Domain
{
    public sealed class SimContext
    {
        public readonly SimConfig Cfg;
        public int Tick;
        public float NGlobal;
        public float ScaleFactor;

        public SimContext(SimConfig cfg, float initialGlobal, float initialScale)
        {
            Cfg = cfg;
            NGlobal = initialGlobal;
            ScaleFactor = initialScale;
            Tick = 0;
        }
    }
}
