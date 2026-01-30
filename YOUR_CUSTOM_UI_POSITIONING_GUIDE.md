# UI Positioning Fix - YOUR Custom Guide

**Your UI Structure:**
```
Main Camera
SimulationManager
UICanvas
?? TopBar
?? RightDock
EventSystem
UIManager
```

**Positioning Strategy:** TopBar pinned to top, RightDock pinned to right, Main Camera fills remaining space.

---

## ? **Phase 1: Canvas Setup (2 min)**

### **Select: `Canvas` (root Canvas GameObject)**

**Canvas Component:**
- Render Mode: `Screen Space - Overlay`

**Canvas Scaler Component:**
```
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1920 x 1080
Screen Match Mode: Match Width Or Height
Match: 0.5
```

**Result:** ? Consistent screen-space scaling

---

## ? **Phase 2: UICanvas Setup (3 min)**

### **Select: `UICanvas`**

**RectTransform:**
```
Anchors:
?? Anchor Min: (0, 0)
?? Anchor Max: (1, 1)
?? Pivot: (0.5, 0.5)

Offsets:
?? Left: 0
?? Right: 0
?? Top: 0
?? Bottom: 0
```

**Optional Components:**
- Image (transparent, just to see bounds)

**Result:** ? UICanvas fills entire screen

---

## ? **Phase 3: TopBar (5 min)**

### **Select: `TopBar`**

**RectTransform:**
```
Anchors:
?? Anchor Min: (0, 1)    ? Top-left corner
?? Anchor Max: (1, 1)    ? Top-right corner (stretches horizontally)
?? Pivot: (0.5, 1)       ? Center-top

Position & Size:
?? Left: 0
?? Right: 0
?? Top: 0
?? Height: 56            ? Fixed height!
```

**Components to Add (if not present):**

1. **Horizontal Layout Group:**
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

2. **Image (background):**
   ```
   Color: Dark gray
   R: 20, G: 20, B: 25, A: 255
   ```

**Result:** ? TopBar spans full width at top, 56px tall

---

## ? **Phase 4: RightDock (10 min)**

### **Select: `RightDock`**

**RectTransform:**
```
Anchors:
?? Anchor Min: (1, 0)    ? Bottom-right corner
?? Anchor Max: (1, 1)    ? Top-right corner (stretches vertically)
?? Pivot: (1, 0.5)       ? Right-center

Position & Size:
?? Right: 0              ? Pinned to right edge
?? Width: 420            ? Fixed width!
?? Top: 56               ? Leaves space for TopBar
?? Bottom: 0             ? Touches bottom
```

**Why Top: 56?** RightDock starts below TopBar (which is 56px tall)

**Components to Add (if not present):**

1. **Layout Element:**
   ```
   Preferred Width: 420
   Min Width: 420
   Flexible Width: 0      ? Does NOT stretch
   Flexible Height: 1
   ```

2. **Vertical Layout Group:**
   ```
   Padding: 10 (all sides)
   Spacing: 10
   Child Alignment: Upper Center
   Child Control Size:
   ?? Width: ?
   ?? Height: ? (let children control)
   Child Force Expand:
   ?? Width: ?
   ?? Height: ?
   ```

3. **Image (background):**
   ```
   Color: Dark gray
   R: 25, G: 25, B: 35, A: 230
   ```

**Result:** ? RightDock is 420px wide, pinned to right edge, fills vertical space below TopBar

---

## ? **Phase 5: RightDock Children (10 min)**

### **5.1: DockModeRow (or Mode Dropdown Row)**

**Select:** Inside RightDock, whatever contains the mode dropdown

**If it exists, add components:**

1. **Layout Element:**
   ```
   Preferred Height: 44
   Min Height: 44
   Flexible Height: 0
   ```

2. **Horizontal Layout Group:**
   ```
   Padding: 8 (all sides)
   Spacing: 8
   Child Alignment: Middle Left
   Child Force Expand Height: ?
   ```

**Children (Mode Label + Mode Dropdown):**

**Mode Label:**
- Add Layout Element: Preferred Width: 50

**Mode Dropdown:**
- Add Layout Element: Preferred Width: 220

**If DockModeRow doesn't exist:**
- See `RIGHTDOCK_CLEAN_RESET.md` to create it

---

### **5.2: ContentArea (or DockContent)**

**Select:** Inside RightDock, container for SetupPanel/InspectPanel/ExportPanel

**Common names:** ContentArea, DockContent, PanelContainer

**Add components:**

1. **Layout Element:**
   ```
   Flexible Height: 1    ? Takes remaining vertical space!
   Min Height: 200
   ```

**Result:** ? ContentArea fills remaining space below DockModeRow

---

### **5.3: Panels (SetupPanel, InspectPanel, ExportPanel)**

**Select:** Each panel inside ContentArea

**For SetupPanel:**

**RectTransform:**
```
Anchors:
?? Anchor Min: (0, 0)
?? Anchor Max: (1, 1)    ? Stretch to fill parent!
?? Pivot: (0.5, 0.5)

Offsets: All 0
```

**Components:**

1. **Vertical Layout Group:**
   ```
   Padding: 10 (all sides)
   Spacing: 10
   Child Alignment: Upper Center
   Child Control Size: Width ?
   Child Force Expand: Width ?
   ```

**SetActive:** `true` (visible by default)

---

**Repeat for InspectPanel:**
- Same RectTransform settings (anchors stretch)
- Same Vertical Layout Group
- **SetActive:** `false` (hidden)

---

**Repeat for ExportPanel:**
- Same RectTransform settings
- Same Vertical Layout Group
- **SetActive:** `false` (hidden)

**Result:** ? All 3 panels occupy the same space (ContentArea), only one visible at a time

---

## ? **Phase 6: Sections Inside SetupPanel (10 min)**

**Select:** Each section GameObject inside SetupPanel

**For sections that are containers (MechanismSummary, MechanismsSection, etc.):**

**RectTransform:**
- ?? Do NOT set anchors manually! Parent (SetupPanel) has LayoutGroup.

**Components:**

1. **Vertical Layout Group** (if it's a container with children)
2. **Layout Element** (for size control):
   ```
   For fixed-height sections:
   - Preferred Height: (appropriate value, e.g., 50 for MechanismSummary)
   
   For flexible sections:
   - Leave Preferred Height empty
   - OR Flexible Height: 0
   ```

**Result:** ? Sections stack vertically inside SetupPanel, controlled by parent LayoutGroup

---

## ? **Phase 7: Main Camera & SimulationManager (No Changes Needed)**

**Main Camera:** Renders the simulation grid - positioned independently

**SimulationManager:** Script GameObject - no UI positioning needed

**These are SEPARATE from UI layout - don't touch them!**

---

## ?? **Testing (5 min)**

### **Test 1: Visual Check (Game View @ 1920x1080)**

1. **Set Game view** to 1920 x 1080
2. **Press Play**
3. **Check:**
   - [ ] TopBar spans full width at top (56px tall)
   - [ ] RightDock on right edge (420px wide)
   - [ ] RightDock fills vertical space below TopBar
   - [ ] Simulation grid visible on left (not covered by RightDock)
   - [ ] NO floating elements in center

### **Test 2: Panel Switching**

1. **In Play mode**, find mode dropdown (Setup/Inspect/Export)
2. **Switch to Inspect** ? InspectPanel appears
3. **Switch to Export** ? ExportPanel appears
4. **Switch back to Setup** ? SetupPanel appears
5. **Check:** Panels swap smoothly, no position jumping

### **Test 3: Screen Scaling**

1. **Set Game view** to 1280 x 720
2. **Check:**
   - [ ] UI scales proportionally
   - [ ] TopBar still at top
   - [ ] RightDock still on right
   - [ ] No elements cut off

### **Test 4: Console Check**

1. **Open Console** (Ctrl+Shift+C)
2. **Check:**
   - [ ] No layout warnings
   - [ ] No anchor errors
   - [ ] No missing component errors

**If all tests pass:** ? **You're done!**

---

## ?? **Expected Layout (1920x1080)**

```
???????????????????????????????????????????????????????????
? TopBar (1920 × 56px)                    [Controls]     ?
???????????????????????????????????????????????????????????
?                              ? RightDock (420px wide)   ?
?                              ????????????????????????????
?                              ? Mode: [Setup ?]   (44px) ?
?  Main Camera View            ????????????????????????????
?  (Simulation Grid)           ? ContentArea              ?
?                              ? ??????????????????????   ?
?  Fills remaining space       ? ? SetupPanel         ?   ?
?  Left of RightDock           ? ? (active)           ?   ?
?                              ? ?                    ?   ?
?                              ? ? Sections stack     ?   ?
?                              ? ? vertically here    ?   ?
?                              ? ??????????????????????   ?
?                              ?                          ?
???????????????????????????????????????????????????????????
```

**Measurements:**
- TopBar: 1920 × 56
- RightDock: 420 × ~1008 (1080 - 56 - padding)
- Main Camera View: ~1476 × ~1008 (remaining space)

---

## ?? **Troubleshooting**

### **TopBar doesn't span full width:**
- Check Anchor Min: (0,1), Anchor Max: (1,1)
- Check Left: 0, Right: 0

### **RightDock not 420px wide:**
- Check Width: 420
- Check Anchor Min: (1,0), Anchor Max: (1,1)

### **RightDock covers TopBar:**
- Check Top: 56 (leaves space for TopBar)

### **Panels don't fill ContentArea:**
- Check Anchors: Min (0,0), Max (1,1)
- Check Offsets: All 0

### **Elements still floating:**
- Check parent has correct anchors first
- Then check child anchors

---

## ? **Success Criteria**

After completing all phases:

- [x] TopBar: Full width, 56px tall, pinned to top
- [x] RightDock: 420px wide, pinned to right, fills height below TopBar
- [x] Main Camera: Renders to remaining space (left of RightDock)
- [x] Panels: Switch smoothly without position changes
- [x] NO floating elements in center
- [x] UI scales properly at different resolutions

---

## ?? **Phase Checklist**

- [ ] Phase 1: Canvas setup (2 min)
- [ ] Phase 2: UICanvas setup (3 min)
- [ ] Phase 3: TopBar positioning (5 min)
- [ ] Phase 4: RightDock positioning (10 min)
- [ ] Phase 5: RightDock children (10 min)
- [ ] Phase 6: SetupPanel sections (10 min)
- [ ] Phase 7: Testing (5 min)

**Total time:** ~45 minutes

---

## ?? **Result**

? Clean, professional UI layout  
? TopBar at top (56px)  
? RightDock on right (420px)  
? Main Camera fills remaining space  
? No floating elements  
? Responsive to different screen sizes  

**You're all set!** ??

---

**Next steps:** If you want to add sections to SetupPanel, see:
- `RIGHTDOCK_ADD_SECTIONS.md` (basic sections)
- `CONDITIONAL_SECTIONS_COMPLETE_GUIDE.md` (conditional sections)
