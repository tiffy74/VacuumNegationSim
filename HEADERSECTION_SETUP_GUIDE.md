# HeaderSection Setup - Fix Button Navigation

**Problem:** 
1. Only Mechanisms and Core Parameters buttons work
2. Multiple sections overlap when buttons clicked
3. Need to close previous section when opening new one

**Solution:** HeaderSectionController manages button clicks and ensures only one section visible at a time.

---

## ? **Step 1: Add HeaderSectionController Script**

**File created:** `Assets/Viable/Core.Unity/UI/HeaderSectionController.cs`

This script:
- ? Wires all 6 buttons (Mechanisms, Core Parameters, TopologyDetails, InflowDetails, DiffusionDetails, ViabilityDetails)
- ? Shows only ONE section at a time
- ? Hides all other sections when a button is clicked
- ? Sets Mechanisms as default visible section

---

## ? **Step 2: Apply Script to HeaderSection**

### **In Unity:**

1. **Select `HeaderSection`** GameObject in Hierarchy
   - This should be the parent containing your HeaderSectionRow buttons

2. **Add Component** ? `HeaderSectionController`

3. **Wire in Inspector:**

**Header Buttons:**
- `mechanismsButton` ? Drag the Mechanisms button
- `coreParametersButton` ? Drag the Core Parameters button
- `topologyDetailsButton` ? Drag the Topology Details button
- `inflowDetailsButton` ? Drag the Inflow Details button
- `diffusionDetailsButton` ? Drag the Diffusion Details button
- `viabilityDetailsButton` ? Drag the Viability Details button

**Content Sections:**
- `mechanismsSection` ? Drag MechanismsSection GameObject
- `coreParametersSection` ? Drag CoreParametersSection GameObject
- `topologyDetailsSection` ? Drag TopologyDetailsSection GameObject
- `inflowDetailsSection` ? Drag InflowDetailsSection GameObject
- `diffusionDetailsSection` ? Drag DiffusionDetailsSection GameObject
- `viabilityDetailsSection` ? Drag ViabilityDetailsSection GameObject

---

## ? **Step 3: Remove Old Button OnClick Events**

**For EACH button (Mechanisms, Core Parameters, etc.):**

1. **Select button** in Hierarchy
2. **In Inspector ? Button component:**
   - Find **OnClick()** section
   - **Remove any existing listeners** (click `-` button to remove)
3. **Leave OnClick empty** - HeaderSectionController will handle it

**Why?** The controller wires buttons in code, so we don't need manual OnClick events.

---

## ? **Step 4: Verify Section Hierarchy**

**Expected structure:**

```
SetupPanel
?? HeaderSection (has HeaderSectionController script)
?  ?? HeaderSectionRow1
?  ?  ?? MechanismsButton
?  ?  ?? CoreParametersButton
?  ?  ?? TopologyDetailsButton
?  ?? HeaderSectionRow2
?     ?? InflowDetailsButton
?     ?? DiffusionDetailsButton
?     ?? ViabilityDetailsButton
?
?? MechanismsSection (content below buttons)
?? CoreParametersSection
?? TopologyDetailsSection
?? InflowDetailsSection
?? DiffusionDetailsSection
?? ViabilityDetailsSection
```

**Important:** All 6 content sections should be **siblings** (at the same level), NOT nested inside each other.

---

## ? **Step 5: Ensure Sections Fill Available Space**

**For EACH section (MechanismsSection, CoreParametersSection, etc.):**

1. **Select section** in Hierarchy
2. **RectTransform:**
   - ?? Do NOT set anchors manually if parent has LayoutGroup
   - If parent (SetupPanel) has Vertical Layout Group, sections are auto-positioned

3. **Add Layout Element** (if not present):
   ```
   Preferred Height: (leave empty for flexible)
   Flexible Height: 1  ? Takes remaining space
   ```

**Why?** Each section should fill the space below HeaderSection when active.

---

## ?? **Testing**

### **Test 1: Single Section Visible**

1. **Press Play**
2. **Check:** Only MechanismsSection visible by default
3. **Click Core Parameters button** ? MechanismsSection hides, CoreParametersSection shows
4. **Click Topology Details button** ? CoreParametersSection hides, TopologyDetailsSection shows
5. **Result:** Only ONE section visible at a time ?

---

### **Test 2: All Buttons Work**

**Click each button and verify section appears:**

- [ ] Mechanisms button ? MechanismsSection
- [ ] Core Parameters button ? CoreParametersSection
- [ ] Topology Details button ? TopologyDetailsSection
- [ ] Inflow Details button ? InflowDetailsSection
- [ ] Diffusion Details button ? DiffusionDetailsSection
- [ ] Viability Details button ? ViabilityDetailsSection

---

### **Test 3: No Overlap**

1. **Click Mechanisms button**
2. **Check:** Only Mechanisms dropdowns visible
3. **Click Core Parameters button**
4. **Check:** Mechanisms dropdowns disappear, Core Parameters inputs appear
5. **Result:** No overlapping content ?

---

### **Test 4: Console Check**

1. **Open Console** (Ctrl+Shift+C)
2. **Click each button**
3. **Check:** Should see log messages:
   ```
   HeaderSectionController: Showing MechanismsSection
   HeaderSectionController: Showing CoreParametersSection
   HeaderSectionController: Showing TopologyDetailsSection
   ...
   ```
4. **No errors** ?

---

## ?? **Troubleshooting**

### **Issue A: Button doesn't show its section**

**Cause:** Button or section not wired in HeaderSectionController

**Fix:**
1. Select HeaderSection
2. Check Inspector ? HeaderSectionController
3. Verify button and section are both dragged into correct fields
4. If null/missing, drag the GameObjects from Hierarchy

---

### **Issue B: Section still overlaps with previous**

**Cause:** Section not hiding properly

**Check:**
1. Section GameObject is wired to controller (not null)
2. Section is a direct child of SetupPanel (or correct parent)
3. Section has correct SetActive state (starts inactive except Mechanisms)

**Fix:**
1. Select each section (except Mechanisms)
2. **Uncheck** active checkbox in Inspector
3. Only MechanismsSection should be active by default

---

### **Issue C: Button click does nothing**

**Cause:** Old OnClick event still attached

**Fix:**
1. Select button
2. Button component ? OnClick()
3. Remove all listeners (click `-` for each)
4. HeaderSectionController will wire it in code

---

### **Issue D: Console shows "section is null"**

**Cause:** Section GameObject not found or not wired

**Fix:**
1. Check section exists in Hierarchy
2. Check spelling matches exactly (e.g., "TopologyDetailsSection" not "TopologySection")
3. Drag section GameObject to controller field

---

## ?? **Expected Behavior**

### **Startup:**
```
SetupPanel
?? HeaderSection (buttons visible)
?? MechanismsSection (ACTIVE, visible)
    CoreParametersSection (inactive, hidden)
    TopologyDetailsSection (inactive, hidden)
    InflowDetailsSection (inactive, hidden)
    DiffusionDetailsSection (inactive, hidden)
    ViabilityDetailsSection (inactive, hidden)
```

### **After Clicking "Core Parameters":**
```
SetupPanel
?? HeaderSection (buttons visible)
?? MechanismsSection (inactive, hidden)
    CoreParametersSection (ACTIVE, visible) ? Switched!
    TopologyDetailsSection (inactive, hidden)
    InflowDetailsSection (inactive, hidden)
    DiffusionDetailsSection (inactive, hidden)
    ViabilityDetailsSection (inactive, hidden)
```

---

## ?? **Optional Enhancements**

### **Enhancement 1: Button Visual Feedback**

**Show which button is currently active:**

Update `HeaderSectionController.ShowSection()` to highlight active button:

```csharp
private Button currentActiveButton;

private void ShowSection(GameObject sectionToShow, Button clickedButton)
{
    // ... existing code ...
    
    // Update button colors
    ResetAllButtonColors();
    if (clickedButton != null)
    {
        SetButtonHighlighted(clickedButton, true);
        currentActiveButton = clickedButton;
    }
}

private void SetButtonHighlighted(Button button, bool highlighted)
{
    var colors = button.colors;
    colors.normalColor = highlighted ? Color.cyan : Color.white;
    button.colors = colors;
}
```

---

### **Enhancement 2: Keyboard Navigation**

**Allow Tab/Shift+Tab to switch sections:**

```csharp
private void Update()
{
    if (Input.GetKeyDown(KeyCode.Tab))
    {
        if (Input.GetKey(KeyCode.LeftShift))
            ShowPreviousSection();
        else
            ShowNextSection();
    }
}
```

---

## ? **Success Criteria**

After setup:

- [ ] HeaderSectionController script on HeaderSection GameObject
- [ ] All 6 buttons wired in Inspector
- [ ] All 6 sections wired in Inspector
- [ ] Old OnClick events removed from buttons
- [ ] Press Play ? MechanismsSection visible by default
- [ ] Click Core Parameters ? CoreParametersSection shows, others hide
- [ ] Click Topology Details ? TopologyDetailsSection shows, others hide
- [ ] Click Inflow Details ? InflowDetailsSection shows
- [ ] Click Diffusion Details ? DiffusionDetailsSection shows
- [ ] Click Viability Details ? ViabilityDetailsSection shows
- [ ] No overlapping content
- [ ] Only ONE section visible at a time
- [ ] No Console errors

---

## ?? **Result**

? All 6 buttons work  
? Only one section visible at a time  
? No overlapping content  
? Clean button navigation  
? Easy to add more sections later  

**Time to implement:** 5-10 minutes

**Enjoy your working HeaderSection navigation!** ??
