# VIABLE UI Layout Fix - Positioning Guide (NO REFACTORING)

**GOAL:** Fix UI element positioning so nothing floats in random space.

**SCOPE:** Adjust RectTransform anchors, pivots, and Layout Groups ONLY. Do NOT create/delete objects or refactor code.

**TIME:** 30-45 minutes

---

## ?? **CRITICAL RULES**

1. ? **Every UI element** must have explicit anchors OR be governed by a LayoutGroup
2. ? **Never leave default center anchors** (0.5, 0.5) unless intentionally centered
3. ? **Use Layout Groups** for automatic positioning
4. ? **Use LayoutElement** to control size within Layout Groups
5. ? **Do NOT** manually set anchoredPosition if parent has LayoutGroup
6. ? **Do NOT** create or delete any GameObjects
7. ? **Do NOT** refactor scripts or architecture

---

## ?? **Step-by-Step Positioning Fixes**

### **Step 0: Verify Canvas Setup**

**Select:** `Canvas` GameObject

**Check Inspector:**

1. **Canvas Component:**
   - Render Mode: `Screen Space - Overlay`

2. **Canvas Scaler Component:**
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
   - Screen Match Mode: `Match Width Or Height`
   - Match: `0.5`

**Result:** ? Canvas uses consistent screen-space scaling

---

### **Step 1: Fix UIRoot (Full-Screen Container)**

**Find:** `Canvas` ? `UICanvas` (or whatever your root container is called)

**RectTransform Settings:**

```
Anchors:
?? Anchor Min: (0, 0)    ? Bottom-left corner
?? Anchor Max: (1, 1)    ? Top-right corner
?? Pivot: (0.5, 0.5)     ? Center

Offsets:
?? Left: 0
?? Right: 0
?? Top: 0
?? Bottom: 0
```

**Visual Check:** Blue RectTransform outline touches all 4 screen edges in Scene view

**Optional:** Add Image component (transparent) to see bounds in Game view

---

### **Step 2: Fix TopBar (Pinned to Top, Fixed Height)**

**Find:** `UICanvas` ? `TopBar`

**RectTransform Settings:**

```
Anchors:
?? Anchor Min: (0, 1)    ? Top-left corner
?? Anchor Max: (1, 1)    ? Top-right corner (stretches horizontally)
?? Pivot: (0.5, 1)       ? Center-top

Offsets:
?? Left: 0
?? Right: 0
?? Top: 0
?? Pos Y: 0 (or Height: 56)
```

**Height:** Exactly `56` pixels

**Components:**

1. **Add Horizontal Layout Group** (if not exists):
   ```
   Padding: 8 (all sides)
   Spacing: 8
   Child Alignment: Middle Left
   Child Control Size:
   ?? Width: ?
   ?? Height: ?
   Child Force Expand:
   ?? Width: ?
   ?? Height: ?
   ```

2. **Add Image** (background):
   - Color: Dark (R: 20, G: 20, B: 25, A: 255)

**Visual Check:** TopBar spans full width at top, 56px tall

---

### **Step 3: Fix MainSplit (Fills Space Below TopBar)**

**Find:** `UICanvas` ? `MainSplit` (or whatever contains SimView + Dock)

**If MainSplit doesn't exist, find the container that holds both SimView and RightDock**

**RectTransform Settings:**

```
Anchors:
?? Anchor Min: (0, 0)    ? Bottom-left
?? Anchor Max: (1, 1)    ? Top-right
?? Pivot: (0.5, 0.5)

Offsets:
?? Left: 0
?? Right: 0
?? Top: 56   ? CRITICAL! Leaves space for TopBar
?? Bottom: 0
```

**Components:**

1. **Add Horizontal Layout Group** (if not exists):
   ```
   Padding: 8 (all sides)
   Spacing: 8
   Child Alignment: Upper Left
   Child Control Size:
   ?? Width: ?
   ?? Height: ?
   Child Force Expand:
   ?? Width: ?
   ?? Height: ?
   ```

**Visual Check:** MainSplit fills screen below TopBar (top edge at Y = -56 from top)

---

### **Step 4: Fix SimView (Left Side, Flexible Width)**

**Find:** `MainSplit` ? `SimView` (or similar)

**RectTransform Settings:**

```
?? DO NOT set anchors manually! Let parent LayoutGroup control position.

If you see manual values, CLEAR THEM:
- Anchored Position: (0, 0) is fine
- But anchors should match parent's LayoutGroup behavior
```

**Components:**

1. **Add Layout Element** (if not exists):
   ```
   Preferred Width: (leave empty)
   Preferred Height: (leave empty)
   Flexible Width: 1   ? Takes remaining space!
   Flexible Height: 1
   Min Width: 400      ? Prevents collapse
   Min Height: 300
   ```

2. **Ensure NO manually set size** (sizeDelta should be controlled by LayoutGroup)

**Visual Check:** SimView fills left side, grows/shrinks with window

---

### **Step 5: Fix RightDock (Right Side, Fixed Width 420px)**

**Find:** `MainSplit` ? `RightDock`

**RectTransform Settings:**

```
?? DO NOT set anchors manually! Let parent LayoutGroup control position.
```

**Components:**

1. **Add Layout Element** (if not exists):
   ```
   Preferred Width: 420   ? Fixed width!
   Preferred Height: (leave empty)
   Flexible Width: 0      ? Does NOT stretch
   Flexible Height: 1
   Min Width: 420
   ```

2. **Add Vertical Layout Group** (if not exists):
   ```
   Padding: 10 (all sides)
   Spacing: 10
   Child Alignment: Upper Center
   Child Control Size:
   ?? Width: ?
   ?? Height: ? (let children control their height)
   Child Force Expand:
   ?? Width: ?
   ?? Height: ?
   ```

3. **Add Image** (background):
   - Color: Dark gray (R: 25, G: 25, B: 35, A: 230)

**Visual Check:** RightDock is exactly 420px wide, pinned to right edge

---

### **Step 6: Fix DockModeRow (Fixed Height 44px)**

**Find:** `RightDock` ? `DockModeRow`

**RectTransform Settings:**

```
?? Controlled by parent LayoutGroup - do NOT set anchors manually!
```

**Components:**

1. **Add Layout Element**:
   ```
   Preferred Height: 44   ? Fixed height!
   Min Height: 44
   Flexible Height: 0
   ```

2. **Add Horizontal Layout Group** (if not exists):
   ```
   Padding: 8 (all sides)
   Spacing: 8
   Child Alignment: Middle Left
   Child Control Size:
   ?? Width: ?
   ?? Height: ?
   Child Force Expand:
   ?? Width: ?
   ?? Height: ?
   ```

**Children (ModeLabel + ModeDropdown):**

**ModeLabel:**
- Add Layout Element: Preferred Width: 50

**ModeDropdown:**
- Add Layout Element: Preferred Width: 220

**Visual Check:** Row is 44px tall, label + dropdown sit side-by-side

---

### **Step 7: Fix DockContent (Flexible Height, Contains Panels)**

**Find:** `RightDock` ? `ContentArea` (or `DockContent`)

**RectTransform Settings:**

```
?? Controlled by parent LayoutGroup!
```

**Components:**

1. **Add Layout Element**:
   ```
   Flexible Height: 1   ? Takes remaining vertical space!
   Min Height: 200
   ```

2. **Optional Image** (to see bounds)

**Visual Check:** ContentArea fills remaining space below DockModeRow

---

### **Step 8: Fix Panel Containers (SetupPanel, InspectPanel, ExportPanel)**

**Find:** `ContentArea` ? `SetupPanel`, `InspectPanel`, `ExportPanel`

**For EACH panel:**

**RectTransform Settings:**

```
Anchors:
?? Anchor Min: (0, 0)    ? Bottom-left
?? Anchor Max: (1, 1)    ? Top-right (STRETCH to fill parent!)
?? Pivot: (0.5, 0.5)

Offsets:
?? Left: 0
?? Right: 0
?? Top: 0
?? Bottom: 0
```

**Why?** All 3 panels occupy the SAME space (DockContent), only one active at a time.

**Components:**

1. **Add Vertical Layout Group** (if not exists):
   ```
   Padding: 10 (all sides)
   Spacing: 10
   Child Alignment: Upper Center
   Child Control Size: Width ?
   Child Force Expand: Width ?
   ```

2. **Optional Image** (different color per panel for testing)

**SetActive State:**
- SetupPanel: `true` (visible by default)
- InspectPanel: `false`
- ExportPanel: `false`

**Visual Check:** Only SetupPanel visible, fills ContentArea exactly

---

### **Step 9: Fix Section Containers Inside SetupPanel**

**Find:** `SetupPanel` ? `MechanismSummary`, `MechanismsSection`, etc.

**For EACH section:**

**RectTransform:**

```
?? Controlled by parent LayoutGroup! Do NOT set anchors manually.
```

**Components:**

**For sections that should have fixed height:**
- Add Layout Element: Preferred Height: (appropriate value)

**For sections that should be flexible:**
- Add Layout Element: Flexible Height: 0 (or leave empty)

**All sections should have:**
- Vertical Layout Group (if it's a container with children)

---

## ?? **Common Issues & Fixes**

### **Issue 1: Element Floats in Center of Screen**

**Cause:** Default anchors (0.5, 0.5) with no parent LayoutGroup

**Fix:**
1. Check parent has LayoutGroup? If yes, remove manual anchors
2. If no LayoutGroup, set anchors explicitly:
   - For full-stretch: AnchorMin (0,0), AnchorMax (1,1)
   - For top-pinned: AnchorMin (0,1), AnchorMax (1,1)
   - For fixed position: Set appropriate anchors for corner/edge

---

### **Issue 2: Element Clips Outside Container**

**Cause:** Manual anchoredPosition overrides LayoutGroup

**Fix:**
1. Select element
2. Remove any manual Position X/Y values
3. Let LayoutGroup control position
4. Use LayoutElement to control SIZE only

---

### **Issue 3: Panels Don't Fill DockContent**

**Cause:** Anchors not set to stretch

**Fix:**
1. Select panel (SetupPanel, etc.)
2. Set Anchors: Min (0,0), Max (1,1)
3. Set Offsets: All 0
4. Result: Panel fills parent exactly

---

### **Issue 4: RightDock Not Fixed Width**

**Cause:** Missing LayoutElement with Preferred Width

**Fix:**
1. Select RightDock
2. Add Layout Element (if missing)
3. Set Preferred Width: 420
4. Set Flexible Width: 0
5. Result: Dock stays 420px wide

---

## ? **Final Verification Checklist**

### **At 1920x1080 Game View:**

- [ ] TopBar spans full width (1920px)
- [ ] TopBar height is 56px
- [ ] TopBar is pinned to top edge
- [ ] SimView fills left side
- [ ] RightDock is exactly 420px wide
- [ ] RightDock is pinned to right edge
- [ ] RightDock fills vertical space below TopBar
- [ ] DockModeRow is 44px tall
- [ ] DockContent fills remaining space below DockModeRow
- [ ] SetupPanel fills DockContent exactly
- [ ] Switching dropdown swaps panels with no position change
- [ ] No element appears outside expected bounds
- [ ] No "floating" elements in center of screen

---

## ?? **Expected Pixel Values**

At 1920x1080:

| Element | Width | Height | Position |
|---------|-------|--------|----------|
| Canvas | 1920 | 1080 | (0, 0) |
| TopBar | 1920 | 56 | Top edge |
| MainSplit | 1920 | 1024 | Below TopBar (1080 - 56) |
| SimView | ~1476 | 1008 | Left (1920 - 420 - 2×8 padding) |
| RightDock | 420 | 1008 | Right edge |
| DockModeRow | 404 | 44 | Top of Dock (420 - 2×8 padding) |
| DockContent | 404 | 948 | Below DockModeRow (1008 - 44 - 2×10) |

**Note:** Values approximate due to padding/spacing

---

## ?? **Testing Procedure**

1. **Open Unity**
2. **Set Game view to 1920x1080**
3. **Press Play**
4. **Visual checks:**
   - TopBar at top? ?
   - RightDock on right, 420px? ?
   - SimView fills left? ?
   - Dropdown works, panels swap? ?
5. **Switch Game view to 1280x720**
6. **Check scaling:**
   - UI scales proportionally? ?
   - No elements cut off? ?
7. **Done!**

---

## ?? **Quick Fix Script (If Needed)**

If you want to verify values programmatically, add this to a test script:

```csharp
// Check RightDock width
var dock = GameObject.Find("RightDock");
var rect = dock.GetComponent<RectTransform>();
Debug.Log($"Dock Width: {rect.rect.width}"); // Should be ~420

// Check TopBar height
var topBar = GameObject.Find("TopBar");
var topRect = topBar.GetComponent<RectTransform>();
Debug.Log($"TopBar Height: {topRect.rect.height}"); // Should be ~56
```

---

## ?? **Summary**

**What You Did:**
- ? Set explicit anchors for all container elements
- ? Added Layout Groups for automatic positioning
- ? Added Layout Elements for size control
- ? Fixed stretching behavior for panels
- ? Ensured RightDock has fixed 420px width
- ? Ensured TopBar has fixed 56px height

**What You Did NOT Do:**
- ? Create or delete GameObjects
- ? Refactor scripts or architecture
- ? Change code logic
- ? Rebuild UI from scratch

**Result:**
? Clean, predictable layout  
? No floating elements  
? Everything positioned correctly  
? Responsive to different screen sizes  

---

**Time:** 30-45 minutes to apply all fixes

**Difficulty:** Easy (just RectTransform adjustments)

**Result:** Professional, well-positioned UI! ??
