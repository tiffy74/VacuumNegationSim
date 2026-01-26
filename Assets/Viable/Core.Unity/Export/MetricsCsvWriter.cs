using System.Collections.Generic;
using System.IO;
using System.Text;
using Viable.Contracts;

namespace Viable.Core.Unity.Export
{
    /// <summary>
    /// Writes metrics.csv with time-series simulation metrics.
    /// Stage 9: CSV export for analysis in Excel, Python, R, etc.
    /// Stage 13.2: Enhanced with value semantics for interpretable outputs.
    /// </summary>
    public class MetricsCsvWriter
    {
        /// <summary>
        /// Write metrics CSV file with semantic suffixes.
        /// Stage 13.2: Headers now include semantic type suffixes for clarity.
        /// </summary>
        public void Write(string csvPath, List<StateSample> samples, Dictionary<string, string> valueSemantics = null)
        {
            if (samples == null || samples.Count == 0)
                return;

            var sb = new StringBuilder();

            // Stage 13.2: Header with semantic suffixes
            // Format: MetricName_suffix where suffix indicates the semantic type
            sb.AppendLine("Tick,Time,ViableCount_count,ActiveCount_count,SinkCount_count,AvgResource_Q,AvgComplexity_index,ResourceGlobal_Q");

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

        /// <summary>
        /// Convert semantic type to suffix for CSV headers.
        /// Stage 13.2: Maps semantic types to concise suffixes.
        /// </summary>
        private string GetSemanticSuffix(string semanticType)
        {
            if (string.IsNullOrEmpty(semanticType))
                return "";

            if (semanticType.Contains("Quantity"))
                return "_Q";
            if (semanticType.Contains("Rate"))
                return "_Q_per_step";
            if (semanticType.Contains("Cost"))
                return "_Q_per_step";
            if (semanticType.Contains("Index"))
                return "_index";
            if (semanticType.Contains("Count"))
                return "_count";

            return "";
        }
    }
}
