using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Domain
{
    public sealed class SimConfig
    {
        public float NGlobalMax, GlobalReplenishPerTick, MinEnergyForPersistence;

        public float EthreshBase, GlobalScarcityK, EntropyPenalty, DecayLoss;

        public float PropagateFrac, MinBudgetToPropagate, ActivationCost;

        public float EntropyGainPerUse, EntropyDiffuseRate, EntropyDecay;

        public float NlocalMax, VacuumEventProbability, VacuumEventEntropy;

        public float ExpansionRate, MatterAheadThreshold;
    }
}
