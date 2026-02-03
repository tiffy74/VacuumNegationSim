# UI Positioning Fix - Using YOUR Actual GameObjects

**Problem:** The guides use generic names (MainSplit, SimView) but you don't have those in your Unity hierarchy.

**Solution:** Let's find YOUR actual GameObject names and create a custom guide.

---

## ?? **Step 1: Identify Your Current UI Structure (5 min)**

### **In Unity Hierarchy, look for:**

1. **Canvas** (or similar top-level UI element)
   - Note the exact name: `____________________`

2. **Under Canvas, you likely have:**
   - Something for the top bar (controls, preset selector)
     - Note the name: `____________________`
   - Something that contains the simulation view
     - Note the name: `____________________`
   - Something for the right panel (RightDock? UI panel?)
     - Note the name: `____________________`

3. **Inside RightDock (or right panel), you should have:**
   - Mode dropdown row (with dropdown to switch Setup/Inspect/Export)
     - Note the name: `____________________`
   - Content area that holds the panels
     - Note the name: `____________________`
   - SetupPanel, InspectPanel, ExportPanel
     - Note if these exist: ? Yes ? No

---

## ?? **Step 2: Fill Out Your UI Map**

**Based on what you found, fill this out:**

```
Canvas (or your root name: _____________)
?? TopBar (or your name: _____________)
?? [Container for SimView + RightDock] (your name: _____________)
?  ?? [Simulation View] (your name: _____________)
?  ?? RightDock (or your name: _____________)
?     ?? [Mode dropdown row] (your name: _____________)
?     ?? [Content area] (your name: _____________)
?        ?? SetupPanel
?        ?? InspectPanel
?        ?? ExportPanel
```

---

## ?? **Step 3: Apply Positioning Fixes Using YOUR Names**

### **Phase 1: Canvas & Root**

**Find:** `Canvas` (or whatever your root is)

**Settings:**
- Canvas Scaler: UI Scale Mode = `Scale With Screen Size`
- Reference Resolution: `1920 x 1080`, Match: `0.5`

---

### **Phase 2: TopBar**

**Find:** Whatever GameObject contains your preset dropdown, play button, etc.

**Common names to look for:**
- `TopBar`
- `ControlPanel`
- `UIPanel`
- `Header`
- Or something similar

**RectTransform Settings:**
```
Anchor Min: (0, 1)
Anchor Max: (1, 1)
Pivot: (0.5, 1)
Left: 0, Right: 0, Top: 0
Height: 56
```

**Components:**
- Horizontal Layout Group (Padding: 8, Spacing: 8)
- Image (background)

---

### **Phase 3: Container (holds SimView + RightDock)**

**Find:** The GameObject that contains BOTH your simulation view AND RightDock

**Common names:**
- `UICanvas`
- `MainPanel`
- `Content`
- Or might not exist (if SimView and RightDock are direct children of Canvas)

**If it exists:**
```
RectTransform:
Anchor Min: (0, 0)
Anchor Max: (1, 1)
Left: 0, Right: 0, Bottom: 0
Top: 56 ?? (leaves space for TopBar)

Components:
- Horizontal Layout Group (Padding: 8, Spacing: 8)
```

**If it doesn't exist:**
You might need to create it! Or RightDock and SimView might use different positioning.

---

### **Phase 4: Simulation View**

**Find:** Whatever GameObject shows the grid/simulation

**Common names:**
- `SimulationView`
- `GridView`
- `Viewport`
- `Camera`
- Or a GameObject with a Camera component

**If it's inside a Horizontal Layout Group with RightDock:**
```
Components:
- Add Layout Element
  Flexible Width: 1
  Flexible Height: 1
  Min Width: 400
  Min Height: 300
```

**If it's positioned differently:**
- Tell me the structure and I'll give custom instructions

---

### **Phase 5: RightDock**

**Find:** `RightDock` (you said you have this!)

**RectTransform:**
- ?? Do NOT set anchors if parent has Horizontal Layout Group!

**Components:**

**If parent has Horizontal Layout Group:**
```
Add Layout Element:
- Preferred Width: 420
- Flexible Width: 0
- Flexible Height: 1
- Min Width: 420
```

**Always add:**
```
Vertical Layout Group:
- Padding: 10 (all sides)
- Spacing: 10
- Child Force Expand Width: ?

Image (background):
- Color: R: 25, G: 25, B: 35, A: 230
```

**If parent does NOT have Horizontal Layout Group:**
```
RectTransform:
- Anchor Min: (1, 0) ? Top-right corner
- Anchor Max: (1, 1) ? Bottom-right corner
- Pivot: (1, 0.5)
- Right: 0
- Width: 420
- Top: 56 (below TopBar)
- Bottom: 0
```

---

### **Phase 6: Mode Dropdown Row**

**Find:** Inside RightDock, whatever contains the dropdown

**If it exists:**
```
Components:
- Layout Element: Preferred Height: 44, Min Height: 44
- Horizontal Layout Group: Padding 8, Spacing 8
```

**Children (label + dropdown):**
- Label: Layout Element Preferred Width: 50
- Dropdown: Layout Element Preferred Width: 220

**If it doesn't exist yet:**
- Follow `RIGHTDOCK_CLEAN_RESET.md` to create it

---

### **Phase 7: Content Area**

**Find:** Inside RightDock, container for SetupPanel/InspectPanel/ExportPanel

**Common names:**
- `ContentArea`
- `DockContent`
- `PanelContainer`

**Components:**
```
Layout Element:
- Flexible Height: 1 ??
- Min Height: 200
```

---

### **Phase 8: Panels**

**Find:** `SetupPanel`, `InspectPanel`, `ExportPanel` (inside Content Area)

**For EACH panel:**
```
RectTransform:
- Anchor Min: (0, 0)
- Anchor Max: (1, 1)
- Offsets: All 0

Components:
- Vertical Layout Group (Padding: 10, Spacing: 10)

SetActive:
- SetupPanel: true
- InspectPanel: false
- ExportPanel: false
```

---

## ?? **Common Scenarios**

### **Scenario A: You have UICanvas ? TopBar + RightDock directly**

```
UICanvas
?? TopBar ? Fix this
?? RightDock ? Fix this
```

**TopBar:** Anchor to top (0,1) to (1,1), Height 56  
**RightDock:** Anchor to right (1,0) to (1,1), Width 420, Top: 56

**No container needed!** SimView might be a separate GameObject or Camera.

---

### **Scenario B: You have a container for layout**

```
UICanvas
?? TopBar
?? MainPanel (or similar)
   ?? [SimView]
   ?? RightDock
```

**TopBar:** Anchor top, Height 56  
**MainPanel:** Stretch (0,0) to (1,1), Top offset 56, Horizontal Layout Group  
**SimView:** Layout Element Flexible Width 1  
**RightDock:** Layout Element Preferred Width 420  

---

### **Scenario C: You only have RightDock, no SimView GameObject**

**RightDock** might be positioned absolutely:
```
RectTransform:
- Anchor Min: (1, 0)
- Anchor Max: (1, 1)
- Pivot: (1, 0.5)
- Right: 0
- Width: 420
- Top: 56
- Bottom: 0
```

**SimView** is just the Camera rendering to the rest of the screen.

---

## ?? **Step 4: Tell Me Your Structure**

**Please provide:**

1. **Top-level structure:**
   ```
   Canvas
   ?? [Your GameObject 1]: __________
   ?? [Your GameObject 2]: __________
   ?? [Your GameObject 3]: __________
   ```

2. **RightDock structure:**
   ```
   RightDock
   ?? [Child 1]: __________
   ?? [Child 2]: __________
   ?? [Child 3]: __________
   ```

3. **Any GameObjects that show the simulation grid:**
   - Name: __________
   - Has Camera? ? Yes ? No

**Then I'll create a CUSTOM guide with YOUR exact GameObject names!**

---

## ?? **Quick Diagnostic**

**Run this test:**

1. In Unity Hierarchy, expand Canvas (or root)
2. **Count how many immediate children** it has
3. **List them here:**
   - Child 1: __________
   - Child 2: __________
   - Child 3: __________
   - (etc.)

4. **Does RightDock exist?** ? Yes ? No
5. **If yes, where is it?**
   - Direct child of Canvas? ? Yes ? No
   - Inside another container? ? Yes, name: __________

---

## ?? **Next Steps**

**Option A:** Fill out the structure above and I'll create a custom guide

**Option B:** Screenshot your Unity Hierarchy (Canvas expanded) and describe it

**Option C:** List the top 5-10 GameObjects you see under Canvas

**Then I can give you EXACT positioning instructions with YOUR GameObject names!**

---

**Time:** 10 minutes to identify, then we create custom guide

**Result:** Positioning fixes using YOUR actual Unity structure! ??
