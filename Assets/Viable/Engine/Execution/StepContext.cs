using System;
using Viable.Contracts; // ADDED: For TopologyMode and AdjacencyMode
using Viable.Engine.ExpansionModels; // Stage 14: For IExpansionModel

namespace Viable.Engine.Execution
{
    /// <summary>
    /// Runtime execution context for simulation stepping.
    /// Contains configuration, current tick, global state, and deterministic RNG.
    /// Stage 13.7: Added topology support for neighbor connectivity.
    /// Stage 13.9: Added adjacency mode support.
    /// Stage 14: Added expansion model plugin support.
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

        /// <summary>
        /// Expansion model plugin for sink/region formation dynamics.
        /// Stage 14: Scientists can swap different expansion models (Viability, Cosmological, Biological, etc.)
        /// </summary>
        public IExpansionModel ExpansionModel { get; private set; }

        public StepContext(
            Configuration.SimulationConfiguration config,
            float initialGlobal,
            float initialScale,
            int? seed = null,
            TopologyMode topology = TopologyMode.RectGrid,
            AdjacencyMode adjacency = AdjacencyMode.EdgeOnly,
            IExpansionModel expansionModel = null) // Stage 14: Add expansion model parameter
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            ResourceGlobal = initialGlobal;
            ScaleFactor = initialScale;
            Tick = 0;
            Topology = topology;
            Adjacency = adjacency;
            Rng = seed.HasValue ? new Random(seed.Value) : new Random();
            
            // Stage 14: Use provided expansion model or create default from factory
            ExpansionModel = expansionModel ?? CreateDefaultExpansionModel();
        }

        /// <summary>
        /// Set the expansion model at runtime.
        /// Stage 14: Allows changing expansion model mid-simulation (for research).
        /// </summary>
        public void SetExpansionModel(IExpansionModel model)
        {
            ExpansionModel = model ?? CreateDefaultExpansionModel();
        }

        /// <summary>
        /// Create the default expansion model (Viability Boundary Pressure).
        /// </summary>
        private IExpansionModel CreateDefaultExpansionModel()
        {
            // Use factory to create default model with default config
            return ExpansionModelFactory.Create(Contracts.ExpansionModel.DefaultViabilityBoundaryPressure);
        }

        /// <summary>
        /// Creates a new context from scenario definition.
        /// Stage 13.7: Extracts topology from scenario EngineConfig.
        /// Stage 13.9: Extracts adjacency mode from config.
        /// Stage 14: Creates expansion model from config or parameters.
        /// </summary>
        public static StepContext FromScenario(Contracts.ScenarioDefinition scenario, Configuration.SimulationConfiguration config)
        {
            // Default: use config's max * fraction
            float initialGlobal = config.ResourceGlobalMax * 0.2f;
            float initialScale = 1.0f;

            // Override from scenario parameters if provided
            // Check for explicit "resourceGlobal" first (exact initial value)
            if (scenario.Parameters.TryGetValue("resourceGlobal", out double rg))
            {
                initialGlobal = (float)rg;
            }
            // Otherwise derive from resourceGlobalMax (same as InitializeSimulation behavior)
            else if (scenario.Parameters.TryGetValue("resourceGlobalMax", out double rgMax))
            {
                initialGlobal = (float)rgMax * 0.2f; // Match original 20% startup
            }

            // Check for scaleFactor parameter
            if (scenario.Parameters.TryGetValue("scaleFactor", out double sf))
            {
                initialScale = (float)sf;
            }

            // Extract topology and adjacency from scenario/config
            TopologyMode topology = scenario.EngineConfig?.TopologyMode ?? TopologyMode.RectGrid;
            AdjacencyMode adjacency = config.AdjacencyMode;
            
            // Stage 14: Create expansion model
            IExpansionModel expansionModel = null;
            
            // First try ExpansionConfig if available
            if (scenario.EngineConfig?.ExpansionConfig != null)
            {
                expansionModel = ExpansionModelFactory.CreateFromConfig(scenario.EngineConfig.ExpansionConfig);
            }
            // Fallback: check parameters for expansion model type (passed as int from Unity)
            else if (scenario.Parameters.TryGetValue("expansionModelType", out double modelTypeDouble))
            {
                var modelType = (ExpansionModel)(int)modelTypeDouble;
                var expansionConfig = new ExpansionConfig { Model = modelType };
                expansionModel = ExpansionModelFactory.CreateFromConfig(expansionConfig);
            }
            
            return new StepContext(config, initialGlobal, initialScale, scenario.Seed, topology, adjacency, expansionModel);
        }
    }
}
