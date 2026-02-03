# VIABLE UI Documentation Index

**Last Updated:** 2026-01-26  
**Stage:** 13 - Mechanism Configuration UI

This index helps you find the right documentation for your task.

---

## ?? Quick Navigation

### **?? Start Here:**
1. **[MASTER_UI_SETUP_GUIDE.md](MASTER_UI_SETUP_GUIDE.md)** ? **START WITH THIS!**
   - Complete step-by-step setup instructions
   - All UI components from scratch
   - Inspector wiring details
   - Everything in one place

2. **[QUICK_REFERENCE_HIERARCHY.md](QUICK_REFERENCE_HIERARCHY.md)**
   - Visual hierarchy tree
   - Component script summary
   - Inspector wiring quick reference
   - Common issues troubleshooting

---

## ?? Detailed Guides (Reference Only)

These guides provide additional context but **refer to MASTER_UI_SETUP_GUIDE.md for actual setup:**

### TopBar:
- **[TOPBAR_SETUP.md](TOPBAR_SETUP.md)**
  - Additional TopBar context
  - See MASTER_UI_SETUP_GUIDE.md Part 2 for setup

### RightDock:
- **[RIGHTDOCK_SETUP.md](RIGHTDOCK_SETUP.md)**
  - Additional RightDock context
  - See MASTER_UI_SETUP_GUIDE.md Part 3 for setup

### Detail Sections:
- **[DETAIL_SECTIONS_SETUP.md](DETAIL_SECTIONS_SETUP.md)**
  - Section behavior and testing
  - See MASTER_UI_SETUP_GUIDE.md Part 4.2-4.5 for setup

### Core Parameters:
- **[CORE_PARAMETERS_SETUP.md](CORE_PARAMETERS_SETUP.md)**
  - Parameter descriptions and formatting
  - See MASTER_UI_SETUP_GUIDE.md Part 4.6 for setup

---

## ?? Progress & Design Docs

### Implementation Status:
- **[IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md)**
  - Steps 1-7 complete (50%)
  - Files created summary
  - Next steps roadmap

### Design Verification:
- **[DESIGN_ALIGNMENT_CHECK.md](DESIGN_ALIGNMENT_CHECK.md)**
  - Original design principles
  - Implementation verification
  - Terminology consistency check

---

## ?? Code Files Reference

### Configuration:
- `Assets/Viable/Core.Unity/Configuration/WorkingScenarioConfig.cs`
  - Unity-side config model
  - Holds all UI edits
  - Clone() method for editing

### UI Framework:
- `Assets/Viable/Core.Unity/UI/IConfigSection.cs`
  - Interface for all sections
  - Bind/RefreshVisibility/ApplyEdits
  - CollapsibleSection base class

### UI Components:
- `Assets/Viable/Core.Unity/UI/TopBarUI.cs`
- `Assets/Viable/Core.Unity/UI/RightDockUI.cs`
- `Assets/Viable/Core.Unity/UI/MechanismsSection.cs`
- `Assets/Viable/Core.Unity/UI/TopologyDetailsSection.cs`
- `Assets/Viable/Core.Unity/UI/InflowDetailsSection.cs`
- `Assets/Viable/Core.Unity/UI/DiffusionDetailsSection.cs`
- `Assets/Viable/Core.Unity/UI/ViabilityDetailsSection.cs`
- `Assets/Viable/Core.Unity/UI/CoreParametersSection.cs`

---

## ?? Workflow Guide

### **New to this UI? Follow this order:**

1. ? **Read original design principles** (DESIGN_ALIGNMENT_CHECK.md)
2. ? **Follow MASTER_UI_SETUP_GUIDE.md** (Part 1-4)
3. ? **Use QUICK_REFERENCE_HIERARCHY.md** for wiring reference
4. ? **Test each component** as you build it
5. ? **Steps 8-14** (InspectTab, ExportTab, Modals, Integration)

### **Already built the UI? Need reference?**

- **GameObject hierarchy:** QUICK_REFERENCE_HIERARCHY.md
- **Inspector wiring:** QUICK_REFERENCE_HIERARCHY.md ? Component Scripts Summary
- **Section behavior:** DETAIL_SECTIONS_SETUP.md
- **Parameter formatting:** CORE_PARAMETERS_SETUP.md

### **Troubleshooting?**

- **Check:** MASTER_UI_SETUP_GUIDE.md ? Part 6: Troubleshooting
- **Common issues:** QUICK_REFERENCE_HIERARCHY.md ? Common Issues table

---

## ?? Current Status

### ? Completed (Steps 1-7):
- WorkingScenarioConfig data model
- IConfigSection interface + CollapsibleSection base
- TopBarUI (preset, run controls, speed, seed, export)
- RightDockUI (3-tab system, NO SCROLL!)
- MechanismsSection (6 dropdowns + mechanism summary)
- 4 context-sensitive detail sections
- CoreParametersSection (8 curated parameters)

### ? Remaining (Steps 8-14):
- InspectTab (live metrics display)
- ExportTab (export configuration)
- Apply & Restart flow
- AdvancedParametersModal (paginated, no scroll)
- PointSourceEditor modal (paginated list)
- UIManager integration
- End-to-end testing

---

## ?? Key Design Rules (Reminder)

### **DO:**
- ? Fixed-height panels
- ? Collapsible sections
- ? Context-sensitive controls
- ? Mechanism summary visible
- ? 6-12 curated parameters max
- ? Modals for advanced params

### **DO NOT:**
- ? Add ScrollRect anywhere!
- ? Expose all parameters in main panel
- ? Apply changes mid-run

---

## ?? Search Tips

Looking for specific information?

| What You Need | Where to Find It |
|---------------|------------------|
| Complete setup steps | MASTER_UI_SETUP_GUIDE.md |
| Hierarchy tree | QUICK_REFERENCE_HIERARCHY.md |
| Inspector wiring | QUICK_REFERENCE_HIERARCHY.md ? Key Inspector Wiring |
| Section behavior | DETAIL_SECTIONS_SETUP.md |
| Parameter details | CORE_PARAMETERS_SETUP.md |
| Design principles | DESIGN_ALIGNMENT_CHECK.md |
| Progress status | IMPLEMENTATION_PROGRESS.md |
| Troubleshooting | MASTER_UI_SETUP_GUIDE.md ? Part 6 |

---

## ?? Support

If documentation is unclear or contradictory:

1. **Primary source:** MASTER_UI_SETUP_GUIDE.md
2. **Quick reference:** QUICK_REFERENCE_HIERARCHY.md
3. **Specific details:** Individual guides (TOPBAR, RIGHTDOCK, etc.)

**All guides now reference the master guide to avoid confusion!**

---

**Ready to build? Start with: `MASTER_UI_SETUP_GUIDE.md` ??**
