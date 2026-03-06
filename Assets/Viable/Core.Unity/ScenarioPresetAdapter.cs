using System.Collections.Generic;
using Viable.Contracts;
using Viable.Engine.Configuration;
using Viable.Engine.Execution;

namespace Viable.Core.Unity
{
    /// <summary>
    /// Converts a ScenarioPreset (Unity ScriptableObject) into Engine-compatible types.
    /// This is the bridge between the Unity layer and the pure Engine.
    /// </summary>
    public static class ScenarioPresetAdapter
    {
        /// <summary>
        /// Convert a ScenarioPreset to a ScenarioDefinition for Engine consumption.
        /// </summary>
        public static ScenarioDefinition ToScenarioDefinition(ScenarioPreset preset)
        {
            var scenario = new ScenarioDefinition
            {
                ScenarioId = preset.PresetId,
                ScenarioName = preset.PresetName,
                Description = preset.Description,
                GridWidth = preset.GridWidth,
                GridHeight = preset.GridHeight,
                Seed = preset.Seed,
                Parameters = preset.GetParametersDictionary()
            };

            // NEW: Map EngineConfig including GridTopology
            if (preset.MechanismConfig != null)
            {
                scenario.EngineConfig = new EngineConfig
                {
                    TopologyMode = preset.MechanismConfig.GridTopology, // NEW: Cell shape (Rect/Tri/Hex)
                    AdjacencyMode = preset.MechanismConfig.AdjacencyMode, // NEW: Edge-only vs edge+vertex
                    InflowMode = MapInflowMode(preset.MechanismConfig.InflowMode),
                    BoundaryMode = MapBoundaryMode(preset.MechanismConfig.BoundaryMode),
                    DiffusionMode = MapDiffusionMode(preset.MechanismConfig.DiffusionMode),
                    ViabilityRule = MapViabilityRule(preset.MechanismConfig.ViabilityRule),
                    // Sink controls
                    SinkFormationThreshold = preset.MechanismConfig.SinkFormationThreshold,
                    SinkDrainFraction = preset.MechanismConfig.SinkDrainFraction,
                    SinkRecoilFraction = preset.MechanismConfig.SinkRecoilFraction,
                    InitialSinkCount = preset.MechanismConfig.InitialSinkCount,
                    SinkSpacing = preset.MechanismConfig.SinkSpacing,
                    SinkRandomness = preset.MechanismConfig.SinkRandomness,
                    
                    // Domain masking configuration
                    MaskShape = MapMaskShape(preset.MechanismConfig.DomainMode, preset.MechanismConfig.MaskShape),
                    MaskRadius = preset.MechanismConfig.MaskRadiusOuter,
                    MaskInnerRadius = preset.MechanismConfig.MaskRadiusInner,
                    CorridorWidth = preset.MechanismConfig.MaskCorridorWidth,
                    HoleProbability = preset.MechanismConfig.MaskPercolationProbability,
                    
                    // Point sources
                    PointSources = MapPointSources(preset.MechanismConfig.PointSources),
                    
                    // Hysteresis
                    HysteresisOnThreshold = preset.MechanismConfig.HysteresisOnThreshold,
                    HysteresisOffThreshold = preset.MechanismConfig.HysteresisOffThreshold
                };
            }

            return scenario;
        }

        // Helper methods to map Unity enums to Contracts enums
        private static InflowMode MapInflowMode(Configuration.InflowMode mode)
        {
            return mode switch
            {
                Configuration.InflowMode.UniformField => InflowMode.Uniform,
                Configuration.InflowMode.PointSources => InflowMode.PointSources,
                Configuration.InflowMode.EdgeSources => InflowMode.Uniform, // Map to Uniform for now
                _ => InflowMode.Uniform
            };
        }

        private static BoundaryMode MapBoundaryMode(Configuration.BoundaryMode mode)
        {
            return mode switch
            {
                Configuration.BoundaryMode.Closed => BoundaryMode.Reflecting,
                Configuration.BoundaryMode.Open => BoundaryMode.Absorbing,
                Configuration.BoundaryMode.Wrap => BoundaryMode.PeriodicWrap,
                _ => BoundaryMode.Absorbing
            };
        }

        private static DiffusionMode MapDiffusionMode(Configuration.DiffusionMode mode)
        {
            return mode switch
            {
                Configuration.DiffusionMode.VonNeumann4 => DiffusionMode.VonNeumann4,
                Configuration.DiffusionMode.Moore8 => DiffusionMode.Moore8,
                Configuration.DiffusionMode.Anisotropic => DiffusionMode.Anisotropic,
                _ => DiffusionMode.VonNeumann4
            };
        }

        private static ViabilityRule MapViabilityRule(Configuration.ViabilityRuleMode mode)
        {
            return mode switch
            {
                Configuration.ViabilityRuleMode.Simple => ViabilityRule.HardThreshold,
                Configuration.ViabilityRuleMode.Hysteresis => ViabilityRule.Hysteresis,
                _ => ViabilityRule.HardThreshold
            };
        }

        private static MaskShape MapMaskShape(Configuration.DomainMode domain, Configuration.MaskShape shape)
        {
            // If FullDomain, return Rectangle (no masking)
            if (domain == Configuration.DomainMode.FullDomain)
                return MaskShape.Rectangle;

            // Otherwise map the shape
            return shape switch
            {
                Configuration.MaskShape.Rectangle => MaskShape.Rectangle,
                Configuration.MaskShape.Circle => MaskShape.Circle,
                Configuration.MaskShape.Ring => MaskShape.Ring,
                Configuration.MaskShape.Corridor => MaskShape.Corridor,
                Configuration.MaskShape.PercolationHoles => MaskShape.PercolationHoles,
                _ => MaskShape.Rectangle
            };
        }

        private static List<PointSourceConfig> MapPointSources(List<Configuration.PointSourceData> sources)
        {
            var result = new List<PointSourceConfig>();
            if (sources != null)
            {
                foreach (var src in sources)
                {
                    result.Add(new PointSourceConfig
                    {
                        X = src.X,
                        Y = src.Y,
                        Strength = src.Strength
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Build a SimulationConfiguration from the preset's parameters.
        /// This maps parameter keys to configuration fields.
        /// </summary>
        public static SimulationConfiguration ToSimulationConfiguration(ScenarioPreset preset)
        {
            var config = new SimulationConfiguration();

            // Map topology/adjacency from preset mechanism config if present
            if (preset.MechanismConfig != null)
            {
                config.TopologyMode = preset.MechanismConfig.GridTopology;
                config.AdjacencyMode = preset.MechanismConfig.AdjacencyMode;
            }

            // Map parameters to configuration fields
            // Using helper to get parameter with default fallback
            config.ResourceGlobalMax = (float)preset.GetParameter("resourceGlobalMax", 5e7);
            config.GlobalReplenishPerTick = (float)preset.GetParameter("globalReplenishPerTick", 200);
            config.MinResourceForPersistence = (float)preset.GetParameter("minResourceForPersistence", 5);

            config.EthreshBase = (float)preset.GetParameter("ethreshBase", 0.18);
            config.GlobalScarcityK = (float)preset.GetParameter("globalScarcityK", 0.3);
            config.ComplexityPenalty = (float)preset.GetParameter("complexityPenalty", 0.02);
            config.DecayLoss = (float)preset.GetParameter("decayLoss", 0.003);

            config.PropagateFrac = (float)preset.GetParameter("propagateFrac", 0.25);
            config.MinBudgetToPropagate = (float)preset.GetParameter("minBudgetToPropagate", 0.1);
            config.ActivationCost = (float)preset.GetParameter("activationCost", 0.25);

            config.ComplexityGainPerUse = (float)preset.GetParameter("complexityGainPerUse", 0.2);
            config.ComplexityDiffusionRate = (float)preset.GetParameter("complexityDiffusionRate", 0.2);
            config.ComplexityDecay = (float)preset.GetParameter("complexityDecay", 0.02);

            config.ResourceLocalMax = (float)preset.GetParameter("resourceLocalMax", 5e4);
            config.PerturbationProbability = (float)preset.GetParameter("perturbationProbability", 0.0002);
            config.PerturbationComplexity = (float)preset.GetParameter("perturbationComplexity", 0.5);
            config.ExpansionRate = (float)preset.GetParameter("expansionRate", 1.0);
            config.MatterAheadThreshold = (float)preset.GetParameter("matterAheadThreshold", 0);

            config.SinkFormationThreshold = (float)preset.GetParameter("sinkFormationThreshold", 0.5);
            config.SinkDrainFraction = (float)preset.GetParameter("sinkDrainFraction", 0);
            config.SinkRecoilFraction = (float)preset.GetParameter("sinkRecoilFraction", 0);

            config.RegionExpansionChance = (float)preset.GetParameter("regionExpansionChance", 0.25);
            config.RegionExpansionCost = (float)preset.GetParameter("regionExpansionCost", 0.05);
            config.RegionExpansionMinSource = (float)preset.GetParameter("regionExpansionMinSource", 0.1);
            config.RegionExpansionRequiresViability = preset.GetParameter("regionExpansionRequiresViability", 0) > 0.5;
            config.RegionExpansionSeedsResource = preset.GetParameter("regionExpansionSeedsResource", 1) > 0.5;
            config.RegionSeedResource = (float)preset.GetParameter("regionSeedResource", 0.1);

            // Sink controls
            if (preset.MechanismConfig != null)
            {
                config.SinkFormationThreshold = preset.MechanismConfig.SinkFormationThreshold;
                config.SinkDrainFraction = preset.MechanismConfig.SinkDrainFraction;
                config.SinkRecoilFraction = preset.MechanismConfig.SinkRecoilFraction;
                config.InitialSinkCount = preset.MechanismConfig.InitialSinkCount;
                config.SinkSpacing = preset.MechanismConfig.SinkSpacing;
                config.SinkRandomness = preset.MechanismConfig.SinkRandomness;
            }

            config.ComplexityGainFromGradient = (float)preset.GetParameter("complexityGainFromGradient", 0.02);
            config.ComplexityGainNearSink = (float)preset.GetParameter("complexityGainNearSink", 0.05);
            config.ComplexityViabilityGainA = (float)preset.GetParameter("complexityViabilityGainA", 0.5);
            config.ComplexityViabilityGainK = (float)preset.GetParameter("complexityViabilityGainK", 1.0);

            config.ShowComplexityTint = preset.ShowComplexityTint;
            config.ViabilityColorScale = 40f;

            return config;
        }

        /// <summary>
        /// Build a SimulationConfiguration from a ScenarioDefinition.
        /// This is used when UI creates a scenario from WorkingConfig.
        /// CRITICAL: Maps EngineConfig mechanism modes to SimulationConfiguration.
        /// </summary>
        public static SimulationConfiguration ToSimulationConfiguration(ScenarioDefinition scenario)
        {
            var config = new SimulationConfiguration();

            // Map EngineConfig mechanism modes
            if (scenario.EngineConfig != null)
            {
                config.InflowMode = scenario.EngineConfig.InflowMode;
                config.BoundaryMode = scenario.EngineConfig.BoundaryMode;
                config.DiffusionMode = scenario.EngineConfig.DiffusionMode;
                config.ViabilityRule = scenario.EngineConfig.ViabilityRule;
                config.TopologyMode = scenario.EngineConfig.TopologyMode;
                config.AdjacencyMode = scenario.EngineConfig.AdjacencyMode;
                config.MaskShape = scenario.EngineConfig.MaskShape;

                // Sink controls
                config.SinkFormationThreshold = (float)scenario.EngineConfig.SinkFormationThreshold;
                config.SinkDrainFraction = (float)scenario.EngineConfig.SinkDrainFraction;
                config.SinkRecoilFraction = (float)scenario.EngineConfig.SinkRecoilFraction;
                config.InitialSinkCount = scenario.EngineConfig.InitialSinkCount;
                config.SinkSpacing = (float)scenario.EngineConfig.SinkSpacing;
                config.SinkRandomness = (float)scenario.EngineConfig.SinkRandomness;

                config.HysteresisOnThreshold = scenario.EngineConfig.HysteresisOnThreshold;
                config.HysteresisOffThreshold = scenario.EngineConfig.HysteresisOffThreshold;

                config.MaskRadius = scenario.EngineConfig.MaskRadius;
                config.MaskInnerRadius = scenario.EngineConfig.MaskInnerRadius;
                config.CorridorWidth = scenario.EngineConfig.CorridorWidth;
                config.HoleProbability = scenario.EngineConfig.HoleProbability;

                // NOTE: Anisotropy parameters are in EngineConfig but not yet in SimulationConfiguration
                // They will be added when anisotropic diffusion is fully implemented
                // For now, anisotropic diffusion mode is stored but direction/bias ignored
            }

            // Map numeric parameters
            if (scenario.Parameters != null)
            {
                config.ResourceGlobalMax = (float)GetParam(scenario.Parameters, "resourceGlobalMax", 5e7);
                config.GlobalReplenishPerTick = (float)GetParam(scenario.Parameters, "globalReplenishPerTick", 200);
                config.MinResourceForPersistence = (float)GetParam(scenario.Parameters, "minResourceForPersistence", 5);

                config.EthreshBase = (float)GetParam(scenario.Parameters, "ethreshBase", 0.18);
                config.GlobalScarcityK = (float)GetParam(scenario.Parameters, "globalScarcityK", 0.3);
                config.ComplexityPenalty = (float)GetParam(scenario.Parameters, "complexityPenalty", 0.02);
                config.DecayLoss = (float)GetParam(scenario.Parameters, "decayLoss", 0.003);

                config.PropagateFrac = (float)GetParam(scenario.Parameters, "propagateFrac", 0.25);
                config.MinBudgetToPropagate = (float)GetParam(scenario.Parameters, "minBudgetToPropagate", 0.1);
                config.ActivationCost = (float)GetParam(scenario.Parameters, "activationCost", 0.25);

                config.ComplexityGainPerUse = (float)GetParam(scenario.Parameters, "complexityGainPerUse", 0.2);
                config.ComplexityDiffusionRate = (float)GetParam(scenario.Parameters, "complexityDiffusionRate", 0.2);
                config.ComplexityDecay = (float)GetParam(scenario.Parameters, "complexityDecay", 0.02);

                config.ResourceLocalMax = (float)GetParam(scenario.Parameters, "resourceLocalMax", 5e4);
                config.PerturbationProbability = (float)GetParam(scenario.Parameters, "perturbationProbability", 0.0002);
                config.PerturbationComplexity = (float)GetParam(scenario.Parameters, "perturbationComplexity", 0.5);
                config.ExpansionRate = (float)GetParam(scenario.Parameters, "expansionRate", 1.0);
                config.MatterAheadThreshold = (float)GetParam(scenario.Parameters, "matterAheadThreshold", 0);

                config.SinkFormationThreshold = (float)GetParam(scenario.Parameters, "sinkFormationThreshold", 0.5);
                config.SinkDrainFraction = (float)GetParam(scenario.Parameters, "sinkDrainFraction", 0);
                config.SinkRecoilFraction = (float)GetParam(scenario.Parameters, "sinkRecoilFraction", 0);

                config.RegionExpansionChance = (float)GetParam(scenario.Parameters, "regionExpansionChance", 0.25);
                config.RegionExpansionCost = (float)GetParam(scenario.Parameters, "regionExpansionCost", 0.05);
                config.RegionExpansionMinSource = (float)GetParam(scenario.Parameters, "regionExpansionMinSource", 0.1);
                config.RegionExpansionRequiresViability = GetParam(scenario.Parameters, "regionExpansionRequiresViability", 0) > 0.5;
                config.RegionExpansionSeedsResource = GetParam(scenario.Parameters, "regionExpansionSeedsResource", 1) > 0.5;
                config.RegionSeedResource = (float)GetParam(scenario.Parameters, "regionSeedResource", 0.1);

                config.ComplexityGainFromGradient = (float)GetParam(scenario.Parameters, "complexityGainFromGradient", 0.02);
                config.ComplexityGainNearSink = (float)GetParam(scenario.Parameters, "complexityGainNearSink", 0.05);
                config.ComplexityViabilityGainA = (float)GetParam(scenario.Parameters, "complexityViabilityGainA", 0.5);
                config.ComplexityViabilityGainK = (float)GetParam(scenario.Parameters, "complexityViabilityGainK", 1.0);
            }

            config.ShowComplexityTint = false;
            config.ViabilityColorScale = 40f;

            return config;
        }

        /// <summary>
        /// Helper to get parameter with default fallback.
        /// </summary>
        private static double GetParam(Dictionary<string, double> parameters, string key, double defaultValue)
        {
            return parameters != null && parameters.ContainsKey(key) ? parameters[key] : defaultValue;
        }

        /// <summary>
        /// Create a StepContext from preset configuration.
        /// </summary>
        public static StepContext CreateStepContext(ScenarioPreset preset, SimulationConfiguration config)
        {
            return new StepContext(
                config,
                preset.InitialResourceGlobal,
                preset.ScaleFactor,
                preset.Seed
            );
        }

        /// <summary>
        /// Create a default RunRequest for headless execution.
        /// Can be customized per use case (export, testing, etc.)
        /// </summary>
        public static RunRequest CreateDefaultRunRequest(int steps = 1000, int sampleEvery = 10)
        {
            return new RunRequest
            {
                Steps = steps,
                SampleEvery = sampleEvery,
                EmitEvents = true
            };
        }
    }
}
