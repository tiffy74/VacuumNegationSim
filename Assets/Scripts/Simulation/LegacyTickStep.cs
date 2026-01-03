using System;
using Assets.Scripts.Domain;
using Assets.Scripts.Events;

namespace Assets.Scripts.Simulation
{
    /// <summary>
    /// Legacy 4-Pass Simulation Step
    /// 
    /// Concrete implementation of ISimStep that wraps the original multi-pass simulation logic.
    /// Executes energy propagation, viability computation, global replenishment, entropy diffusion,
    /// and post-processing (field wave, black hole growth) in a fixed sequence.
    /// 
    /// Design: Preserves original simulation behavior while fitting into new engine architecture.
    /// Named "Legacy" to indicate this is the historical implementation that will be refactored later.
    /// </summary>
    public sealed class LegacyTickStep : ISimStep
    {
        // ============================================================================
        // PRIVATE FIELDS: INJECTED DEPENDENCIES
        // ============================================================================
        
        /// <summary>
        /// Delegate for computing viability from energy flow and state.
        /// Signature: (incomingFlow, localEnergy, entropy) → viability
        /// Injected from SimulationController to maintain separation of concerns.
        /// </summary>
        private readonly Func<float, float, float, float> _viabilityFunc;

        /// <summary>
        /// Delegate for counting persistence configurations at a cell.
        /// Signature: (cellIndex) → configurationCount
        /// Used by Pass2 to compute configuration-based entropy.
        /// </summary>
        private readonly Func<int, int> _persistenceFunc;

        /// <summary>
        /// Delegate for post-processing operations (field wave, BH growth).
        /// Executed after Pass1-4 complete.
        /// Injected from SimulationController to allow custom post-processing logic.
        /// </summary>
        private readonly Action _postProcessing;

        // ============================================================================
        // CONSTRUCTOR
        // ============================================================================

        /// <summary>
        /// Initializes the legacy tick step with injected dependencies.
        /// </summary>
        /// <param name="viabilityFunc">Function to compute viability</param>
        /// <param name="persistenceFunc">Function to count persistence configurations</param>
        /// <param name="postProcessing">Callback for post-processing (field wave, BH growth)</param>
        public LegacyTickStep(
            Func<float, float, float, float> viabilityFunc,
            Func<int, int> persistenceFunc,
            Action postProcessing)
        {
            _viabilityFunc = viabilityFunc ?? throw new ArgumentNullException(nameof(viabilityFunc));
            _persistenceFunc = persistenceFunc ?? throw new ArgumentNullException(nameof(persistenceFunc));
            _postProcessing = postProcessing ?? throw new ArgumentNullException(nameof(postProcessing));
        }

        // ============================================================================
        // PUBLIC API: ISTEP IMPLEMENTATION
        // ============================================================================

        /// <summary>
        /// Executes one complete simulation tick using the legacy 4-pass algorithm.
        /// 
        /// Execution Sequence:
        /// 1. Pass 1a: Gather Outflow (energy propagation with BH attraction)
        /// 2. Pass 1b: Gather Inflow (seed region pulse)
        /// 3. Pass 2: Apply Energy & Compute Viability
        /// 4. Pass 3: Global Energy Replenishment
        /// 5. Pass 4: Entropy Diffusion
        /// 6. Post-Processing: Field Wave Propagation & Black Hole Growth
        /// 
        /// All passes modify state in-place for performance (zero allocation per tick).
        /// </summary>
        /// <param name="state">Grid state to modify</param>
        /// <param name="ctx">Simulation context (tick counter, global energy, etc.)</param>
        public void Execute(GridState state, SimContext ctx)
        {
            // Extract config for convenience
            var cfg = ctx.Config;

            // ========================================================================
            // PASS 1a: ENERGY OUTFLOW (Energy Propagation with Black Hole Attraction)
            // ========================================================================
            // Purpose: Distributes energy from active cells to neighbors
            // Features:
            // - Mass-weighted black hole attraction (cells near BHs receive more energy)
            // - Boundary void leakage → black hole formation (charge accumulation)
            // - Energy reflection from blocked paths (stable dependencies)
            // Output: state.Incoming[] filled with energy destined for each cell
            ExecutePass1aEnergyOutflow(state, ctx, cfg);

            // ========================================================================
            // PASS 1b: ENERGY INFLOW (Seed Region Pulse)
            // ========================================================================
            // Purpose: Adds continuous energy to central seed region
            // Features:
            // - Exponentially decaying pulse to 5x5 center block
            // - Ensures seed remains active during early simulation
            // Output: Additional energy added to state.Incoming[]
            ExecutePass1bEnergyInflow(state, ctx, cfg);

            // ========================================================================
            // PASS 2: APPLY ENERGY & COMPUTE VIABILITY
            // ========================================================================
            // Purpose: Applies incoming energy and updates cell states
            // Features:
            // - Energy application: Nlocal += Incoming
            // - Activation cost from global pool
            // - Entropy computation (configuration + gradient)
            // - Viability computation (persistence criterion)
            // - Active state update (Active = V > 0 && N > min)
            // - Baseline decay
            // Output: Updated Nlocal[], Entropy[], V[], Active[]
            ExecutePass2ApplyEnergyAndViability(state, ctx, cfg);

            // ========================================================================
            // PASS 3: GLOBAL ENERGY REPLENISHMENT
            // ========================================================================
            // Purpose: Replenishes global energy pool at constant rate
            // Features:
            // - Constant replenishment rate per tick
            // - Capped at NGlobalMax
            // Output: Updated ctx.NGlobal
            ExecutePass3GlobalReplenishment(ctx, cfg);

            // ========================================================================
            // PASS 4: ENTROPY DIFFUSION
            // ========================================================================
            // Purpose: Smooths entropy gradients across grid
            // Features:
            // - Discrete Laplacian operator (heat equation)
            // - Entropy decay
            // - Black holes maintain maximum entropy (S=1)
            // Output: Smoothed Entropy[]
            ExecutePass4EntropyDiffusion(state, cfg);

            // ========================================================================
            // POST-PROCESSING: FIELD WAVE & BLACK HOLE GROWTH
            // ========================================================================
            // Purpose: Handles geometric expansion (field and black holes)
            // Features:
            // - Field wave: Configuration space expansion at boundaries
            // - Black hole growth: Geometric collapse propagation
            // Output: Updated FieldPresent[], IsBlackHole[], BH entities
            ExecutePostProcessing();
        }

        // ============================================================================
        // PRIVATE: PASS 1a - ENERGY OUTFLOW
        // ============================================================================

        /// <summary>
        /// Executes Pass 1a: Energy propagation with mass-weighted black hole attraction.
        /// </summary>
        private void ExecutePass1aEnergyOutflow(GridState state, SimContext ctx, SimConfig cfg)
        {
            Pass1.GatherOutflow(
                width: state.W,
                height: state.H,
                Nlocal: state.Nlocal,
                V: state.V,
                Active: state.Active,
                IsVacuum: state.IsVacuum,
                incoming: state.Incoming,
                MinBudgetToPropagate: cfg.MinBudgetToPropagate,
                PropagateFrac: cfg.PropagateFrac,
                FieldPresent: state.FieldPresent,
                IsBlackHole: state.IsBlackHole,
                BlackHoleCharge: state.BlackHoleCharge,
                blackHoleThreshold: cfg.BlackHoleFormThreshold,
                MatterAheadThreshold: 0f, // Currently unused parameter
                FieldFirstTick: state.FieldFirstTick,
                EnergyFirstTick: state.EnergyFirstTick,
                BlackHoleId: state.BlackHoleId,
                BlackHoleParent: state.BlackHoleParent,
                BlackHoleMass: state.BlackHoleMass,
                NextBlackHoleId: ref state.NextBlackHoleId,
                tick: ctx.Tick,
                boundaryHitsThisTick: out int boundaryHits,
                maxChargeThisTick: out float maxCharge,
                debugForceBHOnFirstBoundaryHit: false
            );

            // Note: boundaryHits and maxCharge are logged in Pass1 itself
        }

        // ============================================================================
        // PRIVATE: PASS 1b - ENERGY INFLOW
        // ============================================================================

        /// <summary>
        /// Executes Pass 1b: Adds continuous energy pulse to central seed region.
        /// </summary>
        private void ExecutePass1bEnergyInflow(GridState state, SimContext ctx, SimConfig cfg)
        {
            Pass1.GatherInflow(
                width: state.W,
                height: state.H,
                Idx: state.Idx,
                Nlocal: state.Nlocal,
                FieldFirstTick: state.FieldFirstTick,
                EnergyFirstTick: state.EnergyFirstTick,
                FieldPresent: state.FieldPresent,
                Active: state.Active,
                incoming: state.Incoming,
                tick: ctx.Tick
            );
        }

        // ============================================================================
        // PRIVATE: PASS 2 - APPLY ENERGY & VIABILITY
        // ============================================================================

        /// <summary>
        /// Executes Pass 2: Applies incoming energy, computes entropy and viability.
        /// </summary>
        private void ExecutePass2ApplyEnergyAndViability(GridState state, SimContext ctx, SimConfig cfg)
        {
            Pass2.ApplyAndViability(
                Idx: state.Idx,
                width: state.W,
                height: state.H,
                Nlocal: state.Nlocal,
                Entropy: state.Entropy,
                V: state.V,
                Active: state.Active,
                IsVacuum: state.IsVacuum,
                incoming: state.Incoming,
                zeroEnergyTicks: state.ZeroEnergyTicks,
                MinBudgetToPropagate: cfg.MinBudgetToPropagate,
                ActivationCost: cfg.ActivationCost,
                NGlobal: ref ctx.NGlobal,
                NlocalMax: cfg.NlocalMax,
                EntropyGainPerUse: cfg.EntropyGainPerUse,
                DecayLoss: cfg.DecayLoss,
                EntropyPenalty: cfg.EntropyPenalty, // Currently unused
                VacuumEventProbability: cfg.VacuumEventProbability,
                VacuumEventEntropy: cfg.VacuumEventEntropy,
                ComputeViability: _viabilityFunc,
                CountPersistenceConfigurations: _persistenceFunc,
                GridWidth: state.W,
                PropagateFrac: cfg.PropagateFrac,
                FieldPresent: state.FieldPresent,
                IsBlackHole: state.IsBlackHole,
                FieldFirstTick: state.FieldFirstTick,
                EnergyFirstTick: state.EnergyFirstTick,
                tick: ctx.Tick
            );
        }

        // ============================================================================
        // PRIVATE: PASS 3 - GLOBAL REPLENISHMENT
        // ============================================================================

        /// <summary>
        /// Executes Pass 3: Replenishes global energy pool.
        /// </summary>
        private void ExecutePass3GlobalReplenishment(SimContext ctx, SimConfig cfg)
        {
            Pass3.GlobalRecharge(
                NGlobal: ref ctx.NGlobal,
                NGlobalMax: cfg.NGlobalMax,
                GlobalReplenishPerTick: cfg.GlobalReplenishPerTick
            );
        }

        // ============================================================================
        // PRIVATE: PASS 4 - ENTROPY DIFFUSION
        // ============================================================================

        /// <summary>
        /// Executes Pass 4: Diffuses entropy using discrete Laplacian operator.
        /// </summary>
        private void ExecutePass4EntropyDiffusion(GridState state, SimConfig cfg)
        {
            Pass4.EntropyDiffuse(
                width: state.W,
                height: state.H,
                Entropy: state.Entropy,
                entropyNext: state.EntropyNext,
                EntropyDiffuseRate: cfg.EntropyDiffuseRate,
                EntropyDecay: cfg.EntropyDecay,
                IsBlackHole: state.IsBlackHole
            );
        }

        // ============================================================================
        // PRIVATE: POST-PROCESSING
        // ============================================================================

        /// <summary>
        /// Executes post-processing callback (field wave propagation and BH growth).
        /// </summary>
        private void ExecutePostProcessing()
        {
            _postProcessing?.Invoke();
        }
    }
}
