using Viable.Contracts;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.ExpansionModels
{
    /// <summary>
    /// Interface for expansion model plugins.
    /// Each plugin encapsulates the logic for a specific expansion paradigm
    /// (viability-constraint, cosmological, biological, fluid dynamics, etc.)
    /// 
    /// Scientists can implement this interface to create custom expansion models
    /// without modifying core simulation code.
    /// </summary>
    public interface IExpansionModel
    {
        // ===== Identity =====
        
        /// <summary>
        /// The enum value identifying this model type.
        /// </summary>
        ExpansionModel ModelType { get; }

        /// <summary>
        /// Display name for UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Scientific description of the model's behavior.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Category for UI grouping (e.g., "Viability", "Cosmological", "Biological").
        /// </summary>
        string Category { get; }

        // ===== Configuration =====

        /// <summary>
        /// Configure the model with parameters from ExpansionConfig.
        /// Called once when the model is selected.
        /// </summary>
        void Configure(ExpansionConfig config);

        /// <summary>
        /// Get default configuration for this model.
        /// Used to populate UI with sensible defaults.
        /// </summary>
        ExpansionConfig GetDefaultConfig();

        // ===== Core Sink Formation Logic =====

        /// <summary>
        /// Determine if a sink should form at the given cell.
        /// Called each tick for eligible boundary cells.
        /// </summary>
        /// <param name="cellIdx">Cell index in flattened array</param>
        /// <param name="x">Cell X coordinate</param>
        /// <param name="y">Cell Y coordinate</param>
        /// <param name="state">Current grid state</param>
        /// <param name="context">Step context with config and RNG</param>
        /// <param name="currentCharge">Accumulated charge at this cell</param>
        /// <returns>True if sink should form</returns>
        bool ShouldFormSink(int cellIdx, int x, int y, GridState state, StepContext context, float currentCharge);

        /// <summary>
        /// Determine if an existing sink should expand to the given cell.
        /// Called for cells adjacent to existing sinks.
        /// </summary>
        bool ShouldExpandSink(int cellIdx, int x, int y, GridState state, StepContext context, int sinkNeighborCount, int totalNeighborCount);

        /// <summary>
        /// Determine if an existing sink cell should shrink/evaporate.
        /// Only called if AllowShrinkage is true in config.
        /// </summary>
        bool ShouldShrinkSink(int cellIdx, int x, int y, GridState state, StepContext context);

        // ===== Rate Calculations =====

        /// <summary>
        /// Get the effective formation rate at the current tick.
        /// Used for probabilistic formation decisions.
        /// </summary>
        double GetFormationRate(int tick, int totalCells, int currentSinkCount);

        /// <summary>
        /// Get the current expansion phase for multi-phase models.
        /// Used for diagnostics and visualization.
        /// </summary>
        ExpansionPhase GetCurrentPhase(int tick);

        // ===== Lifecycle Hooks =====

        /// <summary>
        /// Called once at the start of each simulation tick.
        /// Use for pre-calculations, field updates, etc.
        /// </summary>
        void OnTickStart(GridState state, StepContext context);

        /// <summary>
        /// Called once at the end of each simulation tick.
        /// Use for cleanup, statistics, etc.
        /// </summary>
        void OnTickEnd(GridState state, StepContext context);

        /// <summary>
        /// Called when a new sink is formed.
        /// Use for model-specific sink initialization.
        /// </summary>
        void OnSinkFormed(int cellIdx, int x, int y, GridState state, StepContext context);

        /// <summary>
        /// Called when sinks merge.
        /// Use for model-specific merge handling.
        /// </summary>
        void OnSinksMerged(int rootSinkId, int mergedSinkId, GridState state, StepContext context);
    }
}
