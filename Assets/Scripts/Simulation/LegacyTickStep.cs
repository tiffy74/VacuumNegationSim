using System;
using UnityEngine;
using Assets.Scripts.Domain;
using Assets.Scripts.Events;

// [DEPRECATED - Phase 5] This file will be removed in Phase 7
// New location: Assets/Viable/Engine/SimulationStepper.cs
// DO NOT modify this file - changes go to new location

namespace Assets.Scripts.Simulation
{
    public sealed class LegacyTickStep : ISimStep
    {
        private readonly Func<float, float, float, float> _computeViability;
        private readonly Func<int, int> _countPersistenceConfigurations;

        // Hooked behaviours (so controller can inject exact methods)
        private readonly Action _expandRegions;

        public LegacyTickStep(
            Func<float, float, float, float> computeViability,
            Func<int, int> countPersistenceConfigurations,
            Action expandRegions
        )
        {
            _computeViability = computeViability ?? throw new ArgumentNullException(nameof(computeViability));
            _countPersistenceConfigurations = countPersistenceConfigurations ?? throw new ArgumentNullException(nameof(countPersistenceConfigurations));
            _expandRegions = expandRegions ?? throw new ArgumentNullException(nameof(expandRegions));
        }

        public void Execute(StateGrid s, SimContext ctx)
        {
            // -----------------------------
            // A) Region expansion first
            // -----------------------------
            _expandRegions();

            // -----------------------------
            // B) Pass1: outflow (resource transport attempt) + sink formation via boundary charge
            // -----------------------------
            int boundaryHitsThisTick;
            float maxChargeThisTick;
            int newSinks = Pass1.GatherOutflow(
                s.W, s.H,
                s.ResourceLocal, s.V, s.Active, s.IsInactive, s.Incoming,
                ctx.Cfg.MinBudgetToPropagate,
                ctx.Cfg.PropagateFrac,
                s.ActiveRegion,
                s.IsSink,
                s.SinkCharge,
                ctx.Cfg.SinkFormationThreshold,
                ctx.Cfg.MatterAheadThreshold,
                s.RegionActivationTick,
                s.ResourceFirstTick,
                s.SinkId,
                s.SinkParent,
                s.SinkMass,
                ref s.NextSinkId,
                ctx.Tick,
                out boundaryHitsThisTick,
                out maxChargeThisTick
            );

            // -----------------------------
            // C) Pass1: central inflow pulse / seeding
            // -----------------------------
            Pass1.GatherInflow(
                s.W, s.H,
                (x, y) => s.Idx(x, y),
                s.ResourceLocal,
                s.RegionActivationTick,
                s.ResourceFirstTick,
                s.ActiveRegion,
                s.Active,
                s.Incoming,
                ctx.Tick
            );

            // -----------------------------
            // D) Pass2: deposit inflow + complexity/viability/active update
            // -----------------------------
            Pass2.ApplyAndViability(
                (x, y) => s.Idx(x, y),
                s.W, s.H,
                s.ResourceLocal, s.ComplexityMetric, s.V, s.Active, s.IsInactive,
                s.Incoming,
                s.ZeroResourceTicks,
                ctx.Cfg.MinBudgetToPropagate,
                ctx.Cfg.ActivationCost,
                ref ctx.ResourceGlobal,
                ctx.Cfg.ResourceLocalMax,
                ctx.Cfg.ComplexityGainPerUse,
                ctx.Cfg.DecayLoss,
                ctx.Cfg.ComplexityPenalty,
                ctx.Cfg.PerturbationProbability,
                ctx.Cfg.PerturbationComplexity,
                _computeViability,
                _countPersistenceConfigurations,
                s.W,
                ctx.Cfg.PropagateFrac,
                s.ActiveRegion,
                s.IsSink,
                s.RegionActivationTick,
                s.ResourceFirstTick,
                ctx.Tick
            );

            // -----------------------------
            // E) Sink region behaviour
            // IMPORTANT: for "hard-wall sink halo", set SinkDrainFraction = 0 in config.
            // -----------------------------
            //float drained = 0f;
            //if (ctx.Cfg.SinkDrainFraction > 0f)
            //{
            //    drained = SinkRegions.SinkAbsorbResource(
            //        s,
            //        ctx.Cfg.SinkDrainFraction,
            //        false, // activeRegionOnly
            //        ctx.Cfg.SinkRecoilFraction
            //    );
            //}

            // -----------------------------
            // F) Global recharge + scale factor
            // -----------------------------
            Pass3.GlobalRecharge(ref ctx.ResourceGlobal, ctx.Cfg.ResourceGlobalMax, ctx.Cfg.GlobalReplenishPerTick);
            ctx.ScaleFactor *= ctx.Cfg.ExpansionRate;

            // -----------------------------
            // G) Complexity diffusion
            // -----------------------------
            Pass4.ComplexityDiffuse(s.W, s.H, s.ComplexityMetric, s.ComplexityNext, ctx.Cfg.ComplexityDiffusionRate, ctx.Cfg.ComplexityDecay, s.IsSink);

            // -----------------------------
            // H) Diagnostics (every 20 ticks)
            // -----------------------------
            if (ctx.Tick % 20 == 0)
            {
                LogDiagnostics(s, ctx, newSinks, 0);
            }
        }

        private static void LogDiagnostics(StateGrid s, SimContext ctx, int newSinks, float drained)
        {
            // Counts
            int regionCount = 0;
            int sinkCells = 0;

            // Resource stats
            int resourceCount = 0;
            float maxSinkMass = 0f;

            // Halo/ring stats
            float ringSum = 0f;
            int ringCount = 0;

            float regionSum = 0f;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    if (s.ActiveRegion[i])
                    {
                        regionCount++;
                        regionSum += s.ResourceLocal[i];
                    }

                    if (s.ResourceLocal[i] > 0.1f) resourceCount++;

                    if (!s.IsSink[i]) continue;

                    sinkCells++;

                    // ring around sink cell
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H) continue;

                        int ni = s.Idx(nx, ny);
                        if (!s.ActiveRegion[ni]) continue;
                        if (s.IsSink[ni]) continue;

                        ringSum += s.ResourceLocal[ni];
                        ringCount++;
                    }
                }
            }

            // Estimate max mass by scanning SinkMass array
            for (int id = 1; id < s.SinkMass.Length; id++)
            {
                if (s.SinkParent[id] == 0) continue;
                if (s.SinkMass[id] > maxSinkMass) maxSinkMass = s.SinkMass[id];
            }

            float ringAvg = ringCount > 0 ? ringSum / ringCount : 0f;
            float regionAvg = regionCount > 0 ? regionSum / regionCount : 0f;

            Debug.Log(
                $"Tick {ctx.Tick} | " +
                $"RegionCount={regionCount} | SinkCells={sinkCells} | NewSinks={newSinks} | " +
                $"Drained={drained:F4} | MaxSinkMass={maxSinkMass:F4} | " +
                $"ResourceCount>0.1={resourceCount} | " +
                $"RingAvgR={ringAvg:F4} RegionAvgR={regionAvg:F4} Ring/Region={(regionAvg > 0f ? ringAvg / regionAvg : 0f):F2} | " +
                $"ResourceGlobal={ctx.ResourceGlobal:F4}"
            );
        }
    }
}
