using System.Collections.Generic;

namespace Viable.Contracts
{
    /// <summary>
    /// Discrete event that occurred during simulation execution.
    /// Stage 13.9: Added "RefinementCreated" event type conventions for future adaptive refinement.
    /// </summary>
    /// <remarks>
    /// Standard Event Types:
    /// 
    /// - "SinkFormed": Sink region created at boundary
    ///   Data: { "x": int, "y": int, "sinkId": int, "charge": double }
    /// 
    /// - "RegionExpanded": Active region grew into new cell
    ///   Data: { "x": int, "y": int, "fromX": int, "fromY": int }
    /// 
    /// - "RefinementCreated" (Stage 13.9 - Future):
    ///   Adaptive refinement created subgrid within a cell
    ///   Data: {
    ///     "parentId": int,          // ID of parent cell being refined
    ///     "x": int,                 // Parent cell X coordinate
    ///     "y": int,                 // Parent cell Y coordinate
    ///     "disorderIndex": double,  // Disorder metric that triggered refinement
    ///     "level": int,             // Refinement depth level (0 = base grid)
    ///     "subgridSide": int,       // Size of subgrid (e.g., 2×2, 4×4)
    ///     "lineageDepth": int       // Depth in refinement hierarchy
    ///   }
    /// </remarks>
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
