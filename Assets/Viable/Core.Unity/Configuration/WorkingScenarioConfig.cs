using System;
using System.Collections.Generic;
using UnityEngine;

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

        [Header("Mechanism Modes")]
        public TopologyMode Topology = TopologyMode.FullDomain;
        public BoundaryMode Boundary = BoundaryMode.Wrap;
        public InflowMode Inflow = InflowMode.UniformField;
        public DiffusionMode Diffusion = DiffusionMode.Moore8;
        public ViabilityRuleMode ViabilityRule = ViabilityRuleMode.Simple;
        public string PhaseSetId = "Standard";

        [Header("Topology Details (when MaskedDomain)")]
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

        [Header("Core Parameters (Curated)")]
        public double ResourceGlobalMax = 5e7;
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
        /// Get mechanism summary for display.
        /// </summary>
        public string GetMechanismSummary()
        {
            return $"Topology: {Topology} • Boundary: {Boundary} • Inflow: {Inflow} • Diffusion: {Diffusion} • Viability: {ViabilityRule}";
        }
    }

    #region Enums

    public enum TopologyMode
    {
        FullDomain,
        MaskedDomain
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
