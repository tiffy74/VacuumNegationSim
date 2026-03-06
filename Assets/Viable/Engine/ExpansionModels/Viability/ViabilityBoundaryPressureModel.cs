using Viable.Contracts;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.ExpansionModels.Viability
{
    /// <summary>
    /// Viability Boundary Pressure Expansion Model.
    /// 
    /// THE ORIGINAL VIABLE MODEL - expansion emerges from viability dynamics alone.
    /// 
    /// Paradigm:
    /// - No external driving force (unlike inflation, dark energy)
    /// - Sinks form at boundaries when resource pressure accumulates
    /// - Expansion is a consequence of local viability constraints
    /// - System self-organizes based on resource flow and viability thresholds
    /// 
    /// This model demonstrates that complex spatial patterns can emerge
    /// from simple local viability rules without invoking external forces.
    /// </summary>
    public class ViabilityBoundaryPressureModel : ExpansionModelBase
    {
        public override ExpansionModel ModelType => ExpansionModel.DefaultViabilityBoundaryPressure;
        
        public override string DisplayName => "Default: Viability Boundary Pressure";
        
        public override string Description => 
            "Original Viable model: Sinks form at boundaries when resource pressure " +
            "exceeds threshold. Expansion emerges from viability dynamics alone, " +
            "without external driving forces.";
        
        public override string Category => "Viability-Constraint";

        /// <summary>
        /// Sink forms when accumulated charge at boundary exceeds threshold.
        /// This is the core mechanism: resource "pressure" at inactive boundaries
        /// creates sinks when it reaches critical value.
        /// </summary>
        public override bool ShouldFormSink(int cellIdx, int x, int y, GridState state, StepContext context, float currentCharge)
        {
            // Respect formation delay (protects seed region)
            if (context.Tick < Config.FormationDelayTicks)
                return false;

            // Core viability boundary pressure rule:
            // Sink forms when charge exceeds threshold
            float threshold = context.Config.SinkFormationThreshold;
            return currentCharge >= threshold;
        }

        /// <summary>
        /// Sink expands when majority of neighbors are sinks.
        /// Simple geometric expansion - fills gaps in sink regions.
        /// </summary>
        public override bool ShouldExpandSink(int cellIdx, int x, int y, GridState state, StepContext context, int sinkNeighborCount, int totalNeighborCount)
        {
            // Only expand into non-active regions
            if (state.ActiveRegion[cellIdx])
                return false;

            // Majority rule: more than half neighbors must be sinks
            int threshold = (totalNeighborCount / 2) + 1;
            return sinkNeighborCount >= threshold;
        }

        /// <summary>
        /// In the original model, sinks never shrink.
        /// Once formed, they are permanent attractors.
        /// </summary>
        public override bool ShouldShrinkSink(int cellIdx, int x, int y, GridState state, StepContext context)
        {
            return false; // Sinks are permanent in this model
        }

        /// <summary>
        /// Formation rate is constant in the viability model.
        /// The actual formation is driven by charge accumulation, not rate.
        /// </summary>
        public override double GetFormationRate(int tick, int totalCells, int currentSinkCount)
        {
            if (tick < Config.FormationDelayTicks)
                return 0.0;

            // Constant base rate - actual formation determined by charge threshold
            return Config.BaseFormationRate;
        }

        /// <summary>
        /// The viability model has a simple two-phase structure:
        /// - Initialization: Seed region establishes
        /// - MainExpansion: System evolves based on viability dynamics
        /// </summary>
        public override ExpansionPhase GetCurrentPhase(int tick)
        {
            if (tick < Config.FormationDelayTicks)
                return ExpansionPhase.Initialization;

            // Could add detection of equilibrium based on sink/region counts
            return ExpansionPhase.MainExpansion;
        }

        public override ExpansionConfig GetDefaultConfig()
        {
            return new ExpansionConfig
            {
                Model = ExpansionModel.DefaultViabilityBoundaryPressure,
                BaseFormationRate = 0.1,
                FormationDelayTicks = 10,
                AllowMerging = true,
                AllowShrinkage = false
            };
        }
    }
}
