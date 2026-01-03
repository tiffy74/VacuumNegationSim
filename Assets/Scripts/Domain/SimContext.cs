using System;

namespace Assets.Scripts.Domain
{
    /// <summary>
    /// Simulation Context - Global Simulation State
    /// 
    /// Holds mutable global state that changes each tick, distinct from per-cell
    /// state (GridState) and immutable configuration (SimConfig).
    /// 
    /// Lifetime: Created once at simulation start, passed to all steps each tick.
    /// 
    /// Design Philosophy:
    /// - Mutable: Values change during simulation (unlike SimConfig)
    /// - Global: Shared across entire grid (unlike per-cell arrays in GridState)
    /// - Minimal: Only essential global counters and pools
    /// - Non-Serialized: Transient state, reset on restart
    /// 
    /// Separation of Concerns:
    /// - GridState: Per-cell arrays (energy, entropy, viability, etc.)
    /// - SimContext: Global scalars (tick counter, energy pool, scale factor)
    /// - SimConfig: Immutable parameters (thresholds, rates, colors)
    /// </summary>
    public sealed class SimContext
    {
        // ============================================================================
        // PUBLIC FIELDS: CONFIGURATION REFERENCE
        // ============================================================================

        /// <summary>
        /// Reference to immutable simulation configuration.
        /// Provides access to all simulation parameters (thresholds, rates, etc.)
        /// without duplicating data in context.
        /// 
        /// Marked readonly to prevent reassignment (config itself is immutable).
        /// </summary>
        public readonly SimConfig Config;

        // ============================================================================
        // PUBLIC FIELDS: GLOBAL ENERGY POOL
        // ============================================================================

        /// <summary>
        /// Global Energy Pool
        /// 
        /// Represents the available energy reservoir for cell activation costs.
        /// Acts as a "cosmological constant" or vacuum energy source.
        /// 
        /// Dynamics:
        /// - Decreased: When cells activate (Pass2 draws from pool)
        /// - Increased: By Pass3 replenishment (constant rate per tick)
        /// - Capped: Cannot exceed Config.NGlobalMax
        /// 
        /// Physical Analogy: Dark energy density / vacuum energy
        /// 
        /// Range: [0, Config.NGlobalMax]
        /// Units: Arbitrary energy units (same as Nlocal[])
        /// </summary>
        public float NGlobal;

        // ============================================================================
        // PUBLIC FIELDS: SPATIAL SCALE FACTOR
        // ============================================================================

        /// <summary>
        /// Spatial Scale Factor
        /// 
        /// Represents the expansion/contraction of configuration space.
        /// Currently unused but reserved for future cosmic expansion simulation.
        /// 
        /// Potential Uses:
        /// - Friedmann equation integration (cosmological expansion)
        /// - Grid cell spacing adjustment
        /// - Distance-dependent interaction strengths
        /// 
        /// Physical Analogy: Scale factor a(t) in FLRW metric
        /// 
        /// Default: 1.0 (no expansion)
        /// Units: Dimensionless scale factor
        /// </summary>
        public float ScaleFactor;

        // ============================================================================
        // PUBLIC FIELDS: TICK COUNTER
        // ============================================================================

        /// <summary>
        /// Simulation Tick Counter
        /// 
        /// Monotonically increasing counter tracking simulation time.
        /// Incremented by SimulationEngine at start of each tick.
        /// 
        /// Uses:
        /// - Time-based logic (e.g., black hole formation after tick 10)
        /// - Arrival tracking (FieldFirstTick, EnergyFirstTick)
        /// - Diagnostic logging (log every N ticks)
        /// - Temporal analysis (track propagation speed)
        /// 
        /// Physical Analogy: Discrete time steps in numerical simulation
        /// 
        /// Initial Value: 0
        /// Increment: +1 per SimulationEngine.Tick()
        /// Range: [0, int.MaxValue] (effectively unbounded)
        /// </summary>
        public int Tick;

        // ============================================================================
        // CONSTRUCTOR
        // ============================================================================

        /// <summary>
        /// Initializes simulation context with configuration and initial global state.
        /// </summary>
        /// <param name="config">Immutable simulation configuration (must not be null)</param>
        /// <param name="initialNGlobal">Initial global energy pool value</param>
        /// <param name="initialScale">Initial spatial scale factor (default: 1.0)</param>
        /// <exception cref="ArgumentNullException">Thrown if config is null</exception>
        public SimContext(SimConfig config, float initialNGlobal, float initialScale)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            NGlobal = initialNGlobal;
            ScaleFactor = initialScale;
            Tick = 0; // Always start at tick 0
        }

        // ============================================================================
        // PUBLIC METHODS: STATE INSPECTION
        // ============================================================================

        /// <summary>
        /// Gets the global energy scarcity factor (0 = abundant, 1 = depleted).
        /// Useful for diagnostic purposes and adaptive thresholds.
        /// </summary>
        /// <returns>Scarcity factor in [0, 1]</returns>
        public float GetGlobalScarcity()
        {
            if (Config.NGlobalMax <= 0f)
                return 0f;

            return 1f - (NGlobal / Config.NGlobalMax);
        }

        /// <summary>
        /// Checks if global energy pool is effectively depleted (below epsilon).
        /// </summary>
        /// <param name="epsilon">Threshold for considering pool empty (default: 1e-6)</param>
        /// <returns>True if NGlobal is below epsilon</returns>
        public bool IsGlobalEnergyDepleted(float epsilon = 1e-6f)
        {
            return NGlobal < epsilon;
        }

        /// <summary>
        /// Gets a formatted summary string for debugging/logging.
        /// </summary>
        /// <returns>Human-readable context summary</returns>
        public override string ToString()
        {
            return $"SimContext[Tick={Tick}, NGlobal={NGlobal:F1}/{Config.NGlobalMax:F1}, Scale={ScaleFactor:F3}]";
        }
    }
}
