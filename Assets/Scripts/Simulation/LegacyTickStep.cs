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
            bool[] bhPrev = (bool[])s.IsBlackHole.Clone();

            void EnforceBlackHoleInvariants()
            {
                for (int i = 0; i < s.Len; i++)
                {
                    if (!s.IsBlackHole[i] && !bhPrev[i]) continue;
                    s.IsBlackHole[i] = true;      // sticky
                    s.FieldPresent[i] = false;    // no field on BH
                    s.Active[i] = 0;              // inert
                    s.Nlocal[i] = 0f;             // no energy
                    s.Incoming[i] = 0f;           // no inflow
                    s.Entropy[i] = 1f;            // max entropy for rendering
                }
            }

            Pass1.GatherOutflow(
                s.W, s.H,
                s.Nlocal, s.V, s.Active, s.IsVacuum, s.Incoming,
                ctx.Cfg.MinBudgetToPropagate, ctx.Cfg.PropagateFrac,
                s.FieldPresent, s.IsBlackHole, s.BlackHoleCharge,
                ctx.Cfg.BlackHoleFormThreshold, ctx.Cfg.MatterAheadThreshold,
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
            EnforceBlackHoleInvariants();

            _propagateFieldWave();
            EnforceBlackHoleInvariants();

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
                (i) => _countPersistenceConfigurations(i),
                s.W,
                ctx.Cfg.PropagateFrac,
                s.FieldPresent,
                s.IsBlackHole,
                s.FieldFirstTick,
                s.EnergyFirstTick,
                ctx.Tick
            );
            EnforceBlackHoleInvariants();

            BlackHoles.BlackHoleAttractEnergy(s, 4f, true);
            EnforceBlackHoleInvariants();

            Pass3.GlobalRecharge(ref ctx.NGlobal, ctx.Cfg.NGlobalMax, ctx.Cfg.GlobalReplenishPerTick);
            ctx.ScaleFactor *= ctx.Cfg.ExpansionRate;
            Pass4.EntropyDiffuse(s.W, s.H, s.Entropy, s.EntropyNext, ctx.Cfg.EntropyDiffuseRate, ctx.Cfg.EntropyDecay);
            EnforceBlackHoleInvariants();
        }
    }
}
