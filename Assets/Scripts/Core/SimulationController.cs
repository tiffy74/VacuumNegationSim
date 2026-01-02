using Assets.Scripts.Domain;
using Assets.Scripts.Events;
using Assets.Scripts.Simulation;
using Assets.Scripts.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RenderMode = Assets.Scripts.Unity.RenderMode;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Main Unity Controller for the Vacuum Negation Simulation
    /// 
    /// Bridges Unity lifecycle (MonoBehaviour) with pure simulation engine.
    /// Manages inspector parameters, visualization, and user interaction.
    /// Orchestrates the 4-pass simulation tick via SimulationEngine.
    /// 
    /// Architecture:
    /// - Unity Layer: This controller (input, visualization, coroutines)
    /// - Engine Layer: SimulationEngine (tick orchestration, no Unity deps)
    /// - Domain Layer: GridState, SimContext (pure data structures)
    /// - Events Layer: Pass1-4, BlackHoles, FieldWave (pure simulation logic)
    /// </summary>
    public class SimulationController : MonoBehaviour
    {
        // ============================================================================
        // PRIVATE CONSTANTS
        // ============================================================================
        
        private const int NeighborCount = 4; // 4-way grid connectivity
        private const int PersistenceConfigCount = 16; // 2^4 for 4 neighbors
        private const float MinViabilityEpsilon = 1e-6f; // Prevent division by zero
        private const float BlackHoleEnergyEps = 1e-6f;

        // ============================================================================
        // INSPECTOR PARAMETERS: GLOBAL BUDGET
        // ============================================================================
        
        [Header("Global Budget")]
        [Tooltip("Maximum global energy pool capacity")]
        [SerializeField] public float NGlobalMax = 5e7f;
        
        [Tooltip("Current global energy available for activation costs")]
        [SerializeField] public float NGlobal = 1e7f;
        
        [Tooltip("Energy added to global pool per tick")]
        [SerializeField] public float GlobalReplenishPerTick = 200;
        
        [Tooltip("Minimum energy for a cell to persist")]
        [SerializeField] public float MinEnergyForPersistence = 5f;

        // ============================================================================
        // INSPECTOR PARAMETERS: VIABILITY / THRESHOLD
        // ============================================================================
        
        [Header("Viability / Threshold")]
        [Tooltip("Base energy threshold for persistence")]
        [SerializeField] public float EthreshBase = 0.18f;
        
        [Tooltip("How much global scarcity raises threshold")]
        [SerializeField] public float GlobalScarcityK = 0.3f;
        
        [Tooltip("Entropy penalty factor (currently unused in viability)")]
        [SerializeField] public float EntropyPenalty = 0.02f;
        
        [Tooltip("Baseline energy decay per tick")]
        [SerializeField] public float DecayLoss = 0.003f;
        
        [Tooltip("Entropy boost to viability (A parameter)")]
        [SerializeField] public float EntropyViabilityGainA = 0.5f;
        
        [Tooltip("Entropy boost to viability (K parameter)")]
        [SerializeField] public float EntropyViabilityGainK = 1.0f;

        // ============================================================================
        // INSPECTOR PARAMETERS: PROPAGATION
        // ============================================================================
        
        [Header("Propagation")]
        [Tooltip("Fraction of energy cell sends per tick")]
        [SerializeField] public float PropagateFrac = 0.25f;
        
        [Tooltip("Minimum energy to propagate")]
        [SerializeField] public float MinBudgetToPropagate = 0.1f;
        
        [Tooltip("Global energy cost to activate a cell")]
        [SerializeField] public float ActivationCost = 0.25f;

        // ============================================================================
        // INSPECTOR PARAMETERS: ENTROPY DYNAMICS
        // ============================================================================
        
        [Header("Entropy Dynamics")]
        [Tooltip("Entropy gain per active energy use")]
        [SerializeField] public float EntropyGainPerUse = 0.2f;
        
        [Tooltip("Entropy diffusion rate (Laplacian coefficient)")]
        [SerializeField] public float EntropyDiffuseRate = 0.2f;
        
        [Tooltip("Entropy decay per tick")]
        [SerializeField] public float EntropyDecay = 0.02f;
        
        [Tooltip("Entropy gain from energy gradients")]
        [SerializeField] public float EntropyGainFromGradient = 0.02f;
        
        [Tooltip("Entropy gain near black holes")]
        [SerializeField] public float EntropyGainNearBH = 0.05f;

        // ============================================================================
        // INSPECTOR PARAMETERS: LOCAL LIMITS
        // ============================================================================
        
        [Header("Local Limits")]
        [Tooltip("Maximum energy per cell")]
        [SerializeField] public float NlocalMax = 5e4f;
        
        [Tooltip("Probability of random vacuum event per cell")]
        [SerializeField] public float VacuumEventProbability = 0.0002f;
        
        [Tooltip("Entropy spike from vacuum event")]
        [SerializeField] public float VacuumEventEntropy = 0.5f;
        
        [Tooltip("Scale factor per tick (currently unused)")]
        [SerializeField] public float ExpansionRate = 1.0f;

        // ============================================================================
        // INSPECTOR PARAMETERS: FIELD PROPAGATION
        // ============================================================================
        
        [Header("Field Propagation")]
        [Tooltip("Probability of field expanding per tick")]
        [SerializeField] float FieldAdvanceChance = 0.25f;
        
        [Tooltip("Energy cost to activate configuration space")]
        [SerializeField] float FieldAdvanceCost = 0.05f;
        
        [Tooltip("Min energy in source to expand field")]
        [SerializeField] float FieldAdvanceMinSource = 0.1f;
        
        [Tooltip("Require source to be viable for field expansion")]
        [SerializeField] bool RequireViabilityForField = false;
        
        [Tooltip("Seed minimal energy when field expands")]
        [SerializeField] bool SeedEnergyOnFieldAdvance = true;
        
        [Tooltip("Energy seeded at new field locations")]
        [SerializeField] float FieldSeedEnergy = 0.1f;

        // ============================================================================
        // INSPECTOR PARAMETERS: BLACK HOLES
        // ============================================================================
        
        [Header("Black Hole")]
        [Tooltip("Charge threshold for BH formation")]
        [SerializeField] public float BlackHoleFormThreshold = 0.5f;
        
        [Tooltip("Energy drained per tick (currently disabled)")]
        [SerializeField] public float BlackHoleDrainFrac = 0f;
        
        [Tooltip("Energy recoil/radiation fraction (Hawking-like)")]
        [SerializeField] public float BlackHoleRecoilFrac = 0f;

        // ============================================================================
        // INSPECTOR PARAMETERS: VISUALIZATION
        // ============================================================================
        
        [Header("Colors")]
        [Tooltip("Void color (no config space, no energy)")]
        [SerializeField] public Color VoidColor = new Color(0.05f, 0.05f, 0.08f, 1f);
        
        [Tooltip("Enable entropy color tinting")]
        [SerializeField] public bool ShowEntropyTint = false;
        
        [Tooltip("Vacuum energy color (purple)")]
        [SerializeField] public Color VacuumEnergyColor = new Color(0.48f, 0.25f, 0.52f, 1f);
        
        [Tooltip("Nullspace/dim field color")]
        [SerializeField] public Color NullspaceColor = new Color(0.15f, 0.0f, 0.25f, 1f);
        
        [Tooltip("Initial scale factor (currently unused)")]
        [SerializeField] public float ScaleFactor = 1.0f;
        
        [Tooltip("Spatial threshold K (currently unused)")]
        [SerializeField] public float SpatialThreshK = -0.6f;
        
        [Tooltip("Spatial decay K (currently unused)")]
        [SerializeField] public float SpatialDecayK = 0.5f;

        [Header("Render Mode")]
        [Tooltip("Visualization mode (Viability/Energy/Entropy)")]
        [SerializeField] private RenderMode renderMode = RenderMode.Viability;

        [Tooltip("Simulation ticks per second")]
        [SerializeField] float ticksPerSecond = 10f;

        // ============================================================================
        // PRIVATE FIELDS: ENGINE & STATE
        // ============================================================================
        
        private SimulationEngine engine;
        private SimContext ctx;
        private GridState state;
        private GridRenderer gridRenderer;
        
        private SimulationGrid Grid;
        private CellVisualiser[,] views;
        
        private bool running = true;

        // ============================================================================
        // PRIVATE FIELDS: TRACKING ARRAYS (OWNED BY CONTROLLER)
        // ============================================================================
        
        [NonSerialized] public float[] rNorm;
        [NonSerialized] public int[] FieldFirstTick;
        [NonSerialized] public int[] EnergyFirstTick;

        // ============================================================================
        // COMPUTED PROPERTIES
        // ============================================================================

        /// <summary>
        /// Effective energy threshold adjusted for global scarcity.
        /// Higher scarcity → higher threshold → harder to persist.
        /// </summary>
        private float EthreshEff
        {
            get
            {
                float scarcity = 1f - (ctx.NGlobal / Mathf.Max(1f, NGlobalMax));
                return EthreshBase * (1f + GlobalScarcityK * scarcity);
            }
        }

        // ============================================================================
        // UNITY LIFECYCLE: INITIALIZATION
        // ============================================================================

        /// <summary>
        /// Unity Start: Initialize simulation components and begin simulation loop.
        /// </summary>
        void Start()
        {
            InitializeGridAndVisuals();
            InitializeSimulationState();
            InitializeSimulationEngine();
            InitializeRenderer();
            
            StartCoroutine(SimLoop());
        }

        /// <summary>
        /// Initializes the grid and spawns visual cells.
        /// </summary>
        private void InitializeGridAndVisuals()
        {
            Grid = GetComponent<SimulationGrid>();
            views = new CellVisualiser[Grid.Width, Grid.Height];
            Grid.SpawnVisualCells(views);
        }

        /// <summary>
        /// Initializes pure simulation state (GridState + SimContext).
        /// </summary>
        private void InitializeSimulationState()
        {
            var cfg = BuildConfigFromInspectorParameters();
            
            state = new GridState(Grid.Width, Grid.Height);
            ctx = new SimContext(cfg, NGlobal, ScaleFactor);
            
            InitStateInto(state, ctx);
        }

        /// <summary>
        /// Initializes the simulation engine with 4-pass legacy tick step.
        /// </summary>
        private void InitializeSimulationEngine()
        {
            engine = new SimulationEngine(state, new ISimStep[]
            {
                new LegacyTickStep(
                    ComputeViability,
                    CountPersistenceConfigurations,
                    ExecutePostProcessingPasses
                )
            });
        }

        /// <summary>
        /// Initializes the grid renderer for visualization.
        /// </summary>
        private void InitializeRenderer()
        {
            gridRenderer = new GridRenderer(
                Grid.Width, Grid.Height, views,
                VoidColor,
                NullspaceColor,
                ShowEntropyTint
            );
            gridRenderer.ViabilityColorScale = 40f;
        }

        // ============================================================================
        // UNITY LIFECYCLE: UPDATE
        // ============================================================================

        /// <summary>
        /// Unity Update: Handle input for render mode switching.
        /// </summary>
        void Update()
        {
            HandleRenderModeInput();
        }

        /// <summary>
        /// Handles keyboard input for switching visualization modes.
        /// Currently locked to Viability mode.
        /// </summary>
        private void HandleRenderModeInput()
        {
            if (Keyboard.current == null) return;
            
            renderMode = RenderMode.Viability;
            
            // Disabled multi-mode switching for now
            // Uncomment to enable:
            /*
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                renderMode = RenderMode.Viability;
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
                renderMode = RenderMode.Energy;
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
                renderMode = RenderMode.Entropy;
            */
        }

        // ============================================================================
        // SIMULATION LOOP
        // ============================================================================

        /// <summary>
        /// Main simulation coroutine: ticks simulation at specified rate.
        /// </summary>
        IEnumerator SimLoop()
        {
            var delay = new WaitForSeconds(1f / Mathf.Max(1f, ticksPerSecond));
            
            while (running)
            {
                TickSimulation();
                yield return delay;
            }
        }

        /// <summary>
        /// Executes one simulation tick: engine update, diagnostics, visualization.
        /// </summary>
        void TickSimulation()
        {
            LogTickStart();
            
            engine.Tick(ctx);
            
            LogCellDiagnostics();
            LogRegionDiagnostics();
            LogTickSummary(state, ctx);
            
            UpdateVisualsFromState(state);
        }

        // ============================================================================
        // PUBLIC API: PLAYBACK CONTROLS
        // ============================================================================

        /// <summary>
        /// Resumes simulation loop.
        /// </summary>
        public void Play()
        {
            running = true;
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(SimLoop());
        }

        /// <summary>
        /// Pauses simulation loop.
        /// </summary>
        public void Pause()
        {
            running = false;
        }

        /// <summary>
        /// Executes a single simulation tick (manual step).
        /// </summary>
        public void Step()
        {
            TickSimulation();
        }

        /// <summary>
        /// Restarts simulation from initial state.
        /// </summary>
        public void RestartSimulation()
        {
            InitStateInto(state, ctx);
        }

        // ============================================================================
        // CONFIGURATION BUILDING
        // ============================================================================

        /// <summary>
        /// Builds SimConfig from inspector parameters.
        /// </summary>
        private SimConfig BuildConfigFromInspectorParameters()
        {
            return new SimConfig
            {
                // Global budget
                NGlobalMax = NGlobalMax,
                GlobalReplenishPerTick = GlobalReplenishPerTick,
                MinEnergyForPersistence = MinEnergyForPersistence,

                // Viability
                EthreshBase = EthreshBase,
                GlobalScarcityK = GlobalScarcityK,
                EntropyPenalty = EntropyPenalty,
                DecayLoss = DecayLoss,

                // Propagation
                PropagateFrac = PropagateFrac,
                MinBudgetToPropagate = MinBudgetToPropagate,
                ActivationCost = ActivationCost,

                // Entropy
                EntropyGainPerUse = EntropyGainPerUse,
                EntropyDiffuseRate = EntropyDiffuseRate,
                EntropyDecay = EntropyDecay,

                // Local limits
                NlocalMax = NlocalMax,
                VacuumEventProbability = VacuumEventProbability,
                VacuumEventEntropy = VacuumEventEntropy,
                ExpansionRate = ExpansionRate,

                // Black holes
                BlackHoleFormThreshold = BlackHoleFormThreshold,
                BlackHoleDrainFrac = BlackHoleDrainFrac,
                BlackHoleRecoilFrac = BlackHoleRecoilFrac,

                // Field propagation
                FieldAdvanceChance = FieldAdvanceChance,
                FieldAdvanceCost = FieldAdvanceCost,
                FieldAdvanceMinSource = FieldAdvanceMinSource,
                FieldAdvanceRequiresViability = RequireViabilityForField,
                FieldAdvanceSeedsEnergy = SeedEnergyOnFieldAdvance,
                FieldSeedEnergy = FieldSeedEnergy,

                // Entropy advanced
                EntropyGainFromGradient = EntropyGainFromGradient,
                EntropyGainNearBH = EntropyGainNearBH,
                EntropyViabilityGainA = EntropyViabilityGainA,
                EntropyViabilityGainK = EntropyViabilityGainK,

                // Visualization
                VoidColor = VoidColor,
                NullspaceColor = NullspaceColor,
                ShowEntropyTint = ShowEntropyTint,
                ViabilityColorScale = 40f
            };
        }

        // ============================================================================
        // STATE INITIALIZATION
        // ============================================================================

        /// <summary>
        /// Initializes simulation state with a central seed region.
        /// Seeds a 5x5 block at grid center with initial energy and active field.
        /// </summary>
        void InitStateInto(GridState s, SimContext ctx)
        {
            ClearAllStateArrays(s);
            ResetBlackHoleEntities(s);
            ResetSimulationCounters(ctx);
            SeedCentralEnergyBlock(s, ctx);
            ConfigureUnityPerformance();
        }

        /// <summary>
        /// Clears all per-cell state arrays to default values.
        /// </summary>
        private void ClearAllStateArrays(GridState s)
        {
            int len = s.Len;

            for (int i = 0; i < len; i++)
            {
                s.Nlocal[i] = 0f;
                s.Entropy[i] = 0f;
                s.V[i] = 0f;
                s.Active[i] = 0;
                s.Incoming[i] = 0f;
                s.EntropyNext[i] = 0f;
                s.IsVacuum[i] = false;
                s.FieldPresent[i] = false;
                s.IsBlackHole[i] = false;
                s.BlackHoleCharge[i] = 0f;
                s.BlackHoleId[i] = 0;
                s.ZeroEnergyTicks[i] = 0;
                s.FieldFirstTick[i] = -1;
                s.EnergyFirstTick[i] = -1;
            }
        }

        /// <summary>
        /// Resets black hole entity tracking arrays.
        /// </summary>
        private void ResetBlackHoleEntities(GridState s)
        {
            for (int i = 0; i < s.BlackHoleParent.Length; i++)
                s.BlackHoleParent[i] = 0;

            for (int i = 0; i < s.BlackHoleMass.Length; i++)
                s.BlackHoleMass[i] = 0f;

            s.NextBlackHoleId = 1;
        }

        /// <summary>
        /// Resets simulation tick counter and scale factor.
        /// </summary>
        private void ResetSimulationCounters(SimContext ctx)
        {
            ctx.Tick = 0;
            ctx.ScaleFactor = 1.0f;
        }

        /// <summary>
        /// Seeds a 5x5 central block with initial energy and active field.
        /// This is the "big bang" seed that starts the simulation.
        /// </summary>
        private void SeedCentralEnergyBlock(GridState s, SimContext ctx)
        {
            int cx = s.W / 2;
            int cy = s.H / 2;
            int seedRadius = 2;

            for (int dy = -seedRadius; dy <= seedRadius; dy++)
            {
                for (int dx = -seedRadius; dx <= seedRadius; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;

                    if (x < 0 || x >= s.W || y < 0 || y >= s.H)
                        continue;

                    int idx = s.Idx(x, y);

                    s.Nlocal[idx] = 50f; // Initial seed energy
                    s.Incoming[idx] = 0f;
                    s.Entropy[idx] = 0f;
                    s.Active[idx] = 1;
                    s.FieldPresent[idx] = true;

                    if (s.FieldFirstTick[idx] == -1) s.FieldFirstTick[idx] = ctx.Tick;
                    if (s.EnergyFirstTick[idx] == -1) s.EnergyFirstTick[idx] = ctx.Tick;
                }
            }
        }

        /// <summary>
        /// Configures Unity performance settings for high frame rate.
        /// </summary>
        private void ConfigureUnityPerformance()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }

        // ============================================================================
        // POST-PROCESSING PASSES (CALLBACK FOR ENGINE)
        // ============================================================================

        /// <summary>
        /// Executes post-processing passes: field propagation and black hole growth.
        /// Called by LegacyTickStep after Pass1-4 complete.
        /// </summary>
        private void ExecutePostProcessingPasses()
        {
            PropagateFieldWave();
            GrowBlackHoles();
        }

        /// <summary>
        /// Executes field wave propagation (configuration space expansion).
        /// </summary>
        private void PropagateFieldWave()
        {
            FieldWave.PropagateFieldWave(
                state,
                ctx.Tick,
                FieldAdvanceChance,
                FieldAdvanceCost,
                FieldAdvanceMinSource,
                RequireViabilityForField,
                SeedEnergyOnFieldAdvance,
                FieldSeedEnergy
            );
        }

        /// <summary>
        /// Executes black hole growth (geometric expansion of collapsed regions).
        /// </summary>
        private void GrowBlackHoles()
        {
            BlackHoles.GrowBlackHoles(state);
        }

        // ============================================================================
        // VIABILITY COMPUTATION
        // ============================================================================

        /// <summary>
        /// Computes viability: thermodynamic persistence criterion.
        /// V > 0 → structure persists
        /// V < 0 → structure decays
        /// 
        /// Formula: V = (inflow × entropy_boost - decay) / threshold
        /// Entropy BOOSTS viability (counter-intuitive but represents resilience).
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        float ComputeViability(float incomingFlow, float nlocal, float entropy)
        {
            float gain = 1f + EntropyViabilityGainA * (1f - Mathf.Exp(-EntropyViabilityGainK * Mathf.Max(0f, entropy)));
            return (incomingFlow * gain - DecayLoss) / Mathf.Max(MinViabilityEpsilon, EthreshEff);
        }

        // ============================================================================
        // PERSISTENCE CONFIGURATION COUNTING
        // ============================================================================

        /// <summary>
        /// Counts how many neighbor configurations sustain energy at a cell.
        /// Uses 4-way neighbors (16 total configurations).
        /// Higher count = more resilient structure.
        /// </summary>
        int CountPersistenceConfigurations(int cellIndex)
        {
            int w = state.W;
            int h = state.H;
            int len = state.Len;

            if (cellIndex < 0 || cellIndex >= len)
                return 0;

            int count = 0;
            int x = cellIndex % w;
            int y = cellIndex / w;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            // Test all 16 configurations (2^4)
            for (int config = 0; config < PersistenceConfigCount; config++)
            {
                float simulatedEnergy = state.Nlocal[cellIndex];

                // Sum contributions from "active" neighbors in this configuration
                for (int n = 0; n < NeighborCount; n++)
                {
                    if (((config >> n) & 1) == 0) continue;

                    int nx = x + dx[n];
                    int ny = y + dy[n];

                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        continue;

                    int neighborIdx = state.Idx(nx, ny);
                    simulatedEnergy += PropagateFrac * state.Nlocal[neighborIdx] * 0.25f;
                }

                // Configuration sustains energy if total exceeds persistence threshold
                if (simulatedEnergy > MinEnergyForPersistence)
                    count++;
            }

            return count;
        }

        // ============================================================================
        // VISUALIZATION
        // ============================================================================

        /// <summary>
        /// Updates visual rendering from current simulation state.
        /// </summary>
        private void UpdateVisualsFromState(GridState s)
        {
            if (gridRenderer == null || views == null) return;
            gridRenderer.Render(s, ctx, renderMode);
        }

        // ============================================================================
        // DIAGNOSTICS & LOGGING
        // ============================================================================

        /// <summary>
        /// Logs tick start event (every 10 ticks).
        /// </summary>
        private void LogTickStart()
        {
            if (ctx.Tick % 10 == 0)
                Debug.Log($"[Tick {ctx.Tick}] TickSimulation start");
        }

        /// <summary>
        /// Logs diagnostic info for specific cells (center and frontier).
        /// </summary>
        private void LogCellDiagnostics()
        {
            int cx = state.W / 2;
            int cy = state.H / 2;
            int frontX = Math.Min(state.W - 1, cx + 5);
            int frontY = cy;
            int ci = state.Idx(cx, cy);
            int fi = state.Idx(frontX, frontY);

            Debug.Log(
                $"[Tick {ctx.Tick}] Center: In={state.Incoming[ci]:F4} N={state.Nlocal[ci]:F4} Field={state.FieldPresent[ci]} Active={state.Active[ci]} V={state.V[ci]:F4} | " +
                $"Front({frontX},{frontY}): In={state.Incoming[fi]:F4} N={state.Nlocal[fi]:F4} Field={state.FieldPresent[fi]} Active={state.Active[fi]} V={state.V[fi]:F4}");
        }

        /// <summary>
        /// Logs regional statistics (every 20 ticks).
        /// </summary>
        private void LogRegionDiagnostics()
        {
            if (ctx.Tick % 20 != 0) return;

            int cx = state.W / 2;
            int cy = state.H / 2;

            LogOuterRegionStatistics(cx, cy);
            LogFrontierStatistics(cx, cy);
        }

        /// <summary>
        /// Logs statistics for outer region (excluding seed patch).
        /// </summary>
        private void LogOuterRegionStatistics(int cx, int cy)
        {
            int vPosOuter = 0;
            int inPosOuter = 0;
            float vMinOuter = float.PositiveInfinity;
            float vMaxOuter = float.NegativeInfinity;

            for (int y = 0; y < state.H; y++)
            {
                for (int x = 0; x < state.W; x++)
                {
                    // Skip seed patch
                    if (x >= cx - 2 && x <= cx + 2 && y >= cy - 2 && y <= cy + 2)
                        continue;

                    int idx = state.Idx(x, y);
                    if (state.V[idx] > 0f)
                    {
                        vPosOuter++;
                        if (state.V[idx] < vMinOuter) vMinOuter = state.V[idx];
                        if (state.V[idx] > vMaxOuter) vMaxOuter = state.V[idx];
                    }
                    if (state.Incoming[idx] > 0f) inPosOuter++;
                }
            }

            if (vMinOuter == float.PositiveInfinity) vMinOuter = 0f;
            if (vMaxOuter == float.NegativeInfinity) vMaxOuter = 0f;

            Debug.Log($"[Tick {ctx.Tick}] Outer region: V>0 count={vPosOuter}, Incoming>0 count={inPosOuter}, Vmin={vMinOuter:F4}, Vmax={vMaxOuter:F4}");
        }

        /// <summary>
        /// Logs frontier diagnostics (field boundary, recent arrivals).
        /// </summary>
        private void LogFrontierStatistics(int cx, int cy)
        {
            int fieldCount = 0;
            int fieldArrivals = 0;
            int energyCount = 0;
            int viableCount = 0;
            int blackHoleCount = 0;
            int frontierViable = 0;
            float frontierVmin = float.PositiveInfinity;
            float frontierVmax = float.NegativeInfinity;

            int arrivalTick = ctx.Tick - 1;

            for (int i = 0; i < state.Len; i++)
            {
                if (state.FieldPresent[i])
                {
                    fieldCount++;
                    if (state.FieldFirstTick[i] == arrivalTick || state.FieldFirstTick[i] == ctx.Tick)
                        fieldArrivals++;
                }

                if (state.IsBlackHole[i])
                    blackHoleCount++;

                if (state.Nlocal[i] > MinBudgetToPropagate)
                    energyCount++;
                if (state.V[i] > 0f)
                    viableCount++;

                // Frontier: recent field arrivals (last 5 ticks, excluding seed)
                if (state.FieldPresent[i] && state.FieldFirstTick[i] >= ctx.Tick - 5)
                {
                    int fx = i % state.W;
                    int fy = i / state.W;
                    if (!(fx >= cx - 2 && fx <= cx + 2 && fy >= cy - 2 && fy <= cy + 2))
                    {
                        if (state.V[i] > 0f)
                        {
                            frontierViable++;
                            if (state.V[i] < frontierVmin) frontierVmin = state.V[i];
                            if (state.V[i] > frontierVmax) frontierVmax = state.V[i];
                        }
                    }
                }
            }

            if (frontierVmin == float.PositiveInfinity) frontierVmin = 0f;
            if (frontierVmax == float.NegativeInfinity) frontierVmax = 0f;

            Debug.Log($"[Tick {ctx.Tick}] FieldCount={fieldCount} Arrivals={fieldArrivals} BlackHoles={blackHoleCount} " +
                      $"EnergyCount>{MinBudgetToPropagate}={energyCount} Viable={viableCount} " +
                      $"FrontierViable={frontierViable} FrontierVmin={frontierVmin:F3} FrontierVmax={frontierVmax:F3}");
        }

        /// <summary>
        /// Logs comprehensive tick summary statistics.
        /// </summary>
        void LogTickSummary(GridState s, SimContext ctx)
        {
            var stats = ComputeTickStatistics(s);
            
            string baseMsg = FormatTickSummaryMessage(stats, ctx);

            if (HasNaNValues(stats))
            {
                Debug.Log(baseMsg + FormatNaNDiagnostics(stats));
            }
            else
            {
                Debug.Log(baseMsg);
            }
        }

        /// <summary>
        /// Statistics container for tick summary.
        /// </summary>
        private struct TickStatistics
        {
            public float SumIncoming;
            public float SumViabilityPos;
            public float SumEntropy;
            public int ActiveCount;
            public float VMin;
            public float VMax;
            public int VPosCount;
            public int VNaN;
            public int InNaN;
            public int NNaN;
            public int ENaN;
        }

        /// <summary>
        /// Computes statistical summary of current tick state.
        /// </summary>
        private TickStatistics ComputeTickStatistics(GridState s)
        {
            var stats = new TickStatistics
            {
                VMin = float.PositiveInfinity,
                VMax = float.NegativeInfinity
            };

            for (int i = 0; i < s.Len; i++)
            {
                float inc = s.Incoming[i];
                float n = s.Nlocal[i];
                float ent = s.Entropy[i];
                float v = s.V[i];

                stats.SumIncoming += inc;
                stats.SumEntropy += ent;

                if (s.Active[i] == 1) stats.ActiveCount++;

                if (float.IsNaN(inc) || float.IsInfinity(inc)) stats.InNaN++;
                if (float.IsNaN(n) || float.IsInfinity(n)) stats.NNaN++;
                if (float.IsNaN(ent) || float.IsInfinity(ent)) stats.ENaN++;

                if (float.IsNaN(v) || float.IsInfinity(v))
                {
                    stats.VNaN++;
                    continue;
                }

                if (v > 0f)
                {
                    stats.VPosCount++;
                    stats.SumViabilityPos += v;
                }

                if (v < stats.VMin) stats.VMin = v;
                if (v > stats.VMax) stats.VMax = v;
            }

            if (stats.VMin == float.PositiveInfinity) stats.VMin = float.NaN;
            if (stats.VMax == float.NegativeInfinity) stats.VMax = float.NaN;

            return stats;
        }

        /// <summary>
        /// Formats tick summary message.
        /// </summary>
        private string FormatTickSummaryMessage(TickStatistics stats, SimContext ctx)
        {
            float avgIncoming = stats.SumIncoming / state.Len;
            float avgEntropy = stats.SumEntropy / state.Len;
            float avgVposAllCells = stats.SumViabilityPos / state.Len;

            return $"Tick {ctx.Tick} | AvgIn={avgIncoming:F3} | AvgV+={avgVposAllCells:F3} | AvgS={avgEntropy:F3} | " +
                   $"Active={stats.ActiveCount} | V+={stats.VPosCount} | Vmin={stats.VMin:F3} | Vmax={stats.VMax:F3} | NGlobal={ctx.NGlobal:F1}";
        }

        /// <summary>
        /// Checks if any NaN values were detected.
        /// </summary>
        private bool HasNaNValues(TickStatistics stats)
        {
            return stats.VNaN > 0 || stats.InNaN > 0 || stats.NNaN > 0 || stats.ENaN > 0;
        }

        /// <summary>
        /// Formats NaN diagnostics message.
        /// </summary>
        private string FormatNaNDiagnostics(TickStatistics stats)
        {
            return $" | VNaN={stats.VNaN} InNaN={stats.InNaN} NNaN={stats.NNaN} ENaN={stats.ENaN}";
        }
    }
}