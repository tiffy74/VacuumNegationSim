using System.Collections.Generic;

namespace Viable.Contracts
{
    /// <summary>
    /// Defines a simulation scenario with initial parameters and configuration.
    /// </summary>
    public sealed class ScenarioDefinition
    {
        /// <summary>
        /// Unique identifier for this scenario (e.g., "default", "high-stress").
        /// </summary>
        public string ScenarioId { get; set; } = "default";

        /// <summary>
        /// Human-readable scenario name.
        /// </summary>
        public string ScenarioName { get; set; } = "Default Scenario";

        /// <summary>
        /// Scenario description for documentation.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Simulation parameters (e.g., "resourceGlobalMax", "decayLoss").
        /// Keys are parameter names, values are numeric parameters.
        /// </summary>
        public Dictionary<string, double> Parameters { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Initial state configuration (serialized as generic object for now).
        /// Will be strongly typed once state model is extracted to Engine.
        /// </summary>
        public object InitialState { get; set; }

        /// <summary>
        /// Random seed for deterministic execution. Null = non-deterministic.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Grid dimensions if applicable.
        /// </summary>
        public int GridWidth { get; set; } = 64;

        /// <summary>
        /// Grid dimensions if applicable.
        /// </summary>
        public int GridHeight { get; set; } = 64;

        /// <summary>
        /// Plugin/extension identifiers for optional scenario behaviors.
        /// </summary>
        public string[] Plugins { get; set; } = new string[0];

        /// <summary>
        /// Engine mechanism configuration.
        /// Stage 13: Enables behavioral preset diversity.
        /// If null, defaults are used (preserves current behavior).
        /// </summary>
        public EngineConfig EngineConfig { get; set; }
    }
}
