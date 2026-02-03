# How to Manually Populate Dropdowns in Unity - Complete Guide

**Problem:** Dropdowns are empty and I don't know how to add options manually

**Solution:** This guide shows EXACTLY how to add options to TMP_Dropdown in Unity Inspector

---

## ?? **Quick Overview**

Unity dropdowns need their options manually added in the Inspector. The scripts populate them via code, but if you're building UI from scratch, here's how to add options manually **before** the script runs.

---

## ?? **Method 1: Let the Script Populate (RECOMMENDED)**

**Good news:** All the section scripts automatically populate dropdowns in their `Start()` methods!

**What this means:**
- You DON'T need to manually add options
- The script will populate them when you press Play
- Options are defined in the script code

**Example from MechanismsSection.cs:**
```csharp
private void PopulateTopologyDropdown()
{
    topologyDropdown.ClearOptions();
    topologyDropdown.AddOptions(new List<string>
    {
        "Full Domain",
        "Masked Domain"
    });
}
```

**To use this method:**
1. Create the dropdown in Unity
2. Wire it to the script in Inspector
3. Press Play
4. Script automatically populates options ?

**? This is the easiest method! Skip to "Method 3" if you want to use this approach.**

---

## ?? **Method 2: Manually Add Options (If You Want Pre-Populated Dropdowns)**

**Use this if:** You want to see dropdown options in Editor (not just Play mode)

### **Step-by-Step: Manually Populate a Dropdown**

#### **Example: TopologyDropdown in MechanismsSection**

1. **Select the Dropdown GameObject** (e.g., `TopologyDropdown`)

2. **In Inspector**, find the **TMP_Dropdown component**

3. **Find the "Options" section:**
   ```
   TMP Dropdown (Script)
   ?? Interactable: ?
   ?? Transition: ColorTint
   ?? Navigation: Automatic
   ?? Template: (Dropdown Template)
   ?? Caption Text: (Text - TextMeshProUGUI)
   ?? Item Text: (Text - TextMeshProUGUI)
   ?? Value: 0
   ?? Alpha Fade Speed: 0.15
   ?? Options ? LOOK HERE!
       Size: 0  ? Change this!
   ```

4. **Change "Size" from 0 to the number of options you need**
   - For Topology: Change to **2** (Full Domain, Masked Domain)
   - Press Enter

5. **Options fields appear:**
   ```
   Options
   ?? Size: 2
   ?? Element 0
   ?   ?? Text: [empty]
   ?   ?? Image: None (Sprite)
   ?? Element 1
   ?   ?? Text: [empty]
   ?   ?? Image: None (Sprite)
   ```

6. **Click "Element 0" ? Enter text:**
   - Text: "Full Domain"
   - Leave Image as None

7. **Click "Element 1" ? Enter text:**
   - Text: "Masked Domain"
   - Leave Image as None

8. **Result:**
   ```
   Options
   ?? Size: 2
   ?? Element 0
   ?   ?? Text: "Full Domain"
   ?   ?? Image: None (Sprite)
   ?? Element 1
   ?   ?? Text: "Masked Domain"
   ?   ?? Image: None (Sprite)
   ```

9. **Save scene** (Ctrl+S)

---

## ?? **Complete Option Lists for All Dropdowns**

### **MechanismsSection:**

**TopologyDropdown:**
- Size: 2
- Element 0: "Full Domain"
- Element 1: "Masked Domain"

**BoundaryDropdown:**
- Size: 3
- Element 0: "Closed (Reflective)"
- Element 1: "Open (Absorbing)"
- Element 2: "Wrap (Periodic)"

**InflowDropdown:**
- Size: 3
- Element 0: "Uniform Field"
- Element 1: "Point Sources"
- Element 2: "Edge Sources"

**DiffusionDropdown:**
- Size: 3
- Element 0: "Von Neumann (4-neighbor)"
- Element 1: "Moore (8-neighbor)"
- Element 2: "Anisotropic"

**ViabilityDropdown:**
- Size: 2
- Element 0: "Simple Threshold"
- Element 1: "Hysteresis"

**PhaseSetDropdown:**
- Size: 2
- Element 0: "Standard"
- Element 1: "Custom (Advanced)"

---

### **TopologyDetailsSection:**

**MaskShapeDropdown:**
- Size: 5
- Element 0: "Rectangle"
- Element 1: "Circle"
- Element 2: "Ring"
- Element 3: "Corridor"
- Element 4: "Percolation Holes"

---

### **DiffusionDetailsSection:**

**DirectionDropdown:**
- Size: 4
- Element 0: "North (?)"
- Element 1: "East (?)"
- Element 2: "South (?)"
- Element 3: "West (?)"

---

### **DockModeController:**

**ModeDropdown:**
- Size: 3
- Element 0: "Setup"
- Element 1: "Inspect"
- Element 2: "Export"

**?? NOTE:** This one MUST be populated by script (see Method 3), not manually!

---

## ?? **Method 3: Using Script Population (RECOMMENDED)**

**Best practice:** Let scripts populate dropdowns automatically

### **Why This Is Better:**

? Options match the script logic exactly  
? No manual sync needed when code changes  
? Dropdowns always correct  
? Less error-prone  

### **How It Works:**

1. **Create dropdown in Unity** (empty options OK)
2. **Wire to script field** in Inspector
3. **Script populates in Start()** method
4. **Done!**

### **Example: MechanismsSection**

**In Unity:**
- Create `TopologyDropdown` (TMP_Dropdown)
- Leave Options empty (Size: 0)
- Drag to `MechanismsSection` ? `topologyDropdown` field

**Script handles rest:**
```csharp
protected override void Start()
{
    base.Start();
    
    // Script populates automatically!
    PopulateTopologyDropdown();
    PopulateBoundaryDropdown();
    // ... etc
}
```

**When you press Play:**
- Dropdowns populate automatically ?
- Options match script logic ?
- No manual work needed ?

---

## ?? **When to Use Each Method**

### **Use Method 1 (Script Population):**
- ? **Most cases** - simplest, least error-prone
- ? When options might change in code
- ? When building UI from scratch

### **Use Method 2 (Manual Population):**
- ?? **Only if:** You need to see options in Editor (not just Play mode)
- ?? **Only if:** You're testing layout without running
- ?? **Warning:** Must manually sync if script changes!

### **Use Method 3 (Hybrid):**
- Add 1 option manually (for visual layout testing)
- Let script populate full list on Play

---

## ? **Testing Dropdown Population**

### **Test 1: Script Population Works**

1. **Create dropdown** with Size: 0 (empty)
2. **Wire to script field**
3. **Press Play**
4. **Click dropdown**
5. **Should show options** ?

### **Test 2: Manual Population Works**

1. **Create dropdown**
2. **Manually add options** (Size: 2, add text)
3. **DON'T press Play**
4. **Click dropdown in Editor**
5. **Should show options** ?

### **Test 3: Script Overrides Manual**

1. **Manually add options** (e.g., "Test1", "Test2")
2. **Wire to script field**
3. **Press Play**
4. **Click dropdown**
5. **Should show SCRIPT options, not manual ones** ?

**Why?** Scripts call `ClearOptions()` first, then add their own.

---

## ?? **Quick Reference: Dropdown Sizes**

| Dropdown | Size | Location |
|----------|------|----------|
| TopologyDropdown | 2 | MechanismsSection |
| BoundaryDropdown | 3 | MechanismsSection |
| InflowDropdown | 3 | MechanismsSection |
| DiffusionDropdown | 3 | MechanismsSection |
| ViabilityDropdown | 2 | MechanismsSection |
| PhaseSetDropdown | 2 | MechanismsSection |
| MaskShapeDropdown | 5 | TopologyDetailsSection |
| DirectionDropdown | 4 | DiffusionDetailsSection |
| ModeDropdown | 3 | DockModeController |

---

## ?? **Troubleshooting**

### **Dropdown empty when I press Play:**
- Check: Dropdown wired to script field?
- Check: Script's Start() method running?
- Check: Console for errors?

### **Dropdown shows wrong options:**
- Check: Did script populate (not manual options)?
- Check: Script's Populate method has correct options?

### **Can't see options in Editor (not Play mode):**
- **This is normal!** Script populates on Play
- If you need Editor preview, use Method 2 (manual)

### **Manual options disappear on Play:**
- **This is normal!** Script calls ClearOptions() first
- Use script population (Method 1) instead

---

## ?? **Recommendation**

**For this project:** Use **Method 1 (Script Population)**

**Why:**
- All scripts already have Populate methods ?
- Zero manual work needed ?
- Always correct options ?
- No sync issues ?

**Your workflow:**
1. Create dropdown GameObject
2. Wire to script field
3. Press Play
4. Done! Options populate automatically ?

---

**Time:** ~30 seconds per dropdown (just wire it!)  
**Result:** Dropdowns populate automatically on Play! ??
