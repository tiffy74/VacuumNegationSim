using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Viable.Contracts;

namespace Viable.Core.Unity.Export
{
    /// <summary>
    /// Writes manifest.json with run metadata, file list, and checksums.
    /// Stage 9: Export system manifest generation.
    /// </summary>
    public class ManifestWriter
    {
        /// <summary>
        /// Write manifest file.
        /// </summary>
        public void Write(string manifestPath, string exportDir, ScenarioDefinition scenario, RunRequest request, RunResult result)
        {
            var manifest = new ExportManifest
            {
                RunId = result.RunId ?? Guid.NewGuid().ToString(),
                ExportedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Engine = new EngineInfo
                {
                    Name = "VIABLE",
                    Version = result.Metadata?.EngineVersion ?? "1.0.0",
                    SchemaVersion = result.Metadata?.SchemaVersion ?? "1.0"
                },
                Unity = new UnityInfo
                {
                    Version = Application.unityVersion,
                    Platform = Application.platform.ToString()
                },
                Scenario = new ScenarioInfo
                {
                    PresetId = scenario.ScenarioId,
                    PresetName = scenario.ScenarioName,
                    Description = scenario.Description,
                    Seed = scenario.Seed,
                    GridWidth = scenario.GridWidth,
                    GridHeight = scenario.GridHeight
                },
                Execution = new ExecutionInfo
                {
                    Steps = result.StepsExecuted,
                    DurationMs = result.ExecutionTimeMs,
                    SamplesCollected = result.Samples?.Count ?? 0,
                    EventsRecorded = result.Events?.Count ?? 0
                },
                Files = CollectFileInfo(exportDir)
            };

            string json = JsonUtility.ToJson(manifest, prettyPrint: true);
            File.WriteAllText(manifestPath, json);
        }

        /// <summary>
        /// Collect file information from export directory.
        /// </summary>
        private List<FileInfo> CollectFileInfo(string exportDir)
        {
            var files = new List<FileInfo>();
            var dirInfo = new DirectoryInfo(exportDir);

            foreach (var fileInfo in dirInfo.GetFiles())
            {
                // Skip manifest itself (it's being written)
                if (fileInfo.Name == "manifest.json")
                    continue;

                files.Add(new FileInfo
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    Sha256 = "" // Will be filled by ChecksumWriter
                });
            }

            return files;
        }
    }

    // Manifest data structures (serializable by Unity JsonUtility)
    [Serializable]
    public class ExportManifest
    {
        public string RunId;
        public string ExportedAtUtc;
        public EngineInfo Engine;
        public UnityInfo Unity;
        public ScenarioInfo Scenario;
        public ExecutionInfo Execution;
        public List<FileInfo> Files;
    }

    [Serializable]
    public class EngineInfo
    {
        public string Name;
        public string Version;
        public string SchemaVersion;
    }

    [Serializable]
    public class UnityInfo
    {
        public string Version;
        public string Platform;
    }

    [Serializable]
    public class ScenarioInfo
    {
        public string PresetId;
        public string PresetName;
        public string Description;
        public int? Seed;
        public int GridWidth;
        public int GridHeight;
    }

    [Serializable]
    public class ExecutionInfo
    {
        public int Steps;
        public double DurationMs;
        public int SamplesCollected;
        public int EventsRecorded;
    }

    [Serializable]
    public class FileInfo
    {
        public string Name;
        public long Size;
        public string Sha256;
    }
}
