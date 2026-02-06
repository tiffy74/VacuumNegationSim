using System;
using System.Collections.Generic;
using UnityEngine;

namespace Viable.Core.Unity
{
    /// <summary>
    /// ScriptableObject preset for configuring and running a simulation scenario.
    /// This freezes a specific configuration that can be loaded, run, and exported.
    /// </summary>
    [CreateAssetMenu(fileName = "NewScenarioPreset", menuName = "Viable/Scenario Preset", order = 1)]
    public class ScenarioPreset : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this preset (e.g., 'constraints-expansion-demo')")]
        public string PresetId = "default";

        [Tooltip("Human-readable name displayed in UI")]
        public string PresetName = "Default Scenario";

        [Tooltip("Description of what this preset demonstrates")]
        [TextArea(3, 6)]
        public string Description = "A scenario demonstrating configuration space dynamics.";

        [Header("Grid Configuration")]
        [Tooltip("Grid width in cells")]
        public int GridWidth = 64;

        [Tooltip("Grid height in cells")]
        public int GridHeight = 64;

        [Header("Execution")]
        [Tooltip("Random seed for deterministic execution (null = random)")]
        public int? Seed = 42;

        [Tooltip("Initial global resource pool")]
        public float InitialResourceGlobal = 1e7f;

        [Tooltip("Scale factor for simulation")]
        public float ScaleFactor = 1.0f;

        [Tooltip("Delta time per step")]
        public float DeltaTime = 1.0f;

        [Header("Simulation Parameters")]
        [Tooltip("Core simulation parameters - add as needed")]
        public List<ParameterEntry> Parameters = new List<ParameterEntry>();

        [Header("Mechanism Configuration")]
        [Tooltip("Mechanism modes (Inflow, Boundary, Diffusion, etc.) - optional, uses defaults if not set")]
        public EngineConfigData MechanismConfig = new EngineConfigData();

        [Header("Visualization")]
        [Tooltip("Inactive cell color")]
        public Color InactiveColor = new Color(0.05f, 0.05f, 0.08f, 1f);

        [Tooltip("Dormant region color")]
        public Color DormantRegionColor = new Color(0.15f, 0.0f, 0.25f, 1f);

        [Tooltip("Show complexity tint overlay")]
        public bool ShowComplexityTint = false;

        [Tooltip("Ticks per second for real-time visualization")]
        public float TicksPerSecond = 10f;

        /// <summary>
        /// Get a parameter value by key, or return default if not found.
        /// </summary>
        public double GetParameter(string key, double defaultValue = 0.0)
        {
            var entry = Parameters.Find(p => p.Key == key);
            return entry != null ? entry.Value : defaultValue;
        }

        /// <summary>
        /// Set a parameter value (adds if not present).
        /// </summary>
        public void SetParameter(string key, double value)
        {
            var entry = Parameters.Find(p => p.Key == key);
            if (entry != null)
            {
                entry.Value = value;
            }
            else
            {
                Parameters.Add(new ParameterEntry { Key = key, Value = value });
            }
        }

        /// <summary>
        /// Convert parameters list to dictionary for Engine consumption.
        /// </summary>
        public Dictionary<string, double> GetParametersDictionary()
        {
            var dict = new Dictionary<string, double>();
            foreach (var param in Parameters)
            {
                if (!string.IsNullOrEmpty(param.Key))
                {
                    dict[param.Key] = param.Value;
                }
            }
            return dict;
        }
    }

    /// <summary>
    /// Serializable key-value pair for parameters.
    /// </summary>
    [Serializable]
    public class ParameterEntry
    {
        public string Key;
        public double Value;
    }

    /// <summary>
    /// Serializable mechanism configuration for presets.
    /// Stores mechanism modes that define simulation behavior.
    /// </summary>
    [Serializable]
    public class EngineConfigData
    {
        [Header("Mechanism Modes")]
        [Tooltip("How resources flow into the system")]
        public Configuration.InflowMode InflowMode = Configuration.InflowMode.UniformField;

        [Tooltip("How boundaries behave (wrap, absorb, reflect)")]
        public Configuration.BoundaryMode BoundaryMode = Configuration.BoundaryMode.Wrap;

        [Tooltip("Diffusion pattern (4-neighbor, 8-neighbor, anisotropic)")]
        public Configuration.DiffusionMode DiffusionMode = Configuration.DiffusionMode.Moore8;

        [Tooltip("Viability rule (simple threshold, hysteresis)")]
        public Configuration.ViabilityRuleMode ViabilityRule = Configuration.ViabilityRuleMode.Simple;

        [Tooltip("Grid cell shape (rectangular, triangular, hexagonal)")]
        public Contracts.TopologyMode GridTopology = Contracts.TopologyMode.RectGrid; // NEW: Cell tessellation

        [Tooltip("Active domain shape (full grid or masked region)")]
        public Configuration.DomainMode DomainMode = Configuration.DomainMode.FullDomain; // RENAMED: Domain masking

        [Header("Domain Details (when MaskedDomain)")]
        public Configuration.MaskShape MaskShape = Configuration.MaskShape.Circle;
        public float MaskRadiusOuter = 20f;
        public float MaskRadiusInner = 10f;
        public float MaskCorridorWidth = 8f;
        public float MaskPercolationProbability = 0.3f;

        [Header("Point Sources (when InflowMode = PointSources)")]
        public List<Configuration.PointSourceData> PointSources = new List<Configuration.PointSourceData>();

        [Header("Hysteresis (when ViabilityRule = Hysteresis)")]
        public float HysteresisOnThreshold = 0.5f;
        public float HysteresisOffThreshold = -0.5f;

        [Header("Anisotropic Diffusion (when DiffusionMode = Anisotropic)")]
        public Configuration.DiffusionDirection AnisotropicDirection = Configuration.DiffusionDirection.North;
        public float AnisotropicBias = 0.7f;
    }
}
