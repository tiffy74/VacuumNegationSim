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
        public float ResourceGlobalMax, GlobalReplenishPerTick, MinResourceForPersistence;

        public float EthreshBase, GlobalScarcityK, ComplexityPenalty, DecayLoss;

        public float PropagateFrac, MinBudgetToPropagate, ActivationCost;

        public float ComplexityGainPerUse, ComplexityDiffusionRate, ComplexityDecay;

        public float ResourceLocalMax, PerturbationProbability, PerturbationComplexity;

        public float ExpansionRate, MatterAheadThreshold;

        // Sink regions
        public float SinkFormationThreshold = 0.5f;
        public float SinkDrainFraction = 0.1f;
        public float SinkRecoilFraction = 0f;

        // Region expansion coupling
        public float RegionExpansionCost = 0.2f;
        public float RegionExpansionMinSource = 1.0f;
        public float RegionExpansionChance = 0.3f;
        public bool RegionExpansionRequiresViability = false;

        // Optional resource seeding when region expands
        public bool RegionExpansionSeedsResource = false;
        public float RegionSeedResource = 0.01f;

        // Complexity coupling
        public float ComplexityGainFromGradient = 0.02f;
        public float ComplexityGainNearSink = 0.05f;
        public float ComplexityViabilityGainA = 0.5f;
        public float ComplexityViabilityGainK = 1.0f;

        // Rendering
        public Color InactiveColor = new Color(0.05f, 0.05f, 0.08f, 1f);
        public Color RegionColor = new Color(0.15f, 0.0f, 0.25f, 1f); // dim purple
        public Color DormantRegionColor = new Color(0.15f, 0.0f, 0.25f, 1f);
        public bool ShowComplexityTint = false;
        public float ViabilityColorScale = 40f;

        // --- Sink influence field + transport ---
        public int SinkPotentialRadius = 10;           // cells (Manhattan radius)
        public float SinkPotentialScale = 1f;          // global multiplier for potential strength
        public float SinkFlowBias = 3f;                // how strongly Pass1 prefers high-potential neighbors
        public float SinkViabilityBoost = 0.5f;        // adds to effective inflow used for viability calc
    }
}
