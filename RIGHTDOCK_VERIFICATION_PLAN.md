# ?? RightDock Components - Systematic Verification Plan

## ?? **Overview**

This document provides a systematic verification methodology for all RightDock UI components, ensuring they:
1. ? **Function correctly** (UI interaction works)
2. ? **Actually affect simulation** (not just cosmetic)
3. ? **Changes are observable** (measurable differences)
4. ? **Match theoretical expectations** (documented behavior)

---

## ?? **Verification Methodology**

For EACH component, we will:

**i.** Identify the component we are testing  
**ii.** Note where it is positioned within the code/interface  
**iii.** Detail what it is meant to be doing (intent + theory)  
**iv.** Predict what we expect with NO changes  
**v.** Predict what we expect WITH changes  
**vi.** Run sim with NO changes ? Record results  
**vii.** Run sim WITH changes ? Record results  
**viii.** Compare data ? Verify predictions  
**ix.** Determine if criteria met  

---

## ?? **RightDock Structure**

```
RightDock
?? DockModeController (dropdown panel switcher)
?
?? Setup Panel (3 sections)
?  ?? Mechanism Dropdowns
?  ?? Core Parameters
?  ?? Region/Sink Settings
?
?? Inspect Panel
?  ?? (Real-time simulation metrics)
?
?? Export Panel
   ?? (Export configuration)
```

---

## ?? **Components to Test**

### **Category A: Panel Switching**
1. Mode Dropdown (Setup/Inspect/Export)

### **Category B: Mechanism Dropdowns (Setup Panel)**
2. Inflow Mode Dropdown
3. Boundary Mode Dropdown
4. Diffusion Mode Dropdown
5. Viability Rule Dropdown

### **Category C: Core Parameters (Setup Panel)**
6. Decay Loss Slider
7. Resource Global Max Slider
8. Global Replenish Rate Slider
9. E-Threshold Base Slider

### **Category D: Region/Sink Settings (Setup Panel)**
10. Region Expansion Chance Slider
11. Sink Formation Threshold Slider

### **Category E: Inspect Panel**
12. Real-time Metrics Display

---

# ?? **COMPONENT VERIFICATIONS**

---

## **Component 1: Mode Dropdown (Panel Switcher)**

### **i. Component Identification**
- **Name:** Mode Dropdown (DockModeController)
- **File:** `Assets/Viable/Core.Unity/UI/DockModeController.cs`
- **Unity Hierarchy:** `Canvas/RightDock/ModeDropdown`

### **ii. Position in Code/Interface**
```csharp
// DockModeController.cs
private void OnModeChanged(int modeIndex)
{
    // Hide all panels
    setupPanel.SetActive(false);
    inspectPanel.SetActive(false);
    exportPanel.SetActive(false);
    
    // Show selected panel
    switch (modeIndex) { ... }
}
```

**UI Position:** Top of RightDock, dropdown with options: Setup / Inspect / Export

### **iii. Intent & Theory**
**What it does:**
- Switches between 3 panels: Setup, Inspect, Export
- **Only changes visibility** - does NOT modify simulation
- Pure UI navigation component

**Theoretical Justification:**
- Separates configuration (Setup) from observation (Inspect) from export (Export)
- Reduces UI clutter by showing only relevant controls
- Follows "mode-based interface" design pattern

### **iv. Prediction: NO Changes**
**Baseline Behavior:**
- Dropdown starts with "Setup" selected
- Setup panel visible, Inspect/Export hidden
- Clicking dropdown shows 3 options
- Selecting option switches visible panel

**Expected Result:** Panel visibility changes, but simulation unaffected.

### **v. Prediction: WITH Changes**
**Test Actions:**
1. Switch to "Inspect" ? Should show Inspect panel
2. Switch to "Export" ? Should show Export panel
3. Switch back to "Setup" ? Should show Setup panel

**Expected Result:** 
- Only visible panel changes
- No simulation reset
- No parameter changes
- Dropdown auto-closes after selection (if CloseDropdownDelayed implemented)

### **vi. Test: NO Changes (Baseline)**

**Steps:**
1. Press Play
2. Note initial panel (should be Setup)
3. Check if Inspect/Export hidden

**Record Results:**
- [ ] Dropdown shows "Setup" on startup
- [ ] Setup panel visible
- [ ] Inspect/Export panels hidden
- [ ] Simulation runs normally

**Console Expected:**
```
[DockModeController] Switched to Setup panel
```

### **vii. Test: WITH Changes**

**Steps:**
1. Press Play
2. Select "Inspect" from dropdown
3. Verify Inspect panel shows, Setup hidden
4. Select "Export"
5. Verify Export panel shows, Inspect hidden
6. Select "Setup" again
7. Verify Setup panel shows, Export hidden

**Record Results:**
- [ ] Dropdown selection updates visible panel
- [ ] Only 1 panel visible at a time
- [ ] Panel switching is instant (no delay)
- [ ] Simulation continues running (not affected)

**Console Expected:**
```
[DockModeController] Switched to Inspect panel
[DockModeController] Switched to Export panel
[DockModeController] Switched to Setup panel
```

### **viii. Compare & Verify**

**Comparison:**
| Aspect | Baseline | With Changes | Match? |
|--------|----------|--------------|--------|
| Panel Visibility | Setup | Changes per selection | ? |
| Simulation State | Running | Running | ? |
| Dropdown Closes | - | Auto-close | ? / ? |

**Verification Questions:**
- [ ] Does panel switch instantly?
- [ ] Does only 1 panel show at a time?
- [ ] Does simulation continue unaffected?
- [ ] Does dropdown auto-close (if implemented)?

### **ix. Criteria Assessment**

**Pass Criteria:**
- ? Panel switching works instantly
- ? Only 1 panel visible at a time
- ? Simulation unaffected by panel switches
- ? No errors in Console

**Result:** PASS / FAIL / PARTIAL

**Notes:**
- If dropdown doesn't auto-close, add `CloseDropdownDelayed()` coroutine
- If panels overlap, check `SetActive(false)` calls execute before `SetActive(true)`

---

## **Component 2: Inflow Mode Dropdown**

### **i. Component Identification**
- **Name:** Inflow Mode Dropdown
- **File:** (To be located in Setup Panel)
- **Unity Hierarchy:** `Canvas/RightDock/SetupPanel/MechanismSection/InflowModeDropdown`

### **ii. Position in Code/Interface**
**Expected Structure:**
```csharp
// In Setup Panel or mechanism controller
private TMP_Dropdown inflowModeDropdown;

private void OnInflowModeChanged(int index)
{
    workingConfig.InflowMode = (InflowMode)index;
    // Options: Uniform, PointSources, Gradient, etc.
}
```

**UI Position:** Setup Panel ? Mechanism Section ? First dropdown

### **iii. Intent & Theory**

**What it does:**
- Controls **how resource enters the simulation**
- Changes global resource distribution pattern

**Theoretical Justification:**

**Inflow Modes:**

| Mode | Theory | Biological Analogy |
|------|--------|-------------------|
| **Uniform** | Resource distributed evenly across grid | Uniform nutrient bath |
| **Point Sources** | Resource from specific locations | Hydrothermal vents, food sources |
| **Gradient** | Resource varies spatially | Chemical gradient, light gradient |
| **Boundary** | Resource enters from edges | Ocean surface, atmospheric exchange |

**Key Parameter:** `EngineConfig.InflowMode`

**Affects:**
- Spatial resource distribution
- Expansion patterns (follow resource flow)
- Viability topology (regions form near inflow)

### **iv. Prediction: NO Changes (Uniform)**

**Baseline (Default = Uniform):**
- Resource distributed evenly every tick
- `GlobalReplenishPerTick` divided equally across all cells
- Expansion is **radially symmetric** from seed
- No preferred direction

**Expected Visual:**
- Circular expansion from center
- Uniform yellow/green gradient
- Symmetric growth pattern

**Expected Metrics:**
- ViableCount increases uniformly
- ResourceGlobal stays relatively constant
- No spatial bias in expansion

### **v. Prediction: WITH Changes (Point Sources)**

**Test Configuration:**
- Switch to **PointSources** mode
- Place 2-3 point sources at corners of grid
- Same total resource, but concentrated

**Expected Changes:**

**Visual:**
- Expansion towards point sources
- Multiple separate regions form
- Asymmetric growth pattern
- Resource "hotspots" visible

**Metrics:**
- ViableCount similar, but spatial distribution different
- Resource concentrated at sources
- Regions form along paths to sources

**Physics Explanation:**
- Cells near sources get more resource
- Higher viability near sources
- Expansion follows resource gradient
- Competing regions form between sources

### **vi. Test: NO Changes (Uniform Baseline)**

**Steps:**
1. Load "Default" preset (uses Uniform inflow)
2. Note Inflow Mode dropdown shows "Uniform"
3. Click Play ? Run 100 ticks
4. Observe expansion pattern
5. Export results

**Record Results:**

**Visual Observations:**
- Expansion shape: Circular / Asymmetric
- Frontier pattern: Uniform / Patchy
- Sink distribution: Random / Clustered

**Metrics (from Export CSV):**
- Tick 0: ViableCount = ___, ResourceGlobal = ___
- Tick 50: ViableCount = ___, ResourceGlobal = ___
- Tick 100: ViableCount = ___, ResourceGlobal = ___

**Screenshot:** Save image at Tick 100

### **vii. Test: WITH Changes (Point Sources)**

**Steps:**
1. **IMPORTANT:** Do NOT reload preset (starts fresh)
2. Setup Panel ? Inflow Mode ? Select "Point Sources"
3. (If point source UI exists) Place 3 sources at corners
4. Click "Apply & Restart" (or Load preset if needed)
5. Click Play ? Run 100 ticks
6. Observe expansion pattern
7. Export results

**Record Results:**

**Visual Observations:**
- Expansion shape: Multiple regions / Single blob
- Frontier pattern: Directed towards sources / Uniform
- Sink distribution: Near sources / Random

**Metrics (from Export CSV):**
- Tick 0: ViableCount = ___, ResourceGlobal = ___
- Tick 50: ViableCount = ___, ResourceGlobal = ___
- Tick 100: ViableCount = ___, ResourceGlobal = ___

**Screenshot:** Save image at Tick 100

### **viii. Compare & Verify**

**Visual Comparison:**
| Aspect | Uniform | Point Sources | Different? |
|--------|---------|---------------|------------|
| Expansion Shape | Circular | Multiple regions | ? / ? |
| Frontier Pattern | Uniform gradient | Directed growth | ? / ? |
| Sink Distribution | Random | Near sources | ? / ? |

**Metrics Comparison:**
| Metric | Uniform (T100) | Point Sources (T100) | Difference |
|--------|----------------|----------------------|------------|
| ViableCount | ___ | ___ | ___ |
| ActiveCount | ___ | ___ | ___ |
| ResourceGlobal | ___ | ___ | ___ |

**Spatial Analysis:**
- Overlay screenshots side-by-side
- Measure expansion radius from center
- Count distinct regions
- Identify asymmetry

### **ix. Criteria Assessment**

**Pass Criteria:**
- ? Inflow Mode dropdown changes value
- ? Visual expansion pattern IS DIFFERENT
- ? Point Sources create asymmetric growth
- ? Multiple regions form near sources
- ? Metrics show spatial differences

**Failure Modes:**

**? Patterns Look Identical:**
- Bug: InflowMode not passed to engine
- Fix: Check `SimulationController.RestartWithScenario()` uses `EngineConfig.InflowMode`

**? Point Sources Not Visible:**
- Bug: Point source positions not set
- Fix: Check `EngineConfig.PointSources` array populated

**? Dropdown Changes But Simulation Doesn't:**
- Bug: WorkingScenarioConfig ? ScenarioDefinition conversion missing InflowMode
- Fix: Check `ScenarioPresetAdapter.ToScenarioDefinition()` includes EngineConfig

**Result:** PASS / FAIL / PARTIAL

**Notes:**
- If point source UI doesn't exist yet, manually edit preset file to test
- Compare exported CSVs using Python/Excel to quantify differences
- Take screenshots at same tick for fair comparison

---

## **Component 3: Boundary Mode Dropdown**

### **i. Component Identification**
- **Name:** Boundary Mode Dropdown
- **File:** (To be located in Setup Panel)
- **Unity Hierarchy:** `Canvas/RightDock/SetupPanel/MechanismSection/BoundaryModeDropdown`

### **ii. Position in Code/Interface**
```csharp
private TMP_Dropdown boundaryModeDropdown;

private void OnBoundaryModeChanged(int index)
{
    workingConfig.BoundaryMode = (BoundaryMode)index;
    // Options: Reflective, Absorbing, Wrap, etc.
}
```

### **iii. Intent & Theory**

**What it does:**
- Controls **what happens at grid edges**
- Affects expansion topology and resource flow

**Boundary Modes:**

| Mode | Behavior | Physical Analogy |
|------|----------|------------------|
| **Reflective** | Waves bounce back | Wall, mirror |
| **Absorbing** | Resources/cells disappear at edge | Open boundary, cliff |
| **Wrap (Toroidal)** | Edges connect (torus topology) | Video game wraparound, periodic boundary |
| **Sticky** | Cells stick to boundary | Adhesive surface |

**Theoretical Impact:**

**Reflective:**
- Expansion hits wall, bounces back
- Resource accumulates at edges
- Higher density near boundaries

**Absorbing:**
- Expansion dies at edges
- Resource lost at boundaries
- Lower density near edges

**Wrap:**
- Grid is actually a torus (donut shape)
- No true "edge" - seamless wraparound
- Symmetric expansion in all directions
- Fair competition (no edge effects)

### **iv. Prediction: NO Changes (Default)**

**Baseline (likely Reflective or Absorbing):**
- Expansion stops at grid edges
- Edge cells have fewer neighbors (boundary effect)
- Asymmetric viability near edges

**Expected Visual:**
- Expansion stops at boundaries
- Edge regions different behavior
- Corner cells most constrained

### **v. Prediction: WITH Changes (Wrap)**

**Test Configuration:**
- Switch to **Wrap (Toroidal)** mode
- Same seed, same parameters

**Expected Changes:**

**Visual:**
- Expansion can cross edges (appears on opposite side)
- No "wall" effect
- Perfectly symmetric expansion

**Metrics:**
- Higher ViableCount (no boundary loss)
- Uniform edge density
- Symmetric patterns

**Physics:**
- Edge cells have full 4 neighbors (wraps to opposite edge)
- No boundary penalty
- True periodic boundary conditions

### **vi-ix:** [Same structure as Component 2 - test with/without, compare, assess]

---

## **Component 4-11:** [Continue with same format for each component]

---

# ?? **SUMMARY SCORECARD**

## **Component Verification Status**

| Component | Tested | Affects Sim | Matches Theory | Pass/Fail |
|-----------|--------|-------------|----------------|-----------|
| 1. Mode Dropdown | [ ] | N/A (UI only) | ? | ___ |
| 2. Inflow Mode | [ ] | ? / ? | ? / ? | ___ |
| 3. Boundary Mode | [ ] | ? / ? | ? / ? | ___ |
| 4. Diffusion Mode | [ ] | ? / ? | ? / ? | ___ |
| 5. Viability Rule | [ ] | ? / ? | ? / ? | ___ |
| 6. Decay Loss | [ ] | ? / ? | ? / ? | ___ |
| 7. Resource Global Max | [ ] | ? / ? | ? / ? | ___ |
| 8. Global Replenish | [ ] | ? / ? | ? / ? | ___ |
| 9. E-Threshold Base | [ ] | ? / ? | ? / ? | ___ |
| 10. Region Expansion | [ ] | ? / ? | ? / ? | ___ |
| 11. Sink Formation | [ ] | ? / ? | ? / ? | ___ |
| 12. Inspect Panel | [ ] | N/A (display) | ? / ? | ___ |

---

## **Overall Assessment**

**Total Components:** 12  
**UI-Only (no sim impact expected):** 2  
**Sim-Affecting (must show differences):** 10

**Scores:**
- **Tested:** ___ / 12
- **Affects Simulation:** ___ / 10
- **Matches Theory:** ___ / 12
- **Pass:** ___ / 12

**Status:**
- ? **PASS (10-12):** RightDock fully functional
- ?? **NEEDS WORK (6-9):** Some components not wired
- ? **FAIL (< 6):** Major issues, RightDock cosmetic only

---

## ?? **Common Issues & Fixes**

### **Issue: Dropdown Changes But Simulation Unchanged**

**Symptoms:**
- Dropdown selection changes
- No visual difference in simulation
- Metrics identical

**Diagnosis:**
1. Check if `workingConfig` updated
2. Check if "Apply & Restart" calls `RestartWithScenario()`
3. Check if `ScenarioDefinition` includes changed parameter
4. Check if engine actually uses parameter

**Fix Chain:**
```
UI Dropdown
  ? onValueChanged
workingConfig.InflowMode = newValue
  ? "Apply & Restart" button
ScenarioPresetAdapter.ToScenarioDefinition(workingConfig)
  ? includes EngineConfig.InflowMode
SimulationController.RestartWithScenario(scenarioDef)
  ? BuildPhasePipeline(phaseSetId)
SimulationStepper uses EngineConfig.InflowMode
  ?
ACTUAL BEHAVIOR CHANGES
```

**If any link breaks, dropdown is cosmetic!**

---

### **Issue: "Apply & Restart" Button Missing**

**Symptoms:**
- Can change dropdowns
- No way to apply changes

**Fix:**
- TopBarUI has "Apply & Restart" button
- Currently TODO in `OnApplyAndRestart()`
- Need to implement conversion: `WorkingScenarioConfig` ? `ScenarioDefinition` ? `RestartWithScenario()`

---

### **Issue: Changes Reset When Switching Panels**

**Symptoms:**
- Change parameter in Setup
- Switch to Inspect
- Switch back to Setup
- Parameter reset to default

**Fix:**
- Ensure WorkingScenarioConfig persists across panel switches
- UI should read from config, not reset it
- `DockModeController.OnModeChanged()` should NOT rebuild panels

---

## ?? **Testing Workflow**

### **Quick Verification (30 minutes)**

Test 2-3 most critical components:
1. Inflow Mode (most dramatic visual difference)
2. Decay Loss (easy to quantify)
3. Boundary Mode (clear edge behavior)

### **Full Verification (2-3 hours)**

Test all 12 components systematically:
- 1 component = ~15 minutes
- Run baseline, run test, compare, document

### **Automated Verification (Future)**

Create Python script:
1. Load baseline CSV
2. Load test CSV
3. Compute difference metrics
4. Generate comparison report
5. Auto-detect if parameters had effect

---

## ?? **Documentation Template**

For each component, fill out:

```markdown
## Component X: [Name]

### Test Date: YYYY-MM-DD
### Tester: [Your Name]

### Baseline Run
- Configuration: [preset/parameters]
- Visual: [description + screenshot]
- Metrics: [CSV data]

### Test Run
- Changed: [parameter] from [X] to [Y]
- Visual: [description + screenshot]
- Metrics: [CSV data]

### Comparison
- Visual Difference: Yes / No / Partial
- Metrics Difference: [quantified]
- Matches Theory: Yes / No

### Result: PASS / FAIL
### Notes: [any issues, bugs, observations]
```

---

**Created:** 2024  
**Status:** ?? Ready for RightDock Verification  
**Use:** Systematically verify each RightDock component affects simulation
