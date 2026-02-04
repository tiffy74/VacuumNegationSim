# ?? Topology Dropdown - Complete Verification

## Component: Topology Mode Dropdown

**Test Date:** [To be filled]  
**Tester:** [To be filled]  
**Category:** Mechanism Dropdown (Setup Panel)

---

## **i. Component Identification**

- **Name:** Topology Mode Dropdown
- **File:** (To be located in Setup Panel mechanism section)
- **Unity Hierarchy:** `Canvas/RightDock/SetupPanel/MechanismSection/TopologyDropdown`
- **Enum:** `Viable.Contracts.TopologyMode`
- **EngineConfig Property:** `EngineConfig.TopologyMode`

---

## **ii. Position in Code/Interface**

### **Expected Code Structure:**
```csharp
// In Setup Panel or Mechanism Controller
private TMP_Dropdown topologyDropdown;

private void OnTopologyChanged(int index)
{
    workingConfig.TopologyMode = (TopologyMode)index;
    // Options: 
    //   0 = RectGrid (Full Domain)
    //   1 = MaskedDomain (Masked Domain)
}
```

### **UI Position:**
- **Location:** RightDock ? Setup Panel ? Mechanisms Section
- **Label:** "Topology" or "Domain Type"
- **Dropdown Options (2 total):**
  1. **Full Domain** (maps to `TopologyMode.RectGrid`)
  2. **Masked Domain** (maps to `TopologyMode.MaskedDomain`)

### **Enum Definition:**
```csharp
public enum TopologyMode
{
    RectGrid = 0,         // Full Domain: complete rectangular grid
    MaskedDomain = 1,     // Masked Domain: shaped/constrained domain
    HexGrid = 2,          // Future: hexagonal grid (not in dropdown yet)
    GraphDomain = 3       // Future: graph-based domain (not in dropdown yet)
}
```

**Note:** Only first 2 enum values are exposed in UI dropdown. HexGrid and GraphDomain are future features (Stage 13.8+).

---

## **iii. Intent & Theory**

### **What It Does:**
Controls **the shape and connectivity of the simulation domain** - determines which cells exist and how they connect to neighbors.

### **Theoretical Justification:**

#### **Grid Topology in Spatial Simulations**

**Core Concept:**
- Topology defines the "shape of space" the simulation lives on
- Affects:
  - **Cell connectivity** (which cells are neighbors)
  - **Diffusion patterns** (how resources spread)
  - **Expansion topology** (how regions grow)
  - **Boundary conditions** (what happens at edges)

---

### **Topology Modes (Currently Implemented):**

| Mode | UI Label | Description | Physical Analogy | Status |
|------|----------|-------------|------------------|--------|
| **RectGrid** | **Full Domain** | Standard rectangular grid | Petri dish, flat surface | ? Default |
| **MaskedDomain** | **Masked Domain** | Subset of rect grid (shaped domain) | Island, confined channel | ? Implemented |
| ~~HexGrid~~ | ~~(not in UI)~~ | Hexagonal tiling | Honeycomb, tissue | ?? Future (Stage 13.8) |
| ~~GraphDomain~~ | ~~(not in UI)~~ | Arbitrary connectivity graph | Network | ?? Future (Stage 13.9) |

---

### **Theoretical Impact by Mode:**

#### **1. Full Domain (RectGrid)**

**Theory:**
- **Von Neumann neighborhood:** 4 neighbors (N, S, E, W)
- **Manhattan distance:** \|x1-x2\| + \|y1-y2\|
- **Diffusion:** Axis-aligned spread
- **Expansion:** Square/diamond patterns
- **All cells active:** No constraints, full grid available

**When to Use:**
- Standard simulations
- Baseline comparisons
- Well-understood topology
- Maximum spatial extent

**Example Systems:**
- Cellular automata (Game of Life)
- Reaction-diffusion systems
- Standard grid models

**UI Expectations:**
- Label: "Full Domain"
- No additional controls needed
- Simple, straightforward option

---

#### **2. Masked Domain**

**Theory:**
- **Rectangular grid with holes/obstacles**
- Some cells are "inactive" (masked out)
- Defines **complex boundaries** within grid
- Creates **constrained environments**

**Mask Shapes (via `EngineConfig.MaskShape`):**

| Shape | Description | Use Case |
|-------|-------------|----------|
| **Rectangle** | Full grid (no mask) | Default (same as Full Domain) |
| **Circle** | Circular domain | Droplet, colony |
| **Ring** | Annular region | Vortex ring, donut |
| **Corridor** | Narrow channel | Diffusion in tubes |
| **PercolationHoles** | Random obstacles | Porous medium |

**When to Use:**
- **Confined geometries** (biological droplets, microfluidic channels)
- **Obstacle avoidance** (growth around barriers)
- **Percolation studies** (connectivity through random media)
- **Shaped domains** (circular, annular, corridor)

**Examples:**
- Bacterial growth in petri dish (circle)
- Diffusion through porous rock (percolation holes)
- Flow in narrow channels (corridor)

**UI Expectations:**
- Label: "Masked Domain"
- Additional controls appear when selected:
  - **Mask Shape** dropdown (Circle, Ring, Corridor, etc.)
  - **Mask Radius** slider/input
  - **Inner Radius** (for Ring shape)
  - **Corridor Width** (for Corridor shape)

---

## **iv. Prediction: NO Changes (Full Domain Baseline)**

### **Test Configuration:**
- **Topology:** Full Domain
- **Grid Size:** 64×64
- **Preset:** Default
- **Run Duration:** 100 ticks

### **Expected Visual:**
- **Expansion shape:** Square/circular from center seed
- **Frontier:** Smooth, continuous
- **Domain:** All 4096 cells (64×64) potentially active
- **Boundary:** Rectangular grid edges at (0,0) to (63,63)

### **Expected Metrics:**
- **ViableCount:** Standard growth curve, can reach ~4096
- **ActiveCount:** Follows viable count
- **Expansion rate:** ~4 cells/tick radially (Von Neumann)

### **Screenshot Expectations:**
- Grid clearly shows rectangular cells
- Expansion symmetric in cardinal directions (N, S, E, W)
- Slight axis bias (faster along axes than diagonals due to Manhattan distance)
- **No masked cells** - all cells can potentially become active

---

## **v. Prediction: WITH Changes (Masked Domain - Circle)**

### **Test Configuration:**
- **Topology:** Masked Domain
- **MaskShape:** Circle
- **MaskRadius:** 25 cells (half grid width)
- **Grid Size:** 64×64
- **Preset:** Default (otherwise same parameters)
- **Run Duration:** 100 ticks

### **Expected Changes:**

#### **Visual:**
- **Domain shape:** Circular boundary (not rectangular)
- **Expansion:** Radial spread until hitting circle edge
- **Edge behavior:** Growth stops at circular boundary
- **Masked cells:** Dark/inactive outside circle

#### **Metrics:**
- **ViableCount:** Lower max (fewer cells in circle than full grid)
- **Max viable cells:** ~? × 25² ? 1963 (vs 4096 for full grid)
- **Expansion rate:** Same initially, stops at boundary

#### **Physics:**
- Cells outside circle are **permanently inactive**
- Diffusion cannot cross mask boundary
- Expansion constrained to circular region
- Creates **finite domain** effect

### **Theoretical Differences:**

| Aspect | Full Domain | Masked Domain (Circle) |
|--------|-------------|------------------------|
| **Max cells** | 64 × 64 = 4096 | ? × 25² ? 1963 |
| **Boundary shape** | Rectangle | Circle |
| **Edge effects** | Corner cells | Uniform boundary |
| **Expansion limit** | Grid edges | Circle radius |
| **Spatial symmetry** | Rectangular | Radial |
| **Masked cells** | None (all active) | ~2133 cells masked out |

---

## **vi. Test: NO Changes (Full Domain Baseline)**

### **Steps:**
1. **Setup:**
   - Press Play in Unity Editor
   - RightDock ? Setup Panel
   - Mechanism Section ? Topology Dropdown
   - Verify shows "Full Domain" selected

2. **Load Preset:**
   - TopBar ? Preset Dropdown ? "Default"
   - Click "Load" button
   - Verify Console: `[TopBarUI] Loaded preset: Default`

3. **Check Configuration:**
   - Topology Dropdown still shows "Full Domain"
   - **No additional mask controls visible** (mask controls hidden for Full Domain)

4. **Run Simulation:**
   - Click "Play" button
   - Let run to 100 ticks
   - Observe expansion pattern

5. **Export:**
   - Click "Export" button
   - Note export path from Console

### **Record Results:**

#### **Visual Observations:**
- **Expansion shape:** [Circle / Square / Diamond / Irregular]
- **Domain boundary:** [Rectangular / Circular / Other]
- **Frontier smoothness:** [Smooth / Jagged / Patchy]
- **Cell count at T100:** [___]
- **Masked cells visible:** [Yes / No] (should be NO)

#### **Metrics (from Export CSV):**
```
Tick 0:   ViableCount = ___, ActiveCount = ___, ResourceGlobal = ___
Tick 50:  ViableCount = ___, ActiveCount = ___, ResourceGlobal = ___
Tick 100: ViableCount = ___, ActiveCount = ___, ResourceGlobal = ___
```

#### **Screenshot:**
- **File:** `T100_FullDomain_Baseline.png`
- **Notes:** [Any observations about grid shape, expansion pattern]

#### **Console Logs:**
```
[Expected logs about initialization, topology mode]
Look for: TopologyMode = RectGrid (or 0)
```

---

## **vii. Test: WITH Changes (Masked Domain - Circle)**

### **Steps:**
1. **Setup:**
   - **IMPORTANT:** Restart Unity Editor scene (or click Restart button)
   - RightDock ? Setup Panel ? Mechanism Section

2. **Change Topology:**
   - Topology Dropdown ? Select "Masked Domain"
   - **Expected:** Additional mask controls appear:
     - Mask Shape Dropdown
     - Mask Radius Slider (or input field)
     - (Possibly Inner Radius, Corridor Width depending on shape)

3. **Configure Mask:**
   - Mask Shape ? Select "Circle"
   - Mask Radius ? Set to 25 (half of 64×64 grid)
   
4. **Apply & Restart:**
   - Click "Apply & Restart" button (TopBar)
   - **OR** if not implemented: Load preset, then manually set topology
   - Verify simulation resets to tick 0

5. **Run Simulation:**
   - Click "Play"
   - Let run to 100 ticks
   - **Watch carefully:** Does expansion stop at circular boundary?

6. **Export:**
   - Click "Export"
   - Note export path

### **Record Results:**

#### **Visual Observations:**
- **Domain boundary:** [Circular / Still Rectangular / No visible change]
- **Expansion limit:** [Stops at circle / Reaches grid edges / Other]
- **Masked cells:** [Dark/inactive outside circle / All cells active]
- **Cell count at T100:** [___ (should be < 1963 if circle works)]

#### **Metrics (from Export CSV):**
```
Tick 0:   ViableCount = ___, ActiveCount = ___, ResourceGlobal = ___
Tick 50:  ViableCount = ___, ActiveCount = ___, ResourceGlobal = ___
Tick 100: ViableCount = ___, ActiveCount = ___, ResourceGlobal = ___
```

**Expected:** ViableCount at T100 should be **significantly lower** than baseline (< 1963 vs ~3000+)

#### **Screenshot:**
- **File:** `T100_MaskedDomain_Circle.png`
- **Notes:** [Is circular boundary visible? Does growth stop at boundary?]

#### **Console Logs:**
```
Look for:
[Expected] TopologyMode = MaskedDomain (or 1)
[Expected] MaskShape = Circle (or 1)
[Expected] MaskRadius = 25.0
```

---

## **viii. Compare & Verify**

### **Visual Comparison:**

| Aspect | Full Domain | Masked Domain (Circle) | Different? | Notes |
|--------|-------------|------------------------|------------|-------|
| **Domain shape** | Rectangle | Circle | ? / ? | |
| **Max expansion** | Grid edges (64×64) | Circle radius 25 | ? / ? | |
| **Visible boundary** | Grid border | Circular mask | ? / ? | |
| **Cell count T100** | ___ (~4096 max) | ___ (< 1963?) | ? / ? | |
| **Masked cells** | None visible | Outside circle masked | ? / ? | |

### **Metrics Comparison:**

| Metric | Full Domain (T100) | Masked Domain (T100) | Ratio | Expected Ratio |
|--------|--------------------|-----------------------|-------|----------------|
| ViableCount | ___ | ___ | ___ | ~0.48 (1963/4096) |
| ActiveCount | ___ | ___ | ___ | ~0.48 |
| ResourceGlobal | ___ | ___ | ___ | Similar (global pool) |

### **Spatial Analysis:**

**Side-by-Side Screenshots:**
1. Load both screenshots in image viewer
2. **Overlay if possible** (transparency)
3. **Measure:**
   - Expansion radius from center
   - Number of active cells at edges
   - Boundary shape (circular vs rectangular)

**Quantitative Check:**
- Count cells in final frame
- Full Domain: Should approach grid size (4096 max)
- Circle: Should not exceed ? × 25² ? 1963

---

### **Verification Questions:**

- [ ] **Q1:** Does dropdown show "Full Domain" and "Masked Domain" options (2 total)?
- [ ] **Q2:** When selecting "Masked Domain", do mask controls appear?
- [ ] **Q3:** When selecting "Full Domain", do mask controls HIDE?
- [ ] **Q4:** Is circular boundary VISIBLE in Masked Domain simulation?
- [ ] **Q5:** Does expansion STOP at circular boundary?
- [ ] **Q6:** Is ViableCount at T100 significantly LOWER with circle mask?
- [ ] **Q7:** Do screenshots show CLEAR visual difference?

---

## **ix. Criteria Assessment**

### **Pass Criteria:**

**UI Functionality:**
- ? Topology dropdown exists and is accessible
- ? Dropdown shows exactly 2 options: "Full Domain" and "Masked Domain"
- ? Selecting option updates `workingConfig.TopologyMode`
- ? Mask controls appear/disappear based on selection
- ? "Apply & Restart" button triggers simulation restart

**Simulation Impact:**
- ? Full Domain: Expansion reaches grid edges (~4096 cells max)
- ? Masked Domain (Circle): Expansion stops at circle boundary (< 1963 cells)
- ? **ViableCount ratio ? 0.48** (circle area / grid area)
- ? Visual boundary CLEARLY VISIBLE in screenshots

**Theory Match:**
- ? Circular mask creates circular domain (not rectangular)
- ? Cells outside mask are inactive/dark
- ? Diffusion cannot cross mask boundary
- ? Metrics match expected spatial constraints

---

### **Failure Modes & Diagnosis:**

#### **? Scenario 1: Dropdown Changes But Simulation Unchanged**

**Symptoms:**
- Both tests look identical
- ViableCount reaches ~4096 in both cases
- No circular boundary visible

**Diagnosis:**
- TopologyMode not passed to engine
- Mask not applied during initialization
- EngineConfig.TopologyMode ignored
- Full Domain and Masked Domain both map to RectGrid

**Debug Steps:**
```csharp
// In SimulationController.InitializeSimulation()
Debug.Log($"TopologyMode: {config.EngineConfig.TopologyMode}");
Debug.Log($"MaskShape: {config.EngineConfig.MaskShape}");
Debug.Log($"MaskRadius: {config.EngineConfig.MaskRadius}");

// In engine initialization
Debug.Log($"Applying mask: {maskApplied}");
Debug.Log($"Active cells after mask: {activeCellCount}");
```

**Fix:**
1. Check `ScenarioPresetAdapter.ToScenarioDefinition()` includes TopologyMode
2. Check engine actually applies mask during `InitStateInto()`
3. Check mask configuration propagates through pipeline
4. Verify dropdown index 1 maps to `TopologyMode.MaskedDomain`

---

#### **? Scenario 2: Mask Controls Don't Appear**

**Symptoms:**
- Select "Masked Domain" from dropdown
- No additional UI controls appear
- Cannot set MaskShape or MaskRadius

**Diagnosis:**
- Conditional UI not implemented
- Mask parameters hard-coded or missing
- UI toggle logic missing from OnTopologyChanged

**Fix:**
```csharp
// In Setup Panel mechanism controller
private void OnTopologyChanged(int index)
{
    // Map dropdown index to enum
    TopologyMode mode = (TopologyMode)index; // 0=RectGrid, 1=MaskedDomain
    workingConfig.TopologyMode = mode;
    
    // Show/hide mask controls based on topology
    if (mode == TopologyMode.MaskedDomain)
    {
        maskControlsPanel.SetActive(true);
        Debug.Log("[MechanismController] Showing mask controls");
    }
    else
    {
        maskControlsPanel.SetActive(false);
        Debug.Log("[MechanismController] Hiding mask controls");
    }
}
```

---

#### **? Scenario 3: Dropdown Shows Wrong Labels**

**Symptoms:**
- Dropdown shows "RectGrid" and "MaskedDomain" instead of "Full Domain" and "Masked Domain"
- Or shows 4 options (RectGrid, MaskedDomain, HexGrid, GraphDomain)

**Diagnosis:**
- Dropdown options not customized
- Using enum names directly instead of user-friendly labels

**Fix:**
```csharp
// In Setup Panel initialization
topologyDropdown.ClearOptions();
topologyDropdown.AddOptions(new List<string>
{
    "Full Domain",    // Index 0 ? TopologyMode.RectGrid
    "Masked Domain"   // Index 1 ? TopologyMode.MaskedDomain
});
topologyDropdown.value = 0; // Default to Full Domain
topologyDropdown.onValueChanged.AddListener(OnTopologyChanged);
```

---

#### **? Scenario 4: Partial Masking (Circular But Wrong Size)**

**Symptoms:**
- Circular boundary visible
- But wrong radius (too small or too large)
- ViableCount ratio ? 0.48

**Diagnosis:**
- MaskRadius not scaled correctly
- Grid coordinate system mismatch
- Mask center not at grid center

**Debug:**
```csharp
// Check mask application
Debug.Log($"Grid size: {gridWidth}x{gridHeight}");
Debug.Log($"Mask radius (cells): {maskRadius}");
Debug.Log($"Expected max cells: {Math.PI * maskRadius * maskRadius}");
Debug.Log($"Actual active cells: {activeCellCount}");
Debug.Log($"Mask center: ({centerX}, {centerY})");
```

**Fix:**
- Verify mask radius in grid coordinates (not world coordinates)
- Check mask center is grid center: `(gridWidth/2, gridHeight/2)`
- Ensure mask applied before simulation starts

---

### **Result Assessment:**

**Status:** PASS / FAIL / PARTIAL

**Score:**
- [ ] UI Dropdown Shows 2 Options (1 point)
- [ ] Mask Controls Toggle Correctly (1 point)
- [ ] Simulation Shows Visual Difference (2 points)
- [ ] Metrics Match Theory (ViableCount ratio ? 0.48) (2 points)

**Total:** ___ / 6 points

**PASS Threshold:** 5/6 points (allows minor UI issues but requires simulation impact)

---

### **Notes:**

**Observations:**
- [Any unexpected behaviors]
- [Performance impacts]
- [Visual artifacts]

**Issues Found:**
- [List any bugs discovered]
- [Missing features]
- [Configuration problems]

**Recommendations:**
- [Suggested improvements]
- [Additional tests needed]
- [Documentation updates]

---

## **x. Next Steps**

### **If Test PASSES:**
1. ? Document baseline and circle mask metrics
2. ? Test other mask shapes (within Masked Domain):
   - Ring (annulus)
   - Corridor (narrow channel)
   - PercolationHoles (random obstacles)
3. ? Compare different mask radii (10, 20, 30 cells)
4. ? Test interaction with other mechanisms (diffusion modes, boundary modes)
5. ? Move to next mechanism dropdown verification

### **If Test FAILS:**
1. ? Document exact failure mode
2. ? Add Debug.Log statements to trace data flow:
   - UI Dropdown ? WorkingConfig.TopologyMode
   - WorkingConfig ? ScenarioDefinition.EngineConfig.TopologyMode
   - ScenarioDefinition ? Engine mask application
3. ? Check if mask application code exists in engine
4. ? Verify Stage 13 mask features are implemented
5. ? Fix broken pipeline link
6. ? Re-run verification

---

## **xi. UI Implementation Notes**

### **Dropdown Options Summary:**

| Dropdown Index | Label | TopologyMode Enum | Description |
|----------------|-------|-------------------|-------------|
| 0 | "Full Domain" | `TopologyMode.RectGrid` | All cells active, full 64×64 grid |
| 1 | "Masked Domain" | `TopologyMode.MaskedDomain` | Shaped domain with mask controls |

**NOT in dropdown (future features):**
- ~~HexGrid~~ (index 2) - Future Stage 13.8
- ~~GraphDomain~~ (index 3) - Future Stage 13.9

### **Expected UI Behavior:**

**When "Full Domain" selected:**
- Mask controls panel: **Hidden**
- All 4096 cells potentially active
- Standard rectangular grid simulation

**When "Masked Domain" selected:**
- Mask controls panel: **Visible**
- Shows:
  - Mask Shape dropdown (Circle, Ring, Corridor, etc.)
  - Mask Radius slider/input
  - Additional shape-specific controls (Inner Radius for Ring, Width for Corridor)

---

## **xii. Theoretical Deep Dive**

### **Why Topology Matters in Spatial Simulations**

**Mathematical Foundation:**
- Topology defines the **connectivity graph** of the simulation
- Diffusion operator depends on **neighbor relationships**
- Laplacian operator: ?²u = ?(u_neighbor - u_center)

**Physical Implications:**

**1. Full Domain (RectGrid):**
```
?²u ? (u_N + u_S + u_E + u_W - 4u_center) / h²
```
- Axis-aligned Laplacian
- Manhattan distance metric
- Anisotropic (biased toward axes)
- **No spatial constraints**

**2. Masked Domain (Circle):**
```
?²u = {Laplacian if inside circle
       0 if outside (Dirichlet boundary)}
```
- Finite domain with boundary conditions
- Affects long-range correlations
- Changes equilibrium states
- **Spatial constraints enforce bounded growth**

---

### **Biological Relevance:**

**Full Domain (Unconstrained Growth):**
- **In vitro cell culture** on large dishes
- **Bacterial lawn** on agar plates
- **Unrestricted expansion** scenarios

**Masked Domain (Confined Growth):**
- **Bacterial colonies** in droplets (circular constraint)
- **Embryonic development** in finite volumes
- **Tissue growth** in defined anatomical regions
- **Microfluidic devices** with shaped channels

---

### **Computational Considerations:**

**Memory:**
- Full Domain: Dense array (N × M cells), all potentially active
- Masked Domain: Dense array but some cells permanently inactive

**Performance:**
- Full Domain: All cells processed (max computation)
- Masked Domain: Masked cells skipped (slightly faster if many masked)

**Scalability:**
- Full Domain: O(N²) for N×N grid
- Circle Mask: O(?R²) ? 0.78 N² (fewer active cells)

---

## **xiii. Related Tests**

**Before testing Topology:**
- ? Verify dropdown auto-close works (from previous tests)
- ? Verify button feedback works (from previous tests)

**After testing Topology:**
- Test mask shapes within Masked Domain:
  - Circle vs Ring (different inner/outer structure)
  - Corridor (anisotropic constraint)
  - PercolationHoles (random connectivity)
- Test interaction: Masked Domain + different boundary modes
- Test interaction: Masked Domain + different diffusion modes

**Comprehensive Test:**
- Run same preset with both topology modes
- Compare expansion patterns, metrics, equilibrium states
- Verify each produces DISTINCT behavior

---

**Created:** 2024  
**Updated:** 2024 (2-option dropdown)  
**Status:** ?? Ready for Topology Dropdown Verification  
**Stage:** 13.6 - Mechanism UI Integration  
**Priority:** HIGH (foundational mechanism)  
**Implementation:** 2 options: Full Domain, Masked Domain
