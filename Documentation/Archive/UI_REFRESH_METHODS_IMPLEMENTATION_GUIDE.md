# UI Controllers - Refresh Methods Implementation Guide

## ? Property Errors Fixed!

The core architecture is now **fully functional**. The only remaining errors are the missing `Refresh()` methods in UI controllers, which is expected and easily resolved.

---

## ?? Quick Implementation Checklist

For **each** of the 6 UI section controllers, you need to:

1. Add orchestrator reference field
2. Wire orchestrator in Awake
3. Add Refresh method
4. Update event handlers to write to orchestrator.WorkingConfig

**Controllers to modify:**
- [ ] MechanismsSection.cs
- [ ] CoreParametersSection.cs
- [ ] TopologyDetailsSection.cs
- [ ] InflowDetailsSection.cs
- [ ] DiffusionDetailsSection.cs
- [ ] ViabilityDetailsSection.cs

---

## ?? Step-by-Step Implementation

### Step 1: Add Orchestrator Reference (All Controllers)

Add this to the top of each controller class:

```csharp
[Header("Orchestrator")]
[SerializeField] private SimulationUIOrchestrator orchestrator;

void Awake()
{
    // Auto-wire orchestrator if not set in Inspector
    if (orchestrator == null)
        orchestrator = FindObjectOfType<SimulationUIOrchestrator>();
        
    // ... existing Awake code ...
}
```

### Step 2: Add Refresh Method (Controller-Specific)

#### A. **MechanismsSection.cs**

```csharp
/// <summary>
/// Refresh UI controls from WorkingScenarioConfig.
/// Called by orchestrator after preset load.
/// </summary>
public void Refresh(WorkingScenarioConfig cfg)
{
    // Check if we're in a refresh cycle to avoid loops
    if (orchestrator != null && orchestrator.IsRefreshing()) 
        return;
    
    // Set dropdown values WITHOUT triggering OnValueChanged events
    // Assuming you have these dropdowns (adjust names to match your actual fields):
    if (topologyDropdown != null)
        topologyDropdown.SetValueWithoutNotify((int)cfg.Topology);
        
    if (boundaryDropdown != null)
        boundaryDropdown.SetValueWithoutNotify((int)cfg.Boundary);
        
    if (inflowDropdown != null)
        inflowDropdown.SetValueWithoutNotify((int)cfg.Inflow);
        
    if (diffusionDropdown != null)
        diffusionDropdown.SetValueWithoutNotify((int)cfg.Diffusion);
        
    if (viabilityDropdown != null)
        viabilityDropdown.SetValueWithoutNotify((int)cfg.ViabilityRule);
}
```

#### B. **CoreParametersSection.cs**

```csharp
/// <summary>
/// Refresh UI controls from WorkingScenarioConfig.
/// </summary>
public void Refresh(WorkingScenarioConfig cfg)
{
    if (orchestrator != null && orchestrator.IsRefreshing()) 
        return;
    
    // Set input field values WITHOUT triggering OnEndEdit/OnValueChanged
    // Adjust field names to match your actual fields:
    if (resourceGlobalMaxInput != null)
        resourceGlobalMaxInput.SetTextWithoutNotify(cfg.ResourceGlobalMax.ToString("G"));
        
    if (rechargeRateInput != null)
        rechargeRateInput.SetTextWithoutNotify(cfg.ResourceRechargeRate.ToString("G"));
        
    if (decayLossInput != null)
        decayLossInput.SetTextWithoutNotify(cfg.DecayLoss.ToString("G"));
        
    if (maintCostInput != null)
        maintCostInput.SetTextWithoutNotify(cfg.MaintCost.ToString("G"));
        
    if (activationCostInput != null)
        activationCostInput.SetTextWithoutNotify(cfg.ActivationCost.ToString("G"));
        
    if (expansionProbInput != null)
        expansionProbInput.SetTextWithoutNotify(cfg.ExpansionProbability.ToString("G"));
        
    if (inflowPerCellInput != null)
        inflowPerCellInput.SetTextWithoutNotify(cfg.InflowPerCell.ToString("G"));
        
    if (diffusionRateInput != null)
        diffusionRateInput.SetTextWithoutNotify(cfg.DiffusionRate.ToString("G"));
}
```

#### C. **TopologyDetailsSection.cs**

```csharp
/// <summary>
/// Refresh UI controls from WorkingScenarioConfig.
/// </summary>
public void Refresh(WorkingScenarioConfig cfg)
{
    if (orchestrator != null && orchestrator.IsRefreshing()) 
        return;
    
    // Set topology/mask controls
    if (maskShapeDropdown != null)
        maskShapeDropdown.SetValueWithoutNotify((int)cfg.MaskType);
        
    if (maskRadiusInput != null)
        maskRadiusInput.SetTextWithoutNotify(cfg.MaskRadiusOuter.ToString("F1"));
        
    if (maskInnerRadiusInput != null)
        maskInnerRadiusInput.SetTextWithoutNotify(cfg.MaskRadiusInner.ToString("F1"));
        
    if (corridorWidthInput != null)
        corridorWidthInput.SetTextWithoutNotify(cfg.MaskCorridorWidth.ToString("F1"));
        
    if (holeProbabilityInput != null)
        holeProbabilityInput.SetTextWithoutNotify(cfg.MaskPercolationProbability.ToString("F2"));
}
```

#### D. **InflowDetailsSection.cs**

```csharp
/// <summary>
/// Refresh UI controls from WorkingScenarioConfig.
/// </summary>
public void Refresh(WorkingScenarioConfig cfg)
{
    if (orchestrator != null && orchestrator.IsRefreshing()) 
        return;
    
    // Update point sources display
    // If you have a method to display point sources list:
    UpdatePointSourcesDisplay(cfg.PointSources);
    
    // Or if you just show count:
    if (pointSourceCountLabel != null)
        pointSourceCountLabel.text = $"Point Sources: {cfg.PointSources.Count}";
}

private void UpdatePointSourcesDisplay(List<PointSourceData> sources)
{
    // Update your UI to show the sources
    // This depends on how you're displaying them (list, text, etc.)
}
```

#### E. **DiffusionDetailsSection.cs**

```csharp
/// <summary>
/// Refresh UI controls from WorkingScenarioConfig.
/// </summary>
public void Refresh(WorkingScenarioConfig cfg)
{
    if (orchestrator != null && orchestrator.IsRefreshing()) 
        return;
    
    // Set anisotropic diffusion controls
    if (anisotropicDirectionDropdown != null)
        anisotropicDirectionDropdown.SetValueWithoutNotify((int)cfg.AnisotropicDirection);
        
    if (anisotropicBiasSlider != null)
        anisotropicBiasSlider.SetValueWithoutNotify(cfg.AnisotropicBias);
        
    if (anisotropicBiasLabel != null)
        anisotropicBiasLabel.text = cfg.AnisotropicBias.ToString("F2");
}
```

#### F. **ViabilityDetailsSection.cs**

```csharp
/// <summary>
/// Refresh UI controls from WorkingScenarioConfig.
/// </summary>
public void Refresh(WorkingScenarioConfig cfg)
{
    if (orchestrator != null && orchestrator.IsRefreshing()) 
        return;
    
    // Set hysteresis threshold controls
    if (onThresholdInput != null)
        onThresholdInput.SetTextWithoutNotify(cfg.HysteresisOnThreshold.ToString("F2"));
        
    if (offThresholdInput != null)
        offThresholdInput.SetTextWithoutNotify(cfg.HysteresisOffThreshold.ToString("F2"));
}
```

### Step 3: Update Event Handlers (All Controllers)

**Pattern:** Event handlers should write to `orchestrator.WorkingConfig` instead of storing locally.

**Example** (MechanismsSection.cs):

```csharp
// OLD (if you had local storage):
void OnInflowModeChanged(int index)
{
    currentInflowMode = (InflowMode)index;  // ? Don't store locally
}

// NEW (write to orchestrator):
void OnInflowModeChanged(int index)
{
    if (orchestrator == null) return;
    orchestrator.WorkingConfig.Inflow = (InflowMode)index;  // ? Write to single source of truth
}
```

**Apply this pattern to ALL event handlers:**
- Dropdown `OnValueChanged` events
- Input field `OnEndEdit` events
- Slider `OnValueChanged` events
- Button click events that modify configuration

---

## ?? Unity Scene Setup

After code changes, you need to set up the orchestrator in Unity:

### 1. Create Orchestrator GameObject

1. In Unity Hierarchy, right-click ? Create Empty
2. Name it: `SimulationUIOrchestrator`
3. Add Component ? `SimulationUIOrchestrator` script

### 2. Wire Inspector Fields

Select the `SimulationUIOrchestrator` GameObject:

**Required Fields:**
- **Simulation Controller:** Drag the `SimulationController` GameObject
- **Default Preset:** Drag your internal demo preset from Project window

**Auto-Wired (but verify):**
- TopBarUI
- MechanismsSection
- CoreParametersSection
- TopologyDetailsSection
- InflowDetailsSection
- DiffusionDetailsSection
- ViabilityDetailsSection

*(These auto-wire in Awake if not set in Inspector)*

### 3. Wire Apply Button

**In TopBarUI (or wherever your Apply button is):**

1. Select the Apply button GameObject
2. In Inspector, find `Button` component
3. Under `OnClick()`:
   - Click `+` to add event
   - Drag `SimulationUIOrchestrator` GameObject to the object field
   - Select: `SimulationUIOrchestrator.ApplyAndRestart()`

### 4. Wire Preset Dropdown

**In PresetSelectorUI:**

1. Find your preset dropdown's `OnValueChanged` event
2. Wire it to: `SimulationUIOrchestrator.LoadPreset(ScenarioPreset)`

---

## ?? Testing After Implementation

### Test 1: Preset Load (Default Behavior)
```
1. Press Play in Unity
2. Preset dropdown should show internal demo preset
3. Click "Apply" button
4. Simulation should behave exactly as before
? PASS if default demo works unchanged
```

### Test 2: Mechanism Change (InflowMode)
```
1. In MechanismsSection, set Inflow = "Point Sources"
2. Click "Edit Sources" button
3. Add 2-3 point sources
4. Click "Apply"
5. Simulation should form islands near sources
? PASS if point source behavior is different from default
```

### Test 3: Viability Change (Hysteresis)
```
1. In MechanismsSection, set Viability Rule = "Hysteresis"
2. In ViabilityDetailsSection:
   - Set ON Threshold = 0.5
   - Set OFF Threshold = -0.5
3. Click "Apply"
4. Simulation should show delayed collapse (persistence)
? PASS if cells persist longer before dying
```

### Test 4: Boundary Change (Periodic Wrap)
```
1. In MechanismsSection, set Boundary = "Wrap"
2. Click "Apply"
3. Simulation should wrap at edges (no boundary death)
? PASS if edge behavior changes
```

### Test 5: Persistence Across Panels
```
1. In Setup mode, change a value (e.g., decay loss)
2. Switch to Inspect mode
3. Switch back to Setup mode
4. Value should still be there
? PASS if values persist
```

---

## ?? Troubleshooting

### Error: "orchestrator is null"
**Fix:** Make sure orchestrator GameObject exists in scene and is wired in Inspector

### Error: "Refresh method not found"
**Fix:** You haven't added the Refresh method to that controller yet

### UI doesn't update after preset load
**Fix:** Check that Refresh method is calling `SetValueWithoutNotify()` correctly

### Apply button does nothing
**Fix:** Check that button's OnClick is wired to `orchestrator.ApplyAndRestart()`

### Values revert after switching modes
**Fix:** Make sure event handlers write to `orchestrator.WorkingConfig`, not local storage

### Event loop / flickering UI
**Fix:** Ensure Refresh method checks `orchestrator.IsRefreshing()` at the start

---

## ? Implementation Complete When:

- [ ] All 6 UI controllers have Refresh methods
- [ ] All controllers have orchestrator reference
- [ ] All event handlers write to orchestrator.WorkingConfig
- [ ] Orchestrator GameObject created in scene
- [ ] Apply button wired to orchestrator
- [ ] Preset dropdown wired to orchestrator
- [ ] Build succeeds (no errors)
- [ ] All 5 test scenarios pass

---

## ?? Summary

**What You're Adding:**
1. **27 lines per controller** (orchestrator ref + Refresh method)
2. **Update event handlers** to write to orchestrator.WorkingConfig
3. **5 minutes of Unity scene setup** (create GameObject, wire buttons)

**What You Get:**
- ? UI changes actually affect the simulation
- ? Preset system fully functional
- ? Apply & Restart workflow working
- ? Single source of truth (no config drift)
- ? Event loop prevention built-in
- ? Default demo preserved

**Estimated Time:** 30-60 minutes (depending on your UI code familiarity)

---

**The core architecture is solid and complete. This is just the final wiring step!** ??
