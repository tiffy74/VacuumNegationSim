using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Vector2Int GridPosition;      // Grid coordinates (x, y)

    // Core state
    public bool IsVacuum = true;         // True if this cell is not currently negated
    public bool IsNegationSource = false;

    // Thermodynamic fields
    public float Energy = 2.0f;            // Total stored energy
    public float Viability = 2.0f;         // Computed each tick from Energy / Entropy resistance

    public float Entropy = 0f;           // Current entropy (resists viability)
    public float EntropyNext = 0f;       // Used for diffusion
    public bool IsEntropyLocked = false; // If true, entropy does not diffuse (e.g. shield walls)

    // Optional extensions
    public float Symmetry = 1f;          // Placeholder for future symmetry-based viability
    public Cell[] Neighbors;             // Cached 4-way or 8-way neighbors
    public bool IsSource = false;
    
    // public List<Cell> Neighbors = new List<Cell>();
    public CellVisualiser Visual;

    public Cell(Vector2Int position)
    {
        GridPosition = position;
    }
}
