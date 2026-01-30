# Stage 13: Unity UI Implementation - Progress Summary

**Last Updated:** 2026-01-26  
**Current Stage:** 13.10-13.11 (Unity UI Setup)

---

## ? Completed Stages (1-9)

You have already completed the **Engine-side mechanism configuration**:

| Stage | Status | What Was Done |
|-------|--------|---------------|
| **13.1** | ? | WorkingScenarioConfig data model |
| **13.2** | ? | IConfigSection interface |
| **13.3** | ? | TopBarUI prefab |
| **13.4** | ? | Point Sources mechanism |
| **13.5** | ? | Boundary modes |
| **13.6** | ? | Diffusion modes |
| **13.7** | ? | Hysteresis viability rule |
| **13.8** | ? | Masked domain topology |
| **13.9** | ? | Refinement stubs (future) |

**All Engine-side work complete!** Now implementing Unity UI.

---

## ?? Current Work (Stages 10-14)

### **Stage 13.10: Context-Sensitive Detail Sections** ?

**Document:** `STAGE13_10_UNITY_DETAIL_SECTIONS.md`

**What to build:**
- TopologyDetailsSection (conditional: Masked Domain)
- InflowDetailsSection (conditional: Point Sources)
- DiffusionDetailsSection (conditional: Anisotropic)
- ViabilityDetailsSection (conditional: Hysteresis)

**Status:** Instructions provided in Stage 13.1-9 style ?

---

### **Stage 13.11: CoreParametersSection** ?

**Document:** `STAGE13_11_UNITY_CORE_PARAMETERS.md`

**What to build:**
- 8 curated parameter input fields
- Scientific notation support
- Advanced Parameters button (placeholder)
- Always visible section

**Status:** Instructions provided in Stage 13.1-9 style ?

---

### **Stage 13.12: InspectTab** ?? NEXT

**What to build:**
- Live metrics display
- Update every frame (or configurable rate)
- Show: Step, Viable, Active, Sinks, Resource Global, Mean Viability
- Read-only text display

**Status:** Instructions needed

---

### **Stage 13.13: ExportTab** ?? TODO

**What to build:**
- Export level dropdown
- Sample rate input
- Export path display
- Export button
- Last export info

**Status:** Instructions needed

---

### **Stage 13.14: Apply & Restart Flow** ?? TODO

**What to implement:**
- Validate WorkingScenarioConfig
- Convert to ScenarioDefinition
- Call SimulationController.ResetAndRun()
- Error handling

**Status:** Instructions needed

---

## ?? Files Created (Stages 10-11)

### Code Files (Already Exist):
- `Assets/Viable/Core.Unity/Configuration/WorkingScenarioConfig.cs` ?
- `Assets/Viable/Core.Unity/UI/IConfigSection.cs` ?
- `Assets/Viable/Core.Unity/UI/TopBarUI.cs` ?
- `Assets/Viable/Core.Unity/UI/RightDockUI.cs` ?
- `Assets/Viable/Core.Unity/UI/MechanismsSection.cs` ?
- `Assets/Viable/Core.Unity/UI/TopologyDetailsSection.cs` ?
- `Assets/Viable/Core.Unity/UI/InflowDetailsSection.cs` ?
- `Assets/Viable/Core.Unity/UI/DiffusionDetailsSection.cs` ?
- `Assets/Viable/Core.Unity/UI/ViabilityDetailsSection.cs` ?
- `Assets/Viable/Core.Unity/UI/CoreParametersSection.cs` ?

### Setup Instructions (NEW - Stage 13.1-9 Style):
- `STAGE13_10_UNITY_DETAIL_SECTIONS.md` ? **Follow this!**
- `STAGE13_11_UNITY_CORE_PARAMETERS.md` ? **Follow this!**

---

## ?? Your Next Steps

### **Step 1:** Build Detail Sections (Stage 13.10)

**Follow:** `STAGE13_10_UNITY_DETAIL_SECTIONS.md`

**In Unity:**
1. Create TopologyDetailsSection GameObject in `SectionsContainer`
2. Create InflowDetailsSection GameObject
3. Create DiffusionDetailsSection GameObject
4. Create ViabilityDetailsSection GameObject
5. Wire Inspector fields for each
6. Wire MechanismsSection.OnMechanismChanged event

**Test:** Change mechanism dropdowns ? Sections show/hide correctly

---

### **Step 2:** Build Core Parameters (Stage 13.11)

**Follow:** `STAGE13_11_UNITY_CORE_PARAMETERS.md`

**In Unity:**
1. Create CoreParametersSection GameObject in `SectionsContainer`
2. Add 8 parameter input rows
3. Add Advanced Parameters button
4. Wire Inspector fields

**Test:** Input fields accept numbers and scientific notation

---

### **Step 3:** Request Next Stages

Once Steps 1-2 are complete, I'll provide instructions for:
- Stage 13.12: InspectTab (live metrics)
- Stage 13.13: ExportTab (export config)
- Stage 13.14: Apply & Restart flow

---

## ?? UI Architecture

```
UICanvas
??? TopBar ? (Stage 13.3)
?
??? RightDock ? (Stage 13.3)
    ??? TabButtonRow (Setup | Inspect | Export)
    ??? ContentArea
        ?
        ??? SetupPanel ?
        ?   ??? MechanismSummary ? (Stage 13.5)
        ?   ??? SectionsContainer
        ?       ??? MechanismsSection ? (Stage 13.5)
        ?       ??? TopologyDetailsSection ? (Stage 13.10)
        ?       ??? InflowDetailsSection ? (Stage 13.10)
        ?       ??? DiffusionDetailsSection ? (Stage 13.10)
        ?       ??? ViabilityDetailsSection ? (Stage 13.10)
        ?       ??? CoreParametersSection ? (Stage 13.11)
        ?
        ??? InspectPanel ?? (Stage 13.12)
        ?   ??? LiveMetricsText
        ?
        ??? ExportPanel ?? (Stage 13.13)
            ??? Export controls
```

---

## ? Quality Checklist

### Completed:
- [x] Engine-side mechanisms (Stages 13.1-13.9)
- [x] C# scripts for all UI sections
- [x] Setup instructions in Stage 13.1-9 style

### In Progress:
- [ ] Unity scene setup for detail sections (Stage 13.10)
- [ ] Unity scene setup for core parameters (Stage 13.11)

### TODO:
- [ ] InspectTab implementation (Stage 13.12)
- [ ] ExportTab implementation (Stage 13.13)
- [ ] Apply & Restart flow (Stage 13.14)
- [ ] End-to-end testing

---

## ?? How to Proceed

1. **Open Unity**
2. **Follow `STAGE13_10_UNITY_DETAIL_SECTIONS.md`**
   - Build 4 detail sections
   - Test visibility logic
3. **Follow `STAGE13_11_UNITY_CORE_PARAMETERS.md`**
   - Build core parameters section
   - Test input validation
4. **Report back when done**
   - I'll provide Stage 13.12-13.14 instructions

---

## ?? Notes

- **All instructions now match Stage 13.1-9 style**
- **No need to rebuild TopBar/RightDock** (already done)
- **Follow documents in order:** 13.10 ? 13.11 ? (ask for 13.12)
- **Test each stage** before proceeding

---

**Current Focus:** Stages 13.10 & 13.11 (Unity scene setup for detail sections + core parameters)

**Next:** Stages 13.12-13.14 (InspectTab, ExportTab, Apply & Restart)
