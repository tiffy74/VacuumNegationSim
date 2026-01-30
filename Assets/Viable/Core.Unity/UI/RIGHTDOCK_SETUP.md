# RightDock UI Setup Instructions

## ?? CRITICAL: NO SCROLL VIEWS
**Per original design:** This UI uses **fixed height panels with context-sensitive sections**. 
**DO NOT add ScrollRect or Scroll View components anywhere!**

The panel height is fixed, and content is managed by:
1. **Collapsible sections (accordion)** - Users expand/collapse sections manually
2. **Context-sensitive visibility** - Only relevant controls are shown
3. **"Advanced..." buttons** - Complex parameters open in modals (future step)

---

## Unity Hierarchy Setup

### 1. Create RightDock GameObject

1. **In Unity Hierarchy**, right-click `UICanvas` ? Create Empty
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

### 2. Create Tab Button Row

#### **A. Create TabButtonRow Container**

1. Right-click `RightDock` ? Create Empty
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

#### **B. Create Setup Tab Button**

1. Right-click `TabButtonRow` ? UI ? Button - TextMeshPro
2. **Name:** `SetupTabButton`
3. **Button Text:** "Setup"
4. **Layout Element:**
   - Flexible Width: 1

#### **C. Create Inspect Tab Button**

1. Right-click `TabButtonRow` ? UI ? Button - TextMeshPro
2. **Name:** `InspectTabButton`
3. **Button Text:** "Inspect"
4. **Layout Element:**
   - Flexible Width: 1

#### **D. Create Export Tab Button**

1. Right-click `TabButtonRow` ? UI ? Button - TextMeshPro
2. **Name:** `ExportTabButton`
3. **Button Text:** "Export"
4. **Layout Element:**
   - Flexible Width: 1

---

### 3. Create Tab Content Area

#### **A. Create ContentArea Container**

1. Right-click `RightDock` ? Create Empty
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

5. **?? DO NOT add Scroll Rect!** Fixed height only.

---

### 4. Create Setup Panel

#### **A. Create SetupPanel**

1. Right-click `ContentArea` ? Create Empty
2. **Name:** `SetupPanel`
3. **RectTransform:**
   - Anchor: Stretch
   - Leave defaults (layout group will manage)

4. **Add Vertical Layout Group:**
   - Padding: 5 all sides
   - Spacing: 10
   - Child Force Expand: Width ?
   - Child Alignment: Upper Center

#### **B. Add Mechanism Summary (Read-only) - IMPORTANT!**

Per original design: This shows all selected modes at a glance.

1. Right-click `SetupPanel` ? UI ? Panel
2. **Name:** `MechanismSummaryPanel`
3. **Background:** Slightly darker gray
4. **Layout Element:**
   - Min Height: 50
   - Preferred Height: 50

5. Inside panel, create:
   - **Text - TextMeshPro**
   - **Name:** `MechanismSummaryText`
   - **Text (placeholder):** "Topology: FullDomain • Boundary: Wrap • Inflow: UniformField • Diffusion: Moore8 • Viability: Simple"
   - **Font Size:** 11
   - **Color:** Light cyan (R:0.7, G:0.9, B:1.0) - informational color
   - **Alignment:** Left, Top
   - **Wrapping:** Enabled
   - **Overflow:** Ellipsis

**Purpose:** This dynamically updates to show current mechanism selections without needing to expand sections.

#### **C. Add Collapsible Sections Container**

1. Right-click `SetupPanel` ? Create Empty
2. **Name:** `SectionsContainer`
3. **Add Vertical Layout Group:**
   - Spacing: 5
   - Child Force Expand: Width ?

**Note:** Individual sections (MechanismsSection, TopologyDetailsSection, etc.) will be children of this container.

---

### 5. Create Inspect Panel

#### **A. Create InspectPanel**

1. Right-click `ContentArea` ? Create Empty
2. **Name:** `InspectPanel`
3. **Initially:** SetActive = **false** (hidden by default)
4. **RectTransform:**
   - Anchor: Stretch

5. **Add Vertical Layout Group:**
   - Padding: 10 all sides
   - Spacing: 10
   - Child Force Expand: Width ?

#### **B. Add Live Metrics Placeholder**

1. Right-click `InspectPanel` ? UI ? Text - TextMeshPro
2. **Name:** `LiveMetricsText`
3. **Text (placeholder):** "Step: 0\nViable: 0\nActive: 0\nSinks: 0\nResource Global: 0\nMean Viability: 0.00"
4. **Font Size:** 12
5. **Alignment:** Left
6. **Color:** White

**Note:** This will be updated in real-time by InspectTab component (Step 8).

---

### 6. Create Export Panel

#### **A. Create ExportPanel**

1. Right-click `ContentArea` ? Create Empty
2. **Name:** `ExportPanel`
3. **Initially:** SetActive = **false** (hidden by default)
4. **RectTransform:**
   - Anchor: Stretch

5. **Add Vertical Layout Group:**
   - Padding: 10 all sides
   - Spacing: 10
   - Child Force Expand: Width ?

#### **B. Add Export Controls Placeholder**

1. Right-click `ExportPanel` ? UI ? Text - TextMeshPro
2. **Name:** `ExportInfoText`
3. **Text (placeholder):** "Export Level: Standard\nSample Rate: 10\nPath: [Auto]\nLast Export: [None]"
4. **Font Size:** 12
5. **Alignment:** Left

**Note:** This will be replaced with actual export configuration UI (Step 9).

---

## Wire Components to RightDockUI Script

### 1. Add RightDockUI Component

1. **Select `RightDock` GameObject**
2. **Add Component** ? Search "RightDockUI"

### 2. Drag Components to Script Fields

**In Inspector, RightDockUI component:**

**Tab Buttons:**
- **Setup Tab Button:** Drag `SetupTabButton` ? field
- **Inspect Tab Button:** Drag `InspectTabButton` ? field
- **Export Tab Button:** Drag `ExportTabButton` ? field

**Tab Panels:**
- **Setup Panel:** Drag `SetupPanel` ? field
- **Inspect Panel:** Drag `InspectPanel` ? field
- **Export Panel:** Drag `ExportPanel` ? field

**Tab Button Texts:**
- **Setup Tab Text:** Expand `SetupTabButton` ? Drag child `Text (TMP)` ? field
- **Inspect Tab Text:** Expand `InspectTabButton` ? Drag child `Text (TMP)` ? field
- **Export Tab Text:** Expand `ExportTabButton` ? Drag child `Text (TMP)` ? field

**Colors (Optional):**
- **Active Tab Color:** Light blue (R:0.3, G:0.6, B:0.9)
- **Inactive Tab Color:** Gray (R:0.4, G:0.4, B:0.4)

### 3. Save Scene

Press **Ctrl+S** to save.

---

## Visual Result

You should see a fixed-width dock on the right side:

```
??????????????????????
? [Setup][Inspect][Export] ? ? Tab buttons
??????????????????????
? Mechanism Summary: ?
? "Topology: Full •  ?
?  Boundary: Wrap •  ?
?  Inflow: Uniform"  ?
??????????????????????
? ? Mechanisms       ?
?                    ?
? ? Topology Details ?
?                    ?
? ? Core Parameters  ?
?                    ?
?                    ?
?   (Fixed height,   ?
?    NO SCROLL!)     ?
?                    ?
??????????????????????
```

---

## Key Design Principles (Per Original Instructions)

### ? DO:
- Use fixed-height panels
- Show only relevant controls (context-sensitive)
- Use collapsible sections (accordion)
- Keep "Mechanism Summary" visible and updated
- Limit visible parameters to 6-12 curated fields
- Use modals for advanced parameters (no scroll)

### ? DO NOT:
- Add ScrollRect or Scroll View components
- Expose all parameters in the main panel
- Create multi-step wizards
- Apply changes mid-run (only on "Apply & Restart")

---

## Next Steps

After creating RightDock:
1. Implement MechanismsSection with dropdowns and mechanism summary update
2. Implement context-sensitive detail sections (TopologyDetails, InflowDetails, etc.)
3. Implement CoreParametersSection (6-12 curated parameters)
4. Wire to WorkingScenarioConfig
5. Implement InspectTab (Step 8)
6. Implement ExportTab (Step 9)
