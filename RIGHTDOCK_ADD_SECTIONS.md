# Adding Sections to Clean RightDock

**Prerequisites:** You've completed `RIGHTDOCK_CLEAN_RESET.md` and have a working RightDock with dropdown panel switching.

**Goal:** Add sections to SetupPanel one at a time, testing after each addition.

**Time:** 10-20 min per section

---

## ?? **Section Priority Order**

Add in this order (simplest ? most complex):

1. ? **MechanismSummary** - Read-only status display
2. ? **MechanismsSection** - 6 mechanism dropdowns
3. ? **CoreParametersSection** - 8 parameter inputs
4. ? **TopologyDetailsSection** - Conditional (Masked Domain)
5. ? **InflowDetailsSection** - Conditional (Point Sources)
6. ? **DiffusionDetailsSection** - Conditional (Anisotropic)
7. ? **ViabilityDetailsSection** - Conditional (Hysteresis)

---

## ?? **Section 1: MechanismSummary (10 min)**

**What it does:** Shows current mechanism selections at a glance (read-only)

**Example:** "Topology: Full • Boundary: Wrap • Inflow: Uniform • Diffusion: Moore8 • Viability: Simple"

### **Unity Setup:**

1. **Select `SetupPanel`**
2. **Delete** placeholder text (if exists)
3. **Add Vertical Layout Group:**
   - Padding: 10
   - Spacing: 10
   - Child Force Expand: Width ?
4. **Right-click `SetupPanel`** ? UI ? Panel
5. **Name:** `MechanismSummary`
6. **Add Layout Element:**
   - Min Height: 50, Preferred Height: 50
7. **Background Color:** Slightly darker (R: 20, G: 20, B: 25)
8. **Right-click `MechanismSummary`** ? UI ? Text - TextMeshPro
9. **Name:** `SummaryText`
10. **Settings:**
    - Text: "Topology: Full • Boundary: Wrap • Inflow: Uniform • Diffusion: Moore8 • Viability: Simple"
    - Font Size: 11
    - Color: Light cyan (R: 0.7, G: 0.9, B: 1.0)
    - Alignment: Left, Top
    - Wrapping: Enabled
    - Overflow: Ellipsis

### **Test:**
- [ ] Summary text visible at top of SetupPanel
- [ ] Text wraps if too long
- [ ] Looks clean and readable

---

## ?? **Section 2: MechanismsSection (20 min)**

**What it does:** 6 dropdowns to select mechanisms

### **Unity Setup:**

1. **Right-click `SetupPanel`** ? Create Empty
2. **Name:** `MechanismsSection`
3. **Add Components:**
   - `MechanismsSection` script
   - `Vertical Layout Group` (Spacing: 5)
4. **Create Header** (collapsible):
   - Right-click `MechanismsSection` ? UI ? Panel
   - Name: `HeaderPanel`, Height: 30
   - Add Button component
   - Inside: Text (TMP) "Mechanisms", Bold, Size: 16
5. **Create ContentPanel:**
   - Right-click `MechanismsSection` ? Create Empty
   - Name: `ContentPanel`
   - Add Vertical Layout Group (Padding: 10, Spacing: 8)

6. **Add 6 Dropdown Rows** (same pattern for each):

**Pattern:**
```
ContentPanel
?? TopologyRow (Horizontal Layout, Spacing: 10)
   ?? Label (Text TMP): "Topology:", Width: 100
   ?? TopologyDropdown (TMP_Dropdown), Flexible: 1
      Options: "Full Domain", "Masked Domain"
```

**6 Rows:**
- TopologyRow ? TopologyDropdown
- BoundaryRow ? BoundaryDropdown
- InflowRow ? InflowDropdown
- DiffusionRow ? DiffusionDropdown
- ViabilityRow ? ViabilityDropdown
- PhaseSetRow ? PhaseSetDropdown

### **Wire Inspector:**

**Select `MechanismsSection`**, wire:
- All 6 dropdowns to script fields
- `mechanismSummaryText` ? Drag `SummaryText` from MechanismSummary
- Header fields (headerObject, contentObject, toggleButton, headerText)

### **Update Script:**

Add to `MechanismsSection.cs`:

```csharp
private void Start()
{
    // ... existing code

    // Register with UIController
    if (UIController.Instance != null)
        UIController.Instance.RegisterSection(this);
}

private void OnDestroy()
{
    if (UIController.Instance != null)
        UIController.Instance.UnregisterSection(this);
}

public void Bind(WorkingScenarioConfig config)
{
    currentConfig = config;
    
    if (UIController.Instance?.IsRefreshing == true)
        return;
    
    // Update dropdowns from config
    topologyDropdown.value = (int)config.Topology;
    boundaryDropdown.value = (int)config.Boundary;
    // ... etc
    
    UpdateSummary();
}

private void OnTopologyChanged(int value)
{
    if (UIController.Instance?.IsRefreshing == true)
        return;
    
    if (currentConfig != null)
    {
        currentConfig.Topology = (TopologyMode)value;
        UpdateSummary();
        OnMechanismChanged?.Invoke();
    }
}
```

### **Test:**
- [ ] All 6 dropdowns visible
- [ ] Changing dropdown updates summary text
- [ ] No errors in Console

---

## ?? **Section 3: CoreParametersSection (20 min)**

**What it does:** 8 curated parameter inputs

### **Unity Setup:**

1. **Right-click `SetupPanel`** ? Create Empty
2. **Name:** `CoreParametersSection`
3. **Add Components:**
   - `CoreParametersSection` script
   - `Vertical Layout Group`
4. **Create Header + ContentPanel** (same pattern)
5. **Add 8 Input Rows** (same pattern):

**Pattern:**
```
ContentPanel
?? ResourceGlobalMaxRow (Horizontal Layout, Spacing: 10)
   ?? Label: "Global Resource Max:", Width: 150
   ?? ResourceGlobalMaxInput (TMP_InputField)
      Content Type: Standard (allows scientific notation)
      Placeholder: "5e7"
      Flexible: 1
```

**8 Rows:**
- ResourceGlobalMax
- ResourceRechargeRate
- DecayLoss
- MaintCost
- ActivationCost
- ExpansionProbability
- InflowPerCell
- DiffusionRate

### **Wire Inspector + Update Script:**

Same pattern as MechanismsSection:
- Add UIController registration
- Add Bind() with refresh guard
- Add handlers with refresh guards

### **Test:**
- [ ] All 8 inputs visible
- [ ] Can type numbers and scientific notation
- [ ] Values persist when switching panels
- [ ] No errors

---

## ?? **Sections 4-7: Conditional Sections (20 min each)**

These sections show/hide based on mechanism selections.

**? COMPLETE GUIDE:** See `CONDITIONAL_SECTIONS_COMPLETE_GUIDE.md`

**Quick Summary:**

1. **TopologyDetailsSection** - Shows when Topology = "Masked Domain"
   - Mask shape dropdown
   - Dynamic parameter inputs (radius, width, probability)
   - 4 rows that show/hide based on shape

2. **InflowDetailsSection** - Shows when Inflow = "Point Sources"
   - Point source count display
   - Point source list text
   - "Edit Sources" button

3. **DiffusionDetailsSection** - Shows when Diffusion = "Anisotropic"
   - Direction dropdown (North/East/South/West)
   - Bias slider (0.0 - 1.0)
   - Bias value text

4. **ViabilityDetailsSection** - Shows when Viability = "Hysteresis"
   - ON threshold input
   - OFF threshold input
   - Explanation text

**All follow same pattern:**
- Create section GameObject in SetupPanel
- Add script + Vertical Layout Group
- Create Header (Panel + Button + Text)
- Create ContentPanel with controls
- Wire Inspector (dropdown/inputs + collapsible fields)
- Wire to MechanismsSection.OnMechanismChanged event

**See `CONDITIONAL_SECTIONS_COMPLETE_GUIDE.md` for step-by-step Unity instructions with exact settings!**

---

## ? **Testing After Each Section**

After adding **each** section:

1. **Press Play**
2. **Check:**
   - [ ] Section visible in correct position
   - [ ] Controls work (dropdowns, inputs, etc.)
   - [ ] Values persist when switching panels
   - [ ] No errors in Console
3. **Switch panels:**
   - [ ] Go to Inspect
   - [ ] Come back to Setup
   - [ ] Section still there, values intact
4. **Stop Play, save scene**

---

## ?? **Progress Tracker**

- [ ] MechanismSummary (read-only)
- [ ] MechanismsSection (6 dropdowns)
- [ ] CoreParametersSection (8 inputs)
- [ ] TopologyDetailsSection (conditional)
- [ ] InflowDetailsSection (conditional)
- [ ] DiffusionDetailsSection (conditional)
- [ ] ViabilityDetailsSection (conditional)

---

## ?? **When All Sections Added:**

? Complete Setup panel with all controls  
? Value persistence working  
? Mechanism visibility logic working  
? Ready for InspectTab and ExportTab implementation  

---

**Time per section:** 10-20 minutes  
**Total time:** 2-3 hours for all sections  
**Approach:** Add one, test, then add next (don't rush!)
