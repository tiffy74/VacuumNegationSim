using UnityEngine;
using System.Collections;
using Viable.Engine;
using Viable.Engine.State;
using Viable.Engine.Execution;
using Viable.Engine.Configuration;
using Viable.Engine.Interfaces; // ADDED: For IStepPhase
using Viable.Contracts;
using Viable.Core.Unity.Visuals;
using Viable.Core.Unity.Export; // ADDED: For export system

namespace Viable.Core.Unity.Controllers
{
    /// <summary>
    /// Unity controller for running Viable Engine simulations.
    /// Can run from a ScenarioPreset (productized) or manual Inspector settings (legacy).
    /// Stage 8: Refactored to support preset system while preserving current behavior.
    /// Stage 9: Added export functionality for reproducible research.
    /// </summary>
    public class SimulationController : MonoBehaviour
    {
        // ===== Preset System (Stage 8) =====
        [Header("Preset System")]
        [Tooltip("Load scenario from preset (recommended). Leave null to use legacy Inspector settings.")]
        [SerializeField] private ScenarioPreset scenarioPreset;

        [Tooltip("If true, overrides preset parameters with Inspector values (for tuning)")]
        [SerializeField] private bool allowInspectorOverride = false;

        // ===== Engine Components =====
        private SimulationRunner runner;
        private StepContext context;
        private GridState state;
        private SimulationConfiguration config;

        // Store last run for export (Stage 9)
        private ScenarioDefinition lastScenario;
        private RunRequest lastRequest;
        private RunResult lastResult; // ADDED: Store run result

        // ===== Legacy Inspector Settings (kept for backward compatibility) =====
        [Header("Legacy Settings (used if no preset loaded)")]
        
        [Header("Global Resource Pool")]
        [SerializeField] private float ResourceGlobalMax = 5e7f;
        [SerializeField] private float ResourceGlobal = 1e7f;
        [SerializeField] private float GlobalReplenishPerTick = 200;
        [SerializeField] private float MinResourceForPersistence = 5f;

        [Header("Viability / Threshold")]
        [SerializeField] private float EthreshBase = 0.18f;
        [SerializeField] private float GlobalScarcityK = 0.3f;
        [SerializeField] private float ComplexityPenalty = 0.02f;
        [SerializeField] private float DecayLoss = 0.003f;
        [SerializeField] private float ComplexityViabilityGainA = 0.5f;
        [SerializeField] private float ComplexityViabilityGainK = 1.0f;

        [Header("Propagation")]
        [SerializeField] private float PropagateFrac = 0.25f;
        [SerializeField] private float MinBudgetToPropagate = 0.1f;
        [SerializeField] private float ActivationCost = 0.25f;

        [Header("Complexity Dynamics")]
        [SerializeField] private float ComplexityGainPerUse = 0.2f;
        [SerializeField] private float ComplexityDiffusionRate = 0.2f;
        [SerializeField] private float ComplexityDecay = 0.02f;
        [SerializeField] private float ComplexityGainFromGradient = 0.02f;
        [SerializeField] private float ComplexityGainNearSink = 0.05f;

        [Header("Local Limits")]
        [SerializeField] private float ResourceLocalMax = 5e4f;
        [SerializeField] private float PerturbationProbability = 0.0002f;
        [SerializeField] private float PerturbationComplexity = 0.5f;
        [SerializeField] private float ExpansionRate = 1.0f;

        [Header("Region Expansion")]
        [SerializeField] private float RegionExpansionChance = 0.25f;
        [SerializeField] private float RegionExpansionCost = 0.05f;
        [SerializeField] private float RegionExpansionMinSource = 0.1f;
        [SerializeField] private bool RequireViabilityForRegion = false;
        [SerializeField] private bool SeedResourceOnRegionExpansion = true;
        [SerializeField] private float RegionSeedResource = 0.1f;

        [Header("Sink Regions")]
        [SerializeField] private float SinkFormationThreshold = 0.5f;
        [SerializeField] private float SinkDrainFraction = 0f;
        [SerializeField] private float SinkRecoilFraction = 0f;
        
        [Header("Initial Sink Placement")]
        [Tooltip("Number of sinks to place at simulation start (0 = none, sinks form naturally)")]
        [SerializeField] private int InitialSinkCount = 0;
        [Tooltip("Spacing between sinks in grid pattern")]
        [SerializeField] private float SinkSpacing = 10f;
        [Tooltip("Random offset for sink positions (0 = perfect grid, 1 = fully random)")]
        [SerializeField][Range(0f, 1f)] private float SinkRandomness = 0f;

        [Header("Visualization")]
        [SerializeField] private Color InactiveColor = new Color(0.05f, 0.05f, 0.08f, 1f);
        [SerializeField] private bool ShowComplexityTint = false;
        [SerializeField] private Color DormantRegionColor = new Color(0.15f, 0.0f, 0.25f, 1f);
        [SerializeField] private float ScaleFactor = 1.0f;
        [SerializeField] private float ticksPerSecond = 10f;

        [Header("Render Mode")]
        [SerializeField] private Rendering.RenderMode renderMode = Rendering.RenderMode.Viability;

        // ===== Unity Components =====
        public SimulationGrid Grid;
        public CellVisualiser[,] views;
        private Rendering.GridRenderer gridRenderer;
        private bool running = false; // CHANGED: Start paused, wait for user to press Play

        void Start()
        {
            InitializeSimulation();
            
            // Don't auto-start the simulation loop
            // Wait for user to press Play button in UI
            Debug.Log("[SimulationController] Simulation initialized. Press Play button to start.");
        }

        /// <summary>
        /// Initialize simulation from preset or legacy Inspector settings.
        /// </summary>
        private void InitializeSimulation()
        {
            // Get grid reference
            Grid = GetComponent<SimulationGrid>();
            
            // Determine grid size and configuration source
            int gridWidth, gridHeight;
            
            if (scenarioPreset != null)
            {
                Debug.Log($"[SimulationController] Loading from preset: {scenarioPreset.PresetName}");
                
                // Use preset configuration
                gridWidth = scenarioPreset.GridWidth;
                gridHeight = scenarioPreset.GridHeight;
                
                // Build configuration from preset
                config = ScenarioPresetAdapter.ToSimulationConfiguration(scenarioPreset);
                
                // Allow Inspector overrides if enabled
                if (allowInspectorOverride)
                {
                    Debug.LogWarning("[SimulationController] Inspector overrides enabled - using Inspector values where set");
                    ApplyInspectorOverrides(config);
                }
                
                // Update Grid size (if dynamic)
                Grid.Width = gridWidth;
                Grid.Height = gridHeight;

                // Store scenario for export (Stage 9)
                lastScenario = ScenarioPresetAdapter.ToScenarioDefinition(scenarioPreset);
                
                // Create default run request for export
                lastRequest = new RunRequest
                {
                    Steps = 0, // Will be updated as simulation runs
                    SampleEvery = 10,
                    EmitEvents = false
                };
            }
            else
            {
                Debug.Log("[SimulationController] No preset loaded - using legacy Inspector settings");
                
                // Use Grid's current size
                gridWidth = Grid.Width;
                gridHeight = Grid.Height;
                
                // Build configuration from Inspector
                config = BuildLegacyConfig();

                // Create minimal scenario definition for legacy mode
                lastScenario = new ScenarioDefinition
                {
                    ScenarioId = "legacy-inspector-run",
                    ScenarioName = "Legacy Inspector Configuration",
                    Description = "Run using Inspector parameter values",
                    GridWidth = gridWidth,
                    GridHeight = gridHeight,
                    Seed = null,
                    Parameters = new System.Collections.Generic.Dictionary<string, double>()
                };

                // Create default run request
                lastRequest = new RunRequest
                {
                    Steps = 0,
                    SampleEvery = 10,
                    EmitEvents = false
                };
            }

            // Spawn visual cells
            views = new CellVisualiser[gridWidth, gridHeight];
            Grid.SpawnVisualCells(views);

            // Create Engine state
            state = new GridState(gridWidth, gridHeight);
            
            // Create Engine context
            int? seed = scenarioPreset != null ? scenarioPreset.Seed : null;
            float resourceGlobal = scenarioPreset != null ? scenarioPreset.InitialResourceGlobal : ResourceGlobal;
            float scaleFactor = scenarioPreset != null ? scenarioPreset.ScaleFactor : ScaleFactor;
            
            context = new StepContext(config, resourceGlobal, scaleFactor, seed);

            // Initialize state
            InitStateInto(state, context);

            // Stage 13.3: Create phases using phase set configuration
            // For now, always use default pipeline (SimulationStepper)
            // Future stages will use PhaseFactory when assembly issues resolved
            string phaseSetId = lastScenario.EngineConfig?.PhaseSetId ?? "default";
            var phases = BuildPhasePipeline(phaseSetId);

            // Create Engine runner with configured phases
            runner = new SimulationRunner(state, phases);

            // Setup renderer
            Color inactiveColor = scenarioPreset != null ? scenarioPreset.InactiveColor : InactiveColor;
            Color dormantColor = scenarioPreset != null ? scenarioPreset.DormantRegionColor : DormantRegionColor;
            bool showComplexity = scenarioPreset != null ? scenarioPreset.ShowComplexityTint : ShowComplexityTint;
            
            gridRenderer = new Rendering.GridRenderer(
                gridWidth, gridHeight, views,
                inactiveColor, dormantColor, showComplexity
            );
            gridRenderer.ViabilityColorScale = 40f;

            // CHANGED: Don't auto-start the simulation loop
            // User must press Play button in UI to start
            // StartCoroutine(SimLoop()); // REMOVED
            
            Debug.Log("[SimulationController] Ready. Waiting for Play button.");
        }

        /// <summary>
        /// Build configuration from legacy Inspector settings (pre-Stage 8 behavior).
        /// </summary>
        private SimulationConfiguration BuildLegacyConfig()
        {
            return new SimulationConfiguration
            {
                ResourceGlobalMax = ResourceGlobalMax,
                GlobalReplenishPerTick = GlobalReplenishPerTick,
                MinResourceForPersistence = MinResourceForPersistence,

                EthreshBase = EthreshBase,
                GlobalScarcityK = GlobalScarcityK,
                ComplexityPenalty = ComplexityPenalty,
                DecayLoss = DecayLoss,

                PropagateFrac = PropagateFrac,
                MinBudgetToPropagate = MinBudgetToPropagate,
                ActivationCost = ActivationCost,

                ComplexityGainPerUse = ComplexityGainPerUse,
                ComplexityDiffusionRate = ComplexityDiffusionRate,
                ComplexityDecay = ComplexityDecay,

                ResourceLocalMax = ResourceGlobalMax,
                PerturbationProbability = PerturbationProbability,
                PerturbationComplexity = PerturbationComplexity,

                ExpansionRate = ExpansionRate,
                MatterAheadThreshold = 0f,

                SinkFormationThreshold = SinkFormationThreshold,
                SinkDrainFraction = SinkDrainFraction,
                SinkRecoilFraction = SinkRecoilFraction,

                RegionExpansionChance = RegionExpansionChance,
                RegionExpansionCost = RegionExpansionCost,
                RegionExpansionMinSource = RegionExpansionMinSource,
                RegionExpansionRequiresViability = RequireViabilityForRegion,
                RegionExpansionSeedsResource = SeedResourceOnRegionExpansion,
                RegionSeedResource = RegionSeedResource,

                ComplexityGainFromGradient = ComplexityGainFromGradient,
                ComplexityGainNearSink = ComplexityGainNearSink,
                ComplexityViabilityGainA = ComplexityViabilityGainA,
                ComplexityViabilityGainK = ComplexityViabilityGainK,

                ShowComplexityTint = ShowComplexityTint,
                ViabilityColorScale = 40f
            };
        }

        /// <summary>
        /// Apply Inspector overrides to preset configuration (for parameter tuning).
        /// </summary>
        private void ApplyInspectorOverrides(SimulationConfiguration config)
        {
            // Only override if Inspector value differs from default
            // This allows tuning while still using preset as base
            config.DecayLoss = DecayLoss;
            config.EthreshBase = EthreshBase;
            config.GlobalScarcityK = GlobalScarcityK;
            // Add more overrides as needed for tuning
        }

        /// <summary>
        /// Build simulation phase pipeline based on PhaseSetId.
        /// Stage 13.3: For now, always returns default pipeline.
        /// Stage 13.4: Supports InflowMode selection within default pipeline.
        /// Future: Will use PhaseFactory when assembly issues resolved.
        /// </summary>
        private System.Collections.Generic.List<IStepPhase> BuildPhasePipeline(string phaseSetId)
        {
            // Stage 13.4: Check InflowMode to determine pipeline configuration
            var engineConfig = lastScenario.EngineConfig ?? new EngineConfig();
            
            var phases = new System.Collections.Generic.List<IStepPhase>();

            if (engineConfig.InflowMode == Contracts.InflowMode.PointSources)
            {
                // Point sources mode: Use SimulationStepper with point sources
                phases.Add(new SimulationStepper(CountPersistenceConfigurations, engineConfig.PointSources));
            }
            else
            {
                // Default: Uniform inflow (current behavior)
                phases.Add(new SimulationStepper(CountPersistenceConfigurations));
            }

            return phases;
        }

        IEnumerator SimLoop()
        {
            float fps = scenarioPreset != null ? scenarioPreset.TicksPerSecond : ticksPerSecond;
            var delay = new WaitForSeconds(1f / Mathf.Max(1f, fps));
            
            while (running)
            {
                TickSimulation();
                yield return delay;
            }
        }

        public void Play() 
        { 
            running = true; 
            if (!gameObject.activeInHierarchy) return; 
            StartCoroutine(SimLoop()); 
        }
        
        public void Pause() 
        { 
            running = false; 
        }
        
        public void Step() 
        { 
            TickSimulation(); 
        }

        void InitStateInto(GridState s, StepContext ctx)
        {
            s.Reset();

            // Seed central region (5×5 block)
            int cx = s.W / 2;
            int cy = s.H / 2;

            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;

                    if (x < 0 || x >= s.W || y < 0 || y >= s.H)
                        continue;

                    int idx = s.Idx(x, y);

                    s.ResourceLocal[idx] = 50f;
                    s.Active[idx] = 1;
                    s.ActiveRegion[idx] = true;
                    s.RegionActivationTick[idx] = ctx.Tick;
                    s.ResourceFirstTick[idx] = ctx.Tick;
                }
            }

            // Place initial sinks if configured
            if (InitialSinkCount > 0)
            {
                PlaceInitialSinks(s, ctx);
            }

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }

        /// <summary>
        /// Place initial sinks in grid or random pattern.
        /// Stage 13.5: Allows user to control sink placement.
        /// </summary>
        private void PlaceInitialSinks(GridState s, StepContext ctx)
        {
            int sinksPlaced = 0;
            
            // Get seed from preset or use random
            int seed = (scenarioPreset != null && scenarioPreset.Seed.HasValue) 
                ? scenarioPreset.Seed.Value 
                : System.Environment.TickCount;
            
            if (SinkRandomness >= 0.99f)
            {
                // Fully random placement
                var random = new System.Random(seed);
                
                while (sinksPlaced < InitialSinkCount)
                {
                    int x = random.Next(0, s.W);
                    int y = random.Next(0, s.H);
                    int idx = s.Idx(x, y);
                    
                    // Don't place on central seed region
                    int cx = s.W / 2;
                    int cy = s.H / 2;
                    if (Mathf.Abs(x - cx) <= 3 && Mathf.Abs(y - cy) <= 3)
                        continue;
                    
                    // Don't place on existing sink
                    if (s.IsSink[idx])
                        continue;
                    
                    s.IsSink[idx] = true;
                    sinksPlaced++;
                    
                    Debug.Log($"[SimulationController] Placed random sink {sinksPlaced} at ({x}, {y})");
                }
            }
            else
            {
                // Grid pattern with optional randomness
                int sinksPerSide = Mathf.CeilToInt(Mathf.Sqrt(InitialSinkCount));
                float spacing = SinkSpacing > 0 ? SinkSpacing : (s.W / (float)(sinksPerSide + 1));
                
                var random = new System.Random(seed);
                
                for (int sy = 0; sy < sinksPerSide && sinksPlaced < InitialSinkCount; sy++)
                {
                    for (int sx = 0; sx < sinksPerSide && sinksPlaced < InitialSinkCount; sx++)
                    {
                        // Base grid position
                        float baseX = (sx + 1) * spacing;
                        float baseY = (sy + 1) * spacing;
                        
                        // Add randomness
                        float offsetX = (float)(random.NextDouble() - 0.5) * spacing * SinkRandomness;
                        float offsetY = (float)(random.NextDouble() - 0.5) * spacing * SinkRandomness;
                        
                        int x = Mathf.Clamp((int)(baseX + offsetX), 0, s.W - 1);
                        int y = Mathf.Clamp((int)(baseY + offsetY), 0, s.H - 1);
                        
                        // Don't place on central seed region
                        int cx = s.W / 2;
                        int cy = s.H / 2;
                        if (Mathf.Abs(x - cx) <= 3 && Mathf.Abs(y - cy) <= 3)
                            continue;
                        
                        int idx = s.Idx(x, y);
                        
                        if (!s.IsSink[idx])
                        {
                            s.IsSink[idx] = true;
                            sinksPlaced++;
                            
                            Debug.Log($"[SimulationController] Placed grid sink {sinksPlaced} at ({x}, {y})");
                        }
                    }
                }
            }
            
            Debug.Log($"[SimulationController] Placed {sinksPlaced}/{InitialSinkCount} initial sinks");
        }

        void TickSimulation()
        {
            if (context.Tick % 10 == 0)
                Debug.Log($"[Tick {context.Tick}] TickSimulation start (Engine)");

            // Step simulation
            runner.Step(context);

            // Update lastResult after each step (for export)
            UpdateLastResult();

            // Optional diagnostics
            if (context.Tick % 20 == 0)
            {
                LogDiagnostics();
            }

            LogTickSummary();
            UpdateVisualsFromState();
        }

        /// <summary>
        /// Update lastResult with current simulation state.
        /// Called after each step to keep export data fresh.
        /// </summary>
        private void UpdateLastResult()
        {
            if (lastResult == null)
            {
                lastResult = new RunResult
                {
                    RunId = System.Guid.NewGuid().ToString(),
                    ScenarioId = lastScenario?.ScenarioId ?? "unknown",
                    Metadata = Contracts.EngineMetadata.Current(),
                    Samples = new System.Collections.Generic.List<StateSample>()
                };
            }

            lastResult.StepsExecuted = context.Tick;
            lastResult.FinalTime = context.Tick * context.DeltaTime;
            lastResult.FinalState = state; // Could be expensive - only set if needed
            lastResult.SummaryMetrics = ComputeCurrentMetrics();

            // Collect samples every 10 ticks for export
            if (context.Tick % 10 == 0)
            {
                var sample = new StateSample
                {
                    StepIndex = context.Tick,
                    Time = context.Tick * context.DeltaTime,
                    Metrics = ComputeCurrentMetrics()
                };
                lastResult.Samples.Add(sample);
            }
        }

        /// <summary>
        /// Compute current metrics for export.
        /// </summary>
        private System.Collections.Generic.Dictionary<string, double> ComputeCurrentMetrics()
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

            return new System.Collections.Generic.Dictionary<string, double>
            {
                ["viableCount"] = viableCount,
                ["activeCount"] = activeCount,
                ["sinkCount"] = sinkCount,
                ["avgResource"] = totalResource / state.Len,
                ["avgComplexity"] = totalComplexity / state.Len,
                ["resourceGlobal"] = context.ResourceGlobal
            };
        }

        private void UpdateVisualsFromState()
        {
            if (gridRenderer == null || views == null) return;
            gridRenderer.Render(state, context, renderMode);
        }

        int CountPersistenceConfigurations(int cellIndex)
        {
            if (cellIndex < 0 || cellIndex >= state.Len)
                return 0;

            int count = 0;
            int x = cellIndex % state.W;
            int y = cellIndex / state.W;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int configMask = 0; configMask < 16; configMask++) // FIXED: Renamed from 'config'
            {
                float simulatedResource = state.ResourceLocal[cellIndex];

                for (int n = 0; n < 4; n++)
                {
                    if (((configMask >> n) & 1) == 0) continue; // FIXED: Updated variable name

                    int nx = x + dx[n];
                    int ny = y + dy[n];

                    if (nx < 0 || nx >= state.W || ny < 0 || ny >= state.H)
                        continue;

                    int neighborIdx = state.Idx(nx, ny);
                    simulatedResource += config.PropagateFrac * state.ResourceLocal[neighborIdx] * 0.25f; // FIXED: Now works
                }

                if (simulatedResource > config.MinResourceForPersistence) // FIXED: Now works
                    count++;
            }

            return count;
        }

        public void RestartSimulation()
        {
            InitStateInto(state, context);
        }

        void LogDiagnostics()
        {
            int regionCount = 0;
            int sinkCount = 0;
            int viableCount = 0;
            int resourceCount = 0;

            for (int i = 0; i < state.Len; i++)
            {
                if (state.ActiveRegion[i]) regionCount++;
                if (state.IsSink[i]) sinkCount++;
                if (state.V[i] > 0f) viableCount++;
                if (state.ResourceLocal[i] > config.MinBudgetToPropagate) resourceCount++;
            }

            Debug.Log($"[Tick {context.Tick}] Regions={regionCount} Sinks={sinkCount} Viable={viableCount} Resource>{config.MinBudgetToPropagate}={resourceCount}");
        }

        void LogTickSummary()
        {
            float sumIncoming = 0f;
            float sumViabilityPos = 0f;
            float sumComplexity = 0f;
            int activeCount = 0;
            float vMin = float.PositiveInfinity;
            float vMax = float.NegativeInfinity;
            int vPosCount = 0;

            for (int i = 0; i < state.Len; i++)
            {
                float inc = state.Incoming[i];
                float c = state.ComplexityMetric[i];
                float v = state.V[i];

                sumIncoming += inc;
                sumComplexity += c;

                if (state.Active[i] == 1) activeCount++;

                if (!float.IsNaN(v) && !float.IsInfinity(v))
                {
                    if (v > 0f)
                    {
                        vPosCount++;
                        sumViabilityPos += v;
                    }

                    if (v < vMin) vMin = v;
                    if (v > vMax) vMax = v;
                }
            }

            float avgIncoming = sumIncoming / state.Len;
            float avgComplexity = sumComplexity / state.Len;
            float avgVposAllCells = sumViabilityPos / state.Len;

            if (vMin == float.PositiveInfinity) vMin = float.NaN;
            if (vMax == float.NegativeInfinity) vMax = float.NaN;

            Debug.Log(
                $"Tick {context.Tick} | AvgIn={avgIncoming:F3} | AvgV+={avgVposAllCells:F3} | AvgC={avgComplexity:F3} | " +
                $"Active={activeCount} | V+={vPosCount} | Vmin={vMin:F3} | Vmax={vMax:F3} | ResourceGlobal={context.ResourceGlobal:F1}");
        }

        // ===== Stage 9 Preparation: Export Access =====

        /// <summary>
        /// Export the last simulation run to disk.
        /// Creates a timestamped export directory with manifest, CSVs, and checksums.
        /// </summary>
        public void ExportLastRun()
        {
            ExportLastRun(RunExportOptions.ForLevel(ExportLevel.Publication));
        }

        /// <summary>
        /// Export the last simulation run with custom options.
        /// </summary>
        public void ExportLastRun(RunExportOptions options)
        {
            if (lastScenario == null)
            {
                Debug.LogWarning("[SimulationController] No scenario to export! Run a simulation first.");
                return;
            }

            if (lastResult == null)
            {
                Debug.LogWarning("[SimulationController] No result to export! Run a simulation first.");
                return;
            }

            try
            {
                var exporter = new RunExporter();
                string exportPath = exporter.Export(lastScenario, lastRequest, lastResult, options);
                
                Debug.Log($"[SimulationController] ? Export complete: {exportPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimulationController] ? Export failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Get last scenario for export (Stage 9).
        /// </summary>
        public ScenarioDefinition GetLastScenario() => lastScenario;

        /// <summary>
        /// Get last run result for export (Stage 9).
        /// </summary>
        public RunResult GetLastResult() => lastResult;

        [ContextMenu("Export Current Run")]
        public void ExportCurrentRun()
        {
            ExportLastRun();
        }

        // ===== Stage 12: UI Support Methods =====

        /// <summary>
        /// Get current simulation tick for UI display.
        /// </summary>
        public int GetCurrentTick()
        {
            return context != null ? context.Tick : 0;
        }

        /// <summary>
        /// Get current grid state for UI metrics.
        /// </summary>
        public GridState GetCurrentState()
        {
            return state;
        }

        /// <summary>
        /// Get current step context for UI access.
        /// </summary>
        public StepContext GetCurrentContext()
        {
            return context;
        }

        /// <summary>
        /// Get current summary metrics for UI display.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, double> GetCurrentMetrics()
        {
            return ComputeCurrentMetrics();
        }

        /// <summary>
        /// Load a preset at runtime and restart simulation.
        /// Stage 12: Enables UI preset switching.
        /// </summary>
        public void LoadPreset(ScenarioPreset preset)
        {
            if (preset == null)
            {
                Debug.LogWarning("[SimulationController] Cannot load null preset");
                return;
            }

            scenarioPreset = preset;
            Debug.Log($"[SimulationController] Loading preset: {preset.PresetName}");

            // Pause current simulation
            Pause();

            // Stop existing coroutine if running
            StopAllCoroutines();

            // Reinitialize with new preset
            InitializeSimulation();

            Debug.Log($"[SimulationController] Preset loaded: {preset.PresetName}");
        }

        /// <summary>
        /// Restart simulation with a new ScenarioDefinition and RunRequest.
        /// CRITICAL: This is the ONLY entry point for UI-driven restarts.
        /// Called by: SimulationUIOrchestrator.ApplyAndRestart()
        /// </summary>
        public void RestartWithScenario(ScenarioDefinition scenario, RunRequest request)
        {
            if (scenario == null)
            {
                Debug.LogError("[SimulationController] Cannot restart with null scenario");
                return;
            }

            Debug.Log($"[SimulationController] RestartWithScenario: {scenario.ScenarioId}");

            // Stop current simulation
            Pause();
            StopAllCoroutines();

            // Store for export
            lastScenario = scenario;
            lastRequest = request;
            lastResult = null; // Reset result

            // Update grid size if changed
            int gridWidth = scenario.GridWidth;
            int gridHeight = scenario.GridHeight;

            // Check if grid size changed
            bool gridSizeChanged = (state != null && (state.W != gridWidth || state.H != gridHeight));

            if (gridSizeChanged)
            {
                Debug.Log($"[SimulationController] Grid size changed: {gridWidth}×{gridHeight}");
                Grid.Width = gridWidth;
                Grid.Height = gridHeight;

                // Respawn visual cells
                views = new CellVisualiser[gridWidth, gridHeight];
                Grid.SpawnVisualCells(views);
            }

            // Build SimulationConfiguration from ScenarioDefinition
            config = ScenarioPresetAdapter.ToSimulationConfiguration(scenario);

            // Create new GridState
            state = new GridState(gridWidth, gridHeight);

            // Create new StepContext
            float resourceGlobal = scenario.Parameters != null && scenario.Parameters.ContainsKey("resourceGlobalMax")
                ? (float)scenario.Parameters["resourceGlobalMax"] * 0.2f // Start at 20% of max
                : config.ResourceGlobalMax * 0.2f;

            context = new StepContext(config, resourceGlobal, ScaleFactor, scenario.Seed);

            // Initialize state
            InitStateInto(state, context);

            // Rebuild phase pipeline with new configuration
            string phaseSetId = scenario.EngineConfig?.PhaseSetId ?? "default";
            var phases = BuildPhasePipeline(phaseSetId);

            // Create new runner
            runner = new SimulationRunner(state, phases);

            // Recreate renderer if grid size changed
            if (gridSizeChanged || gridRenderer == null)
            {
                gridRenderer = new Rendering.GridRenderer(
                    gridWidth, gridHeight, views,
                    InactiveColor, DormantRegionColor, ShowComplexityTint
                );
                gridRenderer.ViabilityColorScale = 40f;
            }

            Debug.Log($"[SimulationController] Restart complete. Ready to play.");
        }

        /// <summary>
        /// Export and return the export path for UI feedback.
        /// Stage 12: Enables UI to show export location.
        /// </summary>
        public string ExportLastRunWithPath()
        {
            return ExportLastRunWithPath(RunExportOptions.ForLevel(ExportLevel.Publication));
        }

        /// <summary>
        /// Export with options and return path.
        /// </summary>
        public string ExportLastRunWithPath(RunExportOptions options)
        {
            if (lastScenario == null || lastResult == null)
            {
                Debug.LogWarning("[SimulationController] No run to export");
                return null;
            }

            try
            {
                var exporter = new RunExporter();
                string exportPath = exporter.Export(lastScenario, lastRequest, lastResult, options);
                Debug.Log($"[SimulationController] ? Export complete: {exportPath}");
                return exportPath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimulationController] ? Export failed: {ex.Message}");
                return null;
            }
        }
    }
}
