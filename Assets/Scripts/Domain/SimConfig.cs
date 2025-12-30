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

        // Black holes
        public float BlackHoleFormThreshold = 0.5f;
        public float BlackHoleDrainFrac = 0.1f;
        public float BlackHoleRecoilFrac = 0f;

        // Field wave coupling
        public float FieldAdvanceCost = 0.2f;
        public float FieldAdvanceMinSource = 1.0f;
        public float FieldAdvanceChance = 0.3f;
        public bool FieldAdvanceRequiresViability = false;

        // Optional energy seeding when field advances
        public bool FieldAdvanceSeedsEnergy = false;
        public float FieldSeedEnergy = 0.01f;

        // Entropy coupling
        public float EntropyGainFromGradient = 0.02f;
        public float EntropyGainNearBH = 0.05f;
        public float EntropyViabilityGainA = 0.5f;
        public float EntropyViabilityGainK = 1.0f;

        // Rendering
        public Color VoidColor = new Color(0.05f, 0.05f, 0.08f, 1f);
        public Color FieldColor = new Color(0.15f, 0.0f, 0.25f, 1f); // dim purple
        public Color NullspaceColor = new Color(0.15f, 0.0f, 0.25f, 1f);
        public bool ShowEntropyTint = false;
        public float ViabilityColorScale = 40f; // because your Vmax is ~0.02-0.04

        // --- BH influence field + transport ---
        public int BlackHolePotentialRadius = 10;      // cells (Manhattan radius)
        public float BlackHolePotentialScale = 1f;     // global multiplier for potential strength
        public float BlackHoleFlowBias = 3f;           // how strongly Pass1 prefers high-potential neighbors
        public float BlackHoleViabilityBoost = 0.5f;   // adds to effective inflow used for viability calc

    }
}
