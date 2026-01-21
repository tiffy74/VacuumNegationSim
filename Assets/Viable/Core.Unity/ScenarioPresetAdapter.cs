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
            return new ScenarioDefinition
            {
                ScenarioId = preset.PresetId,
                ScenarioName = preset.PresetName,
                Description = preset.Description,
                GridWidth = preset.GridWidth,
                GridHeight = preset.GridHeight,
                Seed = preset.Seed,
                Parameters = preset.GetParametersDictionary()
            };
        }

        /// <summary>
        /// Build a SimulationConfiguration from the preset's parameters.
        /// This maps parameter keys to configuration fields.
        /// </summary>
        public static SimulationConfiguration ToSimulationConfiguration(ScenarioPreset preset)
        {
            var config = new SimulationConfiguration();

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

            config.ComplexityGainFromGradient = (float)preset.GetParameter("complexityGainFromGradient", 0.02);
            config.ComplexityGainNearSink = (float)preset.GetParameter("complexityGainNearSink", 0.05);
            config.ComplexityViabilityGainA = (float)preset.GetParameter("complexityViabilityGainA", 0.5);
            config.ComplexityViabilityGainK = (float)preset.GetParameter("complexityViabilityGainK", 1.0);

            config.ShowComplexityTint = preset.ShowComplexityTint;
            config.ViabilityColorScale = 40f;

            return config;
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
