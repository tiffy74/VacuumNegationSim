using System;

namespace Viable.Engine.Execution
{
    /// <summary>
    /// Runtime execution context for simulation stepping.
    /// Contains configuration, current tick, global state, and deterministic RNG.
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

        public StepContext(Configuration.SimulationConfiguration config, float initialGlobal, float initialScale, int? seed = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            ResourceGlobal = initialGlobal;
            ScaleFactor = initialScale;
            Tick = 0;
            Rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        /// <summary>
        /// Creates a new context from scenario definition.
        /// </summary>
        public static StepContext FromScenario(Contracts.ScenarioDefinition scenario, Configuration.SimulationConfiguration config)
        {
            float initialGlobal = config.ResourceGlobalMax * 0.5f; // Default: half capacity
            float initialScale = 1.0f;
            
            // Override from scenario parameters if provided
            if (scenario.Parameters.TryGetValue("resourceGlobal", out double rg))
                initialGlobal = (float)rg;
            
            return new StepContext(config, initialGlobal, initialScale, scenario.Seed);
        }
    }
}
