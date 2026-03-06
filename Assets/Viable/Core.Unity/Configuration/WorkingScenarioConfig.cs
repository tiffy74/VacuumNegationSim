using System;
using System.Collections.Generic;
using UnityEngine;
using Viable.Contracts; // ADDED: For TopologyMode enum

namespace Viable.Core.Unity.Configuration
{
    /// <summary>
    /// Unity-side working configuration that holds all UI edits.
    /// Only pushed to Engine on "Apply & Restart".
    /// This allows editing without breaking reproducibility mid-run.
    /// </summary>
    [Serializable]
    public class WorkingScenarioConfig
    {
        [Header("Identity")]
        public string ScenarioId = "Custom";
        public int Seed = 42;
        
        [Header("Grid Dimensions")]
        public int GridWidth = 64;
        public int GridHeight = 64;

        [Header("Expansion Model")]
        [Tooltip("Expansion model type (stored as int to avoid assembly type conflicts)")]
        public int ExpansionModelTypeInt = 0; // 0 = DefaultViabilityBoundaryPressure
        
        /// <summary>
        /// Get the expansion model type as the enum value.
        /// Uses int storage to avoid Unity assembly type conflicts.
        /// </summary>
        public Contracts.ExpansionModel ExpansionModelType
        {
            get => (Contracts.ExpansionModel)ExpansionModelTypeInt;
            set => ExpansionModelTypeInt = (int)value;

        }
        [Header("Scenario Preset")]
        [Tooltip("Pre-Configured Parameters to match defined scenarios)")]
        public int ScenarioPresetInt = 0; // 0 = Default

        /// <summary>
        /// Get the Scenario preset type as the enum value.
        /// Uses int storage to avoid Unity assembly type conflicts.
        /// </summary>
        public ScenarioPresetType ScenarioPresetSelection
        {
            get => (ScenarioPresetType)ScenarioPresetInt;
            set => ScenarioPresetInt = (int)value;
        }

        [Header("Mechanism Modes")]
        public Contracts.TopologyMode GridTopology = Contracts.TopologyMode.RectGrid; // Cell shape (Rect/Tri/Hex)
        public AdjacencyMode Adjacency = AdjacencyMode.EdgeOnly; // Neighbor connectivity (edge vs edge+vertex)
        public DomainMode Domain = DomainMode.FullDomain; // Active region shape
        public BoundaryMode Boundary = BoundaryMode.Wrap;
        public InflowMode Inflow = InflowMode.UniformField;
        public DiffusionMode Diffusion = DiffusionMode.Moore8;
        public ViabilityRuleMode ViabilityRule = ViabilityRuleMode.Simple;
        public string PhaseSetId = "Standard";

        [Header("Domain Details (when MaskedDomain)")]
        public MaskShape MaskType = MaskShape.Circle;
        public float MaskRadiusOuter = 20f;
        public float MaskRadiusInner = 10f;
        public float MaskCorridorWidth = 8f;
        public float MaskPercolationProbability = 0.3f;

        [Header("Inflow Details (when PointSources)")]
        public List<PointSourceData> PointSources = new List<PointSourceData>();

        [Header("Diffusion Details (when Anisotropic)")]
        public DiffusionDirection AnisotropicDirection = DiffusionDirection.North;
        public float AnisotropicBias = 0.7f;

        [Header("Viability Details (when Hysteresis)")]
        public float HysteresisOnThreshold = 0.5f;
        public float HysteresisOffThreshold = -0.5f;

        [Header("Sink Controls")]
        public double SinkFormationThreshold = 0.5;
        public double SinkDrainFraction = 0.0;
        public double SinkRecoilFraction = 0.0;

        [Header("Sink Placement")]
        public int InitialSinkCount = 0;
        public float SinkSpacing = 10f;
        [Range(0f, 1f)] public float SinkRandomness = 0f;

        [Header("Core Parameters (Curated)")]
        public double ResourceGlobalMax = 5e7;
        public double InitialResourceGlobal = 1e7f;
        public double ScaleFactor = 1.0;  // Scale factor for simulation dynamics
        public double ResourceRechargeRate = 1e6;
        public double DecayLoss = 0.003;
        public double MaintCost = 1.0;
        public double ActivationCost = 5.0;
        public double ExpansionProbability = 0.005;
        public double InflowPerCell = 1e4;
        public double DiffusionRate = 0.1;

        [Header("Advanced Parameters (Not shown in main UI)")]
        public Dictionary<string, double> AdvancedParams = new Dictionary<string, double>();

        /// <summary>
        /// Clone this config for editing without modifying the original.
        /// </summary>
        public WorkingScenarioConfig Clone()
        {
            var clone = (WorkingScenarioConfig)MemberwiseClone();
            clone.PointSources = new List<PointSourceData>(PointSources);
            clone.AdvancedParams = new Dictionary<string, double>(AdvancedParams);
            return clone;
        }

        /// <summary>
        /// Create WorkingScenarioConfig from a ScenarioPreset (deep copy).
        /// Maps all preset values to WorkingConfig fields.
        /// </summary>
        public static WorkingScenarioConfig FromPreset(ScenarioPreset preset)
        {
            if (preset == null)
                throw new System.ArgumentNullException(nameof(preset));

            var config = new WorkingScenarioConfig
            {
                ScenarioId = preset.PresetName ?? "Custom",
                Seed = preset.Seed ?? 42,
                GridWidth = preset.GridWidth,
                GridHeight = preset.GridHeight,
                PhaseSetId = "Standard"
            };

            // Map mechanism modes from preset's MechanismConfig
            if (preset.MechanismConfig != null)
            {
                // NEW: Map GridTopology from preset
                config.GridTopology = preset.MechanismConfig.GridTopology;
                config.Adjacency = preset.MechanismConfig.AdjacencyMode;
                Debug.Log($"[WorkingScenarioConfig] Loaded GridTopology from preset: {config.GridTopology}");
                
                config.Domain = preset.MechanismConfig.DomainMode; // RENAMED
                config.Boundary = preset.MechanismConfig.BoundaryMode;
                config.Inflow = preset.MechanismConfig.InflowMode;
                config.Diffusion = preset.MechanismConfig.DiffusionMode;
                config.ViabilityRule = preset.MechanismConfig.ViabilityRule;

                // Sink controls
                config.SinkFormationThreshold = preset.MechanismConfig.SinkFormationThreshold;
                config.SinkDrainFraction = preset.MechanismConfig.SinkDrainFraction;
                config.SinkRecoilFraction = preset.MechanismConfig.SinkRecoilFraction;
                config.InitialSinkCount = preset.MechanismConfig.InitialSinkCount;
                config.SinkSpacing = preset.MechanismConfig.SinkSpacing;
                config.SinkRandomness = preset.MechanismConfig.SinkRandomness;

                // Domain details
                config.MaskType = preset.MechanismConfig.MaskShape;
                config.MaskRadiusOuter = preset.MechanismConfig.MaskRadiusOuter;
                config.MaskRadiusInner = preset.MechanismConfig.MaskRadiusInner;
                config.MaskCorridorWidth = preset.MechanismConfig.MaskCorridorWidth;
                config.MaskPercolationProbability = preset.MechanismConfig.MaskPercolationProbability;

                // Point sources (deep copy)
                config.PointSources = new List<PointSourceData>();
                foreach (var src in preset.MechanismConfig.PointSources)
                {
                    config.PointSources.Add(new PointSourceData(src.X, src.Y, src.Strength));
                }

                // Hysteresis
                config.HysteresisOnThreshold = preset.MechanismConfig.HysteresisOnThreshold;
                config.HysteresisOffThreshold = preset.MechanismConfig.HysteresisOffThreshold;

                // Anisotropic diffusion
                config.AnisotropicDirection = preset.MechanismConfig.AnisotropicDirection;
                config.AnisotropicBias = preset.MechanismConfig.AnisotropicBias;
            }

            // Map core parameters using GetParameter method
            config.ResourceGlobalMax = preset.GetParameter("resourceGlobalMax", 5e7);
            config.InitialResourceGlobal = preset.InitialResourceGlobal; // CRITICAL: Copy from preset
            config.ScaleFactor = preset.ScaleFactor; // Copy scale factor from preset
            config.ResourceRechargeRate = preset.GetParameter("globalReplenishPerTick", 200);
            config.DecayLoss = preset.GetParameter("decayLoss", 0.003);
            config.MaintCost = preset.GetParameter("ethreshBase", 0.18); // Using ethreshBase as maintCost proxy
            config.ActivationCost = preset.GetParameter("activationCost", 0.25);
            config.ExpansionProbability = preset.GetParameter("regionExpansionChance", 0.25);
            config.InflowPerCell = preset.GetParameter("inflowPerCell", 1e4);
            config.DiffusionRate = preset.GetParameter("complexityDiffusionRate", 0.2);

            // Copy all parameters to advanced params
            var paramDict = preset.GetParametersDictionary();
            foreach (var kvp in paramDict)
            {
                config.AdvancedParams[kvp.Key] = kvp.Value;
            }

            return config;
        }

        /// <summary>
        /// Get mechanism summary for display.
        /// </summary>
        public string GetMechanismSummary()
        {
            return $"Grid: {GridTopology} • Domain: {Domain} • Boundary: {Boundary} • Inflow: {Inflow} • Diffusion: {Diffusion} • Viability: {ViabilityRule}";
        }
    }

    #region Enums

    /// <summary>
    /// Available scenario preset types for quick configuration.
    /// </summary>
    public enum ScenarioPresetType
    {
        Default = 0,
        ViabilityBoundaryPressure = 1,
        // Add additional preset types as needed
    }

    public enum DomainMode // RENAMED from TopologyMode
    {
        FullDomain,     // Entire grid is active
        MaskedDomain    // Only cells inside mask shape are active
    }

    public enum BoundaryMode
    {
        Closed,    // Reflective
        Open,      // Absorbing
        Wrap       // Periodic
    }

    public enum InflowMode
    {
        UniformField,
        PointSources,
        EdgeSources
    }

    public enum DiffusionMode
    {
        VonNeumann4,
        Moore8,
        Anisotropic
    }

    public enum ViabilityRuleMode
    {
        Simple,
        Hysteresis
    }

    public enum MaskShape
    {
        Rectangle,
        Circle,
        Ring,
        Corridor,
        PercolationHoles
    }

    public enum DiffusionDirection
    {
        North,
        East,
        South,
        West
    }

    #endregion

    #region Data Structures

    [Serializable]
    public class PointSourceData
    {
        public int X;
        public int Y;
        public double Strength;

        public PointSourceData(int x, int y, double strength)
        {
            X = x;
            Y = y;
            Strength = strength;
        }
    }

    #endregion
}
