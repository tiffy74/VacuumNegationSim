# ?? Preset Loading Issue - Fixed

## **Problem:**
When loading the application and pressing Play, it runs the **last preset in the list** instead of the **Default** preset.

---

## **Root Cause:**

The issue was caused by a **synchronization problem** between:
1. **SimulationController** - Has its own `scenarioPreset` field in Inspector
2. **SimulationUIOrchestrator** - Has a `defaultPreset` field
3. **PresetControlsSection** (UI Dropdown) - Shows available presets

When the app started, these three components could load **different presets**, causing the dropdown to show one thing while the simulation runs another.

---

## **Fix Applied:**

### **1. SimulationController now syncs with Orchestrator**

The `SimulationController` now checks the orchestrator's `CurrentPreset` **before** using its own Inspector field:

```csharp
// Try to get preset from orchestrator FIRST
var orchestrator = FindFirstObjectByType<Controllers.SimulationUIOrchestrator>();
if (orchestrator != null && orchestrator.CurrentPreset != null)
{
    // Use preset from orchestrator (ensures UI sync)
    scenarioPreset = orchestrator.CurrentPreset;
    Debug.Log($"[SimulationController] Using preset from orchestrator: {scenarioPreset.PresetName}");
}
```

### **2. Orchestrator auto-loads first preset**

If no `defaultPreset` is set in the orchestrator's Inspector, it now **auto-loads** the first "Default" or "00_" preset:

```csharp
if (defaultPreset != null)
{
    LoadPreset(defaultPreset);
}
else
{
    AutoLoadFirstPreset(); // NEW: Auto-find and load default
}
```

### **3. Delayed initialization**

The `SimulationController` now waits one frame before initializing, allowing the orchestrator to load the preset first:

```csharp
void Start()
{
    // Wait one frame to let UI orchestrator initialize first
    StartCoroutine(DelayedInitialization());
}
```

### **4. Better logging**

Added debug logs to help track which preset is being loaded:
- `[SimulationUIOrchestrator] Initializing with default preset: [Name]`
- `[SimulationController] Using preset from orchestrator: [Name]`
- `[PresetControlsSection] Dropdown populated with X presets. First preset: [Name]`

---

## **How to Verify the Fix:**

### **Check Console Logs on Startup:**

You should see logs in this order:
```
[SimulationUIOrchestrator] Initializing with default preset: Default
[PresetControlsSection] Dropdown populated with 5 presets. First preset: 'Default'
[SimulationUIOrchestrator] Synced dropdown to default preset: Default
[SimulationController] Using preset from orchestrator: Default
[SimulationController] Using topology from preset: RectGrid
[SimulationController] Simulation initialized. Press Play button to start.
```

### **Visual Check:**
1. Open Unity
2. Enter Play mode
3. Check the preset dropdown in Right Dock ? Setup Panel
4. It should show **"Default"** (or your first preset)
5. Click the Play button
6. Simulation should run with **Default** preset parameters

---

## **Unity Inspector Setup:**

### **SimulationUIOrchestrator:**
1. Find `SimulationUIOrchestrator` GameObject in scene
2. In Inspector, find `Default Preset` field
3. Drag your **Default** preset ScriptableObject here
4. This ensures the orchestrator loads the correct preset on startup

### **SimulationController (Optional):**
1. Find `SimulationController` GameObject in scene
2. In Inspector, find `Scenario Preset` field
3. **Leave it EMPTY** (it will auto-sync with orchestrator)
4. Or assign the **same Default preset** as the orchestrator

**Recommendation:** Leave `SimulationController.scenarioPreset` empty and only set `SimulationUIOrchestrator.defaultPreset`.

---

## **Initialization Flow (Fixed):**

```
App Start
   ?
1. SimulationUIOrchestrator.Awake()
   ?
   Load defaultPreset (or auto-find first)
   ?
   WorkingConfig = preset data
   CurrentPreset = preset reference
   ?
2. PresetControlsSection.Start()
   ?
   Populate dropdown with all presets
   ?
   Set dropdown value = 0 (first preset)
   ?
3. SimulationUIOrchestrator.Start()
   ?
   Wait 1 frame
   ?
   Sync dropdown to CurrentPreset
   ?
4. SimulationController.Start()
   ?
   Wait 1 frame (let orchestrator finish)
   ?
   Get CurrentPreset from orchestrator
   ?
   Initialize simulation with that preset
   ?
Ready to Play!
```

---

## **Troubleshooting:**

### **Still loading wrong preset?**

**Check Console Logs:**
- Look for `[SimulationUIOrchestrator] Initializing with default preset: [Name]`
- Look for `[SimulationController] Using preset from orchestrator: [Name]`
- Verify they both say the **same preset name**

**Check Inspector:**
- `SimulationUIOrchestrator` ? `Default Preset` field should be set
- `SimulationController` ? `Scenario Preset` field should be **empty** (or same as orchestrator)

**Check Preset Files:**
- Ensure `Assets/Viable/Core.Unity/Presets/Examples/` contains your presets
- Ensure there's a preset named "Default" or starting with "00_"
- Check the preset's `PresetName` property matches

### **Dropdown shows wrong preset?**

**Check Dropdown Initialization:**
- Look for `[PresetControlsSection] Dropdown populated with X presets. First preset: [Name]`
- The first preset should be "Default" or your intended default

**Check Sorting Logic:**
- Presets are sorted alphabetically FIRST
- Then "Default" or "00_" preset is moved to front
- If no "Default" found, alphabetically first preset stays at index 0

### **Simulation runs but dropdown doesn't match?**

This indicates a sync issue. Check:
- `SimulationUIOrchestrator.SyncPresetDropdownAfterInit()` is being called
- `PresetControlsSection.SelectPreset()` is working correctly
- Console shows: `[SimulationUIOrchestrator] Synced dropdown to default preset: [Name]`

---

## **Best Practices:**

1. ? **Always set** `SimulationUIOrchestrator.defaultPreset` in Inspector
2. ? **Leave empty** `SimulationController.scenarioPreset` (let it auto-sync)
3. ? **Name your default preset** "Default" or start it with "00_" for auto-detection
4. ? **Check Console logs** on startup to verify correct preset loaded
5. ? **Use Reset button** to return to default preset anytime

---

## **Summary:**

The fix ensures that:
- ? **Orchestrator loads first** (has the preset)
- ? **Controller syncs** with orchestrator (uses same preset)
- ? **Dropdown syncs** with orchestrator (shows correct selection)
- ? **All three components** use the **same preset** on startup

**Result:** When you press Play, it runs the Default preset (or first preset in dropdown), not a random last preset!

---

**Fix tested and working!** ??
