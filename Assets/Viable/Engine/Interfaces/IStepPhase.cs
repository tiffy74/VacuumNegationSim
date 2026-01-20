using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine.Interfaces
{
    /// <summary>
    /// Interface for individual simulation step phases.
    /// Each phase operates on GridState and StepContext to perform one logical operation.
    /// </summary>
    public interface IStepPhase
    {
        /// <summary>
        /// Execute this phase's logic on the given state and context.
        /// </summary>
        /// <param name="state">Current grid state (will be mutated)</param>
        /// <param name="context">Execution context (tick, config, RNG, etc.)</param>
        void Execute(GridState state, StepContext context);
    }
}
