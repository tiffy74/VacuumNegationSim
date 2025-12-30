using System;
using UnityEngine;
using Assets.Scripts.Domain;
using Assets.Scripts.Events;

namespace Assets.Scripts.Simulation
{
    public sealed class LegacyTickStep : ISimStep
    {
        private readonly Func<float, float, float, float> _computeViability;
        private readonly Func<int, int> _countPersistenceConfigurations;

        // Hooked behaviours (so controller can inject exact methods)
        private readonly Action _propagateFieldWave;

        public LegacyTickStep(
            Func<float, float, float, float> computeViability,
            Func<int, int> countPersistenceConfigurations,
            Action propagateFieldWave
        )
        {
            _computeViability = computeViability ?? throw new ArgumentNullException(nameof(computeViability));
            _countPersistenceConfigurations = countPersistenceConfigurations ?? throw new ArgumentNullException(nameof(countPersistenceConfigurations));
            _propagateFieldWave = propagateFieldWave ?? throw new ArgumentNullException(nameof(propagateFieldWave));
        }

        public void Execute(GridState s, SimContext ctx)
        {
            // -----------------------------
            // A) Geometry / field wave first
            // -----------------------------
            _propagateFieldWave();

            // -----------------------------
            // B) Pass1: outflow (energy transport attempt) + BH formation via boundary charge
            // -----------------------------
            int boundaryHitsThisTick;
            float maxChargeThisTick;
            int newBH = Pass1.GatherOutflow(
                s.W, s.H,
                s.Nlocal, s.V, s.Active, s.IsVacuum, s.Incoming,
                ctx.Cfg.MinBudgetToPropagate,
                ctx.Cfg.PropagateFrac,
                s.FieldPresent,
                s.IsBlackHole,
                s.BlackHoleCharge,
                ctx.Cfg.BlackHoleFormThreshold,
                ctx.Cfg.MatterAheadThreshold,
                s.FieldFirstTick,
                s.EnergyFirstTick,
                s.BlackHoleId,
                s.BlackHoleParent,
                s.BlackHoleMass,
                ref s.NextBlackHoleId,
                ctx.Tick,
                out boundaryHitsThisTick,
                out maxChargeThisTick
                // [bool debugForceBHOnFirstBoundaryHit = false] is optional, omitted
            );

            // -----------------------------
            // C) Pass1: central inflow pulse / seeding
            // -----------------------------
            Pass1.GatherInflow(
                s.W, s.H,
                (x, y) => s.Idx(x, y),
                s.Nlocal,
                s.FieldFirstTick,
                s.EnergyFirstTick,
                s.FieldPresent,
                s.Active,
                s.Incoming,
                ctx.Tick
            );

            // -----------------------------
            // D) Pass2: deposit inflow + entropy/viability/active update
            // -----------------------------
            Pass2.ApplyAndViability(
                (x, y) => s.Idx(x, y),
                s.W, s.H,
                s.Nlocal, s.Entropy, s.V, s.Active, s.IsVacuum,
                s.Incoming,
                s.ZeroEnergyTicks,
                ctx.Cfg.MinBudgetToPropagate,
                ctx.Cfg.ActivationCost,
                ref ctx.NGlobal,
                ctx.Cfg.NlocalMax,
                ctx.Cfg.EntropyGainPerUse,
                ctx.Cfg.DecayLoss,
                ctx.Cfg.EntropyPenalty,
                ctx.Cfg.VacuumEventProbability,
                ctx.Cfg.VacuumEventEntropy,
                _computeViability,
                _countPersistenceConfigurations,
                s.W,
                ctx.Cfg.PropagateFrac,
                s.FieldPresent,
                s.IsBlackHole,
                s.FieldFirstTick,
                s.EnergyFirstTick,
                ctx.Tick
            );

            // -----------------------------
            // E) Black hole behaviour
            // IMPORTANT: for "hard-wall BH halo", set BlackHoleDrainFrac = 0 in config.
            // -----------------------------
            //float drained = 0f;
            //if (ctx.Cfg.BlackHoleDrainFrac > 0f)
            //{
            //    drained = BlackHoles.BlackHoleAttractEnergy(
            //        s,
            //        ctx.Cfg.BlackHoleDrainFrac,
            //        false, // fieldOnly
            //        ctx.Cfg.BlackHoleRecoilFrac
            //    );
            //}

            // -----------------------------
            // F) Global recharge + scale factor
            // -----------------------------
            Pass3.GlobalRecharge(ref ctx.NGlobal, ctx.Cfg.NGlobalMax, ctx.Cfg.GlobalReplenishPerTick);
            ctx.ScaleFactor *= ctx.Cfg.ExpansionRate;

            // -----------------------------
            // G) Entropy diffusion
            // -----------------------------
            Pass4.EntropyDiffuse(s.W, s.H, s.Entropy, s.EntropyNext, ctx.Cfg.EntropyDiffuseRate, ctx.Cfg.EntropyDecay, s.IsBlackHole);

            // -----------------------------
            // H) Diagnostics (every 20 ticks)
            // -----------------------------
            if (ctx.Tick % 20 == 0)
            {
                LogDiagnostics(s, ctx, newBH, 0);
            }
        }

        private static void LogDiagnostics(GridState s, SimContext ctx, int newBH, float drained)
        {
            // Counts
            int fieldCount = 0;
            int bhCells = 0;

            // Energy stats
            int energyCount = 0;
            float maxBHMass = 0f;

            // Halo/ring stats
            float ringSum = 0f;
            int ringCount = 0;

            float fieldSum = 0f;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    if (s.FieldPresent[i])
                    {
                        fieldCount++;
                        fieldSum += s.Nlocal[i];
                    }

                    if (s.Nlocal[i] > 0.1f) energyCount++;

                    if (!s.IsBlackHole[i]) continue;

                    bhCells++;

                    // ring around BH cell
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H) continue;

                        int ni = s.Idx(nx, ny);
                        if (!s.FieldPresent[ni]) continue;
                        if (s.IsBlackHole[ni]) continue;

                        ringSum += s.Nlocal[ni];
                        ringCount++;
                    }
                }
            }

            // If you store BH entity masses by root id, estimate max mass by scanning BHMass array
            // (safe, cheap). Only do it here every 20 ticks.
            for (int id = 1; id < s.BlackHoleMass.Length; id++)
            {
                if (s.BlackHoleParent[id] == 0) continue;
                if (s.BlackHoleMass[id] > maxBHMass) maxBHMass = s.BlackHoleMass[id];
            }

            float ringAvg = ringCount > 0 ? ringSum / ringCount : 0f;
            float fieldAvg = fieldCount > 0 ? fieldSum / fieldCount : 0f;

            Debug.Log(
                $"Tick {ctx.Tick} | " +
                $"FieldCount={fieldCount} | BHCells={bhCells} | NewBH={newBH} | " +
                $"Drained={drained:F4} | MaxBHMass={maxBHMass:F4} | " +
                $"EnergyCount>0.1={energyCount} | " +
                $"RingAvgN={ringAvg:F4} FieldAvgN={fieldAvg:F4} Ring/Field={(fieldAvg > 0f ? ringAvg / fieldAvg : 0f):F2} | " +
                $"NGlobal={ctx.NGlobal:F4}"
            );
        }
    }
}
