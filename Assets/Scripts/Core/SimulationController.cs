using Assets.Scripts.Events;
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

    // ===== Budget & viability knobs =====
    [Header("Global Budget")]
    [SerializeField] public float NGlobalMax = 2e10f;
    [SerializeField] public float NGlobal = 1e9f;
    [SerializeField] public float GlobalReplenishPerTick = 100;
    [SerializeField] public float MinEnergyForPersistence = 1f;

    [Header("Viability / Threshold")]
    [SerializeField] public float EthreshBase = 10f;
    [SerializeField] public float GlobalScarcityK = 2f;
    [SerializeField] public float EntropyPenalty = 0.01f;
    [SerializeField] public float DecayLoss = 0.0001f;

    [Header("Propagation")]
    [SerializeField] public float PropagateFrac = 0.6f;
    [SerializeField] public float MinBudgetToPropagate = 0.01f;
    [SerializeField] public float ActivationCost = 1e3f;

    [Header("Entropy Dynamics")]
    [SerializeField] public float EntropyGainPerUse = 0.001f;
    [SerializeField] public float EntropyDiffuseRate = 0.001f;
    [SerializeField] public float EntropyDecay = 0.01f;

    [Header("Local Limits")]
    [SerializeField] public float NlocalMax = 2e7f;
    [SerializeField] public float VacuumEventProbability = 0.001f; // Probability per cell
    [SerializeField] public float VacuumEventEntropy = 1f; // The amount of entropy per vacuum event
    [SerializeField] public float ExpansionRate = 1.01f; // Scale factor per tick

    [Header("Colors")]
    [SerializeField] public Color VoidColor = new Color(0.05f, 0.05f, 0.08f, 1f);
    [SerializeField] public bool ShowEntropyTint = true;
    [SerializeField] public Color VacuumEnergyColor = new Color(0.48f, 0.25f, 0.52f, 1f);
    [SerializeField] public Color NullspaceColor = new Color(0.15f, 0.0f, 0.25f, 1f);
    [SerializeField] public float ScaleFactor = 1.0f; // Initial scale
    [SerializeField] public float SpatialThreshK = -0.6f;
    [SerializeField] public float SpatialDecayK = 0.5f;

    // In SimulationController.cs
    private float[] BlackHoleCharge;
    // ===== Per-cell state (flattened arrays sized Width*Height) =====
    public float[] Nlocal;     // 0..NlocalMax
    public float[] Entropy;    // 0..1
    public float[] V;          // viability (we'll clamp for color)
    public byte[] Active;     // 0/1
    public float[] rNorm;
    private bool[] IsBlackHole;
    private bool[] FieldPresent; // true if field/configuration space is present
    public const float MatterAheadThreshold = 0.1f;
    public int[] FieldFirstTick;   // -1 = not yet private
    public int[] EnergyFirstTick;  // -1 = no energy yet
    // ===== Working buffers =====
    float[] incoming;     // Pass1 -> Pass2
    float[] entropyNext;  // diffusion ping-pong
    bool[] IsVacuum; // true if cell is a permanent vacuum
    int Idx(int x, int y) => y * Grid.Width + x;
    int[] zeroEnergyTicks;
    public int tick = 0;

    public SimulationGrid Grid;
    public CellVisualiser[,] views;
    private List<Vector2Int> activeCells = new List<Vector2Int>();

    [SerializeField] float ticksPerSecond = 5f;
    bool running = true;

    void Start()
    {
        Grid = GetComponent<SimulationGrid>();
        views = new CellVisualiser[Grid.Width, Grid.Height];
        Grid.SpawnVisualCells(views); // Pass the array to be filled
        InitState();
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
            float scarcity = 1f - (NGlobal / Mathf.Max(1f, NGlobalMax));
            return EthreshBase * (1f + GlobalScarcityK * scarcity);
        }
    }

    void InitState()
    {
        int len = Grid.Width * Grid.Height;

        Nlocal = new float[len];
        Entropy = new float[len];
        V = new float[len];
        Active = new byte[len];
        incoming = new float[len];
        entropyNext = new float[len];
        IsVacuum = new bool[len];
        zeroEnergyTicks = new int[len];
        IsBlackHole = new bool[Grid.Width * Grid.Height];
        for (int i = 0; i < IsBlackHole.Length; i++)
            IsBlackHole[i] = false;

        for (int i = 0; i < len; i++)
        {
            Nlocal[i] = 0f;
            Entropy[i] = 0f;
            V[i] = 0f;
            Active[i] = 0;
            IsVacuum[i] = false;
            zeroEnergyTicks[i] = 0;
        }

        // allocate and clear field presence
        FieldPresent = new bool[Grid.Width * Grid.Height];
        for (int i = 0; i < FieldPresent.Length; i++)
            FieldPresent[i] = false;

        // seed a small central block with energy and field
        int cx = Grid.Width / 2;
        int cy = Grid.Height / 2;
        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                int x = cx + dx;
                int y = cy + dy;
                if (x < 0 || x >= Grid.Width || y < 0 || y >= Grid.Height)
                    continue;

                int i = Idx(x, y);
                Nlocal[i] = 50f;       // starting energy
                incoming[i] = 0f;
                Entropy[i] = 0f;
                Active[i] = 1;
                FieldPresent[i] = true;
            }
        }

        BlackHoleCharge = new float[Grid.Width * Grid.Height];
        for (int i = 0; i < BlackHoleCharge.Length; i++)
            BlackHoleCharge[i] = 0f;

        FieldFirstTick = new int[len];
        EnergyFirstTick = new int[len];
        for (int i = 0; i < len; i++) { FieldFirstTick[i] = -1; EnergyFirstTick[i] = -1; }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    void TickSimulation()
    {
        // 1. Expand the field wave first
        PropagateFieldWave();

        // Pass the required arguments to GatherOutflow  
        Assets.Scripts.Events.Pass1.GatherOutflow(
            Grid.Width,
            Grid.Height,
            Nlocal,
            V,
            IsVacuum,
            incoming,
            MinBudgetToPropagate,
            PropagateFrac,
            FieldPresent,
            IsBlackHole,
            BlackHoleCharge,
            200.0f,
            MatterAheadThreshold,
            FieldFirstTick,
            EnergyFirstTick
        );

        // Add inflow to all seeded cells  
        int cx = Grid.Width / 2, cy = Grid.Height / 2;
        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                int x = cx + dx;
                int y = cy + dy;
                if (x >= 0 && x < Grid.Width && y >= 0 && y < Grid.Height)
                {
                    int i = Idx(x, y);

                    // continuous inflow to the central patch
                    incoming[i] += 0.1f;

                    // ensure these stay as seeded/field cells (optional but consistent)
                    Nlocal[i] = Mathf.Max(Nlocal[i], 50f);
                    Active[i] = 1;
                    FieldPresent[i] = true;
                    if (FieldFirstTick[i] == -1) FieldFirstTick[i] = tick;
                    if (EnergyFirstTick[i] == -1) EnergyFirstTick[i] = tick;
                }
            }
        }



        // Pass the required arguments to ApplyAndViability  
        Assets.Scripts.Events.Pass2.ApplyAndViability(
            Idx,
            Grid.Width,
            Grid.Height,
            Nlocal,
            Entropy,
            V,
            Active,
            IsVacuum,
            incoming,
            zeroEnergyTicks,
            MinBudgetToPropagate,
            ActivationCost,
            ref NGlobal,
            NlocalMax,
            EntropyGainPerUse,
            DecayLoss,
            EntropyPenalty,
            VacuumEventProbability,
            VacuumEventEntropy,
            ComputeViability,
            CountPersistenceConfigurations,
            Grid.Width,
            PropagateFrac,
            FieldPresent,
            IsBlackHole,
            FieldFirstTick,
            EnergyFirstTick
        );
        BlackHoleAttractEnergy();
        // after Pass2.ApplyAndViability(...)
        //for (int i = 0; i < IsBlackHole.Length; i++)
        //{
        //    if (!IsBlackHole[i])
        //    {
        //        continue;
        //    }

        //    Nlocal[i] = 0f;
        //    V[i] = 0f;
        //    Active[i] = 0;
        //    Entropy[i] = 1f; // or some high entropy to mark them
        //}

        

        // Fix for CS7036: Pass the required arguments to GlobalRecharge  
        Assets.Scripts.Events.Pass3.GlobalRecharge(
        ref NGlobal,
        NGlobalMax,
        GlobalReplenishPerTick
        );

        ScaleFactor *= ExpansionRate;
        tick++;
        Debug.Log($"Tick {tick}");

        Assets.Scripts.Events.Pass4.EntropyDiffuse(
            Grid.Width,
            Grid.Height,
            Entropy,
            entropyNext,
            EntropyDiffuseRate,
            EntropyDecay
        );
        // GrowBlackHoles();
        UpdateVisuals();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    float ComputeViability(float incomingFlow, float nlocal, float entropy)
    {
        float Eloss = DecayLoss + EntropyPenalty * entropy;
        return (incomingFlow - Eloss) / Mathf.Max(MinViabilityEpsilon, EthreshEff); // V = (Ein - Eloss) / Ethresh
    }


    void InjectNegationBurst(Vector2Int position, int i)
    {
        Cell cell = Grid.GetCell(position);
        if (cell == null) return;

        cell.IsVacuum = false;
        cell.Energy = 50.0f; // Strong burst to ensure propagation
        float inFlow = 0;
        cell.Viability = ComputeViability(inFlow, Nlocal[i], Entropy[i]);
        cell.IsNegationSource = true;

        GameObject visualGO = GameObject.Find($"Cell_{position.x}_{position.y}");
        if (visualGO != null)
        {
            var visual = visualGO.GetComponent<CellVisualiser>();
            visual?.SetColor(Color.red);
        }
        activeCells.Clear();
        activeCells.Add(position);
        Debug.Log($"Negation burst injected at {position.x}, {position.y}");
    }

    void PropagateFieldWave()
    {
        bool[] nextField = (bool[])FieldPresent.Clone();
        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int i = Idx(x, y);
                if (!FieldPresent[i])
                {
                    // If any neighbor has the field, this cell may get it (with randomness)
                    int[] dx = { 0, 0, -1, 1 };
                    int[] dy = { -1, 1, 0, 0 };
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d], ny = y + dy[d];
                        if (nx >= 0 && nx < Grid.Width && ny >= 0 && ny < Grid.Height)
                        {
                            int ni = Idx(nx, ny);
                            if (FieldPresent[ni] && UnityEngine.Random.value < 0.05f) // 20% chance to expand
                            {
                                nextField[i] = true;
                                if (!FieldPresent[i] && FieldPresent[ni] && UnityEngine.Random.value < 0.2f)
                                {
                                    nextField[i] = true; if (FieldFirstTick[i] == -1) FieldFirstTick[i] = tick;   // current global tick break; }
                                }
                                    break;
                            }
                        }
                    }
                }
            }
        }
        FieldPresent = nextField;
    }
    void BlackHoleAttractEnergy()
    {
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };
        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int i = Idx(x, y);
                if (!IsBlackHole[i]) continue;

                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx < 0 || nx >= Grid.Width || ny < 0 || ny >= Grid.Height)
                        continue;
                    int ni = Idx(nx, ny);
                    if (!IsBlackHole[ni])
                    {
                        // Drain a fraction of neighbor's energy into the black hole
                        float absorbed = Nlocal[ni] * 0.2f; // 20% per tick, adjust as needed
                        Nlocal[ni] -= absorbed;
                        // Optionally, you can accumulate this in the black hole, or just let it vanish
                    }
                }
            }
        }
    }
    void GrowBlackHoles()
    {
        bool[] nextBlackHole = (bool[])IsBlackHole.Clone();
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int i = Idx(x, y);
                if (!IsBlackHole[i]) continue;

                // For each neighbor, if it's not already a black hole but is adjacent to two or more black holes, convert it
                int blackHoleNeighbors = 0;
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx < 0 || nx >= Grid.Width || ny < 0 || ny >= Grid.Height)
                        continue;
                    int ni = Idx(nx, ny);
                    if (IsBlackHole[ni])
                        blackHoleNeighbors++;
                }

                // If a neighbor is not a black hole but is adjacent to two or more black holes, convert it
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx < 0 || nx >= Grid.Width || ny < 0 || ny >= Grid.Height)
                        continue;
                    int ni = Idx(nx, ny);
                    if (!IsBlackHole[ni])
                    {
                        // If this neighbor is adjacent to at least two black holes, convert it
                        int neighborBlackHoles = 0;
                        for (int dd = 0; dd < 4; dd++)
                        {
                            int nnx = nx + dx[dd];
                            int nny = ny + dy[dd];
                            if (nnx < 0 || nnx >= Grid.Width || nny < 0 || nny >= Grid.Height)
                                continue;
                            int nni = Idx(nnx, nny);
                            if (IsBlackHole[nni])
                                neighborBlackHoles++;
                        }
                        if (neighborBlackHoles >= 2)
                            nextBlackHole[ni] = true;
                    }
                }
            }
        }
        IsBlackHole = nextBlackHole;
    }
    void UpdateVisuals()
    {
        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int i = Idx(x, y);
                var vis = views[x, y];
                if (vis == null) continue;

                if (IsBlackHole[i])
                    vis.SetColor(Color.black);
                else if (FieldPresent[i] && (Active[i] == 0 || V[i] <= 0f))
                    vis.SetColor(Color.yellow);
                else if (IsVacuum[i] || Active[i] == 0 || V[i] <= 0f)
                    vis.SetColor(VoidColor);
                else
                    vis.SetViabilityWithEntropy(V[i], Entropy[i]);

                // Optional: debug for center cell
                if (x == Grid.Width / 2 && y == Grid.Height / 2)
                    Debug.Log($"Center: Active={Active[i]}, V={V[i]}, Nlocal={Nlocal[i]}, Entropy={Entropy[i]}");
                if (x == Grid.Width / 2 + 1 && y == Grid.Height / 2)
                    Debug.Log($"Right Neighbor: Active={Active[i]}, V={V[i]}, Nlocal={Nlocal[i]}, Entropy={Entropy[i]}");
            }
        }
    }

    int CountPersistenceConfigurations(int cellIndex)
    {
        int count = 0;
        int x = cellIndex % Grid.Width;
        int y = cellIndex / Grid.Width;

        // 4 neighbors: up, down, left, right
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        // There are 16 possible neighbor activation states (2^4)
        for (int config = 0; config < 16; config++)
        {
            float simulatedEnergy = Nlocal[cellIndex];

            for (int n = 0; n < 4; n++)
            {
                bool neighborActive = ((config >> n) & 1) == 1;
                int nx = x + dx[n];
                int ny = y + dy[n];

                // Bounds check
                if (nx < 0 || nx >= Grid.Width || ny < 0 || ny >= Grid.Height)
                    continue;

                int neighborIdx = Idx(nx, ny);

                if (neighborActive)
                {
                    // Use actual neighbor energy for inflow
                    simulatedEnergy += PropagateFrac * Nlocal[neighborIdx] * 0.25f;
                }
            }

            if (simulatedEnergy > MinEnergyForPersistence)
                count++;
        }

        return count;
    }
    public void RestartSimulation()
    {
        InitState();
    }
}