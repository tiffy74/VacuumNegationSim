using System;
using System.IO;
using System.Text;
using Viable.Contracts;

namespace Viable.Core.Unity.Export
{
    /// <summary>
    /// Writes summary.md with human-readable simulation report.
    /// Stage 9: Markdown summary for documentation.
    /// </summary>
    public class SummaryWriter
    {
        /// <summary>
        /// Write summary markdown file.
        /// </summary>
        public void Write(string summaryPath, ScenarioDefinition scenario, RunRequest request, RunResult result)
        {
            var sb = new StringBuilder();

            // Title
            sb.AppendLine($"# Viable Engine Run Summary");
            sb.AppendLine();
            sb.AppendLine($"**Run ID:** `{result.RunId}`");
            sb.AppendLine($"**Exported:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();

            // Scenario
            sb.AppendLine("## Scenario");
            sb.AppendLine();
            sb.AppendLine($"**Preset:** {scenario.ScenarioName} (`{scenario.ScenarioId}`)");
            if (!string.IsNullOrEmpty(scenario.Description))
            {
                sb.AppendLine();
                sb.AppendLine($"**Description:**");
                sb.AppendLine($"> {scenario.Description}");
            }
            sb.AppendLine();
            sb.AppendLine($"**Grid Size:** {scenario.GridWidth}×{scenario.GridHeight}");
            sb.AppendLine($"**Seed:** {scenario.Seed?.ToString() ?? "None (non-deterministic)"}");
            sb.AppendLine();

            // Execution
            sb.AppendLine("## Execution");
            sb.AppendLine();
            sb.AppendLine($"**Steps:** {result.StepsExecuted}");
            sb.AppendLine($"**Duration:** {result.ExecutionTimeMs:F2} ms");
            sb.AppendLine($"**Performance:** {(result.StepsExecuted / (result.ExecutionTimeMs / 1000.0)):F1} steps/sec");
            sb.AppendLine();

            // Results
            if (result.SummaryMetrics != null && result.SummaryMetrics.Count > 0)
            {
                sb.AppendLine("## Final Metrics");
                sb.AppendLine();
                sb.AppendLine("| Metric | Value |");
                sb.AppendLine("|--------|-------|");

                foreach (var kvp in result.SummaryMetrics)
                {
                    string key = kvp.Key;
                    string value = FormatMetricValue(kvp.Value);
                    sb.AppendLine($"| {key} | {value} |");
                }
                sb.AppendLine();
            }

            // Samples
            if (result.Samples != null && result.Samples.Count > 0)
            {
                sb.AppendLine("## Data");
                sb.AppendLine();
                sb.AppendLine($"**Samples Collected:** {result.Samples.Count}");
                sb.AppendLine($"**Sample Interval:** Every {request?.SampleEvery ?? 1} steps");
                sb.AppendLine();
                
                // Stage 13.2: Value semantics legend
                if (result.ValueSemantics != null && result.ValueSemantics.Count > 0)
                {
                    sb.AppendLine("### Value Semantics");
                    sb.AppendLine();
                    sb.AppendLine("Metric suffixes in `metrics.csv` indicate value types:");
                    sb.AppendLine();
                    sb.AppendLine("- `_Q` = Quantity (abstract resource units)");
                    sb.AppendLine("- `_Q_per_step` = Rate or Cost (quantity per simulation step)");
                    sb.AppendLine("- `_index` = Index (dimensionless, normalized metric)");
                    sb.AppendLine("- `_count` = Count (integer quantity)");
                    sb.AppendLine();
                    sb.AppendLine("**Note:** Q is an abstract quantity unit without physical dimensions.");
                    sb.AppendLine("Time basis is per simulation step (not physical time).");
                    sb.AppendLine();
                }
            }

            // Events
            if (result.Events != null && result.Events.Count > 0)
            {
                sb.AppendLine($"**Events Recorded:** {result.Events.Count}");
                sb.AppendLine();
            }

            // Files
            sb.AppendLine("## Exported Files");
            sb.AppendLine();
            sb.AppendLine("- `scenario.json` - Scenario definition");
            if (request != null)
                sb.AppendLine("- `request.json` - Run request");
            if (result.Samples != null && result.Samples.Count > 0)
                sb.AppendLine("- `metrics.csv` - Time-series metrics");
            if (result.Events != null && result.Events.Count > 0)
                sb.AppendLine("- `events.csv` - Simulation events");
            sb.AppendLine("- `manifest.json` - Run metadata");
            sb.AppendLine("- `checksums.txt` - SHA-256 file hashes");
            sb.AppendLine();

            // Reproducibility
            if (scenario.Seed.HasValue)
            {
                sb.AppendLine("## Reproducibility");
                sb.AppendLine();
                sb.AppendLine($"This run used **seed {scenario.Seed.Value}** for deterministic execution.");
                sb.AppendLine("To reproduce these results:");
                sb.AppendLine();
                sb.AppendLine("1. Load the same preset: `" + scenario.ScenarioId + "`");
                sb.AppendLine($"2. Use the same seed: `{scenario.Seed.Value}`");
                sb.AppendLine($"3. Run for {result.StepsExecuted} steps");
                sb.AppendLine();
                sb.AppendLine("The checksums in `checksums.txt` can be used to verify identical output.");
                sb.AppendLine();
            }

            // Footer
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine($"*Generated by Viable Engine {result.Metadata?.EngineVersion ?? "1.0.0"}*");

            File.WriteAllText(summaryPath, sb.ToString());
        }

        /// <summary>
        /// Format metric value for display.
        /// </summary>
        private string FormatMetricValue(double value)
        {
            // Use scientific notation for very large/small numbers
            if (Math.Abs(value) >= 1e6 || (Math.Abs(value) < 0.01 && value != 0))
                return $"{value:E2}";

            // Use decimal for normal numbers
            if (value == (int)value)
                return $"{value:F0}";

            return $"{value:F2}";
        }
    }
}
