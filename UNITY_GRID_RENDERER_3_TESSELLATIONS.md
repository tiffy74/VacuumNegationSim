# ?? Unity Grid Renderer: 3 Tessellation Visual Implementation

## **Current State: Square-Only Rendering**

### **Problem:**
- Unity currently renders **only square cells** (rectangular tessellation)
- When Triangular or Hexagonal topology is selected, the **logic changes** but **visuals don't**
- Result: Confusing - user sees squares but simulation uses different neighbor connectivity

### **Solution:**
- Implement **dynamic sprite switching** based on `TopologyMode`
- Create sprite assets for all 3 tessellations
- Update grid renderer to position cells correctly for each topology

---

## **?? Implementation Checklist**

### **Phase 1: Sprite Assets (Unity Editor)**
- [ ] Create Triangular cell sprite (equilateral triangle)
- [ ] Create Hexagonal cell sprite (regular hexagon)
- [ ] Ensure sprites have correct pivot points
- [ ] Create color gradient materials for each shape

### **Phase 2: Code Changes**
- [ ] Update `GridVisualizer` to handle 3 tessellations
- [ ] Implement topology-specific cell positioning
- [ ] Add sprite switching based on `TopologyMode`
- [ ] Update camera bounds for different tessellation shapes

### **Phase 3: Testing**
- [ ] Verify rectangular grid (baseline)
- [ ] Verify triangular grid displays correctly
- [ ] Verify hexagonal grid displays correctly
- [ ] Test switching between topologies at runtime

---

## **?? Phase 1: Creating Sprite Assets**

### **Option A: Procedural Sprites (Recommended)**

**Advantage:** No external assets needed, perfect geometric shapes

```csharp
// In GridVisualizer.cs or new SpriteGenerator.cs
using UnityEngine;

public static class GridSpriteGenerator
{
    /// <summary>
    /// Generate a square sprite (current default).
    /// </summary>
    public static Sprite GenerateSquareSprite(int resolution = 64)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
        
        // Fill with white (will be tinted by cell color)
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(
            texture,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f), // Pivot at center
            resolution
        );
    }
    
    /// <summary>
    /// Generate an equilateral triangle sprite.
    /// Pointing UP for triangular tessellation.
    /// </summary>
    public static Sprite GenerateTriangleSprite(int resolution = 64)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
        
        // Initialize to transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        // Draw equilateral triangle
        // Vertices: Top (center, top), Bottom-left, Bottom-right
        float height = resolution * 0.866f; // sqrt(3)/2 for equilateral
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Convert to centered coordinates
                float px = x - resolution / 2f;
                float py = y - resolution / 4f; // Shift up 1/4 for better centering
                
                // Equilateral triangle bounds
                // Top vertex: (0, height/2)
                // Bottom-left: (-width/2, -height/2)
                // Bottom-right: (width/2, -height/2)
                
                bool insideTriangle = 
                    py <= height / 2f && // Below top vertex
                    py >= -height / 2f && // Above bottom edge
                    Mathf.Abs(px) <= (height / 2f - py) / Mathf.Sqrt(3); // Within angled sides
                
                if (insideTriangle)
                    pixels[y * resolution + x] = Color.white;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(
            texture,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f), // Pivot at center
            resolution
        );
    }
    
    /// <summary>
    /// Generate a regular hexagon sprite.
    /// Flat-top orientation (common for hexagonal grids).
    /// </summary>
    public static Sprite GenerateHexagonSprite(int resolution = 64)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
        
        // Initialize to transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        // Draw regular hexagon (flat-top)
        float radius = resolution / 2f - 2; // Padding
        float centerX = resolution / 2f;
        float centerY = resolution / 2f;
        
        // Hexagon vertices (flat-top orientation)
        Vector2[] hexVertices = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.PI / 3f * i; // 60° increments
            hexVertices[i] = new Vector2(
                centerX + radius * Mathf.Cos(angle),
                centerY + radius * Mathf.Sin(angle)
            );
        }
        
        // Fill hexagon using point-in-polygon test
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                if (IsPointInPolygon(new Vector2(x, y), hexVertices))
                    pixels[y * resolution + x] = Color.white;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(
            texture,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f), // Pivot at center
            resolution
        );
    }
    
    /// <summary>
    /// Point-in-polygon test (ray casting algorithm).
    /// </summary>
    private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        int j = polygon.Length - 1;
        
        for (int i = 0; i < polygon.Length; i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / 
                           (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
            j = i;
        }
        
        return inside;
    }
}
```

---

### **Option B: Import Sprite Assets**

**If you prefer pre-made sprites:**

**Sprite Requirements:**

| Shape | Size | Pivot | Format |
|-------|------|-------|--------|
| Square | 64×64 | Center (0.5, 0.5) | PNG, transparent background |
| Triangle | 64×64 | Center (0.5, 0.5) | PNG, transparent, equilateral |
| Hexagon | 64×64 | Center (0.5, 0.5) | PNG, transparent, flat-top |

**Unity Setup:**
1. Import sprites to `Assets/Viable/Core.Unity/Sprites/GridCells/`
2. Set **Texture Type**: Sprite (2D and UI)
3. Set **Pixels Per Unit**: 64
4. Set **Pivot**: Center
5. Set **Mesh Type**: Full Rect

---

## **?? Phase 2: Code Implementation**

### **Step 1: Update GridVisualizer to Store Topology**

```csharp
// In GridVisualizer.cs
using UnityEngine;
using Viable.Contracts;

public class GridVisualizer : MonoBehaviour
{
    [Header("Topology Configuration")]
    [SerializeField] private TopologyMode currentTopology = TopologyMode.RectGrid;
    
    [Header("Sprite References")]
    [SerializeField] private Sprite rectangularSprite;
    [SerializeField] private Sprite triangularSprite;
    [SerializeField] private Sprite hexagonalSprite;
    
    [Header("Cell Prefab")]
    [SerializeField] private GameObject cellPrefab; // Must have SpriteRenderer
    
    private SpriteRenderer[,] cellRenderers; // 2D array of cell renderers
    
    /// <summary>
    /// Initialize grid with specified topology.
    /// Called when simulation starts or topology changes.
    /// </summary>
    public void InitializeGrid(int width, int height, TopologyMode topology)
    {
        currentTopology = topology;
        
        // Clear existing grid if any
        ClearGrid();
        
        // Generate sprites if not assigned
        if (rectangularSprite == null)
            rectangularSprite = GridSpriteGenerator.GenerateSquareSprite();
        if (triangularSprite == null)
            triangularSprite = GridSpriteGenerator.GenerateTriangleSprite();
        if (hexagonalSprite == null)
            hexagonalSprite = GridSpriteGenerator.GenerateHexagonSprite();
        
        // Create cell grid
        cellRenderers = new SpriteRenderer[width, height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Instantiate cell
                GameObject cell = Instantiate(cellPrefab, transform);
                SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                
                // Set sprite based on topology
                renderer.sprite = GetSpriteForTopology(topology);
                
                // Position cell based on topology
                cell.transform.localPosition = GetCellPosition(x, y, topology);
                
                // Store reference
                cellRenderers[x, y] = renderer;
            }
        }
        
        // Adjust camera to fit grid
        AdjustCameraForTopology(width, height, topology);
    }
    
    /// <summary>
    /// Get appropriate sprite for current topology.
    /// </summary>
    private Sprite GetSpriteForTopology(TopologyMode topology)
    {
        switch (topology)
        {
            case TopologyMode.RectGrid:
                return rectangularSprite;
            case TopologyMode.TriGrid:
                return triangularSprite;
            case TopologyMode.HexGrid:
                return hexagonalSprite;
            default:
                Debug.LogWarning($"[GridVisualizer] Unknown topology: {topology}, defaulting to Rectangular");
                return rectangularSprite;
        }
    }
    
    /// <summary>
    /// Calculate cell position based on topology.
    /// Different tessellations have different spacing patterns.
    /// </summary>
    private Vector3 GetCellPosition(int x, int y, TopologyMode topology)
    {
        switch (topology)
        {
            case TopologyMode.RectGrid:
                return GetRectangularPosition(x, y);
            
            case TopologyMode.TriGrid:
                return GetTriangularPosition(x, y);
            
            case TopologyMode.HexGrid:
                return GetHexagonalPosition(x, y);
            
            default:
                return GetRectangularPosition(x, y);
        }
    }
    
    /// <summary>
    /// Rectangular grid: Simple 1:1 spacing.
    /// </summary>
    private Vector3 GetRectangularPosition(int x, int y)
    {
        float cellSize = 1.0f; // Adjust as needed
        return new Vector3(x * cellSize, y * cellSize, 0);
    }
    
    /// <summary>
    /// Triangular grid: Offset every other row.
    /// Uses "flat-side-down" orientation with alternating offset.
    /// </summary>
    private Vector3 GetTriangularPosition(int x, int y)
    {
        float cellWidth = 1.0f;
        float cellHeight = 0.866f; // sqrt(3)/2 for equilateral triangles
        
        // Offset every other row
        float xOffset = (y % 2 == 0) ? 0 : cellWidth * 0.5f;
        
        return new Vector3(
            x * cellWidth + xOffset,
            y * cellHeight,
            0
        );
    }
    
    /// <summary>
    /// Hexagonal grid: Flat-top orientation with offset rows.
    /// Uses axial coordinate system.
    /// </summary>
    private Vector3 GetHexagonalPosition(int x, int y)
    {
        // Flat-top hexagon dimensions
        float hexWidth = 1.0f;
        float hexHeight = 0.866f; // sqrt(3)/2
        
        // Offset every other row
        float xOffset = (y % 2 == 0) ? 0 : hexWidth * 0.5f;
        
        return new Vector3(
            x * hexWidth * 0.75f + xOffset, // 0.75 = 3/4 for flat-top hex spacing
            y * hexHeight,
            0
        );
    }
    
    /// <summary>
    /// Adjust camera to fit entire grid based on topology.
    /// </summary>
    private void AdjustCameraForTopology(int width, int height, TopologyMode topology)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        // Calculate grid bounds
        Vector3 minPos = GetCellPosition(0, 0, topology);
        Vector3 maxPos = GetCellPosition(width - 1, height - 1, topology);
        
        Vector3 center = (minPos + maxPos) / 2f;
        float gridWidth = Mathf.Abs(maxPos.x - minPos.x) + 1;
        float gridHeight = Mathf.Abs(maxPos.y - minPos.y) + 1;
        
        // Position camera
        mainCamera.transform.position = new Vector3(center.x, center.y, -10);
        
        // Adjust orthographic size to fit grid
        float aspectRatio = mainCamera.aspect;
        float requiredHeight = gridHeight / 2f;
        float requiredWidth = gridWidth / (2f * aspectRatio);
        
        mainCamera.orthographicSize = Mathf.Max(requiredHeight, requiredWidth) * 1.1f; // 10% padding
    }
    
    /// <summary>
    /// Update cell color (called every frame for active cells).
    /// </summary>
    public void UpdateCellColor(int x, int y, Color color)
    {
        if (cellRenderers != null && 
            x >= 0 && x < cellRenderers.GetLength(0) &&
            y >= 0 && y < cellRenderers.GetLength(1))
        {
            cellRenderers[x, y].color = color;
        }
    }
    
    /// <summary>
    /// Clear all cells (when restarting simulation).
    /// </summary>
    private void ClearGrid()
    {
        if (cellRenderers == null) return;
        
        foreach (var renderer in cellRenderers)
        {
            if (renderer != null)
                Destroy(renderer.gameObject);
        }
        
        cellRenderers = null;
    }
}
```

---

### **Step 2: Integrate with SimulationController**

```csharp
// In SimulationController.cs

using Viable.Contracts;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    [SerializeField] private GridVisualizer gridVisualizer;
    
    private TopologyMode currentTopology = TopologyMode.RectGrid;
    
    /// <summary>
    /// Initialize simulation with specified topology.
    /// Called when loading preset or changing topology.
    /// </summary>
    public void InitializeSimulation(ScenarioDefinition scenario)
    {
        // ... existing initialization code ...
        
        // Extract topology from scenario
        currentTopology = scenario.EngineConfig.TopologyMode;
        
        // Initialize grid visualization
        gridVisualizer.InitializeGrid(
            scenario.Width,
            scenario.Height,
            currentTopology
        );
        
        Debug.Log($"[SimulationController] Initialized with topology: {currentTopology}");
        
        // ... rest of initialization ...
    }
    
    /// <summary>
    /// Update grid visualization each tick.
    /// </summary>
    private void UpdateGridVisualization()
    {
        // Get current simulation state
        var state = GetCurrentState(); // Your existing method
        
        for (int y = 0; y < state.Height; y++)
        {
            for (int x = 0; x < state.Width; x++)
            {
                // Get cell state
                var cell = state.GetCell(x, y);
                
                // Calculate color based on cell state
                Color cellColor = CalculateCellColor(cell);
                
                // Update visualization
                gridVisualizer.UpdateCellColor(x, y, cellColor);
            }
        }
    }
    
    /// <summary>
    /// Calculate cell color based on viability, resource, etc.
    /// </summary>
    private Color CalculateCellColor(CellState cell)
    {
        if (cell.IsSink)
            return Color.magenta; // Sink cells
        
        if (cell.IsViable)
        {
            // Gradient from yellow (low resource) to green (high resource)
            float resourceRatio = cell.Resource / maxResource;
            return Color.Lerp(Color.yellow, Color.green, resourceRatio);
        }
        
        return new Color(0.1f, 0.1f, 0.1f, 1f); // Dark gray for inactive
    }
}
```

---

### **Step 3: Connect to Topology Dropdown**

```csharp
// In Setup Panel or MechanismController

private void OnTopologyChanged(int index)
{
    TopologyMode newTopology = (TopologyMode)index;
    workingConfig.TopologyMode = newTopology;
    
    Debug.Log($"[MechanismController] Topology changed to: {newTopology}");
    
    // Show message: "Apply & Restart to see changes"
    // Or auto-restart if desired
}
```

---

## **?? Phase 3: Visual Verification**

### **Test 1: Rectangular (Baseline)**

**Expected Visual:**
```
? ? ? ? ?
? ? ? ? ?
? ? ? ? ?   ? Regular square grid, aligned
? ? ? ? ?      No gaps, perfect rows
? ? ? ? ?
```

**Checklist:**
- [ ] Cells are square sprites
- [ ] Perfect alignment (no gaps or overlaps)
- [ ] Grid fills screen appropriately
- [ ] Camera centered on grid

---

### **Test 2: Triangular**

**Expected Visual:**
```
  ? ? ? ? ?
 ? ? ? ? ? ?   ? Alternating row offset
  ? ? ? ? ?      Triangles pointing up
 ? ? ? ? ? ?      Creates hexagonal outline
  ? ? ? ? ?
```

**Checklist:**
- [ ] Cells are triangle sprites
- [ ] Every other row offset by half-width
- [ ] No gaps between triangles
- [ ] Expansion shows hexagonal pattern

---

### **Test 3: Hexagonal**

**Expected Visual:**
```
  ? ? ? ?
 ? ? ? ? ?   ? Offset rows (flat-top hexagons)
  ? ? ? ?      Tightly packed
 ? ? ? ? ?      No gaps
  ? ? ? ?
```

**Checklist:**
- [ ] Cells are hexagon sprites
- [ ] Flat-top orientation (not pointy-top)
- [ ] Every other row offset
- [ ] No gaps between hexagons
- [ ] Expansion shows circular pattern

---

## **?? Common Issues & Fixes**

### **Issue 1: Sprites Not Showing**

**Symptoms:**
- Empty grid or invisible cells

**Fix:**
```csharp
// Check sprite generation
Debug.Log($"Rectangular sprite: {rectangularSprite != null}");
Debug.Log($"Triangular sprite: {triangularSprite != null}");
Debug.Log($"Hexagonal sprite: {hexagonalSprite != null}");

// Check SpriteRenderer component
if (cellPrefab.GetComponent<SpriteRenderer>() == null)
    Debug.LogError("Cell prefab missing SpriteRenderer!");
```

---

### **Issue 2: Gaps Between Cells**

**Symptoms:**
- Visible spaces between triangles or hexagons

**Fix:**
- Adjust `cellWidth` / `cellHeight` in position calculations
- For triangles: Try `cellHeight = 0.87f` instead of `0.866f`
- For hexagons: Try `hexWidth * 0.76f` instead of `0.75f`
- Add slight overlap: `cellWidth * 1.01f`

---

### **Issue 3: Wrong Alignment**

**Symptoms:**
- Cells not forming proper tessellation
- Overlapping cells

**Fix:**
```csharp
// Debug cell positions
Debug.Log($"Cell ({x},{y}) at position: {GetCellPosition(x, y, topology)}");

// Visualize cell positions in Scene view
private void OnDrawGizmos()
{
    if (cellRenderers == null) return;
    
    Gizmos.color = Color.red;
    for (int y = 0; y < cellRenderers.GetLength(1); y++)
    {
        for (int x = 0; x < cellRenderers.GetLength(0); x++)
        {
            if (cellRenderers[x, y] != null)
                Gizmos.DrawWireSphere(cellRenderers[x, y].transform.position, 0.1f);
        }
    }
}
```

---

## **?? Visual Comparison (In Unity)**

### **Side-by-Side Test:**

**Procedure:**
1. Run simulation with Rectangular ? Screenshot
2. Restart ? Switch to Triangular ? Screenshot
3. Restart ? Switch to Hexagonal ? Screenshot
4. Compare screenshots side-by-side

**Expected Differences:**

| Topology | Visual Shape | Gaps | Alignment |
|----------|--------------|------|-----------|
| Rectangular | Perfect squares | None | Grid-aligned |
| Triangular | Equilateral ? | None | Offset rows |
| Hexagonal | Flat ? | None | Offset rows |

---

## **?? Optional: Enhanced Visuals**

### **Cell Border Outlines**

```csharp
// Add subtle borders to distinguish cells
private void AddCellBorder(SpriteRenderer renderer)
{
    // Create outline material
    Material outlineMat = new Material(Shader.Find("Sprites/Outline"));
    outlineMat.SetFloat("_OutlineWidth", 0.05f);
    outlineMat.SetColor("_OutlineColor", Color.black);
    
    renderer.material = outlineMat;
}
```

---

### **Smooth Color Transitions**

```csharp
// Lerp cell colors over time for smoother visuals
private IEnumerator TransitionCellColor(SpriteRenderer renderer, Color targetColor, float duration)
{
    Color startColor = renderer.color;
    float elapsed = 0f;
    
    while (elapsed < duration)
    {
        renderer.color = Color.Lerp(startColor, targetColor, elapsed / duration);
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    renderer.color = targetColor;
}
```

---

## **?? Implementation Checklist Summary**

### **Code Files to Modify:**
- [ ] Create `GridSpriteGenerator.cs` (procedural sprite generation)
- [ ] Update `GridVisualizer.cs` (topology-aware positioning)
- [ ] Update `SimulationController.cs` (pass topology to visualizer)
- [ ] Update mechanism dropdown handler (topology change callback)

### **Unity Editor Setup:**
- [ ] Assign Cell Prefab to GridVisualizer (with SpriteRenderer)
- [ ] (Optional) Import sprite assets if not using procedural generation
- [ ] Test all 3 topologies in Play mode
- [ ] Verify camera auto-adjusts for each topology

### **Verification:**
- [ ] Rectangular: Square grid, no gaps
- [ ] Triangular: Offset triangles, hexagonal pattern
- [ ] Hexagonal: Flat-top hexagons, circular expansion
- [ ] All 3 visually distinct

---

## **?? Implementation Order**

**Week 1: Sprites & Basic Rendering**
1. Create `GridSpriteGenerator.cs`
2. Test sprite generation in isolation
3. Update `GridVisualizer` to use sprites

**Week 2: Positioning & Camera**
4. Implement `GetTriangularPosition()`
5. Implement `GetHexagonalPosition()`
6. Test camera adjustment

**Week 3: Integration & Testing**
7. Connect to `SimulationController`
8. Test topology switching
9. Visual verification (all 3 tessellations)

---

**This guide provides everything needed to visually represent all 3 tessellations in Unity!** ??

Let me know when you're ready to implement and I can provide more detailed step-by-step instructions for any specific part!
