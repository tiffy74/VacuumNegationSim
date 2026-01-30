# RightDock Clean Reset Guide

**Goal:** Delete broken RightDock, rebuild from scratch with dropdown-based panel switching

**Time:** 30-45 minutes

**Keep:** TopBar (it's working fine!)

---

## ?? **What We're Building**

A clean, minimal RightDock with:
- ? Single dropdown to switch panels (Setup/Inspect/Export)
- ? No tab buttons
- ? Value persistence via WorkingScenarioConfig
- ? Clean layout (no scroll bars)
- ? Ready to add sections incrementally

---

## ?? **Step 1: Delete Broken RightDock (5 min)**

### **In Unity:**

1. **Open your scene**
2. **Find `RightDock` in Hierarchy**
3. **Right-click `RightDock`** ? Delete
4. **Confirm** - Yes, delete it all!
5. **Save scene** (Ctrl+S)

? **Result:** Clean slate! TopBar still works, RightDock is gone.

---

## ??? **Step 2: Create New RightDock (10 min)**

### **2.1: Create RightDock Container**

1. **Right-click UICanvas** ? Create Empty
2. **Name:** `RightDock`
3. **Add RectTransform settings:**
   - Anchor: **Right-stretch** (right edge, full height)
   - Width: **340**
   - Left: **-340**, Right: **0**
   - Top: **60** (below TopBar), Bottom: **0**
4. **Add Image component:**
   - Color: Dark gray (R: 25, G: 25, B: 35, A: 230)
5. **Add Vertical Layout Group:**
   - Padding: 10 all sides
   - Spacing: 10
   - Child Force Expand: Width ?, Height ?
   - Child Alignment: Upper Center

---

### **2.2: Add Mode Selector Row**

1. **Right-click `RightDock`** ? Create Empty
2. **Name:** `DockModeRow`
3. **Add Layout Element:**
   - Preferred Height: 44
4. **Add Horizontal Layout Group:**
   - Padding: 8
   - Spacing: 8
   - Child Alignment: Middle Left
   - Child Force Expand: Width ?, Height ?

5. **Add Mode Label:**
   - Right-click `DockModeRow` ? UI ? Text - TextMeshPro
   - Name: `ModeLabel`
   - Text: "Mode:"
   - Font Size: 14
   - Add Layout Element: Preferred Width: 50

6. **Add Mode Dropdown:**
   - Right-click `DockModeRow` ? UI ? Dropdown - TextMeshPro
   - Name: `ModeDropdown`
   - Add Layout Element: Flexible Width: 1
   - Clear default options (will be populated by script)

7. **Save scene** (Ctrl+S)

---

### **2.3: Add Content Area**

1. **Right-click `RightDock`** ? Create Empty
2. **Name:** `ContentArea`
3. **Add Layout Element:**
   - Flexible Height: 1 (takes remaining space)
4. **Add Vertical Layout Group:**
   - Padding: 5
   - Spacing: 10
   - Child Force Expand: Width ?

---

### **2.4: Create 3 Panel Placeholders**

**Create SetupPanel:**
1. Right-click `ContentArea` ? UI ? Panel
2. Name: `SetupPanel`
3. Color: Slightly lighter gray (R: 30, G: 30, B: 40, A: 200)
4. Add child Text (TMP): "Setup Panel - Ready to add sections"
5. **SetActive: true** (default panel)

**Create InspectPanel:**
1. Right-click `ContentArea` ? UI ? Panel
2. Name: `InspectPanel`
3. Color: Same as SetupPanel
4. Add child Text (TMP): "Inspect Panel - Will show live metrics"
5. **SetActive: false** (hidden by default)

**Create ExportPanel:**
1. Right-click `ContentArea` ? UI ? Panel
2. Name: `ExportPanel`
3. Color: Same as SetupPanel
4. Add child Text (TMP): "Export Panel - Will show export controls"
5. **SetActive: false** (hidden by default)

6. **Save scene** (Ctrl+S)

---

## ?? **Step 3: Wire Components (10 min)**

### **3.1: Add DockModeController**

1. **Select `RightDock` GameObject**
2. **Add Component** ? Search "DockModeController"
3. **In Inspector, wire fields:**
   - `modeDropdown` ? Drag `ModeDropdown`
   - `setupPanel` ? Drag `SetupPanel`
   - `inspectPanel` ? Drag `InspectPanel`
   - `exportPanel` ? Drag `ExportPanel`
4. **Save scene**

---

### **3.2: Add RightDockUI Component**

1. **Select `RightDock` GameObject**
2. **Add Component** ? Search "RightDockUI"
3. **In Inspector, wire fields:**
   - `setupPanel` ? Drag `SetupPanel`
   - `inspectPanel` ? Drag `InspectPanel`
   - `exportPanel` ? Drag `ExportPanel`
   - `dockModeController` ? Drag `RightDock` (DockModeController component)
4. **Save scene**

---

### **3.3: Create UIController (if not exists)**

1. **Check Hierarchy** - Do you already have `UIController`?
   - **Yes:** Skip this step
   - **No:** Continue below

2. **Right-click root** ? Create Empty
3. **Name:** `UIController`
4. **Add Component** ? Search "UIController"
5. **In Inspector:**
   - `workingConfig` ? Leave empty (auto-initializes)
   - `currentPreset` ? Leave empty (set on load)
   - `sections` ? Leave empty (sections register themselves)
6. **Save scene**

---

## ? **Step 4: Test (5 min)**

### **Test 1: Basic Display**

1. **Press Play**
2. **Check:**
   - [ ] RightDock visible on right side
   - [ ] Mode dropdown visible at top
   - [ ] SetupPanel visible (with placeholder text)
   - [ ] No errors in Console

---

### **Test 2: Panel Switching**

1. **In Play mode:**
2. **Click Mode dropdown**
3. **Should show 3 options:**
   - Setup
   - Inspect
   - Export
4. **Select "Inspect"**
   - InspectPanel appears
   - SetupPanel disappears
5. **Select "Export"**
   - ExportPanel appears
6. **Select "Setup"**
   - Back to SetupPanel
7. **Check Console:** No errors

---

## ?? **You Now Have:**

? Clean RightDock with dropdown panel switching  
? No broken tab buttons  
? Ready to add sections incrementally  
? TopBar still working  

---

## ?? **Next Steps: Add Sections Incrementally**

Now that RightDock works, add sections **one at a time**:

### **Priority Order:**

**1. MechanismSummary (5 min)**
- Read-only text showing current mechanism selections
- Always visible at top of SetupPanel

**2. MechanismsSection (15 min)**
- 6 dropdowns for mechanism selection
- Always visible

**3. CoreParametersSection (15 min)**
- 8 curated parameter inputs
- Always visible

**4. Context-Sensitive Sections (20 min each):**
- TopologyDetailsSection (conditional)
- InflowDetailsSection (conditional)
- DiffusionDetailsSection (conditional)
- ViabilityDetailsSection (conditional)

---

## ?? **Current Hierarchy:**

```
UICanvas
??? TopBar ? (WORKING - Don't touch!)
?
??? RightDock ? (NEW - CLEAN!)
    ??? DockModeRow
    ?   ??? ModeLabel ("Mode:")
    ?   ??? ModeDropdown (Setup/Inspect/Export)
    ?
    ??? ContentArea
        ??? SetupPanel (visible)
        ?   ??? PlaceholderText
        ?
        ??? InspectPanel (hidden)
        ?   ??? PlaceholderText
        ?
        ??? ExportPanel (hidden)
            ??? PlaceholderText
```

---

## ?? **Troubleshooting:**

### **Dropdown doesn't show options:**
- Check: DockModeController.Start() is running
- Check: modeDropdown field is wired in Inspector

### **Panels don't switch:**
- Check: All 3 panels wired to DockModeController
- Check: Console for errors

### **RightDock not visible:**
- Check: RectTransform anchor settings (Right-stretch)
- Check: Width = 340, Left = -340, Right = 0

---

## ?? **Ready to Add First Section?**

Once testing passes, follow:
**`RIGHTDOCK_ADD_SECTIONS.md`** (I'll create this next)

---

**Time to complete:** 30 minutes  
**Difficulty:** Easy (just following steps)  
**Result:** Clean, working RightDock with dropdown panel switching! ??
