using System.Collections.Generic;

namespace Viable.Contracts
{
    /// <summary>
    /// Complete result of a simulation run, including metadata, events, samples, and final state.
    /// </summary>
    public sealed class RunResult
    {
        /// <summary>
        /// Unique identifier for this run (GUID).
        /// Stage 9: Added for export system.
        /// </summary>
        public string RunId { get; set; } = System.Guid.NewGuid().ToString();

        /// <summary>
        /// Engine metadata for version tracking.
        /// </summary>
        public EngineMetadata Metadata { get; set; } = EngineMetadata.Current();

        /// <summary>
        /// Scenario identifier that was executed.
        /// </summary>
        public string ScenarioId { get; set; } = string.Empty;

        /// <summary>
        /// Actual number of steps executed (may differ from request if stopped early).
        /// </summary>
        public int StepsExecuted { get; set; }

        /// <summary>
        /// Final simulated time reached.
        /// </summary>
        public double FinalTime { get; set; }

        /// <summary>
        /// Ordered list of events that occurred during execution.
        /// </summary>
        public List<SimulationEvent> Events { get; set; } = new List<SimulationEvent>();

        /// <summary>
        /// State samples taken at specified intervals.
        /// </summary>
        public List<StateSample> Samples { get; set; } = new List<StateSample>();

        /// <summary>
        /// Final state at end of execution (serialized as generic object for now).
        /// </summary>
        public object FinalState { get; set; }

        /// <summary>
        /// Summary metrics computed over entire run (e.g., "avgViability", "sinkCount").
        /// </summary>
        public Dictionary<string, double> SummaryMetrics { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Optional stop reason if execution terminated early.
        /// </summary>
        public string StopReason { get; set; }

        /// <summary>
        /// Execution duration in real time (milliseconds).
        /// </summary>
        public double ExecutionTimeMs { get; set; }
    }
}
