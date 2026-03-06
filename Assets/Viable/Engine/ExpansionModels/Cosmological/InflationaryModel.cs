using System;
using Viable.Contracts;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.ExpansionModels.Cosmological
{
    /// <summary>
    /// Inflationary Expansion Model.
    /// 
    /// Paradigm:
    /// - Rapid early expansion that decays exponentially over time
    /// - Models cosmic inflation ? matter-dominated transition
    /// - Early "burst" of sink formation followed by slowdown
    /// 
    /// Key Parameters:
    /// - InflationPeakTick: When formation rate peaks
    /// - InflationDecayRate: How quickly rate decays after peak
    /// </summary>
    public class InflationaryModel : ExpansionModelBase
    {
        public override ExpansionModel ModelType => ExpansionModel.Inflationary;
        
        public override string DisplayName => "Inflationary";
        
        public override string Description => 
            "Cosmological inflation model: Rapid early sink formation that decays " +
            "exponentially. Models cosmic inflation followed by matter-dominated era.";
        
        public override string Category => "Cosmological";

        /// <summary>
        /// Sink formation uses time-dependent rate that peaks early then decays.
        /// </summary>
        public override bool ShouldFormSink(int cellIdx, int x, int y, GridState state, StepContext context, float currentCharge)
        {
            // Get time-dependent formation rate
            double rate = GetFormationRate(context.Tick, state.Len, CountSinks(state));
            
            // Threshold scales inversely with rate (higher rate = lower threshold)
            float effectiveThreshold = (float)(context.Config.SinkFormationThreshold / Math.Max(0.01, rate * 10));
            
            // Still require minimum charge, but threshold is modulated by inflation rate
            return currentCharge >= effectiveThreshold;
        }

        /// <summary>
        /// Sink expansion follows standard majority rule but with rate modulation.
        /// </summary>
        public override bool ShouldExpandSink(int cellIdx, int x, int y, GridState state, StepContext context, int sinkNeighborCount, int totalNeighborCount)
        {
            if (state.ActiveRegion[cellIdx])
                return false;

            // During inflation phase, expansion is more aggressive
            double rate = GetFormationRate(context.Tick, state.Len, CountSinks(state));
            
            // Lower threshold during high-rate periods
            int threshold = rate > 0.5 
                ? Math.Max(1, totalNeighborCount / 3) // More aggressive during inflation
                : (totalNeighborCount / 2) + 1;       // Standard majority after
            
            return sinkNeighborCount >= threshold;
        }

        /// <summary>
        /// Inflationary rate: exponential rise to peak, then exponential decay.
        /// </summary>
        public override double GetFormationRate(int tick, int totalCells, int currentSinkCount)
        {
            if (tick < Config.FormationDelayTicks)
                return 0.0;

            int t = tick - Config.FormationDelayTicks;
            
            if (t < Config.InflationPeakTick)
            {
                // Rising phase: exponential approach to peak
                double riseRate = 1.0 - Math.Exp(-t * 0.3);
                return Config.BaseFormationRate * riseRate * 3.0; // Amplified during rise
            }
            else
            {
                // Decay phase: exponential decay from peak
                int tDecay = t - Config.InflationPeakTick;
                double decayFactor = Math.Exp(-Config.InflationDecayRate * tDecay);
                return Config.BaseFormationRate * decayFactor;
            }
        }

        /// <summary>
        /// Inflationary model has distinct phases.
        /// </summary>
        public override ExpansionPhase GetCurrentPhase(int tick)
        {
            if (tick < Config.FormationDelayTicks)
                return ExpansionPhase.Initialization;

            int t = tick - Config.FormationDelayTicks;
            
            if (t < Config.InflationPeakTick / 2)
                return ExpansionPhase.EarlyExpansion;
            
            if (t < Config.InflationPeakTick)
                return ExpansionPhase.Transition;
            
            if (t < Config.InflationPeakTick * 3)
                return ExpansionPhase.Deceleration;
            
            return ExpansionPhase.Equilibrium;
        }

        public override ExpansionConfig GetDefaultConfig()
        {
            return new ExpansionConfig
            {
                Model = ExpansionModel.Inflationary,
                BaseFormationRate = 0.15,
                FormationDelayTicks = 10,
                AllowMerging = true,
                AllowShrinkage = false,
                InflationPeakTick = 20,
                InflationDecayRate = 0.08
            };
        }
    }
}
