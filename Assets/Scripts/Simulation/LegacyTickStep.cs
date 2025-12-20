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
            // 1. Expand the field wave first
            _propagateFieldWave();

            Pass1.GatherOutflow(
                s.W, s.H,
                s.Nlocal, s.V, s.IsVacuum, s.Incoming,
                ctx.Cfg.MinBudgetToPropagate, ctx.Cfg.PropagateFrac,
                s.FieldPresent, s.IsBlackHole, s.BlackHoleCharge,
                200.0f, ctx.Cfg.MatterAheadThreshold,
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

            _growBlackHoles();
            _blackHoleAttractEnergy();

            Pass3.GlobalRecharge(ref ctx.NGlobal, ctx.Cfg.NGlobalMax, ctx.Cfg.GlobalReplenishPerTick);

            ctx.ScaleFactor *= ctx.Cfg.ExpansionRate;

            Pass4.EntropyDiffuse(s.W, s.H, s.Entropy, s.EntropyNext, ctx.Cfg.EntropyDiffuseRate, ctx.Cfg.EntropyDecay);
        }
    }
}
