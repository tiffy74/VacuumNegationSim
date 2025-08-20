using System;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class SimulationController : MonoBehaviour
{
    // ===== Budget & viability knobs =====
    [Header("Global Budget")]
    [SerializeField] float NGlobalMax = 10000000000000f;
    [SerializeField] float NGlobal = 5000000000f;     // start half-full
    [SerializeField] float GlobalReplenishPerTick = 25f;
    [SerializeField] float MinEnergyForPersistence = 0.01f;

    [Header("Viability / Threshold")]
    [SerializeField] float EthreshBase = 0.0001f;
    [SerializeField] float GlobalScarcityK = 2f;     // scarcity raises threshold
    [SerializeField] float EntropyPenalty = 0.0000000001f;   // entropy suppresses V
    [SerializeField] float DecayLoss = 0.000000000000001f;  // baseline local decay

    [Header("Propagation")]
    [SerializeField] float PropagateFrac = 0.1f;
    [SerializeField] float MinBudgetToPropagate = 0.0000000001f;
    [SerializeField] float ActivationCost = 0.1f; // draw from global when cell activates



    [Header("Entropy Dynamics")]
    [SerializeField] float EntropyGainPerUse = 0.0001f; // activity -> entropy
    [SerializeField] float EntropyDiffuseRate = 0.20f; // 5-point stencil diffusion
    [SerializeField] float EntropyDecay = 0.1f; // relax toward 0

    [Header("Local Limits")]
    [SerializeField] float NlocalMax = 1000.0f;

    [Header("Visuals")]
    [SerializeField] Color VoidColor = new Color(0.05f, 0.05f, 0.08f, 1f);
    [SerializeField] bool ShowEntropyTint = true;

    // ===== Per-cell state (flattened arrays sized Width*Height) =====
    float[] Nlocal;     // 0..NlocalMax
    float[] Entropy;    // 0..1
    float[] V;          // viability (we'll clamp for color)
    byte[] Active;     // 0/1

    // ===== Working buffers =====
    float[] incoming;     // Pass1 -> Pass2
    float[] entropyNext;  // diffusion ping-pong
    bool[] IsVacuum; // true if cell is a permanent vacuum
    int Idx(int x, int y) => y * Grid.Width + x;
    int[] zeroEnergyTicks;
    private int tick = 0;


    public SimulationGrid Grid;
    public CellVisualiser[,] views;
    private List<Vector2Int> activeCells = new List<Vector2Int>();

    [SerializeField] float ticksPerSecond = 30f;
    bool running = true;

    void Start()
    {
        views = new CellVisualiser[Grid.Width, Grid.Height];
        Grid = GetComponent<SimulationGrid>();
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


        for (int i = 0; i < len; i++) 
        { 
            Nlocal[i] = 0f;
            Entropy[i] = 0f;
            V[i] = 0f;
            Active[i] = 0;
            IsVacuum[i] = false;
            zeroEnergyTicks[i] = 0;
        }

        int cx = Grid.Width / 2, cy = Grid.Height / 2;
        for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
            {
                int x = cx + dx, y = cy + dy;
                if (x >= 0 && x < Grid.Width && y >= 0 && y < Grid.Height)
                {
                    int i = Idx(x, y);
                    Nlocal[i] = 100.0f;
                    incoming[i] = 10.05f;
                    Entropy[i] = 0f;
                    Active[i] = 1;
                }
            }


        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    void TickSimulation()
    {
        
        tick++;
        Debug.Log($"Tick {tick}");

        Pass1_GatherOutflow();

        // Add inflow to all seeded cells
        int cx = Grid.Width / 2, cy = Grid.Height / 2;
        for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
            {
                int x = cx + dx, y = cy + dy;
                if (x >= 0 && x < Grid.Width && y >= 0 && y < Grid.Height)
                    incoming[Idx(x, y)] += 1.0f; // Increase from 0.05f to 0.1f
            }


        Pass2_ApplyAndViability();
        Pass3_GlobalRecharge();
        Pass4_EntropyDiffuse();
        UpdateVisuals();
        

    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    float ComputeViability(float incomingFlow, float nlocal, float entropy)
    {
        float Eloss = DecayLoss + EntropyPenalty * entropy;
        return (incomingFlow - Eloss) / Mathf.Max(1e-6f, EthreshEff); // V = (Ein - Eloss) / Ethresh
    }

    void Pass1_GatherOutflow()
    {
        Array.Clear(incoming, 0, incoming.Length);

        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int i = Idx(x, y);
                if (Nlocal[i] <= MinBudgetToPropagate) continue;
                if (V[i] <= 0f) continue; // only viable cells push outward

                float available = Nlocal[i] * PropagateFrac;
                if (available <= 0f) continue;

                float portion = available * 0.2f; // 4-neighbour split

                if (x > 0 && !IsVacuum[i - 1]) incoming[i - 1] += portion;
                if (x < Grid.Width - 1 && !IsVacuum[i + 1]) incoming[i + 1] += portion;
                if (y > 0 && !IsVacuum[i - Grid.Width]) incoming[i - Grid.Width] += portion;
                if (y < Grid.Height - 1 && !IsVacuum[i + Grid.Width]) incoming[i + Grid.Width] += portion;

                if (portion > 0f)
                    Debug.Log($"Cell ({x},{y}) outflow portion={portion} to neighbors");
            }
        }
    }

    void Pass2_ApplyAndViability()
    {
        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int i = Idx(x, y);
                if (IsVacuum[i]) continue; // Skip vacuums

                int persistenceConfigs = CountPersistenceConfigurations(i);
                int totalConfigs = 16; // 2^4 for 4 neighbors
                float entropy = Mathf.Log(1 + persistenceConfigs) / Mathf.Log(1 + totalConfigs);

                // Occasionally, force high entropy and low energy
                if (UnityEngine.Random.value < 0.01f)
                {
                    entropy = 1f;
                    Nlocal[i] = 0f; // cell becomes vacuum
                }

                // Spend outflow now (same rule used in Pass1)
                float outflow = (V[i] > 0f) ? (Nlocal[i] * PropagateFrac) : 0f;
                Nlocal[i] = Mathf.Max(0f, Nlocal[i] - outflow);

                float inFlow = incoming[i];

                // Activation kick from global pool if toggling on
                if (Active[i] == 0 && inFlow > 0f && NGlobal > 0f)
                {
                    float draw = Mathf.Min(ActivationCost, NGlobal);
                    NGlobal -= draw;
                    inFlow += draw;
                }

                // Update local budget (cap)
                Nlocal[i] = Mathf.Min(NlocalMax, Nlocal[i] + inFlow);

                // Entropy grows with activity
                float cappedActivity = Mathf.Min(outflow + inFlow, 1.0f);
                Entropy[i] = Mathf.Clamp01(entropy + EntropyGainPerUse);
                
                // Viability
                V[i] = ComputeViability(inFlow, Nlocal[i], Entropy[i]);

                // State
                if (V[i] > 0.01f && Nlocal[i] > MinBudgetToPropagate) Active[i] = 1;
                //else if (V[i] <= 0f) Active[i] = 0;


                // Baseline local decay
                Nlocal[i] = Mathf.Max(Nlocal[i] - DecayLoss);

                // Attraction bonus for non-viable, non-vacuum cells
                int activeNeighbors = 0;
                if (x > 0 && Active[i - 1] == 1) activeNeighbors++;
                if (x < Grid.Width - 1 && Active[i + 1] == 1) activeNeighbors++;
                if (y > 0 && Active[i - Grid.Width] == 1) activeNeighbors++;
                if (y < Grid.Height - 1 && Active[i + Grid.Width] == 1) activeNeighbors++;

                if (!IsVacuum[i] && V[i] <= 0f && Nlocal[i] > 0f)
                    Nlocal[i] = Mathf.Min(0.5f, Nlocal[i] + 0.05f * activeNeighbors); // cap at 0.5f for static cells

                // Set vacuum status after all updates
                if (Nlocal[i] <= 0f && incoming[i] <= 0f)
                    zeroEnergyTicks[i]++;
                else
                    zeroEnergyTicks[i] = 0;

                //if (zeroEnergyTicks[i] > 5) // e.g., 5 ticks of zero energy
                //    IsVacuum[i] = true;
            }
        }
    }
    void Pass3_GlobalRecharge()
    {
        NGlobal = Mathf.Min(NGlobalMax, NGlobal + GlobalReplenishPerTick);
    }
    void Pass4_EntropyDiffuse()
    {
        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int i = Idx(x, y);

                float c = Entropy[i];
                float n = (y > 0) ? Entropy[i - Grid.Width] : c;
                float s = (y < Grid.Height - 1) ? Entropy[i + Grid.Width] : c;
                float w = (x > 0) ? Entropy[i - 1] : c;
                float e = (x < Grid.Width - 1) ? Entropy[i + 1] : c;

                float lap = (n + s + w + e - 4f * c);
                float diffused = c + EntropyDiffuseRate * lap;
                diffused = Mathf.Max(0f, diffused - EntropyDecay);
                entropyNext[i] = Mathf.Clamp01(diffused);
            }
        }
        // Commit
        for (int i = 0; i < entropyNext.Length; i++) Entropy[i] = entropyNext[i];
    }

    //void InjectNegationBurst(Vector2Int position)
    //{
    //    Cell cell = Grid.GetCell(position);
    //    if (cell == null) return;

    //    cell.IsVacuum = false;
    //    cell.Energy = 50.0f; // Strong burst to ensure propagation
    //    cell.Viability = ComputeViability(inFlow, Nlocal[i], Entropy[i]);
    //    cell.IsNegationSource = true;

    //    GameObject visualGO = GameObject.Find($"Cell_{position.x}_{position.y}");
    //    if (visualGO != null)
    //    {
    //        var visual = visualGO.GetComponent<CellVisualiser>();
    //        visual?.SetColor(Color.red);
    //    }
    //    activeCells.Clear();
    //    activeCells.Add(position);
    //    Debug.Log($"Negation burst injected at {position.x}, {position.y}");
    //}

    Color Colorize(int i)
    {
        float v = Mathf.Clamp01(V[i]);       // activity / persistence
        float e = Mathf.Clamp01(Entropy[i]); // inhibition
        float g = Mathf.Clamp01(Nlocal[i]);  // reserves

        if (Active[i] == 0 && v <= 0.01f && g <= 0.01f)
            return VoidColor;

        // base luminance from viability
        float baseL = Mathf.Lerp(0.1f, 1f, v);

        // entropy adds red tint (optional), reserves add faint green
        float r = Mathf.Clamp01(baseL + (ShowEntropyTint ? 0.6f * e : 0f));
        float gb = Mathf.Clamp01(baseL * (1f - 0.4f * (ShowEntropyTint ? e : 0f)));
        float gBoost = 0.3f * g;

        return new Color(r, Mathf.Clamp01(gb + gBoost), gb, 1f);
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

                // Use both viability and entropy for visualization
                if (IsVacuum[i])
                    vis.SetColor(Color.black); // or VoidColor
                else if (V[i] <= 0f)
                    vis.SetColor(new Color(0.2f, 0.0f, 0.2f, 1f)); // static, non-viable, non-vacuum (purple-ish)
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

    float SampleConfigurationEnergy(int cellIndex, int config)
    {
        // Example: random or deterministic function
        // Could use Perlin noise, neighbor states, etc.
        return UnityEngine.Random.Range(0f, NlocalMax);
    }
    //private float ComputeViability(Cell c)
    //{
    //    // Basic viability: higher energy & lower entropy = higher viability
    //    float entropy = c.TotalEntropy; // we just added this property
    //    float threshold = 50f; // arbitrary test threshold for now

    //    float raw = (c.Energy - entropy * threshold) / threshold;
    //    float v = Mathf.Clamp01(raw);

    //    // Debug log for center cell every 30 frames
    //    if (c.GridPosition.x == Grid.Width / 2 && c.GridPosition.y == Grid.Height / 2 && Time.frameCount % 30 == 0)
    //    {
    //        Debug.Log($"[VIABILITY] Center -> E={c.Energy:F2}, Entropy={entropy:F3}, V={v:F3}");
    //    }

    //    return v;
    //}
}
