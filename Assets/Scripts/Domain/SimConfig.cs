using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        // Field wave coupling
        public float FieldAdvanceCost = 0.2f;
        public float FieldAdvanceMinSource = 1.0f;
        public float FieldAdvanceChance = 0.3f;
        public bool FieldAdvanceRequiresViability = false;

        // Optional energy seeding when field advances
        public bool FieldAdvanceSeedsEnergy = false;
        public float FieldSeedEnergy = 0.01f;

        // Rendering
        public Color VoidColor = new Color(0.05f, 0.05f, 0.08f, 1f);
        public Color FieldColor = new Color(0.15f, 0.0f, 0.25f, 1f); // dim purple
        public Color NullspaceColor = new Color(0.15f, 0.0f, 0.25f, 1f);
        public bool ShowEntropyTint = false;
        public float ViabilityColorScale = 40f; // because your Vmax is ~0.02-0.04
    }
}
