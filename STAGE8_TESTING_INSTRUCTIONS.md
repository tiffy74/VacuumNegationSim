# Stage 8 Complete - Testing Instructions

**Status:** ? **CODE COMPLETE** - Ready for Unity Testing

---

## ?? **What Was Done**

All Unity layer files moved to `Assets/Viable/Core.Unity/` with:
- ? Preset system (ScenarioPreset + Adapter)
- ? Refactored SimulationController (preset-aware)
- ? Moved rendering files (GridRenderer)
- ? Moved visual files (CameraController, CellVisualiser)
- ? Moved grid management (SimulationGrid)
- ? Created first preset (ConstraintsExpansionDemo)
- ? Added protection documentation

---

## ?? **TESTING REQUIRED - Do This Now**

### **Step 1: Close Visual Studio**
```
File ? Exit
(Let Unity reimport files)
```

### **Step 2: Open Unity**
```
Double-click Unity project
Wait for reimport (1-2 minutes)
```

### **Step 3: Check Console**
**Expected:** Warnings about missing scripts (normal - scene needs updating)  
**If errors:** Share them and I'll help fix

### **Step 4: Fix Scene References**

**SimulationManager GameObject:**
1. Remove old `SimulationController` component
2. Add Component ? `Viable.Core.Unity.Controllers.SimulationController`
3. Settings:
   - Scenario Preset ? `ConstraintsExpansionDemo.asset`
   - Allow Inspector Override ? `false`
   - Grid ? (drag SimulationManager GameObject)

4. Remove old `SimulationGrid` component
5. Add Component ? `Viable.Core.Unity.Controllers.SimulationGrid`
6. Settings:
   - Width: 64
   - Height: 64
   - Cell Size: 0.1
   - Cell Prefab: (your cell prefab)

**Main Camera:**
1. Remove old `CameraController` component
2. Add Component ? `Viable.Core.Unity.Visuals.CameraController`
3. Settings:
   - Grid ? (drag SimulationManager GameObject)

**Cell Prefab:**
1. Open prefab
2. Remove old `CellVisualiser` component
3. Add Component ? `Viable.Core.Unity.Visuals.CellVisualiser`
4. Save prefab

### **Step 5: Test Simulation**
```
Press Play
```

**Expected behavior:**
- ? Central 5×5 seed appears (white)
- ? Yellow frontier expands
- ? Magenta sinks form at boundaries
- ? Console shows tick logs
- ? Visual output identical to before

**If it works:** ?? Stage 8 successful!

### **Step 6: Run Tests**
```
Window ? General ? Test Runner
Click "Run All"
```

**Expected:** ? 25/25 tests passing

---

## ?? **Troubleshooting**

### **Error: "Type 'Controllers' not found"**
```
Assets ? Reimport All
Wait for completion
Restart Unity
```

### **Error: "Preset asset missing script"**
The .asset file has a placeholder GUID. Fix:
```
1. Delete ConstraintsExpansionDemo.asset
2. Assets ? Create ? Viable ? Scenario Preset
3. Name: "ConstraintsExpansionDemo"
4. Fill in values from README_INTERNAL.md
5. Save in Presets/Internal/ folder
```

### **Error: "Grid field is null"**
```
Select Main Camera
CameraController component
Grid field ? Drag "SimulationManager" GameObject
```

### **Warning: "Missing script references"**
Normal - happens when files move. Just:
```
1. Remove old component (broken)
2. Add new component (from Core.Unity namespace)
3. Reconfigure settings
```

---

## ? **Success Checklist**

After testing, verify:
- [ ] Unity Console: 0 errors
- [ ] Simulation runs and looks identical
- [ ] Preset loads (check Console for "Loading from preset" log)
- [ ] All 25 tests pass
- [ ] Camera centers correctly
- [ ] Colors match (yellow frontier, magenta sinks)

---

## ?? **If Everything Works**

Commit Stage 8:
```sh
git add Assets/Viable/Core.Unity/
git add STAGE8_IMPLEMENTATION.md
git add STAGE8_TESTING_COMPLETE.md
git commit -m "feat: Stage 8 - Core.Unity productisation complete

Move Unity layer to Core.Unity with preset system:
- Created ScenarioPreset (ScriptableObject)
- Refactored SimulationController (preset-aware)
- Moved all visualization files
- Created ConstraintsExpansionDemo preset
- Protected internal preset for reproducibility

Structure:
  Core.Unity/
    Controllers/ (SimulationController, SimulationGrid)
    Rendering/ (GridRenderer)
    Visuals/ (CameraController, CellVisualiser)
    Presets/Internal/ (Demo preset + protection)

Backward compatible: works with or without preset
Stage 9 ready: export accessors in place

Tested: Unity compiles, sim runs, tests pass"

git push origin Viable
```

---

## ?? **Next: Stage 9**

Once Stage 8 is verified:
1. Export mechanism (RunExporter)
2. Manifest/CSV writers
3. Export button in UI
4. Run artifact generation

---

**Current Status:** ? Code complete, awaiting Unity testing  
**Action Required:** Follow testing steps above  
**Expected Time:** 15-20 minutes to test and verify

