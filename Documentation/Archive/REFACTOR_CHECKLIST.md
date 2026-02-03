# UI Refactor - Quick Start Checklist

**Goal:** Replace tab buttons with dropdown, ensure value persistence, add Reset button

**Time:** 1-2 hours total

---

## ? **Phase 1: Unity Scene Setup (30-45 min)**

### **Task 1.1: Replace Tab Buttons with Dropdown**

- [ ] Open Unity scene
- [ ] In Hierarchy, find: `RightDock` ? `TabButtonRow`
- [ ] **Delete** or disable `TabButtonRow` (has 3 button children)
- [ ] Right-click `RightDock` ? Create Empty ? Name: `DockModeRow`
- [ ] Select `DockModeRow`, add components:
  - [ ] `Horizontal Layout Group` (Padding: 8, Spacing: 8)
  - [ ] `Layout Element` (Preferred Height: 44)
- [ ] Right-click `DockModeRow` ? UI ? Text - TextMeshPro ? Name: `ModeLabel`
  - [ ] Text: "Mode:"
  - [ ] Layout Element: Preferred Width: 50
- [ ] Right-click `DockModeRow` ? UI ? Dropdown - TextMeshPro ? Name: `ModeDropdown`
  - [ ] Layout Element: Preferred Width: 220
- [ ] Save scene (Ctrl+S)

**Time:** 10 min

---

### **Task 1.2: Add DockModeController**

- [ ] Select `RightDock` GameObject
- [ ] Add Component ? Search "DockModeController"
- [ ] In Inspector, wire fields:
  - [ ] `modeDropdown` ? Drag `ModeDropdown`
  - [ ] `setupPanel` ? Drag `SetupPanel`
  - [ ] `inspectPanel` ? Drag `InspectPanel`
  - [ ] `exportPanel` ? Drag `ExportPanel`
- [ ] Save scene

**Time:** 5 min

---

### **Task 1.3: Update RightDockUI**

- [ ] Select `RightDock` GameObject
- [ ] Find `RightDockUI` component in Inspector
- [ ] Clear old tab button fields (they're removed from script)
- [ ] Wire new field:
  - [ ] `dockModeController` ? Drag `RightDock` (DockModeController component)
- [ ] Save scene

**Time:** 2 min

---

### **Task 1.4: Create UIController**

- [ ] In Hierarchy, right-click root ? Create Empty
- [ ] Name: `UIController`
- [ ] Add Component ? Search "UIController"
- [ ] In Inspector:
  - [ ] `workingConfig` ? Leave empty (auto-initializes)
  - [ ] `currentPreset` ? Leave empty (set on load)
  - [ ] `sections` ? Leave empty (sections register themselves)
- [ ] Save scene

**Time:** 3 min

---

### **Task 1.5: Add Reset Button**

- [ ] Select `TopBar` GameObject
- [ ] Find good position (e.g., after Seed input)
- [ ] Right-click `TopBar` ? UI ? Button - TextMeshPro
- [ ] Name: `ResetButton`
- [ ] Button text: "Reset"
- [ ] Add Layout Element: Preferred Width: 70
- [ ] In Inspector, Button component ? onClick:
  - [ ] Click `+`
  - [ ] Drag `UIController` GameObject
  - [ ] Select: `UIController` ? `ResetToPresetDefaults()`
- [ ] Save scene

**Time:** 5 min

---

## ? **Phase 2: Update Section Scripts (20-30 min each)**

For **each** section (MechanismsSection, TopologyDetailsSection, etc.):

### **Task 2.1: Add Refresh Guard to Bind()**

```csharp
public void Bind(WorkingScenarioConfig config)
{
    currentConfig = config;
    
    // ADD THIS:
    if (UIController.Instance != null && UIController.Instance.IsRefreshing)
        return;
    
    // ... existing UI update code
}
```

---

### **Task 2.2: Add Guards to All Handlers**

```csharp
private void OnDropdownChanged(int value)
{
    // ADD THIS:
    if (UIController.Instance != null && UIController.Instance.IsRefreshing)
        return;
    
    // ... existing write to currentConfig
}

private void OnInputChanged(string text)
{
    // ADD THIS:
    if (UIController.Instance != null && UIController.Instance.IsRefreshing)
        return;
    
    // ... existing write to currentConfig
}
```

**Apply to ALL:**
- `onValueChanged` listeners
- `onEndEdit` listeners
- Any method that writes to `currentConfig`

---

### **Task 2.3: Register/Unregister with UIController**

```csharp
private void Start()
{
    // ... existing event wiring

    // ADD THIS:
    if (UIController.Instance != null)
        UIController.Instance.RegisterSection(this);
}

private void OnDestroy()
{
    // ADD THIS:
    if (UIController.Instance != null)
        UIController.Instance.UnregisterSection(this);
}
```

---

### **Sections Checklist:**

- [ ] `MechanismsSection.cs`
  - [ ] Add guard to Bind()
  - [ ] Add guards to all 6 dropdown handlers
  - [ ] Register/Unregister
- [ ] `TopologyDetailsSection.cs`
  - [ ] Add guard to Bind()
  - [ ] Add guards to dropdown + input handlers
  - [ ] Register/Unregister
- [ ] `InflowDetailsSection.cs`
  - [ ] Add guard to Bind()
  - [ ] Add guards to button handler
  - [ ] Register/Unregister
- [ ] `CoreParametersSection.cs`
  - [ ] Add guard to Bind()
  - [ ] Add guards to all 8 input handlers
  - [ ] Register/Unregister

**Time:** 20-30 min per section × 4 sections = 80-120 min

---

## ? **Phase 3: Testing (15-20 min)**

### **Test 1: Panel Switching**

- [ ] Press Play
- [ ] Mode dropdown shows "Setup" by default
- [ ] Change to "Inspect" ? InspectPanel visible
- [ ] Change to "Export" ? ExportPanel visible
- [ ] Change back to "Setup" ? SetupPanel visible

---

### **Test 2: Value Persistence**

- [ ] In Setup panel, change:
  - [ ] Topology: "Masked Domain"
  - [ ] Mask Shape: "Ring"
  - [ ] Outer Radius: 25
  - [ ] Inner Radius: 12
- [ ] Switch to "Inspect" (dropdown)
- [ ] Switch back to "Setup"
- [ ] **Verify:** All values still 25, 12, Ring, Masked Domain ?

---

### **Test 3: Reset Functionality**

- [ ] Edit multiple values (mechanisms, parameters)
- [ ] Press "Reset" button in TopBar
- [ ] **Verify:** All values revert to preset defaults ?

---

### **Test 4: No Event Loops**

- [ ] Press Reset
- [ ] Check Console
- [ ] **Verify:** No errors, no infinite loops ?

---

### **Test 5: Mechanism Visibility**

- [ ] Change Diffusion to "Anisotropic"
- [ ] **Verify:** DiffusionDetailsSection appears ?
- [ ] Switch to Inspect
- [ ] Switch back to Setup
- [ ] **Verify:** DiffusionDetailsSection still visible ?

---

## ? **Phase 4: Final Checks**

- [ ] Build project (Ctrl+B) ? No compile errors
- [ ] Play scene ? No runtime errors
- [ ] All sections show/hide correctly
- [ ] Values persist across panel switches
- [ ] Reset button works
- [ ] Console clean (no warnings/errors)

---

## ?? **If Something Breaks:**

### **Dropdown doesn't populate:**
- Check: DockModeController.Start() is running
- Check: modeDropdown field is wired

### **Values reset when switching:**
- Check: No Bind() calls in OnEnable()
- Check: Handlers write to currentConfig immediately

### **Infinite loop on Reset:**
- Check: ALL handlers have IsRefreshing guard
- Check: Bind() has IsRefreshing guard

### **Reset button doesn't work:**
- Check: UIController exists in scene
- Check: onClick event wired correctly
- Check: Preset loaded first

---

## ?? **Progress Tracker**

**Phase 1: Unity Scene** ?
- [ ] Task 1.1: Replace tabs with dropdown
- [ ] Task 1.2: Add DockModeController
- [ ] Task 1.3: Update RightDockUI
- [ ] Task 1.4: Create UIController
- [ ] Task 1.5: Add Reset button

**Phase 2: Update Scripts** ?
- [ ] MechanismsSection
- [ ] TopologyDetailsSection
- [ ] InflowDetailsSection
- [ ] CoreParametersSection

**Phase 3: Testing** ?
- [ ] Panel switching works
- [ ] Values persist
- [ ] Reset works
- [ ] No event loops
- [ ] Mechanism visibility

**Phase 4: Final Checks** ?
- [ ] Build succeeds
- [ ] No runtime errors
- [ ] Everything works smoothly

---

## ?? **When Complete:**

? Cleaner UI (dropdown instead of buttons)  
? Values **always** persist  
? Reset button works  
? Ready for next stages (InspectTab, ExportTab)

---

**Estimated Total Time:** 1-2 hours

**Start with:** Phase 1 (Unity scene setup)

**Good luck!** ??
