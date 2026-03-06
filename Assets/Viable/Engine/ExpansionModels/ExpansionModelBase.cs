using System;
using Viable.Contracts;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.ExpansionModels
{
    /// <summary>
    /// Base class for expansion model plugins.
    /// Provides default implementations and common functionality.
    /// Subclasses only need to override what's specific to their model.
    /// </summary>
    public abstract class ExpansionModelBase : IExpansionModel
    {
        // ===== Configuration =====
        protected ExpansionConfig Config { get; private set; }

        // ===== Abstract Properties (must override) =====
        
        public abstract ExpansionModel ModelType { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public abstract string Category { get; }

        // ===== Configuration =====

        public virtual void Configure(ExpansionConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public virtual ExpansionConfig GetDefaultConfig()
        {
            return new ExpansionConfig
            {
                Model = ModelType,
                BaseFormationRate = 0.1,
                FormationDelayTicks = 10,
                AllowMerging = true,
                AllowShrinkage = false
            };
        }

        // ===== Core Logic (default implementations) =====

        /// <summary>
        /// Default sink formation: threshold-based.
        /// Override for model-specific formation rules.
        /// </summary>
        public virtual bool ShouldFormSink(int cellIdx, int x, int y, GridState state, StepContext context, float currentCharge)
        {
            // Don't form before delay ticks
            if (context.Tick < Config.FormationDelayTicks)
                return false;

            // Default: threshold-based formation
            return currentCharge >= context.Config.SinkFormationThreshold;
        }

        /// <summary>
        /// Default sink expansion: majority of neighbors are sinks.
        /// Override for model-specific expansion rules.
        /// </summary>
        public virtual bool ShouldExpandSink(int cellIdx, int x, int y, GridState state, StepContext context, int sinkNeighborCount, int totalNeighborCount)
        {
            // Default: majority rule
            int threshold = (totalNeighborCount / 2) + 1;
            return sinkNeighborCount >= threshold;
        }

        /// <summary>
        /// Default: sinks never shrink.
        /// Override for models that support contraction.
        /// </summary>
        public virtual bool ShouldShrinkSink(int cellIdx, int x, int y, GridState state, StepContext context)
        {
            // Default: no shrinkage
            return false;
        }

        // ===== Rate Calculations =====

        public virtual double GetFormationRate(int tick, int totalCells, int currentSinkCount)
        {
            if (tick < Config.FormationDelayTicks)
                return 0.0;

            return Config.BaseFormationRate;
        }

        public virtual ExpansionPhase GetCurrentPhase(int tick)
        {
            if (tick < Config.FormationDelayTicks)
                return ExpansionPhase.Initialization;

            return ExpansionPhase.MainExpansion;
        }

        // ===== Lifecycle Hooks (default: no-op) =====

        public virtual void OnTickStart(GridState state, StepContext context) { }
        public virtual void OnTickEnd(GridState state, StepContext context) { }
        public virtual void OnSinkFormed(int cellIdx, int x, int y, GridState state, StepContext context) { }
        public virtual void OnSinksMerged(int rootSinkId, int mergedSinkId, GridState state, StepContext context) { }

        // ===== Helper Methods for Subclasses =====

        /// <summary>
        /// Get deterministic random value for this cell and tick.
        /// Use for probabilistic decisions that must be reproducible.
        /// </summary>
        protected double GetDeterministicRandom(StepContext context)
        {
            return context.Rng.NextDouble();
        }

        /// <summary>
        /// Count cells matching a predicate.
        /// </summary>
        protected int CountCells(GridState state, Func<int, bool> predicate)
        {
            int count = 0;
            for (int i = 0; i < state.Len; i++)
            {
                if (predicate(i))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Count current sink cells.
        /// </summary>
        protected int CountSinks(GridState state)
        {
            return CountCells(state, i => state.IsSink[i]);
        }

        /// <summary>
        /// Count current active region cells.
        /// </summary>
        protected int CountActiveRegion(GridState state)
        {
            return CountCells(state, i => state.ActiveRegion[i]);
        }
    }
}
