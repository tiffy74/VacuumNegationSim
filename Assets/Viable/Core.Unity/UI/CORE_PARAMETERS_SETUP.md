# CoreParametersSection Setup Guide

## Overview
Core parameters section contains the most commonly adjusted simulation parameters. Always visible in Setup tab.

---

## UI Layout

```
CoreParametersSection (CollapsibleSection)
?? Header: "Core Parameters"
?? Resource Parameters Group:
?  ?? ResourceGlobalMaxInput (TMP_InputField)
?  ?  Label: "Global Resource Pool Max"
?  ?  Placeholder: "5e7"
?  ?? ResourceRechargeRateInput (TMP_InputField)
?  ?  Label: "Recharge Rate (per step)"
?  ?  Placeholder: "1e6"
?  ?? DecayLossInput (TMP_InputField)
?     Label: "Decay Loss (fraction)"
?     Placeholder: "0.003"
?
?? Cost Parameters Group:
?  ?? MaintCostInput (TMP_InputField)
?  ?  Label: "Maintenance Cost"
?  ?  Placeholder: "1.0"
?  ?? ActivationCostInput (TMP_InputField)
?     Label: "Activation Cost"
?     Placeholder: "5.0"
?
?? Propagation Parameters Group:
?  ?? ExpansionProbabilityInput (TMP_InputField)
?  ?  Label: "Expansion Probability"
?  ?  Placeholder: "0.005"
?  ?? InflowPerCellInput (TMP_InputField)
?  ?  Label: "Inflow per Cell"
?  ?  Placeholder: "1e4"
?  ?? DiffusionRateInput (TMP_InputField)
?     Label: "Diffusion Rate"
?     Placeholder: "0.1"
?
?? AdvancedParamsButton (Button)
   Text: "Advanced Parameters..."
```

---

## Parameter Descriptions

### Resource Parameters:

**1. Global Resource Pool Max** (`ResourceGlobalMax`)
- **Type:** Double (large number, scientific notation)
- **Default:** 5e7 (50 million)
- **Description:** Maximum capacity of the global resource pool
- **Format:** Scientific notation for display (e.g., "5.00E+07")

**2. Recharge Rate** (`ResourceRechargeRate`)
- **Type:** Double (large number, scientific notation)
- **Default:** 1e6 (1 million per step)
- **Description:** Amount of resource added to global pool each step
- **Format:** Scientific notation for display

**3. Decay Loss** (`DecayLoss`)
- **Type:** Double (fraction)
- **Default:** 0.003
- **Description:** Fraction of local resource lost to decay each step
- **Format:** Standard decimal (e.g., "0.0030")
- **Range:** Typically 0.001 to 0.01

### Cost Parameters:

**4. Maintenance Cost** (`MaintCost`)
- **Type:** Double
- **Default:** 1.0
- **Description:** Resource cost per cell to maintain active state
- **Format:** Standard decimal (e.g., "1.00")

**5. Activation Cost** (`ActivationCost`)
- **Type:** Double
- **Default:** 5.0
- **Description:** Resource cost to activate a dormant cell
- **Format:** Standard decimal (e.g., "5.00")
- **Note:** Should be > MaintCost to represent activation barrier

### Propagation Parameters:

**6. Expansion Probability** (`ExpansionProbability`)
- **Type:** Double (probability)
- **Default:** 0.005 (0.5%)
- **Description:** Probability per step that an active cell attempts to expand to neighbor
- **Format:** Standard decimal (e.g., "0.0050")
- **Range:** 0.0 to 1.0

**7. Inflow per Cell** (`InflowPerCell`)
- **Type:** Double (large number, scientific notation)
- **Default:** 1e4 (10,000)
- **Description:** Resource delivered to each active cell per step
- **Format:** Scientific notation if large
- **Note:** Total system inflow = InflowPerCell × ActiveCells

**8. Diffusion Rate** (`DiffusionRate`)
- **Type:** Double (fraction)
- **Default:** 0.1 (10%)
- **Description:** Fraction of resource gradient that diffuses to neighbors
- **Format:** Standard decimal (e.g., "0.100")
- **Range:** 0.0 to 1.0

---

## Unity Setup Steps

### 1. Create Section GameObject

In RightDockUI ? SetupTab ? Content:

```
Right-click Content ? Create Empty
Name: "CoreParametersSection"
Add Component: CoreParametersSection
Add Component: Vertical Layout Group
  - Spacing: 10
  - Child Force Expand: Width = true, Height = false
```

### 2. Create Header (from CollapsibleSection)

```
Right-click CoreParametersSection ? UI ? Panel
Name: "HeaderPanel"
Add Component: Button (for collapse/expand)

Inside HeaderPanel:
- Create TextMeshProUGUI: "HeaderText"
  Text: "Core Parameters"
  Font Size: 16
  Font Style: Bold
```

### 3. Create Content Container

```
Right-click CoreParametersSection ? Create Empty
Name: "ContentPanel"
Add Component: Vertical Layout Group
  - Spacing: 8
  - Padding: Left/Right = 10, Top/Bottom = 10
```

### 4. Create Parameter Groups

Inside ContentPanel, create three groups:

#### **Group 1: Resource Parameters**

```
Create Empty ? Name: "ResourceGroup"
Add Vertical Layout Group

Inside ResourceGroup:
?? ResourceGlobalMaxRow:
?  ?? Label (TextMeshProUGUI): "Global Resource Pool Max"
?  ?? InputField (TMP_InputField)
?
?? ResourceRechargeRateRow:
?  ?? Label: "Recharge Rate (per step)"
?  ?? InputField
?
?? DecayLossRow:
   ?? Label: "Decay Loss (fraction)"
   ?? InputField
```

#### **Group 2: Cost Parameters**

```
Create Empty ? Name: "CostGroup"
Add Vertical Layout Group

Inside CostGroup:
?? MaintCostRow:
?  ?? Label: "Maintenance Cost"
?  ?? InputField
?
?? ActivationCostRow:
   ?? Label: "Activation Cost"
   ?? InputField
```

#### **Group 3: Propagation Parameters**

```
Create Empty ? Name: "PropagationGroup"
Add Vertical Layout Group

Inside PropagationGroup:
?? ExpansionProbabilityRow:
?  ?? Label: "Expansion Probability"
?  ?? InputField
?
?? InflowPerCellRow:
?  ?? Label: "Inflow per Cell"
?  ?? InputField
?
?? DiffusionRateRow:
   ?? Label: "Diffusion Rate"
   ?? InputField
```

### 5. Create Advanced Button

```
Right-click ContentPanel ? UI ? Button - TextMeshPro
Name: "AdvancedParamsButton"
Text: "Advanced Parameters..."
Colors: 
  - Normal: Light gray
  - Highlighted: White
  - Pressed: Dark gray
```

---

## Input Field Configuration

For **all input fields**:

```
TMP_InputField Settings:
- Character Validation: Decimal
- Content Type: Standard
- Line Type: Single Line
- Placeholder Text: (see defaults above)
- Font Size: 14
- Text Color: White
```

### Character Limit:
- No limit (allows scientific notation like "5e7")

### Placeholder Colors:
- Use semi-transparent white (alpha ~0.5)

---

## Inspector Wiring

Select CoreParametersSection GameObject, in Inspector:

### Core Parameter Input Fields:
- `resourceGlobalMaxInput` ? InputField in ResourceGlobalMaxRow
- `resourceRechargeRateInput` ? InputField in ResourceRechargeRateRow
- `decayLossInput` ? InputField in DecayLossRow
- `maintCostInput` ? InputField in MaintCostRow
- `activationCostInput` ? InputField in ActivationCostRow
- `expansionProbabilityInput` ? InputField in ExpansionProbabilityRow
- `inflowPerCellInput` ? InputField in InflowPerCellRow
- `diffusionRateInput` ? InputField in DiffusionRateRow

### Advanced Parameters Button:
- `advancedParamsButton` ? Button GameObject

### Inherited CollapsibleSection Fields:
- `headerObject` ? HeaderPanel GameObject
- `contentObject` ? ContentPanel GameObject
- `toggleButton` ? Button component in HeaderPanel
- `headerText` ? TextMeshProUGUI in HeaderPanel

---

## Scientific Notation Support

The section automatically handles scientific notation:

### Display Format:
```csharp
// Large numbers use scientific notation
5e7 ? "5.00E+07"
1e6 ? "1.00E+06"

// Small numbers use standard notation
0.003 ? "0.0030"
5.0 ? "5.00"
```

### Parsing:
- Supports: "5e7", "5E7", "5.0e+7", "50000000"
- Uses `CultureInfo.InvariantCulture` for consistency
- Validates input in real-time

---

## Behavior

### On Load (Bind):
1. Input fields populate with config values
2. Large numbers formatted in scientific notation
3. Small numbers formatted as decimals

### On Edit:
1. Changes apply immediately to `WorkingScenarioConfig`
2. Invalid input is rejected (keeps previous value)
3. Scientific notation is parsed correctly

### On Apply & Restart:
1. All values validated
2. Applied to `ScenarioDefinition`
3. Simulation restarts with new parameters

---

## Visual Polish

### Grouping:
- Add subtle separators (horizontal lines) between groups
- Use slightly darker background for each group panel

### Labels:
- Font: Bold
- Size: 12
- Color: Light gray
- Align: Left

### Input Fields:
- Background: Dark gray
- Text: White
- Border: Subtle outline
- Hover: Slight brightness increase
- Focus: Border highlight (blue/cyan)

### Advanced Button:
- Position: Bottom of section
- Width: Full width minus padding
- Height: 30px
- Background: Darker than input fields
- Text: "Advanced Parameters..."
- Icon (optional): "..." or gear icon

---

## Testing Checklist

### Display:
- [ ] All 8 parameter inputs visible
- [ ] Labels aligned correctly
- [ ] Scientific notation displays for large numbers
- [ ] Decimal notation displays for small numbers
- [ ] Advanced button at bottom

### Editing:
- [ ] Can type in all input fields
- [ ] Scientific notation accepted (e.g., "5e7")
- [ ] Standard notation accepted (e.g., "50000000")
- [ ] Invalid input rejected
- [ ] Changes apply to config immediately

### Functionality:
- [ ] Bind() populates fields correctly
- [ ] ApplyEdits() reads fields correctly
- [ ] RefreshVisibility() always shows section
- [ ] Advanced button click logs message (placeholder)

### Integration:
- [ ] Loads from preset correctly
- [ ] Saves to WorkingScenarioConfig
- [ ] Works with Apply & Restart flow

---

## Next Steps

After CoreParametersSection is set up:

1. **Step 8:** Implement InspectTab with live metrics
2. **Step 9:** Implement ExportTab configuration
3. **Step 10:** Implement Apply & Restart flow

---

## Notes

- Scientific notation support is critical for large resource values
- Input validation uses `CultureInfo.InvariantCulture` for consistency
- Advanced parameters button is a placeholder (Step 11 implements modal)
- All changes apply immediately to working config (no "Apply" button in section)
- Final application to Engine happens via TopBar "Apply & Restart" button
