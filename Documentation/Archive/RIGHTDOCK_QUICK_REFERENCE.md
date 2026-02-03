# RightDock Setup - Quick Reference Card

**Status:** Clean RightDock with dropdown panel switching ?

**Current Task:** Adding sections to SetupPanel

---

## ?? **Which Guide To Use?**

### **? Already Completed:**
- [x] `RIGHTDOCK_CLEAN_RESET.md` - Built clean RightDock structure
- [x] Basic panel switching works (Setup/Inspect/Export)

### **? Working On Now:**
Choose based on what you're adding:

| What You're Adding | Use This Guide | Time |
|-------------------|----------------|------|
| **MechanismSummary** | `RIGHTDOCK_ADD_SECTIONS.md` ? Section 1 | 10 min |
| **MechanismsSection** | `RIGHTDOCK_ADD_SECTIONS.md` ? Section 2 | 20 min |
| **CoreParametersSection** | `RIGHTDOCK_ADD_SECTIONS.md` ? Section 3 | 20 min |
| **Conditional Sections (All 4)** | `CONDITIONAL_SECTIONS_COMPLETE_GUIDE.md` | 60-80 min |

---

## ?? **Current Priority Order**

Add sections in this order:

1. ? **MechanismSummary** (10 min)
   - Read-only text showing mechanism selections
   - Always visible at top

2. ? **MechanismsSection** (20 min)
   - 6 dropdowns for mechanisms
   - Always visible

3. ? **CoreParametersSection** (20 min)
   - 8 parameter inputs
   - Always visible

4. ? **4 Conditional Sections** (60-80 min total)
   - TopologyDetailsSection
   - InflowDetailsSection
   - DiffusionDetailsSection
   - ViabilityDetailsSection

---

## ?? **Quick Setup Pattern**

All sections follow this pattern:

### **1. Create Structure:**
```
SetupPanel
?? [SectionName] (Empty)
   ?? HeaderPanel (Panel + Button + Text)
   ?? ContentPanel (Empty)
      ?? [Rows with controls]
```

### **2. Add Components:**
- Section script
- Vertical Layout Group

### **3. Wire Inspector:**
- Drag all controls to script fields
- Drag header elements (headerObject, contentObject, etc.)

### **4. Test:**
- Press Play
- Check controls work
- No errors in Console

---

## ?? **Common Unity Operations**

### **Create Horizontal Row:**
1. Right-click parent ? Create Empty
2. Add Horizontal Layout Group (Spacing: 10)
3. Add children: Label + Control

### **Add Label:**
- UI ? Text - TextMeshPro
- Set text, font size
- Add Layout Element: Preferred Width

### **Add Dropdown:**
- UI ? Dropdown - TextMeshPro
- Add Layout Element: Flexible Width: 1
- Set options manually

### **Add Input Field:**
- UI ? Input Field - TextMeshPro
- Set Content Type (Decimal, Standard, etc.)
- Set Placeholder
- Add Layout Element: Flexible Width: 1

### **Add Slider:**
- UI ? Slider
- Set Min, Max, Value
- Whole Numbers: OFF (for decimal)
- Add Layout Element: Flexible Width: 1

---

## ? **Testing Checklist**

After adding each section:

- [ ] Section visible in SetupPanel
- [ ] Controls work (dropdown, input, slider, etc.)
- [ ] No errors in Console
- [ ] Switch to Inspect panel
- [ ] Switch back to Setup ? Section still there
- [ ] Save scene (Ctrl+S)

---

## ?? **Quick Troubleshooting**

### **Section not visible:**
- Check: GameObject SetActive = true?
- Check: Layout Groups on parent?

### **Control not wired:**
- Check: Inspector field has GameObject dragged?
- Check: Dragged GameObject, not component?

### **Dropdown empty:**
- Check: Options added manually in Inspector?

### **Layout broken:**
- Check: Layout Element settings (Preferred/Flexible)
- Check: Parent has Layout Group component?

---

## ?? **Progress Tracker**

**Basic Structure:**
- [x] RightDock created
- [x] Dropdown panel switching works
- [x] 3 panel placeholders (Setup/Inspect/Export)

**SetupPanel Sections:**
- [ ] MechanismSummary (Section 1)
- [ ] MechanismsSection (Section 2)
- [ ] CoreParametersSection (Section 3)
- [ ] TopologyDetailsSection (Section 4)
- [ ] InflowDetailsSection (Section 5)
- [ ] DiffusionDetailsSection (Section 6)
- [ ] ViabilityDetailsSection (Section 7)

**Total Estimated Time:** 2-3 hours for all sections

---

## ?? **Current Action**

**If you haven't added any sections yet:**
? Start with `RIGHTDOCK_ADD_SECTIONS.md` Section 1 (MechanismSummary)

**If you've added Sections 1-3 (Summary + Mechanisms + CoreParams):**
? Switch to `CONDITIONAL_SECTIONS_COMPLETE_GUIDE.md` for the 4 conditional sections

**If you're stuck:**
? Check troubleshooting above, or come back with specific error

---

**Keep this file open for quick reference!** ??
