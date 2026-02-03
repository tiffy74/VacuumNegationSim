# Troubleshooting Guide - Viable Engine UI

## ?? **Quick Problem Finder**

**Select your problem category:**
- [Dropdown Issues](#dropdown-issues)
- [Preset Loading Issues](#preset-loading-issues)
- [UI Positioning Issues](#ui-positioning-issues)
- [Modal/Editor Issues](#modaleditor-issues)
- [Simulation Not Responding to UI](#simulation-not-responding-to-ui)

---

## ?? **Dropdown Issues**

### **Problem 1: All Dropdowns Show Same Options**

**Symptom:** Every dropdown shows "Full Masked Domain; Option B; Option C"

**Root Cause:** All dropdown fields wired to the same GameObject

**Solution:**
1. Select MechanismsSection in Hierarchy
2. Inspector ? Check dropdown fields
3. Click each field ? Should highlight **different** GameObjects
4. If same GameObject highlighted for multiple fields:
   - Find the correct Row for each dropdown
   - Expand Row ? Find **Dropdown** child
   - Drag Dropdown to corresponding field

**Example:**
```
? CORRECT:
topologyDropdown ? TopologyRow ? Dropdown
boundaryDropdown ? BoundaryRow ? Dropdown
inflowDropdown ? InflowRow ? Dropdown

? WRONG:
topologyDropdown ? TopologyRow ? Dropdown
boundaryDropdown ? TopologyRow ? Dropdown  ? Same!
inflowDropdown ? TopologyRow ? Dropdown    ? Same!
```

**Full Guide:** [DROPDOWN_FIX_SUMMARY.md](../DROPDOWN_FIX_SUMMARY.md)

---

### **Problem 2: Dropdown Popup Too Small**

**Symptom:** Click dropdown ? Options cut off, can't see all choices

**Root Cause:** Template height too small

**Solution:** For EACH dropdown:
1. Select Dropdown GameObject in Hierarchy
2. Expand ? Select **Template** child
3. Inspector ? RectTransform ? Height: **250-300**
4. Verify Anchors:
   - Anchor Min Y: **0** (bottom)
   - Anchor Max Y: **0** (bottom)
   - Pivot Y: **1** (top)

**Quick Fix:** Set height to 300 for all dropdowns

**Full Guide:** [DROPDOWN_TEMPLATE_HEIGHT_FIX.md](../DROPDOWN_TEMPLATE_HEIGHT_FIX.md)

---

### **Problem 3: Dropdown is Empty**

**Symptom:** Click dropdown ? No options appear

**Root Cause:** Dropdown not populated on Start()

**Check Console:**
```
? GOOD:
[MechanismsSection] OnEnable() called
[MechanismsSection] Topology dropdown populated with 2 options

? BAD:
[MechanismsSection] topologyDropdown is NULL!
```

**Solution:**
1. Select section GameObject (e.g., MechanismsSection)
2. Inspector ? Find dropdown field (e.g., "Topology Dropdown")
3. Drag correct Dropdown child to field
4. Press Play ? Check Console for "populated" messages

**Full Guide:** [DROPDOWN_TROUBLESHOOTING.md](../DROPDOWN_TROUBLESHOOTING.md)

---

## ?? **Preset Loading Issues**

### **Problem 4: Preset Dropdown Empty**

**Symptom:** TopBar preset dropdown shows no presets

**Check Console:**
```
? BAD:
[TopBarUI] presetDropdown is null
[TopBarUI] No presets found, using fallback list
```

**Solution A: Dropdown Not Wired**
1. Select TopBar GameObject
2. Inspector ? TopBarUI component
3. Find **Preset Dropdown** field
4. Drag `TopBar ? PresetControls ? PresetDropdown` to field

**Solution B: Presets Missing**
1. Check folder: `Assets/Viable/Core.Unity/Presets/Examples/`
2. Should see 5 `.asset` files:
   - 01_Balanced_Growth.asset
   - 02_Resource_Collapse.asset
   - 03_RapidExpansion.asset
   - 04_Competitive.asset
   - 05_Stochastic_Dynamics.asset
3. If missing, create presets or restore from backup

**Full Guide:** [TOPBAR_PRESET_FIX.md](../TOPBAR_PRESET_FIX.md)

---

### **Problem 5: Preset Loads But Nothing Changes**

**Symptom:** Select preset ? Click Load ? Simulation looks the same

**Root Cause:** UI doesn't control simulation yet (Stage 13 issue)

**Workaround:**
1. Open preset file in Inspector
2. Manually edit values
3. Save preset
4. Restart Unity

**Permanent Fix:** See [UI_TO_SIMULATION_BRIDGE_FIX.md](../UI_TO_SIMULATION_BRIDGE_FIX.md)

---

## ?? **UI Positioning Issues**

### **Problem 6: UI Elements Overlap**

**Symptom:** Buttons/text overlapping, unreadable

**Solution:**
1. Select overlapping element
2. Inspector ? RectTransform
3. Check anchors match desired behavior:
   - Top-left corner: Anchors (0, 1, 0, 1)
   - Stretch horizontally: Anchors (0, 1, Y, Y)
   - Center: Anchors (0.5, 0.5, 0.5, 0.5)
4. Adjust Pos X/Y/Width/Height to position correctly

**Full Guide:** [UI_POSITIONING_YOUR_GAMEOBJECTS.md](../UI_POSITIONING_YOUR_GAMEOBJECTS.md)

---

### **Problem 7: Detail Section Escapes RightDock**

**Symptom:** InflowDetailsSection appears outside RightDock boundaries

**Solution:**
1. Select detail section GameObject
2. Set Parent: `RightDock ? ContentArea ? SetupPanel ? Content`
3. RectTransform:
   - Anchors: Stretch (0, 1, 0, 1)
   - Pos X/Y: 0, 0
   - Offset: 0, 0
4. Check parent has ContentSizeFitter (Vertical Fit: Preferred Size)

**Full Guide:** [URGENT_FIX_CHILDREN_ESCAPING_RIGHTDOCK.md](../URGENT_FIX_CHILDREN_ESCAPING_RIGHTDOCK.md)

---

## ?? **Modal/Editor Issues**

### **Problem 8: Point Source Editor Doesn't Open**

**Symptom:** Click "Edit Sources" ? Nothing happens

**Check Console:**
```
? BAD:
[InflowDetailsSection] PointSourceEditorModal reference is null!
[InflowDetailsSection] No config bound!
```

**Solution:**
1. Select InflowDetailsSection in Hierarchy
2. Inspector ? Find **Point Source Editor Modal** field
3. Drag PointSourceEditorModal GameObject to field
4. Verify InflowDetailsSection has config (auto-created in Start())

**Full Guide:** [POINT_SOURCE_EDITOR_GUIDE.md](../POINT_SOURCE_EDITOR_GUIDE.md)

---

### **Problem 9: Modal Opens But Grid Not Clickable**

**Symptom:** Point Source Editor opens, but can't click grid

**Solution:**
1. Select ModalGrid GameObject
2. Add **Grid Layout Group** component
3. Add **Toggle Group** component
4. Each cell needs **Toggle** component
5. Wire cell clicks to `OnCellClicked(int x, int y)` method

**Full Guide:** [POINT_SOURCE_EDITOR_GUIDE.md](../POINT_SOURCE_EDITOR_GUIDE.md) section 4

---

## ?? **Simulation Not Responding to UI**

### **Problem 10: Changing Dropdowns Doesn't Affect Simulation**

**Symptom:** Change Inflow dropdown ? Simulation looks the same

**Root Cause:** UI updates `WorkingScenarioConfig`, but simulation loads from `ScenarioPreset`

**Status:** ?? Known issue - Stage 13 in progress

**Temporary Workaround:**
1. Open preset file in Project window
2. Edit values directly in Inspector
3. Save preset
4. Restart Unity

**Permanent Fix:** [UI_TO_SIMULATION_BRIDGE_FIX.md](../UI_TO_SIMULATION_BRIDGE_FIX.md)

**Priority Tasks:**
1. Create `WorkingConfigToPresetAdapter.cs`
2. Implement "Apply & Restart" button
3. Wire button to `SimulationController.LoadPreset()`

---

### **Problem 11: Apply & Restart Button Does Nothing**

**Symptom:** Click "Apply & Restart" ? No logs, no changes

**Root Cause:** Button not implemented yet

**Check Code:**
```csharp
// Line 200 in TopBarUI.cs
private void OnApplyAndRestart()
{
    Debug.Log("[TopBarUI] Apply & Restart");
    // TODO: Convert workingConfig ? ScenarioDefinition/RunRequest
    // simulationController.ResetAndRun(...)
}
```

**Solution:** Implement the TODO (see Stage 13 guide)

---

## ?? **Event Wiring Issues**

### **Problem 12: Dropdown Change Doesn't Trigger Visibility**

**Symptom:** Change Inflow dropdown ? InflowDetailsSection doesn't appear

**Root Cause:** OnMechanismChanged event not wired

**Solution:**
1. Select MechanismsSection in Hierarchy
2. Inspector ? Find **On Mechanism Changed()** event
3. Click **+** to add listener
4. Drag each detail section GameObject to listener
5. Select function: `RefreshVisibility()`

**Repeat for all detail sections:**
- InflowDetailsSection
- TopologyDetailsSection
- DiffusionDetailsSection
- ViabilityDetailsSection

**Full Guide:** [WIRE_ONMECHANISMCHANGED_GUIDE.md](../WIRE_ONMECHANISMCHANGED_GUIDE.md)

---

## ?? **Quick Diagnostic Checklist**

### **UI Not Working? Check These:**

**TopBar:**
- [ ] TopBarUI component attached
- [ ] All 10 fields wired (dropdown, buttons, inputs)
- [ ] Console shows `[TopBarUI] Initialized`
- [ ] Console shows `[TopBarUI] Loaded X preset names`

**RightDock:**
- [ ] MechanismsSection component attached
- [ ] All 7 dropdown fields wired (each to DIFFERENT dropdown)
- [ ] Console shows `[MechanismsSection] OnEnable() called`
- [ ] Console shows 6 "dropdown populated" messages

**Presets:**
- [ ] 5 preset files exist in `Assets/Viable/Core.Unity/Presets/Examples/`
- [ ] Preset files are `.asset` files (not `.cs`)
- [ ] Console shows `Loaded X presets`

**Dropdowns:**
- [ ] Template height 250-300 for each dropdown
- [ ] Template anchors at bottom (Anchor Max Y = 0)
- [ ] Dropdown populated logs in Console

**Detail Sections:**
- [ ] Parent set to ContentArea ? SetupPanel ? Content
- [ ] OnMechanismChanged event wired
- [ ] RefreshVisibility() called on dropdown change

---

## ?? **Still Stuck?**

### **Check These Documents:**

**Configuration:**
- [TOPBAR_UI_CONFIGURATION_GUIDE.md](../TOPBAR_UI_CONFIGURATION_GUIDE.md) - 15 pages
- [RIGHTDOCK_UI_CONFIGURATION_GUIDE.md](../RIGHTDOCK_UI_CONFIGURATION_GUIDE.md) - 18 pages

**Testing:**
- [UI_TESTING_GUIDE.md](../UI_TESTING_GUIDE.md) - Test procedures
- [UI_COMPLETION_CHECKLIST.md](../UI_COMPLETION_CHECKLIST.md) - 87-point checklist

**Entry Point:**
- [UI_VERIFICATION_START_HERE.md](../UI_VERIFICATION_START_HERE.md) - Start here if lost

---

## ?? **Report a New Issue**

**Before reporting:**
1. Check Console for errors
2. Verify all fields wired in Inspector
3. Check this troubleshooting guide
4. Search existing documentation

**When reporting:**
- Screenshot of Console errors
- Screenshot of Inspector (component with issue)
- Steps to reproduce
- Expected vs actual behavior

---

**Last Updated:** 2024  
**Status:** ? Complete troubleshooting coverage for Stage 12
