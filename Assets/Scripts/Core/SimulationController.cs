using System.Collections.Generic;
using UnityEngine;

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
        SeedEntropyTerrain(0.12f, 1.0f); // tweak scale for smoothness, amplitude for max entropy
        InjectNegationBurst(new Vector2Int(Grid.Width / 2, Grid.Height / 2));
    }

    void Update()
    {
        TickSimulation();
    }

    //void TickSimulation()
    //{
    //    PropagateActiveCells();
    //    UpdateEntropyAndColors();
    //}

    void PropagateActiveCells()
    {
        List<Vector2Int> nextActive = new List<Vector2Int>();

        foreach (var pos in activeCells)
        {
            Cell cell = Grid.GetCell(pos);
            if (cell == null || cell.IsVacuum || cell.Viability <= 1f)
                continue;

            foreach (Cell neighbor in cell.Neighbors)
            {
                if (neighbor == null || !neighbor.IsVacuum)
                    continue;

                float propagatedEnergy = cell.Energy * 0.95f;
                float entropyPenalty = neighbor.Entropy;
                float propagatedViability = (propagatedEnergy - entropyPenalty * 0.25f) / 1.0f;

                if (propagatedViability > 1f)
                {
                    neighbor.IsVacuum = false;
                    neighbor.Energy = propagatedEnergy;
                    neighbor.Viability = propagatedViability;
                    nextActive.Add(neighbor.GridPosition);
                }
            }

            cell.Energy *= 0.995f;
            cell.Viability = cell.Energy / 1.0f;
        }

        activeCells = nextActive;
    }
    void UpdateEntropyAndColors()
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                var cell = Grid.Grid[x, y];
                if (cell == null) continue;

                // Passive entropy growth
                cell.Entropy += 0.0005f;
                if (cell.Entropy > 1f)
                    cell.Entropy = 1f;

                object updatedVisual = UpdateCellVisual(cell, x, y);
            }
        }
    }
    void TickSimulation()
    {
        List<Vector2Int> nextActiveCells = new List<Vector2Int>();

        foreach (var pos in activeCells)
        {
            Cell cell = Grid.GetCell(pos);
            if (cell == null || cell.IsVacuum || cell.Viability <= 1f)
                continue;

            foreach (Cell neighbor in cell.Neighbors)
            {
                if (neighbor == null || !neighbor.IsVacuum)
                    continue;

                float propagatedEnergy = cell.Energy * 0.95f;
                float entropyPenalty = neighbor.Entropy;
                float propagatedViability = (propagatedEnergy - entropyPenalty * 0.25f) / 1.0f;

                if (propagatedViability > 1f)
                {
                    neighbor.IsVacuum = false;
                    neighbor.Energy = propagatedEnergy;
                    neighbor.Viability = propagatedViability;

                    // Add to queue for next tick
                    nextActiveCells.Add(neighbor.GridPosition);

                    GameObject visualGO = GameObject.Find($"Cell_{neighbor.GridPosition.x}_{neighbor.GridPosition.y}");
                    
                }
            }

            // Decay energy of source cell
            cell.Energy *= 0.995f;
            cell.Viability = cell.Energy / 1.0f;
            cell.Entropy += 0.0002f;
            Debug.Log($"cell.Energy = {cell.Energy}, cell.Viability = {cell.Viability}");
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

                cell.Entropy += 0.002f;
                if (cell.Entropy > 1f)
                    cell.Entropy = 1f;

                GameObject visualGO = GameObject.Find($"Cell_{x}_{y}");
                if (visualGO != null)
                {
                    var visual = visualGO.GetComponent<CellVisualizer>();

                    if (cell.IsVacuum)
                    {
                        // Vacuum: gray-scale based on entropy
                        float brightness = Mathf.Lerp(0.2f, 0.7f, 1 - cell.Entropy);
                        visual.SetColor(new Color(brightness, brightness, brightness));
                    }
                    else
                    {
                        // 1. First get the viability-based color
                        Color viabilityColor = visual.GetViabilityColor(cell.Viability); // NEW METHOD

                        // 2. Fade that color toward gray based on entropy
                        float entropyFade = Mathf.Clamp01(cell.Entropy);
                        Color finalColor = Color.Lerp(viabilityColor, Color.gray, entropyFade);

                        visual.SetColor(finalColor);
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
        cell.Energy = 500.0f; // Strong burst to ensure propagation
        cell.Viability = 500.0f;
        cell.IsNegationSource = true;

        GameObject visualGO = GameObject.Find($"Cell_{position.x}_{position.y}");
        if (visualGO != null)
        {
            var visual = visualGO.GetComponent<CellVisualizer>();
            visual?.SetColor(Color.red);
        }
        activeCells.Clear();
        activeCells.Add(position);
        Debug.Log($"Negation burst injected at {position.x}, {position.y}");
    }

    void SeedEntropyWells(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(5, Grid.Width - 5);
            int y = Random.Range(5, Grid.Height - 5);
            Cell cell = Grid.Grid[x, y];
            if (cell != null)
            {
                cell.Entropy = 1.0f;
                Debug.Log($"Entropy well seeded at {x}, {y}");
            }
        }
    }

    void SeedEntropyTerrain(float scale = 0.1f, float amplitude = 1.0f)
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                float noise = Mathf.PerlinNoise(x * scale, y * scale); // smooth noise
                Cell cell = Grid.Grid[x, y];
                if (cell != null)
                    cell.Entropy = noise * amplitude;
            }
        }

        Debug.Log("Entropy terrain seeded using Perlin noise.");
    }

    // Add the missing UpdateCellVisual method to resolve the CS0103 error.  
    private object UpdateCellVisual(Cell cell, int x, int y)
    {
        GameObject visualGO = GameObject.Find($"Cell_{x}_{y}");
        if (visualGO != null)
        {
            var visual = visualGO.GetComponent<CellVisualizer>();
            if (cell.IsVacuum)
            {
                // Vacuum: gray-scale based on entropy  
                float brightness = Mathf.Lerp(0.2f, 0.7f, 1 - cell.Entropy);
                visual.SetColor(new Color(brightness, brightness, brightness));
            }
            else
            {
                // 1. First get the viability-based color  
                Color viabilityColor = visual.GetViabilityColor(cell.Viability);

                // 2. Fade that color toward gray based on entropy  
                float entropyFade = Mathf.Clamp01(cell.Entropy);
                Color finalColor = Color.Lerp(viabilityColor, Color.gray, entropyFade);

                visual.SetColor(finalColor);
            }
        }

        return visualGO;
    }
}
