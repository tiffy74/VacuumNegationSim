using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Viable.Engine;
using Viable.Engine.State;
using Viable.Engine.Execution;
using Viable.Engine.Configuration;
using Assets.Scripts.Unity;

public class SimulationController : MonoBehaviour
{
    // ===== Engine Components =====
    private SimulationRunner runner;
    private StepContext context;
    private GridState state;
    
    private SimulationConfiguration BuildEngineConfig()
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

    // ===== Budget & viability knobs =====
    [Header("Global Resource Pool")]
    [SerializeField] public float ResourceGlobalMax = 5e7f;
    [SerializeField] public float ResourceGlobal = 1e7f;
    [SerializeField] public float GlobalReplenishPerTick = 200;
    [SerializeField] public float MinResourceForPersistence = 5f;

    [Header("Viability / Threshold")]
    [SerializeField] public float EthreshBase = 0.18f;
    [SerializeField] public float GlobalScarcityK = 0.3f;
    [SerializeField] public float ComplexityPenalty = 0.02f;
    [SerializeField] public float DecayLoss = 0.003f;
    [SerializeField] public float ComplexityViabilityGainA = 0.5f;
    [SerializeField] public float ComplexityViabilityGainK = 1.0f;

    [Header("Propagation")]
    [SerializeField] public float PropagateFrac = 0.25f;
    [SerializeField] public float MinBudgetToPropagate = 0.1f;
    [SerializeField] public float ActivationCost = 0.25f;

    [Header("Complexity Dynamics")]
    [SerializeField] public float ComplexityGainPerUse = 0.2f;
    [SerializeField] public float ComplexityDiffusionRate = 0.2f;
    [SerializeField] public float ComplexityDecay = 0.02f;
    [SerializeField] public float ComplexityGainFromGradient = 0.02f;
    [SerializeField] public float ComplexityGainNearSink = 0.05f;

    [Header("Local Limits")]
    [SerializeField] public float ResourceLocalMax = 5e4f;
    [SerializeField] public float PerturbationProbability = 0.0002f;
    [SerializeField] public float PerturbationComplexity = 0.5f;
    [SerializeField] public float ExpansionRate = 1.0f;

    [Header("Region Expansion")]
    [SerializeField] float RegionExpansionChance = 0.25f;
    [SerializeField] float RegionExpansionCost = 0.05f;
    [SerializeField] float RegionExpansionMinSource = 0.1f;
    [SerializeField] bool RequireViabilityForRegion = false;
    [SerializeField] bool SeedResourceOnRegionExpansion = true;
    [SerializeField] float RegionSeedResource = 0.1f;

    [Header("Colors")]
    [SerializeField] public Color InactiveColor = new Color(0.05f, 0.05f, 0.08f, 1f);
    [SerializeField] public bool ShowComplexityTint = false;
    [SerializeField] public Color DormantRegionColor = new Color(0.15f, 0.0f, 0.25f, 1f);
    [SerializeField] public float ScaleFactor = 1.0f;

    [Header("Render Mode")]
    [SerializeField] private Assets.Scripts.Unity.RenderMode renderMode = Assets.Scripts.Unity.RenderMode.Viability;

    [SerializeField] float ticksPerSecond = 10f;

    public SimulationGrid Grid;
    public CellVisualiser[,] views;
    private GridRenderer gridRenderer;
    bool running = true;

    [Header("Sink Regions")]
    [SerializeField] public float SinkFormationThreshold = 0.5f;
    [SerializeField] public float SinkDrainFraction = 0f;
    [SerializeField] public float SinkRecoilFraction = 0f;

    void Start()
    {
        Grid = GetComponent<SimulationGrid>();
        views = new CellVisualiser[Grid.Width, Grid.Height];
        Grid.SpawnVisualCells(views);

        var cfg = BuildEngineConfig();

        // Create Engine state
        state = new GridState(Grid.Width, Grid.Height);
        
        // Create Engine context (with deterministic seed if desired)
        context = new StepContext(cfg, ResourceGlobal, ScaleFactor, seed: null);

        // Initialize state
        InitStateInto(state, context);

        // Create Engine runner with SimulationStepper
        var stepper = new SimulationStepper(CountPersistenceConfigurations);
        runner = new SimulationRunner(state, new[] { stepper });

        gridRenderer = new GridRenderer(
            Grid.Width, Grid.Height, views,
            InactiveColor,
            DormantRegionColor,
            ShowComplexityTint
        );
        gridRenderer.ViabilityColorScale = 40f;

        StartCoroutine(SimLoop());
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        renderMode = Assets.Scripts.Unity.RenderMode.Viability;
    }

    IEnumerator SimLoop()
    {
        var delay = new WaitForSeconds(1f / Mathf.Max(1f, ticksPerSecond));
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
        s.Reset(); // Use GridState's Reset() method

        // Seed a small central block with resource + active region
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
                s.Incoming[idx] = 0f;
                s.ComplexityMetric[idx] = 0f;
                s.Active[idx] = 1;
                s.ActiveRegion[idx] = true;

                if (s.RegionActivationTick[idx] == -1) s.RegionActivationTick[idx] = ctx.Tick;
                if (s.ResourceFirstTick[idx] == -1) s.ResourceFirstTick[idx] = ctx.Tick;
            }
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    void TickSimulation()
    {
        if (context.Tick % 10 == 0)
            Debug.Log($"[Tick {context.Tick}] TickSimulation start (Engine)");

        // Call Engine to step simulation
        runner.Step(context);

        // Diagnostics (optional - can be removed once confident)
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
        
        // GridRenderer still uses old StateGrid type - need adapter
        // For now, we'll pass state directly (GridRenderer needs updating in future)
        gridRenderer.Render(state, context, renderMode);
    }

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

        for (int config = 0; config < 16; config++)
        {
            float simulatedResource = state.ResourceLocal[cellIndex];

            for (int n = 0; n < 4; n++)
            {
                if (((config >> n) & 1) == 0) continue;

                int nx = x + dx[n];
                int ny = y + dy[n];

                if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                    continue;

                int neighborIdx = state.Idx(nx, ny);

                simulatedResource += PropagateFrac * state.ResourceLocal[neighborIdx] * 0.25f;
            }

            if (simulatedResource > MinResourceForPersistence)
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
        int cx = state.W / 2;
        int cy = state.H / 2;
        
        int regionCount = 0;
        int sinkCount = 0;
        int viableCount = 0;
        int resourceCount = 0;

        for (int i = 0; i < state.Len; i++)
        {
            if (state.ActiveRegion[i]) regionCount++;
            if (state.IsSink[i]) sinkCount++;
            if (state.V[i] > 0f) viableCount++;
            if (state.ResourceLocal[i] > MinBudgetToPropagate) resourceCount++;
        }

        Debug.Log($"[Tick {context.Tick}] RegionCount={regionCount} Sinks={sinkCount} Viable={viableCount} ResourceCount>{MinBudgetToPropagate}={resourceCount}");
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
}