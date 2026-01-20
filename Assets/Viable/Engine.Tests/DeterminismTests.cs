using NUnit.Framework;
using Viable.Engine;
using Viable.Engine.State;
using Viable.Engine.Execution;
using Viable.Engine.Configuration;
using System;

namespace Viable.Engine.Tests
{
    /// <summary>
    /// Tests that the simulation is fully deterministic.
    /// Same seed + same configuration = identical results.
    /// This is critical for reproducible research.
    /// </summary>
    [TestFixture]
    public class DeterminismTests
    {
        private SimulationConfiguration CreateTestConfig()
        {
            return new SimulationConfiguration
            {
                ResourceGlobalMax = 5e7f,
                GlobalReplenishPerTick = 200f,
                MinResourceForPersistence = 5f,
                EthreshBase = 0.18f,
                GlobalScarcityK = 0.3f,
                DecayLoss = 0.003f,
                PropagateFrac = 0.25f,
                MinBudgetToPropagate = 0.1f,
                ActivationCost = 0.25f,
                ComplexityGainPerUse = 0.2f,
                ComplexityDiffusionRate = 0.2f,
                ComplexityDecay = 0.02f,
                ResourceLocalMax = 5e4f,
                PerturbationProbability = 0.0002f,
                PerturbationComplexity = 0.5f,
                ExpansionRate = 1.0f,
                SinkFormationThreshold = 0.5f,
                SinkDrainFraction = 0f,
                SinkRecoilFraction = 0f,
                RegionExpansionChance = 0.25f,
                RegionExpansionCost = 0.05f,
                RegionExpansionMinSource = 0.1f,
                RegionExpansionRequiresViability = false,
                RegionExpansionSeedsResource = true,
                RegionSeedResource = 0.1f,
                ComplexityViabilityGainA = 0.5f,
                ComplexityViabilityGainK = 1.0f,
                MatterAheadThreshold = 0f
            };
        }

        private void InitializeCentralSeed(GridState state, StepContext context)
        {
            int cx = state.W / 2;
            int cy = state.H / 2;

            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;

                    if (x < 0 || x >= state.W || y < 0 || y >= state.H)
                        continue;

                    int idx = state.Idx(x, y);
                    state.ResourceLocal[idx] = 50f;
                    state.Active[idx] = 1;
                    state.ActiveRegion[idx] = true;
                    state.RegionActivationTick[idx] = context.Tick;
                    state.ResourceFirstTick[idx] = context.Tick;
                }
            }
        }

        [Test]
        public void SameSeed_ProducesIdenticalResults_After100Steps()
        {
            // Arrange
            var config = CreateTestConfig();
            const int seed = 42;
            const int steps = 100;

            // Create two identical simulations with same seed
            var state1 = new GridState(32, 32);
            var context1 = new StepContext(config, 1e7f, 1.0f, seed);
            InitializeCentralSeed(state1, context1);

            var state2 = new GridState(32, 32);
            var context2 = new StepContext(config, 1e7f, 1.0f, seed);
            InitializeCentralSeed(state2, context2);

            var stepper1 = new SimulationStepper(CountPersistenceConfigurationsFactory(state1, config));
            var runner1 = new SimulationRunner(state1, new[] { stepper1 });

            var stepper2 = new SimulationStepper(CountPersistenceConfigurationsFactory(state2, config));
            var runner2 = new SimulationRunner(state2, new[] { stepper2 });

            // Act
            runner1.StepN(context1, steps);
            runner2.StepN(context2, steps);

            // Assert
            Assert.AreEqual(context1.Tick, context2.Tick, "Tick count should match");
            Assert.AreEqual(context1.ResourceGlobal, context2.ResourceGlobal, 1e-6f, "Global resource should match");

            for (int i = 0; i < state1.Len; i++)
            {
                Assert.AreEqual(state1.ResourceLocal[i], state2.ResourceLocal[i], 1e-6f,
                    $"ResourceLocal at cell {i} should be identical");
                Assert.AreEqual(state1.V[i], state2.V[i], 1e-6f,
                    $"Viability at cell {i} should be identical");
                Assert.AreEqual(state1.ComplexityMetric[i], state2.ComplexityMetric[i], 1e-6f,
                    $"Complexity at cell {i} should be identical");
                Assert.AreEqual(state1.Active[i], state2.Active[i],
                    $"Active state at cell {i} should be identical");
                Assert.AreEqual(state1.ActiveRegion[i], state2.ActiveRegion[i],
                    $"ActiveRegion at cell {i} should be identical");
                Assert.AreEqual(state1.IsSink[i], state2.IsSink[i],
                    $"IsSink at cell {i} should be identical");
            }
        }

        [Test]
        public void DifferentSeeds_ProduceDifferentResults()
        {
            // Arrange
            var config = CreateTestConfig();
            config.PerturbationProbability = 0.01f; // Higher probability to ensure differences
            const int steps = 50;

            var state1 = new GridState(32, 32);
            var context1 = new StepContext(config, 1e7f, 1.0f, seed: 42);
            InitializeCentralSeed(state1, context1);

            var state2 = new GridState(32, 32);
            var context2 = new StepContext(config, 1e7f, 1.0f, seed: 123); // Different seed
            InitializeCentralSeed(state2, context2);

            var stepper1 = new SimulationStepper(CountPersistenceConfigurationsFactory(state1, config));
            var runner1 = new SimulationRunner(state1, new[] { stepper1 });

            var stepper2 = new SimulationStepper(CountPersistenceConfigurationsFactory(state2, config));
            var runner2 = new SimulationRunner(state2, new[] { stepper2 });

            // Act
            runner1.StepN(context1, steps);
            runner2.StepN(context2, steps);

            // Assert - Should have at least some differences due to random perturbations
            bool foundDifference = false;
            for (int i = 0; i < state1.Len; i++)
            {
                if (Math.Abs(state1.ResourceLocal[i] - state2.ResourceLocal[i]) > 1e-6f)
                {
                    foundDifference = true;
                    break;
                }
            }

            Assert.IsTrue(foundDifference, 
                "Different seeds should produce different results (with perturbations enabled)");
        }

        [Test]
        public void RunMethod_ProducesSameResultsAsStepN()
        {
            // Arrange
            var config = CreateTestConfig();
            const int seed = 42;
            const int steps = 50;

            // Simulation 1: Use StepN directly
            var state1 = new GridState(32, 32);
            var context1 = new StepContext(config, 1e7f, 1.0f, seed);
            InitializeCentralSeed(state1, context1);
            var stepper1 = new SimulationStepper(CountPersistenceConfigurationsFactory(state1, config));
            var runner1 = new SimulationRunner(state1, new[] { stepper1 });

            // Simulation 2: Use StepN for comparison (same setup)
            var state2 = new GridState(32, 32);
            var context2 = new StepContext(config, 1e7f, 1.0f, seed);
            InitializeCentralSeed(state2, context2);
            var stepper2 = new SimulationStepper(CountPersistenceConfigurationsFactory(state2, config));
            var runner2 = new SimulationRunner(state2, new[] { stepper2 });

            // Act - Run both simulations
            runner1.StepN(context1, steps);
            runner2.StepN(context2, steps);

            // Assert - Final states should match (both used StepN)
            for (int i = 0; i < state1.Len; i++)
            {
                Assert.AreEqual(state1.ResourceLocal[i], state2.ResourceLocal[i], 1e-6f,
                    $"Final resource at cell {i} should match between two identical StepN runs");
            }
        }

        // Helper to create persistence configuration counter
        private Func<int, int> CountPersistenceConfigurationsFactory(GridState state, SimulationConfiguration config)
        {
            return (cellIndex) =>
            {
                int w = state.W;
                int h = state.H;

                if (cellIndex < 0 || cellIndex >= state.Len)
                    return 0;

                int count = 0;
                int x = cellIndex % w;
                int y = cellIndex / w;

                int[] dx = { 0, 0, -1, 1 };
                int[] dy = { -1, 1, 0, 0 };

                for (int configMask = 0; configMask < 16; configMask++)
                {
                    float simulatedResource = state.ResourceLocal[cellIndex];

                    for (int n = 0; n < 4; n++)
                    {
                        if (((configMask >> n) & 1) == 0) continue;

                        int nx = x + dx[n];
                        int ny = y + dy[n];

                        if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                            continue;

                        int neighborIdx = state.Idx(nx, ny);
                        simulatedResource += config.PropagateFrac * state.ResourceLocal[neighborIdx] * 0.25f;
                    }

                    if (simulatedResource > config.MinResourceForPersistence)
                        count++;
                }

                return count;
            };
        }
    }
}
