# Default Preset Setup & Return to Initial State

## ?? Problem Fixed

**Issue:** No way to return to default demo behavior after changing settings.

**Solution:** Orchestrator now loads default preset on startup and syncs with preset dropdown.

---

## ? Setup Steps

### 1. Assign Default Preset in Inspector

**Select SimulationUIOrchestrator GameObject:**

In Inspector, find:
- **Default Preset** field

**Drag your internal demo preset** from Project window into this field.

**Example:**
- If your demo preset is at: `Assets/Viable/Core.Unity/Presets/Examples/InternalDemo.asset`
- Drag `InternalDemo.asset` to the **Default Preset** field

---

### 2. Ensure Preset is in Dropdown List

Your default preset **must be in the PresetSelectorUI's search path**:

**Check:**
1. Open Project window
2. Navigate to: `Assets/Viable/Core.Unity/Presets/Examples/`
3. Verify your default preset exists there

**If not there:**
1. Move or duplicate your preset to that folder
2. Or update `PresetSelectorUI.presetFolderPath` in Inspector

---

## ?? How It Works Now

### On Startup:

```
1. Orchestrator.Awake():
   - Loads defaultPreset ? WorkingConfig
   - Refreshes all UI controls

2. Orchestrator.Start():
   - Tells PresetSelectorUI to select the default preset
   - Dropdown shows correct preset

3. User sees:
   - Dropdown shows default preset name
   - All UI controls match default values
   - Ready to click Play with default behavior
```

### When User Selects Different Preset:

```
1. User changes dropdown selection
   ?
2. PresetSelectorUI.OnPresetSelected():
   - Loads new preset into orchestrator.WorkingConfig
   - Refreshes all UI controls
   ?
3. User clicks Apply button
   ?
4. Orchestrator.ApplyAndRestart():
   - Builds ScenarioDefinition from WorkingConfig
   - Restarts simulation with new config
```

### To Return to Default:

```
1. User selects default preset from dropdown
   (e.g., "Internal Demo")
   ?
2. UI controls update to default values
   ?
3. User clicks Apply
   ?
4. Simulation restarts with original demo behavior ?
```

---

## ?? Testing

### Test 1: Default Preset Loads on Startup

```
1. Make sure Default Preset is assigned in orchestrator Inspector
2. Press Play in Unity
3. Check preset dropdown - should show default preset name
4. Check UI controls - should match default values
? PASS if dropdown shows correct preset
```

### Test 2: Can Return to Default After Changes

```
1. Press Play
2. Change some settings (e.g., Inflow = Point Sources)
3. Click Apply - simulation changes
4. Select default preset from dropdown
5. Click Apply - simulation returns to original behavior
? PASS if demo behavior restored
```

### Test 3: Dropdown Syncs with Orchestrator

```
1. Press Play
2. Note which preset is selected in dropdown
3. That preset should match orchestrator.CurrentPreset
4. UI controls should match that preset's values
? PASS if dropdown, orchestrator, and UI all match
```

---

## ?? Troubleshooting

### Issue: Dropdown is empty or doesn't show presets

**Fix 1:** Check PresetSelectorUI search path
- Inspector ? PresetSelectorUI ? Preset Folder Path
- Should be: `Assets/Viable/Core.Unity/Presets/Examples`

**Fix 2:** Ensure presets are in that folder
- Open Project window ? Navigate to folder
- Verify .asset files exist there

**Fix 3:** Presets might be in Resources instead
- Check: `Assets/Resources/Presets/Examples/`
- Or: `Assets/Resources/Presets/`

---

### Issue: Dropdown shows preset but doesn't load it

**Fix:** Check orchestrator wiring
1. Select SimulationUIOrchestrator GameObject
2. Verify Default Preset field is assigned
3. Press Play
4. Check Console for: `[SimulationUIOrchestrator] Initializing with default preset: <name>`

---

### Issue: Clicking preset in dropdown doesn't change UI

**Fix:** Orchestrator reference missing
1. Select PresetSelectorUI GameObject
2. Inspector ? Orchestrator field
3. Should auto-wire, but manually drag if needed

---

### Issue: Apply button doesn't restart simulation

**Fix:** Apply button not wired
1. Select Apply button GameObject
2. Inspector ? Button component ? OnClick()
3. Should have: `SimulationUIOrchestrator.ApplyAndRestart()`
4. If missing, add it manually

---

## ?? Complete Setup Checklist

**Before pressing Play:**

- [ ] **Orchestrator GameObject** exists in scene
- [ ] **Orchestrator ? Simulation Controller** field assigned
- [ ] **Orchestrator ? Default Preset** field assigned ? KEY!
- [ ] **PresetSelectorUI ? Orchestrator** field auto-wired (verify)
- [ ] **Apply button ? OnClick()** wired to orchestrator.ApplyAndRestart()
- [ ] **Default preset** exists in Presets/Examples folder
- [ ] All 6 UI section controllers have orchestrator references

**After pressing Play:**

- [ ] Console shows: "Initializing with default preset: <name>"
- [ ] Preset dropdown shows default preset selected
- [ ] UI controls show default values
- [ ] Can select different preset from dropdown
- [ ] Can return to default preset from dropdown
- [ ] Click Apply ? simulation restarts with new config

---

## ?? Success!

You now have:
- ? Default preset loads automatically on startup
- ? Preset dropdown syncs with orchestrator
- ? Can always return to default demo behavior
- ? Clear workflow: Select Preset ? Edit ? Apply ? Restart

**Your default demo is never lost!** ??
