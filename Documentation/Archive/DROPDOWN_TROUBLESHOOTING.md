# Dropdown Options Not Populating - Troubleshooting Guide

## ?? **Problem**

The mechanism dropdowns (Topology, Boundary, Inflow, etc.) appear empty or don't show options.

---

## ? **Solution 1: Wire Dropdowns in Unity Inspector**

### **Step 1: Select MechanismsSection GameObject**

1. **In Hierarchy:** Find `SetupPanel ? MechanismsSection`
2. **Select it**

### **Step 2: Check Inspector - MechanismsSection Component**

1. **Look for these fields:**
   - `topologyDropdown`
   - `boundaryDropdown`
   - `inflowDropdown`
   - `diffusionDropdown`
   - `viabilityDropdown`
   - `phaseSetDropdown`

2. **Verify each field has a dropdown assigned:**
   - ? If showing "None (TMP_Dropdown)", the dropdown isn't wired
   - ? If showing "TopologyDropdown (TMP_Dropdown)", it's wired

### **Step 3: Wire Missing Dropdowns**

**For each empty field:**
1. **Find the corresponding dropdown** in MechanismsSection hierarchy:
   - TopologyDropdown
   - BoundaryDropdown
   - InflowDropdown
   - DiffusionDropdown
   - ViabilityDropdown
   - PhaseSetDropdown

2. **Drag the dropdown GameObject** to the corresponding field

---

## ? **Solution 2: Check Dropdown GameObjects Are Active**

### **Step 1: Expand MechanismsSection in Hierarchy**

Look for:
```
MechanismsSection
?? TopologyDropdown (should be active ?)
?? BoundaryDropdown (should be active ?)
?? InflowDropdown (should be active ?)
?? ...
```

### **Step 2: Check Active Checkbox**

**For each dropdown GameObject:**
- ? Checkbox next to name should be **checked**
- ? If **unchecked**, the dropdown is inactive

**Fix:** Click the checkbox to activate the GameObject

---

## ? **Solution 3: Test Dropdown Population**

### **Step 1: Press Play in Unity**

### **Step 2: In Game View, Click a Dropdown**

**Expected result:**
```
Topology Dropdown:
?? Full Domain
?? Masked Domain
```

**If you see options:** ? Dropdowns are working!

**If dropdown is empty:** Check Console for errors

---

## ? **Solution 4: Check Console for Errors**

### **Press Play and Check Console**

**Look for:**
```
[MechanismsSection] topologyDropdown is null!
[MechanismsSection] boundaryDropdown is null!
```

**If you see these:** Dropdowns aren't wired (go back to Solution 1)

---

## ? **Solution 5: Manually Test Dropdown Population**

### **Add Debug Logs to MechanismsSection.cs**

**Add after `PopulateTopologyDropdown()`:**

```csharp
private void PopulateTopologyDropdown()
{
    if (topologyDropdown == null)
    {
        Debug.LogError("[MechanismsSection] topologyDropdown is null!");
        return;
    }
    
    topologyDropdown.ClearOptions();
    topologyDropdown.AddOptions(new List<string>
    {
        "Full Domain",
        "Masked Domain"
    });
    
    Debug.Log($"[MechanismsSection] Populated Topology dropdown with {topologyDropdown.options.Count} options");
}
```

**Press Play and check Console:**
- ? "Populated Topology dropdown with 2 options" ? Working
- ? "topologyDropdown is null!" ? Not wired in Inspector

---

## ?? **Visual Checklist**

### **MechanismsSection Inspector Should Look Like:**

```
MechanismsSection (Script)
?? Mechanism Dropdowns
?  ?? Topology Dropdown: TopologyDropdown (TMP_Dropdown) ?
?  ?? Boundary Dropdown: BoundaryDropdown (TMP_Dropdown) ?
?  ?? Inflow Dropdown: InflowDropdown (TMP_Dropdown) ?
?  ?? Diffusion Dropdown: DiffusionDropdown (TMP_Dropdown) ?
?  ?? Viability Dropdown: ViabilityDropdown (TMP_Dropdown) ?
?  ?? Phase Set Dropdown: PhaseSetDropdown (TMP_Dropdown) ?
?
?? External References
?  ?? Mechanism Summary Text: (TextMeshProUGUI)
?
?? Events
   ?? On Mechanism Changed()
```

**All fields should show GameObject references, NOT "None"!**

---

## ?? **Common Issues**

### **Issue A: Dropdowns show "None (TMP_Dropdown)"**

**Cause:** Dropdowns not wired in Inspector

**Fix:**
1. Select MechanismsSection
2. Drag each dropdown GameObject to its field
3. Press Play again

---

### **Issue B: Dropdowns exist but no options visible**

**Cause:** Dropdowns might have Template inactive

**Fix:**
1. **Select a dropdown** (e.g., TopologyDropdown)
2. **Expand in Hierarchy** ? Find `Template`
3. **Template should be inactive by default** (Unity will activate it when clicked)
4. **Check Template ? Viewport ? Content ? Item**
   - Should have Text (Legacy) or TextMeshPro component

---

### **Issue C: Dropdowns show but clicking does nothing**

**Cause:** Template or Item setup incorrect

**Fix:**
1. **Select dropdown**
2. **Inspector ? TMP_Dropdown component:**
   - Template: Drag Template child
   - Caption Text: Drag Label (TextMeshProUGUI)
   - Item Text: Drag Template ? Viewport ? Content ? Item ? Label

---

## ? **Quick Test Script**

**Create this script to test if dropdowns work:**

```csharp
using UnityEngine;
using TMPro;

public class DropdownTester : MonoBehaviour
{
    void Start()
    {
        var dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogError("No TMP_Dropdown found!");
            return;
        }

        Debug.Log($"Dropdown has {dropdown.options.Count} options:");
        foreach (var option in dropdown.options)
        {
            Debug.Log($"  - {option.text}");
        }
    }
}
```

**Attach to a dropdown and press Play to see options in Console.**

---

## ? **Success Criteria**

After fixing:

- [ ] All dropdown fields in MechanismsSection Inspector show references (not "None")
- [ ] Press Play ? No "null" errors in Console
- [ ] Click dropdown ? Options appear (e.g., "Full Domain", "Masked Domain")
- [ ] Select option ? Dropdown shows selected value
- [ ] Other dropdowns also work

---

## ?? **Most Likely Cause**

**99% of the time, dropdowns not showing options means:**

**They're not wired in the Unity Inspector!**

**Fix:**
1. Select MechanismsSection
2. Drag all 6 dropdowns to their fields
3. Done!

---

**Let me know if the dropdowns are wired but still empty!** ??
