# ?? Grid Topology Quick Reference Card

## **3 Options = 3 Regular Tessellations**

```
???????????????????????????????????????????????????????????????
?  Grid Topology Dropdown (Setup Panel ? Mechanisms)         ?
???????????????????????????????????????????????????????????????
?  1. Rectangular  (? 4 neighbors)  ? Current Default        ?
?  2. Triangular   (? 6 neighbors)  ?? Implement Stage 13.7   ?
?  3. Hexagonal    (? 6 neighbors)  ?? Implement Stage 13.8   ?
???????????????????????????????????????????????????????????????
```

---

## **Visual Comparison**

### **Rectangular:**
```
? ? ? ? ?
? ? ? ? ?
? ? ? ? ?  ? 4 neighbors
? ? ? ? ?    Square/diamond pattern
? ? ? ? ?    Axis bias: STRONG
```
**Shape:** Square with 45° bias  
**Neighbors:** 4 (N, S, E, W)  
**Symmetry:** 4-fold (90°)  
**Biology:** Artificial grid  
**Status:** ? Implemented  

---

### **Triangular:**
```
  ? ? ? ?
 ? ? ? ? ?
? ? ? ? ? ?  ? 6 neighbors
 ? ? ? ? ?     Hexagonal outline
  ? ? ? ?      Axis bias: WEAK
```
**Shape:** Hexagonal outline  
**Neighbors:** 6 (dual of hexagon)  
**Symmetry:** 6-fold (60°)  
**Biology:** Crystal structures  
**Status:** ?? To implement (Stage 13.7)  

---

### **Hexagonal:**
```
  ? ? ? ?
 ? ? ? ? ?
? ? ? ? ? ?  ? 6 neighbors (uniform distance)
 ? ? ? ? ?     Perfect circle
  ? ? ? ?      Axis bias: NONE
```
**Shape:** Perfect circle  
**Neighbors:** 6 (all equidistant)  
**Symmetry:** 6-fold (60°)  
**Biology:** Honeycomb, tissues  
**Status:** ?? To implement (Stage 13.8)  

---

## **Key Differences at a Glance**

| Feature | ? Rectangular | ? Triangular | ? Hexagonal |
|---------|--------------|--------------|-------------|
| **Neighbors** | 4 | 6 | 6 |
| **Shape** | Square | Hexagon | Circle |
| **Symmetry** | 90° | 60° | 60° |
| **Axis Bias** | ? Strong | ?? Weak | ? None |
| **Diffusion** | Anisotropic | Semi-isotropic | **Isotropic** |
| **Speed** | Fastest | Medium | Slowest |
| **Biology** | Grid models | Crystals | **Tissues** |

---

## **Expected Metrics**

### **Expansion Rate (cells/tick):**
```
Rectangular:   ~4 cells/tick  (4 neighbors)
Triangular:    ~6 cells/tick  (6 neighbors)
Hexagonal:     ~6 cells/tick  (6 neighbors)
```

### **ViableCount at T=100:**
```
Rectangular:   3000-4000  (baseline)
Triangular:    4500-6000  (+50% more cells)
Hexagonal:     4500-6000  (+50% more cells)
```

### **Expansion Shape:**
```
Rectangular:   ? Square/Diamond (4-fold symmetry)
Triangular:    ? Hexagonal outline (6-fold symmetry)
Hexagonal:     ? Nearly perfect circle (isotropic)
```

---

## **Dropdown Code**

```csharp
// Setup Panel initialization
topologyDropdown.ClearOptions();
topologyDropdown.AddOptions(new List<string>
{
    "Rectangular",    // Index 0 ? TopologyMode.RectGrid
    "Triangular",     // Index 1 ? TopologyMode.TriGrid  
    "Hexagonal"       // Index 2 ? TopologyMode.HexGrid
});
topologyDropdown.value = 0; // Default = Rectangular
topologyDropdown.onValueChanged.AddListener(OnTopologyChanged);
```

---

## **Enum Mapping**

```csharp
public enum TopologyMode
{
    RectGrid = 0,      // ? Rectangular (4 neighbors)
    TriGrid = 1,       // ? Triangular  (6 neighbors)
    HexGrid = 2,       // ? Hexagonal   (6 neighbors)
    MaskedDomain = 3,  // Future: constrained domains
    GraphDomain = 4    // Future: arbitrary graphs
}
```

---

## **Verification Quick Test**

### **1. Load Preset**
- Select topology from dropdown
- Load "Default" preset
- Click "Play"

### **2. Run 100 Ticks**
- Watch expansion pattern
- Note shape and symmetry

### **3. Compare:**

| Topology | Expected Shape | Expected Symmetry |
|----------|---------------|------------------|
| Rectangular | ? Square/Diamond | 4-fold (axis-aligned) |
| Triangular | ? Hexagonal | 6-fold |
| Hexagonal | ? Circular | Isotropic (no bias) |

### **4. Pass Criteria:**
- ? All 3 look **DIFFERENT**
- ? Triangular/Hexagonal more circular than Rectangular
- ? Hexagonal most symmetric (no axis bias)

---

## **Why These 3?**

**Mathematical Theorem:**
> There are exactly **3 regular tessellations** of the Euclidean plane:
> - {4,4} Squares (4 at each vertex)
> - {3,6} Triangles (6 at each vertex)
> - {6,3} Hexagons (3 at each vertex)

**Biological Relevance:**
- **Rectangular:** Not natural, but computational standard
- **Triangular:** Crystal lattices, dense packing (vertices of hexagons)
- **Hexagonal:** **Most common** in nature (honeycomb, plant cells, tissues)

**Simulation Benefit:**
- Complete coverage of regular tessellation behaviors
- Different symmetries (4-fold vs 6-fold)
- Different isotropy (anisotropic vs isotropic)
- Direct comparison of spatial effects

---

## **Implementation Stages**

### **Stage 13.6 (Current):**
? Enum updated (`RectGrid`, `TriGrid`, `HexGrid`)  
? Dropdown shows 3 options  
? Verification documents created  

### **Stage 13.7 (Triangular):**
?? Implement 6-neighbor indexing (offset coordinates)  
?? Test 6-fold symmetry  
?? Renderer: Show triangles or offset hexagons  

### **Stage 13.8 (Hexagonal):**
?? Implement axial/cube coordinates  
?? Test isotropy (circular expansion)  
?? Renderer: Show hexagons  

---

## **Files Created**

1. **GRID_TOPOLOGY_3_TESSELLATIONS_VERIFICATION.md**  
   - Full verification plan (30+ pages)
   - Theory, predictions, procedures

2. **GRID_TOPOLOGY_IMPLEMENTATION_SUMMARY.md**  
   - Quick reference (this file)
   - Implementation roadmap

3. **GRID_TOPOLOGY_QUICK_REFERENCE.md**  
   - 1-page cheat sheet
   - Visual comparisons

4. **MechanismEnums.cs** (updated)  
   - 3-option enum

5. **EngineConfig.cs** (updated)  
   - TopologyMode property

---

## **Key Takeaway**

```
?????????????????????????????????????????????????????
?  3 Regular Tessellations = 3 Spatial Behaviors   ?
?                                                   ?
?  ? Rectangular ? Square expansion (4-fold)       ?
?  ? Triangular  ? Hexagonal expansion (6-fold)    ?
?  ? Hexagonal   ? Circular expansion (isotropic)  ?
?                                                   ?
?  Covers ALL regular 2D tessellations!            ?
?????????????????????????????????????????????????????
```

---

**Print this page for quick reference during implementation! ??**
