# VIABLE Compact Instrument Panel UI - Complete Setup Guide

**Last Updated:** 2026-01-26  
**Design:** Fixed-height panels, NO SCROLL VIEWS, context-sensitive sections

---

## ?? Overview

This guide provides **step-by-step instructions** to build the complete UI in Unity. Follow the sections in order:

1. **UICanvas Setup** - Root canvas
2. **TopBar** - Always-visible controls (Preset, Run, Speed, Seed, Export)
3. **RightDock** - Collapsible 3-tab panel (Setup | Inspect | Export)
4. **Setup Tab Sections** - Mechanisms, Details, Core Parameters
5. **Inspect Tab** - Live metrics (Step 8)
6. **Export Tab** - Export configuration (Step 9)

---

## ?? CRITICAL DESIGN RULES

### **DO:**
- ? Use **fixed-height panels**
- ? Use **collapsible sections** (accordion)
- ? Show **only relevant controls** (context-sensitive)
- ? Keep **Mechanism Summary** visible and updated
- ? Limit visible parameters to **6-12 curated fields**
- ? Use **modals for advanced parameters** (no scroll in modals either)

### **DO NOT:**
- ? Add **ScrollRect** or **Scroll View** components anywhere!
- ? Expose every parameter in the main panel
- ? Apply changes mid-run (only on "Apply & Restart")

---

# Part 1: UICanvas Setup

## 1.1 Create UICanvas (if not exists)

1. **In Unity Hierarchy**, right-click ? UI ? Canvas
2. **Name:** `UICanvas`
3. **Canvas settings:**
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 × 1080
     - Match: 0.5 (Width/Height balance)

---

# Part 2: TopBar Setup

## 2.1 Create TopBar GameObject

1. **Right-click `UICanvas`** ? Create Empty
2. **Name:** `TopBar`
3. **RectTransform settings:**
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

---

## 2.2 Add TopBar UI Elements (Left to Right)

### **A. Preset Dropdown**
1. Right-click `TopBar` ? UI ? Dropdown - TextMeshPro
2. **Name:** `PresetDropdown`
3. **Add Layout Element:**
   - Preferred Width: 150
4. **Placeholder:** "Select Preset..."

### **B. Load Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `LoadButton`
3. **Button Text:** "Load"
4. **Add Layout Element:**
   - Preferred Width: 60

### **C. Apply & Restart Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `ApplyRestartButton`
3. **Button Text:** "Apply & Restart"
4. **Add Layout Element:**
   - Preferred Width: 130
5. **Color Tint:** Light green (indicates primary action)

### **D. Spacer 1**
1. Right-click `TopBar` ? Create Empty
2. **Name:** `Spacer1`
3. **Add Layout Element:**
   - Min Width: 20
   - Preferred Width: 20

### **E. Play/Pause Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `PlayPauseButton`
3. **Button Text:** "Play"
4. **Add Layout Element:**
   - Preferred Width: 70

### **F. Step Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `StepButton`
3. **Button Text:** "Step"
4. **Add Layout Element:**
   - Preferred Width: 60

### **G. Restart Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `RestartButton`
3. **Button Text:** "Restart"
4. **Add Layout Element:**
   - Preferred Width: 70

### **H. Spacer 2**
1. Right-click `TopBar` ? Create Empty
2. **Name:** `Spacer2`
3. **Add Layout Element:** Min Width: 20

### **I. Speed Label**
1. Right-click `TopBar` ? UI ? Text - TextMeshPro
2. **Name:** `SpeedLabel`
3. **Text:** "Speed:"
4. **Add Layout Element:** Preferred Width: 50

### **J. Speed Dropdown**
1. Right-click `TopBar` ? UI ? Dropdown - TextMeshPro
2. **Name:** `SpeedDropdown`
3. **Add Layout Element:**
   - Preferred Width: 140

### **K. Seed Label**
1. Right-click `TopBar` ? UI ? Text - TextMeshPro
2. **Name:** `SeedLabel`
3. **Text:** "Seed:"
4. **Add Layout Element:** Preferred Width: 45

### **L. Seed Input**
1. Right-click `TopBar` ? UI ? Input Field - TextMeshPro
2. **Name:** `SeedInput`
3. **Content Type:** Integer Number
4. **Text (default):** "42"
5. **Add Layout Element:**
   - Preferred Width: 70

### **M. Spacer 3**
1. Right-click `TopBar` ? Create Empty
2. **Name:** `Spacer3`
3. **Add Layout Element:** Min Width: 20

### **N. Export Button**
1. Right-click `TopBar` ? UI ? Button - TextMeshPro
2. **Name:** `ExportButton`
3. **Button Text:** "Export Run"
4. **Add Layout Element:**
   - Preferred Width: 100

---

## 2.3 Wire TopBarUI Component

1. **Select `TopBar` GameObject**
2. **Add Component** ? Search "TopBarUI" ? Click Add

3. **In Inspector, drag components to fields:**

   **Preset Controls:**
   - `presetDropdown` ? Drag `PresetDropdown`
   - `loadButton` ? Drag `LoadButton`
   - `applyRestartButton` ? Drag `ApplyRestartButton`

   **Run Controls:**
   - `playPauseButton` ? Drag `PlayPauseButton`
   - `playPauseButtonText` ? Expand `PlayPauseButton` ? Drag child `Text (TMP)`
   - `stepButton` ? Drag `StepButton`
   - `restartButton` ? Drag `RestartButton`

   **Configuration:**
   - `speedDropdown` ? Drag `SpeedDropdown`
   - `seedInput` ? Drag `SeedInput`

   **Export:**
   - `exportButton` ? Drag `ExportButton`

4. **Save Scene** (Ctrl+S)

---

# Part 3: RightDock Setup

## 3.1 Create RightDock GameObject

1. **Right-click `UICanvas`** ? Create Empty
2. **Name:** `RightDock`
3. **RectTransform settings:**
   - **Anchor:** Right-stretch (right edge, full height)
   - **Width:** 320
   - **Left:** -320, Right: 0
   - **Top:** 60 (below TopBar), Bottom: 0
   - **Pivot:** (1, 0.5)

4. **Add Image component** (background):
   - Color: Dark gray (R:25, G:25, B:35, A:230)

---

## 3.2 Create Tab Button Row

1. **Right-click `RightDock`** ? Create Empty
2. **Name:** `TabButtonRow`
3. **RectTransform:**
   - Anchor: Top-stretch
   - Height: 40
   - Left/Right/Top: 0

4. **Add Horizontal Layout Group:**
   - Padding: 5 all sides
   - Spacing: 5
   - Child Force Expand: Both ?
   - Child Alignment: Middle Center

### **A. Create Setup Tab Button**
1. Right-click `TabButtonRow` ? UI ? Button - TextMeshPro
2. **Name:** `SetupTabButton`
3. **Button Text:** "Setup"
4. **Add Layout Element:** Flexible Width: 1

### **B. Create Inspect Tab Button**
1. Right-click `TabButtonRow` ? UI ? Button - TextMeshPro
2. **Name:** `InspectTabButton`
3. **Button Text:** "Inspect"
4. **Add Layout Element:** Flexible Width: 1

### **C. Create Export Tab Button**
1. Right-click `TabButtonRow` ? UI ? Button - TextMeshPro
2. **Name:** `ExportTabButton`
3. **Button Text:** "Export"
4. **Add Layout Element:** Flexible Width: 1

---

## 3.3 Create Content Area

1. **Right-click `RightDock`** ? Create Empty
2. **Name:** `ContentArea`
3. **RectTransform:**
   - Anchor: Stretch (all edges)
   - Left/Right: 0
   - Top: 40 (below tab buttons)
   - Bottom: 0

4. **Add Vertical Layout Group:**
   - Padding: 10 all sides
   - Spacing: 10
   - Child Force Expand: Width ?, Height ?
   - Child Alignment: Upper Center

5. **?? DO NOT add Scroll Rect!** (Fixed height only)

---

## 3.4 Create Setup Panel

1. **Right-click `ContentArea`** ? Create Empty
2. **Name:** `SetupPanel`
3. **RectTransform:** Anchor: Stretch (leave defaults)

4. **Add Vertical Layout Group:**
   - Padding: 5 all sides
   - Spacing: 10
   - Child Force Expand: Width ?
   - Child Alignment: Upper Center

---

### 3.4.1 Add Mechanism Summary Panel (IMPORTANT!)

This shows current mechanism selections at a glance.

1. **Right-click `SetupPanel`** ? UI ? Panel
2. **Name:** `MechanismSummaryPanel`
3. **Background Color:** Slightly darker gray (R:20, G:20, B:25)
4. **Add Layout Element:**
   - Min Height: 50
   - Preferred Height: 50

5. **Right-click `MechanismSummaryPanel`** ? UI ? Text - TextMeshPro
6. **Name:** `MechanismSummaryText`
7. **Text (placeholder):**
   ```
   Topology: FullDomain • Boundary: Wrap • Inflow: UniformField • Diffusion: Moore8 • Viability: Simple
   ```
8. **Font Size:** 11
9. **Color:** Light cyan (R:0.7, G:0.9, B:1.0) - informational color
10. **Alignment:** Left, Top
11. **Wrapping:** Enabled
12. **Overflow:** Ellipsis

---

### 3.4.2 Create Sections Container

1. **Right-click `SetupPanel`** ? Create Empty
2. **Name:** `SectionsContainer`
3. **Add Vertical Layout Group:**
   - Spacing: 5
   - Child Force Expand: Width ?

**Note:** All Setup Tab sections will be children of `SectionsContainer`.

---

## 3.5 Create Inspect Panel

1. **Right-click `ContentArea`** ? Create Empty
2. **Name:** `InspectPanel`
3. **SetActive:** **false** (hidden by default)
4. **RectTransform:** Anchor: Stretch

5. **Add Vertical Layout Group:**
   - Padding: 10 all sides
   - Spacing: 10
   - Child Force Expand: Width ?

6. **Right-click `InspectPanel`** ? UI ? Text - TextMeshPro
7. **Name:** `LiveMetricsText`
8. **Text (placeholder):**
   ```
   Step: 0
   Viable: 0
   Active: 0
   Sinks: 0
   Resource Global: 0
   Mean Viability: 0.00
   ```
9. **Font Size:** 12
10. **Color:** White

---

## 3.6 Create Export Panel

1. **Right-click `ContentArea`** ? Create Empty
2. **Name:** `ExportPanel`
3. **SetActive:** **false** (hidden by default)
4. **RectTransform:** Anchor: Stretch

5. **Add Vertical Layout Group:**
   - Padding: 10 all sides
   - Spacing: 10
   - Child Force Expand: Width ?

6. **Right-click `ExportPanel`** ? UI ? Text - TextMeshPro
7. **Name:** `ExportInfoText`
8. **Text (placeholder):**
   ```
   Export Level: Standard
   Sample Rate: 10
   Path: [Auto]
   Last Export: [None]
   ```
9. **Font Size:** 12

---

## 3.7 Wire RightDockUI Component

1. **Select `RightDock` GameObject**
2. **Add Component** ? Search "RightDockUI" ? Click Add

3. **In Inspector, drag components:**

   **Tab Buttons:**
   - `setupTabButton` ? Drag `SetupTabButton`
   - `inspectTabButton` ? Drag `InspectTabButton`
   - `exportTabButton` ? Drag `ExportTabButton`

   **Tab Panels:**
   - `setupPanel` ? Drag `SetupPanel`
   - `inspectPanel` ? Drag `InspectPanel`
   - `exportPanel` ? Drag `ExportPanel`

   **Tab Button Texts:**
   - `setupTabText` ? Expand `SetupTabButton` ? Drag `Text (TMP)`
   - `inspectTabText` ? Expand `InspectTabButton` ? Drag `Text (TMP)`
   - `exportTabText` ? Expand `ExportTabButton` ? Drag `Text (TMP)`

   **Colors (Optional):**
   - Active Tab Color: Light blue (R:0.3, G:0.6, B:0.9)
   - Inactive Tab Color: Gray (R:0.4, G:0.4, B:0.4)

4. **Save Scene** (Ctrl+S)

---

# Part 4: Setup Tab Sections

All sections are children of **`SetupPanel` ? `SectionsContainer`**.

---

## 4.1 MechanismsSection (Always Visible)

### 4.1.1 Create GameObject

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `MechanismsSection`
3. **Add Component:** MechanismsSection
4. **Add Vertical Layout Group:**
   - Spacing: 5
   - Child Force Expand: Width ?

### 4.1.2 Create Header Panel (Collapsible)

1. **Right-click `MechanismsSection`** ? UI ? Panel
2. **Name:** `HeaderPanel`
3. **Add Button component** (for collapse/expand)
4. **Add Layout Element:** Min Height: 30, Preferred Height: 30

5. **Inside `HeaderPanel`:**
   - Right-click ? UI ? Text - TextMeshPro
   - **Name:** `HeaderText`
   - **Text:** "Mechanisms"
   - **Font Size:** 16
   - **Font Style:** Bold
   - **Color:** White

### 4.1.3 Create Content Panel

1. **Right-click `MechanismsSection`** ? Create Empty
2. **Name:** `ContentPanel`
3. **Add Vertical Layout Group:**
   - Padding: 10 all sides
   - Spacing: 8
   - Child Force Expand: Width ?

### 4.1.4 Add Mechanism Dropdowns

Inside `ContentPanel`, create 6 dropdown rows:

#### **Row 1: Topology**
1. Right-click `ContentPanel` ? Create Empty ? Name: `TopologyRow`
2. Add Horizontal Layout Group (Spacing: 10)
3. **Inside `TopologyRow`:**
   - Text (TMP): Label "Topology:", Preferred Width: 100
   - Dropdown (TMP): Name `TopologyDropdown`, Flexible Width: 1
   - Options: "Full Domain", "Masked Domain"

#### **Row 2: Boundary**
1. Right-click `ContentPanel` ? Create Empty ? Name: `BoundaryRow`
2. Add Horizontal Layout Group
3. **Inside `BoundaryRow`:**
   - Label: "Boundary:", Width: 100
   - Dropdown: `BoundaryDropdown`
   - Options: "Closed (Reflective)", "Open (Absorbing)", "Wrap (Periodic)"

#### **Row 3: Inflow**
1. Right-click `ContentPanel` ? Create Empty ? Name: `InflowRow`
2. Add Horizontal Layout Group
3. **Inside `InflowRow`:**
   - Label: "Inflow:", Width: 100
   - Dropdown: `InflowDropdown`
   - Options: "Uniform Field", "Point Sources", "Edge Sources"

#### **Row 4: Diffusion**
1. Right-click `ContentPanel` ? Create Empty ? Name: `DiffusionRow`
2. Add Horizontal Layout Group
3. **Inside `DiffusionRow`:**
   - Label: "Diffusion:", Width: 100
   - Dropdown: `DiffusionDropdown`
   - Options: "Von Neumann (4-neighbor)", "Moore (8-neighbor)", "Anisotropic"

#### **Row 5: Viability**
1. Right-click `ContentPanel` ? Create Empty ? Name: `ViabilityRow`
2. Add Horizontal Layout Group
3. **Inside `ViabilityRow`:**
   - Label: "Viability:", Width: 100
   - Dropdown: `ViabilityDropdown`
   - Options: "Simple Threshold", "Hysteresis"

#### **Row 6: Phase Set**
1. Right-click `ContentPanel` ? Create Empty ? Name: `PhaseSetRow`
2. Add Horizontal Layout Group
3. **Inside `PhaseSetRow`:**
   - Label: "Phase Set:", Width: 100
   - Dropdown: `PhaseSetDropdown`
   - Options: "Standard", "Custom (Advanced)"

### 4.1.5 Wire MechanismsSection Component

**Select `MechanismsSection` GameObject**, in Inspector:

- **Mechanism Dropdowns:**
  - `topologyDropdown` ? Drag `TopologyDropdown`
  - `boundaryDropdown` ? Drag `BoundaryDropdown`
  - `inflowDropdown` ? Drag `InflowDropdown`
  - `diffusionDropdown` ? Drag `DiffusionDropdown`
  - `viabilityDropdown` ? Drag `ViabilityDropdown`
  - `phaseSetDropdown` ? Drag `PhaseSetDropdown`

- **External References:**
  - `mechanismSummaryText` ? Drag `MechanismSummaryText` (from MechanismSummaryPanel)

- **CollapsibleSection fields:**
  - `headerObject` ? Drag `HeaderPanel`
  - `contentObject` ? Drag `ContentPanel`
  - `toggleButton` ? Drag `HeaderPanel` (Button component)
  - `headerText` ? Drag `HeaderText`

---

## 4.2 TopologyDetailsSection (Conditional)

**Shows when:** Topology = "Masked Domain"

### 4.2.1 Create GameObject

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `TopologyDetailsSection`
3. **Add Component:** TopologyDetailsSection
4. **Add Vertical Layout Group:** Spacing: 5

### 4.2.2 Create Header + Content (same pattern as MechanismsSection)

- Header with "Topology Details" text
- ContentPanel with Vertical Layout Group

### 4.2.3 Add Mask Shape Dropdown

Inside ContentPanel:
1. Create Row: `MaskShapeRow`
2. Label: "Mask Shape:", Dropdown: `MaskShapeDropdown`
3. Options: "Rectangle", "Circle", "Ring", "Corridor", "Percolation Holes"

### 4.2.4 Add Dynamic Parameter Rows (GameObjects for visibility control)

Create 4 rows (initially all hidden):

#### **RadiusOuterRow:**
- Label: "Outer Radius:"
- InputField: `RadiusOuterInput`, Placeholder: "20.0"

#### **RadiusInnerRow:**
- Label: "Inner Radius:"
- InputField: `RadiusInnerInput`, Placeholder: "10.0"

#### **CorridorWidthRow:**
- Label: "Corridor Width:"
- InputField: `CorridorWidthInput`, Placeholder: "8.0"

#### **PercolationProbRow:**
- Label: "Hole Probability:"
- InputField: `PercolationProbInput`, Placeholder: "0.30"

### 4.2.5 Wire TopologyDetailsSection Component

**Select `TopologyDetailsSection` GameObject**, in Inspector:

- **Mask Configuration:**
  - `maskShapeDropdown` ? Drag `MaskShapeDropdown`
  - `radiusOuterInput` ? Drag `RadiusOuterInput`
  - `radiusInnerInput` ? Drag `RadiusInnerInput`
  - `corridorWidthInput` ? Drag `CorridorWidthInput`
  - `percolationProbInput` ? Drag `PercolationProbInput`

- **Field Visibility GameObjects:**
  - `radiusOuterRow` ? Drag `RadiusOuterRow` GameObject
  - `radiusInnerRow` ? Drag `RadiusInnerRow` GameObject
  - `corridorWidthRow` ? Drag `CorridorWidthRow` GameObject
  - `percolationProbRow` ? Drag `PercolationProbRow` GameObject

- **CollapsibleSection fields:** (header, content, button, text)

---

## 4.3 InflowDetailsSection (Conditional)

**Shows when:** Inflow = "Point Sources"

### 4.3.1 Create GameObject

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `InflowDetailsSection`
3. **Add Component:** InflowDetailsSection
4. **Add Vertical Layout Group**

### 4.3.2 Create Header + Content

- Header: "Inflow Details"
- ContentPanel with Vertical Layout Group

### 4.3.3 Add Point Source Display

Inside ContentPanel:

1. **Text (TMP):** `PointSourceCountText`
   - Text: "Point Sources: 0"
   - Font Size: 12, Bold

2. **Text (TMP):** `PointSourceListText`
   - Text: "No point sources defined. Click 'Edit Sources' to add."
   - Font Size: 11
   - Wrapping: Enabled
   - Min Height: 60

3. **Button:** `EditPointSourcesButton`
   - Text: "Edit Sources..."
   - Preferred Width: Full

### 4.3.4 Wire InflowDetailsSection Component

**Select `InflowDetailsSection` GameObject**, in Inspector:

- **Point Source Display:**
  - `pointSourceCountText` ? Drag `PointSourceCountText`
  - `pointSourceListText` ? Drag `PointSourceListText`
  - `editPointSourcesButton` ? Drag `EditPointSourcesButton`

- **CollapsibleSection fields**

---

## 4.4 DiffusionDetailsSection (Conditional)

**Shows when:** Diffusion = "Anisotropic"

### 4.4.1 Create GameObject

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `DiffusionDetailsSection`
3. **Add Component:** DiffusionDetailsSection
4. **Add Vertical Layout Group**

### 4.4.2 Create Header + Content

- Header: "Diffusion Details"
- ContentPanel

### 4.4.3 Add Anisotropic Controls

Inside ContentPanel:

1. **Row 1: Direction Dropdown**
   - Label: "Direction:"
   - Dropdown: `DirectionDropdown`
   - Options: "North (?)", "East (?)", "South (?)", "West (?)"

2. **Row 2: Bias Slider**
   - Label: "Bias:"
   - Slider: `BiasSlider` (Min: 0, Max: 1, Value: 0.5)
   - Text: `BiasValueText` ("Bias: 0.50")

### 4.4.4 Wire DiffusionDetailsSection Component

**Select `DiffusionDetailsSection` GameObject**, in Inspector:

- **Anisotropic Configuration:**
  - `directionDropdown` ? Drag `DirectionDropdown`
  - `biasSlider` ? Drag `BiasSlider`
  - `biasValueText` ? Drag `BiasValueText`

- **CollapsibleSection fields**

---

## 4.5 ViabilityDetailsSection (Conditional)

**Shows when:** ViabilityRule = "Hysteresis"

### 4.5.1 Create GameObject

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `ViabilityDetailsSection`
3. **Add Component:** ViabilityDetailsSection
4. **Add Vertical Layout Group**

### 4.5.2 Create Header + Content

- Header: "Viability Details"
- ContentPanel

### 4.5.3 Add Hysteresis Controls

Inside ContentPanel:

1. **Row 1: ON Threshold**
   - Label: "ON Threshold:"
   - InputField: `OnThresholdInput`, Placeholder: "0.50"

2. **Row 2: OFF Threshold**
   - Label: "OFF Threshold:"
   - InputField: `OffThresholdInput`, Placeholder: "-0.50"

3. **Explanation Text (TMP):** `ExplanationText`
   - Text:
     ```
     Hysteresis: Cells turn ON when viability > ON threshold,
     and turn OFF when viability < OFF threshold.
     ON threshold should be > OFF threshold to prevent flickering.
     ```
   - Font Size: 10
   - Color: Light gray (dimmed)
   - Wrapping: Enabled

### 4.5.4 Wire ViabilityDetailsSection Component

**Select `ViabilityDetailsSection` GameObject**, in Inspector:

- **Hysteresis Configuration:**
  - `onThresholdInput` ? Drag `OnThresholdInput`
  - `offThresholdInput` ? Drag `OffThresholdInput`
  - `explanationText` ? Drag `ExplanationText`

- **CollapsibleSection fields**

---

## 4.6 CoreParametersSection (Always Visible)

### 4.6.1 Create GameObject

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `CoreParametersSection`
3. **Add Component:** CoreParametersSection
4. **Add Vertical Layout Group**

### 4.6.2 Create Header + Content

- Header: "Core Parameters"
- ContentPanel

### 4.6.3 Add 8 Parameter Input Rows

Inside ContentPanel, create 8 rows (same pattern):

#### **Resource Parameters:**

**Row 1: Global Resource Pool Max**
- Label: "Global Resource Max:", Width: 150
- InputField: `ResourceGlobalMaxInput`, Placeholder: "5e7"

**Row 2: Recharge Rate**
- Label: "Recharge Rate:", Width: 150
- InputField: `ResourceRechargeRateInput`, Placeholder: "1e6"

**Row 3: Decay Loss**
- Label: "Decay Loss:", Width: 150
- InputField: `DecayLossInput`, Placeholder: "0.003"

#### **Cost Parameters:**

**Row 4: Maintenance Cost**
- Label: "Maintenance Cost:", Width: 150
- InputField: `MaintCostInput`, Placeholder: "1.0"

**Row 5: Activation Cost**
- Label: "Activation Cost:", Width: 150
- InputField: `ActivationCostInput`, Placeholder: "5.0"

#### **Propagation Parameters:**

**Row 6: Expansion Probability**
- Label: "Expansion Probability:", Width: 150
- InputField: `ExpansionProbabilityInput`, Placeholder: "0.005"

**Row 7: Inflow per Cell**
- Label: "Inflow per Cell:", Width: 150
- InputField: `InflowPerCellInput`, Placeholder: "1e4"

**Row 8: Diffusion Rate**
- Label: "Diffusion Rate:", Width: 150
- InputField: `DiffusionRateInput`, Placeholder: "0.1"

### 4.6.4 Add Advanced Parameters Button

1. **Button:** `AdvancedParamsButton`
2. **Text:** "Advanced Parameters..."
3. **Preferred Width:** Full
4. **Color:** Slightly darker background

### 4.6.5 Wire CoreParametersSection Component

**Select `CoreParametersSection` GameObject**, in Inspector:

- **Core Parameter Input Fields:**
  - `resourceGlobalMaxInput` ? Drag `ResourceGlobalMaxInput`
  - `resourceRechargeRateInput` ? Drag `ResourceRechargeRateInput`
  - `decayLossInput` ? Drag `DecayLossInput`
  - `maintCostInput` ? Drag `MaintCostInput`
  - `activationCostInput` ? Drag `ActivationCostInput`
  - `expansionProbabilityInput` ? Drag `ExpansionProbabilityInput`
  - `inflowPerCellInput` ? Drag `InflowPerCellInput`
  - `diffusionRateInput` ? Drag `DiffusionRateInput`

- **Advanced Parameters Button:**
  - `advancedParamsButton` ? Drag `AdvancedParamsButton`

- **CollapsibleSection fields**

---

# Part 5: Final Integration

## 5.1 Wire Sections to RightDockUI (Optional)

If RightDockUI manages sections, add array:

**Select `RightDock` GameObject**, in Inspector:
- Find "Setup Tab Sections" array
- Size: 6
- Element 0: Drag `MechanismsSection`
- Element 1: Drag `TopologyDetailsSection`
- Element 2: Drag `InflowDetailsSection`
- Element 3: Drag `DiffusionDetailsSection`
- Element 4: Drag `ViabilityDetailsSection`
- Element 5: Drag `CoreParametersSection`

---

# Part 6: Testing Checklist

## TopBar:
- [ ] All buttons visible and clickable
- [ ] Preset dropdown works
- [ ] Seed input accepts numbers
- [ ] Speed dropdown populates

## RightDock:
- [ ] Tab buttons switch panels
- [ ] Only one panel visible at a time
- [ ] Active tab highlighted
- [ ] NO SCROLL BARS appear

## Setup Tab:
- [ ] Mechanism Summary displays and updates
- [ ] MechanismsSection always visible
- [ ] TopologyDetailsSection shows/hides based on Topology dropdown
- [ ] InflowDetailsSection shows/hides based on Inflow dropdown
- [ ] DiffusionDetailsSection shows/hides based on Diffusion dropdown
- [ ] ViabilityDetailsSection shows/hides based on Viability dropdown
- [ ] CoreParametersSection always visible
- [ ] All collapsible sections expand/collapse

## Inspect Tab:
- [ ] Shows placeholder text (Step 8 will populate)

## Export Tab:
- [ ] Shows placeholder text (Step 9 will populate)

---

# Troubleshooting

## Issue: Sections not appearing
- **Check:** GameObject SetActive = true
- **Check:** Parent panel (SetupPanel) is active
- **Check:** Vertical Layout Group exists

## Issue: Scroll bars appearing
- **Fix:** Remove any Scroll Rect components
- **Fix:** Ensure panels have fixed height, not "fit content"

## Issue: Sections not hiding/showing
- **Check:** RefreshVisibility() logic in section scripts
- **Check:** MechanismsSection.OnMechanismChanged event wired

## Issue: Inspector fields not wired
- **Fix:** Drag GameObjects to serialized fields in Inspector
- **Check:** Component scripts are attached

---

# Summary

You now have:
- ? TopBar with all controls
- ? RightDock with 3 tabs (NO SCROLL!)
- ? Mechanism Summary (auto-updating)
- ? MechanismsSection (6 dropdowns)
- ? 4 context-sensitive detail sections
- ? CoreParametersSection (8 curated params)
- ? Collapsible section framework
- ? Fixed-height panels throughout

**Next Steps:**
- Step 8: Implement InspectTab live metrics
- Step 9: Implement ExportTab configuration
- Step 10: Implement Apply & Restart flow
- Step 11-12: Modals (Advanced params, Point sources)
- Step 13-14: Integration & testing

---

**END OF SETUP GUIDE**
