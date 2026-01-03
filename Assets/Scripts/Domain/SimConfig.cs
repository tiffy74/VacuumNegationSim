using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Domain
{
    /// <summary>
    /// Simulation Configuration - Immutable Parameter Container
    /// 
    /// Encapsulates all simulation parameters in a single, immutable configuration object.
    /// Acts as a data transfer object (DTO) with no behavior beyond data storage.
    /// 
    /// Design Philosophy:
    /// - Immutable by Convention: No setters, initialized once
    /// - Single Source of Truth: All tunable parameters in one place
    /// - No Unity Dependencies: Can be serialized/tested independently
    /// - Organized by Category: Grouped into logical sections
    /// 
    /// Lifetime: Created once at simulation start from inspector parameters,
    ///           referenced (but not modified) throughout simulation.
    /// 
    /// Usage Pattern:
    ///   var config = new SimConfig();
    ///   config.NGlobalMax = 5e7f;  // Set all parameters
    ///   var ctx = new SimContext(config, initialEnergy, scale);
    ///   // config is now "frozen" - never modified again
    /// </summary>
    public sealed class SimConfig
    {
        // ============================================================================
        // CATEGORY: GLOBAL BUDGET
        // ============================================================================

        /// <summary>
        /// Maximum capacity of global energy pool.
        /// Physical Analogy: Total vacuum energy density of universe.
        /// Units: Arbitrary energy units (same as Nlocal[])
        /// Typical Value: 5e7
        /// </summary>
        public float NGlobalMax;

        /// <summary>
        /// Energy added to global pool per tick.
        /// Physical Analogy: Cosmological constant / dark energy influx.
        /// Units: Energy per tick
        /// Typical Value: 200
        /// </summary>
        public float GlobalReplenishPerTick;

        /// <summary>
        /// Minimum energy for a cell to contribute to persistence configurations.
        /// Used by CountPersistenceConfigurations() to determine viable states.
        /// Units: Energy
        /// Typical Value: 5
        /// </summary>
        public float MinEnergyForPersistence;

        // ============================================================================
        // CATEGORY: VIABILITY / THRESHOLD
        // ============================================================================

        /// <summary>
        /// Base energy threshold for structure persistence.
        /// Adjusted by global scarcity to create adaptive difficulty.
        /// Physical Analogy: Activation energy barrier.
        /// Units: Energy
        /// Typical Value: 0.18
        /// </summary>
        public float EthreshBase;

        /// <summary>
        /// How strongly global energy scarcity raises persistence threshold.
        /// Formula: EthreshEff = EthreshBase * (1 + GlobalScarcityK * scarcity)
        /// Range: [0, 1] where 0 = no effect, 1 = doubles threshold when depleted
        /// Typical Value: 0.3
        /// </summary>
        public float GlobalScarcityK;

        /// <summary>
        /// Entropy penalty factor (legacy parameter - currently unused).
        /// Originally intended to penalize high entropy in viability calculation.
        /// Kept for backward compatibility.
        /// Typical Value: 0.02
        /// </summary>
        public float EntropyPenalty;

        /// <summary>
        /// Baseline energy decay per tick (dissipation/thermalization).
        /// Applied uniformly to all cells after viability computation.
        /// Physical Analogy: Energy dissipation / heat loss.
        /// Units: Energy per tick
        /// Typical Value: 0.003
        /// </summary>
        public float DecayLoss;

        // ============================================================================
        // CATEGORY: PROPAGATION
        // ============================================================================

        /// <summary>
        /// Fraction of cell's energy sent to neighbors per tick.
        /// Formula: available = Nlocal[i] * PropagateFrac
        /// Range: [0, 1] where 0 = no propagation, 1 = all energy sent
        /// Typical Value: 0.25 (25% per tick)
        /// </summary>
        public float PropagateFrac;

        /// <summary>
        /// Minimum energy required for a cell to propagate to neighbors.
        /// Prevents tiny amounts of energy from causing unnecessary computation.
        /// Units: Energy
        /// Typical Value: 0.1
        /// </summary>
        public float MinBudgetToPropagate;

        /// <summary>
        /// Energy drawn from global pool when a cell activates.
        /// Represents "cost" of creating new active structure.
        /// Physical Analogy: Nucleation barrier in phase transitions.
        /// Units: Energy
        /// Typical Value: 0.25
        /// </summary>
        public float ActivationCost;

        // ============================================================================
        // CATEGORY: ENTROPY DYNAMICS
        // ============================================================================

        /// <summary>
        /// Entropy gain per unit of active energy use.
        /// Applied when cell has positive incoming flow.
        /// Physical Analogy: Entropy production in irreversible processes.
        /// Units: Entropy per energy
        /// Typical Value: 0.2
        /// </summary>
        public float EntropyGainPerUse;

        /// <summary>
        /// Entropy diffusion rate (discrete Laplacian coefficient).
        /// Formula: S_new = S + EntropyDiffuseRate * ∇²S
        /// Physical Analogy: Thermal diffusivity.
        /// Range: [0, 0.25] for numerical stability
        /// Typical Value: 0.2
        /// </summary>
        public float EntropyDiffuseRate;

        /// <summary>
        /// Entropy decay per tick (entropy loss/ordering).
        /// Applied after diffusion to prevent unbounded growth.
        /// Units: Entropy per tick
        /// Typical Value: 0.02
        /// </summary>
        public float EntropyDecay;

        /// <summary>
        /// Entropy gain from energy gradients with neighbors.
        /// Higher gradients = more spatial organization = higher entropy.
        /// Units: Entropy per energy gradient
        /// Typical Value: 0.02
        /// </summary>
        public float EntropyGainFromGradient;

        /// <summary>
        /// Entropy gain for cells adjacent to black holes.
        /// Represents increased disorder near singularities.
        /// Units: Entropy per tick (when near BH)
        /// Typical Value: 0.05
        /// </summary>
        public float EntropyGainNearBH;

        /// <summary>
        /// Amplitude of entropy boost to viability (A parameter in exponential).
        /// Formula: boost = 1 + A * (1 - exp(-K * entropy))
        /// Range: [0, 1] where 0 = no boost, 1 = double viability at high entropy
        /// Typical Value: 0.5
        /// </summary>
        public float EntropyViabilityGainA;

        /// <summary>
        /// Rate constant for entropy boost to viability (K parameter in exponential).
        /// Controls how quickly entropy saturates the boost.
        /// Units: 1/entropy
        /// Typical Value: 1.0
        /// </summary>
        public float EntropyViabilityGainK;

        // ============================================================================
        // CATEGORY: LOCAL LIMITS
        // ============================================================================

        /// <summary>
        /// Maximum energy per cell (prevents unbounded accumulation).
        /// Energy inflow clamped to this value.
        /// Units: Energy
        /// Typical Value: 5e4
        /// </summary>
        public float NlocalMax;

        /// <summary>
        /// Probability of random vacuum event per cell per tick.
        /// Vacuum events cause stochastic energy collapse.
        /// Physical Analogy: Quantum vacuum fluctuations.
        /// Range: [0, 1] where 0.0002 = 0.02% chance per cell per tick
        /// Typical Value: 0.0002
        /// </summary>
        public float VacuumEventProbability;

        /// <summary>
        /// Entropy spike applied during vacuum event.
        /// Represents sudden increase in disorder from collapse.
        /// Units: Entropy
        /// Typical Value: 0.5
        /// </summary>
        public float VacuumEventEntropy;

        /// <summary>
        /// Spatial expansion rate per tick (currently unused).
        /// Reserved for future cosmic expansion simulation.
        /// Physical Analogy: Hubble parameter H(t).
        /// Units: 1/tick
        /// Typical Value: 1.0 (no expansion)
        /// </summary>
        public float ExpansionRate;

        /// <summary>
        /// Matter-ahead threshold (legacy parameter - currently unused).
        /// Originally for predictive energy propagation.
        /// Kept for backward compatibility.
        /// </summary>
        public float MatterAheadThreshold;

        // ============================================================================
        // CATEGORY: BLACK HOLES
        // ============================================================================

        /// <summary>
        /// Charge threshold for black hole formation at boundary voids.
        /// Energy must accumulate to this level before collapse occurs.
        /// Physical Analogy: Critical mass for gravitational collapse.
        /// Units: Energy
        /// Default/Typical: 0.5
        /// </summary>
        public float BlackHoleFormThreshold = 0.5f;

        /// <summary>
        /// Fraction of neighbor energy drained per tick by black holes.
        /// Currently disabled (set to 0) - kept for future Hawking radiation.
        /// Range: [0, 1] where 0 = no drain, 1 = instant absorption
        /// Default: 0.1 (10% per tick)
        /// Current: 0.0 (disabled)
        /// </summary>
        public float BlackHoleDrainFrac = 0.1f;

        /// <summary>
        /// Fraction of drained energy radiated back out (Hawking-like radiation).
        /// Currently disabled (set to 0).
        /// Physical Analogy: Hawking radiation (inversely proportional to mass).
        /// Range: [0, 1] where 0 = all absorbed, 1 = all radiated
        /// Default/Current: 0.0 (disabled)
        /// </summary>
        public float BlackHoleRecoilFrac = 0f;

        /// <summary>
        /// Radius for black hole gravitational potential field (Manhattan distance).
        /// Used by ComputePotential() to spread BH influence.
        /// Units: Grid cells
        /// Default: 10
        /// </summary>
        public int BlackHolePotentialRadius = 10;

        /// <summary>
        /// Global multiplier for black hole potential strength.
        /// Scales the influence of BH potential field.
        /// Units: Dimensionless multiplier
        /// Default: 1.0
        /// </summary>
        public float BlackHolePotentialScale = 1f;

        /// <summary>
        /// How strongly Pass1 energy flow biases toward high-potential neighbors.
        /// Higher values = stronger attraction toward black holes.
        /// Units: Dimensionless weight factor
        /// Default: 3.0
        /// </summary>
        public float BlackHoleFlowBias = 3f;

        /// <summary>
        /// Viability boost for cells near black holes (currently unused).
        /// Could represent increased persistence due to stable geometry.
        /// Units: Effective energy addition for viability calc
        /// Default: 0.5
        /// </summary>
        public float BlackHoleViabilityBoost = 0.5f;

        // ============================================================================
        // CATEGORY: FIELD PROPAGATION
        // ============================================================================

        /// <summary>
        /// Energy cost for a cell to expand configuration space to a neighbor.
        /// Deducted from source cell when field propagates.
        /// Physical Analogy: Energy cost to "activate" latent geometry.
        /// Units: Energy
        /// Default: 0.2
        /// </summary>
        public float FieldAdvanceCost = 0.2f;

        /// <summary>
        /// Minimum energy in source cell required to expand field.
        /// Prevents depleted cells from attempting expansion.
        /// Units: Energy
        /// Default: 1.0
        /// </summary>
        public float FieldAdvanceMinSource = 1.0f;

        /// <summary>
        /// Probability of field expansion per eligible boundary cell per tick.
        /// Stochastic to prevent deterministic wave patterns.
        /// Range: [0, 1] where 0 = no expansion, 1 = always expands
        /// Default: 0.3 (30% chance)
        /// </summary>
        public float FieldAdvanceChance = 0.3f;

        /// <summary>
        /// Require source cell to be viable (V > 0) for field expansion.
        /// If true, only healthy structures can grow configuration space.
        /// Default: false (allow any active cell to expand)
        /// </summary>
        public bool FieldAdvanceRequiresViability = false;

        /// <summary>
        /// Seed minimal energy when field expands to new location.
        /// Prevents field-energy desynchronization.
        /// Default: false (no seeding)
        /// </summary>
        public bool FieldAdvanceSeedsEnergy = false;

        /// <summary>
        /// Amount of energy seeded when field expands (if FieldAdvanceSeedsEnergy = true).
        /// Units: Energy
        /// Default: 0.01
        /// </summary>
        public float FieldSeedEnergy = 0.01f;

        // ============================================================================
        // CATEGORY: VISUALIZATION
        // ============================================================================

        /// <summary>
        /// Color for void regions (no configuration space present).
        /// Default: Very dark purple/black (0.05, 0.05, 0.08)
        /// </summary>
        public Color VoidColor = new Color(0.05f, 0.05f, 0.08f, 1f);

        /// <summary>
        /// Color for active field cells (legacy - superseded by NullspaceColor).
        /// Kept for backward compatibility.
        /// Default: Dim purple (0.15, 0, 0.25)
        /// </summary>
        public Color FieldColor = new Color(0.15f, 0.0f, 0.25f, 1f);

        /// <summary>
        /// Color for inactive field cells (configuration space present but no energy).
        /// Default: Dim purple (0.15, 0, 0.25)
        /// </summary>
        public Color NullspaceColor = new Color(0.15f, 0.0f, 0.25f, 1f);

        /// <summary>
        /// Enable entropy-based color tinting in Viability render mode.
        /// When true, high entropy adds blue tint to viability gradient.
        /// Default: false (pure viability colors)
        /// </summary>
        public bool ShowEntropyTint = false;

        /// <summary>
        /// Multiplier for viability display (legacy - unused due to normalization).
        /// Originally scaled viability to visible range.
        /// Kept for backward compatibility.
        /// Default: 40.0 (based on typical Vmax ~0.02-0.04)
        /// </summary>
        public float ViabilityColorScale = 40f;
    }
}
