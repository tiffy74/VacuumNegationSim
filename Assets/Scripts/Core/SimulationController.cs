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
            MatterAheadThreshold = MatterAheadThreshold
        };
    }

    // ===== Budget & viability knobs =====
    [Header("Global Budget")]
    [SerializeField] public float NGlobalMax = 5e7f;
    [SerializeField] public float NGlobal = 1e7f;
    [SerializeField] public float GlobalReplenishPerTick = 200;
    [SerializeField] public float MinEnergyForPersistence = 5f;

    [Header("Viability / Threshold")]
    [SerializeField] public float EthreshBase = 0.25f;
    [SerializeField] public float GlobalScarcityK = 0.3f;
    [SerializeField] public float EntropyPenalty = 0.15f;
    [SerializeField] public float DecayLoss = 0.01f;

    [Header("Propagation")]
    [SerializeField] public float PropagateFrac = 0.12f;
    [SerializeField] public float MinBudgetToPropagate = 2f;
    [SerializeField] public float ActivationCost = 2f;

    [Header("Entropy Dynamics")]
    [SerializeField] public float EntropyGainPerUse = 0.02f;
    [SerializeField] public float EntropyDiffuseRate = 0.06f;
    [SerializeField] public float EntropyDecay = 0.015f;

    [Header("Local Limits")]
    [SerializeField] public float NlocalMax = 5e4f;
    [SerializeField] public float VacuumEventProbability = 0.0002f; // Probability per cell
    [SerializeField] public float VacuumEventEntropy = 0.5f; // The amount of entropy per vacuum event
    [SerializeField] public float ExpansionRate = 1.0f; // Scale factor per tick

    [Header("Colors")]
    [SerializeField] public Color VoidColor = new Color(0.05f, 0.05f, 0.08f, 1f);
    [SerializeField] public bool ShowEntropyTint = false;
    [SerializeField] public Color VacuumEnergyColor = new Color(0.48f, 0.25f, 0.52f, 1f);
    [SerializeField] public Color NullspaceColor = new Color(0.15f, 0.0f, 0.25f, 1f);
    [SerializeField] public float ScaleFactor = 1.0f; // Initial scale
    [SerializeField] public float SpatialThreshK = -0.6f;
    [SerializeField] public float SpatialDecayK = 0.5f;

    [SerializeField] float ticksPerSecond = 10f;

    public float FieldAdvanceCost = 0.2f;      // energy paid by the source cell
    public float FieldAdvanceMinSource = 1.0f; // must have at least this much to expand field
    public float FieldAdvanceChance = 0.3f;    // probability once conditions are met


    // ===== Per-cell state (flattened arrays sized Width*Height) =====
    public float[] rNorm;
    public const float MatterAheadThreshold = 0.5f;
    public int[] FieldFirstTick;   // -1 = not yet private
    public int[] EnergyFirstTick;  // -1 = no energy yet
    int Idx(int x, int y) => y * Grid.Width + x;
    public int tick = 0;

    public SimulationGrid Grid;
    public CellVisualiser[,] views;
    private List<Vector2Int> activeCells = new List<Vector2Int>();
    private GridRenderer renderer;
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
            () => FieldWave.PropagateFieldWave(state, ctx.Tick, 0.5f),
            () => BlackHoles.GrowBlackHoles(state),
            () => BlackHoles.BlackHoleAttractEnergy(state)
        )
        });

        renderer = new GridRenderer(Grid.Width, Grid.Height, views);

        // 6) Start the loop (unchanged)
        StartCoroutine(SimLoop());
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

    void InitStateInto(GridState s, SimContext ctx)
    {
        int len = s.Len;

        // ---- Clear arrays ----
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

            s.ZeroEnergyTicks[i] = 0;

            s.FieldFirstTick[i] = -1;
            s.EnergyFirstTick[i] = -1;
        }

        // ---- Seed a small central block with energy + field (matches your current InitState) ----
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

                int i = s.Idx(x, y);

                s.Nlocal[i] = 0f;   // starting energy (same as your controller)
                s.Incoming[i] = 0f;
                s.Entropy[i] = 0f;
                s.Active[i] = 1;
                s.FieldPresent[i] = true;

                // Your original InitState did NOT set first-tick markers here, so we don't either.
                // If you want, you can set:
                if (s.FieldFirstTick[i] == -1) s.FieldFirstTick[i] = ctx.Tick;
                if (s.EnergyFirstTick[i] == -1) s.EnergyFirstTick[i] = ctx.Tick;
            }
        }

        // ---- Reset global simulation counters ----
        ctx.Tick = 0;

        // Keep NGlobal/ScaleFactor as whatever you passed into ctx, or reset them here if desired:
        // ctx.NGlobal = cfgInitialNGlobal;  (if you store that)
        ctx.ScaleFactor = 1.0f;

        // ---- Frame rate settings (optional; remove once Unity adapter is separated) ----
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    void TickSimulation()
    {
        engine.Tick(ctx);
        
        if (ctx.Tick % 10 == 0) // log every 10 ticks
            LogTickSummary(state, ctx);
        UpdateVisualsFromState(state);

    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    float ComputeViability(float incomingFlow, float nlocal, float entropy)
    {
        float Eloss = DecayLoss + EntropyPenalty * entropy;
        return (incomingFlow - Eloss) / Mathf.Max(MinViabilityEpsilon, EthreshEff); // V = (Ein - Eloss) / Ethresh
    }

    private void UpdateVisualsFromState(GridState s)
    {
        // Safety: if called before Start() fully initialises
        if (renderer == null || views == null) return;

        renderer.Render(s);
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

//void UpdateVisuals()
//{
//    for (int y = 0; y < Grid.Height; y++)
//    {
//        for (int x = 0; x < Grid.Width; x++)
//        {
//            int i = Idx(x, y);
//            var vis = views[x, y];
//            if (vis == null) continue;

//            if (FieldPresent[i] && (Active[i] == 0 || V[i] <= 0f))
//            { 

//                    // Just-activated field: bright wave front
//                    vis.SetColor(Color.yellow);
//            }
//            else if (FieldPresent[i] && (Active[i] > 0 || V[i] > 0f))
//            {
//                float vForColor = Mathf.Clamp01(V[i] * 2.0f);
//                vis.SetViabilityWithEntropy(vForColor, Entropy[i]);
//            }
//            else if (IsVacuum[i] || Active[i] == 0 || V[i] <= 0f)
//            {
//                vis.SetColor(Color.darkViolet);
//            }



//            // Optional: debug for center cell
//            if (x == Grid.Width / 2 && y == Grid.Height / 2)
//                Debug.Log($"Center: Active={Active[i]}, V={V[i]}, Nlocal={Nlocal[i]}, Entropy={Entropy[i]}");
//            if (x == Grid.Width / 2 + 1 && y == Grid.Height / 2)
//                Debug.Log($"Right Neighbor: Active={Active[i]}, V={V[i]}, Nlocal={Nlocal[i]}, Entropy={Entropy[i]}");
//        }
//    }
//}

//void InitStateInto()
//{
//    int len = Grid.Width * Grid.Height;

//    Nlocal = new float[len];
//    Entropy = new float[len];
//    V = new float[len];
//    Active = new byte[len];
//    incoming = new float[len];
//    entropyNext = new float[len];
//    IsVacuum = new bool[len];
//    zeroEnergyTicks = new int[len];
//    IsBlackHole = new bool[Grid.Width * Grid.Height];
//    for (int i = 0; i < IsBlackHole.Length; i++)
//        IsBlackHole[i] = false;

//    for (int i = 0; i < len; i++)
//    {
//        Nlocal[i] = 0f;
//        Entropy[i] = 0f;
//        V[i] = 0f;
//        Active[i] = 0;
//        IsVacuum[i] = false;
//        zeroEnergyTicks[i] = 0;
//    }

//    // allocate and clear field presence
//    FieldPresent = new bool[Grid.Width * Grid.Height];
//    for (int i = 0; i < FieldPresent.Length; i++)
//        FieldPresent[i] = false;

//    // seed a small central block with energy and field
//    int cx = Grid.Width / 2;
//    int cy = Grid.Height / 2;
//    for (int dy = -2; dy <= 2; dy++)
//    {
//        for (int dx = -2; dx <= 2; dx++)
//        {
//            int x = cx + dx;
//            int y = cy + dy;
//            if (x < 0 || x >= Grid.Width || y < 0 || y >= Grid.Height)
//                continue;

//            int i = Idx(x, y);
//            Nlocal[i] = 5000f;       // starting energy
//            incoming[i] = 0f;
//            Entropy[i] = 0f;
//            Active[i] = 1;
//            FieldPresent[i] = true;
//        }
//    }

//    BlackHoleCharge = new float[Grid.Width * Grid.Height];
//    for (int i = 0; i < BlackHoleCharge.Length; i++)
//        BlackHoleCharge[i] = 0f;

//    FieldFirstTick = new int[len];
//    EnergyFirstTick = new int[len];
//    for (int i = 0; i < len; i++) { FieldFirstTick[i] = -1; EnergyFirstTick[i] = -1; }

//    QualitySettings.vSyncCount = 0;
//    Application.targetFrameRate = 120;
//}

//void PropagateFieldWave(GridState state, SimContext ctx)
//{
//    bool[] nextField = (bool[])FieldPresent.Clone();
//    for (int y = 0; y < Grid.Height; y++)
//    {
//        for (int x = 0; x < Grid.Width; x++)
//        {
//            int i = Idx(x, y);
//            if (!FieldPresent[i])
//            {
//                // If any neighbor has the field, this cell may get it (with randomness)
//                int[] dx = { 0, 0, -1, 1 };
//                int[] dy = { -1, 1, 0, 0 };
//                for (int d = 0; d < 4; d++)
//                {
//                    int nx = x + dx[d], ny = y + dy[d];
//                    if (nx >= 0 && nx < Grid.Width && ny >= 0 && ny < Grid.Height)
//                    {
//                        int ni = Idx(nx, ny);
//                        if (FieldPresent[ni] && UnityEngine.Random.value < 0.5f) // 20% chance to expand
//                        {
//                            nextField[i] = true;
//                            if (!FieldPresent[i] && FieldPresent[ni] && UnityEngine.Random.value < 0.2f)
//                            {
//                                nextField[i] = true; if (FieldFirstTick[i] == -1) FieldFirstTick[i] = tick;   // current global tick break; }
//                            }
//                                break;
//                        }
//                    }
//                }
//            }
//        }
//    }
//    FieldPresent = nextField;
//}

//void BlackHoleAttractEnergy(GridState s)
//{
//    int[] dx = { 0, 0, -1, 1 };
//    int[] dy = { -1, 1, 0, 0 };
//    for (int y = 0; y < Grid.Height; y++)
//    {
//        for (int x = 0; x < Grid.Width; x++)
//        {
//            int i = Idx(x, y);
//            if (!IsBlackHole[i]) continue;

//            for (int d = 0; d < 4; d++)
//            {
//                int nx = x + dx[d];
//                int ny = y + dy[d];
//                if (nx < 0 || nx >= Grid.Width || ny < 0 || ny >= Grid.Height)
//                    continue;
//                int ni = Idx(nx, ny);
//                if (!IsBlackHole[ni])
//                {
//                    // Drain a fraction of neighbor's energy into the black hole
//                    float absorbed = Nlocal[ni] * 0.2f; // 20% per tick, adjust as needed
//                    Nlocal[ni] -= absorbed;
//                    // Optionally, you can accumulate this in the black hole, or just let it vanish
//                }
//            }
//        }
//    }
//}
//void GrowBlackHoles(GridState s)
//{
//    bool[] nextBlackHole = (bool[])IsBlackHole.Clone();
//    int[] dx = { 0, 0, -1, 1 };
//    int[] dy = { -1, 1, 0, 0 };

//    for (int y = 0; y < Grid.Height; y++)
//    {
//        for (int x = 0; x < Grid.Width; x++)
//        {
//            int i = Idx(x, y);
//            if (!IsBlackHole[i]) continue;

//            // For each neighbor, if it's not already a black hole but is adjacent to two or more black holes, convert it
//            int blackHoleNeighbors = 0;
//            for (int d = 0; d < 4; d++)
//            {
//                int nx = x + dx[d];
//                int ny = y + dy[d];
//                if (nx < 0 || nx >= Grid.Width || ny < 0 || ny >= Grid.Height)
//                    continue;
//                int ni = Idx(nx, ny);
//                if (IsBlackHole[ni])
//                    blackHoleNeighbors++;
//            }

//            // If a neighbor is not a black hole but is adjacent to two or more black holes, convert it
//            for (int d = 0; d < 4; d++)
//            {
//                int nx = x + dx[d];
//                int ny = y + dy[d];
//                if (nx < 0 || nx >= Grid.Width || ny < 0 || ny >= Grid.Height)
//                    continue;
//                int ni = Idx(nx, ny);
//                if (!IsBlackHole[ni])
//                {
//                    // If this neighbor is adjacent to at least two black holes, convert it
//                    int neighborBlackHoles = 0;
//                    for (int dd = 0; dd < 4; dd++)
//                    {
//                        int nnx = nx + dx[dd];
//                        int nny = ny + dy[dd];
//                        if (nnx < 0 || nnx >= Grid.Width || nny < 0 || nny >= Grid.Height)
//                            continue;
//                        int nni = Idx(nnx, nny);
//                        if (IsBlackHole[nni])
//                            neighborBlackHoles++;
//                    }
//                    if (neighborBlackHoles >= 2)
//                        nextBlackHole[ni] = true;
//                }
//            }
//        }
//    }
//    IsBlackHole = nextBlackHole;
//}