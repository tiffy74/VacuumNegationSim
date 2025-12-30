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
    // ===== Constants (do not change) =====
    private const int NeighborCount = 4; // 4-way grid connectivity
    private const int PersistenceConfigCount = 16; // 2^4 for 4 neighbors
    private const float MinViabilityEpsilon = 1e-6f; // Prevent division by zero
    private SimulationEngine engine;
    private SimContext ctx;
    private GridState state;
    private SimConfig BuildConfig()
    {
        return new SimConfig
        {
            NGlobalMax = NGlobalMax,
            GlobalReplenishPerTick = GlobalReplenishPerTick,
            MinEnergyForPersistence = MinEnergyForPersistence,

            EthreshBase = EthreshBase,
            GlobalScarcityK = GlobalScarcityK,
            EntropyPenalty = EntropyPenalty,
            DecayLoss = DecayLoss,

            PropagateFrac = PropagateFrac,
            MinBudgetToPropagate = MinBudgetToPropagate,
            ActivationCost = ActivationCost,

            EntropyGainPerUse = EntropyGainPerUse,
            EntropyDiffuseRate = EntropyDiffuseRate,
            EntropyDecay = EntropyDecay,

            NlocalMax = NlocalMax,
            VacuumEventProbability = VacuumEventProbability,
            VacuumEventEntropy = VacuumEventEntropy,

            ExpansionRate = ExpansionRate,
            // MatterAheadThreshold = MatterAheadThreshold,

            BlackHoleFormThreshold = BlackHoleFormThreshold,
            BlackHoleDrainFrac = BlackHoleDrainFrac,
            BlackHoleRecoilFrac = BlackHoleRecoilFrac,

            FieldAdvanceChance = FieldAdvanceChance,
            FieldAdvanceCost = FieldAdvanceCost,
            FieldAdvanceMinSource = FieldAdvanceMinSource,
            FieldAdvanceRequiresViability = RequireViabilityForField,
            FieldAdvanceSeedsEnergy = SeedEnergyOnFieldAdvance,
            FieldSeedEnergy = FieldSeedEnergy,

            EntropyGainFromGradient = EntropyGainFromGradient,
            EntropyGainNearBH = EntropyGainNearBH,
            EntropyViabilityGainA = EntropyViabilityGainA,
            EntropyViabilityGainK = EntropyViabilityGainK,

            VoidColor = VoidColor,
            NullspaceColor = NullspaceColor,
            ShowEntropyTint = ShowEntropyTint,
            ViabilityColorScale = 40f
        };
    }

    // ===== Budget & viability knobs =====
    [Header("Global Budget")]
    [SerializeField] public float NGlobalMax = 5e7f;
    [SerializeField] public float NGlobal = 1e7f;
    [SerializeField] public float GlobalReplenishPerTick = 200;
    [SerializeField] public float MinEnergyForPersistence = 5f;

    [Header("Viability / Threshold")]
    [SerializeField] public float EthreshBase = 0.18f;
    [SerializeField] public float GlobalScarcityK = 0.3f;
    [SerializeField] public float EntropyPenalty = 0.02f;
    [SerializeField] public float DecayLoss = 0.003f;
    [SerializeField] public float EntropyViabilityGainA = 0.5f;
    [SerializeField] public float EntropyViabilityGainK = 1.0f;

    [Header("Propagation")]
    [SerializeField] public float PropagateFrac = 0.25f;
    [SerializeField] public float MinBudgetToPropagate = 0.1f;
    [SerializeField] public float ActivationCost = 0.25f;

    [Header("Entropy Dynamics")]
    [SerializeField] public float EntropyGainPerUse = 0.2f;
    [SerializeField] public float EntropyDiffuseRate = 0.2f;
    [SerializeField] public float EntropyDecay = 0.02f;
    [SerializeField] public float EntropyGainFromGradient = 0.02f;
    [SerializeField] public float EntropyGainNearBH = 0.05f;

    [Header("Local Limits")]
    [SerializeField] public float NlocalMax = 5e4f;
    [SerializeField] public float VacuumEventProbability = 0.0002f; // Probability per cell
    [SerializeField] public float VacuumEventEntropy = 0.5f; // The amount of entropy per vacuum event
    [SerializeField] public float ExpansionRate = 1.0f; // Scale factor per tick

    [Header("Field Propagation")]
    [SerializeField] float FieldAdvanceChance = 0.25f; // probability once conditions are met
    [SerializeField] float FieldAdvanceCost = 0.05f; // energy paid by the source cell
    [SerializeField] float FieldAdvanceMinSource = 0.1f; // must have at least this much to expand field
    [SerializeField] bool RequireViabilityForField = false;
    [SerializeField] bool SeedEnergyOnFieldAdvance = true;
    [SerializeField] float FieldSeedEnergy = 0.1f;

    [Header("Colors")]
    [SerializeField] public Color VoidColor = new Color(0.05f, 0.05f, 0.08f, 1f);
    [SerializeField] public bool ShowEntropyTint = false;
    [SerializeField] public Color VacuumEnergyColor = new Color(0.48f, 0.25f, 0.52f, 1f);
    [SerializeField] public Color NullspaceColor = new Color(0.15f, 0.0f, 0.25f, 1f);
    [SerializeField] public float ScaleFactor = 1.0f; // Initial scale
    [SerializeField] public float SpatialThreshK = -0.6f;
    [SerializeField] public float SpatialDecayK = 0.5f;

    [Header("Render Mode")]
    [SerializeField] private Assets.Scripts.Unity.RenderMode renderMode = Assets.Scripts.Unity.RenderMode.Viability;

    [SerializeField] float ticksPerSecond = 10f;

    // ===== Per-cell state (flattened arrays sized Width*Height) =====
    [NonSerialized] public float[] rNorm;
    [NonSerialized] public int[] FieldFirstTick;   // -1 = not yet private
    [NonSerialized] public int[] EnergyFirstTick;  // -1 = no energy yet

    public SimulationGrid Grid;
    [NonSerialized] public CellVisualiser[,] views;
    private List<Vector2Int> activeCells = new List<Vector2Int>();
    private GridRenderer gridRenderer;
    bool running = true;

    void Start()
    {
        // 1) Get grid + spawn visuals (unchanged)
        Grid = GetComponent<SimulationGrid>();
        views = new CellVisualiser[Grid.Width, Grid.Height];
        Grid.SpawnVisualCells(views);

        // 2) Build a pure config snapshot from your inspector fields
        var cfg = BuildConfig();

        // 3) Create pure state + context (engine-owned)
        state = new GridState(Grid.Width, Grid.Height);
        ctx = new SimContext(cfg, NGlobal, ScaleFactor);

        // 4) Initialise the simulation state (a refactored version of your InitState)
        // IMPORTANT: this method should ONLY write into `state` arrays + ctx (no visuals).
        InitStateInto(state, ctx);

        // 5) Create the engine with a single “legacy” step to preserve behaviour
        // This step wraps your existing Pass1/2/3/4 calls in the same order as before.
        engine = new SimulationEngine(state, new ISimStep[]
        {
            new LegacyTickStep(
                ComputeViability,
                CountPersistenceConfigurations,
                () => {
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
                    BlackHoles.GrowBlackHoles(state);
                    // BlackHoles.BlackHoleAttractEnergy(state);
                }
            )
        });

        gridRenderer = new GridRenderer(
            Grid.Width, Grid.Height, views,
            VoidColor,               // from your inspector
            NullspaceColor,          // your dim field colour
            ShowEntropyTint
        );
        gridRenderer.ViabilityColorScale = 40f;

        // 6) Start the loop (unchanged)
        StartCoroutine(SimLoop());
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        renderMode = Assets.Scripts.Unity.RenderMode.Viability;

        //if (Keyboard.current == null) return;

        //if (Keyboard.current.digit1Key.wasPressedThisFrame)
        //    renderMode = Assets.Scripts.Unity.RenderMode.Viability;
        //else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        //    renderMode = Assets.Scripts.Unity.RenderMode.Energy;
        //else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        //    renderMode = Assets.Scripts.Unity.RenderMode.Entropy;
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

    // Optional controls:
    public void Play() { running = true; if (!gameObject.activeInHierarchy) return; StartCoroutine(SimLoop()); }
    public void Pause() { running = false; }
    public void Step() { TickSimulation(); }   // single-step (e.g., from a UI button)

    float EthreshEff
    {
        get
        {
            float scarcity = 1f - (ctx.NGlobal / Mathf.Max(1f, NGlobalMax));
            return EthreshBase * (1f + GlobalScarcityK * scarcity);
        }
    }

    // Put this in SimulationController for now (Phase 1).
    // It is a line-for-line port of your current InitState(), but writes into GridState + SimContext.
    // No visuals. No coroutines. No Unity objects (except QualitySettings / Application, which you can remove later).

    [Header("Black Hole")]
    [SerializeField] public float BlackHoleFormThreshold = 0.5f;
    [SerializeField] public float BlackHoleDrainFrac = 0f;
    [SerializeField] public float BlackHoleRecoilFrac = 0f;
    private const float BlackHoleEnergyEps = 1e-6f;

    void InitStateInto(GridState s, SimContext ctx)
    {
        int len = s.Len;

        // ---- Clear per-cell arrays ----
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

        // ---- Clear BH entity arrays (NOT per-cell length) ----
        for (int i = 0; i < s.BlackHoleParent.Length; i++)
            s.BlackHoleParent[i] = 0;

        for (int i = 0; i < s.BlackHoleMass.Length; i++)
            s.BlackHoleMass[i] = 0f;

        s.NextBlackHoleId = 1;

        // ---- Reset global simulation counters ----
        ctx.Tick = 0;
        ctx.ScaleFactor = 1.0f;

        // ---- Seed a small central block with energy + field ----
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

                s.Nlocal[idx] = 50f;          // <-- IMPORTANT: don't seed with 0
                s.Incoming[idx] = 0f;
                s.Entropy[idx] = 0f;
                s.Active[idx] = 1;
                s.FieldPresent[idx] = true;

                if (s.FieldFirstTick[idx] == -1) s.FieldFirstTick[idx] = ctx.Tick;
                if (s.EnergyFirstTick[idx] == -1) s.EnergyFirstTick[idx] = ctx.Tick;
            }
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }


    void TickSimulation()
    {
        if (ctx.Tick % 10 == 0)
            Debug.Log($"[Tick {ctx.Tick}] TickSimulation start (A)");

        engine.Tick(ctx);

        // Debug probe: center cell and a cell toward the front
        int cx = state.W / 2;
        int cy = state.H / 2;
        int frontX = Math.Min(state.W - 1, cx + 5);
        int frontY = cy;
        int ci = state.Idx(cx, cy);
        int fi = state.Idx(frontX, frontY);

        Debug.Log(
            $"[Tick {ctx.Tick}] Center: In={state.Incoming[ci]:F4} N={state.Nlocal[ci]:F4} Field={state.FieldPresent[ci]} Active={state.Active[ci]} V={state.V[ci]:F4} | " +
            $"Front({frontX},{frontY}): In={state.Incoming[fi]:F4} N={state.Nlocal[fi]:F4} Field={state.FieldPresent[fi]} Active={state.Active[fi]} V={state.V[fi]:F4}");

        // Outer-region propagation debug every 20 ticks
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
                    // skip seed patch (5x5 around center)
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

            // Frontier diagnostics near field boundary
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

                // frontier ring: recent field arrivals within last 5 ticks, excluding seed patch
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

            Debug.Log($"[Tick {ctx.Tick}] FieldCount={fieldCount} Arrivals={fieldArrivals} BlackHoles={blackHoleCount} EnergyCount>{MinBudgetToPropagate}={energyCount} Viable={viableCount} FrontierViable={frontierViable} FrontierVmin={frontierVmin:F3} FrontierVmax={frontierVmax:F3}");
        }

        LogTickSummary(state, ctx);
        UpdateVisualsFromState(state);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    float ComputeViability(float incomingFlow, float nlocal, float entropy)
    {
        float gain = 1f + EntropyViabilityGainA * (1f - Mathf.Exp(-EntropyViabilityGainK * Mathf.Max(0f, entropy)));
        return (incomingFlow * gain - DecayLoss) / Mathf.Max(MinViabilityEpsilon, EthreshEff);
    }

    private void UpdateVisualsFromState(GridState s)
    {
        // Safety: if called before Start() fully initialises
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
            float simulatedEnergy = state.Nlocal[cellIndex];

            for (int n = 0; n < 4; n++)
            {
                if (((config >> n) & 1) == 0) continue;

                int nx = x + dx[n];
                int ny = y + dy[n];

                if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                    continue;

                int neighborIdx = state.Idx(nx, ny);

                simulatedEnergy += PropagateFrac * state.Nlocal[neighborIdx] * 0.25f;
            }

            if (simulatedEnergy > MinEnergyForPersistence)
                count++;
        }

        return count;
    }
    public void RestartSimulation()
    {
        // Use the correct signature: pass the required GridState and SimContext arguments
        InitStateInto(state, ctx);
    }

    void LogTickSummary(GridState s, SimContext ctx)
    {
        float sumIncoming = 0f;
        float sumViabilityPos = 0f;
        float sumEntropy = 0f;

        int activeCount = 0;

        float vMin = float.PositiveInfinity;
        float vMax = float.NegativeInfinity;
        int vPosCount = 0;

        int vNaN = 0, inNaN = 0, nNaN = 0, eNaN = 0;

        for (int i = 0; i < s.Len; i++)
        {
            float inc = s.Incoming[i];
            float n = s.Nlocal[i];
            float ent = s.Entropy[i];
            float v = s.V[i];

            sumIncoming += inc;
            sumEntropy += ent;

            if (s.Active[i] == 1) activeCount++;

            if (float.IsNaN(inc) || float.IsInfinity(inc)) inNaN++;
            if (float.IsNaN(n) || float.IsInfinity(n)) nNaN++;
            if (float.IsNaN(ent) || float.IsInfinity(ent)) eNaN++;

            if (float.IsNaN(v) || float.IsInfinity(v))
            {
                vNaN++;
                continue;
            }

            if (v > 0f)
            {
                vPosCount++;
                sumViabilityPos += v;   // sum only positive viability
            }

            if (v < vMin) vMin = v;
            if (v > vMax) vMax = v;
        }

        float avgIncoming = sumIncoming / s.Len;
        float avgEntropy = sumEntropy / s.Len;

        // average over ALL cells, but only summing positive V (your original intent)
        float avgVposAllCells = sumViabilityPos / s.Len;

        // If everything was NaN, keep min/max readable
        if (vMin == float.PositiveInfinity) vMin = float.NaN;
        if (vMax == float.NegativeInfinity) vMax = float.NaN;

        string baseMsg =
            $"Tick {ctx.Tick} | AvgIn={avgIncoming:F3} | AvgV+={avgVposAllCells:F3} | AvgS={avgEntropy:F3} | " +
            $"Active={activeCount} | V+={vPosCount} | Vmin={vMin:F3} | Vmax={vMax:F3} | NGlobal={ctx.NGlobal:F1}";

        if (vNaN > 0 || inNaN > 0 || nNaN > 0 || eNaN > 0)
        {
            Debug.Log(baseMsg + $" | VNaN={vNaN} InNaN={inNaN} NNaN={nNaN} ENaN={eNaN}");
        }
        else
        {
            Debug.Log(baseMsg);
        }
    }
}