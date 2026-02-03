# VIABLE Compact Instrument Panel UI - Implementation Summary

## Status: ? Foundation Complete

### What We've Built

#### 1. **Data Model**
- ? `WorkingScenarioConfig.cs` - Unity-side config model
  - Holds all mechanism modes and parameters
  - Stays in memory until "Apply & Restart"
  - Provides `GetMechanismSummary()` for display
  - Location: `Assets/Viable/Core.Unity/Configuration/`

#### 2. **UI Framework**
- ? `IConfigSection.cs` - Interface and base classes
  - `IConfigSection` interface (Bind/RefreshVisibility/ApplyEdits)
  - `CollapsibleSection` base class (expand/collapse)
  - `ContextSensitiveSection` base class (auto-hide when irrelevant)
  - Location: `Assets/Viable/Core.Unity/UI/`

#### 3. **TopBar UI**
- ? `TopBarUI.cs` - Top bar controller
  - Preset dropdown + Load button
  - Run controls (Play/Pause/Step/Restart)
  - Speed dropdown
  - Seed input field
  - Export button
- ? `TOPBAR_SETUP.md` - Complete Unity setup instructions

#### 4. **RightDock UI**
- ? `RightDockUI.cs` - Right dock with tabs
  - Tab system (Setup/Inspect/Export)
  - Fixed width, fixed height, **no scroll**
  - Tab switching logic
- ? `RIGHTDOCK_SETUP.md` - Complete Unity setup instructions

#### 5. **MechanismsSection**
- ? `MechanismsSection.cs` - Core mechanism dropdowns
  - Topology, Boundary, Inflow, Diffusion, Viability, PhaseSet dropdowns
  - Mechanism summary text (read-only)
  - Binds to WorkingScenarioConfig
  - Fires event when mechanisms change (to refresh detail sections)

---

## Next Steps (To Be Implemented)

### **Context-Sensitive Detail Sections** (Step 6)
These hide/show based on mechanism selections:

1. **TopologyDetailsSection**
   - Visible only when Topology == MaskedDomain
   - MaskShape dropdown
   - Shape-specific parameters (radius, width, probability, etc.)

2. **InflowDetailsSection**
   - Visible only when Inflow == PointSources
   - Point source list (max 5 visible)
   - Add/Remove buttons
   - "Edit…" button for modal editor

3. **DiffusionDetailsSection**
   - Visible only when Diffusion == Anisotropic
   - Direction dropdown (N/E/S/W)
   - Bias slider

4. **ViabilityDetailsSection**
   - Visible only when ViabilityRule == Hysteresis
   - OnThreshold field
   - OffThreshold field

### **Core Parameters Section** (Step 7)
- 6-12 curated parameters with semantic labels
- Compact rows: `Label | Input | (Q/step)`
- "Advanced…" button for modal

### **Inspect Tab** (Step 8)
- Live metrics (read-only)
- Color legend
- Updates per frame

### **Export Tab** (Step 9)
- Export level dropdown
- Sample rate field
- Export button

### **Apply & Restart Flow** (Step 10)
- Convert WorkingScenarioConfig ? ScenarioDefinition
- Wire to SimulationController
- Ensure reproducibility

### **Modals** (Steps 11-12)
- AdvancedParametersModal (category pages, no scroll)
- PointSourceEditorModal (paginated list, no scroll)

### **Integration** (Step 13)
- Wire everything to UIManager
- Initialize all sections
- Handle tab switching

---

## Key Design Principles Implemented

? **No Scroll Views** - Fixed height panels with context-sensitive visibility
? **Progressive Disclosure** - Hide irrelevant controls
? **Mechanism Summary** - One-line read-only status
? **Compact Layout** - Grid visual remains dominant
? **Immediate Updates** - Changes go to WorkingScenarioConfig
? **Reproducibility** - Only "Apply & Restart" pushes to Engine

---

## Files Created

### C# Scripts
```
Assets/Viable/Core.Unity/Configuration/
?? WorkingScenarioConfig.cs

Assets/Viable/Core.Unity/UI/
?? IConfigSection.cs
?? TopBarUI.cs
?? RightDockUI.cs
?? MechanismsSection.cs
?? TOPBAR_SETUP.md
?? RIGHTDOCK_SETUP.md
```

---

## Unity Setup Instructions

### **Immediate Next Steps:**

1. **Open Unity Editor**
   - Let Unity compile the new scripts
   - Generate .meta files

2. **Build TopBar UI** (follow `TOPBAR_SETUP.md`)
   - Create TopBar GameObject with horizontal layout
   - Add all buttons, dropdowns, input fields
   - Wire to TopBarUI script

3. **Build RightDock UI** (follow `RIGHTDOCK_SETUP.md`)
   - Create RightDock GameObject with tab buttons
   - Create 3 tab panels (Setup/Inspect/Export)
   - Add MechanismSummary text to Setup panel
   - Wire to RightDockUI script

4. **Add MechanismsSection**
   - In SetupPanel, create collapsible section
   - Add 6 dropdowns for mechanisms
   - Wire to MechanismsSection script

5. **Test Tab Switching**
   - Press Play
   - Click tab buttons
   - Verify only one panel visible at a time

---

## Testing Checklist

After building TopBar + RightDock:

- [ ] TopBar visible at top, single row
- [ ] RightDock visible on right, fixed width
- [ ] Tab buttons switch between Setup/Inspect/Export panels
- [ ] Mechanism dropdowns populate correctly
- [ ] Mechanism summary updates when dropdowns change
- [ ] No scroll bars anywhere
- [ ] Grid visual remains prominent in center

---

## What's Working Now

? **Data model** - WorkingScenarioConfig holds all settings
? **UI framework** - IConfigSection pattern ready
? **TopBar** - All controls defined
? **RightDock** - Tab system defined
? **MechanismsSection** - Core mechanism controls defined

## What's Next

?? **Context-sensitive sections** - TopologyDetails, InflowDetails, etc.
?? **Core Parameters section** - Curated parameter list
?? **Inspect tab** - Live metrics
?? **Export tab** - Export configuration
?? **Apply & Restart** - Wire to SimulationController
?? **Modals** - Advanced parameters, point source editor

---

**Ready to build the UI in Unity!** Follow the setup instructions in `TOPBAR_SETUP.md` and `RIGHTDOCK_SETUP.md`. ??
