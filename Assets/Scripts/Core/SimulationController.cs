using Assets.Scripts.Domain;
using Assets.Scripts.Events;
using Assets.Scripts.Simulation;
using Assets.Scripts.Unity;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.DedicatedServer;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SimulationController : MonoBehaviour
{
    // ===== Constants =====
    private const int NeighborCount = 4; // 4-way grid connectivity
    private const int PersistenceConfigCount = 16; // 2^4 for 4 neighbors
    private const float MinViabilityEpsilon = 1e-6f; // Prevent division by zero
    private SimulationEngine engine;
    private SimContext ctx;
    private StateGrid state;
    
    private SimConfig BuildConfig()
    {
        return new SimConfig
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

            InactiveColor = InactiveColor,
            DormantRegionColor = DormantRegionColor,
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
    [SerializeField] public Color PerturbationColor = new Color(0.48f, 0.25f, 0.52f, 1f);
    [SerializeField] public Color DormantRegionColor = new Color(0.15f, 0.0f, 0.25f, 1f);
    [SerializeField] public float ScaleFactor = 1.0f;
    [SerializeField] public float SpatialThreshK = -0.6f;
    [SerializeField] public float SpatialDecayK = 0.5f;

    [Header("Render Mode")]
    [SerializeField] private Assets.Scripts.Unity.RenderMode renderMode = Assets.Scripts.Unity.RenderMode.Viability;

    [SerializeField] float ticksPerSecond = 10f;

    // ===== Per-cell state =====
    [NonSerialized] public float[] rNorm;
    [NonSerialized] public int[] RegionActivationTick;
    [NonSerialized] public int[] ResourceFirstTick;

    public SimulationGrid Grid;
    [NonSerialized] public CellVisualiser[,] views;
    private List<Vector2Int> activeCells = new List<Vector2Int>();
    private GridRenderer gridRenderer;
    bool running = true;

    [Header("Sink Regions")]
    [SerializeField] public float SinkFormationThreshold = 0.5f;
    [SerializeField] public float SinkDrainFraction = 0f;
    [SerializeField] public float SinkRecoilFraction = 0f;
    private const float SinkResourceEps = 1e-6f;

    void Start()
    {
        Grid = GetComponent<SimulationGrid>();
        views = new CellVisualiser[Grid.Width, Grid.Height];
        Grid.SpawnVisualCells(views);

        var cfg = BuildConfig();

        state = new StateGrid(Grid.Width, Grid.Height);
        ctx = new SimContext(cfg, ResourceGlobal, ScaleFactor);

        InitStateInto(state, ctx);

        engine = new SimulationEngine(state, new ISimStep[]
        {
            new LegacyTickStep(
                ComputeViability,
                CountPersistenceConfigurations,
                () => {
                    RegionExpansion.ExpandActiveRegion(
                        state,
                        ctx.Tick,
                        RegionExpansionChance,
                        RegionExpansionCost,
                        RegionExpansionMinSource,
                        RequireViabilityForRegion,
                        SeedResourceOnRegionExpansion,
                        RegionSeedResource
                    );
                    SinkRegions.ExpandSinkRegions(state);
                }
            )
        });

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

    System.Collections.IEnumerator SimLoop()
    {
        var delay = new WaitForSeconds(1f / Mathf.Max(1f, ticksPerSecond));
        while (running)
        {
            TickSimulation();
            yield return delay;
        }
    }

    public void Play() { running = true; if (!gameObject.activeInHierarchy) return; StartCoroutine(SimLoop()); }
    public void Pause() { running = false; }
    public void Step() { TickSimulation(); }

    float EthreshEff
    {
        get
        {
            float scarcity = 1f - (ctx.ResourceGlobal / Mathf.Max(1f, ResourceGlobalMax));
            return EthreshBase * (1f + GlobalScarcityK * scarcity);
        }
    }

    void InitStateInto(StateGrid s, SimContext ctx)
    {
        int len = s.Len;

        for (int i = 0; i < len; i++)
        {
            s.ResourceLocal[i] = 0f;
            s.ComplexityMetric[i] = 0f;
            s.V[i] = 0f;
            s.Active[i] = 0;

            s.Incoming[i] = 0f;
            s.ComplexityNext[i] = 0f;

            s.IsInactive[i] = false;
            s.ActiveRegion[i] = false;
            s.IsSink[i] = false;
            s.SinkCharge[i] = 0f;
            s.SinkId[i] = 0;

            s.ZeroResourceTicks[i] = 0;

            s.RegionActivationTick[i] = -1;
            s.ResourceFirstTick[i] = -1;
        }

        for (int i = 0; i < s.SinkParent.Length; i++)
            s.SinkParent[i] = 0;

        for (int i = 0; i < s.SinkMass.Length; i++)
            s.SinkMass[i] = 0f;

        s.NextSinkId = 1;

        ctx.Tick = 0;
        ctx.ScaleFactor = 1.0f;

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
        if (ctx.Tick % 10 == 0)
            Debug.Log($"[Tick {ctx.Tick}] TickSimulation start");

        engine.Tick(ctx);

        int cx = state.W / 2;
        int cy = state.H / 2;
        int frontX = Math.Min(state.W - 1, cx + 5);
        int frontY = cy;
        int ci = state.Idx(cx, cy);
        int fi = state.Idx(frontX, frontY);

        Debug.Log(
            $"[Tick {ctx.Tick}] Center: In={state.Incoming[ci]:F4} R={state.ResourceLocal[ci]:F4} Region={state.ActiveRegion[ci]} Active={state.Active[ci]} V={state.V[ci]:F4} | " +
            $"Front({frontX},{frontY}): In={state.Incoming[fi]:F4} R={state.ResourceLocal[fi]:F4} Region={state.ActiveRegion[fi]} Active={state.Active[fi]} V={state.V[fi]:F4}");

        if (ctx.Tick % 20 == 0)
        {
            int vPosOuter = 0;
            int inPosOuter = 0;
            float vMinOuter = float.PositiveInfinity;
            float vMaxOuter = float.NegativeInfinity;

            for (int y = 0; y < state.H; y++)
            {
                for (int x = 0; x < state.W; x++)
                {
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

            int regionCount = 0;
            int regionArrivals = 0;
            int resourceCount = 0;
            int viableCount = 0;
            int sinkCount = 0;
            int frontierViable = 0;
            float frontierVmin = float.PositiveInfinity;
            float frontierVmax = float.NegativeInfinity;

            int arrivalTick = ctx.Tick - 1;

            for (int i = 0; i < state.Len; i++)
            {
                if (state.ActiveRegion[i])
                {
                    regionCount++;
                    if (state.RegionActivationTick[i] == arrivalTick || state.RegionActivationTick[i] == ctx.Tick)
                        regionArrivals++;
                }

                if (state.IsSink[i])
                    sinkCount++;

                if (state.ResourceLocal[i] > MinBudgetToPropagate)
                    resourceCount++;
                if (state.V[i] > 0f)
                    viableCount++;

                if (state.ActiveRegion[i] && state.RegionActivationTick[i] >= ctx.Tick - 5)
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

            Debug.Log($"[Tick {ctx.Tick}] RegionCount={regionCount} Arrivals={regionArrivals} Sinks={sinkCount} ResourceCount>{MinBudgetToPropagate}={resourceCount} Viable={viableCount} FrontierViable={frontierViable} FrontierVmin={frontierVmin:F3} FrontierVmax={frontierVmax:F3}");
        }

        LogTickSummary(state, ctx);
        UpdateVisualsFromState(state);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    float ComputeViability(float incomingFlow, float resource, float complexity)
    {
        float gain = 1f + ComplexityViabilityGainA * (1f - Mathf.Exp(-ComplexityViabilityGainK * Mathf.Max(0f, complexity)));
        return (incomingFlow * gain - DecayLoss) / Mathf.Max(MinViabilityEpsilon, EthreshEff);
    }

    private void UpdateVisualsFromState(StateGrid s)
    {
        if (gridRenderer == null || views == null) return;
        gridRenderer.Render(s, ctx, renderMode);
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
        InitStateInto(state, ctx);
    }

    void LogTickSummary(StateGrid s, SimContext ctx)
    {
        float sumIncoming = 0f;
        float sumViabilityPos = 0f;
        float sumComplexity = 0f;

        int activeCount = 0;

        float vMin = float.PositiveInfinity;
        float vMax = float.NegativeInfinity;
        int vPosCount = 0;

        int vNaN = 0, inNaN = 0, rNaN = 0, cNaN = 0;

        for (int i = 0; i < s.Len; i++)
        {
            float inc = s.Incoming[i];
            float r = s.ResourceLocal[i];
            float c = s.ComplexityMetric[i];
            float v = s.V[i];

            sumIncoming += inc;
            sumComplexity += c;

            if (s.Active[i] == 1) activeCount++;

            if (float.IsNaN(inc) || float.IsInfinity(inc)) inNaN++;
            if (float.IsNaN(r) || float.IsInfinity(r)) rNaN++;
            if (float.IsNaN(c) || float.IsInfinity(c)) cNaN++;

            if (float.IsNaN(v) || float.IsInfinity(v))
            {
                vNaN++;
                continue;
            }

            if (v > 0f)
            {
                vPosCount++;
                sumViabilityPos += v;
            }

            if (v < vMin) vMin = v;
            if (v > vMax) vMax = v;
        }

        float avgIncoming = sumIncoming / s.Len;
        float avgComplexity = sumComplexity / s.Len;
        float avgVposAllCells = sumViabilityPos / s.Len;

        if (vMin == float.PositiveInfinity) vMin = float.NaN;
        if (vMax == float.NegativeInfinity) vMax = float.NaN;

        string baseMsg =
            $"Tick {ctx.Tick} | AvgIn={avgIncoming:F3} | AvgV+={avgVposAllCells:F3} | AvgC={avgComplexity:F3} | " +
            $"Active={activeCount} | V+={vPosCount} | Vmin={vMin:F3} | Vmax={vMax:F3} | ResourceGlobal={ctx.ResourceGlobal:F1}";

        if (vNaN > 0 || inNaN > 0 || rNaN > 0 || cNaN > 0)
        {
            Debug.Log(baseMsg + $" | VNaN={vNaN} InNaN={inNaN} RNaN={rNaN} CNaN={cNaN}");
        }
        else
        {
            Debug.Log(baseMsg);
        }
    }
}