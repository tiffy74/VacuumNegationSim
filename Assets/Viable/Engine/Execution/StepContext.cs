using System;
using Viable.Contracts; // ADDED: For TopologyMode and AdjacencyMode

namespace Viable.Engine.Execution
{
    /// <summary>
    /// Runtime execution context for simulation stepping.
    /// Contains configuration, current tick, global state, and deterministic RNG.
    /// Stage 13.7: Added topology support for neighbor connectivity.
    /// Stage 13.9: Added adjacency mode support.
    /// </summary>
    public sealed class StepContext
    {
        /// <summary>
        /// Simulation configuration (immutable during run).
        /// </summary>
        public readonly Configuration.SimulationConfiguration Config;

        /// <summary>
        /// Current simulation step/tick.
        /// </summary>
        public int Tick;

        /// <summary>
        /// Global resource pool.
        /// </summary>
        public float ResourceGlobal;

        /// <summary>
        /// Scale factor for expansion dynamics.
        /// </summary>
        public float ScaleFactor;

        /// <summary>
        /// Deterministic random number generator (seeded for reproducibility).
        /// </summary>
        public Random Rng;

        /// <summary>
        /// Time delta per step (for time-based calculations).
        /// Default: 1.0 (one unit per step).
        /// </summary>
        public float DeltaTime = 1.0f;

        /// <summary>
        /// Grid topology mode (Rectangular, Triangular, Hexagonal).
        /// Stage 13.7: Determines neighbor connectivity (4 vs 6 neighbors).
        /// </summary>
        public TopologyMode Topology { get; }

        /// <summary>
        /// Adjacency mode (Edge-only or Edge+Vertex).
        /// Stage 13.9: Determines extended neighbor connectivity.
        /// EdgeOnly: Triangle=3, Rect=4, Hex=6
        /// EdgeAndVertex: Triangle=6, Rect=8, Hex=6
        /// </summary>
        public AdjacencyMode Adjacency { get; }

        public StepContext(
            Configuration.SimulationConfiguration config,
            float initialGlobal,
            float initialScale,
            int? seed = null,
            TopologyMode topology = TopologyMode.RectGrid,
            AdjacencyMode adjacency = AdjacencyMode.EdgeOnly) // ADDED: adjacency parameter
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            ResourceGlobal = initialGlobal;
            ScaleFactor = initialScale;
            Tick = 0;
            Topology = topology;
            Adjacency = adjacency; // ADDED: Store adjacency
            Rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        /// <summary>
        /// Creates a new context from scenario definition.
        /// Stage 13.7: Extracts topology from scenario EngineConfig.
        /// Stage 13.9: Extracts adjacency mode from config.
        /// </summary>
        public static StepContext FromScenario(Contracts.ScenarioDefinition scenario, Configuration.SimulationConfiguration config)
        {
            float initialGlobal = config.ResourceGlobalMax * 0.5f; // Default: half capacity
            float initialScale = 1.0f;
            
            // Override from scenario parameters if provided
            if (scenario.Parameters.TryGetValue("resourceGlobal", out double rg))
                initialGlobal = (float)rg;
            
            // Extract topology and adjacency from scenario/config
            TopologyMode topology = scenario.EngineConfig?.TopologyMode ?? TopologyMode.RectGrid;
            AdjacencyMode adjacency = config.AdjacencyMode;
            
            return new StepContext(config, initialGlobal, initialScale, scenario.Seed, topology, adjacency);
        }
    }
}
