using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class SimulationController : MonoBehaviour
{
    public SimulationGrid Grid;
    private List<Vector2Int> activeCells = new List<Vector2Int>();

    void Start()
    {
        Grid = GetComponent<SimulationGrid>();

        // Inject an initial negation burst in the center
        InjectNegationBurst(new Vector2Int(Grid.Width / 2, Grid.Height / 2));
        // SeedEntropyWells(10); // 10 random high-entropy spots
        // SeedEntropyTerrain(0.12f, 1.0f); // tweak scale for smoothness, amplitude for max entropy
    }

    void Update()
    {
        TickSimulation();
    }

    void TickSimulation()
    {
        List<Vector2Int> nextActiveCells = new List<Vector2Int>();
        foreach (var pos in activeCells)
        {
            Cell cell = Grid.GetCell(pos);
            if (cell == null || cell.IsVacuum)
                continue;
            cell.Viability = ComputeViability(cell);
            if (cell.Viability <= 0.01f)
                continue;

            // Find the most attractive neighbor(s)
            float maxAttractiveness = float.MinValue;
            Cell mostAttractiveNeighbor = null;
            float energyWeight = 1f; // Tune this value

            foreach (Cell neighbor in cell.Neighbors)
            {
                if (neighbor == null || !neighbor.IsVacuum)
                    continue;

                float attractiveness = neighbor.Entropy - neighbor.Energy * energyWeight;
                if (attractiveness > maxAttractiveness)
                {
                    maxAttractiveness = attractiveness;
                    mostAttractiveNeighbor = neighbor;
                }
            }

            // Only propagate to the most attractive neighbor (or you can propagate to all above a threshold)
            if (mostAttractiveNeighbor != null)
            {
                float propagatedEnergy = cell.Energy * 0.95f;
                mostAttractiveNeighbor.IsVacuum = false;
                mostAttractiveNeighbor.Energy = propagatedEnergy;
                mostAttractiveNeighbor.Viability = ComputeViability(mostAttractiveNeighbor);

                // Dynamic entropy growth as before
                float entropyIncrease = Mathf.PerlinNoise(
                    mostAttractiveNeighbor.GridPosition.x * 0.1f,
                    mostAttractiveNeighbor.GridPosition.y * 0.1f
                ) * 0.2f;
                mostAttractiveNeighbor.Entropy += entropyIncrease;
                mostAttractiveNeighbor.Entropy = Mathf.Clamp01(mostAttractiveNeighbor.Entropy);

                // Visual update (optional)
                var vgo = GameObject.Find($"Cell_{mostAttractiveNeighbor.GridPosition.x}_{mostAttractiveNeighbor.GridPosition.y}");
                if (vgo != null)
                {
                    var v = vgo.GetComponent<CellVisualiser>();
                    v?.SetViabilityWithEntropy(ComputeViability(mostAttractiveNeighbor), mostAttractiveNeighbor.Entropy);
                }

                nextActiveCells.Add(mostAttractiveNeighbor.GridPosition);
            }

            // Decay energy of source cell
            cell.Energy *= 0.995f;
            cell.Viability = ComputeViability(cell);
            cell.Entropy += 0.0002f;
            if (cell.Viability > 0.1f)
            {
                CellVisualiser visual = cell.Visualiser;
                visual.SetViabilityWithEntropy(ComputeViability(cell), cell.Entropy);
                nextActiveCells.Add(cell.GridPosition);
            }
        }

        // Update queue
        activeCells = nextActiveCells;

        

        // Second pass: Entropy updates and visual feedback
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                var cell = Grid.Grid[x, y];
                if (cell == null) continue;

                cell.Entropy += 0.01f;
                if (cell.Entropy > 1f)
                    cell.Entropy = 1f;

                GameObject visualGO = GameObject.Find($"Cell_{x}_{y}");
                if (visualGO != null)
                {
                    var visual = visualGO.GetComponent<CellVisualiser>();

                    if (cell.IsVacuum)
                    {
                        float brightness = Mathf.Lerp(0.2f, 0.7f, 1 - cell.Entropy);
                        visual.SetColor(new Color(brightness, brightness, brightness));
                    }
                    else
                    {
                        visual.SetCombinedColor(visual.GetViabilityColor(ComputeViability(cell)), cell.Entropy);

                    }
                }

            }
        }
    }

    void InjectNegationBurst(Vector2Int position)
    {
        Cell cell = Grid.GetCell(position);
        if (cell == null) return;

        cell.IsVacuum = false;
        cell.Energy = 50.0f; // Strong burst to ensure propagation
        cell.Viability = ComputeViability(cell);
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

    private float ComputeViability(Cell c)
    {
        // Basic viability: higher energy & lower entropy = higher viability
        float entropy = c.TotalEntropy; // we just added this property
        float threshold = 50f; // arbitrary test threshold for now

        float raw = (c.Energy - entropy * threshold) / threshold;
        float v = Mathf.Clamp01(raw);

        // Debug log for center cell every 30 frames
        if (c.GridPosition.x == Grid.Width / 2 && c.GridPosition.y == Grid.Height / 2 && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[VIABILITY] Center -> E={c.Energy:F2}, Entropy={entropy:F3}, V={v:F3}");
        }

        return v;
    }
}
