using System;
using Viable.Contracts;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.ExpansionModels.Cosmological
{
    /// <summary>
    /// Cyclic Expansion Model.
    /// 
    /// Paradigm:
    /// - Alternating expansion and contraction phases
    /// - Models cyclic cosmology, Big Bounce scenarios
    /// - Sinks can shrink during contraction (if AllowShrinkage enabled)
    /// 
    /// Key Parameters:
    /// - CyclePeriod: Ticks per full expansion/contraction cycle
    /// - ContractionStrength: How aggressively sinks shrink during contraction
    /// - BounceElasticity: Energy preserved between cycles
    /// </summary>
    public class CyclicModel : ExpansionModelBase
    {
        public override ExpansionModel ModelType => ExpansionModel.Cyclic;
        
        public override string DisplayName => "Cyclic (Big Bounce)";
        
        public override string Description => 
            "Cyclic cosmology model: Alternating expansion and contraction phases. " +
            "Sinks grow during expansion, can shrink during contraction.";
        
        public override string Category => "Cosmological";

        /// <summary>
        /// Sink formation only occurs during expansion phase.
        /// </summary>
        public override bool ShouldFormSink(int cellIdx, int x, int y, GridState state, StepContext context, float currentCharge)
        {
            var phase = GetCurrentPhase(context.Tick);
            
            // No formation during contraction
            if (phase == ExpansionPhase.Contraction)
                return false;
            
            // Standard threshold during expansion, but modulated by phase
            double rate = GetFormationRate(context.Tick, state.Len, CountSinks(state));
            float effectiveThreshold = context.Config.SinkFormationThreshold;
            
            // Lower threshold during peak expansion
            if (phase == ExpansionPhase.MainExpansion)
                effectiveThreshold *= 0.7f;
            
            return currentCharge >= effectiveThreshold;
        }

        /// <summary>
        /// Sink expansion varies by phase.
        /// </summary>
        public override bool ShouldExpandSink(int cellIdx, int x, int y, GridState state, StepContext context, int sinkNeighborCount, int totalNeighborCount)
        {
            if (state.ActiveRegion[cellIdx])
                return false;

            var phase = GetCurrentPhase(context.Tick);
            
            // No expansion during contraction
            if (phase == ExpansionPhase.Contraction)
                return false;
            
            // More aggressive expansion during main expansion phase
            int threshold = phase == ExpansionPhase.MainExpansion
                ? Math.Max(1, totalNeighborCount / 3)
                : (totalNeighborCount / 2) + 1;
            
            return sinkNeighborCount >= threshold;
        }

        /// <summary>
        /// Sinks can shrink during contraction phase.
        /// Edge cells with few sink neighbors evaporate.
        /// </summary>
        public override bool ShouldShrinkSink(int cellIdx, int x, int y, GridState state, StepContext context)
        {
            if (!Config.AllowShrinkage)
                return false;
            
            var phase = GetCurrentPhase(context.Tick);
            
            // Only shrink during contraction
            if (phase != ExpansionPhase.Contraction)
                return false;
            
            // Count sink neighbors
            int sinkNeighbors = 0;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };
            
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= state.W || ny < 0 || ny >= state.H)
                    continue;
                if (state.IsSink[state.Idx(nx, ny)])
                    sinkNeighbors++;
            }
            
            // Edge cells (fewer than 3 sink neighbors) can evaporate
            // Probability based on contraction strength
            if (sinkNeighbors < 3)
            {
                double shrinkProb = Config.ContractionStrength * (3 - sinkNeighbors) / 3.0;
                return context.Rng.NextDouble() < shrinkProb;
            }
            
            return false;
        }

        /// <summary>
        /// Cyclic rate: sinusoidal modulation.
        /// </summary>
        public override double GetFormationRate(int tick, int totalCells, int currentSinkCount)
        {
            if (tick < Config.FormationDelayTicks)
                return 0.0;

            int t = tick - Config.FormationDelayTicks;
            
            // Sinusoidal modulation based on cycle
            double phase = 2.0 * Math.PI * (t + Config.PhaseOffset * Config.CyclePeriod) / Config.CyclePeriod;
            double modulation = 0.5 * (1.0 + Math.Cos(phase)); // 0 to 1
            
            // Scale by bounce elasticity (dampens over time)
            int cycleNum = t / Config.CyclePeriod;
            double damping = Math.Pow(Config.BounceElasticity, cycleNum);
            
            return Config.BaseFormationRate * modulation * damping;
        }

        /// <summary>
        /// Cyclic phases based on position in cycle.
        /// </summary>
        public override ExpansionPhase GetCurrentPhase(int tick)
        {
            if (tick < Config.FormationDelayTicks)
                return ExpansionPhase.Initialization;

            int t = tick - Config.FormationDelayTicks;
            double phaseInCycle = ((t + Config.PhaseOffset * Config.CyclePeriod) % Config.CyclePeriod) / (double)Config.CyclePeriod;
            
            if (phaseInCycle < 0.4)
                return ExpansionPhase.MainExpansion;
            if (phaseInCycle < 0.5)
                return ExpansionPhase.Deceleration;
            if (phaseInCycle < 0.9)
                return ExpansionPhase.Contraction;
            
            return ExpansionPhase.Transition; // Bounce transition
        }

        public override ExpansionConfig GetDefaultConfig()
        {
            return new ExpansionConfig
            {
                Model = ExpansionModel.Cyclic,
                BaseFormationRate = 0.1,
                FormationDelayTicks = 10,
                AllowMerging = true,
                AllowShrinkage = true, // Enable shrinkage for cyclic
                CyclePeriod = 100,
                ContractionStrength = 0.3,
                BounceElasticity = 0.9,
                PhaseOffset = 0.0
            };
        }
    }
}
