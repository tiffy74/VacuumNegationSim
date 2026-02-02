# Dropdown Template Height Fix

## ?? **Problem**

When you click a dropdown, the popup list is **too small** and some options are cut off/hidden.

---

## ? **Solution: Increase Template Height**

The dropdown's **Template** GameObject controls the size of the popup list. We need to make it taller.

---

## ?? **Fix Steps (Do for EACH dropdown)**

### **Step 1: Select a Dropdown**

In Hierarchy, expand to find a dropdown:
```
MechanismsSection
?? Content Panel
   ?? TopologyRow
      ?? Dropdown ? Select this
```

### **Step 2: Expand Dropdown in Hierarchy**

You should see:
```
Dropdown
?? Label (shows current selection)
?? Arrow (the little arrow icon)
?? Template (the popup list) ? We need to modify this
   ?? Viewport
      ?? Content
         ?? Item (the template for each option)
```

### **Step 3: Select Template**

Click on `Template` (child of Dropdown)

### **Step 4: In Inspector ? RectTransform**

**Change the Height:**
- **Current:** Probably ~150 or ~160
- **New:** **250** or **300** (tall enough for all options)

### **Step 5: Verify Anchors**

Make sure Template has these settings:
```
RectTransform:
?? Anchor Min: X: 0, Y: 0
?? Anchor Max: X: 1, Y: 0
?? Pivot: X: 0.5, Y: 1
?? Pos Y: 0 (or small value like 2)
?? Height: 250-300 ? Your new height
```

**Anchors should be at BOTTOM (Y: 0) so the popup expands DOWNWARD.**

---

## ?? **Repeat for All Dropdowns**

You need to do this for **ALL 6 dropdowns**:
1. TopologyRow ? Dropdown ? Template
2. BoundaryRow ? Dropdown ? Template
3. InflowRow ? Dropdown ? Template
4. DiffusionRow ? Dropdown ? Template
5. ViabilityRow ? Dropdown ? Template
6. PhaseSetRow ? Dropdown ? Template

---

## ?? **Quick Method: Edit One, Copy to All**

### **Option A: Prefab Workflow (if dropdowns are from prefab)**

1. Fix ONE dropdown's Template height
2. If it's a prefab, click **"Apply"** in Inspector
3. All instances update automatically ?

### **Option B: Manual Copy**

1. **Select Template** (from fixed dropdown)
2. **Copy Component** (Right-click RectTransform ? Copy Component)
3. **Select Template** (from another dropdown)
4. **Paste Component Values** (Right-click RectTransform ? Paste Component Values)
5. Repeat for all dropdowns

---

## ?? **Visual Guide**

### **Before (Height: 150)**
```
Dropdown ?
???????????????????
? Full Domain     ? ? Visible
? Masked Domain   ? ? Visible
? Option C        ? ? Cut off!
???????????????????
```

### **After (Height: 250)**
```
Dropdown ?
???????????????????
? Full Domain     ? ? Visible
? Masked Domain   ? ? Visible
? Option C        ? ? Now visible!
?                 ?
? (space for more)?
???????????????????
```

---

## ?? **Optimal Heights by Dropdown**

| Dropdown | Options | Recommended Height |
|----------|---------|-------------------|
| Topology | 2 | 120 |
| Boundary | 3 | 150 |
| Inflow | 3 | 150 |
| Diffusion | 3 | 150 |
| Viability | 2 | 120 |
| PhaseSet | 2 | 120 |

**Or use 250 for all** to have consistent sizing and room for future options.

---

## ?? **Advanced: Adjust Item Height**

If options are **still cramped**, you can also increase the **Item** height:

### **Step 1: Select Item**

```
Dropdown ? Template ? Viewport ? Content ? Item
```

### **Step 2: In Inspector ? Layout Element**

**Change Preferred Height:**
- **Current:** Probably ~20
- **New:** **30** or **35** (more spacing)

---

## ? **Testing**

1. **Press Play**
2. **Click a dropdown**
3. **Verify all options visible:**
   - ? No scroll bar needed (if height is sufficient)
   - ? All text fully readable
   - ? No options cut off

---

## ?? **Troubleshooting**

### **Issue A: Template appears above dropdown instead of below**

**Cause:** Anchors/Pivot wrong

**Fix:**
- Template Pivot Y: **1** (top)
- Template Anchor Max Y: **0** (bottom)
- Template Pos Y: **0** or **2**

---

### **Issue B: Template is too wide/narrow**

**Cause:** Width not set to match dropdown

**Fix:**
- Template Anchor Min X: **0**
- Template Anchor Max X: **1** (stretch to full width)

---

### **Issue C: Scrollbar appears even with tall template**

**Cause:** Content inside template has fixed height

**Fix:**
1. Select **Template ? Viewport ? Content**
2. Check **Content Size Fitter** component
3. Vertical Fit: **Preferred Size**

---

## ? **Success Criteria**

After fixing:

- [ ] Click Topology dropdown ? Both options visible
- [ ] Click Boundary dropdown ? All 3 options visible
- [ ] Click Inflow dropdown ? All 3 options visible
- [ ] Click Diffusion dropdown ? All 3 options visible
- [ ] Click Viability dropdown ? Both options visible
- [ ] Click PhaseSet dropdown ? Both options visible
- [ ] No options cut off or hidden
- [ ] No excessive scrolling needed

---

## ?? **Result**

? **Dropdown Templates tall enough**  
? **All options fully visible**  
? **No obscured text**  
? **Clean, readable UI**  

**The dropdowns now work properly!** ??
