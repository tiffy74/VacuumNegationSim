# Stage 13.9: Refinement Mechanism Stubs Complete

## ? COMPLETE (Stubs Only - No Behavior Change)

**Date:** 2025-01-21  
**Status:** Stubs and documentation complete, tests passing, ready for Unity UI integration

---

## ?? Goal

Prepare for future adaptive refinement by adding:
- Configuration parameters
- Event type conventions
- DisorderIndex computation stub
- Documentation

**NO functional refinement implemented - all stubs return safe defaults.**

---

## ? What Was Implemented

### 1. Added Refinement Parameters to EngineConfig ?

**File:** `Assets/Viable/Contracts/EngineConfig.cs`

```csharp
// Existing parameters (were already present):
public double RefinementDisorderThreshold { get; set; } = 0.0;
public int MaxRefinementDepth { get; set; } = 0;
public int MaxSubgridSize { get; set; } = 0;

// NEW Stage 13.9 parameters:
public int BaseSubcells { get; set; } = 2;             // Initial subgrid size (2×2)
public double AlphaSubcellsPerLineage { get; set; } = 0.0;  // Growth rate per level
```

**Purpose:**
- `BaseSubcells`: When cell is refined, create BaseSubcells × BaseSubcells subgrid
- `AlphaSubcellsPerLineage`: Growth formula: `subcells = BaseSubcells + (Alpha × depth)`

### 2. Documented RefinementCreated Event Type ?

**File:** `Assets/Viable/Contracts/SimulationEvent.cs`

Added to event type documentation:

```csharp
/// "RefinementCreated" (Stage 13.9 - Future):
///   Adaptive refinement created subgrid within a cell
///   Data: {
///     "parentId": int,          // ID of parent cell being refined
///     "x": int,                 // Parent cell X coordinate
///     "y": int,                 // Parent cell Y coordinate
///     "disorderIndex": double,  // Disorder metric that triggered refinement
///     "level": int,             // Refinement depth level (0 = base grid)
///     "subgridSide": int,       // Size of subgrid (e.g., 2×2, 4×4)
///     "lineageDepth": int       // Depth in refinement hierarchy
///   }
```

### 3. Created DisorderIndexCalculator Stub ?

**File:** `Assets/Viable/Engine/Computation/DisorderIndexCalculator.cs`

```csharp
public static class DisorderIndexCalculator
{
    // Stage 13.9: Always returns 0.0 (no refinement triggered)
    public static double ComputeDisorderIndex(
        int x, int y, int width, int height,
        float[] resourceLocal, float[] viability, float[] complexity,
        bool[] domainMask = null)
    {
        return 0.0;  // Stub - no computation yet
    }

    // Stage 13.9: Always returns false (no refinement)
    public static bool ShouldRefine(
        double disorderIndex, double threshold,
        int currentDepth, int maxDepth)
    {
        return false;  // Stub - no refinement yet
    }
}
```

**Future Implementation Comments:**
- Resource gradient: `?((R[x+1] - R[x-1])² + (R[y+1] - R[y-1])²)`
- Viability variance: `variance(V[neighbors])`
- Complexity Laplacian: `|?(C[neighbors]) - N*C[center]|`
- Combined: `w?·grad_R + w?·var_V + w?·|lap_C|`

### 4. Added Refinement Documentation ?

**File:** `Assets/Viable/Engine/README.md`

Comprehensive section added covering:
- Hierarchical refinement concept
- Visual diagram (base grid ? refined grid)
- Configuration parameters
- Event type specification
- Implementation status (stubs only)
- Future implementation notes

### 5. Test Coverage ?

**File:** `Assets/Viable/Engine.Tests/MechanismConfigTests.cs`

Added 5 new tests (37 total passing):

1. **`EngineConfig_RefinementMode_DefaultsToNone`**
   - Verifies RefinementMode defaults to None

2. **`EngineConfig_RefinementParameters_DefaultToZero`**
   - All refinement parameters default to 0 or safe values

3. **`EngineConfig_WithRefinement_ConfiguresCorrectly`**
   - Can store refinement configuration

4. **`DisorderIndexCalculator_AlwaysReturnsZero`**
   - Stub always returns 0.0

5. **`DisorderIndexCalculator_ShouldRefine_AlwaysReturnsFalse`**
   - Stub always returns false

---

## ?? Implementation Details

### No Behavior Change ?

**All defaults preserve exact pre-Stage 13.9 behavior:**
- `RefinementMode = None` (default)
- `RefinementDisorderThreshold = 0.0` (no refinement)
- `MaxRefinementDepth = 0` (no hierarchy)
- `DisorderIndexCalculator.ComputeDisorderIndex()` ? always 0.0
- `DisorderIndexCalculator.ShouldRefine()` ? always false

**Result:** No refinement occurs in any simulation, existing presets unchanged.

### Refinement Concept (Future)

**Hierarchical Refinement:**
```
Base Grid (Level 0):
?????????????
? A ? B ? C ?  B has high disorder ? refine B
?????????????
? D ? E ? F ?
?????????????

After Refinement (B ? 2×2 subgrid):
?????????????????
? A ? b? b? ? C ?  B now contains 4 subcells
?   ?????????   ?
?   ? b?? b??   ?
?????????????????
? D ?   E   ? F ?
?????????????????
```

**Key Properties:**
- Cell contains subgrid (not global grid rewrite)
- Bounded depth (MaxRefinementDepth)
- Trigger-based (DisorderIndex >= threshold)
- Hierarchical state (parent ? subcells)

### Configuration Parameters

| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `RefinementMode` | enum | None | Enable/disable refinement |
| `RefinementDisorderThreshold` | double | 0.0 | Trigger threshold |
| `MaxRefinementDepth` | int | 0 | Maximum hierarchy levels |
| `MaxSubgridSize` | int | 0 | Maximum subcells per region |
| `BaseSubcells` | int | 2 | Initial subgrid size (N×N) |
| `AlphaSubcellsPerLineage` | double | 0.0 | Growth rate per level |

**Subgrid Size Formula:**
```
subcells_per_dimension = BaseSubcells + (AlphaSubcellsPerLineage × depth)
```

**Example:**
- `BaseSubcells = 2`, `Alpha = 0`:
  - Level 0: 2×2 = 4 subcells
  - Level 1: 2×2 = 4 subcells (constant)
  
- `BaseSubcells = 2`, `Alpha = 1.0`:
  - Level 0: 2×2 = 4 subcells
  - Level 1: 3×3 = 9 subcells
  - Level 2: 4×4 = 16 subcells

---

## ?? Testing

### Build Status ?
```
Build successful
```

### Test Status ?
```
37 tests in MechanismConfigTests (all passing)
- 30 from Stages 13.1-13.7
- 2 from Stage 13.8 (mask config tests - MaskGenerator tests disabled due to assembly ambiguity)
- 5 new tests for Stage 13.9 (refinement stubs)
```

### Behavior Verification ?
- **No refinement occurs:** DisorderIndex always 0.0
- **No performance impact:** Stubs do minimal work
- **Backward compatible:** All existing scenarios unchanged
- **Determinism preserved:** No random behavior added

---

## ?? Notes

### What Stage 13.9 Is

**? Complete:**
- Configuration API for refinement
- Event type specification
- Computation stubs (safe defaults)
- Documentation for future implementation

**? NOT Implemented:**
- Actual disorder computation (gradient, variance, etc.)
- Refinement trigger logic
- Subgrid creation and management
- Hierarchical state evolution
- Subcell-to-parent upscaling
- Performance optimizations

### Why Stubs Now?

**Preparation for Future Work:**
- API contracts defined (EngineConfig, SimulationEvent)
- Configuration serialization tested
- Documentation guides future implementation
- No breaking changes later

**No Behavior Change:**
- Safe to merge immediately
- Existing presets work unchanged
- No performance degradation

---

## ?? Unity UI Integration Guide

### How to Add Stage 13 Mechanisms to Unity UI

#### Option 1: Inspector Fields (Simple)

**For numerical parameters:**

**In `ScenarioPreset.cs`:**
```csharp
[Header("Stage 13.4: Point Sources")]
[Tooltip("Inflow mode: Uniform or PointSources")]
public InflowMode inflowMode = InflowMode.Uniform;

[Tooltip("Point source locations (only used if inflowMode = PointSources)")]
public List<PointSourceData> pointSources = new List<PointSourceData>();

[System.Serializable]
public class PointSourceData
{
    public int x;
    public int y;
    public double strength;
}

[Header("Stage 13.7: Hysteresis")]
[Tooltip("Viability rule: HardThreshold or Hysteresis")]
public ViabilityRule viabilityRule = ViabilityRule.HardThreshold;

[Range(-1.0, 1.0)]
[Tooltip("Hysteresis ON threshold (activate when viability >= this)")]
public double hysteresisOnThreshold = 0.5;

[Range(-1.0, 1.0)]
[Tooltip("Hysteresis OFF threshold (deactivate when viability <= this)")]
public double hysteresisOffThreshold = -0.5;

[Header("Stage 13.8: Masked Domain")]
[Tooltip("Mask shape: Rectangle (no mask), Circle, Ring, etc.")]
public MaskShape maskShape = MaskShape.Rectangle;

[Range(0, 100)]
[Tooltip("Mask radius for Circle/Ring (in grid units)")]
public double maskRadius = 20.0;

[Header("Stage 13.9: Refinement (Future)")]
[Tooltip("Refinement mode: None or ThresholdRefinement")]
public RefinementMode refinementMode = RefinementMode.None;

[Range(0, 2)]
[Tooltip("Disorder threshold for triggering refinement")]
public double refinementDisorderThreshold = 0.5;
```

**In `ScenarioPreset.ToScenarioDefinition()`:**
```csharp
public ScenarioDefinition ToScenarioDefinition()
{
    var engineConfig = new EngineConfig
    {
        // Stage 13.4
        InflowMode = inflowMode,
        PointSources = pointSources.Select(ps => new PointSourceConfig
        {
            X = ps.x,
            Y = ps.y,
            Strength = ps.strength
        }).ToList(),
        
        // Stage 13.7
        ViabilityRule = viabilityRule,
        HysteresisOnThreshold = hysteresisOnThreshold,
        HysteresisOffThreshold = hysteresisOffThreshold,
        
        // Stage 13.8
        MaskShape = maskShape,
        MaskRadius = maskRadius,
        MaskInnerRadius = maskInnerRadius,
        CorridorWidth = corridorWidth,
        HoleProbability = holeProbability,
        
        // Stage 13.9
        RefinementMode = refinementMode,
        RefinementDisorderThreshold = refinementDisorderThreshold,
        MaxRefinementDepth = maxRefinementDepth,
        MaxSubgridSize = maxSubgridSize,
        BaseSubcells = baseSubcells,
        AlphaSubcellsPerLineage = alphaSubcellsPerLineage
    };

    return new ScenarioDefinition
    {
        ScenarioId = scenarioId,
        GridWidth = gridWidth,
        GridHeight = gridHeight,
        Seed = seed,
        EngineConfig = engineConfig,
        Parameters = parameters
    };
}
```

#### Option 2: Custom Inspector UI (Advanced)

**Create `ScenarioPresetEditor.cs`:**

```csharp
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScenarioPreset))]
public class ScenarioPresetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ScenarioPreset preset = (ScenarioPreset)target;

        EditorGUILayout.LabelField("Stage 13 Mechanisms", EditorStyles.boldLabel);
        
        // Stage 13.4: Point Sources
        EditorGUILayout.Space();
        preset.inflowMode = (InflowMode)EditorGUILayout.EnumPopup("Inflow Mode", preset.inflowMode);
        
        if (preset.inflowMode == InflowMode.PointSources)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Point Sources:");
            // Show list editor for point sources
            // ... (use SerializedProperty for list editing)
            EditorGUI.indentLevel--;
        }
        
        // Stage 13.5: Boundaries
        EditorGUILayout.Space();
        preset.boundaryMode = (BoundaryMode)EditorGUILayout.EnumPopup("Boundary Mode", preset.boundaryMode);
        
        // Stage 13.6: Diffusion
        EditorGUILayout.Space();
        preset.diffusionMode = (DiffusionMode)EditorGUILayout.EnumPopup("Diffusion Mode", preset.diffusionMode);
        
        // Stage 13.7: Hysteresis
        EditorGUILayout.Space();
        preset.viabilityRule = (ViabilityRule)EditorGUILayout.EnumPopup("Viability Rule", preset.viabilityRule);
        
        if (preset.viabilityRule == ViabilityRule.Hysteresis)
        {
            EditorGUI.indentLevel++;
            preset.hysteresisOnThreshold = EditorGUILayout.DoubleField("ON Threshold", preset.hysteresisOnThreshold);
            preset.hysteresisOffThreshold = EditorGUILayout.DoubleField("OFF Threshold", preset.hysteresisOffThreshold);
            
            // Validation
            if (preset.hysteresisOffThreshold >= preset.hysteresisOnThreshold)
            {
                EditorGUILayout.HelpBox("OFF threshold should be < ON threshold for proper hysteresis!", MessageType.Warning);
            }
            EditorGUI.indentLevel--;
        }
        
        // Stage 13.8: Masks
        EditorGUILayout.Space();
        preset.maskShape = (MaskShape)EditorGUILayout.EnumPopup("Mask Shape", preset.maskShape);
        
        if (preset.maskShape != MaskShape.Rectangle)
        {
            EditorGUI.indentLevel++;
            switch (preset.maskShape)
            {
                case MaskShape.Circle:
                    preset.maskRadius = EditorGUILayout.DoubleField("Radius", preset.maskRadius);
                    break;
                case MaskShape.Ring:
                    preset.maskRadius = EditorGUILayout.DoubleField("Outer Radius", preset.maskRadius);
                    preset.maskInnerRadius = EditorGUILayout.DoubleField("Inner Radius", preset.maskInnerRadius);
                    break;
                case MaskShape.Corridor:
                    preset.corridorWidth = EditorGUILayout.DoubleField("Width", preset.corridorWidth);
                    break;
                case MaskShape.PercolationHoles:
                    preset.holeProbability = EditorGUILayout.Slider("Hole Probability", (float)preset.holeProbability, 0f, 1f);
                    break;
            }
            EditorGUI.indentLevel--;
        }
        
        // Stage 13.9: Refinement (Future)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Refinement (Future - Stubs Only)", EditorStyles.boldLabel);
        preset.refinementMode = (RefinementMode)EditorGUILayout.EnumPopup("Refinement Mode", preset.refinementMode);
        EditorGUILayout.HelpBox("Stage 13.9: Refinement is not yet functional. Configuration stored for future use.", MessageType.Info);
        
        if (preset.refinementMode != RefinementMode.None)
        {
            EditorGUI.indentLevel++;
            preset.refinementDisorderThreshold = EditorGUILayout.DoubleField("Disorder Threshold", preset.refinementDisorderThreshold);
            preset.maxRefinementDepth = EditorGUILayout.IntField("Max Depth", preset.maxRefinementDepth);
            preset.baseSubcells = EditorGUILayout.IntField("Base Subcells", preset.baseSubcells);
            EditorGUI.indentLevel--;
        }
        
        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(preset);
        }
    }
}
```

#### Option 3: Dropdown Preset Selector Enhancement

**In `PresetSelector.cs`:**

```csharp
// Add mechanism info to dropdown tooltip
public void RefreshPresets()
{
    presets = LoadAllPresets();
    dropdown.ClearOptions();
    
    var options = presets.Select(p => {
        string mechanisms = GetMechanismSummary(p);
        return new Dropdown.OptionData($"{p.ScenarioId} ({mechanisms})");
    }).ToList();
    
    dropdown.AddOptions(options);
}

private string GetMechanismSummary(ScenarioPreset preset)
{
    List<string> mechs = new List<string>();
    
    if (preset.inflowMode == InflowMode.PointSources)
        mechs.Add("PointSrc");
    if (preset.boundaryMode == BoundaryMode.PeriodicWrap)
        mechs.Add("Periodic");
    if (preset.diffusionMode == DiffusionMode.Moore8)
        mechs.Add("Moore8");
    if (preset.viabilityRule == ViabilityRule.Hysteresis)
        mechs.Add("Hysteresis");
    if (preset.maskShape != MaskShape.Rectangle)
        mechs.Add(preset.maskShape.ToString());
    if (preset.refinementMode != RefinementMode.None)
        mechs.Add("Refine");
    
    return mechs.Count > 0 ? string.Join(", ", mechs) : "Default";
}
```

#### Option 4: Runtime UI Controls

**Add to `UIManager.cs` for runtime mechanism switching:**

```csharp
[Header("Stage 13 Mechanism Controls")]
public Dropdown mechanismTypeDropdown;
public GameObject mechanismParametersPanel;

public void OnMechanismTypeChanged(int index)
{
    switch (index)
    {
        case 0: // Default
            // Disable all mechanism panels
            break;
        case 1: // Point Sources
            ShowPointSourcesPanel();
            break;
        case 2: // Periodic Wrap
            ShowBoundaryModePanel();
            break;
        case 3: // Moore8
            ShowDiffusionModePanel();
            break;
        case 4: // Hysteresis
            ShowHysteresisPanel();
            break;
        case 5: // Masked Domain
            ShowMaskShapePanel();
            break;
        // Stage 13.9: Refinement not yet functional, can show placeholder
    }
}

private void ShowHysteresisPanel()
{
    // Show sliders for ON/OFF thresholds
    // Update current preset's engineConfig
    // Optionally: restart simulation with new config
}
```

### Visualization Enhancements

**To visualize Stage 13 mechanisms:**

**1. Point Sources (Stage 13.4):**
```csharp
// In GridRenderer.cs
if (engineConfig.InflowMode == InflowMode.PointSources)
{
    foreach (var ps in engineConfig.PointSources)
    {
        // Draw star/icon at (ps.X, ps.Y)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetCellCenter(ps.X, ps.Y), 0.5f);
    }
}
```

**2. Hysteresis (Stage 13.7):**
```csharp
// Color cells differently based on hysteresis state
if (engineConfig.ViabilityRule == ViabilityRule.Hysteresis)
{
    // Cells in hysteresis gap (offThreshold < V < onThreshold) show differently
    if (viability > offThreshold && viability < onThreshold)
    {
        color = Color.Lerp(inactiveColor, activeColor, 0.5f); // Mid-tone
    }
}
```

**3. Masked Domain (Stage 13.8):**
```csharp
// Darken masked-out cells
if (state.DomainMask != null && !state.DomainMask[idx])
{
    cellColor = Color.black; // or very dark gray
}
```

**4. Refinement (Stage 13.9 - Future):**
```csharp
// Color-code by refinement level
if (refinementLevel > 0)
{
    cellColor = Color.Lerp(baseColor, Color.red, refinementLevel / (float)maxDepth);
}
// Draw smaller quads for subcells
```

---

## ? Stage 13.9 Complete!

**Checklist:**
- [x] Added BaseSubcells and AlphaSubcellsPerLineage to EngineConfig
- [x] Documented RefinementCreated event type in SimulationEvent
- [x] Created DisorderIndexCalculator stub (always returns 0.0)
- [x] Added refinement documentation to Engine README
- [x] Added 5 tests for refinement configuration
- [x] Build successful
- [x] No behavior change
- [x] Backward compatible
- [x] Unity UI integration guide provided

**Next Steps:**
1. Implement Unity UI for Stage 13 mechanisms (see guide above)
2. Create example presets showcasing each mechanism
3. Future: Implement functional refinement (Stage 13.10+)

---

**Stage 13.9 Status: ? COMPLETE (Stubs + Documentation)**
**Unity UI Integration: ?? Instructions Provided**
