# Stage 13.10 (Continued): Missing Detail Sections

## ?? GOAL

Add the 2 remaining context-sensitive detail sections that you haven't built yet:
- **DiffusionDetailsSection** - Shows when Diffusion = "Anisotropic"
- **ViabilityDetailsSection** - Shows when ViabilityRule = "Hysteresis"

**Prerequisites:** You already have TopologyDetailsSection, InflowDetailsSection, and CoreParametersSection in SetupPanel.

---

## ?? Section 1: DiffusionDetailsSection

**Shows when:** Diffusion dropdown = "Anisotropic"

### Unity Setup:

1. **In Unity Hierarchy**, find: `RightDock` ? `ContentArea` ? `SetupPanel`
2. **Right-click `SetupPanel`** ? Create Empty
3. **Name:** `DiffusionDetailsSection`
4. **Drag to position:** Between `InflowDetailsSection` and `CoreParametersSection`

5. **Add Components:**
   - `DiffusionDetailsSection` script
   - `Vertical Layout Group` (Spacing: 5, Child Force Expand Width: ?)

6. **Create Header:**
   - Right-click `DiffusionDetailsSection` ? UI ? Panel
   - Name: `HeaderPanel`, Min Height: 30, Preferred Height: 30
   - Add `Button` component to HeaderPanel
   - Inside HeaderPanel: Right-click ? UI ? Text - TextMeshPro
     - Name: `HeaderText`
     - Text: "Diffusion Details"
     - Font Size: 16, Bold, Color: White

7. **Create ContentPanel:**
   - Right-click `DiffusionDetailsSection` ? Create Empty
   - Name: `ContentPanel`
   - Add `Vertical Layout Group` (Padding: 10 all sides, Spacing: 8)

8. **Add DirectionRow:**
   ```
   ContentPanel
   ?? DirectionRow (Create Empty, Add Horizontal Layout Group, Spacing: 10)
      ?? DirectionLabel (Text TMP)
      ?  Text: "Direction:"
      ?  Add Layout Element: Preferred Width: 100
      ?
      ?? DirectionDropdown (UI ? Dropdown - TextMeshPro)
         Add Layout Element: Flexible Width: 1
         Options (manually add):
         - "North (?)"
         - "East (?)"
         - "South (?)"
         - "West (?)"
   ```

9. **Add BiasRow:**
   ```
   ContentPanel
   ?? BiasRow (Create Empty, Add Horizontal Layout Group, Spacing: 10)
      ?? BiasLabel (Text TMP)
      ?  Text: "Bias:"
      ?  Preferred Width: 100
      ?
      ?? BiasSlider (UI ? Slider)
      ?  Min Value: 0
      ?  Max Value: 1
      ?  Value: 0.5
      ?  Whole Numbers: Off (unchecked)
      ?  Add Layout Element: Flexible Width: 1
      ?
      ?? BiasValueText (Text TMP)
         Text: "Bias: 0.50"
         Preferred Width: 80
   ```

### Wire Inspector:

**Select `DiffusionDetailsSection` GameObject**, in Inspector:

**Anisotropic Configuration:**
- `directionDropdown` ? Drag `DirectionDropdown`
- `biasSlider` ? Drag `BiasSlider`
- `biasValueText` ? Drag `BiasValueText`

**CollapsibleSection fields:**
- `headerObject` ? Drag `HeaderPanel`
- `contentObject` ? Drag `ContentPanel`
- `toggleButton` ? Drag `HeaderPanel` (Button component)
- `headerText` ? Drag `HeaderText`

### Test:

1. Play scene
2. Find Mechanisms dropdown "Diffusion"
3. Select "Anisotropic" ? DiffusionDetailsSection should appear
4. Select "Moore (8-neighbor)" ? Section should disappear
5. Select "Anisotropic" again ? Section reappears
6. Change direction dropdown ? Should update config
7. Move bias slider ? BiasValueText updates to "Bias: 0.XX"

---

## ?? Section 2: ViabilityDetailsSection

**Shows when:** Viability dropdown = "Hysteresis"

### Unity Setup:

1. **In Unity Hierarchy**, find: `RightDock` ? `ContentArea` ? `SetupPanel`
2. **Right-click `SetupPanel`** ? Create Empty
3. **Name:** `ViabilityDetailsSection`
4. **Drag to position:** Between `DiffusionDetailsSection` and `CoreParametersSection`

5. **Add Components:**
   - `ViabilityDetailsSection` script
   - `Vertical Layout Group` (Spacing: 5, Child Force Expand Width: ?)

6. **Create Header:**
   - Right-click `ViabilityDetailsSection` ? UI ? Panel
   - Name: `HeaderPanel`, Min Height: 30, Preferred Height: 30
   - Add `Button` component
   - Inside: Text TMP named `HeaderText`, Text: "Viability Details", Bold, Size: 16

7. **Create ContentPanel:**
   - Right-click `ViabilityDetailsSection` ? Create Empty
   - Name: `ContentPanel`
   - Add `Vertical Layout Group` (Padding: 10, Spacing: 8)

8. **Add OnThresholdRow:**
   ```
   ContentPanel
   ?? OnThresholdRow (Create Empty, Add Horizontal Layout Group, Spacing: 10)
      ?? OnThresholdLabel (Text TMP)
      ?  Text: "ON Threshold:"
      ?  Preferred Width: 120
      ?
      ?? OnThresholdInput (UI ? Input Field - TextMeshPro)
         Content Type: Decimal Number
         Placeholder: "0.50"
         Add Layout Element: Flexible Width: 1
   ```

9. **Add OffThresholdRow:**
   ```
   ContentPanel
   ?? OffThresholdRow (Create Empty, Add Horizontal Layout Group, Spacing: 10)
      ?? OffThresholdLabel (Text TMP)
      ?  Text: "OFF Threshold:"
      ?  Preferred Width: 120
      ?
      ?? OffThresholdInput (UI ? Input Field - TextMeshPro)
         Content Type: Decimal Number
         Placeholder: "-0.50"
         Add Layout Element: Flexible Width: 1
   ```

10. **Add ExplanationText:**
    ```
    ContentPanel
    ?? ExplanationText (UI ? Text - TextMeshPro)
       Text: 
       "Hysteresis: Cells turn ON when viability > ON threshold,
        and turn OFF when viability < OFF threshold.
        ON threshold should be > OFF threshold to prevent flickering. "
       Font Size: 10
       Color: Light gray (R:0.7, G:0.7, B:0.7)
       Wrapping: Enabled
       Add Layout Element: Preferred Height: 60
    ```

### Wire Inspector:

**Select `ViabilityDetailsSection` GameObject**, in Inspector:

**Hysteresis Configuration:**
- `onThresholdInput` ? Drag `OnThresholdInput`
- `offThresholdInput` ? Drag `OffThresholdInput`
- `explanationText` ? Drag `ExplanationText`

**CollapsibleSection fields:**
- `headerObject` ? Drag `HeaderPanel`
- `contentObject` ? Drag `ContentPanel`
- `toggleButton` ? Drag `HeaderPanel` (Button component)
- `headerText` ? Drag `HeaderText`

### Test:

1. Play scene
2. Find Mechanisms dropdown "Viability"
3. Select "Hysteresis" ? ViabilityDetailsSection appears
4. Select "Simple Threshold" ? Section disappears
5. Select "Hysteresis" again ? Section reappears
6. Type in ON threshold (e.g., "0.6") ? Config updates
7. Type in OFF threshold (e.g., "-0.4") ? Config updates
8. Explanation text wraps correctly

---

## ?? Wire to MechanismsSection (If Not Already Done)

**Check if MechanismsSection.OnMechanismChanged event is wired:**

1. **Select `MechanismsSection` GameObject**
2. **Scroll to bottom of Inspector** ? Look for `OnMechanismChanged` event
3. **If not wired, add listeners:**
   - Click `+` button 2 times (for the 2 new sections)
   - Drag `DiffusionDetailsSection` ? listener, select `RefreshVisibility(WorkingScenarioConfig)`
   - Drag `ViabilityDetailsSection` ? listener, select `RefreshVisibility(WorkingScenarioConfig)`

**Note:** You should already have TopologyDetailsSection and InflowDetailsSection wired. Now add these 2 new ones.

---

## ? Final Testing Checklist

### DiffusionDetailsSection:
- [ ] Hidden when Diffusion = "Von Neumann (4-neighbor)"
- [ ] Hidden when Diffusion = "Moore (8-neighbor)"
- [ ] Visible when Diffusion = "Anisotropic"
- [ ] Direction dropdown has 4 options with arrow symbols
- [ ] Bias slider moves from 0.0 to 1.0
- [ ] Bias value text updates dynamically: "Bias: 0.XX"
- [ ] Config updates immediately when slider moves
- [ ] Section collapses/expands with header click

### ViabilityDetailsSection:
- [ ] Hidden when Viability = "Simple Threshold"
- [ ] Visible when Viability = "Hysteresis"
- [ ] ON threshold input accepts decimal numbers
- [ ] OFF threshold input accepts decimal numbers (including negative)
- [ ] Explanation text displays and wraps correctly
- [ ] Config updates immediately on input
- [ ] Section collapses/expands with header click

### Integration:
- [ ] Both sections start hidden (if default mechanisms don't trigger them)
- [ ] Changing mechanism dropdowns triggers visibility updates
- [ ] No errors in Unity Console
- [ ] No scroll bars appear in RightDock
- [ ] All 6 sections (Mechanisms + 4 details + CoreParameters) fit in fixed height

---

## ?? Troubleshooting

### Issue: Section not appearing when mechanism selected
- **Check:** MechanismsSection.OnMechanismChanged event has listener for this section?
- **Check:** Section GameObject `SetActive = true` in hierarchy?
- **Check:** RefreshVisibility() method in section script matches mechanism enum?

### Issue: Bias slider text not updating
- **Check:** BiasSlider.onValueChanged listener wired in Start()?
- **Check:** BiasValueText field wired in Inspector?
- **Check:** OnBiasChanged() method updates text with `$"Bias: {value:F2}"`?

### Issue: Config not updating
- **Check:** currentConfig not null?
- **Check:** Input field onEndEdit listeners wired?
- **Check:** Bind() method called when preset loads?

---

## ?? Summary

**You've now added:**
- ? DiffusionDetailsSection (Anisotropic direction + bias)
- ? ViabilityDetailsSection (Hysteresis ON/OFF thresholds)

**Your SetupPanel now has all 6 sections:**
1. MechanismSummary (read-only) ?
2. MechanismsSection (always visible) ?
3. TopologyDetailsSection (conditional) ?
4. InflowDetailsSection (conditional) ?
5. DiffusionDetailsSection (conditional) ? **NEW**
6. ViabilityDetailsSection (conditional) ? **NEW**
7. CoreParametersSection (always visible) ?

**Next:** All Stage 13.10 sections complete! Continue to Stage 13.11 (if CoreParametersSection needs more work) or Stage 13.12 (InspectTab implementation).

---

**Stage 13.10 NOW Complete!** ? All 4 context-sensitive detail sections implemented.
