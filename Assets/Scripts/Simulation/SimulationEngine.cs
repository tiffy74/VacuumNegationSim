using Assets.Scripts.Domain;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Simulation
{
    /// <summary>
    /// Simulation Engine - Tick Orchestrator
    /// 
    /// Coordinates the execution of simulation steps in a fixed sequence each tick.
    /// Implements the Template Method pattern: defines the skeleton of tick execution,
    /// delegates implementation to pluggable ISimStep implementations.
    /// 
    /// Design Philosophy:
    /// - Minimal Logic: Engine only coordinates, doesn't implement physics
    /// - Step-Agnostic: Works with any ISimStep implementation
    /// - Fixed Sequence: Steps execute in constructor-defined order
    /// - Single Responsibility: Only manages tick lifecycle
    /// - Zero Allocation: Reuses step list, no per-tick allocation
    /// 
    /// Execution Model:
    ///   1. Increment tick counter (ctx.Tick++)
    ///   2. Execute each step in sequence: step.Execute(state, ctx)
    ///   3. Steps share same GridState reference (modified in-place)
    ///   4. Steps share same SimContext reference (mutable global state)
    /// 
    /// Thread Safety: NOT thread-safe (designed for single-threaded Unity loop)
    /// </summary>
    public sealed class SimulationEngine
    {
        // ============================================================================
        // PRIVATE FIELDS: STEP SEQUENCE
        // ============================================================================

        /// <summary>
        /// Ordered sequence of simulation steps to execute each tick.
        /// Immutable after construction (list contents never change).
        /// Typical sequence: [LegacyTickStep] (wraps Pass1-4 + post-processing)
        /// Future: Could have multiple steps for different physics layers.
        /// </summary>
        private readonly List<ISimStep> _steps;

        // ============================================================================
        // PUBLIC PROPERTIES: STATE ACCESS
        // ============================================================================

        /// <summary>
        /// Reference to simulation grid state.
        /// Exposed as public readonly property for diagnostic/debugging access.
        /// Modification should only occur through step execution, not external access.
        /// </summary>
        public GridState State { get; }

        // ============================================================================
        // CONSTRUCTOR
        // ============================================================================

        /// <summary>
        /// Initializes simulation engine with state and step sequence.
        /// Steps are executed in the order provided.
        /// </summary>
        /// <param name="state">Grid state to operate on (modified in-place by steps)</param>
        /// <param name="steps">Ordered sequence of steps to execute each tick</param>
        /// <exception cref="ArgumentNullException">If state or steps is null</exception>
        public SimulationEngine(GridState state, IEnumerable<ISimStep> steps)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            
            if (steps == null)
                throw new ArgumentNullException(nameof(steps));

            _steps = new List<ISimStep>(steps);
            
            ValidateStepSequence();
        }

        // ============================================================================
        // PUBLIC API: TICK EXECUTION
        // ============================================================================

        /// <summary>
        /// Executes one complete simulation tick.
        /// 
        /// Execution Sequence:
        /// 1. Log tick start (every 10 ticks for performance)
        /// 2. Execute each step in order: step.Execute(State, ctx)
        /// 3. Increment context tick counter
        /// 
        /// Note: Tick counter incremented AFTER steps execute, so steps see
        ///       the current tick number, not the next one.
        /// 
        /// Performance: O(steps.Count) + O(step execution time)
        ///              Typically O(1) steps + O(N) execution = O(N) total
        /// </summary>
        /// <param name="ctx">Simulation context (tick counter, global energy, config)</param>
        /// <exception cref="ArgumentNullException">If ctx is null</exception>
        public void Tick(SimContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            LogTickStart(ctx);
            ExecuteAllSteps(ctx);
            IncrementTickCounter(ctx);
        }

        // ============================================================================
        // PRIVATE METHODS: TICK EXECUTION
        // ============================================================================

        /// <summary>
        /// Logs tick start event for debugging/monitoring.
        /// Only logs every 10 ticks to reduce performance impact.
        /// </summary>
        private void LogTickStart(SimContext ctx)
        {
            if (ctx.Tick % 10 == 0)
            {
                UnityEngine.Debug.Log($"[SimulationEngine] Tick {ctx.Tick} starting with {_steps.Count} steps");
            }
        }

        /// <summary>
        /// Executes all steps in sequence.
        /// Steps share same state and context references.
        /// </summary>
        private void ExecuteAllSteps(SimContext ctx)
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                ExecuteSingleStep(_steps[i], ctx, i);
            }
        }

        /// <summary>
        /// Executes a single step with error handling.
        /// Logs execution for diagnostic purposes.
        /// </summary>
        private void ExecuteSingleStep(ISimStep step, SimContext ctx, int stepIndex)
        {
            try
            {
                step.Execute(State, ctx);
            }
            catch (Exception ex)
            {
                LogStepExecutionError(step, stepIndex, ctx.Tick, ex);
                throw; // Re-throw to halt simulation on critical error
            }
        }

        /// <summary>
        /// Increments the context tick counter.
        /// Called after all steps complete to prepare for next tick.
        /// </summary>
        private void IncrementTickCounter(SimContext ctx)
        {
            ctx.Tick++;
        }

        // ============================================================================
        // PRIVATE METHODS: VALIDATION
        // ============================================================================

        /// <summary>
        /// Validates that step sequence is not empty and contains no null steps.
        /// </summary>
        private void ValidateStepSequence()
        {
            if (_steps.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[SimulationEngine] Initialized with zero steps - engine will do nothing");
            }

            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i] == null)
                {
                    throw new ArgumentException($"Step at index {i} is null", nameof(_steps));
                }
            }
        }

        // ============================================================================
        // PRIVATE METHODS: DIAGNOSTICS & LOGGING
        // ============================================================================

        /// <summary>
        /// Logs detailed error information when step execution fails.
        /// </summary>
        private void LogStepExecutionError(ISimStep step, int stepIndex, int tick, Exception ex)
        {
            string stepTypeName = step?.GetType().Name ?? "Unknown";
            UnityEngine.Debug.LogError(
                $"[SimulationEngine] Step execution failed at tick {tick}:\n" +
                $"  Step: {stepTypeName} (index {stepIndex})\n" +
                $"  Error: {ex.Message}\n" +
                $"  Stack: {ex.StackTrace}");
        }

        // ============================================================================
        // PUBLIC METHODS: DIAGNOSTIC QUERIES
        // ============================================================================

        /// <summary>
        /// Gets the number of steps in the execution sequence.
        /// Useful for diagnostic/monitoring purposes.
        /// </summary>
        public int GetStepCount() => _steps.Count;

        /// <summary>
        /// Gets a read-only view of the step sequence for inspection.
        /// Used for debugging and verification.
        /// </summary>
        public IReadOnlyList<ISimStep> GetSteps() => _steps.AsReadOnly();

        /// <summary>
        /// Gets a summary string describing the engine state.
        /// Useful for debug logging and diagnostics.
        /// </summary>
        public override string ToString()
        {
            return $"SimulationEngine[Steps={_steps.Count}, GridSize={State.W}×{State.H}]";
        }
    }
}
