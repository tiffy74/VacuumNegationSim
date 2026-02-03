# REFACTOR: Unified Header Container Architecture

**Current Problem:** Each CollapsibleSection has its own separate HeaderPanel, leading to:
- Inconsistent styling
- Duplicate layout code
- Harder to maintain
- More complex hierarchy

**Better Solution:** Create a shared HeaderContainer component that manages all section headers uniformly.

---

## ?? **Proposed Architecture**

### **Option A: Shared Header Container (Recommended)**

```
SetupPanel
?? ScrollView (if needed)
   ?? Content
      ?? MechanismSummary (read-only, no header)
      ?
      ?? MechanismsSection
      ?  ?? Header (unified component)
      ?  ?? Content (collapsible)
      ?
      ?? CoreParametersSection
      ?  ?? Header (unified component)
      ?  ?? Content (collapsible)
      ?
      ?? TopologyDetailsSection
      ?  ?? Header (unified component)
      ?  ?? Content (collapsible)
      ?
      ?? [Other sections...]
```

**Benefits:**
- ? Consistent header appearance
- ? Single place to update header styling
- ? Cleaner hierarchy
- ? Easier to add new sections

---

## ?? **Recommended Approach: Use Header Prefab**

**Quick win without major refactoring:**

### **Step 1: Create SectionHeader Prefab (5 min)**

1. **Right-click in Hierarchy** ? Create Empty
2. **Name:** `SectionHeader`
3. **Add RectTransform:** Height 40px
4. **Add Image (background):**
   - Color: R: 30, G: 35, B: 40, A: 255
5. **Add Button component**
6. **Add child TextMeshProUGUI:**
   - Name: `TitleText`
   - Text: "Section Title"
   - Font Size: 16, Bold, White
   - Alignment: Left, Middle
7. **Add child Image (arrow):**
   - Name: `Arrow`
   - Sprite: Triangle
   - Size: 16×16
   - Rotation: 0 (expanded) or -90 (collapsed)
8. **Drag to Project** ? Save as prefab
9. **Delete from Hierarchy**

---

### **Step 2: Replace Section Headers (10 min)**

**For each section (MechanismsSection, CoreParametersSection, etc.):**

1. **Select section ? HeaderPanel**
2. **Note button's OnClick target** (e.g., MechanismsSection.ToggleExpanded)
3. **Delete HeaderPanel**
4. **Drag SectionHeader prefab** into section
5. **Rename:** `Header`
6. **Wire button:**
   - Button ? OnClick ? Section ? ToggleExpanded()
7. **Set title:**
   - TitleText ? Text: "Mechanisms" (or appropriate title)
8. **Wire to CollapsibleSection:**
   - Section ? headerObject: Drag Header
   - Section ? headerText: Drag TitleText

---

### **Step 3: Test (5 min)**

1. **Press Play**
2. **Click each header** ? Expands/collapses
3. **All headers look consistent** ?
4. **Done!**

---

## ?? **Before vs After**

### **Before:**
```
MechanismsSection
?? HeaderPanel (manually configured)
?  ?? Button
?  ?? HeaderText (unique styling)
?? ContentPanel

CoreParametersSection
?? HeaderPanel (manually configured)
?  ?? Button
?  ?? HeaderText (unique styling)
?? ContentPanel
```

### **After:**
```
MechanismsSection
?? Header (prefab instance)
?? ContentPanel

CoreParametersSection
?? Header (prefab instance)
?? ContentPanel
```

---

## ? **Benefits**

- ? Consistent headers across all sections
- ? Update prefab ? all sections update
- ? Cleaner hierarchy
- ? Professional appearance
- ? No code changes needed

---

## ?? **Result**

**Time:** 15-20 minutes for all sections

**Outcome:** Unified, consistent header design! ?
