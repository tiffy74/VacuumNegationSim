using System;
using Assets.Scripts.Domain;
using Assets.Scripts.Events;
using UnityEngine;

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
        
        private readonly Func<float, float, float, float> _viabilityFunc;
        private readonly Func<int, int> _persistenceFunc;
        private readonly Action _postProcessing;

        // ============================================================================
        // CONSTRUCTOR
        // ============================================================================

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

        public void Execute(GridState state, SimContext ctx)
        {
            var cfg = ctx.Config;

            // Reset diagnostic accumulators
            ctx.BlockedIntoBH = 0f;
            ctx.LeakAttempts = 0;
            ctx.LeakEnergy = 0f;

            // Execute simulation passes
            int boundaryHits, newBHs;
            float maxCharge;
            
            ExecutePass1aEnergyOutflow(state, ctx, cfg, out boundaryHits, out maxCharge, out newBHs);
            ExecutePass1bEnergyInflow(state, ctx, cfg);
            ExecutePass2ApplyEnergyAndViability(state, ctx, cfg);
            
            // Compute halo diagnostics
            ComputeAdjEnergyNearBH(state, ctx);
            ComputeBackgroundVsHaloEnergy(state, ctx);
            
            ExecutePass3GlobalReplenishment(ctx, cfg);
            ExecutePass4EntropyDiffusion(state, cfg);
            ExecutePostProcessing();

            // Consolidated logging via Diagnostics class
            var diagnostics = new Diagnostics(state, ctx, cfg.MinBudgetToPropagate);
            diagnostics.LogComprehensiveTick(boundaryHits, maxCharge, newBHs);
            
            // Optional: Enable additional logging every N ticks
            if (ctx.Tick % 20 == 0)
            {
                diagnostics.LogRegionalStatistics();
            }
        }

        // ============================================================================
        // PRIVATE: PASS EXECUTION
        // ============================================================================

        private void ExecutePass1aEnergyOutflow(GridState state, SimContext ctx, SimConfig cfg,
            out int boundaryHits, out float maxCharge, out int newBHs)
        {
            newBHs = Pass1.GatherOutflow(
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
                MatterAheadThreshold: 0f,
                FieldFirstTick: state.FieldFirstTick,
                EnergyFirstTick: state.EnergyFirstTick,
                BlackHoleId: state.BlackHoleId,
                BlackHoleParent: state.BlackHoleParent,
                BlackHoleMass: state.BlackHoleMass,
                NextBlackHoleId: ref state.NextBlackHoleId,
                tick: ctx.Tick,
                boundaryHitsThisTick: out boundaryHits,
                maxChargeThisTick: out maxCharge,
                blockedIntoBH: ref ctx.BlockedIntoBH,
                leakAttempts: ref ctx.LeakAttempts,
                leakEnergy: ref ctx.LeakEnergy,
                debugForceBHOnFirstBoundaryHit: false
            );
        }

        private void ExecutePass1bEnergyInflow(GridState state, SimContext ctx, SimConfig cfg)
        {
            Pass1.GatherInflow(state.W, state.H, state.Idx, state.Nlocal, state.FieldFirstTick,
                state.EnergyFirstTick, state.FieldPresent, state.Active, state.Incoming, ctx.Tick);
        }

        private void ExecutePass2ApplyEnergyAndViability(GridState state, SimContext ctx, SimConfig cfg)
        {
            Pass2.ApplyAndViability(state.Idx, state.W, state.H, state.Nlocal, state.Entropy, state.V,
                state.Active, state.IsVacuum, state.Incoming, state.ZeroEnergyTicks, cfg.MinBudgetToPropagate,
                cfg.ActivationCost, ref ctx.NGlobal, cfg.NlocalMax, cfg.EntropyGainPerUse, cfg.DecayLoss,
                cfg.EntropyPenalty, cfg.VacuumEventProbability, cfg.VacuumEventEntropy, _viabilityFunc,
                _persistenceFunc, state.W, cfg.PropagateFrac, state.FieldPresent, state.IsBlackHole,
                state.FieldFirstTick, state.EnergyFirstTick, ctx.Tick);
        }

        private void ExecutePass3GlobalReplenishment(SimContext ctx, SimConfig cfg)
        {
            Pass3.GlobalRecharge(ref ctx.NGlobal, cfg.NGlobalMax, cfg.GlobalReplenishPerTick);
        }

        private void ExecutePass4EntropyDiffusion(GridState state, SimConfig cfg)
        {
            Pass4.EntropyDiffuse(state.W, state.H, state.Entropy, state.EntropyNext,
                cfg.EntropyDiffuseRate, cfg.EntropyDecay, state.IsBlackHole);
        }

        private void ExecutePostProcessing()
        {
            _postProcessing?.Invoke();
        }

        // ============================================================================
        // PRIVATE: HALO DIAGNOSTICS COMPUTATION
        // ============================================================================

        private void ComputeAdjEnergyNearBH(GridState state, SimContext ctx)
        {
            float totalAdjacentEnergy = 0f;
            int validNeighborCount = 0;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int y = 0; y < state.H; y++)
            {
                for (int x = 0; x < state.W; x++)
                {
                    int idx = state.Idx(x, y);
                    if (!state.IsBlackHole[idx]) continue;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= state.W || ny < 0 || ny >= state.H) continue;

                        int neighborIdx = state.Idx(nx, ny);
                        if (state.IsBlackHole[neighborIdx] || state.IsVacuum[neighborIdx]) continue;

                        totalAdjacentEnergy += state.Nlocal[neighborIdx];
                        validNeighborCount++;
                    }
                }
            }

            ctx.AdjEnergyNearBH = (validNeighborCount > 0) ? (totalAdjacentEnergy / validNeighborCount) : 0f;
        }

        private void ComputeBackgroundVsHaloEnergy(GridState state, SimContext ctx)
        {
            float totalFieldEnergy = 0f;
            int fieldCellCount = 0;
            float totalRingEnergy = 0f;
            int ringCellCount = 0;

            for (int i = 0; i < state.Len; i++)
            {
                if (state.FieldPresent[i] && !state.IsBlackHole[i])
                {
                    totalFieldEnergy += state.Nlocal[i];
                    fieldCellCount++;
                }
            }

            ctx.AvgNField = (fieldCellCount > 0) ? (totalFieldEnergy / fieldCellCount) : 0f;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int y = 0; y < state.H; y++)
            {
                for (int x = 0; x < state.W; x++)
                {
                    int idx = state.Idx(x, y);
                    if (!state.IsBlackHole[idx]) continue;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= state.W || ny < 0 || ny >= state.H) continue;

                        int neighborIdx = state.Idx(nx, ny);
                        if (state.IsBlackHole[neighborIdx] || state.IsVacuum[neighborIdx]) continue;

                        totalRingEnergy += state.Nlocal[neighborIdx];
                        ringCellCount++;
                    }
                }
            }

            ctx.AvgNRingBH = (ringCellCount > 0) ? (totalRingEnergy / ringCellCount) : 0f;
            ctx.RingMinusField = ctx.AvgNRingBH - ctx.AvgNField;

            const float epsilon = 1e-6f;
            ctx.RingRatio = ctx.AvgNRingBH / (ctx.AvgNField + epsilon);
        }
    }
}
