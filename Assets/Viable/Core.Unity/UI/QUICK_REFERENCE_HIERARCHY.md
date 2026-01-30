# VIABLE UI - Quick Reference Hierarchy

**Complete GameObject hierarchy for reference**

```
UICanvas
??? TopBar (RectTransform: Top-stretch, Height: 60)
?   ??? PresetDropdown (TMP_Dropdown)
?   ??? LoadButton (Button)
?   ??? ApplyRestartButton (Button)
?   ??? Spacer1 (Empty, Layout Element)
?   ??? PlayPauseButton (Button)
?   ??? StepButton (Button)
?   ??? RestartButton (Button)
?   ??? Spacer2 (Empty, Layout Element)
?   ??? SpeedLabel (TextMeshProUGUI)
?   ??? SpeedDropdown (TMP_Dropdown)
?   ??? SeedLabel (TextMeshProUGUI)
?   ??? SeedInput (TMP_InputField)
?   ??? Spacer3 (Empty, Layout Element)
?   ??? ExportButton (Button)
?
??? RightDock (RectTransform: Right-stretch, Width: 320)
    ??? TabButtonRow (Height: 40, Horizontal Layout)
    ?   ??? SetupTabButton (Button)
    ?   ??? InspectTabButton (Button)
    ?   ??? ExportTabButton (Button)
    ?
    ??? ContentArea (Vertical Layout, NO SCROLL!)
        ??? SetupPanel (Active by default)
        ?   ??? MechanismSummaryPanel (Height: 50)
        ?   ?   ??? MechanismSummaryText (TextMeshProUGUI)
        ?   ?
        ?   ??? SectionsContainer (Vertical Layout)
        ?       ??? MechanismsSection (Always visible)
        ?       ?   ??? HeaderPanel (Button for collapse)
        ?       ?   ?   ??? HeaderText
        ?       ?   ??? ContentPanel
        ?       ?       ??? TopologyRow
        ?       ?       ?   ??? Label
        ?       ?       ?   ??? TopologyDropdown
        ?       ?       ??? BoundaryRow
        ?       ?       ?   ??? Label
        ?       ?       ?   ??? BoundaryDropdown
        ?       ?       ??? InflowRow
        ?       ?       ?   ??? Label
        ?       ?       ?   ??? InflowDropdown
        ?       ?       ??? DiffusionRow
        ?       ?       ?   ??? Label
        ?       ?       ?   ??? DiffusionDropdown
        ?       ?       ??? ViabilityRow
        ?       ?       ?   ??? Label
        ?       ?       ?   ??? ViabilityDropdown
        ?       ?       ??? PhaseSetRow
        ?       ?           ??? Label
        ?       ?           ??? PhaseSetDropdown
        ?       ?
        ?       ??? TopologyDetailsSection (Conditional: Topology = Masked)
        ?       ?   ??? HeaderPanel
        ?       ?   ??? ContentPanel
        ?       ?       ??? MaskShapeRow
        ?       ?       ?   ??? MaskShapeDropdown
        ?       ?       ??? RadiusOuterRow (dynamic visibility)
        ?       ?       ?   ??? RadiusOuterInput
        ?       ?       ??? RadiusInnerRow (dynamic visibility)
        ?       ?       ?   ??? RadiusInnerInput
        ?       ?       ??? CorridorWidthRow (dynamic visibility)
        ?       ?       ?   ??? CorridorWidthInput
        ?       ?       ??? PercolationProbRow (dynamic visibility)
        ?       ?           ??? PercolationProbInput
        ?       ?
        ?       ??? InflowDetailsSection (Conditional: Inflow = PointSources)
        ?       ?   ??? HeaderPanel
        ?       ?   ??? ContentPanel
        ?       ?       ??? PointSourceCountText
        ?       ?       ??? PointSourceListText
        ?       ?       ??? EditPointSourcesButton
        ?       ?
        ?       ??? DiffusionDetailsSection (Conditional: Diffusion = Anisotropic)
        ?       ?   ??? HeaderPanel
        ?       ?   ??? ContentPanel
        ?       ?       ??? DirectionRow
        ?       ?       ?   ??? DirectionDropdown
        ?       ?       ??? BiasRow
        ?       ?       ?   ??? BiasSlider
        ?       ?       ?   ??? BiasValueText
        ?       ?
        ?       ??? ViabilityDetailsSection (Conditional: Viability = Hysteresis)
        ?       ?   ??? HeaderPanel
        ?       ?   ??? ContentPanel
        ?       ?       ??? OnThresholdRow
        ?       ?       ?   ??? OnThresholdInput
        ?       ?       ??? OffThresholdRow
        ?       ?       ?   ??? OffThresholdInput
        ?       ?       ??? ExplanationText
        ?       ?
        ?       ??? CoreParametersSection (Always visible)
        ?           ??? HeaderPanel
        ?           ??? ContentPanel
        ?               ??? ResourceGlobalMaxRow
        ?               ?   ??? ResourceGlobalMaxInput
        ?               ??? ResourceRechargeRateRow
        ?               ?   ??? ResourceRechargeRateInput
        ?               ??? DecayLossRow
        ?               ?   ??? DecayLossInput
        ?               ??? MaintCostRow
        ?               ?   ??? MaintCostInput
        ?               ??? ActivationCostRow
        ?               ?   ??? ActivationCostInput
        ?               ??? ExpansionProbabilityRow
        ?               ?   ??? ExpansionProbabilityInput
        ?               ??? InflowPerCellRow
        ?               ?   ??? InflowPerCellInput
        ?               ??? DiffusionRateRow
        ?               ?   ??? DiffusionRateInput
        ?               ??? AdvancedParamsButton
        ?
        ??? InspectPanel (SetActive: false by default)
        ?   ??? LiveMetricsText (TextMeshProUGUI)
        ?
        ??? ExportPanel (SetActive: false by default)
            ??? ExportInfoText (TextMeshProUGUI)
```

---

## Component Scripts Summary

| GameObject | Script Component | Purpose |
|------------|------------------|---------|
| TopBar | TopBarUI | Top bar controls |
| RightDock | RightDockUI | Tab switching |
| MechanismsSection | MechanismsSection | Mechanism dropdowns |
| TopologyDetailsSection | TopologyDetailsSection | Mask configuration |
| InflowDetailsSection | InflowDetailsSection | Point source display |
| DiffusionDetailsSection | DiffusionDetailsSection | Anisotropic config |
| ViabilityDetailsSection | ViabilityDetailsSection | Hysteresis thresholds |
| CoreParametersSection | CoreParametersSection | 8 curated parameters |

---

## Key Inspector Wiring

### TopBarUI (on TopBar):
- presetDropdown ? PresetDropdown
- loadButton ? LoadButton
- applyRestartButton ? ApplyRestartButton
- playPauseButton ? PlayPauseButton
- playPauseButtonText ? PlayPauseButton/Text (TMP)
- stepButton ? StepButton
- restartButton ? RestartButton
- speedDropdown ? SpeedDropdown
- seedInput ? SeedInput
- exportButton ? ExportButton

### RightDockUI (on RightDock):
- setupTabButton ? SetupTabButton
- inspectTabButton ? InspectTabButton
- exportTabButton ? ExportTabButton
- setupPanel ? SetupPanel
- inspectPanel ? InspectPanel
- exportPanel ? ExportPanel
- setupTabText ? SetupTabButton/Text (TMP)
- inspectTabText ? InspectTabButton/Text (TMP)
- exportTabText ? ExportTabButton/Text (TMP)

### MechanismsSection (on MechanismsSection):
- topologyDropdown ? TopologyDropdown
- boundaryDropdown ? BoundaryDropdown
- inflowDropdown ? InflowDropdown
- diffusionDropdown ? DiffusionDropdown
- viabilityDropdown ? ViabilityDropdown
- phaseSetDropdown ? PhaseSetDropdown
- mechanismSummaryText ? MechanismSummaryText (in MechanismSummaryPanel)
- headerObject ? HeaderPanel
- contentObject ? ContentPanel
- toggleButton ? HeaderPanel (Button)
- headerText ? HeaderText

### TopologyDetailsSection (on TopologyDetailsSection):
- maskShapeDropdown ? MaskShapeDropdown
- radiusOuterInput ? RadiusOuterInput
- radiusInnerInput ? RadiusInnerInput
- corridorWidthInput ? CorridorWidthInput
- percolationProbInput ? PercolationProbInput
- radiusOuterRow ? RadiusOuterRow (GameObject)
- radiusInnerRow ? RadiusInnerRow (GameObject)
- corridorWidthRow ? CorridorWidthRow (GameObject)
- percolationProbRow ? PercolationProbRow (GameObject)
- + CollapsibleSection fields

### InflowDetailsSection (on InflowDetailsSection):
- pointSourceCountText ? PointSourceCountText
- pointSourceListText ? PointSourceListText
- editPointSourcesButton ? EditPointSourcesButton
- + CollapsibleSection fields

### DiffusionDetailsSection (on DiffusionDetailsSection):
- directionDropdown ? DirectionDropdown
- biasSlider ? BiasSlider
- biasValueText ? BiasValueText
- + CollapsibleSection fields

### ViabilityDetailsSection (on ViabilityDetailsSection):
- onThresholdInput ? OnThresholdInput
- offThresholdInput ? OffThresholdInput
- explanationText ? ExplanationText
- + CollapsibleSection fields

### CoreParametersSection (on CoreParametersSection):
- resourceGlobalMaxInput ? ResourceGlobalMaxInput
- resourceRechargeRateInput ? ResourceRechargeRateInput
- decayLossInput ? DecayLossInput
- maintCostInput ? MaintCostInput
- activationCostInput ? ActivationCostInput
- expansionProbabilityInput ? ExpansionProbabilityInput
- inflowPerCellInput ? InflowPerCellInput
- diffusionRateInput ? DiffusionRateInput
- advancedParamsButton ? AdvancedParamsButton
- + CollapsibleSection fields

---

## CollapsibleSection Fields (All Sections)

Every section that inherits from `CollapsibleSection` needs these 4 fields wired:

- `headerObject` ? HeaderPanel GameObject
- `contentObject` ? ContentPanel GameObject
- `toggleButton` ? Button component on HeaderPanel
- `headerText` ? TextMeshProUGUI in HeaderPanel

---

## Visual States

### Tab Switching:
- **Setup Tab Active:** SetupPanel visible, others hidden
- **Inspect Tab Active:** InspectPanel visible, others hidden
- **Export Tab Active:** ExportPanel visible, others hidden

### Section Visibility (Setup Tab):
- **Always Visible:**
  - MechanismSummary
  - MechanismsSection
  - CoreParametersSection

- **Conditional Visibility:**
  - TopologyDetailsSection ? visible when Topology = "Masked Domain"
  - InflowDetailsSection ? visible when Inflow = "Point Sources"
  - DiffusionDetailsSection ? visible when Diffusion = "Anisotropic"
  - ViabilityDetailsSection ? visible when ViabilityRule = "Hysteresis"

### Collapsible States:
- **Collapsed:** ContentPanel hidden, header shows collapse icon
- **Expanded:** ContentPanel visible, header shows expand icon

---

## Common Issues

| Issue | Cause | Fix |
|-------|-------|-----|
| Scroll bars appear | ScrollRect added | Remove ScrollRect component |
| Sections not visible | SetActive = false | Set SetActive = true in Inspector |
| Dropdowns empty | Not populated in Start() | Check script's Populate methods |
| Wiring broken | Inspector refs cleared | Re-drag GameObjects to script fields |
| Tab switching broken | Wrong panels wired | Check RightDockUI inspector refs |
| Sections don't hide/show | RefreshVisibility() not called | Check MechanismsSection.OnMechanismChanged event |

---

**See `MASTER_UI_SETUP_GUIDE.md` for complete step-by-step instructions.**
