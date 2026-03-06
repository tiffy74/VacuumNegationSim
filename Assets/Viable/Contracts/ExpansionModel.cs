namespace Viable.Contracts
{
    /// <summary>
    /// Expansion model categories for sink/region formation dynamics.
    /// Each model represents a different physical/biological/mathematical paradigm
    /// for how spatial expansion occurs and what drives/constrains it.
    /// </summary>
    public enum ExpansionModel
    {
        // ===== Viability-Constraint Models (Original Contribution) =====
        // Expansion driven by LOCAL viability conditions, not external forces.
        // Unique paradigm: cells expand/contract based on resource viability.
        
        /// <summary>
        /// DEFAULT: Viability constraint with boundary pressure. Sinks form at boundaries
        /// when resource pressure exceeds threshold. No external driving force -
        /// expansion emerges from viability dynamics alone.
        /// This is the original Viable simulation model and the default behavior.
        /// </summary>
        DefaultViabilityBoundaryPressure = 0,

        /// <summary>
        /// Pure viability constraint: Expansion/contraction determined solely
        /// by local viability state. No threshold accumulation.
        /// </summary>
        ViabilityPure = 1,

        /// <summary>
        /// Viability with hysteresis: Different thresholds for expansion vs contraction.
        /// Models systems with memory/inertia in state transitions.
        /// </summary>
        ViabilityHysteresis = 2,

        // ===== Cosmological Models =====
        // Expansion driven by field dynamics, vacuum energy, or geometric effects.
        
        /// <summary>
        /// Inflationary expansion: Rapid early formation that decays exponentially.
        /// Models cosmic inflation ? matter-dominated transition.
        /// </summary>
        Inflationary = 10,

        /// <summary>
        /// Dark energy accelerating expansion: Slow start, accelerating over time.
        /// Models late-time cosmic acceleration (cosmological constant).
        /// </summary>
        DarkEnergyAccelerating = 11,

        /// <summary>
        /// Cyclic/bounce model: Alternating expansion and contraction phases.
        /// Models cyclic cosmology, ekpyrotic scenarios.
        /// </summary>
        Cyclic = 12,

        /// <summary>
        /// Steady-state expansion: Constant creation rate, no beginning/end.
        /// Models Hoyle-Bondi-Gold steady-state cosmology.
        /// </summary>
        SteadyState = 13,

        // ===== Biological Models =====
        // Expansion driven by growth, replication, and resource competition.
        
        /// <summary>
        /// Logistic growth: Expansion rate depends on density relative to carrying capacity.
        /// Models population dynamics, tumor growth.
        /// </summary>
        LogisticGrowth = 20,

        /// <summary>
        /// Turing pattern formation: Reaction-diffusion creates spots/stripes.
        /// Models morphogenesis, self-organizing spatial patterns.
        /// </summary>
        TuringPattern = 21,

        /// <summary>
        /// Angiogenesis/vascular growth: Branching network formation following gradients.
        /// Models blood vessel growth, root systems.
        /// </summary>
        Angiogenesis = 22,

        /// <summary>
        /// Eden model: Random surface growth, compact clusters.
        /// Models bacterial colony edges, crystal growth.
        /// </summary>
        EdenGrowth = 23,

        // ===== Fluid Dynamics Models =====
        // Expansion driven by pressure, viscosity, and interface dynamics.
        
        /// <summary>
        /// Viscous fingering: Saffman-Taylor instability creates dendritic patterns.
        /// Models fluid displacement in porous media.
        /// </summary>
        ViscousFingering = 30,

        /// <summary>
        /// Diffusion-limited aggregation: Random walkers stick on contact.
        /// Models electrodeposition, snowflakes, fractal growth.
        /// </summary>
        DLA = 31,

        /// <summary>
        /// Ballistic deposition: Particles fall and stick where they land.
        /// Models sedimentation, thin film growth.
        /// </summary>
        BallisticDeposition = 32,

        /// <summary>
        /// Convection cells: Self-organizing hexagonal/roll patterns.
        /// Models Rayleigh-Bénard convection.
        /// </summary>
        ConvectionCells = 33,

        // ===== Statistical Physics Models =====
        // Expansion governed by probabilistic rules and phase transitions.
        
        /// <summary>
        /// Percolation: Random occupation with critical threshold.
        /// Models connectivity, gelation, epidemic spread.
        /// </summary>
        Percolation = 40,

        /// <summary>
        /// Ising-like: Spin alignment with temperature and external field.
        /// Models phase transitions, domain formation.
        /// </summary>
        IsingModel = 41,

        /// <summary>
        /// Sandpile/SOC: Self-organized criticality with avalanches.
        /// Models earthquakes, neural cascades.
        /// </summary>
        SelfOrganizedCriticality = 42,

        // ===== Custom/Research =====
        
        /// <summary>
        /// Fully custom: All parameters exposed for novel research models.
        /// </summary>
        Custom = 99
    }

    /// <summary>
    /// Expansion phase within a model (for multi-phase models).
    /// </summary>
    public enum ExpansionPhase
    {
        Initialization = 0,
        EarlyExpansion = 1,
        Transition = 2,
        MainExpansion = 3,
        Deceleration = 4,
        Equilibrium = 5,
        Contraction = 6,
        Collapse = 7
    }
}
