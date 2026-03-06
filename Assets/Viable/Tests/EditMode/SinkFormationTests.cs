using System;
using NUnit.Framework;
using Viable.Engine;
using Viable.Engine.Configuration;
using Viable.Engine.Execution;
using Viable.Engine.State;

public sealed class SinkFormationTests
{
    [Test]
    public void NoSinks_WhenThresholdUnreachable()
    {
        var config = CreateConfig();

        var state = new GridState(16, 16);
        var context = new StepContext(
            config,
            initialGlobal: config.ResourceGlobalMax,
            initialScale: 1f,
            seed: 42,
            topology: config.TopologyMode,
            adjacency: config.AdjacencyMode);

        var runner = new SimulationRunner(state, new[] { new SimulationStepper(_ => 0) });
        runner.Step(context);

        Assert.IsFalse(Array.Exists(state.IsSink, s => s), "Sinks should never be created.");
    }

    private static SimulationConfiguration CreateConfig()
    {
        return new SimulationConfiguration
        {
            SinkFormationThreshold = float.PositiveInfinity,
            InitialSinkCount = 0,
            ResourceGlobalMax = 1e6f,
            GlobalReplenishPerTick = 0f,
            MinResourceForPersistence = 0.1f,
            EthreshBase = 0.2f,
            GlobalScarcityK = 0.1f,
            ComplexityPenalty = 0.01f,
            DecayLoss = 0.01f,
            PropagateFrac = 0.25f,
            MinBudgetToPropagate = 0.1f,
            ActivationCost = 0.1f,
            ComplexityGainPerUse = 0.05f,
            ComplexityDiffusionRate = 0.05f,
            ComplexityDecay = 0.01f,
            ResourceLocalMax = 1000f,
            PerturbationProbability = 0f,
            PerturbationComplexity = 0f,
            ExpansionRate = 1f,
            MatterAheadThreshold = 0f,
            RegionExpansionChance = 0f,
            RegionExpansionCost = 0f,
            RegionExpansionMinSource = 0f,
            RegionExpansionRequiresViability = false,
            RegionExpansionSeedsResource = false,
            RegionSeedResource = 0f,
            ComplexityGainFromGradient = 0f,
            ComplexityGainNearSink = 0f,
            ComplexityViabilityGainA = 0f,
            ComplexityViabilityGainK = 0f
        };
    }
}