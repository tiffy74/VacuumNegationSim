# Stage 13.11: Unity UI Setup - CoreParametersSection

## ?? GOAL

Implement CoreParametersSection with 8 curated simulation parameters in Unity. This section is **always visible** in the Setup tab and contains the most commonly adjusted parameters.

**Prerequisites:** You have TopBar, RightDock, MechanismsSection, and 4 detail sections (Stage 13.10).

---

## ?? What You'll Build

In `SetupPanel` ? `SectionsContainer`, add CoreParametersSection **below** all detail sections.

This section contains:
- 8 parameter input fields (scientific notation supported)
- "Advanced Parameters..." button (opens modal in future stage)
- Always visible (no conditional visibility)
- Curated parameters organized in 3 groups: Resource, Cost, Propagation

---

## ?? Unity Setup

### 1. Create CoreParametersSection GameObject

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `CoreParametersSection`
3. **Add Components:**
   - `CoreParametersSection` script
   - `Vertical Layout Group` (Spacing: 5)

### 2. Create Header + ContentPanel

(Same pattern as other sections)

- **HeaderPanel** (Button, Height: 30)
  - Inside: `HeaderText` (Text TMP): "Core Parameters", Bold, Size: 16

- **ContentPanel** (Vertical Layout Group, Padding: 10, Spacing: 8)

### 3. Add 8 Parameter Input Rows

Inside `ContentPanel`, create 8 rows using this pattern:

**Row Template:**
```
ContentPanel
?? ParameterNameRow (Empty GameObject, Horizontal Layout Group, Spacing: 10)
   ?? Label (Text TMP): "Parameter Name:", Preferred Width: 150
   ?? InputField (TMP_InputField): Name: `ParameterNameInput`
      - Content Type: Standard (allows scientific notation like "5e7")
      - Placeholder: (see defaults below)
      - Preferred Width: Flexible (grows to fill)
```

### Parameter Rows to Create:

#### **Group 1: Resource Parameters**

**Row 1: Global Resource Max**
- Label: "Global Resource Max:"
- InputField: `ResourceGlobalMaxInput`
- Placeholder: "5e7"

**Row 2: Recharge Rate**
- Label: "Recharge Rate:"
- InputField: `ResourceRechargeRateInput`
- Placeholder: "1e6"

**Row 3: Decay Loss**
- Label: "Decay Loss:"
- InputField: `DecayLossInput`
- Placeholder: "0.003"

#### **Group 2: Cost Parameters**

**Row 4: Maintenance Cost**
- Label: "Maintenance Cost:"
- InputField: `MaintCostInput`
- Placeholder: "1.0"

**Row 5: Activation Cost**
- Label: "Activation Cost:"
- InputField: `ActivationCostInput`
- Placeholder: "5.0"

#### **Group 3: Propagation Parameters**

**Row 6: Expansion Probability**
- Label: "Expansion Probability:"
- InputField: `ExpansionProbabilityInput`
- Placeholder: "0.005"

**Row 7: Inflow per Cell**
- Label: "Inflow per Cell:"
- InputField: `InflowPerCellInput`
- Placeholder: "1e4"

**Row 8: Diffusion Rate**
- Label: "Diffusion Rate:"
- InputField: `DiffusionRateInput`
- Placeholder: "0.1"

### 4. Add Advanced Parameters Button

After all parameter rows:

```
ContentPanel
?? AdvancedParamsButton (Button - TextMeshPro)
   Text: "Advanced Parameters..."
   Preferred Width: Full (Layout Element)
   Color: Slightly darker gray (to distinguish from input fields)
```

---

## ?? Wire Inspector

**Select `CoreParametersSection` GameObject**, in Inspector:

### Core Parameter Input Fields:

**Resource Parameters:**
- `resourceGlobalMaxInput` ? Drag `ResourceGlobalMaxInput`
- `resourceRechargeRateInput` ? Drag `ResourceRechargeRateInput`
- `decayLossInput` ? Drag `DecayLossInput`

**Cost Parameters:**
- `maintCostInput` ? Drag `MaintCostInput`
- `activationCostInput` ? Drag `ActivationCostInput`

**Propagation Parameters:**
- `expansionProbabilityInput` ? Drag `ExpansionProbabilityInput`
- `inflowPerCellInput` ? Drag `InflowPerCellInput`
- `diffusionRateInput` ? Drag `DiffusionRateInput`

### Advanced Parameters Button:
- `advancedParamsButton` ? Drag `AdvancedParamsButton`

### CollapsibleSection fields:
- `headerObject` ? Drag `HeaderPanel`
- `contentObject` ? Drag `ContentPanel`
- `toggleButton` ? Drag `HeaderPanel` (Button component)
- `headerText` ? Drag `HeaderText`

---

## ?? Testing

### Manual Tests:

1. **Play scene**
2. **Section always visible** - Should appear regardless of mechanism selections
3. **Input fields accept numbers:**
   - Type `50000000` ? Works
   - Type `5e7` ? Works (scientific notation)
   - Type `0.003` ? Works (decimals)
4. **Placeholder text visible** when fields empty
5. **Advanced button clickable** (logs message in Console for now)

### Value Display Tests:

1. **Load a preset** (if preset system wired)
2. **Check fields populate** with correct values:
   - Large numbers (>1e6) display in scientific notation: "5.00E+07"
   - Small numbers (<1000) display as decimals: "1.00"
3. **Edit a value** ? Check Console log confirms change

### Scientific Notation Tests:

Valid inputs:
- `5e7` ? 50,000,000
- `5E7` ? 50,000,000
- `5.0e+7` ? 50,000,000
- `1e6` ? 1,000,000
- `0.003` ? 0.003
- `5.0` ? 5.0

---

## ?? Parameter Descriptions

(For user reference / tooltips)

| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| **Global Resource Max** | Large number | 5e7 | Maximum capacity of global resource pool |
| **Recharge Rate** | Large number | 1e6 | Resource added to pool each step |
| **Decay Loss** | Fraction | 0.003 | Fraction of local resource lost per step |
| **Maintenance Cost** | Number | 1.0 | Resource cost to maintain active cell |
| **Activation Cost** | Number | 5.0 | Resource cost to activate dormant cell |
| **Expansion Probability** | Probability | 0.005 | Chance per step that cell expands to neighbor |
| **Inflow per Cell** | Large number | 1e4 | Resource delivered to each active cell per step |
| **Diffusion Rate** | Fraction | 0.1 | Fraction of gradient that diffuses to neighbors |

---

## ?? Visual Polish (Optional)

### Add Group Separators:

To visually separate parameter groups:

1. **After Row 3 (Decay Loss):**
   - Create Empty GameObject: `Separator1`
   - Add `Layout Element`: Min Height: 10
   - Add `Image` component: Color = dark gray, Height: 2px

2. **After Row 5 (Activation Cost):**
   - Create `Separator2` (same as above)

### Add Group Labels:

Above each group:

```
ContentPanel
?? ResourceGroupLabel (Text TMP)
?  Text: "Resource Parameters", Font Size: 11, Bold, Color: Light gray
?? (Row 1-3)
?? Separator1
?? CostGroupLabel (Text TMP)
?  Text: "Cost Parameters"
?? (Row 4-5)
?? Separator2
?? PropagationGroupLabel (Text TMP)
?  Text: "Propagation Parameters"
?? (Row 6-8)
```

---

## ? Testing Checklist

### Display:
- [ ] Section always visible
- [ ] All 8 input fields visible
- [ ] Labels aligned correctly (150px width)
- [ ] Input fields stretch to fill remaining width
- [ ] Placeholder text visible when empty
- [ ] Advanced button at bottom

### Input Validation:
- [ ] Can type standard numbers: `1000000`
- [ ] Can type scientific notation: `5e7`
- [ ] Can type decimals: `0.003`
- [ ] Invalid input rejected (non-numeric characters)

### Functionality:
- [ ] Bind() populates fields from config
- [ ] Large numbers format as scientific notation on display
- [ ] Small numbers format as decimals
- [ ] ApplyEdits() reads fields correctly
- [ ] RefreshVisibility() always shows section (always true)
- [ ] Advanced button logs message (placeholder)

### Integration:
- [ ] Loads from preset correctly
- [ ] Changes apply to WorkingScenarioConfig immediately
- [ ] No errors in Console
- [ ] Section collapse/expand works

---

## ?? Troubleshooting

### Issue: Scientific notation not accepted
- **Check:** InputField Content Type = **Standard** (not Decimal or Integer)
- **Fix:** Select InputField ? Inspector ? Content Type ? Standard

### Issue: Values not displaying correctly
- **Check:** Bind() uses FormatScientific() method
- **Check:** TryParseDouble() uses InvariantCulture

### Issue: Advanced button doesn't work
- **Expected:** Button currently logs message only (modal in Stage 13.13)
- **Check:** onClick listener wired in Start()

---

## ?? Summary

**You now have:**
- ? CoreParametersSection with 8 curated parameters
- ? Scientific notation support for large numbers
- ? Decimal support for small numbers
- ? Always visible (no conditional logic)
- ? Advanced button placeholder
- ? Input validation and formatting
- ? Immediate config updates

**Next:** Stage 13.12 - InspectTab with live metrics display

---

**Stage 13.11 Complete!** ? CoreParametersSection implemented with scientific notation support.
