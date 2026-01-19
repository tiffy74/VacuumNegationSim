using System.Collections.Generic;

namespace Viable.Contracts
{
    /// <summary>
    /// Request to run a simulation for a specified scenario.
    /// </summary>
    public sealed class RunRequest
    {
        /// <summary>
        /// Scenario identifier to execute.
        /// </summary>
        public string ScenarioId { get; set; } = "default";

        /// <summary>
        /// Number of discrete steps to execute.
        /// </summary>
        public int Steps { get; set; } = 100;

        /// <summary>
        /// Optional duration in simulated time units (alternative to steps).
        /// </summary>
        public double? Duration { get; set; }

        /// <summary>
        /// Sample state every N steps (1 = every step, 10 = every 10th step).
        /// </summary>
        public int SampleEvery { get; set; } = 1;

        /// <summary>
        /// Parameter overrides for this specific run (merged with scenario defaults).
        /// </summary>
        public Dictionary<string, double> Overrides { get; set; }

        /// <summary>
        /// Stop conditions (e.g., "viabilityBelowThreshold", "allCellsInactive").
        /// </summary>
        public string[] StopConditions { get; set; }

        /// <summary>
        /// Whether to emit detailed events (may impact performance).
        /// </summary>
        public bool EmitEvents { get; set; } = true;

        /// <summary>
        /// Random seed override (overrides scenario seed if provided).
        /// </summary>
        public int? Seed { get; set; }
    }
}
