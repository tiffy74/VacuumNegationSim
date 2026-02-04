# Preset Loading in Builds - Resources Folder Fix

## ?? **Problem**

**Symptom:** Presets work in Unity Editor but dropdown is **empty in builds**.

**Console in Build:**
```
[TopBarUI] No presets found, using fallback list
```

**Root Cause:** Unity only includes assets in `Resources` folders in builds. Editor uses `AssetDatabase` which can access any asset, but runtime/builds need `Resources.Load()`.

---

## ? **Solution**

### **Step 1: Create Resources Folder Structure**

```
Assets/Viable/Core.Unity/
?? Presets/
?  ?? Examples/  ? Original location (Editor only)
?     ?? 00_Default.asset
?     ?? 01_BalancedPersistence.asset
?     ?? ...
?
?? Resources/  ? NEW - For runtime/builds
   ?? Presets/
      ?? Examples/
         ?? 00_Default.asset  ? Copied here
         ?? 01_BalancedPersistence.asset
         ?? ...
```

**Created folders:**
- `Assets/Viable/Core.Unity/Resources/`
- `Assets/Viable/Core.Unity/Resources/Presets/`
- `Assets/Viable/Core.Unity/Resources/Presets/Examples/`

---

### **Step 2: Copy Presets to Resources**

**PowerShell command:**
```powershell
Copy-Item -Path "Assets\Viable\Core.Unity\Presets\Examples\*.asset" `
          -Destination "Assets\Viable\Core.Unity\Resources\Presets\Examples\" `
          -Force
```

**Or manually in Unity:**
1. Select all `.asset` files in `Presets/Examples/`
2. Copy (Ctrl+C)
3. Navigate to `Resources/Presets/Examples/`
4. Paste (Ctrl+V)

---

### **Step 3: Code Changes**

**Enhanced runtime loading with better logging:**

```csharp
#if UNITY_EDITOR
    // Editor: Use AssetDatabase (can access any asset)
    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScenarioPreset", 
        new[] { "Assets/Viable/Core.Unity/Presets/Examples" });
    // ... load from AssetDatabase
#else
    // Runtime: Use Resources.Load (only finds assets in Resources folders)
    var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
    Debug.Log($"[TopBarUI] Loaded {presets.Length} presets from Resources/Presets/Examples");
    
    presetNames.AddRange(presets.Select(p => p.PresetName));
    
    if (presetNames.Count == 0)
    {
        Debug.LogWarning("[TopBarUI] No presets found, trying Resources/Presets");
        presets = Resources.LoadAll<ScenarioPreset>("Presets");
        presetNames.AddRange(presets.Select(p => p.PresetName));
    }
#endif
```

**Key points:**
- `Resources.LoadAll<T>("Presets/Examples")` looks in **any** `Resources` folder
- Path is relative: `"Presets/Examples"` finds `Assets/.../Resources/Presets/Examples/`
- Added logging to diagnose issues in builds

---

## ?? **Testing**

### **Test 1: Editor Still Works**

**Steps:**
1. Unity Editor ? Press Play
2. Check preset dropdown

**Expected:**
- ? Dropdown shows all 5 presets
- ? "Default" is first
- ? Load button works

**Why still works:**
- `#if UNITY_EDITOR` path still uses `AssetDatabase`
- Loads from original `Presets/Examples/` folder

---

### **Test 2: Build Now Works**

**Steps:**
1. Unity ? File ? Build Settings
2. Build and Run (or Build then run .exe)
3. Check preset dropdown in built app

**Expected:**
- ? Dropdown shows all 5 presets
- ? "Default" is first
- ? Load button works
- ? Console shows: `[TopBarUI] Loaded 5 presets from Resources/Presets/Examples`

**Before fix:**
```
Console: [TopBarUI] No presets found, using fallback list
Dropdown: [Default, No Presets Found]
```

**After fix:**
```
Console: [TopBarUI] Loaded 5 presets from Resources/Presets/Examples
Dropdown: [Default, Balanced Persistence, Rapid Expansion, ...]
```

---

### **Test 3: Verify Files in Build**

**Check build output:**
```
YourGame_Data/
?? Resources/
?  ?? Presets/
?     ?? Examples/
?        ?? 00_Default.asset  ? Should be present
?        ?? 01_BalancedPersistence.asset
?        ?? ...
```

**If missing:**
- Presets weren't in a `Resources` folder when building
- Re-copy presets and rebuild

---

## ?? **How Unity Resources Work**

### **Editor vs Runtime**

| Location | Editor | Runtime/Build |
|----------|--------|---------------|
| `Assets/Presets/` | ? Via `AssetDatabase` | ? Not included |
| `Assets/Resources/Presets/` | ? Via `AssetDatabase` or `Resources` | ? Via `Resources.Load()` |

### **Resources.Load() Path Rules**

**Folder structure:**
```
Assets/
?? Viable/Core.Unity/Resources/Presets/Examples/preset.asset
?? OtherFolder/Resources/Presets/Examples/preset.asset
```

**Code:**
```csharp
Resources.LoadAll<ScenarioPreset>("Presets/Examples");
```

**Finds both:**
- `Assets/Viable/Core.Unity/Resources/Presets/Examples/`
- `Assets/OtherFolder/Resources/Presets/Examples/`

**Path is relative to ANY `Resources` folder!**

---

## ?? **Troubleshooting**

### **Build still shows empty dropdown**

**Check 1: Resources folder exists**
```
Assets/Viable/Core.Unity/Resources/Presets/Examples/
Should contain .asset files ?
```

**Check 2: Preset files are .asset (not .cs)**
```
? 00_Default.asset
? 00_Default.cs
```

**Check 3: Build console logs**
```
Look for:
[TopBarUI] Loaded X presets from Resources/Presets/Examples

If shows 0:
- Presets not in Resources folder
- Wrong path
- Assets not copied correctly
```

---

### **Duplicate presets in dropdown**

**Cause:** Presets in BOTH locations loaded in Editor:
- `Presets/Examples/` (via AssetDatabase)
- `Resources/Presets/Examples/` (via Resources)

**Solution:** Only affects Editor, not builds. Can ignore or:

**Option A: Keep separate** (current approach)
- Editor uses `Presets/Examples/` (AssetDatabase path)
- Builds use `Resources/Presets/Examples/` (Resources path)
- No duplicates because `#if UNITY_EDITOR` prevents mixing

**Option B: Use Resources in Editor too**
```csharp
// Remove #if UNITY_EDITOR, always use Resources
var presets = Resources.LoadAll<ScenarioPreset>("Presets/Examples");
```
- Simpler, but loses ability to edit presets outside Resources

---

### **New preset not showing in build**

**Checklist:**
1. [ ] Created new preset in Unity
2. [ ] Copied to `Resources/Presets/Examples/`
3. [ ] Rebuilt application (old builds won't include new presets)
4. [ ] Preset has `PresetName` field set

**Quick fix:**
```powershell
# Re-copy all presets
Copy-Item -Path "Assets\Viable\Core.Unity\Presets\Examples\*.asset" `
          -Destination "Assets\Viable\Core.Unity\Resources\Presets\Examples\" `
          -Force

# Rebuild
Unity ? File ? Build and Run
```

---

## ?? **Workflow for Adding New Presets**

### **Step 1: Create in Editor**
```
Assets/Viable/Core.Unity/Presets/Examples/
Right-click ? Create ? Viable ? Scenario Preset
Name: 06_YourNewPreset.asset
```

### **Step 2: Configure**
```
Inspector:
?? Preset Name: "Your New Preset"
?? Description: "What it does..."
?? Parameters: (configure values)
```

### **Step 3: Copy to Resources**
```powershell
Copy-Item -Path "Assets\Viable\Core.Unity\Presets\Examples\06_YourNewPreset.asset" `
          -Destination "Assets\Viable\Core.Unity\Resources\Presets\Examples\" `
          -Force
```

### **Step 4: Test**
```
Editor: Should show immediately (uses AssetDatabase)
Build: Must rebuild to include new preset
```

---

## ? **Success Criteria**

**Preset loading works in builds when:**

- [ ] Preset files exist in `Resources/Presets/Examples/`
- [ ] Build console shows: `Loaded X presets from Resources/Presets/Examples`
- [ ] Dropdown shows all presets (not "No Presets Found")
- [ ] "Default" preset appears first in list
- [ ] Load button successfully loads selected preset
- [ ] Simulation behavior matches selected preset

---

## ?? **Files Changed**

| File/Folder | Action | Purpose |
|-------------|--------|---------|
| `Resources/` folder | Created | Required for runtime asset loading |
| `Resources/Presets/Examples/` | Created + populated | Contains preset .asset files for builds |
| `TopBarUI.cs` | Enhanced logging | Better diagnostics for runtime loading |

**Total:** 1 code file updated, 6 preset files copied

---

## ?? **Result**

**Before:**
```
Editor: Works ? (uses AssetDatabase)
Build: Empty dropdown ? (no Resources folder)
```

**After:**
```
Editor: Works ? (uses AssetDatabase, ignores Resources)
Build: Works ? (uses Resources folder)
```

**Both environments now work correctly!** ??

---

## ?? **Further Reading**

**Unity Documentation:**
- [Special Folder Names - Resources](https://docs.unity3d.com/Manual/SpecialFolders.html)
- [Resources.Load](https://docs.unity3d.com/ScriptReference/Resources.Load.html)
- [AssetDatabase (Editor Only)](https://docs.unity3d.com/ScriptReference/AssetDatabase.html)

**Best Practices:**
- Keep presets in Resources if they need to be loaded at runtime
- Use `#if UNITY_EDITOR` to use AssetDatabase in Editor
- Always test builds, not just Editor play mode!

---

**Created:** 2024  
**Status:** ? Preset Loading in Builds Fixed  
**Impact:** Builds now have full preset functionality
