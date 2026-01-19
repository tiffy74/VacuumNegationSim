using System.Collections.Generic;

namespace Viable.Contracts
{
    /// <summary>
    /// Discrete event that occurred during simulation execution.
    /// </summary>
    public sealed class SimulationEvent
    {
        /// <summary>
        /// Event type identifier (e.g., "SinkFormed", "RegionExpanded", "ThresholdCrossed").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Step index when event occurred.
        /// </summary>
        public int StepIndex { get; set; }

        /// <summary>
        /// Simulated time when event occurred.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Event-specific data payload (structure depends on event type).
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Optional severity level (Info, Warning, Critical).
        /// </summary>
        public string Severity { get; set; }
    }
}
