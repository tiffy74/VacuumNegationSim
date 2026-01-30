# TopBar UI Setup Instructions

## Unity Hierarchy Setup

### 1. Create TopBar GameObject

1. **In Unity Hierarchy**, right-click `UICanvas` ? Create Empty
2. **Name:** `TopBar`
3. **Add RectTransform settings:**
   - **Anchor:** Top-stretch (top edge, full width)
   - **Height:** 60
   - **Left/Right/Top:** 0
   - **Pivot:** (0.5, 1)

4. **Add Image component** (background):
   - Color: Dark gray (R:30, G:30, B:30, A:220)

5. **Add Horizontal Layout Group:**
   - Padding: 10 all sides
   - Spacing: 10
   - Child Force Expand: Height ?, Width ?
   - Child Alignment: Middle Left

### 2. Add Preset Controls (Left Side)

#### **A. Preset Dropdown**
1. Right-click `TopBar` ? UI ? Dropdown - TextMeshPro
2. **Name:** `PresetDropdown`
3. **Layout Element:**
   - Add Component ? Layout Element
   - Preferred Width: 150

#### **B. Load Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `LoadButton`
3. **Button Text:** "Load"
4. **Layout Element:**
   - Preferred Width: 60

#### **C. Apply & Restart Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `ApplyRestartButton`
3. **Button Text:** "Apply & Restart"
4. **Layout Element:**
   - Preferred Width: 120
5. **Color:** Greenish tint (to indicate primary action)

### 3. Add Spacer (Separator)

1. Right-click `TopBar` ? Create Empty
2. **Name:** `Spacer1`
3. **Layout Element:**
   - Min Width: 20
   - Preferred Width: 20

### 4. Add Run Controls (Center)

#### **A. Play/Pause Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `PlayPauseButton`
3. **Button Text:** "Play"
4. **Layout Element:**
   - Preferred Width: 70

#### **B. Step Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `StepButton`
3. **Button Text:** "Step"
4. **Layout Element:**
   - Preferred Width: 60

#### **C. Restart Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `RestartButton`
3. **Button Text:** "Restart"
4. **Layout Element:**
   - Preferred Width: 70

### 5. Add Spacer

1. Right-click `TopBar` ? Create Empty
2. **Name:** `Spacer2`
3. **Layout Element:**
   - Min Width: 20

### 6. Add Configuration Controls

#### **A. Speed Label + Dropdown**
1. Right-click `TopBar` ? UI ? Text - TextMeshPro
2. **Name:** `SpeedLabel`
3. **Text:** "Speed:"
4. **Layout Element:** Preferred Width: 50

5. Right-click `TopBar` ? UI ? Dropdown - TextMeshPro
6. **Name:** `SpeedDropdown`
7. **Layout Element:** Preferred Width: 120

#### **B. Seed Label + Input**
1. Right-click `TopBar` ? UI ? Text - TextMeshPro
2. **Name:** `SeedLabel`
3. **Text:** "Seed:"
4. **Layout Element:** Preferred Width: 45

5. Right-click `TopBar` ? UI ? Input Field - TextMeshPro
6. **Name:** `SeedInput`
7. **Content Type:** Integer Number
8. **Text:** "42"
9. **Layout Element:** Preferred Width: 70

### 7. Add Spacer

1. Right-click `TopBar` ? Create Empty
2. **Name:** `Spacer3`
3. **Layout Element:**
   - Min Width: 20

### 8. Add Export Button (Right Side)

1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `ExportButton`
3. **Button Text:** "Export Run"
4. **Layout Element:**
   - Preferred Width: 100

---

## Wire Components to TopBarUI Script

### 1. Add TopBarUI Component

1. **Select `TopBar` GameObject**
2. **Add Component** ? Search "TopBarUI"
3. **Click Add**

### 2. Drag Components to Script Fields

**In Inspector, TopBarUI component:**

**Preset Controls:**
- **Preset Dropdown:** Drag `PresetDropdown` ? field
- **Load Button:** Drag `LoadButton` ? field
- **Apply Restart Button:** Drag `ApplyRestartButton` ? field

**Run Controls:**
- **Play Pause Button:** Drag `PlayPauseButton` ? field
- **Play Pause Button Text:** Expand `PlayPauseButton` ? Drag child `Text (TMP)` ? field
- **Step Button:** Drag `StepButton` ? field
- **Restart Button:** Drag `RestartButton` ? field

**Configuration:**
- **Speed Dropdown:** Drag `SpeedDropdown` ? field
- **Seed Input:** Drag `SeedInput` ? field

**Export:**
- **Export Button:** Drag `ExportButton` ? field

### 3. Save Scene

Press **Ctrl+S** to save.

---

## Visual Result

You should see a single row at the top of the screen:

```
??????????????????????????????????????????????????????????????
? [Preset?] [Load] [Apply&Restart] | [Play] [Step] [Restart]?
?   | Speed: [1 step/frame?] Seed: [42] | [Export Run]       ?
??????????????????????????????????????????????????????????????
```

---

## Next Steps

After creating TopBar:
1. Create RightDock with tabs (Setup/Inspect/Export)
2. Wire TopBarUI to UIController
3. Implement preset loading
