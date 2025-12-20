using Assets.Scripts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Simulation
{
    public interface ISimStep
    {
        void Execute(GridState s, SimContext ctx);
    }
}
