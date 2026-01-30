# CollapsibleSection - Complete Explanation

**Question:** "What do you mean 'collapsible sections' you haven't mentioned these previously"

**Answer:** CollapsibleSection is a **base class** that makes UI sections expandable/collapsible (like an accordion). Click the header to show/hide the content.

---

## ?? **What Is CollapsibleSection?**

**CollapsibleSection** is a base class that provides "expand/collapse" functionality for UI sections. Think of it like sections in a settings panel where you click the header to show/hide the content.

### **Visual Example:**

```
??????????????????????????????
? ? Topology Details     [?] ? ? Header (clickable)
??????????????????????????????
? Mask Shape: [Circle ?]    ? ? Content (can be hidden)
? Outer Radius: [20.0]      ?
? Inner Radius: [10.0]      ?
??????????????????????????????

Click header ?

??????????????????????????????
? ? Topology Details     [?] ? ? Header (still visible)
??????????????????????????????
                                ? Content hidden!
```

---

## ?? **Where It Lives**

**File:** `Assets/Viable/Core.Unity/UI/CollapsibleSection.cs`

**Created:** Just now (you were right, I hadn't explained it before!)

**Why it exists:** All your section scripts (`TopologyDetailsSection`, `MechanismsSection`, etc.) inherit from it to get expand/collapse behavior for free.

---

## ?? **How It Works**

### **Structure:**

Every CollapsibleSection has:
1. **HeaderPanel** - Always visible, contains button + text
2. **ContentPanel** - Can be shown/hidden
3. **ToggleButton** - Button component on header (for clicking)
4. **HeaderText** - Shows title + arrow indicator (?/?)

### **Behavior:**

- **Expanded** (default): Content visible, arrow points down ?
- **Collapsed**: Content hidden, arrow points right ?
- **Click header**: Toggles between expanded/collapsed

---

## ?? **Code Structure**

```csharp
public abstract class CollapsibleSection : MonoBehaviour, IConfigSection
{
    // Unity Inspector Fields
    [SerializeField] protected GameObject headerObject;
    [SerializeField] protected GameObject contentObject;
    [SerializeField] protected Button toggleButton;
    [SerializeField] protected TextMeshProUGUI headerText;
    
    // State
    private bool isExpanded;
    
    // Methods
    public void ToggleExpanded() { ... }  // Click header to toggle
    public void SetExpanded(bool expanded) { ... }  // Set state programmatically
    
    // Abstract methods (you implement in derived classes)
    public abstract void Bind(WorkingScenarioConfig config);
    public abstract void RefreshVisibility(WorkingScenarioConfig config);
    public abstract void ApplyEdits(WorkingScenarioConfig config);
}
```

---

## ?? **Relationship to Your Sections**

All your UI section scripts inherit from CollapsibleSection:

```csharp
public class TopologyDetailsSection : CollapsibleSection
{
    // Inherits expand/collapse behavior automatically!
    
    public override void Bind(WorkingScenarioConfig config) { ... }
    public override void RefreshVisibility(WorkingScenarioConfig config) { ... }
    public override void ApplyEdits(WorkingScenarioConfig config) { ... }
}
```

**Same for:**
- `MechanismsSection`
- `CoreParametersSection`
- `InflowDetailsSection`
- `DiffusionDetailsSection`
- `ViabilityDetailsSection`

---

## ??? **How to Use in Unity**

### **Step 1: Create Section Structure**

```
TopologyDetailsSection (Empty GameObject)
?? HeaderPanel (Panel + Button)
?  ?? HeaderText (Text TMP): "Topology Details"
?? ContentPanel (Empty)
   ?? [Your controls here]
```

### **Step 2: Wire Inspector**

Select `TopologyDetailsSection`, in Inspector:

**Collapsible Section Components:**
- `headerObject` ? Drag `HeaderPanel`
- `contentObject` ? Drag `ContentPanel`
- `toggleButton` ? Drag `HeaderPanel` (Button component)
- `headerText` ? Drag `HeaderText`

### **Step 3: Test**

- Press Play
- Click header ? Content hides
- Click header again ? Content shows
- ? Works!

---

## ?? **Why Use CollapsibleSection?**

### **Benefits:**

? **Saves Space** - Collapse sections you're not editing  
? **Cleaner UI** - Less clutter on screen  
? **Consistent** - All sections behave the same way  
? **Reusable** - Write collapse logic once, use everywhere  
? **Professional** - Common UX pattern (like Unity Inspector!)  

### **Without CollapsibleSection:**

? All sections always visible  
? Lots of scrolling  
? Hard to find what you need  
? UI feels cluttered  

### **With CollapsibleSection:**

? Collapse unneeded sections  
? Expand only what you're editing  
? Clean, organized UI  
? Easy to navigate  

---

## ?? **Visual Indicator**

The header text automatically updates to show state:

```
Expanded:   ? Topology Details    (arrow down, content visible)
Collapsed:  ? Topology Details    (arrow right, content hidden)
```

**Code does this automatically!** Just wire the fields in Inspector.

---

## ?? **Testing Expand/Collapse**

1. **In Unity**, build the section structure
2. **Wire Inspector fields** (header, content, button, text)
3. **Press Play**
4. **Click header** ? Should collapse
5. **Click again** ? Should expand
6. **Check Console** ? No errors ?

---

## ?? **Comparison to Other UI Patterns**

| Pattern | Description | Use Case |
|---------|-------------|----------|
| **Tabs** | Switch between mutually exclusive views | Different pages (Setup/Inspect/Export) |
| **Accordion (CollapsibleSection)** | Expand/collapse individual sections | Settings, parameters, details |
| **Drawer** | Slide in/out from edge | Side menus, tool palettes |
| **Modal** | Overlay window | Confirmations, forms |

**Your UI uses:**
- **Tabs** ? DockModeController (Setup/Inspect/Export dropdown)
- **Accordion** ? CollapsibleSection (individual sections)

---

## ?? **Summary**

**Q:** What are collapsible sections?  
**A:** UI sections with clickable headers that show/hide content (like an accordion).

**Q:** Where is it defined?  
**A:** `CollapsibleSection.cs` base class.

**Q:** Why use it?  
**A:** Saves space, cleaner UI, consistent behavior.

**Q:** How do I use it?  
**A:** Inherit from `CollapsibleSection`, create Header+Content structure, wire Inspector fields.

**Q:** Do I need to implement collapse logic?  
**A:** No! Base class handles it automatically. You just implement `Bind()`, `RefreshVisibility()`, `ApplyEdits()`.

---

## ?? **Result**

? Clean, organized UI sections  
? Expand/collapse with one click  
? Consistent behavior across all sections  
? Professional UX pattern  
? Less clutter, easier navigation  

---

**Time to wire per section:** 2-3 minutes (just drag fields in Inspector)

**Result:** Professional collapsible sections like Unity Inspector! ??
