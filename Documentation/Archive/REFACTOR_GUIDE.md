# UI Refactor Guide: Dropdown-Based Panels with Value Persistence

**Date:** 2026-01-26  
**Goal:** Replace tab buttons with dropdown, ensure values persist across panel switches, add Reset functionality

---

## ?? **What Changed**

### **Before (Tab Buttons):**
```
[Setup] [Inspect] [Export] ? 3 buttons
```
- Tab buttons at top
- Values could get lost when switching
- No Reset functionality

### **After (Dropdown):**
```
Mode: [Setup ?] ? Single dropdown
```
- One dropdown selector
- Values ALWAYS persist (WorkingScenarioConfig pattern)
- Reset button in TopBar
- Cleaner, simpler UI

---

## ?? **New Architecture**

### **Key Components:**

1. **WorkingScenarioConfig** (Already exists)
   - **Source of truth** for all UI values
   - Holds mechanisms, parameters, point sources, etc.
   - Changes only apply to Engine on "Apply & Restart"

2. **UIController** (NEW)
   - Singleton that holds `WorkingScenarioConfig` instance
   - Coordinates all sections
   - Provides `RefreshAllSections()` for Reset
   - Guards against event loops with `_isRefreshing` flag

3. **DockModeController** (NEW)
   - Manages dropdown-based panel switching
   - **Only toggles visibility** - does NOT touch data!
   - Replaces RightDockUI tab button logic

4. **WorkingScenarioConfigAdapter** (NEW)
   - Converts `ScenarioPreset` ? `WorkingScenarioConfig`
   - Converts `WorkingScenarioConfig` ? `ScenarioPreset`
   - Deep copies point sources and parameters

---

## ?? **Value Persistence Flow**

### **How It Works:**

```
User edits value in UI
    ?
Value immediately written to WorkingScenarioConfig
    ?
User switches panel (dropdown change)
    ?
DockModeController: SetActive(true/false) panels ONLY
    ?
User switches back
    ?
Values still there! (because WorkingScenarioConfig never changed)
```

### **Critical Rules:**

1. ? **UI controls write to WorkingScenarioConfig immediately** on edit
2. ? **Panel switching ONLY toggles visibility** (no data operations)
3. ? **Bind() is called ONCE** when preset loads or Reset pressed
4. ? **Refresh guard prevents event loops** during Bind()

---

## ?? **Unity Scene Setup**

### **Step 1: Update RightDock Hierarchy**

**Current structure (OLD):**
```
RightDock
??? TabButtonRow
?   ??? SetupTabButton
?   ??? InspectTabButton
?   ??? ExportTabButton
??? ContentArea
    ??? SetupPanel
    ??? InspectPanel
    ??? ExportPanel
```

**New structure (REQUIRED):**
```
RightDock
??? DockModeRow (NEW)
?   ??? ModeLabel ("Mode:")
?   ??? ModeDropdown (TMP_Dropdown)
??? ContentArea
    ??? SetupPanel
    ??? InspectPanel
    ??? ExportPanel
```

**Instructions:**

1. **Delete or disable `TabButtonRow`** and its 3 button children
2. **Create `DockModeRow`:**
   - Right-click `RightDock` ? Create Empty
   - Name: `DockModeRow`
   - Add `Horizontal Layout Group`:
     - Padding: 8
     - Spacing: 8
     - Child Alignment: Middle Left
   - Add `Layout Element`:
     - Preferred Height: 44

3. **Add `ModeLabel`:**
   - Right-click `DockModeRow` ? UI ? Text - TextMeshPro
   - Name: `ModeLabel`
   - Text: "Mode:"
   - Add `Layout Element`:
     - Preferred Width: 50

4. **Add `ModeDropdown`:**
   - Right-click `DockModeRow` ? UI ? Dropdown - TextMeshPro
   - Name: `ModeDropdown`
   - Add `Layout Element`:
     - Preferred Width: 220
   - Options will be populated by script

---

### **Step 2: Add DockModeController**

1. **Select `RightDock` GameObject**
2. **Add Component** ? Search "DockModeController" ? Click Add
3. **In Inspector, wire fields:**
   - `modeDropdown` ? Drag `ModeDropdown`
   - `setupPanel` ? Drag `SetupPanel`
   - `inspectPanel` ? Drag `InspectPanel`
   - `exportPanel` ? Drag `ExportPanel`

---

### **Step 3: Update RightDockUI Component**

1. **Select `RightDock` GameObject**
2. **Find `RightDockUI` component** in Inspector
3. **Remove old tab button references** (they no longer exist in the script)
4. **Wire new field:**
   - `dockModeController` ? Drag `RightDock` (DockModeController component)

---

### **Step 4: Create UIController GameObject**

1. **In Hierarchy**, right-click root ? Create Empty
2. **Name:** `UIController`
3. **Add Component** ? Search "UIController" ? Click Add
4. **In Inspector:**
   - `workingConfig` ? Leave empty (will initialize automatically)
   - `currentPreset` ? Leave empty (will be set when preset loads)
   - `sections` ? Leave empty for now (sections register themselves)

---

### **Step 5: Add Reset Button to TopBar**

1. **Select `TopBar` GameObject**
2. **Find a good position** (e.g., after Seed input, before spacer)
3. **Right-click `TopBar`** ? UI ? Button - TextMeshPro
4. **Name:** `ResetButton`
5. **Set text:** "Reset"
6. **Add Layout Element:**
   - Preferred Width: 70
7. **Wire onClick:**
   - In Inspector, find Button component
   - onClick event ? Click `+`
   - Drag `UIController` GameObject
   - Select `UIController` ? `ResetToPresetDefaults()`

---

## ?? **Section Script Updates**

All section scripts (MechanismsSection, CoreParametersSection, etc.) need these updates:

### **Pattern:**

```csharp
public class ExampleSection : CollapsibleSection, IConfigSection
{
    private WorkingScenarioConfig currentConfig;

    public void Bind(WorkingScenarioConfig config)
    {
        currentConfig = config;

        // Guard: Don't trigger events during bind
        if (UIController.Instance != null && UIController.Instance.IsRefreshing)
            return;

        // Update UI controls from config
        someDropdown.value = (int)config.SomeMode;
        someInput.text = config.SomeValue.ToString();
    }

    private void Start()
    {
        // Wire UI events
        someDropdown.onValueChanged.AddListener(OnDropdownChanged);
        someInput.onEndEdit.AddListener(OnInputChanged);

        // Register with UIController
        if (UIController.Instance != null)
            UIController.Instance.RegisterSection(this);
    }

    private void OnDropdownChanged(int value)
    {
        // Guard: Don't write during refresh
        if (UIController.Instance != null && UIController.Instance.IsRefreshing)
            return;

        // Write immediately to WorkingConfig
        if (currentConfig != null)
        {
            currentConfig.SomeMode = (SomeEnum)value;
            Debug.Log($"[ExampleSection] Updated SomeMode to {currentConfig.SomeMode}");
        }
    }

    private void OnInputChanged(string text)
    {
        // Guard
        if (UIController.Instance != null && UIController.Instance.IsRefreshing)
            return;

        // Write immediately
        if (currentConfig != null && double.TryParse(text, out double val))
        {
            currentConfig.SomeValue = val;
        }
    }

    private void OnDestroy()
    {
        // Unregister
        if (UIController.Instance != null)
            UIController.Instance.UnregisterSection(this);
    }
}
```

### **Key Points:**

1. **Guard with `UIController.Instance.IsRefreshing`** in:
   - All `onValueChanged` handlers
   - All `onEndEdit` handlers
   - Bind() method

2. **Write to `currentConfig` immediately** on every edit

3. **Register/Unregister** with UIController

---

## ? **Testing Checklist**

### **Test 1: Value Persistence**
- [ ] Open Unity, press Play
- [ ] Change Topology to "Masked Domain"
- [ ] Change Mask Shape to "Ring"
- [ ] Enter Outer Radius: 25
- [ ] Enter Inner Radius: 12
- [ ] Switch dropdown to "Inspect"
- [ ] Switch back to "Setup"
- [ ] **Result:** All values should be exactly as entered (25, 12, etc.)

### **Test 2: Reset Functionality**
- [ ] Edit multiple values (mechanisms, parameters, etc.)
- [ ] Press "Reset" button in TopBar
- [ ] **Result:** All values revert to preset defaults

### **Test 3: Mechanism Visibility**
- [ ] Change Diffusion to "Anisotropic"
- [ ] **Result:** DiffusionDetailsSection appears
- [ ] Switch to Inspect tab (dropdown)
- [ ] Switch back to Setup
- [ ] **Result:** DiffusionDetailsSection still visible

### **Test 4: No Event Loops**
- [ ] Press Reset button
- [ ] Check Console
- [ ] **Result:** No infinite loop errors, no stack overflow

---

## ?? **Troubleshooting**

### **Issue: Values reset when switching panels**
- **Cause:** Section is calling `Bind()` on panel activation
- **Fix:** Remove any `OnEnable()` calls to `Bind()` - only call from UIController

### **Issue: Infinite event loop on Reset**
- **Cause:** Missing `UIController.Instance.IsRefreshing` guard
- **Fix:** Add guard to ALL onValueChanged/onEndEdit handlers

### **Issue: Dropdown doesn't populate**
- **Cause:** DockModeController.Start() not running
- **Fix:** Ensure DockModeController component is enabled

### **Issue: Reset button doesn't work**
- **Cause:** No preset loaded in UIController
- **Fix:** Load a preset first (preset dropdown ? Load button)

---

## ?? **Implementation Checklist**

- [ ] ? Created DockModeController.cs
- [ ] ? Updated RightDockUI.cs
- [ ] ? Created UIController.cs
- [ ] ? Created WorkingScenarioConfigAdapter.cs
- [ ] Updated Unity scene:
  - [ ] Replaced TabButtonRow with DockModeRow
  - [ ] Added ModeLabel + ModeDropdown
  - [ ] Wired DockModeController
  - [ ] Added UIController GameObject
  - [ ] Added Reset button to TopBar
- [ ] Updated section scripts:
  - [ ] MechanismsSection
  - [ ] TopologyDetailsSection
  - [ ] InflowDetailsSection
  - [ ] CoreParametersSection
  - [ ] (DiffusionDetailsSection - when built)
  - [ ] (ViabilityDetailsSection - when built)
- [ ] Tested value persistence
- [ ] Tested Reset functionality
- [ ] Verified no event loops

---

## ?? **Next Steps**

After completing this refactor:

1. **Test thoroughly** (use checklist above)
2. **Build missing sections** (Diffusion, Viability) with new pattern
3. **Implement InspectTab** (Stage 13.12)
4. **Implement ExportTab** (Stage 13.13)
5. **Final integration testing**

---

**Status:** ? Scripts created, Unity scene setup required

**Time estimate:** 30-45 minutes for scene setup + section updates
