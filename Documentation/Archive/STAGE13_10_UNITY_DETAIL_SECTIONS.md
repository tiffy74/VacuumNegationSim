# Stage 13.10: Unity UI Setup - Context-Sensitive Detail Sections

## ?? GOAL

Implement 4 context-sensitive detail sections in Unity that show/hide based on mechanism selections:
- **TopologyDetailsSection** - Shows when Topology = "Masked Domain"
- **InflowDetailsSection** - Shows when Inflow = "Point Sources"
- **DiffusionDetailsSection** - Shows when Diffusion = "Anisotropic"
- **ViabilityDetailsSection** - Shows when ViabilityRule = "Hysteresis"

**Prerequisites:** You have already built TopBar, RightDock, and MechanismsSection in Unity (Stages 13.1-13.9).

---

## ?? What You'll Build

In your Unity scene at **`SetupPanel` ? `SectionsContainer`**, add 4 new section GameObjects below `MechanismsSection`.

Each section:
1. Has a collapsible header (inherits from `CollapsibleSection`)
2. Shows/hides automatically based on mechanism dropdown selections
3. Updates `WorkingScenarioConfig` immediately on changes
4. Contains mechanism-specific controls

---

## ?? Section 1: TopologyDetailsSection

**Shows when:** Topology dropdown = "Masked Domain"

### Unity Setup:

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `TopologyDetailsSection`
3. **Add Components:**
   - `TopologyDetailsSection` script
   - `Vertical Layout Group` (Spacing: 5, Child Force Expand Width: ?)

4. **Create Header** (same pattern as MechanismsSection):
   - Right-click `TopologyDetailsSection` ? UI ? Panel
   - Name: `HeaderPanel`, Height: 30
   - Add `Button` component
   - Inside: Text (TMP) named `HeaderText`, Text: "Topology Details", Bold, Size: 16

5. **Create ContentPanel**:
   - Right-click `TopologyDetailsSection` ? Create Empty
   - Name: `ContentPanel`
   - Add `Vertical Layout Group` (Padding: 10, Spacing: 8)

6. **Add Mask Shape Dropdown Row**:
   ```
   ContentPanel
   ?? MaskShapeRow (Horizontal Layout Group)
      ?? Label (Text TMP): "Mask Shape:", Width: 100
      ?? MaskShapeDropdown (TMP_Dropdown), Flexible Width: 1
          Options: Rectangle, Circle, Ring, Corridor, Percolation Holes
   ```

7. **Add Dynamic Parameter Rows** (create 4 rows, initially **SetActive: false**):

   **RadiusOuterRow:**
   ```
   ContentPanel
   ?? RadiusOuterRow (Horizontal Layout, SetActive: false)
      ?? Label: "Outer Radius:"
      ?? RadiusOuterInput (TMP_InputField), Placeholder: "20.0"
   ```

   **RadiusInnerRow:**
   ```
   ContentPanel
   ?? RadiusInnerRow (Horizontal Layout, SetActive: false)
      ?? Label: "Inner Radius:"
      ?? RadiusInnerInput (TMP_InputField), Placeholder: "10.0"
   ```

   **CorridorWidthRow:**
   ```
   ContentPanel
   ?? CorridorWidthRow (Horizontal Layout, SetActive: false)
      ?? Label: "Corridor Width:"
      ?? CorridorWidthInput (TMP_InputField), Placeholder: "8.0"
   ```

   **PercolationProbRow:**
   ```
   ContentPanel
   ?? PercolationProbRow (Horizontal Layout, SetActive: false)
      ?? Label: "Hole Probability:"
      ?? PercolationProbInput (TMP_InputField), Placeholder: "0.30"
   ```

### Wire Inspector:

**Select `TopologyDetailsSection` GameObject**, in Inspector:

**Mask Configuration:**
- `maskShapeDropdown` ? Drag `MaskShapeDropdown`
- `radiusOuterInput` ? Drag `RadiusOuterInput`
- `radiusInnerInput` ? Drag `RadiusInnerInput`
- `corridorWidthInput` ? Drag `CorridorWidthInput`
- `percolationProbInput` ? Drag `PercolationProbInput`

**Field Visibility GameObjects:**
- `radiusOuterRow` ? Drag `RadiusOuterRow` **GameObject**
- `radiusInnerRow` ? Drag `RadiusInnerRow` **GameObject**
- `corridorWidthRow` ? Drag `CorridorWidthRow` **GameObject**
- `percolationProbRow` ? Drag `PercolationProbRow` **GameObject**

**CollapsibleSection fields:**
- `headerObject` ? Drag `HeaderPanel`
- `contentObject` ? Drag `ContentPanel`
- `toggleButton` ? Drag `HeaderPanel` (Button component)
- `headerText` ? Drag `HeaderText`

### Test:

1. Play scene
2. Change Topology dropdown to "Masked Domain" ? Section should appear
3. Change back to "Full Domain" ? Section should disappear
4. Select "Circle" in Mask Shape ? Only Outer Radius row appears
5. Select "Ring" ? Outer + Inner Radius rows appear

---

## ?? Section 2: InflowDetailsSection

**Shows when:** Inflow dropdown = "Point Sources"

### Unity Setup:

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `InflowDetailsSection`
3. **Add Components:**
   - `InflowDetailsSection` script
   - `Vertical Layout Group`

4. **Create Header + ContentPanel** (same pattern)

5. **Add Point Source Display**:
   ```
   ContentPanel
   ?? PointSourceCountText (Text TMP)
   ?  Text: "Point Sources: 0"
   ?  Font Size: 12, Bold
   ?
   ?? PointSourceListText (Text TMP)
   ?  Text: "No point sources defined. Click 'Edit Sources' to add."
   ?  Font Size: 11, Wrapping: Enabled, Min Height: 60
   ?
   ?? EditPointSourcesButton (Button TMP)
      Text: "Edit Sources...", Full Width
   ```

### Wire Inspector:

**Select `InflowDetailsSection` GameObject**:

**Point Source Display:**
- `pointSourceCountText` ? Drag `PointSourceCountText`
- `pointSourceListText` ? Drag `PointSourceListText`
- `editPointSourcesButton` ? Drag `EditPointSourcesButton`

**CollapsibleSection fields** (header, content, button, text)

### Test:

1. Play scene
2. Change Inflow dropdown to "Point Sources" ? Section appears
3. Click "Edit Sources..." button ? Test source added (placeholder behavior)
4. List text updates with source coordinates

---

## ?? Section 3: DiffusionDetailsSection

**Shows when:** Diffusion dropdown = "Anisotropic"

### Unity Setup:

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `DiffusionDetailsSection`
3. **Add Components:** DiffusionDetailsSection, Vertical Layout Group

4. **Create Header + ContentPanel**

5. **Add Anisotropic Controls**:
   ```
   ContentPanel
   ?? DirectionRow (Horizontal Layout)
   ?  ?? Label: "Direction:"
   ?  ?? DirectionDropdown (TMP_Dropdown)
   ?     Options: North (?), East (?), South (?), West (?)
   ?
   ?? BiasRow (Horizontal Layout)
      ?? Label: "Bias:"
      ?? BiasSlider (Slider: Min=0, Max=1, Value=0.5)
      ?? BiasValueText (Text TMP): "Bias: 0.50"
   ```

### Wire Inspector:

**Select `DiffusionDetailsSection` GameObject**:

**Anisotropic Configuration:**
- `directionDropdown` ? Drag `DirectionDropdown`
- `biasSlider` ? Drag `BiasSlider`
- `biasValueText` ? Drag `BiasValueText`

**CollapsibleSection fields**

### Test:

1. Change Diffusion dropdown to "Anisotropic" ? Section appears
2. Change direction dropdown ? Updates config
3. Move bias slider ? Text updates to "Bias: 0.XX"

---

## ?? Section 4: ViabilityDetailsSection

**Shows when:** Viability dropdown = "Hysteresis"

### Unity Setup:

1. **Right-click `SectionsContainer`** ? Create Empty
2. **Name:** `ViabilityDetailsSection`
3. **Add Components:** ViabilityDetailsSection, Vertical Layout Group

4. **Create Header + ContentPanel**

5. **Add Hysteresis Controls**:
   ```
   ContentPanel
   ?? OnThresholdRow (Horizontal Layout)
   ?  ?? Label: "ON Threshold:"
   ?  ?? OnThresholdInput (TMP_InputField), Placeholder: "0.50"
   ?
   ?? OffThresholdRow (Horizontal Layout)
   ?  ?? Label: "OFF Threshold:"
   ?  ?? OffThresholdInput (TMP_InputField), Placeholder: "-0.50"
   ?
   ?? ExplanationText (Text TMP)
      Text: "Hysteresis: Cells turn ON when viability > ON threshold,
             and turn OFF when viability < OFF threshold.
             ON threshold should be > OFF threshold to prevent flickering."
      Font Size: 10, Color: Light gray, Wrapping: Enabled
   ```

### Wire Inspector:

**Select `ViabilityDetailsSection` GameObject**:

**Hysteresis Configuration:**
- `onThresholdInput` ? Drag `OnThresholdInput`
- `offThresholdInput` ? Drag `OffThresholdInput`
- `explanationText` ? Drag `ExplanationText`

**CollapsibleSection fields**

### Test:

1. Change Viability dropdown to "Hysteresis" ? Section appears
2. Type in ON threshold ? Updates config
3. Type in OFF threshold ? Updates config
4. Explanation text displays correctly

---

## ?? Wire to MechanismsSection

**Wire `OnMechanismChanged` event to refresh detail sections:**

### Option 1: In Unity Inspector (Easiest)

1. **Select `MechanismsSection` GameObject**
2. **Find `OnMechanismChanged` event** in Inspector
3. **Add 4 listeners:**
   - Click `+` button 4 times
   - Drag `TopologyDetailsSection` ? listener 1, select `RefreshVisibility()`
   - Drag `InflowDetailsSection` ? listener 2, select `RefreshVisibility()`
   - Drag `DiffusionDetailsSection` ? listener 3, select `RefreshVisibility()`
   - Drag `ViabilityDetailsSection` ? listener 4, select `RefreshVisibility()`

### Option 2: In Code (If you have UIManager)

```csharp
// In UIManager.cs Initialize()
mechanismsSection.OnMechanismChanged += () =>
{
    topologyDetailsSection?.RefreshVisibility(workingConfig);
    inflowDetailsSection?.RefreshVisibility(workingConfig);
    diffusionDetailsSection?.RefreshVisibility(workingConfig);
    viabilityDetailsSection?.RefreshVisibility(workingConfig);
};
```

---

## ? Testing Checklist

### TopologyDetailsSection:
- [ ] Hidden when Topology = "Full Domain"
- [ ] Visible when Topology = "Masked Domain"
- [ ] Mask shape dropdown works
- [ ] Circle: Shows Outer Radius only
- [ ] Ring: Shows Outer + Inner Radius
- [ ] Corridor: Shows Width only
- [ ] Percolation: Shows Probability only
- [ ] Input fields accept numbers
- [ ] Config updates immediately

### InflowDetailsSection:
- [ ] Hidden when Inflow = "Uniform Field" or "Edge Sources"
- [ ] Visible when Inflow = "Point Sources"
- [ ] Count text shows "Point Sources: N"
- [ ] List text displays coordinates
- [ ] Edit button works (adds test source)

### DiffusionDetailsSection:
- [ ] Hidden when Diffusion = "Von Neumann" or "Moore"
- [ ] Visible when Diffusion = "Anisotropic"
- [ ] Direction dropdown has 4 options with arrows
- [ ] Bias slider moves smoothly
- [ ] Bias value text updates to "Bias: 0.XX"

### ViabilityDetailsSection:
- [ ] Hidden when Viability = "Simple Threshold"
- [ ] Visible when Viability = "Hysteresis"
- [ ] ON threshold input accepts numbers
- [ ] OFF threshold input accepts numbers
- [ ] Explanation text displays and wraps correctly

### Integration:
- [ ] All sections start hidden (except if mechanism selected)
- [ ] Changing mechanism dropdown triggers visibility update
- [ ] No errors in Console
- [ ] Sections collapse/expand with header click
- [ ] No scroll bars appear (fixed height)

---

## ?? Troubleshooting

### Issue: Section not appearing
- **Check:** MechanismsSection.OnMechanismChanged event wired?
- **Check:** Section GameObject SetActive = true?
- **Check:** RefreshVisibility() logic in section script?

### Issue: Dynamic rows (RadiusOuter, etc.) not hiding/showing
- **Check:** Row GameObjects wired to `radiusOuterRow` field (not InputField)?
- **Check:** UpdateFieldVisibility() is called in OnMaskShapeChanged()?

### Issue: Config not updating
- **Check:** Input field onEndEdit listeners wired in Start()?
- **Check:** WorkingScenarioConfig passed to Bind()?

---

## ?? Summary

**You now have:**
- ? 4 context-sensitive detail sections
- ? Dynamic field visibility (TopologyDetailsSection)
- ? Point source display (placeholder for modal)
- ? Anisotropic diffusion controls
- ? Hysteresis threshold configuration
- ? All sections show/hide automatically based on mechanism selections
- ? All changes apply immediately to `WorkingScenarioConfig`

**Next:** Stage 13.11 - CoreParametersSection (8 curated simulation parameters)

---

**Stage 13.10 Complete!** ? Context-sensitive detail sections implemented.
