using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [DEPRECATED - Phase 2] This file will be removed in Phase 7
// New location: Assets/Viable/Engine/Execution/StepContext.cs
// DO NOT modify this file - changes go to new location

namespace Assets.Scripts.Domain
{
    public sealed class SimContext
    {
        public readonly SimConfig Cfg;
        public int Tick;
        public float ResourceGlobal;
        public float ScaleFactor;

        public SimContext(SimConfig cfg, float initialGlobal, float initialScale)
        {
            Cfg = cfg;
            ResourceGlobal = initialGlobal;
            ScaleFactor = initialScale;
            Tick = 0;
        }
    }
}
