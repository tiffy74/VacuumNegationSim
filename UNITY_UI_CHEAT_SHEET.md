# Unity UI Cheat Sheet - Build Sections Fast

## ?? Quick Reference: Build DiffusionDetailsSection in 5 Minutes

### Part 1: Create Structure (2 min)

1. **Right-click `SetupPanel`** ? Create Empty ? Name: `DiffusionDetailsSection`
2. **Select it** ? Add Component ? `DiffusionDetailsSection` script
3. **Add Component** ? `Vertical Layout Group` (Spacing: 5, Child Width: ?)

---

### Part 2: Header (1 min)

4. **Right-click `DiffusionDetailsSection`** ? UI ? Panel ? Name: `HeaderPanel`
   - Add Component ? Layout Element (Min/Preferred Height: 30)
   - Add Component ? Button
5. **Right-click `HeaderPanel`** ? UI ? Text - TextMeshPro ? Name: `HeaderText`
   - Text: "Diffusion Details", Size: 16, Bold, White

---

### Part 3: Content Panel (30 sec)

6. **Right-click `DiffusionDetailsSection`** ? Create Empty ? Name: `ContentPanel`
   - Add Component ? Vertical Layout Group (Padding: 10, Spacing: 8)

---

### Part 4: Direction Row (1 min)

7. **Right-click `ContentPanel`** ? Create Empty ? Name: `DirectionRow`
   - Add Component ? Horizontal Layout Group (Spacing: 10)
8. **Right-click `DirectionRow`** ? UI ? Text - TextMeshPro ? Name: `DirectionLabel`
   - Text: "Direction:", Layout Element ? Preferred Width: 100
9. **Right-click `DirectionRow`** ? UI ? Dropdown - TextMeshPro ? Name: `DirectionDropdown`
   - Layout Element ? Flexible Width: 1
   - TMP_Dropdown component ? Options (Size: 4):
     - 0: "North (?)"
     - 1: "East (?)"
     - 2: "South (?)"
     - 3: "West (?)"

---

### Part 5: Bias Row (1 min)

10. **Right-click `ContentPanel`** ? Create Empty ? Name: `BiasRow`
    - Add Component ? Horizontal Layout Group (Spacing: 10)
11. **Right-click `BiasRow`** ? UI ? Text - TextMeshPro ? Name: `BiasLabel`
    - Text: "Bias:", Preferred Width: 100
12. **Right-click `BiasRow`** ? UI ? Slider ? Name: `BiasSlider`
    - Min: 0, Max: 1, Value: 0.5, Whole Numbers: OFF
    - Layout Element ? Flexible Width: 1
13. **Right-click `BiasRow`** ? UI ? Text - TextMeshPro ? Name: `BiasValueText`
    - Text: "Bias: 0.50", Preferred Width: 80

---

### Part 6: Wire Inspector (1 min) ?? CRITICAL!

14. **Select `DiffusionDetailsSection`** (root GameObject)
15. **In Inspector, find DiffusionDetailsSection script**, drag GameObjects:
    - `directionDropdown` ? Drag `DirectionDropdown`
    - `biasSlider` ? Drag `BiasSlider`
    - `biasValueText` ? Drag `BiasValueText`
    - `headerObject` ? Drag `HeaderPanel`
    - `contentObject` ? Drag `ContentPanel`
    - `toggleButton` ? Drag `HeaderPanel` (yes, same!)
    - `headerText` ? Drag `HeaderText`

---

## ? Test

1. **Press Play**
2. **Change Diffusion dropdown to "Anisotropic"**
3. **DiffusionDetailsSection should appear!** ?

---

## ?? Build ViabilityDetailsSection in 5 Minutes

### Part 1: Create Structure

1. **Right-click `SetupPanel`** ? Create Empty ? Name: `ViabilityDetailsSection`
2. Add Component ? `ViabilityDetailsSection` script
3. Add Component ? Vertical Layout Group (Spacing: 5)

### Part 2: Header

4. **Right-click `ViabilityDetailsSection`** ? UI ? Panel ? Name: `HeaderPanel`
   - Layout Element (Height: 30), Button
5. **Right-click `HeaderPanel`** ? UI ? Text - TextMeshPro ? Name: `HeaderText`
   - Text: "Viability Details", Size: 16, Bold

### Part 3: Content Panel

6. **Right-click `ViabilityDetailsSection`** ? Create Empty ? Name: `ContentPanel`
   - Vertical Layout Group (Padding: 10, Spacing: 8)

### Part 4: ON Threshold Row

7. **Right-click `ContentPanel`** ? Create Empty ? Name: `OnThresholdRow`
   - Horizontal Layout Group (Spacing: 10)
8. **Right-click `OnThresholdRow`** ? UI ? Text - TextMeshPro ? Name: `OnThresholdLabel`
   - Text: "ON Threshold:", Preferred Width: 120
9. **Right-click `OnThresholdRow`** ? UI ? Input Field - TextMeshPro ? Name: `OnThresholdInput`
   - Content Type: Decimal Number, Placeholder: "0.50", Flexible Width: 1

### Part 5: OFF Threshold Row

10. **Right-click `ContentPanel`** ? Create Empty ? Name: `OffThresholdRow`
    - Horizontal Layout Group (Spacing: 10)
11. **Right-click `OffThresholdRow`** ? UI ? Text - TextMeshPro ? Name: `OffThresholdLabel`
    - Text: "OFF Threshold:", Preferred Width: 120
12. **Right-click `OffThresholdRow`** ? UI ? Input Field - TextMeshPro ? Name: `OffThresholdInput`
    - Content Type: Decimal Number, Placeholder: "-0.50", Flexible Width: 1

### Part 6: Explanation Text

13. **Right-click `ContentPanel`** ? UI ? Text - TextMeshPro ? Name: `ExplanationText`
    - Text: "Hysteresis: Cells turn ON when viability > ON threshold, and turn OFF when viability < OFF threshold. ON threshold should be > OFF threshold to prevent flickering."
    - Font Size: 10, Color: Light gray (RGB: 0.7, 0.7, 0.7)
    - Wrapping: Enabled
    - Layout Element ? Preferred Height: 60

### Part 7: Wire Inspector ?? CRITICAL!

14. **Select `ViabilityDetailsSection`**, drag GameObjects:
    - `onThresholdInput` ? Drag `OnThresholdInput`
    - `offThresholdInput` ? Drag `OffThresholdInput`
    - `explanationText` ? Drag `ExplanationText`
    - `headerObject` ? Drag `HeaderPanel`
    - `contentObject` ? Drag `ContentPanel`
    - `toggleButton` ? Drag `HeaderPanel`
    - `headerText` ? Drag `HeaderText`

---

## ? Test

1. **Press Play**
2. **Change Viability dropdown to "Hysteresis"**
3. **ViabilityDetailsSection should appear!** ?

---

## ?? Final Step: Wire to MechanismsSection

1. **Select `MechanismsSection`** GameObject
2. **In Inspector, find `OnMechanismChanged` event** (at bottom)
3. **Click `+` button twice** (add 2 listeners)
4. **Listener 1:**
   - Drag `DiffusionDetailsSection` to object slot
   - Function dropdown ? `DiffusionDetailsSection` ? `RefreshVisibility(WorkingScenarioConfig)`
5. **Listener 2:**
   - Drag `ViabilityDetailsSection` to object slot
   - Function dropdown ? `ViabilityDetailsSection` ? `RefreshVisibility(WorkingScenarioConfig)`

---

## ?? Common Mistakes Checklist

Before you start, avoid these:

- [ ] ? Creating GameObject in wrong place (check parent!)
- [ ] ? Forgetting Layout Element on labels/inputs
- [ ] ? Forgetting to wire Inspector fields (script won't work!)
- [ ] ? Using wrong component type (Text vs Text - TextMeshPro)
- [ ] ? Not setting dropdown options
- [ ] ? Not saving scene (Ctrl+S)

---

## ?? Pro Tips

1. **Duplicate to save time:** After building DirectionRow, duplicate it (Ctrl+D) for BiasRow, then just change the children!
2. **Inspector search:** Type component name in "Add Component" to find it fast
3. **Lock Inspector:** Click padlock icon to keep Inspector on current object while clicking in Hierarchy
4. **Undo is your friend:** Ctrl+Z if something goes wrong

---

## ?? Layout Quick Reference

| Component | Purpose | Key Settings |
|-----------|---------|--------------|
| Horizontal Layout Group | Arrange children left-to-right | Spacing: 10 |
| Vertical Layout Group | Arrange children top-to-bottom | Spacing: 5-10, Padding: 10 |
| Layout Element | Control size | Preferred Width (fixed), Flexible Width (fill) |

---

## ?? Common Sizes

| Element Type | Width | Height |
|--------------|-------|--------|
| Label | 100-120px | Auto |
| Input Field | Flexible (1) | Auto |
| Button | Flexible or 80-100px | 30px |
| Header | Flexible | 30px |
| Dropdown | Flexible | Auto |
| Slider | Flexible | Auto |

---

## ?? Help! Something Broke!

### Elements stacked vertically instead of horizontal
? Parent needs **Horizontal Layout Group**, not Vertical

### Input field too small
? Add **Layout Element**, set **Flexible Width: 1**

### Script fields show "None" and won't let me drag
? Make sure you're dragging the **GameObject**, not the component

### Section not appearing when mechanism selected
? Check `OnMechanismChanged` event is wired in MechanismsSection

### Console shows "NullReferenceException"
? Inspector fields not wired! Drag GameObjects to all fields.

---

## ? Final Checklist

After building both sections:

- [ ] DiffusionDetailsSection exists and is wired
- [ ] ViabilityDetailsSection exists and is wired
- [ ] Both sections appear/disappear when mechanism changes
- [ ] Slider updates bias text dynamically
- [ ] Input fields accept numbers
- [ ] Headers are clickable (collapse/expand)
- [ ] No errors in Console
- [ ] Scene saved (Ctrl+S)

---

**Time estimate:** 10-15 minutes total for both sections if you follow this guide!

**Ready? Set a timer and GO!** ????
