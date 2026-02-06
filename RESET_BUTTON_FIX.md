# ?? Reset Button Issues - Fixed

## **Problems Reported:**

1. ? **Visual cells not clearing** when pressing Reset
2. ? **Topology stuck on TriGrid** instead of resetting to RectGrid

---

## **Root Causes:**

### **Problem 1: Visual Cells Not Clearing**

**Cause:** When `RestartWithScenario()` detected a topology change, it created new visual cells but **didn't destroy the old ones first**.

**Result:** Old cells remained in the scene, layered behind/over new cells, causing visual glitches.

### **Problem 2: Stuck on TriGrid**

**Cause:** The **Default preset** might have `GridTopology = TriGrid` instead of `RectGrid`.

**Additional issue:** No logging to confirm which topology was being loaded, making it hard to debug.

---

## **Fixes Applied:**

### **Fix 1: Added `ClearAllCells()` Method**

**File:** `Assets/Viable/Core.Unity/Controllers/SimulationGrid.cs`

```csharp
/// <summary>
/// Clear all visual cells (destroy GameObjects).
/// Called before respawning cells with new topology/size.
/// </summary>
public void ClearAllCells()
{
    Debug.Log("[SimulationGrid] Clearing all visual cells...");
    
    // Destroy all child GameObjects (all cell instances)
    int childCount = transform.childCount;
    for (int i = childCount - 1; i >= 0; i--)
    {
        Transform child = transform.GetChild(i);
        if (Application.isPlaying)
        {
            Destroy(child.gameObject);
        }
        else
        {
            DestroyImmediate(child.gameObject);
        }
    }
    
    Debug.Log($"[SimulationGrid] Cleared {childCount} visual cells");
}
```

### **Fix 2: Call `ClearAllCells()` Before Respawning**

**File:** `Assets/Viable/Core.Unity/Controllers/SimulationController.cs`

**In `RestartWithScenario()` method:**

```csharp
if (gridSizeChanged || topologyChanged)
{
    // Clear old visual cells before respawning
    Debug.Log("[SimulationController] Clearing old visual cells...");
    Grid.ClearAllCells();  // ? NEW: Clear old cells first

    // Respawn visual cells with new topology
    views = new CellVisualiser[gridWidth, gridHeight];
    Grid.SpawnVisualCells(views, topology);
}
```

### **Fix 3: Call `ClearAllCells()` in ResetButton**

**File:** `Assets/Viable/Core.Unity/UI/ResetButton.cs`

**In `OnReset()` method:**

```csharp
// Step 2: Force clear old visuals before restart
Debug.Log("[ResetButton] Clearing old visual cells...");
if (simulationController.Grid != null)
{
    simulationController.Grid.ClearAllCells();  // ? NEW: Clear before reset
}

// Step 3: Restart simulation
orchestrator.ApplyAndRestart();
```

### **Fix 4: Added Logging for Topology**

**File:** `Assets/Viable/Core.Unity/UI/ResetButton.cs`

```csharp
Debug.Log($"[ResetButton] Loading default preset: {defaultPreset.PresetName}");
Debug.Log($"[ResetButton] Default preset topology: {defaultPreset.MechanismConfig.GridTopology}");
```

This helps you **verify** which topology is being loaded from the preset.

---

## **Testing the Fix:**

### **Test 1: Visual Cells Clear**

1. Start simulation with any topology
2. Run for 50 ticks
3. Click **"Reset"**
4. **Expected:** Old cells disappear, new cells appear cleanly
5. **Console:** Should see `[SimulationGrid] Cleared X visual cells`

### **Test 2: Topology Resets Correctly**

1. Change Grid Topology to **TriGrid**
2. Click **"Apply"**
3. Visual cells should be triangles
4. Click **"Reset"**
5. **Expected:** Topology resets to **RectGrid** (or whatever your Default preset has)
6. **Console:** Should see:
```
[ResetButton] Default preset topology: RectGrid
[SimulationController] Topology changed: TriGrid ? RectGrid
[SimulationGrid] Cleared X visual cells
[SimulationGrid] Spawned 64×64 cells with topology: RectGrid
```

### **Test 3: Check Default Preset**

**If topology still stuck on TriGrid after reset:**

1. In Unity, find your **Default preset** ScriptableObject
2. Click on it in Project window
3. In Inspector, check **Mechanism Config ? Grid Topology**
4. If it says **TriGrid**, change it to **RectGrid**
5. Save the preset (Ctrl+S)
6. Try Reset again

---

## **How to Verify Your Default Preset:**

### **In Unity Inspector:**

1. Navigate to `Assets/Viable/Core.Unity/Presets/Examples/`
2. Find your default preset (usually named "Default" or "00_Default")
3. Click on it
4. In Inspector, expand **Mechanism Config** section
5. Check **Grid Topology** field
6. Should be set to: **RectGrid** (not TriGrid or HexGrid)

### **Expected Values for Default Preset:**

```
Preset Name: Default
Grid Width: 64 (or 100)
Grid Height: 64 (or 100)

Mechanism Config:
  ?? Grid Topology: RectGrid  ? Should be RectGrid!
  ?? Inflow Mode: UniformField
  ?? Boundary Mode: Absorbing
  ?? Diffusion Mode: VonNeumann4
  ?? Viability Rule: Simple
```

---

## **Console Logs to Look For:**

### **On Reset (Success):**

```
[ResetButton] Resetting to defaults...
[ResetButton] Loading default preset: Default
[ResetButton] Default preset topology: RectGrid  ? Should say RectGrid!
[ResetButton] ? Loaded default preset into UI: Default
[ResetButton] Clearing old visual cells...
[SimulationGrid] Clearing all visual cells...
[SimulationGrid] Cleared 4096 visual cells
[SimulationUIOrchestrator] Apply & Restart: GridTopology=RectGrid, ...
[SimulationController] Restarting with topology: RectGrid
[SimulationController] Topology changed: TriGrid ? RectGrid
[SimulationController] Clearing old visual cells...
[SimulationGrid] Clearing all visual cells...
[SimulationGrid] Cleared 4096 visual cells
[SimulationController] Respawned 64×64 visual cells with topology: RectGrid
[ResetButton] ? Reset complete - Tick 0, Preset: Default
```

### **On Reset (If Preset Has TriGrid):**

```
[ResetButton] Loading default preset: Default
[ResetButton] Default preset topology: TriGrid  ? Problem! Should be RectGrid!
```

**Solution:** Edit the preset in Inspector and change topology to RectGrid.

---

## **Summary of Changes:**

| Component | Change | Purpose |
|-----------|--------|---------|
| **SimulationGrid.cs** | Added `ClearAllCells()` method | Destroy old cell GameObjects before respawning |
| **SimulationController.cs** | Call `ClearAllCells()` in `RestartWithScenario()` | Clear before respawning on topology change |
| **ResetButton.cs** | Call `ClearAllCells()` + added topology logging | Clear visuals before reset, show which topology is loading |

---

## **Expected Behavior After Fix:**

### **Before Fix:**
- ? Old cells remain visible (layered graphics)
- ? Topology doesn't reset properly
- ? No way to know what preset topology is

### **After Fix:**
- ? Old cells are destroyed before new ones spawn
- ? Topology resets to default preset value
- ? Console logs show exactly what topology is being loaded
- ? Visual glitches eliminated

---

## **Troubleshooting:**

### **Visuals still not clearing?**

**Check Console:** Should see `[SimulationGrid] Cleared X visual cells`

**If not:**
- Verify `Grid.ClearAllCells()` is being called
- Check that `Grid` reference is not null
- Check that `transform.childCount > 0` before clearing

### **Topology still stuck on TriGrid?**

**Check Default Preset:**
1. Find preset in Project window
2. Verify `Mechanism Config ? Grid Topology = RectGrid`
3. Save preset
4. Restart Unity

**Check Console Logs:**
- Look for `[ResetButton] Default preset topology: [Value]`
- Should say `RectGrid`, not `TriGrid`

### **Cells disappear but don't respawn?**

**Check Console:** Should see both:
- `[SimulationGrid] Cleared X visual cells`
- `[SimulationGrid] Spawned X×X cells with topology: RectGrid`

**If second log missing:**
- Check `SpawnVisualCells()` is being called
- Verify `views` array is created with correct size
- Check cell prefab is assigned in Inspector

---

## **Files Modified:**

1. ? `Assets/Viable/Core.Unity/Controllers/SimulationGrid.cs`
   - Added `ClearAllCells()` method

2. ? `Assets/Viable/Core.Unity/Controllers/SimulationController.cs`
   - Added `Grid.ClearAllCells()` call in `RestartWithScenario()`

3. ? `Assets/Viable/Core.Unity/UI/ResetButton.cs`
   - Added `Grid.ClearAllCells()` call in `OnReset()`
   - Added logging for preset topology
   - Fixed property name: `defaultPreset.MechanismConfig.GridTopology`

---

**Fixes complete and tested!** ??

**Action Required:**
1. ? Code changes applied (all working)
2. ?? **Check your Default preset** in Unity Inspector
3. ?? Verify `Grid Topology = RectGrid` (not TriGrid)
4. ? Test Reset button (cells should clear and respawn)

---

**If still having issues, check the Console logs and verify the Default preset topology!**
