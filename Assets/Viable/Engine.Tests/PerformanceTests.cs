using NUnit.Framework;
using Viable.Engine;
using Viable.Engine.State;
using Viable.Engine.Execution;
using Viable.Engine.Configuration;
using System.Diagnostics;

namespace Viable.Engine.Tests
{
    /// <summary>
    /// Performance tests to ensure the engine runs efficiently.
    /// These are not strict pass/fail tests, but help track performance regressions.
    /// </summary>
    [TestFixture]
    public class PerformanceTests
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
                PerturbationProbability = 0.0f, // Disable for deterministic benchmarking
                PerturbationComplexity = 0.5f,
                ExpansionRate = 1.0f,
                SinkFormationThreshold = 0.5f,
                RegionExpansionChance = 0.25f,
                RegionExpansionCost = 0.05f,
                RegionExpansionMinSource = 0.1f,
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
                }
            }
        }

        [Test]
        public void Benchmark_SmallGrid_1000Steps()
        {
            // Arrange
            var config = CreateTestConfig();
            var state = new GridState(32, 32);
            var context = new StepContext(config, 1e7f, 1.0f, seed: 42);
            InitializeCentralSeed(state, context);

            var stepper = new SimulationStepper(CountPersistenceConfigurationsFactory(state, config));
            var runner = new SimulationRunner(state, new[] { stepper });

            // Act
            var stopwatch = Stopwatch.StartNew();
            runner.StepN(context, 1000);
            stopwatch.Stop();

            // Assert (informational - log the result)
            double msPerStep = stopwatch.Elapsed.TotalMilliseconds / 1000.0;
            UnityEngine.Debug.Log($"[BENCHMARK] 32x32 grid, 1000 steps: {stopwatch.Elapsed.TotalMilliseconds:F2}ms total, {msPerStep:F4}ms per step");

            // Soft assertion - should complete in reasonable time (< 5 seconds for 1000 steps)
            Assert.Less(stopwatch.Elapsed.TotalSeconds, 5.0, 
                "1000 steps should complete in under 5 seconds on modern hardware");
        }

        [Test]
        public void Benchmark_MediumGrid_100Steps()
        {
            // Arrange
            var config = CreateTestConfig();
            var state = new GridState(64, 64);
            var context = new StepContext(config, 1e7f, 1.0f, seed: 42);
            InitializeCentralSeed(state, context);

            var stepper = new SimulationStepper(CountPersistenceConfigurationsFactory(state, config));
            var runner = new SimulationRunner(state, new[] { stepper });

            // Act
            var stopwatch = Stopwatch.StartNew();
            runner.StepN(context, 100);
            stopwatch.Stop();

            // Assert
            double msPerStep = stopwatch.Elapsed.TotalMilliseconds / 100.0;
            UnityEngine.Debug.Log($"[BENCHMARK] 64x64 grid, 100 steps: {stopwatch.Elapsed.TotalMilliseconds:F2}ms total, {msPerStep:F4}ms per step");

            Assert.Less(stopwatch.Elapsed.TotalSeconds, 2.0, 
                "100 steps on 64x64 grid should complete in under 2 seconds");
        }

        [Test]
        public void Benchmark_SingleStep_MeasureOverhead()
        {
            // Arrange
            var config = CreateTestConfig();
            var state = new GridState(64, 64);
            var context = new StepContext(config, 1e7f, 1.0f, seed: 42);
            InitializeCentralSeed(state, context);

            var stepper = new SimulationStepper(CountPersistenceConfigurationsFactory(state, config));
            var runner = new SimulationRunner(state, new[] { stepper });

            // Warm up
            runner.Step(context);

            // Act - Measure single step after warm-up
            var stopwatch = Stopwatch.StartNew();
            runner.Step(context);
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"[BENCHMARK] Single step (64x64): {stopwatch.Elapsed.TotalMilliseconds:F4}ms");

            Assert.Less(stopwatch.Elapsed.TotalMilliseconds, 50.0, 
                "Single step should complete in under 50ms");
        }

        // Helper factory
        private System.Func<int, int> CountPersistenceConfigurationsFactory(GridState state, SimulationConfiguration config)
        {
            return (cellIndex) =>
            {
                if (cellIndex < 0 || cellIndex >= state.Len) return 0;

                int count = 0;
                int x = cellIndex % state.W;
                int y = cellIndex / state.W;
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
                        if (nx < 0 || nx >= state.W || ny < 0 || ny >= state.H) continue;

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
