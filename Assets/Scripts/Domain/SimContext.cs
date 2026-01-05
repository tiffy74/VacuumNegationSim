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
        // PUBLIC FIELDS: BLACK HOLE HALO DIAGNOSTICS
        // ============================================================================

        /// <summary>
        /// Blocked Energy Flux Into Black Holes (Diagnostic)
        /// 
        /// Total energy that attempted to propagate into black hole cells during
        /// Pass1.GatherOutflow but was blocked due to geometric constraints.
        /// 
        /// Purpose: Verify hypothesis that halos are caused by blocked propagation.
        /// 
        /// Interpretation:
        /// - Non-zero: Energy is attempting to flow into BHs but cannot
        /// - Zero: No blocked flux (halos must have different cause)
        /// 
        /// Reset: Cleared at start of each tick
        /// Updated: Accumulated during Pass1.GatherOutflow
        /// Logged: After Pass2 completes
        /// 
        /// Units: Energy units (same as Nlocal)
        /// Range: [0, +inf) per tick
        /// </summary>
        public float BlockedIntoBH;

        /// <summary>
        /// Average Energy Adjacent to Black Holes (Diagnostic)
        /// 
        /// Average Nlocal value in the 4-neighbor ring around all black hole cells,
        /// excluding black holes themselves and vacuum cells.
        /// 
        /// Purpose: Measure energy accumulation near black hole boundaries.
        /// 
        /// Interpretation:
        /// - Increases with lag after BlockedIntoBH: Confirms pile-up hypothesis
        /// - No correlation: Halos caused by other mechanisms
        /// 
        /// Computed: After Pass2.ApplyAndViability completes
        /// Logged: Together with BlockedIntoBH
        /// 
        /// Units: Energy units (average of Nlocal)
        /// Range: [0, NlocalMax]
        /// </summary>
        public float AdjEnergyNearBH;

        // ============================================================================
        // PUBLIC FIELDS: LEAK DIAGNOSTICS
        // ============================================================================

        /// <summary>
        /// Leak Attempts Count (Diagnostic)
        /// 
        /// Number of neighbor targets where energy attempted to propagate
        /// but FieldPresent=false (void/non-field neighbors).
        /// 
        /// Purpose: Verify if leak events are actually happening.
        /// 
        /// Interpretation:
        /// - Non-zero: Energy is attempting to leak into voids
        /// - Zero: No leak attempts (field boundaries may be inactive)
        /// 
        /// Reset: Cleared at start of each tick
        /// Updated: Incremented during Pass1.GatherOutflow
        /// Logged: After Pass2 completes
        /// 
        /// Units: Count of neighbor targets
        /// Range: [0, +inf) per tick
        /// </summary>
        public int LeakAttempts;

        /// <summary>
        /// Leak Energy Total (Diagnostic)
        /// 
        /// Total energy that attempted to propagate into void/non-field neighbors
        /// (FieldPresent=false) during Pass1.GatherOutflow.
        /// 
        /// Purpose: Measure magnitude of leak attempts.
        /// 
        /// Interpretation:
        /// - Non-zero: Substantial energy trying to leak
        /// - Zero: No energy leaking (boundaries inactive or no field edges)
        /// 
        /// Reset: Cleared at start of each tick
        /// Updated: Accumulated during Pass1.GatherOutflow
        /// Logged: After Pass2 completes
        /// 
        /// Units: Energy units (same as Nlocal)
        /// Range: [0, +inf) per tick
        /// </summary>
        public float LeakEnergy;

        // ============================================================================
        // PUBLIC FIELDS: BACKGROUND VS HALO ENERGY DIAGNOSTICS
        // ============================================================================

        /// <summary>
        /// Average Energy in Field Cells (Diagnostic)
        /// 
        /// Average Nlocal over all cells where FieldPresent=true AND IsBlackHole=false.
        /// Represents "background" energy level in active field.
        /// 
        /// Purpose: Baseline for comparing halo energy accumulation.
        /// 
        /// Computed: After Pass2.ApplyAndViability completes
        /// Logged: Together with AvgNRingBH
        /// 
        /// Units: Energy units (average of Nlocal)
        /// Range: [0, NlocalMax]
        /// </summary>
        public float AvgNField;

        /// <summary>
        /// Average Energy in Black Hole Adjacent Ring (Diagnostic)
        /// 
        /// Average Nlocal in cells adjacent to black holes (4-neighbor ring),
        /// excluding black holes themselves and vacuum cells.
        /// 
        /// Purpose: Measure "halo" energy level.
        /// 
        /// Computed: After Pass2.ApplyAndViability completes
        /// Logged: Together with AvgNField
        /// 
        /// Units: Energy units (average of Nlocal)
        /// Range: [0, NlocalMax]
        /// </summary>
        public float AvgNRingBH;

        /// <summary>
        /// Ring Energy Minus Field Energy (Diagnostic)
        /// 
        /// Difference: AvgNRingBH - AvgNField
        /// 
        /// Purpose: Quantify excess energy in halos vs background.
        /// 
        /// Interpretation:
        /// - Positive: Halos have higher energy than field background (confirms pile-up)
        /// - Zero: No energy difference
        /// - Negative: Halos depleted (unexpected)
        /// 
        /// Computed: After Pass2.ApplyAndViability completes
        /// Logged: After Pass2 completes
        /// 
        /// Units: Energy units (difference)
        /// Range: (-NlocalMax, +NlocalMax)
        /// </summary>
        public float RingMinusField;

        /// <summary>
        /// Ring Energy Ratio to Field Energy (Diagnostic)
        /// 
        /// Ratio: AvgNRingBH / (AvgNField + epsilon)
        /// 
        /// Purpose: Quantify halo energy as multiplier of background.
        /// 
        /// Interpretation:
        /// - > 1: Halos have more energy than background
        /// - = 1: No energy difference
        /// - < 1: Halos depleted
        /// 
        /// Computed: After Pass2.ApplyAndViability completes
        /// Logged: After Pass2 completes
        /// 
        /// Units: Dimensionless ratio
        /// Range: [0, +inf)
        /// </summary>
        public float RingRatio;

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
            
            // Initialize all diagnostic fields to zero
            BlockedIntoBH = 0f;
            AdjEnergyNearBH = 0f;
            LeakAttempts = 0;
            LeakEnergy = 0f;
            AvgNField = 0f;
            AvgNRingBH = 0f;
            RingMinusField = 0f;
            RingRatio = 0f;
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
