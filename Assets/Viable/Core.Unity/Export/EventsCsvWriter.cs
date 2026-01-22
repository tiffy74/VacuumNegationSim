using System.Collections.Generic;
using System.IO;
using System.Text;
using Viable.Contracts;

namespace Viable.Core.Unity.Export
{
    /// <summary>
    /// Writes events.csv with simulation events.
    /// Stage 9: Event log export for analysis.
    /// </summary>
    public class EventsCsvWriter
    {
        /// <summary>
        /// Write events CSV file.
        /// </summary>
        public void Write(string csvPath, List<SimulationEvent> events)
        {
            if (events == null || events.Count == 0)
                return;

            var sb = new StringBuilder();

            // Header
            sb.AppendLine("StepIndex,Time,EventType,Severity,DataSummary");

            // Data rows
            foreach (var evt in events)
            {
                int stepIndex = evt.StepIndex;
                double time = evt.Time;
                string eventType = EscapeCsv(evt.Type ?? "");
                string severity = EscapeCsv(evt.Severity ?? "");
                string dataSummary = evt.Data != null ? EscapeCsv(FormatData(evt.Data)) : "";

                sb.AppendLine($"{stepIndex},{time:F1},{eventType},{severity},{dataSummary}");
            }

            File.WriteAllText(csvPath, sb.ToString());
        }

        /// <summary>
        /// Format Data dictionary as a string summary.
        /// </summary>
        private string FormatData(Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                return "";

            var parts = new List<string>();
            foreach (var kvp in data)
            {
                parts.Add($"{kvp.Key}={kvp.Value}");
            }
            return string.Join("; ", parts);
        }

        /// <summary>
        /// Escape CSV field (handle commas, quotes, newlines).
        /// </summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // If contains comma, quote, or newline, wrap in quotes and escape internal quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                value = value.Replace("\"", "\"\""); // Escape quotes
                return $"\"{value}\"";
            }

            return value;
        }
    }
}
