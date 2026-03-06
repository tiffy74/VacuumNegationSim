using System;
using Viable.Contracts;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.ExpansionModels.StatisticalPhysics
{
    /// <summary>
    /// Percolation Expansion Model.
    /// 
    /// Paradigm:
    /// - Random site occupation with probability p
    /// - Phase transition at critical threshold pc ? 0.593 (square lattice)
    /// - Below pc: small isolated clusters
    /// - Above pc: spanning cluster forms
    /// 
    /// Key Parameters:
    /// - OccupationProbability: Site occupation probability p
    /// </summary>
    public class PercolationModel : ExpansionModelBase
    {
        public override ExpansionModel ModelType => ExpansionModel.Percolation;
        
        public override string DisplayName => "Percolation";
        
        public override string Description => 
            "Percolation model: Random site occupation with critical threshold. " +
            "Below threshold: isolated clusters. Above: spanning cluster.";
        
        public override string Category => "Statistical Physics";

        /// <summary>
        /// Percolation: each boundary cell has probability p of becoming sink.
        /// Independent of charge - purely probabilistic.
        /// </summary>
        public override bool ShouldFormSink(int cellIdx, int x, int y, GridState state, StepContext context, float currentCharge)
        {
            if (context.Tick < Config.FormationDelayTicks)
                return false;
            
            // Must have some charge (be at boundary)
            if (currentCharge <= 0)
                return false;
            
            // Pure random occupation
            return context.Rng.NextDouble() < Config.OccupationProbability;
        }

        /// <summary>
        /// Percolation expansion: probabilistic growth at boundaries.
        /// </summary>
        public override bool ShouldExpandSink(int cellIdx, int x, int y, GridState state, StepContext context, int sinkNeighborCount, int totalNeighborCount)
        {
            if (state.ActiveRegion[cellIdx])
                return false;

            // Must be adjacent to cluster
            if (sinkNeighborCount == 0)
                return false;
            
            // Each adjacent site has occupation probability
            // Combined probability that at least one "particle" percolates here
            double probNone = Math.Pow(1.0 - Config.OccupationProbability * 0.3, sinkNeighborCount);
            double probAtLeastOne = 1.0 - probNone;
            
            return context.Rng.NextDouble() < probAtLeastOne;
        }

        /// <summary>
        /// Percolation rate is constant - it's the occupation probability that matters.
        /// </summary>
        public override double GetFormationRate(int tick, int totalCells, int currentSinkCount)
        {
            if (tick < Config.FormationDelayTicks)
                return 0.0;

            return Config.OccupationProbability;
        }

        public override ExpansionConfig GetDefaultConfig()
        {
            return new ExpansionConfig
            {
                Model = ExpansionModel.Percolation,
                BaseFormationRate = 0.1,
                FormationDelayTicks = 10,
                AllowMerging = true,
                AllowShrinkage = false,
                OccupationProbability = 0.5 // Just below critical threshold
            };
        }
    }
}
