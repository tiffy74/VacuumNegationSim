# RightDock UI - Complete Configuration Guide

## ?? **What is RightDock?**

**RightDock** is the right-side panel containing:
- **HeaderSection** - Buttons to switch between configuration sections
- **MechanismsSection** - Dropdowns for simulation mechanisms
- **CoreParametersSection** - Input fields for parameters
- **Detail Sections** - Context-specific options (Inflow, Topology, etc.)

---

## ?? **Visual Structure**

```
RightDock (GameObject)
?
?? ContentArea
?  ?? SetupPanel (active by default)
?  ?  ?
?  ?  ?? HeaderSection ????????????????
?  ?  ?  ?? MechanismsButton          ?
?  ?  ?  ?? CoreParametersButton      ?
?  ?  ?  ?? TopologyDetailsButton     ?
?  ?  ?  ?? InflowDetailsButton       ?
?  ?  ?                                ?
?  ?  ?? MechanismsSection ? KEY!     ?
?  ?  ?  ?? ContentPanel              ?
?  ?  ?  ?  ?? TopologyRow            ?
?  ?  ?  ?  ?  ?? Dropdown ????????   ?
?  ?  ?  ?  ?? BoundaryRow         ?   ?
?  ?  ?  ?  ?  ?? Dropdown ????????   ?
?  ?  ?  ?  ?? InflowRow           ?   ?
?  ?  ?  ?  ?  ?? Dropdown ????????   ?
?  ?  ?  ?  ?? DiffusionRow        ?   ?
?  ?  ?  ?  ?  ?? Dropdown ????????   ?
?  ?  ?  ?  ?? ViabilityRow        ?   ?
?  ?  ?  ?  ?  ?? Dropdown ????????   ?
?  ?  ?  ?  ?? PhaseSetRow         ?   ?
?  ?  ?  ?     ?? Dropdown ????????   ?
?  ?  ?  ?                          ?   ?
?  ?  ?  ?? MechanismsSummaryText  ?   ?
?  ?  ?                             ?   ?
?  ?  ?? CoreParametersSection      ?   ?
?  ?  ?? TopologyDetailsSection     ?   ?
?  ?  ?? InflowDetailsSection       ?   ?
?  ?  ?? DiffusionDetailsSection    ?   ?
?  ?  ?? ViabilityDetailsSection    ?   ?
?  ?                                 ?   ?
?  ?? InspectPanel                  ?   ?
?  ?? ExportPanel                   ?   ?
?                                    ?   ?
?  All these dropdowns must be ??????   ?
?  wired in MechanismsSection!          ?
?                                        ?
?? DockModeController ??????????????????
   Controls panel switching
```

---

## ? **MechanismsSection - Inspector Wiring Checklist**

### **Step 1: Select MechanismsSection GameObject**

In Hierarchy: `RightDock ? ContentArea ? SetupPanel ? MechanismsSection`

### **Step 2: Check MechanismsSection Component**

In Inspector, verify **MechanismsSection (Script)** component shows:

```
MechanismsSection (Script)
?? Mechanism Dropdowns
?  ?? Topology Dropdown: [  None (TMP_Dropdown)  ] ? WIRE THIS!
?  ?? Boundary Dropdown: [  None (TMP_Dropdown)  ] ? WIRE THIS!
?  ?? Inflow Dropdown: [  None (TMP_Dropdown)  ] ? WIRE THIS!
?  ?? Diffusion Dropdown: [  None (TMP_Dropdown)  ] ? WIRE THIS!
?  ?? Viability Dropdown: [  None (TMP_Dropdown)  ] ? WIRE THIS!
?  ?? Phase Set Dropdown: [  None (TMP_Dropdown)  ] ? WIRE THIS!
?
?? External References
?  ?? Mechanism Summary Text: [  None (TextMeshProUGUI)  ] ? WIRE THIS!
?
?? Events
   ?? On Mechanism Changed() ? Event for notifying other sections
```

### **Step 3: Wire Each Dropdown**

**CRITICAL:** You must drag the **Dropdown child** from inside each Row, NOT the Row itself!

---

## ?? **Detailed Wiring Guide**

### **Mechanism Dropdowns**

| Field | Drag This GameObject | Location |
|-------|---------------------|----------|
| **Topology Dropdown** | MechanismsSection ? ContentPanel ? TopologyRow ? **Dropdown** | ?? NOT TopologyRow itself! |
| **Boundary Dropdown** | MechanismsSection ? ContentPanel ? BoundaryRow ? **Dropdown** | ?? NOT BoundaryRow itself! |
| **Inflow Dropdown** | MechanismsSection ? ContentPanel ? InflowRow ? **Dropdown** | ?? NOT InflowRow itself! |
| **Diffusion Dropdown** | MechanismsSection ? ContentPanel ? DiffusionRow ? **Dropdown** | ?? NOT DiffusionRow itself! |
| **Viability Dropdown** | MechanismsSection ? ContentPanel ? ViabilityRow ? **Dropdown** | ?? NOT ViabilityRow itself! |
| **Phase Set Dropdown** | MechanismsSection ? ContentPanel ? PhaseSetRow ? **Dropdown** | ?? NOT PhaseSetRow itself! |

### **External References**

| Field | Drag This GameObject | Purpose |
|-------|---------------------|---------|
| **Mechanism Summary Text** | MechanismsSection ? MechanismsSummaryText | Shows current selection summary |

---

## ?? **Functionality Testing**

### **Test 1: Dropdown Population on Activation**

**Expected Behavior:**
- When MechanismsSection becomes active (visible), dropdowns automatically populate

**How to Test:**
1. Press Play
2. Click "Mechanisms" button in HeaderSection
3. MechanismsSection expands
4. Check Console for:
   ```
   [MechanismsSection] OnEnable() called
   [MechanismsSection] Populating Topology dropdown (name: Dropdown, current options: X)
   [MechanismsSection] Topology dropdown populated with 2 options:
     - Full Domain
     - Masked Domain
   [MechanismsSection] Populating Boundary dropdown (name: Dropdown, current options: X)
   [MechanismsSection] Boundary dropdown populated with 3 options:
     - Closed (Reflective)
     - Open (Absorbing)
     - Wrap (Periodic)
   ... (continues for all dropdowns)
   ```

**Code Reference:**
```csharp
// Line 67-72 in MechanismsSection.cs
private void OnEnable()
{
    // Populate dropdowns when section becomes visible
    Debug.Log("[MechanismsSection] OnEnable() called");
    PopulateAllDropdowns();
}
```

**If Failed:**
- ? Console shows `[MechanismsSection] XXX dropdown is NULL!` ? Dropdown not wired
- ? No logs at all ? MechanismsSection component missing or disabled

---

### **Test 2: Topology Dropdown Options**

**Expected Behavior:**
- Topology dropdown shows 2 options

**How to Test:**
1. Press Play
2. Expand MechanismsSection
3. Click Topology dropdown
4. Verify options:
   - **Full Domain**
   - **Masked Domain**

**Code Reference:**
```csharp
// Line 86-100 in MechanismsSection.cs
private void PopulateTopologyDropdown()
{
    if (topologyDropdown == null)
    {
        Debug.LogError("[MechanismsSection] topologyDropdown is NULL!");
        return;
    }
    
    Debug.Log($"[MechanismsSection] Populating Topology dropdown...");
    topologyDropdown.ClearOptions();
    topologyDropdown.AddOptions(new List<string>
    {
        "Full Domain",
        "Masked Domain"
    });
    Debug.Log($"[MechanismsSection] Topology dropdown populated with 2 options");
}
```

---

### **Test 3: Boundary Dropdown Options**

**Expected Behavior:**
- Boundary dropdown shows 3 options

**How to Test:**
1. Click Boundary dropdown
2. Verify options:
   - **Closed (Reflective)**
   - **Open (Absorbing)**
   - **Wrap (Periodic)**

**Code Reference:**
```csharp
// Line 102-116 in MechanismsSection.cs
private void PopulateBoundaryDropdown()
{
    topologyDropdown.AddOptions(new List<string>
    {
        "Closed (Reflective)",
        "Open (Absorbing)",
        "Wrap (Periodic)"
    });
}
```

---

### **Test 4: Inflow Dropdown Options**

**Expected Behavior:**
- Inflow dropdown shows 3 options

**How to Test:**
1. Click Inflow dropdown
2. Verify options:
   - **Uniform Field**
   - **Point Sources**
   - **Edge Sources**

**Code Reference:**
```csharp
// Line 118-132 in MechanismsSection.cs
private void PopulateInflowDropdown()
{
    inflowDropdown.AddOptions(new List<string>
    {
        "Uniform Field",
        "Point Sources",
        "Edge Sources"
    });
}
```

**Special Note:** Selecting "Point Sources" should make **InflowDetailsSection** visible with "Edit Sources" button!

---

### **Test 5: Diffusion Dropdown Options**

**Expected Behavior:**
- Diffusion dropdown shows 3 options

**How to Test:**
1. Click Diffusion dropdown
2. Verify options:
   - **Von Neumann (4-neighbor)**
   - **Moore (8-neighbor)**
   - **Anisotropic**

**Code Reference:**
```csharp
// Line 134-148 in MechanismsSection.cs
private void PopulateDiffusionDropdown()
{
    diffusionDropdown.AddOptions(new List<string>
    {
        "Von Neumann (4-neighbor)",
        "Moore (8-neighbor)",
        "Anisotropic"
    });
}
```

---

### **Test 6: Viability Dropdown Options**

**Expected Behavior:**
- Viability dropdown shows 2 options

**How to Test:**
1. Click Viability dropdown
2. Verify options:
   - **Simple Threshold**
   - **Hysteresis**

**Code Reference:**
```csharp
// Line 150-164 in MechanismsSection.cs
private void PopulateViabilityDropdown()
{
    viabilityDropdown.AddOptions(new List<string>
    {
        "Simple Threshold",
        "Hysteresis"
    });
}
```

---

### **Test 7: PhaseSet Dropdown Options**

**Expected Behavior:**
- PhaseSet dropdown shows 2 options

**How to Test:**
1. Click PhaseSet dropdown
2. Verify options:
   - **Standard**
   - **Custom (Advanced)**

**Code Reference:**
```csharp
// Line 166-180 in MechanismsSection.cs
private void PopulatePhaseSetDropdown()
{
    phaseSetDropdown.AddOptions(new List<string>
    {
        "Standard",
        "Custom (Advanced)"
    });
}
```

---

### **Test 8: Configuration Binding**

**Expected Behavior:**
- Dropdowns set to correct values when bound to a configuration

**How to Test:**
1. This is tested automatically when simulation loads
2. Dropdowns should match the current preset's settings

**Code Reference:**
```csharp
// Line 186-203 in MechanismsSection.cs
public override void Bind(Configuration.WorkingScenarioConfig config)
{
    currentConfig = config;

    // Set dropdown values from config
    if (topologyDropdown != null)
        topologyDropdown.value = (int)config.Topology;

    if (boundaryDropdown != null)
        boundaryDropdown.value = (int)config.Boundary;

    if (inflowDropdown != null)
        inflowDropdown.value = (int)config.Inflow;

    // ... continues for all dropdowns

    UpdateMechanismSummary();
}
```

---

### **Test 9: Dropdown Changes Update Configuration**

**Expected Behavior:**
- Changing a dropdown immediately updates the configuration

**How to Test:**
1. Press Play
2. Change Topology to "Masked Domain"
3. Configuration should update automatically
4. MechanismsSummaryText should update

**Code Reference:**
```csharp
// Line 229-240 in MechanismsSection.cs
private void OnDropdownChanged()
{
    if (currentConfig != null)
    {
        // Apply changes immediately to working config
        ApplyEdits(currentConfig);
        UpdateMechanismSummary();

        // Notify listeners (to refresh detail sections)
        OnMechanismChanged?.Invoke();
    }
}
```

---

### **Test 10: Mechanism Summary Updates**

**Expected Behavior:**
- MechanismsSummaryText shows current selections

**How to Test:**
1. Press Play
2. Check bottom of MechanismsSection
3. Should see text like:
   ```
   Topology: FullDomain • Boundary: Closed • Inflow: UniformField • Diffusion: VonNeumann4 • Viability: Simple
   ```
4. Change a dropdown
5. Summary text updates

**Code Reference:**
```csharp
// Line 242-252 in MechanismsSection.cs
private void UpdateMechanismSummary()
{
    if (mechanismSummaryText != null && currentConfig != null)
    {
        mechanismSummaryText.text = 
            $"Topology: {GetFriendlyName(currentConfig.Topology)} • " +
            $"Boundary: {GetFriendlyName(currentConfig.Boundary)} • " +
            $"Inflow: {GetFriendlyName(currentConfig.Inflow)} • " +
            $"Diffusion: {GetFriendlyName(currentConfig.Diffusion)} • " +
            $"Viability: {GetFriendlyName(currentConfig.ViabilityRule)}";
    }
}
```

---

## ?? **Common Issues & Fixes**

### **Issue A: Dropdowns show "Full Masked Domain; Option B; Option C"**

**Cause:** ALL dropdowns are wired to the SAME dropdown (TopologyDropdown)

**Fix:**
1. Select MechanismsSection
2. Check Inspector - you'll see all fields point to same dropdown
3. **Wire each field to its OWN dropdown:**
   - topologyDropdown ? TopologyRow ? **Dropdown**
   - boundaryDropdown ? BoundaryRow ? **Dropdown**
   - inflowDropdown ? InflowRow ? **Dropdown**
   - etc.

---

### **Issue B: "XXX dropdown is NULL!" error**

**Cause:** Dropdown not wired in Inspector

**Fix:**
1. Note which dropdown is NULL (e.g., "topologyDropdown")
2. Find corresponding Row in Hierarchy
3. Expand Row ? Find **Dropdown** child
4. Drag Dropdown to Inspector field

---

### **Issue C: Dropdown window too small, options cut off**

**Cause:** Dropdown Template height too small

**Fix:** For EACH dropdown:
1. Select Dropdown GameObject
2. Expand ? Find **Template** child
3. Select Template
4. Inspector ? RectTransform ? Height: **250-300**
5. Repeat for all 6 dropdowns

See: `DROPDOWN_TEMPLATE_HEIGHT_FIX.md`

---

### **Issue D: No logs when expanding MechanismsSection**

**Cause:** MechanismsSection component missing or disabled

**Fix:**
1. Select MechanismsSection GameObject
2. Inspector ? Check **MechanismsSection (Script)** exists
3. Check checkbox next to component is **checked** (enabled)
4. Check checkbox next to GameObject name is **checked** (active)

---

## ?? **Quick Verification Checklist**

**After wiring, verify ALL dropdown fields show DIFFERENT dropdowns:**

- [ ] ? Topology Dropdown ? Shows "Dropdown (TMP_Dropdown)" from **TopologyRow**
- [ ] ? Boundary Dropdown ? Shows "Dropdown (TMP_Dropdown)" from **BoundaryRow**
- [ ] ? Inflow Dropdown ? Shows "Dropdown (TMP_Dropdown)" from **InflowRow**
- [ ] ? Diffusion Dropdown ? Shows "Dropdown (TMP_Dropdown)" from **DiffusionRow**
- [ ] ? Viability Dropdown ? Shows "Dropdown (TMP_Dropdown)" from **ViabilityRow**
- [ ] ? Phase Set Dropdown ? Shows "Dropdown (TMP_Dropdown)" from **PhaseSetRow**
- [ ] ? Mechanism Summary Text ? Shows "MechanismsSummaryText (TextMeshProUGUI)"

**IMPORTANT:** In Inspector, clicking each dropdown field should highlight a **DIFFERENT** GameObject in Hierarchy!

---

## ?? **Final Test: Full Functionality**

**Run this complete test sequence:**

1. **Press Play** ? No console errors ?
2. **Click "Mechanisms" button** ? Section expands ?
3. **Check Console** ? See "[MechanismsSection] OnEnable() called" ?
4. **Check Console** ? See 6 "dropdown populated" messages ?
5. **Click Topology dropdown** ? Shows "Full Domain", "Masked Domain" ?
6. **Click Boundary dropdown** ? Shows 3 boundary options ?
7. **Click Inflow dropdown** ? Shows 3 inflow options ?
8. **Click Diffusion dropdown** ? Shows 3 diffusion options ?
9. **Click Viability dropdown** ? Shows 2 viability options ?
10. **Click PhaseSet dropdown** ? Shows 2 phaseset options ?
11. **Change a dropdown** ? Summary text updates ?
12. **Select Inflow: "Point Sources"** ? InflowDetailsSection appears ?

**If ALL tests pass:** ? MechanismsSection is fully functional!

---

## ?? **Result**

? **RightDock/MechanismsSection configured**  
? **All 6 dropdowns wired correctly**  
? **All functionality verified**  
? **Ready for production use**  

**Your RightDock is complete!** ??
