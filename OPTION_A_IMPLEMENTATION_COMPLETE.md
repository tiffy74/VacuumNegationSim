# ? Option A Implementation Complete - Testing Guide

## **What We Just Did:**

### **Files Created:**
1. ? `Assets/Viable/Core.Unity/Rendering/GridSpriteGenerator.cs`
   - Procedurally generates Square, Triangle, and Hexagon sprites
   - No external image files needed
   - Perfect geometric shapes

### **Files Modified:**
2. ? `Assets/Viable/Core.Unity/Controllers/SimulationGrid.cs`
   - Added sprite caching (generated once, reused)
   - Added topology-aware sprite selection
   - Added topology-specific cell positioning (offset rows for Tri/Hex)
   - Added `GetCellPosition()` methods for all 3 tessellations

3. ? `Assets/Viable/Core.Unity/Controllers/SimulationController.cs`
   - Extracts topology from `ScenarioDefinition.EngineConfig.TopologyMode`
   - Passes topology to `Grid.SpawnVisualCells()`
   - Updated both `InitializeSimulation()` and `RestartWithScenario()`

---

## **?? Testing Steps**

### **Step 1: Verify Code Compiles**

1. **Open Unity Editor**
2. **Wait for compilation** (should complete without errors)
3. **Check Console for errors:**
   - ? No errors = Good!
   - ? Errors = Report them and we'll fix

**Expected Console Output:**
```
[SimulationGrid] Spawned 64×64 cells with topology: RectGrid
[SimulationController] Using topology from preset: RectGrid
```

---

### **Step 2: Test Rectangular (Baseline)**

**Purpose:** Verify current behavior still works (no regression).

**Steps:**
1. Press **Play** in Unity
2. Select any preset (e.g., "Default")
3. Click **"Apply & Restart"**
4. Click **"Play"** to run simulation
5. Let run to ~100 ticks

**Expected Result:**
- ? Grid shows **square cells** (current behavior)
- ? Expansion pattern: Square/diamond shape
- ? No visual glitches
- ? Console: `Using topology from preset: RectGrid`

**Screenshot:** `Rectangular_Test.png` (for reference)

---

### **Step 3: Test Triangular (NEW)**

**Purpose:** Verify triangular sprites and offset positioning work.

**Prerequisites:**
- Need to create a preset with `TopologyMode = TriGrid`
- OR manually set in code (temporary test)

**Option A: Create Test Preset**
1. Duplicate "Default" preset
2. Rename to "TestTriangular"
3. In Inspector, find `EngineConfig` ? `TopologyMode`
4. Change to `TriGrid` (Triangular)
5. Save preset

**Option B: Temporary Code Test**
```csharp
// In SimulationController.InitializeSimulation()
// TEMPORARY: Force triangular for testing
topology = TopologyMode.TriGrid;
Debug.Log("[TEST] Forcing Triangular topology for testing");
```

**Steps:**
1. Load "TestTriangular" preset (or use temp code)
2. Click **"Apply & Restart"**
3. **Look at the grid carefully:**
   - Are cells **triangle-shaped**? (not squares)
   - Are rows **offset** (every other row shifted)?
   - Are there **gaps** between triangles?

**Expected Result:**
- ? Cells are **triangles** (not squares!)
- ? Rows are **offset** (alternating)
- ? **No gaps** between triangles
- ? Console: `Using topology: TriGrid`

**If Issues:**
- ? Still showing squares ? Sprite not switching
- ? Gaps between cells ? Adjust spacing constants
- ? Wrong alignment ? Debug cell positions

**Screenshot:** `Triangular_Test.png`

---

### **Step 4: Test Hexagonal (NEW)**

**Purpose:** Verify hexagonal sprites and isotropic expansion.

**Option A: Create Test Preset**
1. Duplicate "Default" preset
2. Rename to "TestHexagonal"
3. Set `TopologyMode` ? `HexGrid`
4. Save preset

**Option B: Temporary Code Test**
```csharp
// TEMPORARY: Force hexagonal for testing
topology = TopologyMode.HexGrid;
Debug.Log("[TEST] Forcing Hexagonal topology for testing");
```

**Steps:**
1. Load "TestHexagonal" preset (or use temp code)
2. Click **"Apply & Restart"**
3. **Look at the grid:**
   - Are cells **hexagon-shaped**?
   - Are hexagons **flat-top** orientation (not pointy-top)?
   - Are rows **offset**?

**Expected Result:**
- ? Cells are **hexagons**
- ? **Flat-top** orientation (flat side on top/bottom)
- ? Rows **offset** (every other row shifted)
- ? **No gaps** between hexagons
- ? Console: `Using topology: HexGrid`

**Screenshot:** `Hexagonal_Test.png`

---

### **Step 5: Visual Comparison**

**Load all 3 screenshots side-by-side:**

| Rectangular | Triangular | Hexagonal |
|-------------|------------|-----------|
| ? ? ? ? ?   | ? ? ? ? ?  | ? ? ? ? ? |
| ? ? ? ? ?   | ? ? ? ? ?  | ? ? ? ? ? |
| ? ? ? ? ?   | ? ? ? ? ?  | ? ? ? ? ? |

**Checklist:**
- [ ] All 3 look **DIFFERENT** (not all squares)
- [ ] Triangular rows are **offset**
- [ ] Hexagonal rows are **offset**
- [ ] No **gaps** or **overlaps** in any topology
- [ ] Cell **colors still work** (viability gradient)

---

## **?? Common Issues & Fixes**

### **Issue 1: All Cells Still Show Squares**

**Symptom:** Changed topology, but cells still look like squares.

**Diagnosis:**
- Sprite not switching correctly
- `GetSpriteForTopology()` not working

**Debug:**
```csharp
// In SimulationGrid.SpawnVisualCells()
Debug.Log($"[TEST] Setting sprite for topology {topology}: {GetSpriteForTopology(topology).name}");
Debug.Log($"[TEST] Rectangular sprite: {rectangularSprite != null}");
Debug.Log($"[TEST] Triangular sprite: {triangularSprite != null}");
Debug.Log($"[TEST] Hexagonal sprite: {hexagonalSprite != null}");
```

**Fix:**
- Check Console logs - are sprites being generated?
- Check if `renderer.sprite = ...` line is executing
- Verify `SpriteRenderer` component exists on prefab

---

### **Issue 2: Sprites Not Showing (Invisible Grid)**

**Symptom:** Grid is empty or transparent.

**Diagnosis:**
- Sprite generation failed
- Texture not applied

**Debug:**
```csharp
// In GridSpriteGenerator.GenerateSquareSprite()
Debug.Log($"[GridSpriteGenerator] Generated square sprite: {texture.width}x{texture.height}");
```

**Fix:**
- Check if sprites are actually being created
- Verify `texture.Apply()` is called
- Check if sprites have valid textures

---

### **Issue 3: Gaps Between Cells**

**Symptom:** Visible spaces between triangles or hexagons.

**Diagnosis:**
- Cell spacing too large
- Offset calculations incorrect

**Fix:**
```csharp
// In SimulationGrid.GetTriangularPosition()
// Try adjusting cellHeight:
float cellHeight = CellSize * 0.87f; // Was 0.866f

// In SimulationGrid.GetHexagonalPosition()
// Try adjusting spacing:
x * hexWidth * 0.76f + xOffset // Was 0.75f
```

**Adjust until no gaps visible.**

---

### **Issue 4: Cells Overlapping**

**Symptom:** Cells are stacked on top of each other.

**Diagnosis:**
- Cell size too large
- Spacing too small

**Fix:**
```csharp
// In SimulationGrid, try smaller CellSize:
public float CellSize = 0.008f; // Was 0.01f
```

---

### **Issue 5: Wrong Alignment (Cells in Wrong Positions)**

**Symptom:** Cells not forming proper tessellation pattern.

**Diagnosis:**
- Offset calculations wrong
- Row indexing issue

**Debug:**
```csharp
// In SimulationGrid.SpawnVisualCells()
if (x < 5 && y < 5)
{
    Debug.Log($"[TEST] Cell ({x},{y}) at position: {GetCellPosition(x, y, topology)}");
}
```

**Fix:**
- Check offset logic: `(y % 2 == 0) ? 0 : offset`
- Verify even/odd row offsets are correct
- Test with small grid (8×8) first

---

## **? Success Criteria**

### **Rectangular (Baseline):**
- [ ] Cells are **square**
- [ ] Grid is **aligned** (no offsets)
- [ ] **No gaps** between cells
- [ ] Expansion pattern: **Square/diamond**
- [ ] Console: `topology: RectGrid`

### **Triangular (NEW):**
- [ ] Cells are **triangles** (equilateral)
- [ ] Rows are **offset** (every other row)
- [ ] Triangles **tile perfectly** (no gaps)
- [ ] Expansion pattern: **Hexagonal outline**
- [ ] Console: `topology: TriGrid`

### **Hexagonal (NEW):**
- [ ] Cells are **hexagons**
- [ ] **Flat-top** orientation
- [ ] Rows are **offset**
- [ ] Hexagons **tile perfectly**
- [ ] Expansion pattern: **Circular** (isotropic)
- [ ] Console: `topology: HexGrid`

---

## **?? Performance Check**

**Expected Performance:**
- **FPS:** Should be same as before (no performance loss)
- **Startup time:** +0.1 seconds (sprite generation)
- **Memory:** +~50 KB (3 sprites cached)

**If Performance Issues:**
- Sprite generation only happens once at startup
- Sprites are reused for all 4096 cells
- No runtime overhead (same as before)

---

## **?? Next Steps After Testing**

### **If All Tests Pass:**
1. ? Document baseline metrics for each topology
2. ? Create presets for Triangular and Hexagonal
3. ? Test expansion patterns (do they look different?)
4. ? Test neighbor connectivity (does logic match visuals?)
5. ? Move to camera auto-framing (optional enhancement)

### **If Tests Fail:**
1. ? Document exact failure (screenshot + console logs)
2. ? Report issue with details:
   - What topology?
   - What visual issue?
   - Console logs?
3. ? I'll provide fix instructions
4. ? Re-test after fix

---

## **?? Quick Test Script**

**For rapid testing, add this to SimulationController:**

```csharp
[ContextMenu("Test Rectangular")]
public void TestRectangular()
{
    Debug.Log("[TEST] Testing Rectangular topology");
    TopologyMode testTopology = TopologyMode.RectGrid;
    views = new CellVisualiser[Grid.Width, Grid.Height];
    Grid.SpawnVisualCells(views, testTopology);
}

[ContextMenu("Test Triangular")]
public void TestTriangular()
{
    Debug.Log("[TEST] Testing Triangular topology");
    TopologyMode testTopology = TopologyMode.TriGrid;
    views = new CellVisualiser[Grid.Width, Grid.Height];
    Grid.SpawnVisualCells(views, testTopology);
}

[ContextMenu("Test Hexagonal")]
public void TestHexagonal()
{
    Debug.Log("[TEST] Testing Hexagonal topology");
    TopologyMode testTopology = TopologyMode.HexGrid;
    views = new CellVisualiser[Grid.Width, Grid.Height];
    Grid.SpawnVisualCells(views, testTopology);
}
```

**Usage:**
1. Right-click `SimulationController` component in Inspector
2. Select "Test Rectangular" / "Test Triangular" / "Test Hexagonal"
3. Grid will respawn with selected topology
4. **Fast iteration for debugging!**

---

## **?? Summary**

**What You Need to Do:**

1. ? **Open Unity** ? Wait for compilation
2. ? **Test Rectangular** ? Verify no regression
3. ? **Test Triangular** ? Create test preset or use temp code
4. ? **Test Hexagonal** ? Create test preset or use temp code
5. ? **Take Screenshots** ? Compare all 3 side-by-side
6. ? **Report Results** ? Tell me what you see!

**Total Time Estimate:** 15-30 minutes

**Risk Level:** ?? **LOW**
- No simulation logic changes
- Pure visual changes
- Easy to revert if issues

---

**Ready to test! Let me know what happens when you run Unity!** ??
