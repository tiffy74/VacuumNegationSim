# Building Conditional Detail Sections - Complete Guide

**Prerequisites:** You've added MechanismSummary, MechanismsSection, and CoreParametersSection to SetupPanel.

**Goal:** Add 4 conditional sections that show/hide based on mechanism selections.

**Time:** 20 minutes per section

---

## ?? **What Are Conditional Sections?**

These sections only appear when specific mechanisms are selected:

| Section | Shows When | Contains |
|---------|------------|----------|
| **TopologyDetailsSection** | Topology = "Masked Domain" | Mask shape, radius, etc. |
| **InflowDetailsSection** | Inflow = "Point Sources" | Point source list |
| **DiffusionDetailsSection** | Diffusion = "Anisotropic" | Direction, bias slider |
| **ViabilityDetailsSection** | Viability = "Hysteresis" | ON/OFF thresholds |

---

## ?? **Section 1: TopologyDetailsSection (20 min)**

**Shows when:** Topology dropdown = "Masked Domain"

### **Unity Setup:**

#### **Step 1: Create Section Container**

1. **Right-click `SetupPanel`** ? Create Empty
2. **Name:** `TopologyDetailsSection`
3. **Add Components:**
   - `TopologyDetailsSection` script
   - `Vertical Layout Group` (Spacing: 5)

#### **Step 2: Create Header**

4. **Right-click `TopologyDetailsSection`** ? UI ? Panel
5. **Name:** `HeaderPanel`
6. **Add Layout Element:**
   - Min Height: 30, Preferred Height: 30
7. **Add Button component** to HeaderPanel
8. **Right-click `HeaderPanel`** ? UI ? Text - TextMeshPro
9. **Name:** `HeaderText`
10. **Settings:**
    - Text: "Topology Details"
    - Font Size: 16
    - Bold: ?
    - Color: White

#### **Step 3: Create Content Panel**

11. **Right-click `TopologyDetailsSection`** ? Create Empty
12. **Name:** `ContentPanel`
13. **Add Vertical Layout Group:**
    - Padding: 10 all sides
    - Spacing: 8
    - Child Force Expand: Width ?

#### **Step 4: Add Mask Shape Row**

14. **Right-click `ContentPanel`** ? Create Empty
15. **Name:** `MaskShapeRow`
16. **Add Horizontal Layout Group:**
    - Spacing: 10
17. **Right-click `MaskShapeRow`** ? UI ? Text - TextMeshPro
18. **Name:** `MaskShapeLabel`
    - Text: "Mask Shape:"
    - Add Layout Element: Preferred Width: 100
19. **Right-click `MaskShapeRow`** ? UI ? Dropdown - TextMeshPro
20. **Name:** `MaskShapeDropdown`
    - Add Layout Element: Flexible Width: 1
    - **?? IMPORTANT:** Leave Options EMPTY (Size: 0)
    - **Why?** Script will populate options automatically on Play!
    - **See:** `HOW_TO_POPULATE_DROPDOWNS.md` for details

**Note on Dropdown Options:**
- You DON'T need to manually add options to any dropdown
- All section scripts have `Populate...Dropdown()` methods
- They run in `Start()` and add options automatically
- This keeps options synced with script logic

#### **Step 5: Add Dynamic Parameter Rows**

**Create 4 rows (initially hidden):**

**A) RadiusOuterRow:**
1. Right-click `ContentPanel` ? Create Empty ? Name: `RadiusOuterRow`
2. Add Horizontal Layout Group (Spacing: 10)
3. **SetActive: false** (initially hidden)
4. Add Label: "Outer Radius:", Width: 100
5. Add InputField (TMP): Name: `RadiusOuterInput`, Placeholder: "20.0", Flexible: 1

**B) RadiusInnerRow:**
1. Right-click `ContentPanel` ? Create Empty ? Name: `RadiusInnerRow`
2. Add Horizontal Layout Group (Spacing: 10)
3. **SetActive: false**
4. Add Label: "Inner Radius:", Width: 100
5. Add InputField (TMP): Name: `RadiusInnerInput`, Placeholder: "10.0", Flexible: 1

**C) CorridorWidthRow:**
1. Right-click `ContentPanel` ? Create Empty ? Name: `CorridorWidthRow`
2. Add Horizontal Layout Group (Spacing: 10)
3. **SetActive: false**
4. Add Label: "Corridor Width:", Width: 100
5. Add InputField (TMP): Name: `CorridorWidthInput`, Placeholder: "8.0", Flexible: 1

**D) PercolationProbRow:**
1. Right-click `ContentPanel` ? Create Empty ? Name: `PercolationProbRow`
2. Add Horizontal Layout Group (Spacing: 10)
3. **SetActive: false**
4. Add Label: "Hole Probability:", Width: 100
5. Add InputField (TMP): Name: `PercolationProbInput`, Placeholder: "0.30", Flexible: 1

#### **Step 6: Wire Inspector**

**Select `TopologyDetailsSection`**, drag in Inspector:

**Mask Configuration:**
- `maskShapeDropdown` ? Drag `MaskShapeDropdown`
- `radiusOuterInput` ? Drag `RadiusOuterInput`
- `radiusInnerInput` ? Drag `RadiusInnerInput`
- `corridorWidthInput` ? Drag `CorridorWidthInput`
- `percolationProbInput` ? Drag `PercolationProbInput`

**Field Visibility GameObjects:**
- `radiusOuterRow` ? Drag `RadiusOuterRow` **GameObject** (not the input!)
- `radiusInnerRow` ? Drag `RadiusInnerRow` **GameObject**
- `corridorWidthRow` ? Drag `CorridorWidthRow` **GameObject**
- `percolationProbRow` ? Drag `PercolationProbRow` **GameObject**

**CollapsibleSection fields:**
- `headerObject` ? Drag `HeaderPanel`
- `contentObject` ? Drag `ContentPanel`
- `toggleButton` ? Drag `HeaderPanel` (Button component)
- `headerText` ? Drag `HeaderText`

#### **Step 7: Wire to MechanismsSection**

**? DETAILED VISUAL GUIDE:** See `WIRE_ONMECHANISMCHANGED_GUIDE.md`

**Quick Instructions:**

1. **Select `MechanismsSection` GameObject** in Hierarchy
2. **In Inspector, scroll down** to "Events" header
3. **Find `On Mechanism Changed ()` event**
4. **Click `+` button** to add listener
5. **Drag `TopologyDetailsSection` GameObject** to object slot
6. **Click "No Function" dropdown** ? Select: `TopologyDetailsSection` ? `RefreshVisibility()`
7. **Save scene**

**Note:** You'll add 4 total listeners (one for each conditional section). See detailed guide for visual walkthrough.

### **Test:**
- [ ] Section hidden by default
- [ ] Change Topology to "Masked Domain" ? Section appears
- [ ] Change back to "Full Domain" ? Section disappears
- [ ] Select "Circle" ? Only Outer Radius row visible
- [ ] Select "Ring" ? Outer + Inner Radius visible

---

## ?? **Section 2: InflowDetailsSection (15 min)**

**Shows when:** Inflow dropdown = "Point Sources"

### **Unity Setup:**

#### **Steps 1-3: Create Structure** (same as TopologyDetailsSection)

1. Create `InflowDetailsSection` (Empty)
2. Add script + Vertical Layout Group
3. Create HeaderPanel with Button + Text ("Inflow Details")
4. Create ContentPanel with Vertical Layout Group

#### **Step 4: Add Point Source Display**

**A) Point Source Count Text:**
1. **Right-click `ContentPanel`** ? UI ? Text - TextMeshPro
2. **Name:** `PointSourceCountText`
3. **Settings:**
   - Text: "Point Sources: 0"
   - Font Size: 12
   - Bold: ?

**B) Point Source List Text:**
1. **Right-click `ContentPanel`** ? UI ? Text - TextMeshPro
2. **Name:** `PointSourceListText`
3. **Settings:**
   - Text: "No point sources defined. Click 'Edit Sources' to add."
   - Font Size: 11
   - Wrapping: Enabled
   - Add Layout Element: Min Height: 60

**C) Edit Button:**
1. **Right-click `ContentPanel`** ? UI ? Button - TextMeshPro
2. **Name:** `EditPointSourcesButton`
3. **Text:** "Edit Sources..."
4. **Add Layout Element:** Preferred Height: 30

#### **Step 5: Wire Inspector**

**Select `InflowDetailsSection`**, drag:

**Point Source Display:**
- `pointSourceCountText` ? Drag `PointSourceCountText`
- `pointSourceListText` ? Drag `PointSourceListText`
- `editPointSourcesButton` ? Drag `EditPointSourcesButton`

**CollapsibleSection fields:**
- Header, content, button, text (same as before)

#### **Step 6: Wire to MechanismsSection**

Same pattern as TopologyDetailsSection

### **Test:**
- [ ] Hidden when Inflow = "Uniform Field"
- [ ] Visible when Inflow = "Point Sources"
- [ ] Button clickable (placeholder action for now)

---

## ?? **Section 3: DiffusionDetailsSection (15 min)**

**Shows when:** Diffusion dropdown = "Anisotropic"

### **Unity Setup:**

#### **Steps 1-3: Create Structure** (same pattern)

1. Create `DiffusionDetailsSection`
2. Add script + Vertical Layout Group
3. Create HeaderPanel ("Diffusion Details")
4. Create ContentPanel

#### **Step 4: Add Direction Row**

1. **Right-click `ContentPanel`** ? Create Empty ? Name: `DirectionRow`
2. **Add Horizontal Layout Group** (Spacing: 10)
3. **Add Label:** "Direction:", Width: 100
4. **Add Dropdown (TMP):** Name: `DirectionDropdown`, Flexible: 1
5. **Options:**
   - "North (?)"
   - "East (?)"
   - "South (?)"
   - "West (?)"

#### **Step 5: Add Bias Row**

1. **Right-click `ContentPanel`** ? Create Empty ? Name: `BiasRow`
2. **Add Horizontal Layout Group** (Spacing: 10)
3. **Add Label:** "Bias:", Width: 100
4. **Add Slider:** Name: `BiasSlider`
   - Min: 0, Max: 1, Value: 0.5
   - Whole Numbers: OFF
   - Add Layout Element: Flexible Width: 1
5. **Add Text (TMP):** Name: `BiasValueText`
   - Text: "Bias: 0.50"
   - Add Layout Element: Preferred Width: 80

#### **Step 6: Wire Inspector**

**Select `DiffusionDetailsSection`**, drag:

**Anisotropic Configuration:**
- `directionDropdown` ? Drag `DirectionDropdown`
- `biasSlider` ? Drag `BiasSlider`
- `biasValueText` ? Drag `BiasValueText`

**CollapsibleSection fields** (same pattern)

#### **Step 7: Wire to MechanismsSection**

Same pattern

### **Test:**
- [ ] Hidden when Diffusion = "Moore (8-neighbor)"
- [ ] Visible when Diffusion = "Anisotropic"
- [ ] Slider updates text: "Bias: 0.XX"

---

## ?? **Section 4: ViabilityDetailsSection (15 min)**

**Shows when:** Viability dropdown = "Hysteresis"

### **Unity Setup:**

#### **Steps 1-3: Create Structure** (same pattern)

1. Create `ViabilityDetailsSection`
2. Add script + Vertical Layout Group
3. Create HeaderPanel ("Viability Details")
4. Create ContentPanel

#### **Step 4: Add ON Threshold Row**

1. **Right-click `ContentPanel`** ? Create Empty ? Name: `OnThresholdRow`
2. **Add Horizontal Layout Group** (Spacing: 10)
3. **Add Label:** "ON Threshold:", Width: 120
4. **Add InputField (TMP):** Name: `OnThresholdInput`
   - Content Type: Decimal Number
   - Placeholder: "0.50"
   - Flexible: 1

#### **Step 5: Add OFF Threshold Row**

1. **Right-click `ContentPanel`** ? Create Empty ? Name: `OffThresholdRow`
2. **Add Horizontal Layout Group** (Spacing: 10)
3. **Add Label:** "OFF Threshold:", Width: 120
4. **Add InputField (TMP):** Name: `OffThresholdInput`
   - Content Type: Decimal Number
   - Placeholder: "-0.50"
   - Flexible: 1

#### **Step 6: Add Explanation Text**

1. **Right-click `ContentPanel`** ? UI ? Text - TextMeshPro
2. **Name:** `ExplanationText`
3. **Settings:**
   - Text: "Hysteresis: Cells turn ON when viability > ON threshold, and turn OFF when viability < OFF threshold. ON threshold should be > OFF threshold to prevent flickering."
   - Font Size: 10
   - Color: Light gray (R: 0.7, G: 0.7, B: 0.7)
   - Wrapping: Enabled
   - Add Layout Element: Preferred Height: 60

#### **Step 7: Wire Inspector**

**Select `ViabilityDetailsSection`**, drag:

**Hysteresis Configuration:**
- `onThresholdInput` ? Drag `OnThresholdInput`
- `offThresholdInput` ? Drag `OffThresholdInput`
- `explanationText` ? Drag `ExplanationText`

**CollapsibleSection fields** (same pattern)

#### **Step 8: Wire to MechanismsSection**

Same pattern

### **Test:**
- [ ] Hidden when Viability = "Simple Threshold"
- [ ] Visible when Viability = "Hysteresis"
- [ ] Both inputs accept numbers
- [ ] Explanation text displays correctly

---

## ? **Final Integration Checklist**

After adding all 4 sections:

### **Check Hierarchy:**
```
SetupPanel
??? MechanismSummary
??? MechanismsSection
??? CoreParametersSection
??? TopologyDetailsSection ?
??? InflowDetailsSection ?
??? DiffusionDetailsSection ?
??? ViabilityDetailsSection ?
```

### **Check MechanismsSection Events:**

1. **Select `MechanismsSection`**
2. **Find `OnMechanismChanged` event**
3. **Should have 4 listeners:**
   - TopologyDetailsSection.RefreshVisibility()
   - InflowDetailsSection.RefreshVisibility()
   - DiffusionDetailsSection.RefreshVisibility()
   - ViabilityDetailsSection.RefreshVisibility()

### **Test Full Flow:**

1. **Press Play**
2. **All sections hidden by default** (if default mechanisms don't trigger)
3. **Change Topology to "Masked Domain"** ? TopologyDetailsSection appears
4. **Change Inflow to "Point Sources"** ? InflowDetailsSection appears
5. **Change Diffusion to "Anisotropic"** ? DiffusionDetailsSection appears
6. **Change Viability to "Hysteresis"** ? ViabilityDetailsSection appears
7. **Switch to Inspect panel** (dropdown)
8. **Switch back to Setup** ? All sections still visible
9. **No errors in Console** ?

---

## ?? **Completion Summary**

**You now have:**
- ? 4 conditional sections
- ? Show/hide based on mechanism selections
- ? All wired to MechanismsSection events
- ? Clean, organized SetupPanel
- ? Ready for testing and refinement

**Total time:** ~60-80 minutes for all 4 sections

**Next:** Fine-tune layouts, add value persistence logic, connect to WorkingScenarioConfig

---

**Difficulty:** Medium (repetitive but straightforward)  
**Result:** Complete conditional section system! ??
