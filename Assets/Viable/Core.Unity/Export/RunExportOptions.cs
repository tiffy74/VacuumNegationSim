namespace Viable.Core.Unity.Export
{
    /// <summary>
    /// Export level determines what data is included in the output.
    /// Stage 9: Export configuration system.
    /// </summary>
    public enum ExportLevel
    {
        /// <summary>
        /// Study level: Metrics + Events, sampled data, compact output.
        /// Use for exploratory analysis and parameter sweeps.
        /// </summary>
        Study,

        /// <summary>
        /// Publication level: Study + Final state + Summary, higher sampling.
        /// Use for manuscript preparation and reproducible research.
        /// </summary>
        Publication,

        /// <summary>
        /// Debug level: Publication + Full state snapshots + Every-step sampling.
        /// Use for detailed diagnostics and debugging.
        /// </summary>
        Debug
    }

    /// <summary>
    /// Configuration options for exporting simulation runs.
    /// Controls what data is exported and how it's formatted.
    /// </summary>
    public class RunExportOptions
    {
        /// <summary>
        /// Export level (Study, Publication, Debug).
        /// </summary>
        public ExportLevel ExportLevel { get; set; } = ExportLevel.Study;

        /// <summary>
        /// Sample state every N steps (0 = no sampling, 1 = every step).
        /// Default: 10 for Study, 5 for Publication, 1 for Debug.
        /// </summary>
        public int SampleEveryNSteps { get; set; } = 10;

        /// <summary>
        /// Include state samples in export (time-series snapshots).
        /// </summary>
        public bool IncludeStateSamples { get; set; } = true;

        /// <summary>
        /// Include final state snapshot in export.
        /// </summary>
        public bool IncludeFinalState { get; set; } = true;

        /// <summary>
        /// Include simulation events in export.
        /// </summary>
        public bool IncludeEvents { get; set; } = true;

        /// <summary>
        /// Include plots/visualizations (not implemented yet - Stage 12).
        /// </summary>
        public bool IncludePlots { get; set; } = false;

        /// <summary>
        /// Compress large output files (not implemented yet).
        /// </summary>
        public bool CompressLargeOutputs { get; set; } = false;

        /// <summary>
        /// Base directory for exports (null = use default).
        /// Default: Application.persistentDataPath + "/Exports/"
        /// </summary>
        public string ExportDirectory { get; set; } = null;

        /// <summary>
        /// Create a preset-based options configuration.
        /// </summary>
        public static RunExportOptions ForLevel(ExportLevel level)
        {
            return level switch
            {
                ExportLevel.Study => new RunExportOptions
                {
                    ExportLevel = ExportLevel.Study,
                    SampleEveryNSteps = 10,
                    IncludeStateSamples = true,
                    IncludeFinalState = false,
                    IncludeEvents = true,
                    IncludePlots = false,
                    CompressLargeOutputs = false
                },
                ExportLevel.Publication => new RunExportOptions
                {
                    ExportLevel = ExportLevel.Publication,
                    SampleEveryNSteps = 5,
                    IncludeStateSamples = true,
                    IncludeFinalState = true,
                    IncludeEvents = true,
                    IncludePlots = false,
                    CompressLargeOutputs = false
                },
                ExportLevel.Debug => new RunExportOptions
                {
                    ExportLevel = ExportLevel.Debug,
                    SampleEveryNSteps = 1,
                    IncludeStateSamples = true,
                    IncludeFinalState = true,
                    IncludeEvents = true,
                    IncludePlots = false,
                    CompressLargeOutputs = true
                },
                _ => new RunExportOptions()
            };
        }
    }
}
