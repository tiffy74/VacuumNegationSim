using System;
using Viable.Contracts;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.ExpansionModels.Biological
{
    /// <summary>
    /// Logistic Growth Expansion Model.
    /// 
    /// Paradigm:
    /// - Growth rate depends on current population relative to carrying capacity
    /// - Models tumor growth, bacterial colonies, population dynamics
    /// - S-curve: slow start, rapid growth, saturation at capacity
    /// 
    /// Key Parameters:
    /// - LogisticGrowthRate: Intrinsic growth rate r
    /// - CarryingCapacity: Maximum sink density K (as fraction of total cells)
    /// </summary>
    public class LogisticGrowthModel : ExpansionModelBase
    {
        public override ExpansionModel ModelType => ExpansionModel.LogisticGrowth;
        
        public override string DisplayName => "Logistic Growth";
        
        public override string Description => 
            "Biological logistic growth model: Growth rate slows as density approaches " +
            "carrying capacity. Models tumor growth, bacterial colonies.";
        
        public override string Category => "Biological";

        /// <summary>
        /// Sink formation probability scales with logistic growth rate.
        /// Near carrying capacity, formation becomes very unlikely.
        /// </summary>
        public override bool ShouldFormSink(int cellIdx, int x, int y, GridState state, StepContext context, float currentCharge)
        {
            if (context.Tick < Config.FormationDelayTicks)
                return false;
            
            // Get density-dependent rate
            int sinkCount = CountSinks(state);
            double rate = GetFormationRate(context.Tick, state.Len, sinkCount);
            
            // If at carrying capacity, no new sinks
            if (rate <= 0.01)
                return false;
            
            // Threshold inversely proportional to growth rate
            // High rate = easier formation, low rate = harder formation
            float effectiveThreshold = (float)(context.Config.SinkFormationThreshold / Math.Max(0.1, rate * 5));
            
            return currentCharge >= effectiveThreshold;
        }

        /// <summary>
        /// Expansion also follows logistic dynamics.
        /// Near capacity, even expansion is suppressed.
        /// </summary>
        public override bool ShouldExpandSink(int cellIdx, int x, int y, GridState state, StepContext context, int sinkNeighborCount, int totalNeighborCount)
        {
            if (state.ActiveRegion[cellIdx])
                return false;

            // Check carrying capacity
            int sinkCount = CountSinks(state);
            double density = (double)sinkCount / state.Len;
            
            // Near capacity, expansion stops
            if (density >= Config.CarryingCapacity * 0.95)
                return false;
            
            // Dynamic threshold based on how far from capacity
            double capacityRatio = density / Config.CarryingCapacity;
            int baseThreshold = (totalNeighborCount / 2) + 1;
            
            // Lower threshold when far from capacity (early growth)
            // Higher threshold when near capacity (saturation)
            int threshold = capacityRatio < 0.5 
                ? Math.Max(1, baseThreshold - 1)
                : baseThreshold + (int)(capacityRatio * 2);
            
            return sinkNeighborCount >= threshold;
        }

        /// <summary>
        /// Logistic growth rate: r * N * (1 - N/K)
        /// </summary>
        public override double GetFormationRate(int tick, int totalCells, int currentSinkCount)
        {
            if (tick < Config.FormationDelayTicks)
                return 0.0;

            double density = (double)currentSinkCount / totalCells;
            double K = Config.CarryingCapacity;
            double r = Config.LogisticGrowthRate;
            
            // At or above carrying capacity, no growth
            if (density >= K)
                return 0.0;
            
            // Logistic equation: dN/dt = r * N * (1 - N/K)
            // For rate, we use: r * (1 - N/K) scaled by base rate
            double logisticFactor = 1.0 - (density / K);
            
            // Add small base rate for initial seeding
            double baseRate = currentSinkCount == 0 ? 0.1 : density;
            
            return Config.BaseFormationRate * r * baseRate * logisticFactor;
        }

        /// <summary>
        /// Phase based on position on S-curve.
        /// </summary>
        public override ExpansionPhase GetCurrentPhase(int tick)
        {
            if (tick < Config.FormationDelayTicks)
                return ExpansionPhase.Initialization;
            
            // Phase determined by density, not time
            // Return MainExpansion since we can't know density here
            return ExpansionPhase.MainExpansion;
        }

        public override ExpansionConfig GetDefaultConfig()
        {
            return new ExpansionConfig
            {
                Model = ExpansionModel.LogisticGrowth,
                BaseFormationRate = 0.2,
                FormationDelayTicks = 10,
                AllowMerging = true,
                AllowShrinkage = false,
                LogisticGrowthRate = 0.5,
                CarryingCapacity = 0.3 // 30% of grid can be sinks
            };
        }
    }
}
