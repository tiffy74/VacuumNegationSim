using System;

namespace Viable.Engine.Configuration
{
    /// <summary>
    /// Simulation configuration parameters (Unity-free).
    /// Color values moved to separate ColorConfiguration for Unity layer.
    /// Stage 13: Added mechanism selectors with safe defaults.
    /// Stage 13.9: Added adjacency mode for neighbor connectivity.
    /// </summary>
    public sealed class SimulationConfiguration
    {
        // ===== Stage 13: Mechanism Selectors (Default = Current Behavior) =====
        
        /// <summary>
        /// Inflow mechanism. Default: Uniform (current behavior).
        /// Stage 13: No behavior change yet, stored for future PhaseFactory use.
        /// </summary>
        public Contracts.InflowMode InflowMode = Contracts.InflowMode.Uniform;

        /// <summary>
        /// Outflow mechanism. Default: Uniform (current behavior).
        /// </summary>
        public Contracts.OutflowMode OutflowMode = Contracts.OutflowMode.Uniform;

        /// <summary>
        /// Diffusion neighborhood. Default: VonNeumann4 (current behavior).
        /// </summary>
        public Contracts.DiffusionMode DiffusionMode = Contracts.DiffusionMode.VonNeumann4;

        /// <summary>
        /// Boundary behavior. Default: Absorbing (current behavior).
        /// </summary>
        public Contracts.BoundaryMode BoundaryMode = Contracts.BoundaryMode.Absorbing;

        /// <summary>
        /// Viability rule. Default: HardThreshold (current behavior).
        /// Stage 13.7: Supports hysteresis (separate on/off thresholds).
        /// </summary>
        public Contracts.ViabilityRule ViabilityRule = Contracts.ViabilityRule.HardThreshold;

        /// <summary>
        /// Hysteresis ON threshold (activate when viability >= this value).
        /// Stage 13.7: Only used when ViabilityRule == Hysteresis.
        /// Default: 0.0 (no hysteresis effect with HardThreshold rule).
        /// </summary>
        public double HysteresisOnThreshold = 0.0;

        /// <summary>
        /// Hysteresis OFF threshold (deactivate when viability <= this value).
        /// Stage 13.7: Only used when ViabilityRule == Hysteresis.
        /// Must be < HysteresisOnThreshold for proper hysteresis behavior.
        /// Default: 0.0 (no hysteresis effect with HardThreshold rule).
        /// </summary>
        public double HysteresisOffThreshold = 0.0;

        /// <summary>
        /// Region mode. Default: CurrentDefault.
        /// </summary>
        public Contracts.RegionMode RegionMode = Contracts.RegionMode.CurrentDefault;

        /// <summary>
        /// Topology mode. Default: RectGrid (current behavior).
        /// </summary>
        public Contracts.TopologyMode TopologyMode = Contracts.TopologyMode.RectGrid;

        /// <summary>
        /// Adjacency mode for neighbor connectivity.
        /// Stage 13.9: Choose between edge-only or edge+vertex neighbors.
        /// Default: EdgeOnly (true topology connectivity: Triangle=3, Rect=4, Hex=6)
        /// EdgeAndVertex: Extended connectivity (Triangle=6, Rect=8, Hex=6)
        /// </summary>
        public Contracts.AdjacencyMode AdjacencyMode = Contracts.AdjacencyMode.EdgeOnly;

        /// <summary>
        /// Mask shape. Default: Rectangle (no mask, current behavior).
        /// </summary>
        public Contracts.MaskShape MaskShape = Contracts.MaskShape.Rectangle;

        /// <summary>
        /// Mask radius for Circle/Ring shapes.
        /// Stage 13.8: Default 0.0 = no mask effect.
        /// </summary>
        public double MaskRadius = 0.0;

        /// <summary>
        /// Inner radius for Ring shape.
        /// Stage 13.8: Default 0.0 = no inner hole.
        /// </summary>
        public double MaskInnerRadius = 0.0;

        /// <summary>
        /// Corridor width for Corridor shape.
        /// Stage 13.8: Default 0.0 = no corridor.
        /// </summary>
        public double CorridorWidth = 0.0;

        /// <summary>
        /// Hole probability for PercolationHoles shape.
        /// Stage 13.8: Default 0.0 = no holes.
        /// </summary>
        public double HoleProbability = 0.0;

        /// <summary>
        /// Refinement mode. Default: None (current behavior, no refinement).
        /// </summary>
        public Contracts.RefinementMode RefinementMode = Contracts.RefinementMode.None;

        // ===== Existing Numeric Parameters =====
        
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
