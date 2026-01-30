# Point Source Editor - Complete Implementation Guide

**What It Does:** Allows users to add, edit, and remove point sources when "Inflow Mode" is set to "Point Sources" in the Mechanisms section.

**Files Created:**
- ? `PointSourceEditorModal.cs` - Modal dialog for editing sources
- ? `PointSourceEditorRow.cs` - Individual source row component
- ? Updated `InflowDetailsSection.cs` - Wires "Edit Sources" button to modal

---

## ?? **Step 1: Create the Modal UI in Unity (10 min)**

### **A. Create Modal Panel**

1. **Right-click UICanvas** ? UI ? Panel
2. **Name:** `PointSourceEditorModal`
3. **RectTransform:**
   - Anchors: Center (0.5, 0.5) to (0.5, 0.5)
   - Width: 500
   - Height: 600
   - Centered on screen

4. **Add Component** ? `PointSourceEditorModal` script

5. **Image (background):**
   - Color: Semi-transparent dark (R: 0, G: 0, B: 0, A: 200)

---

### **B. Create Modal Header**

1. **Right-click PointSourceEditorModal** ? UI ? Panel
2. **Name:** `Header`
3. **RectTransform:**
   - Anchors: Top-stretch (0, 1) to (1, 1)
   - Height: 50
4. **Add child Text - TextMeshPro:**
   - Name: `TitleText`
   - Text: "Edit Point Sources"
   - Font Size: 20, Bold, White
   - Alignment: Center, Middle

---

### **C. Create Close Button**

1. **Right-click Header** ? UI ? Button - TextMeshPro
2. **Name:** `CloseButton`
3. **Position:** Top-right corner of Header
4. **Text:** "×" (close symbol, font size 24)

---

### **D. Create Grid Constraints Row**

1. **Right-click PointSourceEditorModal** ? UI ? Panel
2. **Name:** `GridConstraintsRow`
3. **RectTransform:**
   - Below Header
   - Height: 40
4. **Add Horizontal Layout Group:**
   - Padding: 10 all sides
   - Spacing: 10
   - Child Force Expand: Width ?

5. **Add two Input Fields:**
   - **GridWidthInput** - Label: "Grid Width:", Read-only
   - **GridHeightInput** - Label: "Grid Height:", Read-only

---

### **E. Create Source List Container**

1. **Right-click PointSourceEditorModal** ? UI ? Scroll View
2. **Name:** `SourceListScrollView`
3. **RectTransform:**
   - Fill middle area (below GridConstraintsRow, above footer)
4. **Delete Scrollbar Horizontal** (only need vertical)

5. **Select Viewport ? Content:**
   - Name: `SourceListContainer`
   - Add **Vertical Layout Group:**
     - Padding: 10
     - Spacing: 5
     - Child Force Expand: Width ?
   - Add **Content Size Fitter:**
     - Vertical Fit: Preferred Size

---

### **F. Create Source Row Prefab**

1. **Right-click SourceListContainer** ? Create Empty
2. **Name:** `SourceRowPrefab`
3. **Add Horizontal Layout Group:**
   - Padding: 5
   - Spacing: 10
   - Child Control Size: Height ?
4. **Add Layout Element:**
   - Preferred Height: 40

5. **Add child Input Fields:**

   **X Input:**
   - Name: `XInput`
   - Placeholder: "X"
   - Content Type: Integer Number
   - Add Layout Element: Flexible Width 1

   **Y Input:**
   - Name: `YInput`
   - Placeholder: "Y"
   - Content Type: Integer Number
   - Add Layout Element: Flexible Width 1

   **Strength Input:**
   - Name: `StrengthInput`
   - Placeholder: "Strength (e.g., 1e5)"
   - Content Type: Decimal Number
   - Add Layout Element: Flexible Width 2

6. **Add Remove Button:**
   - Name: `RemoveButton`
   - Text: "Remove"
   - Add Layout Element: Preferred Width 80

7. **Add Component** ? `PointSourceEditorRow` script

8. **Wire in Inspector:**
   - xInput ? Drag XInput
   - yInput ? Drag YInput
   - strengthInput ? Drag StrengthInput
   - removeButton ? Drag RemoveButton

9. **Drag to Project** ? Save as prefab: `SourceRowPrefab`
10. **Delete from Hierarchy** (we'll instantiate it via script)

---

### **G. Create Footer with Buttons**

1. **Right-click PointSourceEditorModal** ? UI ? Panel
2. **Name:** `Footer`
3. **RectTransform:**
   - Anchors: Bottom-stretch (0, 0) to (1, 0)
   - Height: 60
4. **Add Horizontal Layout Group:**
   - Padding: 10
   - Spacing: 10
   - Child Alignment: Middle Center
   - Child Force Expand: Height ?

5. **Add Buttons:**

   **Add Source Button:**
   - Name: `AddSourceButton`
   - Text: "+ Add Source"
   - Color: Green tint
   - Add Layout Element: Preferred Width 150

   **Apply Button:**
   - Name: `ApplyButton`
   - Text: "Apply"
   - Color: Blue tint
   - Add Layout Element: Preferred Width 100

---

### **H. Wire PointSourceEditorModal Script**

**Select PointSourceEditorModal GameObject:**

1. **In Inspector ? PointSourceEditorModal component:**

   **Modal UI:**
   - modalPanel ? Drag PointSourceEditorModal itself
   - closeButton ? Drag CloseButton
   - addSourceButton ? Drag AddSourceButton
   - applyButton ? Drag ApplyButton

   **Source List Container:**
   - sourceListContainer ? Drag SourceListContainer (Content inside ScrollView)
   - sourceRowPrefab ? Drag SourceRowPrefab (from Project, not Hierarchy)

   **Grid Constraints:**
   - gridWidthInput ? Drag GridWidthInput
   - gridHeightInput ? Drag GridHeightInput

---

## ?? **Step 2: Wire Modal to InflowDetailsSection (5 min)**

### **Select InflowDetailsSection GameObject:**

1. **In Inspector ? InflowDetailsSection component:**
   - **Point Source Editor Modal:** Drag `PointSourceEditorModal` GameObject

2. **Done!**

---

## ?? **Step 3: Testing (5 min)**

### **Test 1: Open Modal**

1. **Press Play**
2. **In RightDock:**
   - Click "Mechanisms" button
   - Select **Inflow:** "Point Sources"
3. **InflowDetailsSection appears** (conditional visibility)
4. **Click "Edit Sources" button**
5. **Modal appears** ?

---

### **Test 2: Add Point Sources**

1. **In Modal, click "+ Add Source"**
2. **New row appears** with default values (center of grid)
3. **Edit X, Y, Strength** fields
4. **Click "+ Add Source" again** ? Another row appears
5. **Result:** Multiple sources ?

---

### **Test 3: Remove Point Sources**

1. **Click "Remove" button** on any row
2. **Row disappears** ?

---

### **Test 4: Apply Changes**

1. **Click "Apply" button**
2. **Modal closes**
3. **InflowDetailsSection updates** with new source list
4. **Result:** Changes saved ?

---

### **Test 5: Persistence**

1. **Add sources**
2. **Apply**
3. **Close modal**
4. **Click "Edit Sources" again**
5. **Modal reopens with saved sources** ?

---

## ?? **Visual Mockup**

```
??????????????????????????????????????????????????
? Point Source Editor Modal                    × ?
??????????????????????????????????????????????????
? Grid Width: [64]  Grid Height: [64]            ?
??????????????????????????????????????????????????
? ????????????????????????????????????????????   ?
? ? Source 1:                                ?   ?
? ? X: [32]  Y: [32]  Strength: [1e5] [Remove]?   ?
? ????????????????????????????????????????????   ?
? ? Source 2:                                ?   ?
? ? X: [16]  Y: [16]  Strength: [5e4] [Remove]?   ?
? ????????????????????????????????????????????   ?
? ? Source 3:                                ?   ?
? ? X: [48]  Y: [48]  Strength: [2e5] [Remove]?   ?
? ????????????????????????????????????????????   ?
??????????????????????????????????????????????????
?        [+ Add Source]         [Apply]          ?
??????????????????????????????????????????????????
```

---

## ?? **Troubleshooting**

### **Issue A: "Edit Sources" button does nothing**

**Cause:** PointSourceEditorModal not wired in InflowDetailsSection

**Fix:**
1. Select InflowDetailsSection GameObject
2. Inspector ? Point Source Editor Modal field
3. Drag PointSourceEditorModal GameObject

---

### **Issue B: Modal doesn't appear**

**Cause:** modalPanel field not wired

**Fix:**
1. Select PointSourceEditorModal GameObject
2. Inspector ? modalPanel field
3. Drag PointSourceEditorModal itself (the root panel)

---

### **Issue C: "Add Source" doesn't create rows**

**Cause:** sourceRowPrefab or sourceListContainer not wired

**Fix:**
1. Select PointSourceEditorModal GameObject
2. Verify:
   - sourceListContainer ? Drag Content (inside ScrollView)
   - sourceRowPrefab ? Drag prefab from Project folder

---

### **Issue D: Input fields don't save values**

**Cause:** PointSourceEditorRow fields not wired

**Fix:**
1. Select SourceRowPrefab (in Project)
2. Inspector ? PointSourceEditorRow component
3. Wire:
   - xInput ? XInput field
   - yInput ? YInput field
   - strengthInput ? StrengthInput field
   - removeButton ? RemoveButton

---

### **Issue E: Apply button doesn't update InflowDetailsSection**

**Cause:** Coroutine not polling for modal close

**Fix:**
- This is handled automatically by `RefreshAfterModalCloses()` coroutine
- Check Console for "Refreshed display after modal closed" log

---

## ? **Success Criteria**

After setup:

- [ ] PointSourceEditorModal appears when "Edit Sources" clicked
- [ ] "+ Add Source" creates new rows
- [ ] Input fields editable (X, Y, Strength)
- [ ] "Remove" button deletes rows
- [ ] "Apply" button saves changes
- [ ] Modal closes after Apply
- [ ] InflowDetailsSection updates with new source count/list
- [ ] Reopening modal shows saved sources
- [ ] No Console errors

---

## ?? **Enhancement Ideas**

### **1. Grid Position Picker**
Add a visual grid where users can click to set X/Y instead of typing.

### **2. Validation**
Check that X/Y are within grid bounds, show error if invalid.

### **3. Preset Sources**
Add button to load common configurations (e.g., "Four Corners", "Center + Edges").

### **4. Strength Slider**
Add slider next to input field for easier strength adjustment.

### **5. Visual Preview**
Show a small grid preview with source positions marked.

---

## ?? **Result**

? **Functional Point Source Editor**  
? **Add/Edit/Remove sources via UI**  
? **Changes persist in configuration**  
? **Clean modal dialog UX**  
? **Integrates with existing RightDock setup**  

**Time to implement:** ~20 minutes (10 min UI setup, 5 min wiring, 5 min testing)

**Now users can edit point sources directly in the UI!** ??
