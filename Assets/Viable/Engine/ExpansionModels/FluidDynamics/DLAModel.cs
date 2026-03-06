using System;
using Viable.Contracts;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.ExpansionModels.FluidDynamics
{
    /// <summary>
    /// Diffusion-Limited Aggregation (DLA) Expansion Model.
    /// 
    /// Paradigm:
    /// - New sink cells "stick" to existing sinks with some probability
    /// - Creates fractal, snowflake-like growth patterns
    /// - Tips grow faster (screening effect)
    /// 
    /// Key Parameters:
    /// - StickingProbability: Chance that a boundary cell sticks to form sink
    /// - DiffusionBias: Directional bias in "particle" movement (0 = isotropic)
    /// </summary>
    public class DLAModel : ExpansionModelBase
    {
        public override ExpansionModel ModelType => ExpansionModel.DLA;
        
        public override string DisplayName => "Diffusion-Limited Aggregation";
        
        public override string Description => 
            "DLA model: Particles diffuse and stick on contact with clusters. " +
            "Creates fractal, snowflake-like patterns. Tips grow faster.";
        
        public override string Category => "Fluid Dynamics";

        /// <summary>
        /// DLA formation: boundary cells adjacent to sinks can "stick".
        /// Probability modulated by number of adjacent sinks (tips grow faster).
        /// </summary>
        public override bool ShouldFormSink(int cellIdx, int x, int y, GridState state, StepContext context, float currentCharge)
        {
            if (context.Tick < Config.FormationDelayTicks)
                return false;
            
            // Count adjacent sinks (DLA is about contact)
            int adjacentSinks = CountAdjacentSinks(x, y, state);
            
            // Must be adjacent to at least one sink
            if (adjacentSinks == 0)
                return false;
            
            // DLA key insight: cells with FEWER adjacent sinks (tips) are more likely to grow
            // This creates the fractal branching pattern
            // More exposed = higher screening = higher probability
            double tipBonus = 1.0 + (4 - adjacentSinks) * 0.3; // Bonus for tips
            
            // Sticking probability
            double stickProb = Config.StickingProbability * tipBonus;
            
            // Also consider charge (represents "particle density")
            stickProb *= Math.Min(1.0, currentCharge / context.Config.SinkFormationThreshold);
            
            return context.Rng.NextDouble() < stickProb;
        }

        /// <summary>
        /// DLA expansion: similar to formation, favors tips.
        /// </summary>
        public override bool ShouldExpandSink(int cellIdx, int x, int y, GridState state, StepContext context, int sinkNeighborCount, int totalNeighborCount)
        {
            if (state.ActiveRegion[cellIdx])
                return false;

            // Must have at least one sink neighbor
            if (sinkNeighborCount == 0)
                return false;
            
            // Tip bonus: fewer neighbors = more likely to grow (screening effect)
            double tipBonus = 1.0 + (totalNeighborCount - sinkNeighborCount) * 0.2;
            double expandProb = Config.StickingProbability * 0.5 * tipBonus;
            
            return context.Rng.NextDouble() < expandProb;
        }

        /// <summary>
        /// Count adjacent sink cells.
        /// </summary>
        private int CountAdjacentSinks(int x, int y, GridState state)
        {
            int count = 0;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };
            
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= state.W || ny < 0 || ny >= state.H)
                    continue;
                if (state.IsSink[state.Idx(nx, ny)])
                    count++;
            }
            
            return count;
        }

        /// <summary>
        /// DLA rate is relatively constant - growth is limited by diffusion, not rate.
        /// </summary>
        public override double GetFormationRate(int tick, int totalCells, int currentSinkCount)
        {
            if (tick < Config.FormationDelayTicks)
                return 0.0;

            // DLA has constant rate - the fractal pattern emerges from the
            // sticking probability and screening effect, not rate modulation
            return Config.BaseFormationRate;
        }

        public override ExpansionConfig GetDefaultConfig()
        {
            return new ExpansionConfig
            {
                Model = ExpansionModel.DLA,
                BaseFormationRate = 0.1,
                FormationDelayTicks = 10,
                AllowMerging = true,
                AllowShrinkage = false,
                StickingProbability = 0.8, // High sticking for classic DLA
                DiffusionBias = 0.0        // Isotropic
            };
        }
    }
}
