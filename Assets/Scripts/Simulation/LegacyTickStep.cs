using Assets.Scripts.Domain;
using Assets.Scripts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Simulation
{
    public sealed class LegacyTickStep : ISimStep
    {
        private readonly System.Func<float, float, float, float> _computeViability;
        private readonly System.Func<int, int> _countPersistenceConfigurations;
        private readonly System.Action _propagateFieldWave;
        private readonly System.Action _growBlackHoles;
        private readonly System.Action _blackHoleAttractEnergy;

        public LegacyTickStep(
            System.Func<float, float, float, float> computeViability,
            System.Func<int, int> countPersistenceConfigurations,
            System.Action propagateFieldWave,
            System.Action growBlackHoles,
            System.Action blackHoleAttractEnergy)
        {
            _computeViability = computeViability;
            _countPersistenceConfigurations = countPersistenceConfigurations;
            _propagateFieldWave = propagateFieldWave;
            _growBlackHoles = growBlackHoles;
            _blackHoleAttractEnergy = blackHoleAttractEnergy;
        }

        public void Execute(GridState s, SimContext ctx)
        {
            if (ctx.Tick % 10 == 0)
                UnityEngine.Debug.Log($"[Tick {ctx.Tick}] LegacyTickStep.Execute start (C)");

            void LogStats(string stage)
            {
                if (ctx.Tick % 10 != 0) return;

                double sumN = 0, sumIn = 0, sumEnt = 0;
                int fieldCount = 0, activeCount = 0;
                float vMin = float.PositiveInfinity, vMax = float.NegativeInfinity;

                for (int i = 0; i < s.Len; i++)
                {
                    sumN += s.Nlocal[i];
                    sumIn += s.Incoming[i];
                    sumEnt += s.Entropy[i];
                    if (s.FieldPresent[i]) fieldCount++;
                    if (s.Active[i] == 1) activeCount++;
                    float v = s.V[i];
                    if (v < vMin) vMin = v;
                    if (v > vMax) vMax = v;
                }

                UnityEngine.Debug.Log(
                    $"[Tick {ctx.Tick}] {stage} | sumN={sumN:F3} sumIn={sumIn:F3} field={fieldCount} active={activeCount} vMin={vMin:F3} vMax={vMax:F3} sumEnt={sumEnt:F3}");
            }

            LogStats("BEFORE Pass1");

            // 1. Expand the field wave first
            _propagateFieldWave();

            Pass1.GatherOutflow(
                s.W, s.H,
                s.Nlocal, s.V, s.Active, s.IsVacuum, s.Incoming,
                ctx.Cfg.MinBudgetToPropagate, ctx.Cfg.PropagateFrac,
                s.FieldPresent, s.IsBlackHole, s.BlackHoleCharge,
                1.0f, ctx.Cfg.MatterAheadThreshold,
                s.FieldFirstTick, s.EnergyFirstTick
            );

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

            LogStats("AFTER Pass1");

            Pass2.ApplyAndViability(
                (x, y) => s.Idx(x, y),
                s.W, s.H,
                s.Nlocal, s.Entropy, s.V, s.Active, s.IsVacuum, s.Incoming,
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
                (i) => _countPersistenceConfigurations(i), // FIX: Use the injected delegate and remove extra args
                s.W,
                ctx.Cfg.PropagateFrac,
                s.FieldPresent,
                s.IsBlackHole,
                s.FieldFirstTick,
                s.EnergyFirstTick,
                ctx.Tick
            );

            LogStats("AFTER Pass2");

            // Black holes are static; no spread. Apply attraction/drain.
            BlackHoles.BlackHoleAttractEnergy(s, 0.2f, true);

            Pass3.GlobalRecharge(ref ctx.NGlobal, ctx.Cfg.NGlobalMax, ctx.Cfg.GlobalReplenishPerTick);

            LogStats("AFTER Pass3");

            ctx.ScaleFactor *= ctx.Cfg.ExpansionRate;

            Pass4.EntropyDiffuse(s.W, s.H, s.Entropy, s.EntropyNext, ctx.Cfg.EntropyDiffuseRate, ctx.Cfg.EntropyDecay);

            LogStats("AFTER Pass4");
        }
    }
}
