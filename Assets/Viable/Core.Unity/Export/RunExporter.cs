using System;
using System.IO;
using UnityEngine;
using Viable.Contracts;

namespace Viable.Core.Unity.Export
{
    /// <summary>
    /// Main orchestrator for exporting simulation runs.
    /// Creates export directories, coordinates writers, handles errors.
    /// Stage 9: Export system for reproducible research.
    /// </summary>
    public class RunExporter
    {
        private readonly ManifestWriter manifestWriter;
        private readonly MetricsCsvWriter metricsWriter;
        private readonly EventsCsvWriter eventsWriter;
        private readonly SummaryWriter summaryWriter;
        private readonly ChecksumWriter checksumWriter;

        public RunExporter()
        {
            manifestWriter = new ManifestWriter();
            metricsWriter = new MetricsCsvWriter();
            eventsWriter = new EventsCsvWriter();
            summaryWriter = new SummaryWriter();
            checksumWriter = new ChecksumWriter();
        }

        /// <summary>
        /// Export a simulation run to disk.
        /// </summary>
        /// <param name="scenario">Scenario definition</param>
        /// <param name="request">Run request</param>
        /// <param name="result">Run result</param>
        /// <param name="options">Export options</param>
        /// <returns>Path to export directory</returns>
        public string Export(ScenarioDefinition scenario, RunRequest request, RunResult result, RunExportOptions options)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (options == null) options = RunExportOptions.ForLevel(ExportLevel.Study);

            try
            {
                // Create export directory
                string exportPath = CreateExportDirectory(result, options);
                Debug.Log($"[RunExporter] Exporting to: {exportPath}");

                // Write scenario (simple JSON - just key fields)
                WriteScenarioJson(Path.Combine(exportPath, "scenario.json"), scenario);
                
                // Write request (simple JSON)
                if (request != null)
                    WriteRequestJson(Path.Combine(exportPath, "request.json"), request);

                // Skip result.json for now (GridState is too complex for JsonUtility)
                // Instead, we have metrics.csv and summary.md which are more useful

                // Write metrics CSV
                if (result.Samples != null && result.Samples.Count > 0)
                {
                    string metricsPath = Path.Combine(exportPath, "metrics.csv");
                    metricsWriter.Write(metricsPath, result.Samples);
                }

                // Write events CSV
                if (options.IncludeEvents && result.Events != null && result.Events.Count > 0)
                {
                    string eventsPath = Path.Combine(exportPath, "events.csv");
                    eventsWriter.Write(eventsPath, result.Events);
                }

                // Write summary markdown
                string summaryPath = Path.Combine(exportPath, "summary.md");
                summaryWriter.Write(summaryPath, scenario, request, result);

                // Write manifest (includes file list)
                string manifestPath = Path.Combine(exportPath, "manifest.json");
                manifestWriter.Write(manifestPath, exportPath, scenario, request, result);

                // Write checksums (must be last - computes hashes of all files)
                string checksumsPath = Path.Combine(exportPath, "checksums.txt");
                checksumWriter.Write(checksumsPath, exportPath);

                Debug.Log($"[RunExporter] Export complete: {exportPath}");
                return exportPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RunExporter] Export failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create timestamped export directory.
        /// Format: VIABLE_Run_{UTC_ISO}_{runIdShort}/
        /// </summary>
        private string CreateExportDirectory(RunResult result, RunExportOptions options)
        {
            // Base directory
            string baseDir = options.ExportDirectory;
            if (string.IsNullOrEmpty(baseDir))
                baseDir = Path.Combine(Application.persistentDataPath, "Exports");

            // Ensure base directory exists
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            // Generate folder name
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHHmmssZ");
            string runIdShort = string.IsNullOrEmpty(result.RunId) 
                ? "unknown" 
                : result.RunId.Substring(0, Math.Min(6, result.RunId.Length));
            string folderName = $"VIABLE_Run_{timestamp}_{runIdShort}";

            string exportPath = Path.Combine(baseDir, folderName);
            Directory.CreateDirectory(exportPath);

            return exportPath;
        }

        /// <summary>
        /// Write scenario to simple JSON (just key fields).
        /// </summary>
        private void WriteScenarioJson(string path, ScenarioDefinition scenario)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"ScenarioId\": \"{scenario.ScenarioId}\",");
            sb.AppendLine($"  \"ScenarioName\": \"{scenario.ScenarioName}\",");
            sb.AppendLine($"  \"Description\": \"{EscapeJson(scenario.Description)}\",");
            sb.AppendLine($"  \"GridWidth\": {scenario.GridWidth},");
            sb.AppendLine($"  \"GridHeight\": {scenario.GridHeight},");
            sb.AppendLine($"  \"Seed\": {(scenario.Seed.HasValue ? scenario.Seed.Value.ToString() : "null")}");
            sb.AppendLine("}");
            File.WriteAllText(path, sb.ToString());
        }

        /// <summary>
        /// Write request to simple JSON.
        /// </summary>
        private void WriteRequestJson(string path, RunRequest request)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"Steps\": {request.Steps},");
            sb.AppendLine($"  \"SampleEvery\": {request.SampleEvery},");
            sb.AppendLine($"  \"EmitEvents\": {request.EmitEvents.ToString().ToLower()}");
            sb.AppendLine("}");
            File.WriteAllText(path, sb.ToString());
        }

        /// <summary>
        /// Escape JSON string values.
        /// </summary>
        private string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
