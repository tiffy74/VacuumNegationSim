using System;
using System.Collections.Generic;
using System.Diagnostics;
using Viable.Contracts;
using Viable.Engine.Interfaces;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Viable.Engine
{
    /// <summary>
    /// Main simulation runner that orchestrates step execution and result collection.
    /// Unity-free, deterministic, and suitable for batch execution.
    /// </summary>
    public sealed class SimulationRunner
    {
        private readonly List<IStepPhase> _phases;
        public GridState State { get; }

        public SimulationRunner(GridState state, IEnumerable<IStepPhase> phases)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            _phases = new List<IStepPhase>(phases ?? throw new ArgumentNullException(nameof(phases)));
        }

        /// <summary>
        /// Execute a single simulation step.
        /// </summary>
        /// <param name="context">Step context with configuration and RNG</param>
        public void Step(StepContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            for (int i = 0; i < _phases.Count; i++)
                _phases[i].Execute(State, context);

            context.Tick++;
        }

        /// <summary>
        /// Execute N simulation steps.
        /// </summary>
        /// <param name="context">Step context</param>
        /// <param name="stepCount">Number of steps to execute</param>
        public void StepN(StepContext context, int stepCount)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (stepCount <= 0) throw new ArgumentOutOfRangeException(nameof(stepCount));

            for (int i = 0; i < stepCount; i++)
                Step(context);
        }

        /// <summary>
        /// Run a complete simulation from scenario definition to result.
        /// This is the main entry point for headless execution.
        /// </summary>
        /// <param name="scenario">Scenario definition with parameters and initial state</param>
        /// <param name="request">Run request with step count and sampling configuration</param>
        /// <returns>Complete run result with samples, events, and metrics</returns>
        public RunResult Run(ScenarioDefinition scenario, RunRequest request)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var stopwatch = Stopwatch.StartNew();

            // Build configuration from scenario parameters
            var config = BuildConfigurationFromScenario(scenario);

            // Create execution context
            var context = StepContext.FromScenario(scenario, config);

            // Initialize result
            var result = new RunResult
            {
                ScenarioId = scenario.ScenarioId,
                Metadata = EngineMetadata.Current()
            };

            // Stage 13.2: Populate value semantics for interpretable outputs
            PopulateValueSemantics(result);

            // Execute simulation
            int stepsToExecute = request.Steps;
            int sampleInterval = Math.Max(1, request.SampleEvery);

            for (int i = 0; i < stepsToExecute; i++)
            {
                Step(context);

                // Sample state at specified intervals
                if (request.EmitEvents && (i % sampleInterval == 0 || i == stepsToExecute - 1))
                {
                    var sample = CaptureSample(context, State);
                    result.Samples.Add(sample);
                }

                // TODO: Check stop conditions
            }

            result.StepsExecuted = context.Tick;
            result.FinalTime = context.Tick * context.DeltaTime;
            result.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            result.FinalState = State; // Could serialize to JSON here
            result.SummaryMetrics = ComputeSummaryMetrics(State, context);

            stopwatch.Stop();
            return result;
        }

        /// <summary>
        /// Populate value semantics dictionary describing metric meanings.
        /// Stage 13.2: Enables interpretable outputs without physical units.
        /// </summary>
        private void PopulateValueSemantics(RunResult result)
        {
            result.ValueSemantics["viableCount"] = "Count";
            result.ValueSemantics["activeCount"] = "Count";
            result.ValueSemantics["sinkCount"] = "Count";
            result.ValueSemantics["avgResource"] = "Quantity (Q)";
            result.ValueSemantics["avgComplexity"] = "Index (dimensionless)";
            result.ValueSemantics["resourceGlobal"] = "Quantity (Q)";
            
            // Add semantics for common scenario parameters that might appear in metrics
            result.ValueSemantics["decayLoss"] = "Rate (Q/step)";
            result.ValueSemantics["maintenanceCost"] = "Cost (Q/step)";
            result.ValueSemantics["inflow"] = "Rate (Q/step)";
            result.ValueSemantics["outflow"] = "Rate (Q/step)";
            result.ValueSemantics["viability"] = "Index (dimensionless)";
        }

        /// <summary>
        /// Build SimulationConfiguration from ScenarioDefinition parameters.
        /// Stage 13: Safely reads EngineConfig with defaults preserving current behavior.
        /// </summary>
        private Configuration.SimulationConfiguration BuildConfigurationFromScenario(ScenarioDefinition scenario)
        {
            var config = new Configuration.SimulationConfiguration();

            // Map scenario parameters to configuration fields
            // This is a simplified version - a real implementation would map all parameters
            if (scenario.Parameters.TryGetValue("resourceGlobalMax", out double rgm))
                config.ResourceGlobalMax = (float)rgm;
            if (scenario.Parameters.TryGetValue("decayLoss", out double dl))
                config.DecayLoss = (float)dl;
            // TODO: Map remaining parameters

            // Stage 13: Read EngineConfig if present, otherwise use defaults
            // Defaults preserve current behavior exactly
            var engineConfig = scenario.EngineConfig ?? new EngineConfig();
            
            // Store mechanism selectors in config (no behavior change yet)
            // These will be read by PhaseFactory in future stages
            config.InflowMode = engineConfig.InflowMode;
            config.OutflowMode = engineConfig.OutflowMode;
            config.DiffusionMode = engineConfig.DiffusionMode;
            config.BoundaryMode = engineConfig.BoundaryMode;
            config.ViabilityRule = engineConfig.ViabilityRule;
            config.TopologyMode = engineConfig.TopologyMode;
            config.MaskShape = engineConfig.MaskShape;
            config.RefinementMode = engineConfig.RefinementMode;

            return config;
        }

        /// <summary>
        /// Capture a state sample with metrics.
        /// </summary>
        private StateSample CaptureSample(StepContext context, GridState state)
        {
            return new StateSample
            {
                StepIndex = context.Tick,
                Time = context.Tick * context.DeltaTime,
                State = null, // Could serialize state here
                Metrics = ComputeCurrentMetrics(state, context)
            };
        }

        /// <summary>
        /// Compute current metrics for a sample.
        /// </summary>
        private Dictionary<string, double> ComputeCurrentMetrics(GridState state, StepContext context)
        {
            int viableCount = 0;
            int activeCount = 0;
            int sinkCount = 0;
            double totalResource = 0;
            double totalComplexity = 0;

            for (int i = 0; i < state.Len; i++)
            {
                if (state.V[i] > 0f) viableCount++;
                if (state.Active[i] == 1) activeCount++;
                if (state.IsSink[i]) sinkCount++;
                totalResource += state.ResourceLocal[i];
                totalComplexity += state.ComplexityMetric[i];
            }

            return new Dictionary<string, double>
            {
                ["viableCount"] = viableCount,
                ["activeCount"] = activeCount,
                ["sinkCount"] = sinkCount,
                ["avgResource"] = totalResource / state.Len,
                ["avgComplexity"] = totalComplexity / state.Len,
                ["resourceGlobal"] = context.ResourceGlobal
            };
        }

        /// <summary>
        /// Compute summary metrics over the entire run.
        /// </summary>
        private Dictionary<string, double> ComputeSummaryMetrics(GridState state, StepContext context)
        {
            // Reuse current metrics as final summary
            // In a real implementation, this would aggregate over all samples
            return ComputeCurrentMetrics(state, context);
        }
    }
}


