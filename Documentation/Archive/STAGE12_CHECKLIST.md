# Stage 12 Completion Checklist

## ? Before Building

### Scripts Created (In Project)
- [ ] `UIManager.cs` exists in `Assets/Viable/Core.Unity/UI/`
- [ ] `PresetSelectorUI.cs` exists
- [ ] `SimulationControlsUI.cs` exists
- [ ] `InfoDisplayUI.cs` exists
- [ ] `ExportUI.cs` exists
- [ ] `ParameterEditorUI.cs` exists (optional)

**Status:** If scripts exist in project, ? this section complete

---

### Scene Setup (In Build Scene!)

**?? CRITICAL: Verify you're editing the scene that will build!**

**Which scene builds?**
- Check `File > Build Settings > Scenes In Build`
- Index 0 = scene that will build
- Usually: `Assets/Scenes/Main.unity`

**In that scene, verify:**

#### GameObjects Exist
- [ ] `SimulationManager` GameObject with `SimulationController` component
- [ ] `UIManager` GameObject with `UIManager` component
- [ ] `UICanvas` GameObject (with Canvas Scaler)
- [ ] `EventSystem` GameObject

#### UI Panels Under UICanvas
- [ ] `PresetSelectorPanel` with `PresetSelectorUI` component
- [ ] `SimulationControlsPanel` with `SimulationControlsUI` component
- [ ] `InfoDisplayPanel` with `InfoDisplayUI` component
- [ ] `ExportPanel` with `ExportUI` component

#### References Wired (Inspector)
- [ ] UIManager ? `simulationController` field points to SimulationController
- [ ] UIManager ? `presetSelector` field points to PresetSelectorUI
- [ ] UIManager ? `simulationControls` field points to SimulationControlsUI
- [ ] UIManager ? `infoDisplay` field points to InfoDisplayUI
- [ ] UIManager ? `exportUI` field points to ExportUI

#### Each Panel Has UI Elements
- [ ] PresetSelectorPanel has Dropdown, Description Text, Load Button, Feedback Text
- [ ] SimulationControlsPanel has Play Button, Pause Button, Stop Button, Speed Slider, Tick Text
- [ ] InfoDisplayPanel has all metric text labels
- [ ] ExportPanel has Export Button, Path Text, Feedback Text

---

### Resources Folder
- [ ] `Assets/Resources/Presets/Examples/` folder exists
- [ ] 5 preset files copied there:
  - [ ] 01_BalancedPersistence.asset
  - [ ] 02_ResourceStress.asset
  - [ ] 03_RapidExpansion.asset
  - [ ] 04_CompetingRegions.asset
  - [ ] 05_StochasticDynamics.asset

---

### Pre-Build Tests

#### In Unity Editor
- [ ] Open the build scene (Main.unity)
- [ ] Press Play
- [ ] ? UI panels visible?
- [ ] ? Preset dropdown populates?
- [ ] ? Play button works?
- [ ] ? Info display updates?
- [ ] ? Export button works?
- [ ] ? No console errors?

**If ANY ?:** Fix before building!

#### Save Scene
- [ ] Scene saved (Ctrl+S) - **CRITICAL!**
- [ ] Check Window title shows no asterisk (*)

---

### Build Settings
- [ ] Correct scene at index 0
- [ ] Only scenes you want are checked
- [ ] Platform = PC, Mac & Linux Standalone
- [ ] Architecture = x86_64

---

### Clean Build
- [ ] Delete old Build folder
- [ ] Close Unity
- [ ] Reopen Unity
- [ ] Open build scene
- [ ] Build fresh

---

## ? After Building

### Test Built Executable
- [ ] Run the .exe
- [ ] UI panels visible in standalone?
- [ ] Preset dropdown works?
- [ ] Play/Pause works?
- [ ] Export works?

**If ?:** See `STAGE12_BUILD_TROUBLESHOOTING.md`

---

## ?? Common Issues

### Issue 1: UI Scripts Exist But Not in Scene
**Symptom:** Code compiles, but no UI when you press Play

**Fix:** Follow `STAGE12_UNITY_SETUP.md` to add UI to scene

### Issue 2: UI Works in Editor, Not in Build
**Symptom:** Editor shows UI, exe doesn't

**Fix:** Wrong scene is building. Check Build Settings.

### Issue 3: Build Shows Old Version
**Symptom:** Built exe has old simulation, no UI

**Fix:** See `BUILD_FIX_QUICK.md`

---

## ?? Success Criteria

**Stage 12 Complete When:**
- ? All checkboxes above are ?
- ? UI visible in Editor Play mode
- ? UI visible in built executable
- ? All UI functions work (load preset, play, export)
- ? No console errors
- ? All existing tests still pass (35/35)

---

## ?? Progress Tracker

| Phase | Status | Details |
|-------|--------|---------|
| Scripts Created | ? | In project? |
| Scene Setup | ? | In build scene? |
| References Wired | ? | Inspector complete? |
| Resources Folder | ? | Presets copied? |
| Editor Test | ? | Works in Play mode? |
| Build Test | ? | Works in exe? |

**When all ?:** Stage 12 complete! ??

---

## ??? Quick Diagnostic

Run in Unity Editor to check scene setup:

```csharp
// Add Stage12DiagnosticCheck component to any GameObject
// Press Play, check Console
```

---

**Use this checklist before every build to avoid issues!**
