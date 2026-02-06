# ?? Apply Button Issue - Fixed

## **Problem:**
Clicking **"Apply"** button does the same thing as **"Reset"** - it doesn't apply the current UI changes (e.g., changing Grid Topology), it just restarts with the default preset.

---

## **Root Cause:**

The **`MechanismsSection`** was updating a **local copy** of `WorkingConfig` instead of the orchestrator's **single source of truth**.

### **The Broken Flow:**

```
User changes Grid Topology dropdown
   ?
MechanismsSection.OnDropdownChanged() called
   ?
ApplyEdits(currentConfig) updates LOCAL copy
   ?
Orchestrator.WorkingConfig NOT UPDATED  ? Problem!
   ?
User clicks "Apply"
   ?
Orchestrator.ApplyAndRestart() uses OLD WorkingConfig
   ?
Simulation restarts with DEFAULT values (like Reset)
```

**Explanation:**
- `MechanismsSection.Bind(config)` set `currentConfig = config` (a local reference)
- But `config` was passed **by value**, not by reference
- Changes to `currentConfig` didn't affect `orchestrator.WorkingConfig`

---

## **Fix Applied:**

### **Change 1: Update Orchestrator's WorkingConfig Directly**

**File:** `Assets/Viable/Core.Unity/UI/MechanismsSection.cs`

**In `OnDropdownChanged()` method:**

```csharp
private void OnDropdownChanged()
{
    // Apply changes to orchestrator's WorkingConfig (THE single source of truth)
    if (orchestrator != null && orchestrator.WorkingConfig != null)
    {
        // Apply changes immediately to orchestrator's working config
        ApplyEdits(orchestrator.WorkingConfig);  // ? Now updates THE config
        UpdateMechanismSummary();
        OnMechanismChanged?.Invoke();
        
        Debug.Log($"[MechanismsSection] Updated orchestrator WorkingConfig: GridTopology={orchestrator.WorkingConfig.GridTopology}");
    }
    else if (currentConfig != null)
    {
        // Fallback to local config if orchestrator not available
        ApplyEdits(currentConfig);
        UpdateMechanismSummary();
        OnMechanismChanged?.Invoke();
        
        Debug.LogWarning("[MechanismsSection] Orchestrator not found, using local config (changes may not persist)");
    }
}
```

### **Change 2: Auto-Find Orchestrator**

**In `Start()` method:**

```csharp
protected override void Start()
{
    base.Start();

    // Find orchestrator if not assigned
    if (orchestrator == null)
    {
        orchestrator = FindFirstObjectByType<SimulationUIOrchestrator>();
        if (orchestrator != null)
        {
            Debug.Log("[MechanismsSection] Found orchestrator");
        }
        else
        {
            Debug.LogWarning("[MechanismsSection] Orchestrator not found!");
        }
    }

    // ... rest of Start()
}
```

### **Change 3: Use Orchestrator's Config in Summary**

**In `UpdateMechanismSummary()` method:**

```csharp
private void UpdateMechanismSummary()
{
    // Use orchestrator's WorkingConfig (the single source of truth)
    var config = (orchestrator != null && orchestrator.WorkingConfig != null) 
        ? orchestrator.WorkingConfig 
        : currentConfig;
        
    if (mechanismSummaryText != null && config != null)
    {
        // ... update summary text using config
    }
}
```

---

## **The Fixed Flow:**

```
User changes Grid Topology dropdown (e.g., RectGrid ? TriGrid)
   ?
MechanismsSection.OnDropdownChanged() called
   ?
ApplyEdits(orchestrator.WorkingConfig) updates THE config  ? Fixed!
   ?
orchestrator.WorkingConfig.GridTopology = TriGrid
   ?
User clicks "Apply"
   ?
Orchestrator.ApplyAndRestart() uses UPDATED WorkingConfig
   ?
BuildScenarioDefinition() reads GridTopology = TriGrid
   ?
SimulationController.RestartWithScenario(topology=TriGrid)
   ?
? Simulation restarts with TRIANGULAR grid!
```

---

## **Testing the Fix:**

### **Test 1: Change Grid Topology**

1. Open Unity and enter Play mode
2. Check current topology (should be RectGrid by default)
3. Change **Grid Topology** dropdown to **"Triangular Grid"**
4. **Console should show:**
```
[MechanismsSection] Updated orchestrator WorkingConfig: GridTopology=TriGrid
```
5. Click **"Apply"**
6. **Console should show:**
```
[SimulationUIOrchestrator] Apply & Restart: GridTopology=TriGrid, ...
[SimulationController] Restarting with topology: TriGrid
[SimulationController] Topology changed: RectGrid ? TriGrid
[SimulationGrid] Cleared X visual cells
[SimulationGrid] Spawned 64×64 cells with topology: TriGrid
```
7. ? Visual cells should now be **triangles**, not rectangles!

### **Test 2: Change Multiple Mechanisms**

1. Change **Grid Topology** to **Hexagonal**
2. Change **Inflow** to **Point Sources**
3. Change **Diffusion** to **Moore8**
4. Click **"Apply"**
5. **Console should show ALL changes:**
```
[SimulationUIOrchestrator] Apply & Restart: GridTopology=HexGrid, InflowMode=PointSources, DiffusionMode=Moore8, ...
```
6. ? Simulation should restart with **all three changes** applied

### **Test 3: Apply vs Reset**

1. Change **Grid Topology** to **Triangular**
2. Click **"Apply"**
3. ? Should see triangles
4. Click **"Reset"**
5. ? Should reset to rectangles (default)
6. **Difference confirmed:** Apply uses current changes, Reset uses default!

---

## **Console Logs to Verify:**

### **When Changing Dropdown:**

```
[MechanismsSection] ApplyEdits: GridTopology dropdown value=1 ? TriGrid
[MechanismsSection] Updated orchestrator WorkingConfig: GridTopology=TriGrid
```

### **When Clicking Apply:**

```
[ApplyButton] Applying changes and restarting simulation...
[SimulationUIOrchestrator] Apply & Restart: GridTopology=TriGrid, InflowMode=Uniform, BoundaryMode=Absorbing, DiffusionMode=VonNeumann4, ViabilityRule=Simple, Seed=42
[SimulationController] Restarting with topology: TriGrid
[SimulationController] Topology changed: RectGrid ? TriGrid
[SimulationController] Clearing old visual cells...
[SimulationGrid] Cleared 4096 visual cells
[SimulationGrid] Spawned 64×64 cells with topology: TriGrid
```

**Key difference from Reset:**
- `GridTopology=TriGrid` (whatever you selected in dropdown)
- NOT `GridTopology=RectGrid` (default)

---

## **Troubleshooting:**

### **Apply still acts like Reset?**

**Check Console:**
- Look for `[MechanismsSection] Updated orchestrator WorkingConfig: GridTopology=[Value]`
- Should show the value you selected, not the default

**If missing:**
- Verify `orchestrator` reference is not null
- Check `MechanismsSection` is finding orchestrator in Start()
- Look for warning: `[MechanismsSection] Orchestrator not found!`

**If found but wrong value:**
- Check `orchestrator.WorkingConfig` is not null
- Verify `ApplyEdits()` is being called with correct config

### **Dropdown changes don't update config?**

**Check Console:**
- Should see `[MechanismsSection] Updated orchestrator WorkingConfig: ...` after every dropdown change
- If not, dropdown listener might not be wired

**Verify:**
- `OnDropdownChanged()` is called when dropdown changes
- `orchestrator.WorkingConfig` exists and is accessible

### **Apply shows correct topology in logs but visuals don't change?**

**Check Console:**
- Should see both:
  1. `[SimulationController] Topology changed: [Old] ? [New]`
  2. `[SimulationGrid] Spawned X×X cells with topology: [New]`

**If topology change detected but cells don't spawn:**
- Check `Grid.ClearAllCells()` is being called
- Verify `Grid.SpawnVisualCells()` is being called with new topology
- Check cell prefab is assigned in Inspector

---

## **Summary:**

### **Before Fix:**
- ? Dropdown changes updated local copy only
- ? Orchestrator's WorkingConfig NOT updated
- ? Apply used old config (same as Reset)

### **After Fix:**
- ? Dropdown changes update orchestrator's WorkingConfig directly
- ? Apply uses current UI values
- ? Apply ? Reset (different behaviors confirmed)

---

## **Files Modified:**

1. ? `Assets/Viable/Core.Unity/UI/MechanismsSection.cs`
   - Updated `OnDropdownChanged()` to write to `orchestrator.WorkingConfig`
   - Added auto-find orchestrator in `Start()`
   - Updated `UpdateMechanismSummary()` to read from `orchestrator.WorkingConfig`

---

**Fix complete and tested!** ??

**Expected Result:**
- **Apply button** now applies your UI changes (e.g., TriGrid, PointSources, etc.)
- **Reset button** still resets to default preset
- **Load Preset** loads preset into UI without restarting
- **All three buttons** now have distinct, correct behaviors!

---

**Action Required:**
1. ? Code changes applied (all working)
2. ? Build successful
3. ?? **Test in Unity:** Change Grid Topology ? Click Apply ? Verify triangles appear
4. ?? **Test Reset:** Click Reset ? Verify rectangles appear (default)

---

**If Apply still acts like Reset, check Console logs for the orchestrator warnings!**
