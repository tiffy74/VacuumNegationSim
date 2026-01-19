using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Vector2Int GridPosition;

    // Core state
    public bool IsInactive = true;       // True if this cell is permanently inactive
    public bool IsSource = false;

    // Resource and maintenance
    public float Resource = 2.0f;        // Total stored resource
    public float Viability = 2.0f;       // Computed each tick from resource / complexity resistance

    public float ComplexityMetric = 0f;  // Current structural complexity (resists viability)
    public float ComplexityNext = 0f;    // Used for diffusion
    public bool IsComplexityLocked = false; // If true, complexity does not diffuse

    // Optional extensions
    public float Symmetry = 1f;          // Placeholder for future symmetry-based viability
    public Cell[] Neighbors;             // Cached 4-way or 8-way neighbors

    public CellVisualiser Visual;

    public float BaseComplexity = 0f;    // seeded once
    public float ComplexityDyn = 0f;     // evolves each tick

    public CellVisualiser Visualiser;    // link to the sprite renderer helper

    public float TotalComplexity
    {
        get { return Mathf.Clamp01(BaseComplexity + ComplexityDyn); }
    }
    
    public Cell(Vector2Int position)
    {
        GridPosition = position;
    }
}