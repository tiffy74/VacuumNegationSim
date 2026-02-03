# RefreshVisibility() Fix - Summary

**Problem:** `RefreshVisibility()` method wasn't showing up in Unity Inspector dropdown

**Root Cause:** The method had a parameter `RefreshVisibility(WorkingScenarioConfig config)` which Unity events can't call directly

**Solution:** ? Added parameterless wrapper methods to all 4 conditional sections

---

## ? **What Was Fixed**

### **All 4 Section Scripts Updated:**

1. **TopologyDetailsSection.cs** ?
2. **InflowDetailsSection.cs** ?
3. **DiffusionDetailsSection.cs** ?
4. **ViabilityDetailsSection.cs** ?

### **What Was Added:**

Each section now has TWO `RefreshVisibility` methods:

```csharp
// Original method (takes parameter, not visible in Unity Inspector)
public override void RefreshVisibility(WorkingScenarioConfig config)
{
    bool relevant = (config.SomeMechanism == SomeMode);
    gameObject.SetActive(relevant);
}

// NEW: Wrapper method (no parameters, visible in Unity Inspector!)
public void RefreshVisibility()
{
    if (currentConfig != null)
    {
        RefreshVisibility(currentConfig);
    }
    else
    {
        gameObject.SetActive(false);
    }
}
```

---

## ?? **What You Can Do Now**

### **In Unity:**

1. **Reload Unity** (if it's already open, to pick up the changes)
2. **Wait for compilation** (spinning icon in bottom-right)
3. **Select `MechanismsSection`** GameObject
4. **Scroll to "Events" ? `On Mechanism Changed ()`**
5. **Click `+` to add listener**
6. **Drag a detail section** (e.g., `TopologyDetailsSection`)
7. **Click "No Function" dropdown**
8. **You'll now see TWO options:**
   - ? **`RefreshVisibility()`** ? Use this one!
   - `RefreshVisibility(WorkingScenarioConfig)` ? Don't use

---

## ?? **Important: Which Method to Use**

### **? Correct (Use This):**
```
Function: TopologyDetailsSection ? RefreshVisibility()
                                    ? No parameters!
```

### **? Wrong (Don't Use):**
```
Function: TopologyDetailsSection ? RefreshVisibility(WorkingScenarioConfig)
                                    ? Has parameter, won't work!
```

**Why?** Unity events can only call methods with **no parameters** or **specific Unity types**. The parameterless version is a wrapper that uses the cached `currentConfig`.

---

## ?? **Testing**

After wiring all 4 sections with the **parameterless** `RefreshVisibility()`:

1. **Press Play**
2. **Change Topology to "Masked Domain"**
   - TopologyDetailsSection appears ?
3. **Change Inflow to "Point Sources"**
   - InflowDetailsSection appears ?
4. **Change Diffusion to "Anisotropic"**
   - DiffusionDetailsSection appears ?
5. **Change Viability to "Hysteresis"**
   - ViabilityDetailsSection appears ?

**If sections don't appear:**
- Check: Used **parameterless** `RefreshVisibility()`?
- Check: `currentConfig` is set (should happen when MechanismsSection calls Bind())
- Check: Console for errors

---

## ?? **Troubleshooting**

### **Still don't see RefreshVisibility() in dropdown:**
1. Check Unity finished compiling (no spinning icon)
2. Close/reopen Unity
3. Check Console for compilation errors
4. Verify section script is attached to GameObject

### **Method shows but says "WorkingScenarioConfig" parameter:**
- Wrong method! Look for the one **without** parameters
- There should be two methods in the dropdown
- Select the top one (parameterless)

### **Section appears but doesn't hide:**
- Check: MechanismsSection.Bind() is being called?
- Check: currentConfig is not null?
- Add Debug.Log to verify RefreshVisibility() is called

---

## ?? **Result**

? All 4 conditional sections have Unity-event-compatible `RefreshVisibility()` methods  
? Methods now appear in Unity Inspector dropdown  
? Can wire sections to MechanismsSection.OnMechanismChanged event  
? Sections automatically show/hide when mechanisms change  

---

**Time to wire:** 2-3 minutes for all 4 sections

**Updated guides:**
- `WIRE_ONMECHANISMCHANGED_GUIDE.md` ? Updated with clarification
- All 4 section scripts ? Added parameterless wrappers

---

**Ready to wire in Unity!** ??
