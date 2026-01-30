# UI Positioning - Quick Fix Checklist

**Goal:** Fix floating UI elements - nothing in random space!

**Time:** 30-45 minutes

**Scope:** RectTransform anchors/offsets ONLY - no refactoring!

---

## ? **Step-by-Step Checklist**

### **Phase 1: Canvas & Root (5 min)**

- [ ] **Canvas** ? Canvas Scaler:
  - UI Scale Mode: `Scale With Screen Size`
  - Reference Resolution: `1920 x 1080`
  - Match: `0.5`

- [ ] **UICanvas** (or UIRoot) ? RectTransform:
  - Anchor Min: `(0, 0)`
  - Anchor Max: `(1, 1)`
  - Offsets: All `0`

---

### **Phase 2: TopBar (5 min)**

- [ ] **TopBar** ? RectTransform:
  - Anchor Min: `(0, 1)`
  - Anchor Max: `(1, 1)`
  - Pivot: `(0.5, 1)`
  - Left: `0`, Right: `0`, Top: `0`
  - Height: `56`

- [ ] **TopBar** ? Components:
  - [ ] Add `Horizontal Layout Group`
    - Padding: `8` all sides
    - Spacing: `8`
    - Child Force Expand Height: ?
  - [ ] Add `Image` (background)

---

### **Phase 3: MainSplit (5 min)**

- [ ] **MainSplit** (container for SimView + RightDock) ? RectTransform:
  - Anchor Min: `(0, 0)`
  - Anchor Max: `(1, 1)`
  - Left: `0`, Right: `0`, Bottom: `0`
  - Top: `56` ?? CRITICAL!

- [ ] **MainSplit** ? Components:
  - [ ] Add `Horizontal Layout Group`
    - Padding: `8` all sides
    - Spacing: `8`
    - Child Force Expand: Width ?, Height ?

---

### **Phase 4: SimView (5 min)**

- [ ] **SimView** ? RectTransform:
  - ?? Do NOT set anchors manually!
  - Let parent LayoutGroup control

- [ ] **SimView** ? Components:
  - [ ] Add `Layout Element`
    - Flexible Width: `1`
    - Flexible Height: `1`
    - Min Width: `400`
    - Min Height: `300`

---

### **Phase 5: RightDock (10 min)**

- [ ] **RightDock** ? RectTransform:
  - ?? Do NOT set anchors manually!

- [ ] **RightDock** ? Components:
  - [ ] Add `Layout Element`
    - Preferred Width: `420` ??
    - Flexible Width: `0`
    - Flexible Height: `1`
    - Min Width: `420`
  
  - [ ] Add `Vertical Layout Group`
    - Padding: `10` all sides
    - Spacing: `10`
    - Child Force Expand Width: ?
  
  - [ ] Add `Image` (background)
    - Color: R: 25, G: 25, B: 35, A: 230

---

### **Phase 6: DockModeRow (5 min)**

- [ ] **DockModeRow** ? Components:
  - [ ] Add `Layout Element`
    - Preferred Height: `44`
    - Min Height: `44`
    - Flexible Height: `0`
  
  - [ ] Add `Horizontal Layout Group`
    - Padding: `8` all sides
    - Spacing: `8`
    - Child Alignment: Middle Left

- [ ] **ModeLabel** ? Layout Element:
  - Preferred Width: `50`

- [ ] **ModeDropdown** ? Layout Element:
  - Preferred Width: `220`

---

### **Phase 7: DockContent (5 min)**

- [ ] **DockContent** (or ContentArea) ? Components:
  - [ ] Add `Layout Element`
    - Flexible Height: `1` ??
    - Min Height: `200`

---

### **Phase 8: Panels (10 min)**

**For SetupPanel, InspectPanel, ExportPanel:**

- [ ] **SetupPanel** ? RectTransform:
  - Anchor Min: `(0, 0)`
  - Anchor Max: `(1, 1)`
  - Offsets: All `0`

- [ ] **InspectPanel** ? Same as SetupPanel

- [ ] **ExportPanel** ? Same as SetupPanel

- [ ] **SetActive States:**
  - SetupPanel: `true`
  - InspectPanel: `false`
  - ExportPanel: `false`

---

### **Phase 9: Sections (5-10 min)**

**For each section in SetupPanel:**

- [ ] **Section** ? RectTransform:
  - ?? Do NOT set anchors! Parent has LayoutGroup

- [ ] **Section** ? Components:
  - [ ] Add `Vertical Layout Group` (if container)
  - [ ] Add `Layout Element` (if needs size control)
    - Preferred Height: (appropriate value)
    - OR Flexible Height: (if needed)

---

## ?? **Testing (5 min)**

- [ ] Set Game view to `1920 x 1080`
- [ ] Press Play
- [ ] Visual checks:
  - [ ] TopBar at top, 56px tall, full width
  - [ ] RightDock on right, 420px wide
  - [ ] SimView fills left side
  - [ ] Dropdown row 44px tall
  - [ ] Panels switch without position change
  - [ ] NO floating elements in center
- [ ] Switch Game view to `1280 x 720`
- [ ] Check:
  - [ ] UI scales proportionally
  - [ ] No elements cut off
- [ ] Console check:
  - [ ] No layout warnings
  - [ ] No anchor errors

---

## ?? **Common Issues**

### **Element in center of screen:**
- [ ] Check parent has LayoutGroup
- [ ] If yes: Remove manual anchors
- [ ] If no: Set anchors explicitly

### **Element clips outside:**
- [ ] Remove manual anchoredPosition
- [ ] Let LayoutGroup control
- [ ] Use LayoutElement for SIZE only

### **RightDock not 420px:**
- [ ] Check Layout Element exists
- [ ] Preferred Width: 420
- [ ] Flexible Width: 0

### **Panels don't fill:**
- [ ] Anchors: Min (0,0), Max (1,1)
- [ ] Offsets: All 0

---

## ?? **Progress Tracker**

**Completed:**
- [ ] Phase 1: Canvas & Root
- [ ] Phase 2: TopBar
- [ ] Phase 3: MainSplit
- [ ] Phase 4: SimView
- [ ] Phase 5: RightDock
- [ ] Phase 6: DockModeRow
- [ ] Phase 7: DockContent
- [ ] Phase 8: Panels
- [ ] Phase 9: Sections
- [ ] Testing

**Time estimate:** ~45 minutes for all phases

---

## ? **Success Criteria**

At 1920x1080:
- ? TopBar: 1920 × 56
- ? RightDock: 420 × ~1008
- ? SimView: ~1476 × ~1008
- ? DockModeRow: ~404 × 44
- ? DockContent: ~404 × ~948
- ? NO floating elements
- ? Panel switching works smoothly

---

## ?? **Reference Documents**

- **`UI_POSITIONING_FIX_GUIDE.md`** ? Detailed step-by-step
- **`UI_LAYOUT_VISUAL_REFERENCE.md`** ? Visual diagrams
- **This file** ? Quick checklist

---

**Start with Phase 1, work through sequentially!** ??

**Result:** Clean, professional UI with everything positioned correctly! ?
