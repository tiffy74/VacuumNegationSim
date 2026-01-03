using Assets.Scripts.Domain;

namespace Assets.Scripts.Simulation
{
    /// <summary>
    /// Simulation Step Interface
    /// 
    /// Defines the contract for any simulation logic that operates on grid state.
    /// Implements the Strategy Pattern, allowing different simulation algorithms
    /// to be swapped at runtime without modifying the engine.
    /// 
    /// Design Principles:
    /// - Stateless: Steps should not store simulation data as instance fields
    /// - Side-Effect Only: Modifies state in-place, no return value needed
    /// - Pure Logic: No Unity dependencies, testable in isolation
    /// - Composable: Multiple steps can be chained in sequence
    /// 
    /// Example Implementations:
    /// - LegacyTickStep: Original 4-pass simulation logic
    /// - ParallelTickStep: Multi-threaded pass execution (future)
    /// - DebugTickStep: Step-by-step execution with logging (future)
    /// - ReplayTickStep: Deterministic replay from recorded state (future)
    /// </summary>
    public interface ISimStep
    {
        /// <summary>
        /// Executes one simulation step, modifying the grid state in-place.
        /// 
        /// Contract Requirements:
        /// - Must NOT store simulation state in instance fields
        /// - Must modify state arrays directly (in-place for performance)
        /// - May read/write SimContext for global simulation parameters
        /// - Should be deterministic for same input state
        /// - Should NOT throw exceptions (handle errors gracefully)
        /// 
        /// Execution Context:
        /// - Called by SimulationEngine.Tick() in sequence with other steps
        /// - Shares same GridState reference with other steps
        /// - Context tick counter already incremented before execution
        /// - No assumptions about execution frequency (controlled by engine)
        /// 
        /// Performance Considerations:
        /// - Avoid allocation (use pre-allocated arrays in GridState)
        /// - Minimize logging (expensive in tight loops)
        /// - Consider cache locality (sequential array access preferred)
        /// </summary>
        /// <param name="state">Grid state to modify (shared reference, modified in-place)</param>
        /// <param name="ctx">Simulation context (tick counter, global energy, config)</param>
        void Execute(GridState state, SimContext ctx);
    }
}
