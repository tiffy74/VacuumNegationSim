using Assets.Scripts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Simulation
{
    public sealed class SimulationEngine
    {
        private readonly List<ISimStep> _steps;
        public GridState State { get; }

        public SimulationEngine(GridState state, IEnumerable<ISimStep> steps)
        {
            State = state;
            _steps = new List<ISimStep>(steps);
        }

        public void Tick(SimContext ctx)
        {
            for (int i = 0; i < _steps.Count; i++)
                _steps[i].Execute(State, ctx);

            ctx.Tick++;
        }
    }
}
