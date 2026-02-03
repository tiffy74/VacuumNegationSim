# URGENT FIX: Both Headers AND Panels Visible At Once

**Problem:** When you run the game, you see BOTH:
- DockModeRow (the dropdown header)
- ContentArea panels (SetupPanel, InspectPanel, ExportPanel)

All appearing at the same time, creating visual clutter.

**Expected:** Only the dropdown and ONE active panel should be visible.

---

## ?? **Root Cause Diagnosis**

This happens when:
1. **Hierarchy is wrong** - panels are not inside ContentArea, OR
2. **Multiple panels SetActive(true)** - more than one panel enabled, OR
3. **Missing ContentArea container** - panels are direct children of RightDock

---

## ?? **Step 1: Check Your Hierarchy**

### **Select RightDock in Hierarchy, expand it fully**

**What do you see?**

### **Expected Structure:**
```
RightDock
?? DockModeRow (visible)
?  ?? ModeLabel
?  ?? ModeDropdown
?
?? ContentArea (visible)
   ?? SetupPanel (SetActive: true)
   ?? InspectPanel (SetActive: false)
   ?? ExportPanel (SetActive: false)
```

### **Wrong Structure #1: Panels as direct children**
```
RightDock
?? DockModeRow ?
?? SetupPanel ? (should be INSIDE ContentArea!)
?? InspectPanel ?
?? ExportPanel ?
?? ContentArea (empty) ?
```

### **Wrong Structure #2: No ContentArea**
```
RightDock
?? DockModeRow ?
?? SetupPanel ?
?? InspectPanel ?
?? ExportPanel ?
```

### **Wrong Structure #3: Multiple panels active**
```
RightDock
?? ContentArea
   ?? SetupPanel (SetActive: true) ?
   ?? InspectPanel (SetActive: true) ? Should be false!
   ?? ExportPanel (SetActive: true) ? Should be false!
```

---

## ? **Fix #1: Move Panels Into ContentArea**

**If panels are direct children of RightDock:**

### **Step-by-step:**

1. **In Hierarchy, find `ContentArea`**
   - If it doesn't exist, see Fix #2

2. **Drag `SetupPanel` onto `ContentArea`**
   - This makes it a child of ContentArea

3. **Drag `InspectPanel` onto `ContentArea`**

4. **Drag `ExportPanel` onto `ContentArea`**

5. **Verify order:**
   ```
   ContentArea
   ?? SetupPanel
   ?? InspectPanel
   ?? ExportPanel
   ```

---

## ? **Fix #2: Create ContentArea Container**

**If ContentArea doesn't exist:**

### **Step-by-step:**

1. **Right-click `RightDock`** ? Create Empty

2. **Name it:** `ContentArea`

3. **Drag it BELOW `DockModeRow`** in hierarchy
   ```
   RightDock
   ?? DockModeRow (should be first)
   ?? ContentArea (should be second)
   ```

4. **Configure ContentArea:**

   **RectTransform:**
   - Anchors: (0,0) to (1,1) via Shift+Alt+click stretch preset
   - Offsets: All 0

   **Add Layout Element:**
   - Component ? Layout ? Layout Element
   - Flexible Height: `1`
   - Min Height: `200`

5. **Move panels into ContentArea:**
   - Drag SetupPanel, InspectPanel, ExportPanel as children

---

## ? **Fix #3: Disable Inactive Panels**

**If multiple panels are active:**

### **Step-by-step:**

1. **Select `SetupPanel`**
   - **Check:** Checkbox next to name in Inspector should be `?` (enabled)

2. **Select `InspectPanel`**
   - **Uncheck:** Checkbox next to name (disable it)
   - Result: InspectPanel grays out in Hierarchy

3. **Select `ExportPanel`**
   - **Uncheck:** Checkbox next to name (disable it)
   - Result: ExportPanel grays out in Hierarchy

**Expected result in Hierarchy:**
```
ContentArea
?? SetupPanel (white text = active)
?? InspectPanel (gray text = inactive)
?? ExportPanel (gray text = inactive)
```

---

## ? **Fix #4: Verify RightDock Layout**

**RightDock should stack children vertically:**

### **Select `RightDock`**

**Check for Vertical Layout Group:**

**If missing, add it:**
```
Component ? Layout ? Vertical Layout Group

Settings:
?? Padding: 10 (all sides)
?? Spacing: 10
?? Child Alignment: Upper Center
?? Child Control Size: Width ?
?? Child Force Expand: Width ?
```

**Result:** DockModeRow at top, ContentArea fills remaining space

---

## ?? **Testing After Fixes**

### **Test 1: Hierarchy Check**

**Expected structure:**
```
RightDock
?? DockModeRow (active)
?? ContentArea (active)
   ?? SetupPanel (active) ? white text
   ?? InspectPanel (inactive) ? gray text
   ?? ExportPanel (inactive) ? gray text
```

**If any panel is outside ContentArea:** Move it inside (drag & drop)

---

### **Test 2: Scene View Check**

1. **Select `SetupPanel`**
2. **In Scene view:**
   - Blue outline should be INSIDE ContentArea
   - Should be INSIDE RightDock (right side)

3. **Select `InspectPanel`**
4. **In Scene view:**
   - Should be grayed out (inactive)
   - Blue outline overlaps SetupPanel exactly

---

### **Test 3: Play Mode Check**

1. **Press Play**

2. **In Game view, you should see:**
   - TopBar at top
   - RightDock on right
   - Inside RightDock:
     - DockModeRow at top (dropdown)
     - SetupPanel below it (visible)
     - InspectPanel NOT visible
     - ExportPanel NOT visible

3. **Switch dropdown to "Inspect":**
   - SetupPanel should disappear
   - InspectPanel should appear

4. **Switch dropdown to "Export":**
   - InspectPanel should disappear
   - ExportPanel should appear

---

## ?? **Common Specific Issues**

### **Issue A: "I see DockModeRow, SetupPanel, InspectPanel, AND ExportPanel all at once"**

**Cause:** All panels are SetActive(true)

**Fix:**
1. Select InspectPanel ? Uncheck active checkbox
2. Select ExportPanel ? Uncheck active checkbox
3. Only SetupPanel should have checkmark

---

### **Issue B: "Panels appear OUTSIDE ContentArea in Scene view"**

**Cause:** Panels are not children of ContentArea

**Fix:**
1. Drag SetupPanel onto ContentArea (makes it a child)
2. Drag InspectPanel onto ContentArea
3. Drag ExportPanel onto ContentArea

---

### **Issue C: "ContentArea doesn't exist"**

**Cause:** Container never created

**Fix:** See Fix #2 above (create ContentArea)

---

### **Issue D: "DockModeRow and panels overlap/cover each other"**

**Cause:** RightDock missing Vertical Layout Group

**Fix:**
1. Select RightDock
2. Add Vertical Layout Group
3. Check Child Control Size: Width ?

---

### **Issue E: "Everything stacks but panels are tiny"**

**Cause:** ContentArea missing Flexible Height

**Fix:**
1. Select ContentArea
2. Add/check Layout Element component
3. Set Flexible Height: `1`

---

## ?? **Correct Hierarchy Visual**

```
Canvas
?? UICanvas
   ?? TopBar (56px, top edge)
   ?
   ?? RightDock (420px, right edge)
      ?? Component: Vertical Layout Group ?
      ?
      ?? DockModeRow (44px, at top of RightDock)
      ?  ?? Component: Layout Element (Height: 44) ?
      ?  ?? ModeLabel
      ?  ?? ModeDropdown
      ?
      ?? ContentArea (flexible, fills remaining space)
         ?? Component: Layout Element (Flexible Height: 1) ?
         ?? SetupPanel (SetActive: TRUE)
         ?? InspectPanel (SetActive: FALSE)
         ?? ExportPanel (SetActive: FALSE)
```

---

## ?? **Quick Fix Workflow**

**Do these in order:**

1. **Check ContentArea exists**
   - [ ] If no: Create it (Fix #2)
   - [ ] If yes: Continue

2. **Move panels into ContentArea**
   - [ ] Drag SetupPanel into ContentArea
   - [ ] Drag InspectPanel into ContentArea
   - [ ] Drag ExportPanel into ContentArea

3. **Disable inactive panels**
   - [ ] InspectPanel: Uncheck active
   - [ ] ExportPanel: Uncheck active
   - [ ] SetupPanel: Check active

4. **Verify RightDock Layout Group**
   - [ ] Has Vertical Layout Group component
   - [ ] Child Control Size: Width ?

5. **Verify ContentArea Layout Element**
   - [ ] Has Layout Element component
   - [ ] Flexible Height: 1

6. **Test in Play mode**
   - [ ] Only one panel visible at a time
   - [ ] Dropdown switches panels

---

## ?? **Expected Result**

### **Before Fix:**
```
RightDock:
??????????????????
? Mode: [Setup?] ? ? Dropdown
? Setup content  ? ? Panel 1
? Inspect content? ? Panel 2 (shouldn't show!)
? Export content ? ? Panel 3 (shouldn't show!)
??????????????????
All visible at once ?
```

### **After Fix:**
```
RightDock:
??????????????????
? Mode: [Setup?] ? ? Dropdown
??????????????????
? Setup content  ? ? Only active panel
?                ?
?                ?
??????????????????
Clean, one panel ?
```

---

## ?? **Diagnostic Command**

**Run this in your head while looking at Hierarchy:**

1. Is `ContentArea` a child of `RightDock`? **YES/NO**
2. Are all 3 panels children of `ContentArea`? **YES/NO**
3. Is only `SetupPanel` active (white text)? **YES/NO**
4. Are `InspectPanel` and `ExportPanel` inactive (gray)? **YES/NO**
5. Does `RightDock` have `Vertical Layout Group`? **YES/NO**
6. Does `ContentArea` have `Layout Element` with Flexible Height: 1? **YES/NO**

**If all YES:** Should work! Press Play to test.

**If any NO:** Fix that item first, then test again.

---

## ? **Success Criteria**

After fixes:

- [ ] Hierarchy: ContentArea exists as child of RightDock
- [ ] Hierarchy: All 3 panels are children of ContentArea
- [ ] Hierarchy: Only SetupPanel active (white text)
- [ ] Hierarchy: InspectPanel/ExportPanel inactive (gray text)
- [ ] Components: RightDock has Vertical Layout Group
- [ ] Components: ContentArea has Layout Element (Flexible Height: 1)
- [ ] Play mode: Only ONE panel visible at a time
- [ ] Play mode: Dropdown switches between panels correctly
- [ ] Play mode: No overlapping content

---

## ?? **Time to Fix**

- Check hierarchy: 2 min
- Move panels if needed: 3 min
- Disable panels: 1 min
- Verify components: 2 min
- Test: 2 min
- **Total: ~10 minutes**

---

**Start with Step 1 (check hierarchy), then work through fixes as needed!** ??

**The key is: Panels INSIDE ContentArea, only one active!** ?
