using Assets.Scripts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [DEPRECATED - Phase 5] This file will be removed in Phase 7
// New location: Assets/Viable/Engine/SimulationRunner.cs
// DO NOT modify this file - changes go to new location

namespace Assets.Scripts.Simulation
{
    public sealed class SimulationEngine
    {
        private readonly List<ISimStep> _steps;
        public StateGrid State { get; }

        public SimulationEngine(StateGrid state, IEnumerable<ISimStep> steps)
        {
            State = state;
            _steps = new List<ISimStep>(steps);
        }

        public void Tick(SimContext ctx)
        {
            if (ctx.Tick % 10 == 0)
                UnityEngine.Debug.Log($"[Tick {ctx.Tick}] Engine.Tick start (B)");

            for (int i = 0; i < _steps.Count; i++)
                _steps[i].Execute(State, ctx);

            ctx.Tick++;
        }
    }
}
