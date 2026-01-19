using System.Collections.Generic;

namespace Viable.Contracts
{
    /// <summary>
    /// State snapshot at a specific time point with associated metrics.
    /// </summary>
    public sealed class StateSample
    {
        /// <summary>
        /// Step index when this sample was taken.
        /// </summary>
        public int StepIndex { get; set; }

        /// <summary>
        /// Simulated time of this sample.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Full state snapshot (serialized as generic object for now).
        /// Will be strongly typed once state model is extracted.
        /// </summary>
        public object State { get; set; }

        /// <summary>
        /// Computed metrics at this time point (e.g., "viableCount", "avgResource").
        /// </summary>
        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();
    }
}
