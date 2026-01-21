using UnityEngine;
using System.Collections;
using Viable.Engine;
using Viable.Engine.State;
using Viable.Engine.Execution;
using Viable.Engine.Configuration;
using Viable.Contracts;
using Viable.Core.Unity.Visuals;

namespace Viable.Core.Unity.Controllers
{
    /// <summary>
    /// Unity controller for running Viable Engine simulations.
    /// Can run from a ScenarioPreset (productized) or manual Inspector settings (legacy).
    /// Stage 8: Refactored to support preset system while preserving current behavior.
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
        private bool running = true;

        void Start()
        {
            InitializeSimulation();
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
            }
            else
            {
                Debug.Log("[SimulationController] No preset loaded - using legacy Inspector settings");
                
                // Use Grid's current size
                gridWidth = Grid.Width;
                gridHeight = Grid.Height;
                
                // Build configuration from Inspector
                config = BuildLegacyConfig();
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

            // Create Engine runner
            var stepper = new SimulationStepper(CountPersistenceConfigurations);
            runner = new SimulationRunner(state, new[] { stepper });

            // Setup renderer
            Color inactiveColor = scenarioPreset != null ? scenarioPreset.InactiveColor : InactiveColor;
            Color dormantColor = scenarioPreset != null ? scenarioPreset.DormantRegionColor : DormantRegionColor;
            bool showComplexity = scenarioPreset != null ? scenarioPreset.ShowComplexityTint : ShowComplexityTint;
            
            gridRenderer = new Rendering.GridRenderer(
                gridWidth, gridHeight, views,
                inactiveColor, dormantColor, showComplexity
            );
            gridRenderer.ViabilityColorScale = 40f;

            // Store scenario for export (Stage 9)
            if (scenarioPreset != null)
            {
                lastScenario = ScenarioPresetAdapter.ToScenarioDefinition(scenarioPreset);
            }

            StartCoroutine(SimLoop());
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

                ResourceLocalMax = ResourceLocalMax,
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

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }

        void TickSimulation()
        {
            if (context.Tick % 10 == 0)
                Debug.Log($"[Tick {context.Tick}] TickSimulation start (Engine)");

            // Step simulation
            runner.Step(context);

            // Optional diagnostics
            if (context.Tick % 20 == 0)
            {
                LogDiagnostics();
            }

            LogTickSummary();
            UpdateVisualsFromState();
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
        /// Get last scenario for export (Stage 9).
        /// </summary>
        public ScenarioDefinition GetLastScenario() => lastScenario;
    }
}
