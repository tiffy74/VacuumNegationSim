using System;

namespace Viable.Engine.Configuration
{
    /// <summary>
    /// Simulation configuration parameters (Unity-free).
    /// Color values moved to separate ColorConfiguration for Unity layer.
    /// </summary>
    public sealed class SimulationConfiguration
    {
        // Global resource pool
        public float ResourceGlobalMax;
        public float GlobalReplenishPerTick;
        public float MinResourceForPersistence;

        // Viability thresholds
        public float EthreshBase;
        public float GlobalScarcityK;
        public float ComplexityPenalty;
        public float DecayLoss;

        // Propagation
        public float PropagateFrac;
        public float MinBudgetToPropagate;
        public float ActivationCost;

        // Complexity dynamics
        public float ComplexityGainPerUse;
        public float ComplexityDiffusionRate;
        public float ComplexityDecay;

        // Local limits
        public float ResourceLocalMax;
        public float PerturbationProbability;
        public float PerturbationComplexity;

        // Expansion
        public float ExpansionRate;
        public float MatterAheadThreshold;

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

        // Sink influence field + transport
        public int SinkPotentialRadius = 10;           // cells (Manhattan radius)
        public float SinkPotentialScale = 1f;          // global multiplier for potential strength
        public float SinkFlowBias = 3f;                // how strongly Pass1 prefers high-potential neighbors
        public float SinkViabilityBoost = 0.5f;        // adds to effective inflow used for viability calc

        // Rendering configuration (non-color parameters)
        public bool ShowComplexityTint = false;
        public float ViabilityColorScale = 40f;
    }
}
