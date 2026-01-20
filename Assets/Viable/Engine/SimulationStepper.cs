using System;
using System.Collections.Generic;
using Viable.Contracts;
using Viable.Engine.Interfaces;
using Viable.Engine.State;
using Viable.Engine.Execution;
using Viable.Engine.Steps;
using Viable.Engine.Logic;
using Viable.Engine.Computation;

namespace Viable.Engine
{
    /// <summary>
    /// Standard simulation stepper that orchestrates all step phases.
    /// This is the Engine equivalent of LegacyTickStep.
    /// </summary>
    public sealed class SimulationStepper : IStepPhase
    {
        private readonly Func<int, int> _countPersistenceConfigurations;

        public SimulationStepper(Func<int, int> countPersistenceConfigurations)
        {
            _countPersistenceConfigurations = countPersistenceConfigurations ?? throw new ArgumentNullException(nameof(countPersistenceConfigurations));
        }

        public void Execute(GridState state, StepContext context)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var cfg = context.Config;

            // -----------------------------
            // A) Region expansion first
            // -----------------------------
            RegionExpansionLogic.ExpandActiveRegion(
                state,
                context.Tick,
                cfg.RegionExpansionChance,
                cfg.RegionExpansionCost,
                cfg.RegionExpansionMinSource,
                context.Rng, // Deterministic!
                cfg.RegionExpansionRequiresViability,
                cfg.RegionExpansionSeedsResource,
                cfg.RegionSeedResource
            );

            // Sink expansion (simple geometric growth)
            ExpandSinkRegions(state);

            // -----------------------------
            // B) Pass1: Outflow + sink formation
            // -----------------------------
            int newSinks = OutflowPhase.GatherOutflow(
                state.W, state.H,
                state.ResourceLocal, state.V, state.Active, state.IsInactive, state.Incoming,
                cfg.MinBudgetToPropagate,
                cfg.PropagateFrac,
                state.ActiveRegion,
                state.IsSink,
                state.SinkCharge,
                cfg.SinkFormationThreshold,
                cfg.MatterAheadThreshold,
                state.RegionActivationTick,
                state.ResourceFirstTick,
                state.SinkId,
                state.SinkParent,
                state.SinkMass,
                ref state.NextSinkId,
                context.Tick,
                out int boundaryHits,
                out float maxCharge
            );

            // -----------------------------
            // C) Pass1: Central inflow (seed pulse)
            // -----------------------------
            OutflowPhase.GatherInflow(
                state.W, state.H,
                (x, y) => state.Idx(x, y),
                state.ResourceLocal,
                state.RegionActivationTick,
                state.ResourceFirstTick,
                state.ActiveRegion,
                state.Active,
                state.Incoming,
                context.Tick
            );

            // -----------------------------
            // D) Pass2: Apply inflow + viability
            // -----------------------------
            float thresholdEff = ViabilityCalculator.ComputeEffectiveThreshold(
                context.ResourceGlobal,
                cfg.ResourceGlobalMax,
                cfg.EthreshBase,
                cfg.GlobalScarcityK
            );

            InflowPhase.ApplyAndViability(
                (x, y) => state.Idx(x, y),
                state.W, state.H,
                state.ResourceLocal, state.ComplexityMetric, state.V, state.Active, state.IsInactive,
                state.Incoming,
                state.ZeroResourceTicks,
                cfg.MinBudgetToPropagate,
                cfg.ActivationCost,
                ref context.ResourceGlobal,
                cfg.ResourceLocalMax,
                cfg.ComplexityGainPerUse,
                cfg.DecayLoss,
                cfg.ComplexityPenalty,
                cfg.PerturbationProbability,
                cfg.PerturbationComplexity,
                (inflow, resource, complexity) => ViabilityCalculator.Compute(
                    inflow, resource, complexity,
                    cfg.ComplexityViabilityGainA,
                    cfg.ComplexityViabilityGainK,
                    cfg.DecayLoss,
                    thresholdEff
                ),
                _countPersistenceConfigurations,
                state.W,
                cfg.PropagateFrac,
                state.ActiveRegion,
                state.IsSink,
                state.RegionActivationTick,
                state.ResourceFirstTick,
                context.Tick,
                context.Rng // Deterministic!
            );

            // -----------------------------
            // E) Global recharge + scale
            // -----------------------------
            RechargePhase.GlobalRecharge(ref context.ResourceGlobal, cfg.ResourceGlobalMax, cfg.GlobalReplenishPerTick);
            context.ScaleFactor *= cfg.ExpansionRate;

            // -----------------------------
            // F) Complexity diffusion
            // -----------------------------
            DiffusionPhase.ComplexityDiffuse(
                state.W, state.H,
                state.ComplexityMetric, state.ComplexityNext,
                cfg.ComplexityDiffusionRate, cfg.ComplexityDecay,
                state.IsSink
            );

            // Event emission could happen here (instead of Debug.Log)
            // e.g., context.EmitEvent(new SimulationEvent { Type = "SinkFormed", ... });
        }

        /// <summary>
        /// Simple geometric expansion of sink regions (fills single-cell gaps).
        /// Extracted from SinkRegions.ExpandSinkRegions.
        /// </summary>
        private void ExpandSinkRegions(GridState state)
        {
            bool[] nextSink = (bool[])state.IsSink.Clone();
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int y = 0; y < state.H; y++)
            {
                for (int x = 0; x < state.W; x++)
                {
                    int i = state.Idx(x, y);
                    if (state.IsSink[i]) continue;
                    if (state.ActiveRegion[i]) continue; // Only expand into non-active regions

                    // Check if surrounded by sinks (3+ neighbors)
                    int sinkNeighbors = 0;
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= state.W || ny < 0 || ny >= state.H) continue;
                        int ni = state.Idx(nx, ny);
                        if (state.IsSink[ni]) sinkNeighbors++;
                    }

                    if (sinkNeighbors >= 3)
                    {
                        nextSink[i] = true;
                        // Merge with adjacent sink using SinkLogic
                        SinkLogic.AssignOrMergeAtCell(i, state.W, state.H, nextSink, state.SinkId, state.SinkParent, state.SinkMass, ref state.NextSinkId);
                    }
                }
            }

            state.IsSink = nextSink;
        }
    }
}
