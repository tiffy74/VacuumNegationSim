# ?? Grid Topology Dropdown - Complete Verification
## **3 Fundamental Tessellations: Rectangular, Triangular, Hexagonal**

## Component: Grid Topology Dropdown

**Test Date:** [To be filled]  
**Tester:** [To be filled]  
**Category:** Mechanism Dropdown (Setup Panel)

---

## **i. Component Identification**

- **Name:** Grid Topology Dropdown
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
    //   0 = RectGrid (Rectangular)
    //   1 = TriGrid (Triangular)
    //   2 = HexGrid (Hexagonal)
}
```

### **UI Position:**
- **Location:** RightDock ? Setup Panel ? Mechanisms Section
- **Label:** "Grid Topology" or "Tessellation"
- **Dropdown Options (3 total):**
  1. **Rectangular** (maps to `TopologyMode.RectGrid`)
  2. **Triangular** (maps to `TopologyMode.TriGrid`)
  3. **Hexagonal** (maps to `TopologyMode.HexGrid`)

### **Enum Definition:**
```csharp
public enum TopologyMode
{
    RectGrid = 0,      // Rectangular: 4 neighbors, current default
    TriGrid = 1,       // Triangular: 3 or 6 neighbors, NEW
    HexGrid = 2,       // Hexagonal: 6 neighbors, most symmetric, NEW
    MaskedDomain = 3,  // Future: shaped constraints
    GraphDomain = 4    // Future: arbitrary graphs
}
```

**Note:** Only first 3 enum values are exposed in dropdown. MaskedDomain and GraphDomain are future features.

---

## **iii. Intent & Theory**

### **What It Does:**
Controls **the fundamental tessellation of the simulation space** - determines cell shape, neighbor connectivity, and distance metrics.

### **Theoretical Justification:**

#### **The 3 Regular Tessellations of the Plane**

In 2D Euclidean space, there are exactly **3 regular tessellations** (tilings by identical regular polygons):

| Tessellation | Polygon | Neighbors | Schläfli Symbol | Symmetry Group |
|--------------|---------|-----------|-----------------|----------------|
| **Square** | Rectangle | 4 | {4,4} | p4m |
| **Triangle** | Equilateral ? | 3 or 6* | {3,6} | p6m |
| **Hexagon** | Regular ? | 6 | {6,3} | p6m |

*Triangular grids can use either vertex connectivity (6) or face adjacency (3)

**Why These 3?**
- **Complete coverage** of regular tessellation space
- **Different symmetries** (4-fold vs 6-fold)
- **Different distance metrics** (Manhattan vs Euclidean approximations)
- **Biological relevance** (skin cells, honeycomb, crystal structures)

---

### **Topology Modes (Implemented)**

| Mode | UI Label | Neighbors | Distance Metric | Symmetry | Status |
|------|----------|-----------|-----------------|----------|--------|
| **RectGrid** | **Rectangular** | 4 (N,S,E,W) | Manhattan | 4-fold | ? Default |
| **TriGrid** | **Triangular** | 6 (hexagonal vertex lattice) | Euclidean approx | 6-fold | ?? Implement |
| **HexGrid** | **Hexagonal** | 6 (uniform distance) | Euclidean approx | 6-fold | ?? Implement |

---

### **Detailed Topology Comparisons:**

#### **1. Rectangular Grid (Current Default)**

**Geometry:**
```
+---+---+---+
| ? | ? | ? |
+---+---+---+
| ? | X | ? |  ? Cell X has 4 neighbors
+---+---+---+
| ? | ? | ? |
+---+---+---+
```

**Properties:**
- **Neighbors:** 4 (Von Neumann)
- **Distance to neighbors:** Alternating (1 along axes, ?2 along diagonals if Moore-8)
- **Diffusion:** Anisotropic (faster along axes than diagonals)
- **Laplacian:** ?²u ? (u_N + u_S + u_E + u_W - 4u) / h²

**Advantages:**
- Simple implementation (2D array indexing)
- Well-understood behavior
- Cache-friendly memory layout
- Computationally efficient

**Disadvantages:**
- Axis bias (expansion favors cardinal directions)
- Anisotropic diffusion
- Corner cells have fewer effective neighbors

**When to Use:**
- Standard simulations
- Baseline comparisons
- Computational efficiency priority
- Axis-aligned phenomena

**Biological Examples:**
- Epithelial cell sheets
- Grid-based tissue models
- Lattice-based simulations

---

#### **2. Triangular Grid (NEW - Stage 13.7)**

**Geometry:**
```
  /\  /\  /\
 /  \/  \/  \
/  ? \?/  ? \
\  /X\  /  /  ? Cell X has 6 neighbors (vertex connectivity)
 \/  \/  \/
 ?  ?  ?
```

**Dual Lattice:** Hexagonal (triangles ? hexagons are dual)

**Properties:**
- **Neighbors:** 6 (if vertex connectivity) or 3 (if edge connectivity)
- **Distance to neighbors:** Uniform (if properly scaled)
- **Diffusion:** More isotropic than rectangular
- **Laplacian:** ?²u ? (u_1 + u_2 + u_3 + u_4 + u_5 + u_6 - 6u) / h²

**Advantages:**
- More symmetric than rectangular
- Better Euclidean distance approximation
- Natural for 3-fold symmetry phenomena
- Dual to hexagonal grid (easy conversion)

**Disadvantages:**
- More complex indexing than rectangular
- 6 neighbors = more computation
- Less intuitive visualization

**When to Use:**
- Systems with 3-fold or 6-fold symmetry
- Better isotropic diffusion needed
- Studying tessellation effects
- Comparison with hexagonal

**Biological Examples:**
- Close-packed cell arrangements
- Crystal lattice structures
- Columnar epithelia (hexagonal packing viewed from vertex)

**Implementation Note:**
- Store as offset coordinate system: (q, r) or (x, y) with alternating rows
- Neighbor lookups use 6-direction offsets
- Can reuse rectangular grid data structure with custom neighbor logic

---

#### **3. Hexagonal Grid (NEW - Stage 13.8)**

**Geometry:**
```
  _____       _____
 /     \     /     \
/   ?   \___/   ?   \
\       /   \       /
 \____ /  X  \_____/   ? Cell X has 6 neighbors
 /     \     /     \
/   ?   \___/   ?   \
\       /   \       /
 \_____/     \_____/
```

**Properties:**
- **Neighbors:** 6 (all at equal distance)
- **Distance to neighbors:** Uniform (d)
- **Diffusion:** Isotropic (same in all directions)
- **Laplacian:** ?²u ? (u_1 + u_2 + u_3 + u_4 + u_5 + u_6 - 6u) / h²
- **Most symmetric** regular tessellation

**Advantages:**
- **Perfectly isotropic** diffusion
- All neighbors equidistant
- Natural 6-fold symmetry
- Biologically common (honeycomb, tissues)
- Best continuous space approximation

**Disadvantages:**
- Most complex indexing (axial/cube coordinates)
- Harder to visualize
- 6 neighbors = highest computation per cell
- Non-trivial memory layout

**When to Use:**
- Maximum symmetry needed
- Isotropic phenomena (no directional bias)
- Biological pattern formation
- Comparison with other tessellations

**Biological Examples:**
- **Honeycomb** (bee hives)
- **Plant cell packing** (onion skin)
- **Turtle shell patterns**
- **Turing patterns** (reaction-diffusion)
- **Columnar epithelia**

**Implementation Note:**
- Use **axial coordinates** (q, r) or **cube coordinates** (x, y, z) with x+y+z=0
- Neighbors: 6 directions: {(+1,0), (+1,-1), (0,-1), (-1,0), (-1,+1), (0,+1)}
- Visual rendering: hexagons or offset circles

---

## **iv. Theoretical Predictions**

### **Comparison Table:**

| Property | Rectangular | Triangular | Hexagonal |
|----------|-------------|------------|-----------|
| **Neighbors** | 4 | 6 | 6 |
| **Symmetry** | 4-fold (90°) | 6-fold (60°) | 6-fold (60°) |
| **Distance uniformity** | Non-uniform | Near-uniform | Uniform |
| **Diffusion** | Anisotropic | Semi-isotropic | Isotropic |
| **Expansion rate** | ~4 cells/tick | ~6 cells/tick | ~6 cells/tick |
| **Axis bias** | Strong (4-fold) | Weak (6-fold) | None |
| **Computation** | Fastest | Medium | Slowest |
| **Memory** | Most efficient | Medium | Least efficient |
| **Biological** | Grid models | Crystal structures | Tissues, honeycomb |

---

### **Expected Visual Differences:**

**Rectangular (Current):**
```
Expansion pattern at T=50:
    ? ? ? ? ?
  ? ? ? ? ? ? ?
? ? ? ? ? ? ? ? ?
? ? ? ? ? ? ? ? ?  ? Square/diamond shape
? ? ? ? ? ? ? ? ?     Axis-aligned expansion
  ? ? ? ? ? ? ?
    ? ? ? ? ?
```

**Triangular (Expected):**
```
Expansion pattern at T=50:
      ? ? ?
    ? ? ? ? ?
  ? ? ? ? ? ? ?
? ? ? ? ? ? ? ? ? ?  ? Hexagonal outline
  ? ? ? ? ? ? ?        More circular
    ? ? ? ? ?
      ? ? ?
```

**Hexagonal (Expected):**
```
Expansion pattern at T=50:
      ? ? ?
    ? ? ? ? ?
  ? ? ? ? ? ? ?
? ? ? ? ? ? ? ? ? ?  ? Perfect circle
  ? ? ? ? ? ? ?        Isotropic expansion
    ? ? ? ? ?
      ? ? ?
```

---

## **v. Prediction: NO Changes (Rectangular Baseline)**

### **Test Configuration:**
- **Topology:** Rectangular
- **Grid Size:** 64×64
- **Preset:** Default
- **Run Duration:** 100 ticks

### **Expected Metrics:**
- **ViableCount at T100:** ~3000-4000 (depends on parameters)
- **Expansion shape:** Square/diamond (4-fold symmetry)
- **Frontier growth:** ~4 cells/tick (Von Neumann)

---

## **vi. Prediction: WITH Changes (Triangular)**

### **Test Configuration:**
- **Topology:** Triangular
- **Grid Size:** 64×64 (same total cells, different tessellation)
- **Preset:** Default (same parameters)
- **Run Duration:** 100 ticks

### **Expected Changes:**

**Visual:**
- **Expansion shape:** More hexagonal/circular outline
- **Frontier:** 6-fold symmetry visible
- **Growth rate:** ~50% faster (6 neighbors vs 4)

**Metrics:**
- **ViableCount at T100:** Higher (~4500-6000)
- **Expansion rate:** ~6 cells/tick
- **Symmetry:** 6-fold instead of 4-fold

---

## **vii. Prediction: WITH Changes (Hexagonal)**

### **Test Configuration:**
- **Topology:** Hexagonal
- **Grid Size:** 64×64 cells (same count, hexagonal tessellation)
- **Preset:** Default
- **Run Duration:** 100 ticks

### **Expected Changes:**

**Visual:**
- **Expansion shape:** Nearly perfect circle
- **Frontier:** Smooth, isotropic growth
- **No axis bias:** Equal expansion in all directions

**Metrics:**
- **ViableCount at T100:** Similar to Triangular (~4500-6000)
- **Expansion rate:** ~6 cells/tick
- **Isotropy:** Perfect (no directional preference)

---

## **viii. Verification Tests**

### **Test 1: Rectangular (Baseline)**

**Steps:**
1. Topology Dropdown ? Select "Rectangular"
2. Load Default preset
3. Play ? Run 100 ticks
4. Export ? Record metrics
5. Screenshot at T100

**Record:**
- ViableCount at T100: ___
- Expansion shape: [Square / Diamond / Circular]
- Symmetry: [4-fold / 6-fold / Other]

---

### **Test 2: Triangular (6-neighbor tessellation)**

**Steps:**
1. Restart simulation
2. Topology Dropdown ? Select "Triangular"
3. Load Default preset (same parameters)
4. Play ? Run 100 ticks
5. Export ? Record metrics
6. Screenshot at T100

**Record:**
- ViableCount at T100: ___
- Expansion shape: [More circular / Still square]
- Symmetry: [6-fold visible / 4-fold / Other]
- Growth rate: [Faster / Same / Slower] than Rectangular

---

### **Test 3: Hexagonal (Most symmetric)**

**Steps:**
1. Restart simulation
2. Topology Dropdown ? Select "Hexagonal"
3. Load Default preset
4. Play ? Run 100 ticks
5. Export ? Record metrics
6. Screenshot at T100

**Record:**
- ViableCount at T100: ___
- Expansion shape: [Circular / Hexagonal / Other]
- Symmetry: [Perfect 6-fold / Biased / Other]
- Isotropy: [No axis bias / Some bias]

---

## **ix. Compare & Verify**

### **Metrics Comparison:**

| Metric | Rectangular | Triangular | Hexagonal | Expected Pattern |
|--------|-------------|------------|-----------|------------------|
| ViableCount (T100) | ___ | ___ | ___ | Rect < Tri ? Hex |
| Expansion rate | ~4/tick | ~6/tick | ~6/tick | Hex fastest |
| Symmetry | 4-fold | 6-fold | 6-fold | Clear progression |

### **Visual Comparison:**

| Aspect | Rectangular | Triangular | Hexagonal | All Different? |
|--------|-------------|------------|-----------|----------------|
| Shape | Square/Diamond | Hexagonal | Circular | ? / ? |
| Symmetry | 4-fold | 6-fold | 6-fold | ? / ? |
| Axis bias | Strong | Weak | None | ? / ? |

---

## **x. Pass Criteria**

**UI Functionality:**
- ? Dropdown shows exactly 3 options: "Rectangular", "Triangular", "Hexagonal"
- ? Selecting option updates `workingConfig.TopologyMode`
- ? Dropdown auto-closes after selection

**Simulation Impact:**
- ? All 3 topologies produce **VISIBLY DIFFERENT** expansion patterns
- ? Triangular/Hexagonal show **6-fold symmetry** (not 4-fold)
- ? Hexagonal expansion is **most circular** (least axis bias)
- ? Growth rate: Rectangular < Triangular ? Hexagonal

**Theory Match:**
- ? Rectangular: 4 neighbors, square/diamond pattern
- ? Triangular: 6 neighbors, hexagonal outline
- ? Hexagonal: 6 neighbors, circular/isotropic expansion

---

## **xi. Implementation Notes**

### **Dropdown Setup:**

```csharp
// In Setup Panel initialization
topologyDropdown.ClearOptions();
topologyDropdown.AddOptions(new List<string>
{
    "Rectangular",    // Index 0 ? TopologyMode.RectGrid
    "Triangular",     // Index 1 ? TopologyMode.TriGrid
    "Hexagonal"       // Index 2 ? TopologyMode.HexGrid
});
topologyDropdown.value = 0; // Default to Rectangular
topologyDropdown.onValueChanged.AddListener(OnTopologyChanged);
```

### **Neighbor Indexing:**

**Rectangular (4 neighbors):**
```csharp
int[] dx = {0, 1, 0, -1}; // N, E, S, W
int[] dy = {-1, 0, 1, 0};
```

**Triangular (6 neighbors, offset coordinates):**
```csharp
// Even rows:
int[] dx_even = {1, 0, -1, -1, -1, 0};
int[] dy_even = {0, 1, 1, 0, -1, -1};
// Odd rows: different offsets
```

**Hexagonal (6 neighbors, axial coordinates):**
```csharp
int[] dq = {+1, +1, 0, -1, -1, 0};
int[] dr = {0, -1, -1, 0, +1, +1};
// Note: x + y + z = 0 constraint (cube coordinates)
```

---

## **xii. Theoretical Deep Dive**

### **Why 3 Tessellations Matter**

**Fundamental Theorem:**
Only 3 regular tessellations exist in Euclidean plane:
- {4, 4} - Squares
- {3, 6} - Triangles
- {6, 3} - Hexagons

**Symmetry Groups:**
- Rectangular: p4m (4 rotations, 4 mirrors)
- Triangular: p6m (6 rotations, 6 mirrors)
- Hexagonal: p6m (6 rotations, 6 mirrors)

**Biological Relevance:**
- **Rectangular:** Artificial (grid models, computational convenience)
- **Triangular:** Crystal structures, dense packing (vertices of hexagons)
- **Hexagonal:** Natural packing (honeycomb, plant cells, turtle shells)

**Pattern Formation:**
- Turing patterns depend on grid symmetry
- Hexagonal grids produce more realistic biological patterns
- Rectangular grids show artificial axis-aligned artifacts

---

## **xiii. Next Steps**

### **After Verification:**

**If All 3 Work Correctly:**
1. ? Document baseline metrics for each topology
2. ? Test preset compatibility (do presets work on all topologies?)
3. ? Test parameter sensitivity (how do parameters scale across topologies?)
4. ? Performance comparison (which is fastest?)
5. ? Move to next mechanism verification

**If Implementation Needed:**
1. ?? Implement Triangular neighbor indexing
2. ?? Implement Hexagonal coordinate system
3. ?? Update renderer to display Tri/Hex cells
4. ?? Test diffusion/expansion on new topologies
5. ?? Verify symmetry properties

---

**Created:** 2024  
**Status:** ?? Ready for 3-Tessellation Topology Verification  
**Stage:** 13.7-13.8 - Grid Topology Implementation  
**Priority:** HIGH (fundamental spatial structure)  
**Implementation:** 3 options: Rectangular, Triangular, Hexagonal
