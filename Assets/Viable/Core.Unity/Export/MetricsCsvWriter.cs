using System.Collections.Generic;
using System.IO;
using System.Text;
using Viable.Contracts;

namespace Viable.Core.Unity.Export
{
    /// <summary>
    /// Writes metrics.csv with time-series simulation metrics.
    /// Stage 9: CSV export for analysis in Excel, Python, R, etc.
    /// </summary>
    public class MetricsCsvWriter
    {
        /// <summary>
        /// Write metrics CSV file.
        /// </summary>
        public void Write(string csvPath, List<StateSample> samples)
        {
            if (samples == null || samples.Count == 0)
                return;

            var sb = new StringBuilder();

            // Header
            sb.AppendLine("Tick,Time,ViableCount,ActiveCount,SinkCount,AvgResource,AvgComplexity,ResourceGlobal");

            // Data rows
            foreach (var sample in samples)
            {
                if (sample.Metrics == null) continue;

                int tick = sample.StepIndex;
                double time = sample.Time;

                // Extract metrics (with defaults if missing)
                double viableCount = GetMetric(sample.Metrics, "viableCount", 0);
                double activeCount = GetMetric(sample.Metrics, "activeCount", 0);
                double sinkCount = GetMetric(sample.Metrics, "sinkCount", 0);
                double avgResource = GetMetric(sample.Metrics, "avgResource", 0);
                double avgComplexity = GetMetric(sample.Metrics, "avgComplexity", 0);
                double resourceGlobal = GetMetric(sample.Metrics, "resourceGlobal", 0);

                // Format: Tick,Time,ViableCount,ActiveCount,SinkCount,AvgResource,AvgComplexity,ResourceGlobal
                sb.AppendLine($"{tick},{time:F1},{viableCount:F0},{activeCount:F0},{sinkCount:F0},{avgResource:F2},{avgComplexity:F2},{resourceGlobal:E2}");
            }

            File.WriteAllText(csvPath, sb.ToString());
        }

        /// <summary>
        /// Get metric value from dictionary with fallback.
        /// </summary>
        private double GetMetric(Dictionary<string, double> metrics, string key, double defaultValue)
        {
            return metrics.TryGetValue(key, out double value) ? value : defaultValue;
        }
    }
}
