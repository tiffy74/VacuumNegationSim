# Stage 8 - Preset System

## ?? **Stage Summary**

**Status:** ? **COMPLETE**

**Goal:** Create ScriptableObject preset system for scenario management

**Delivered:**
- ScenarioPreset ScriptableObject (Unity-native configuration storage)
- 5 example presets (Balanced Growth, Collapse, Rapid Expansion, etc.)
- Preset adapter (converts presets to Engine's ScenarioDefinition)
- SimulationController integration (loads presets on Start)

---

## ?? **What Was Built**

### **1. ScenarioPreset ScriptableObject**

**Location:** `Assets/Viable/Core.Unity/ScenarioPreset.cs`

**Purpose:** Save simulation configurations as Unity assets

**Fields:**
```csharp
public class ScenarioPreset : ScriptableObject
{
    // Identity
    public string PresetName;
    public string Description;
    
    // Grid
    public int GridWidth = 64;
    public int GridHeight = 64;
    public int? Seed;
    
    // Initial Conditions
    public float InitialResourceGlobal = 1e7f;
    public float ScaleFactor = 1.0f;
    
    // All simulation parameters (50+ floats)
    public float DecayLoss = 0.003f;
    public float EthreshBase = 0.18f;
    // ... etc
    
    // Visualization
    public Color InactiveColor;
    public Color DormantRegionColor;
    public bool ShowComplexityTint;
    public float TicksPerSecond = 10f;
}
```

**Benefits:**
- **Unity Inspector editing** - No code changes needed
- **Asset files** - Version control friendly
- **Reusable** - Share presets between projects
- **Discoverable** - See all presets in Project window

---

### **2. Example Presets**

**Location:** `Assets/Viable/Core.Unity/Presets/Examples/`

**Created 5 presets:**

| Preset | Purpose | Key Parameters |
|--------|---------|----------------|
| **01_Balanced_Growth** | Default behavior, stable expansion | decayLoss: 0.003, resourceGlobalMax: 5e7 |
| **02_Resource_Collapse** | Rapid collapse from resource scarcity | decayLoss: 0.01, resourceGlobalMax: 1e7 |
| **03_RapidExpansion** | Fast frontier propagation | expansionRate: 2.0, regionExpansionChance: 0.5 |
| **04_Competitive** | Multiple competing regions | perturbationProbability: 0.001, seed varies |
| **05_Stochastic_Dynamics** | High randomness | perturbation high, multiple stochastic effects |

**Guide:** See [Assets/Viable/Core.Unity/Presets/Examples/README_EXAMPLES.md](../../Assets/Viable/Core.Unity/Presets/Examples/README_EXAMPLES.md)

---

### **3. ScenarioPresetAdapter**

**Location:** `Assets/Viable/Core.Unity/ScenarioPresetAdapter.cs`

**Purpose:** Convert ScenarioPreset ? ScenarioDefinition (Engine format)

**Key Methods:**
```csharp
public static class ScenarioPresetAdapter
{
    // Preset ? ScenarioDefinition (for Engine)
    public static ScenarioDefinition ToScenarioDefinition(ScenarioPreset preset);
    
    // Preset ? SimulationConfiguration (for Engine)
    public static SimulationConfiguration ToSimulationConfiguration(ScenarioPreset preset);
}
```

**Why Needed:**
- Engine uses `ScenarioDefinition` (pure C#)
- Unity uses `ScenarioPreset` (ScriptableObject)
- Adapter bridges the two

---

### **4. SimulationController Integration**

**Location:** `Assets/Viable/Core.Unity/Controllers/SimulationController.cs`

**Changes:**
```csharp
[Header("Preset System")]
[SerializeField] private ScenarioPreset scenarioPreset;

private void InitializeSimulation()
{
    if (scenarioPreset != null)
    {
        // Load from preset
        config = ScenarioPresetAdapter.ToSimulationConfiguration(scenarioPreset);
        lastScenario = ScenarioPresetAdapter.ToScenarioDefinition(scenarioPreset);
        Debug.Log($"[SimulationController] Loading from preset: {scenarioPreset.PresetName}");
    }
    else
    {
        // Fallback to legacy Inspector settings
        config = BuildLegacyConfig();
        Debug.Log("[SimulationController] No preset loaded - using legacy Inspector settings");
    }
    
    // ... rest of initialization
}
```

**Backward Compatibility:** Legacy Inspector parameters still work if no preset assigned

---

## ?? **Bug Fixes & Refactoring**

### **Fix 1: Preset-to-Engine Parameter Mapping**

**Problem:** ScenarioPreset has 50+ parameters, easy to miss mappings

**Solution:**
- Explicit mapping in `ToSimulationConfiguration()`
- Every preset field maps to Engine config field
- Compiler errors if field missing

**Files Changed:**
- `ScenarioPresetAdapter.cs` - Added all 50+ mappings

---

### **Fix 2: Default Preset Values**

**Problem:** New presets had uninitialized fields (0 or null)

**Solution:**
- Set sensible defaults in `ScenarioPreset` class
- Example: `DecayLoss = 0.003f` (not 0)
- New presets work immediately

**Files Changed:**
- `ScenarioPreset.cs` - Added default values to all fields

---

### **Fix 3: Seed Handling**

**Problem:** Seed is `int?` (nullable), Engine expects `int?` or generates random

**Solution:**
- Preset: `Seed` can be null (random) or set (deterministic)
- Adapter passes through correctly
- StepContext generates random seed if null

**Files Changed:**
- `ScenarioPresetAdapter.cs` - Preserves nullability
- `StepContext.cs` - Handles null seed correctly

---

### **Fix 4: Legacy Inspector Fallback**

**Problem:** Old scenes broke when preset system added

**Solution:**
- Keep legacy `BuildLegacyConfig()` method
- If no preset assigned, use Inspector values
- Backward compatibility maintained

**Files Changed:**
- `SimulationController.cs` - Kept legacy path

---

## ?? **Implementation Guide**

### **Creating a New Preset**

**Step 1: Create Asset**
```
Unity ? Project ? Right-click in Presets folder
? Create ? Viable ? Scenario Preset
```

**Step 2: Name It**
```
Rename to: "YourPresetName.asset"
```

**Step 3: Configure in Inspector**
```
Select preset asset
Inspector ? Fill in fields:
- Preset Name: "Descriptive Name"
- Description: "What this preset demonstrates"
- Grid Width/Height: 64, 64
- Seed: 42 (or leave blank for random)
- DecayLoss: 0.003
- ... all other parameters
```

**Step 4: Assign to SimulationController**
```
Hierarchy ? Select GameObject with SimulationController
Inspector ? Scenario Preset: Drag your preset here
```

**Step 5: Test**
```
Press Play ? Console should show:
[SimulationController] Loading from preset: YourPresetName
```

---

### **Loading Presets at Runtime**

**In SimulationController:**
```csharp
public void LoadPreset(ScenarioPreset preset)
{
    scenarioPreset = preset;
    InitializeSimulation(); // Reload with new preset
}
```

**From UI (future - Stage 12):**
```csharp
// TopBar preset dropdown
void OnPresetSelected(int index)
{
    var preset = availablePresets[index];
    simulationController.LoadPreset(preset);
}
```

---

## ?? **Preset Organization**

### **Folder Structure**
```
Assets/Viable/Core.Unity/Presets/
?? Examples/           (5 demo presets)
?  ?? 01_Balanced_Growth.asset
?  ?? 02_Resource_Collapse.asset
?  ?? 03_RapidExpansion.asset
?  ?? 04_Competitive.asset
?  ?? 05_Stochastic_Dynamics.asset
?  ?? README_EXAMPLES.md
?
?? Internal/           (Development/testing presets)
?  ?? ConstrainedResourceDiffusion.asset
?  ?? README_INTERNAL.md
?
?? Research_Archive/   (Original research presets)
   ?? VNS_Conservative_Default_Preset.asset
   ?? README_ARCHIVE.md
```

**Examples:** Polished, documented, for users  
**Internal:** Development/testing, may be unstable  
**Archive:** Historical, preserves original research

---

## ?? **Testing Checklist**

### **Test 1: Preset Loads on Start**
- [ ] Assign preset to SimulationController
- [ ] Press Play
- [ ] Console shows `[SimulationController] Loading from preset: [Name]`
- [ ] Simulation runs with preset parameters ?

### **Test 2: Parameter Values Applied**
- [ ] Set `DecayLoss = 0.01` in preset
- [ ] Run simulation
- [ ] Cells should die faster (visible collapse)
- [ ] Verify parameter applied ?

### **Test 3: Seed Determinism**
- [ ] Set `Seed = 42` in preset
- [ ] Run simulation ? Record results
- [ ] Restart Unity
- [ ] Run again ? Results identical ?

### **Test 4: Legacy Fallback**
- [ ] Remove preset from SimulationController (set to None)
- [ ] Press Play
- [ ] Console shows `[SimulationController] No preset loaded - using legacy Inspector settings`
- [ ] Simulation uses Inspector values ?

### **Test 5: Preset Switching (Stage 12)**
- [ ] Load preset A
- [ ] Switch to preset B
- [ ] Behavior changes
- [ ] Switch back to preset A
- [ ] Original behavior restored ?

---

## ?? **Metrics**

**Code Added:**
- `ScenarioPreset.cs`: ~300 lines
- `ScenarioPresetAdapter.cs`: ~200 lines
- Integration in `SimulationController.cs`: ~100 lines
- **Total:** ~600 lines

**Assets Created:**
- 5 example presets
- 1 internal preset
- 1 archived preset
- 3 README files

**Time Investment:**
- Design & implementation: ~10 hours
- Preset creation & testing: ~5 hours
- Documentation: ~3 hours
- **Total:** ~18 hours

---

## ?? **Success Criteria**

**Stage 8 complete when:**

- [x] ScenarioPreset ScriptableObject created
- [x] 5 example presets created
- [x] Adapter converts preset ? ScenarioDefinition
- [x] SimulationController loads presets
- [x] Legacy Inspector fallback works
- [x] Presets organized in folders
- [x] Documentation complete

---

## ?? **Next Stage**

**Stage 9 - Export System**

**Goal:** Publication-ready CSV/JSON export with reproducibility verification

**Why:** Need to export simulation data for analysis (R, Python, Excel)

**Guide:** See [Stage 9 Summary](Stage9_EXPORT_SYSTEM.md)

---

## ?? **Related Documentation**

- **Preset Examples:** [Assets/Viable/Core.Unity/Presets/Examples/README_EXAMPLES.md](../../Assets/Viable/Core.Unity/Presets/Examples/README_EXAMPLES.md)
- **Preset Archive:** [Assets/Viable/Core.Unity/Presets/Research_Archive/README_ARCHIVE.md](../../Assets/Viable/Core.Unity/Presets/Research_Archive/README_ARCHIVE.md)
- **Main README:** [README.md](../../README.md) - Preset system usage

---

**Last Updated:** 2024  
**Status:** ? Stage 8 Complete  
**Next:** ? Stage 9 (Export System)
