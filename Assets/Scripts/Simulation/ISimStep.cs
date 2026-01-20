using Assets.Scripts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [DEPRECATED - Phase 5] This file will be removed in Phase 7
// New location: Assets/Viable/Engine/Interfaces/IStepPhase.cs
// DO NOT modify this file - changes go to new location

namespace Assets.Scripts.Simulation
{
    public interface ISimStep
    {
        void Execute(StateGrid s, SimContext ctx);
    }
}
