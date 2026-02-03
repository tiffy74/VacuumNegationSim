# CRITICAL FIX - UI to Simulation Bridge

## ?? **PROBLEM STATEMENT**

**The UI doesn't control the simulation!**

### Current (Broken) Flow:
```
User changes dropdown
    ?
WorkingScenarioConfig updated ?
    ?
??? NOTHING ???
    ?
SimulationController runs with old preset values ?
    ?
Simulation looks the same every time ?
```

### What Should Happen:
```
User changes dropdown
    ?
WorkingScenarioConfig updated
    ?
Config converted to ScenarioPreset
    ?
SimulationController reloads with new config
    ?
Simulation reflects UI changes ?
```

---

## ?? **ROOT CAUSES**

### **Issue 1: No Bridge Between UI and Simulation**

**File:** `SimulationController.cs`
- Loads from `ScenarioPreset` (ScriptableObject)
- Ignores `WorkingScenarioConfig` completely

**File:** `MechanismsSection.cs`, `CoreParametersSection.cs`, etc.
- Edit `WorkingScenarioConfig`
- Never tell SimulationController

**Result:** UI changes are lost!

---

### **Issue 2: No "Apply Changes" Button**

Users can change dropdowns, but there's no way to say:
> "Apply these changes and restart the simulation"

**Missing:** A button that converts `WorkingScenarioConfig` ? triggers simulation restart

---

### **Issue 3: Sink Control Not Exposed**

**Sink formation controlled by:**
```csharp
// In SimulationConfiguration:
float SinkFormationThreshold = 0.5f;  // When does a cell become a sink?
float SinkDrainFraction = 0f;         // How much does sink drain?
float SinkRecoilFraction = 0f;        // How much bounces back?
```

**But these are NOT in UI!**

**Missing:**
- Dropdown/slider for sink formation threshold
- Controls for number of sinks
- Controls for sink behavior

---

## ? **SOLUTION OVERVIEW**

### **Phase 1: Create UI ? Simulation Bridge** (HIGH PRIORITY)

1. Add "Apply & Restart" button in TopBar/RightDock
2. Create `WorkingScenarioConfig` ? `ScenarioPreset` converter
3. Wire button to trigger `SimulationController.LoadPreset()`

### **Phase 2: Add Sink Controls** (YOUR MAIN REQUEST)

1. Add sink parameters to `WorkingScenarioConfig`
2. Create "Sink Details Section" in RightDock
3. Expose:
   - Sink Formation Threshold
   - Number of Initial Sinks (NEW!)
   - Sink Drain/Recoil settings

### **Phase 3: Add Core Parameters Section** (COMPLETE CONTROL)

1. Expose all simulation parameters in UI
2. Sliders for:
   - Decay rate
   - Resource replenishment
   - Expansion chance
   - Complexity dynamics
   - etc.

---

## ?? **IMPLEMENTATION PLAN**

### **Step 1: Add "Apply Changes" Button**

**Where:** TopBarUI (already exists as `applyRestartButton`)

**Current Code:**
```csharp
// Line 200 in TopBarUI.cs
private void OnApplyAndRestart()
{
    Debug.Log("[TopBarUI] Apply & Restart");
    // TODO: Convert workingConfig ? ScenarioDefinition/RunRequest
    // simulationController.ResetAndRun(...)
}
```

**Fix:** Implement the TODO!

---

### **Step 2: Create WorkingConfig ? Preset Converter**

**New File:** `Assets/Viable/Core.Unity/Configuration/WorkingConfigToPresetAdapter.cs`

**Purpose:** Convert UI's `WorkingScenarioConfig` to `ScenarioPreset` that SimulationController can load

**Code:**
```csharp
public static class WorkingConfigToPresetAdapter
{
    public static ScenarioPreset ToPreset(WorkingScenarioConfig working)
    {
        var preset = ScriptableObject.CreateInstance<ScenarioPreset>();
        
        // Copy all values from working config to preset
        preset.GridWidth = working.GridWidth;
        preset.GridHeight = working.GridHeight;
        preset.Seed = working.Seed;
        
        // Map enums
        preset.TopologyMode = working.Topology;
        preset.BoundaryMode = working.Boundary;
        // ... etc
        
        return preset;
    }
}
```

---

### **Step 3: Wire "Apply" Button**

**File:** `TopBarUI.cs`

**Replace TODO with:**
```csharp
private void OnApplyAndRestart()
{
    if (workingConfig == null)
    {
        Debug.LogError("[TopBarUI] No working config!");
        return;
    }

    // Convert working config to preset
    var preset = WorkingConfigToPresetAdapter.ToPreset(workingConfig);

    // Tell simulation controller to load it
    simulationController?.LoadPreset(preset);

    Debug.Log("[TopBarUI] Applied config and restarted simulation");
}
```

---

### **Step 4: Add Sink Parameters to WorkingScenarioConfig**

**File:** `Assets/Viable/Core.Unity/Configuration/WorkingScenarioConfig.cs`

**Add these fields:**
```csharp
// Sink Control
public float SinkFormationThreshold { get; set; } = 0.5f;
public int InitialSinkCount { get; set; } = 0; // NEW!
public List<Vector2Int> InitialSinkPositions { get; set; } = new List<Vector2Int>(); // NEW!
public float SinkDrainFraction { get; set; } = 0f;
public float SinkRecoilFraction { get; set; } = 0f;
```

---

### **Step 5: Create Sink Details Section UI**

**File:** `Assets/Viable/Core.Unity/UI/SinkDetailsSection.cs`

**UI Elements:**
```
SinkDetailsSection
?? SinkFormationThresholdSlider (0.0 - 1.0)
?? InitialSinkCountInput (0 - 100)
?? EditSinkPositionsButton
?? SinkBehaviorToggles (Drain On/Off, Recoil On/Off)
```

**Visibility:** Always visible (or when advanced mode enabled)

---

### **Step 6: Add Sink Position Editor Modal**

**Similar to PointSourceEditorModal:**
- Click "Edit Sink Positions"
- Modal opens with grid
- Click cells to place sinks
- Apply ? Adds to `InitialSinkPositions`

---

### **Step 7: Apply Sink Config to Engine**

**File:** `SimulationController.cs`

**In `InitStateInto()`:**
```csharp
// After seeding central region...

// Place initial sinks if configured
if (lastScenario.EngineConfig?.InitialSinkPositions != null)
{
    foreach (var sinkPos in lastScenario.EngineConfig.InitialSinkPositions)
    {
        int idx = s.Idx(sinkPos.X, sinkPos.Y);
        s.IsSink[idx] = true;
        s.SinkFormationTick[idx] = 0;
        Debug.Log($"[SimulationController] Placed initial sink at ({sinkPos.X}, {sinkPos.Y})");
    }
}
```

---

## ?? **QUICK WIN: Minimal Fix**

**If you want sinks working NOW, do this:**

### **Option A: Add Sink Count to Inspector**

**File:** `SimulationController.cs`

**Add this field:**
```csharp
[Header("Sink Configuration")]
[SerializeField] private int InitialSinkCount = 0;
[SerializeField] private float SinkSpacing = 10f;
```

**In `InitStateInto()`, add:**
```csharp
// Place sinks in a grid pattern
if (InitialSinkCount > 0)
{
    int sinksPerSide = Mathf.CeilToInt(Mathf.Sqrt(InitialSinkCount));
    int spacing = Mathf.FloorToInt(s.W / (sinksPerSide + 1));
    
    int sinksPlaced = 0;
    for (int sy = 0; sy < sinksPerSide && sinksPlaced < InitialSinkCount; sy++)
    {
        for (int sx = 0; sx < sinksPerSide && sinksPlaced < InitialSinkCount; sx++)
        {
            int x = (sx + 1) * spacing;
            int y = (sy + 1) * spacing;
            
            if (x < s.W && y < s.H)
            {
                int idx = s.Idx(x, y);
                s.IsSink[idx] = true;
                s.SinkFormationTick[idx] = 0;
                sinksPlaced++;
                
                Debug.Log($"[SimulationController] Placed sink {sinksPlaced} at ({x}, {y})");
            }
        }
    }
    
    Debug.Log($"[SimulationController] Placed {sinksPlaced} initial sinks");
}
```

**Result:** You can now set sink count in Inspector! Not in UI, but works immediately.

---

### **Option B: Expose Sink Threshold in Inspector**

**Already exists!** Look for:
```csharp
[SerializeField] private float SinkFormationThreshold = 0.5f;
```

Change this value in Unity Inspector ? Affects when sinks form naturally

---

## ?? **Testing the Fix**

### **Test 1: Sink Count**

1. Select SimulationController in Hierarchy
2. Inspector ? Set `InitialSinkCount = 4`
3. Press Play
4. Should see 4 magenta sinks placed in grid pattern

### **Test 2: Sink Threshold**

1. Inspector ? Set `SinkFormationThreshold = 0.1` (lower = more sinks)
2. Press Play
3. Run for 50 ticks
4. Should see MORE sinks forming naturally

### **Test 3: UI Changes (After Full Fix)**

1. UI ? Set Inflow to "Point Sources"
2. UI ? Click "Apply & Restart"
3. Simulation restarts with point sources
4. Simulation should look different!

---

## ?? **PRIORITY TASKS**

**To make UI actually control simulation:**

| Priority | Task | Effort | Impact |
|----------|------|--------|--------|
| **P0** | Add sink count to Inspector (Quick Win) | 10 min | Sinks work NOW |
| **P1** | Implement "Apply & Restart" button | 30 min | UI changes apply |
| **P2** | Create WorkingConfig ? Preset adapter | 1 hour | Full UI?Sim bridge |
| **P3** | Add Sink Details Section to UI | 2 hours | UI sink control |
| **P4** | Add Sink Position Editor modal | 2 hours | Click-to-place sinks |
| **P5** | Expose all parameters in UI | 4 hours | Complete control |

---

## ? **ACCEPTANCE CRITERIA**

**UI is connected to simulation when:**

- [ ] Changing Inflow dropdown ? Different simulation behavior
- [ ] Setting sink count ? Sinks appear in simulation
- [ ] Clicking "Apply & Restart" ? Simulation restarts with UI settings
- [ ] Changing any parameter ? Visible effect in simulation

---

## ?? **FILES TO MODIFY**

**High Priority:**
1. `SimulationController.cs` - Add initial sink placement
2. `TopBarUI.cs` - Implement "Apply & Restart"
3. `WorkingScenarioConfig.cs` - Add sink parameters

**Medium Priority:**
4. Create `WorkingConfigToPresetAdapter.cs`
5. Create `SinkDetailsSection.cs`

**Low Priority:**
6. Create `SinkPositionEditorModal.cs`
7. Expand `CoreParametersSection.cs`

---

## ?? **RESULT**

After these fixes:

? **UI controls simulation** - Changes apply immediately  
? **Sink control** - Number, position, threshold  
? **All parameters** - Exposed in UI  
? **Apply & Restart** - Button works  
? **Different behaviors** - Each config looks different  

**Your simulation will FINALLY respond to UI changes!** ??

---

**Ready to implement? Let me know which priority level you want to start with!**
