# ?? UI & Simulation Integration - Complete Verification Checklist

## ?? **Overview**

This document provides a systematic test plan to verify that:
1. ? **UI works correctly** (buttons, dropdowns, feedback)
2. ? **UI actually controls simulation** (not just cosmetic)
3. ? **Different presets produce different behaviors**
4. ? **Everything works in builds** (not just Editor)

---

## ?? **Test Categories**

1. [UI Feedback & Polish](#1-ui-feedback--polish) - Does UI feel responsive?
2. [Preset System](#2-preset-system) - Do presets load and affect simulation?
3. [Playback Controls](#3-playback-controls) - Do Play/Pause/Step/Restart work?
4. [Export System](#4-export-system) - Does export create files?
5. [Build Verification](#5-build-verification) - Does it work in standalone build?

---

## 1?? **UI Feedback & Polish**

### **Test 1.1: Button Hover Feedback**

**What to Test:** All buttons should brighten when hovered.

**Steps:**
1. Press Play in Unity Editor
2. Move mouse over each button **WITHOUT CLICKING**:
   - Load
   - Play/Pause
   - Step
   - Restart
   - Export

**Expected Result:**
- ? Button **brightens noticeably** (20% brighter)
- ? Change is **smooth** (0.1s transition)
- ? Easy to see where mouse cursor is

**Pass Criteria:**
- [ ] Load button brightens on hover
- [ ] Play/Pause button brightens on hover
- [ ] Step button brightens on hover
- [ ] Restart button brightens on hover
- [ ] Export button brightens on hover

**If Failed:**
- Check `hoverBrightnessMultiplier` in TopBarUI Inspector
- Increase value to 1.3 or 1.4 for more obvious effect

---

### **Test 1.2: Button Click Feedback**

**What to Test:** Buttons should darken when clicked.

**Steps:**
1. Press Play
2. Click and **hold** each button:
   - Load
   - Play/Pause
   - Step

**Expected Result:**
- ? Button **darkens** when mouse down (30% darker)
- ? Returns to normal when mouse up
- ? Smooth transition

**Pass Criteria:**
- [ ] Buttons darken visibly when pressed
- [ ] Change is noticeable (not subtle)
- [ ] Works for all buttons

**If Failed:**
- Check `pressedDarknessMultiplier` in TopBarUI Inspector
- Decrease value to 0.5 or 0.6 for more obvious effect

---

### **Test 1.3: Play/Pause Pulse Effect**

**What to Test:** Play/Pause button should pulse when state changes.

**Steps:**
1. Press Play
2. Click "Play" button
3. Watch button carefully
4. Click "Pause" button
5. Watch button again

**Expected Result:**
- ? Button **flashes brighter** for 0.15s when clicked
- ? Button text changes: "Play" ? "Pause"
- ? Clear visual confirmation of state change

**Pass Criteria:**
- [ ] Button pulses when Play ? Pause
- [ ] Button pulses when Pause ? Play
- [ ] Text changes correctly
- [ ] Pulse is noticeable

---

### **Test 1.4: Dropdown Auto-Close**

**What to Test:** Dropdowns should close after selection.

**Steps:**
1. Press Play
2. Click Preset dropdown
3. **Single-click** any preset option
4. Check if dropdown closes

**Expected Result:**
- ? Dropdown closes **immediately** (after 1 frame delay)
- ? No need for second click
- ? Selected preset shows in dropdown

**Pass Criteria:**
- [ ] Preset dropdown closes on single-click
- [ ] Speed dropdown closes on single-click
- [ ] Selected value updates correctly
- [ ] No double-click needed

**If Failed:**
- Check Console for: `[TopBarUI] Preset dropdown changed to index X`
- If missing, dropdown event not firing
- Try clicking directly on item text (not checkbox)

---

### **Test 1.5: Default Preset Selection**

**What to Test:** "Default" should be first in dropdown and pre-selected.

**Steps:**
1. Press Play
2. Check Preset dropdown (don't open it yet)
3. Open dropdown
4. Check list order

**Expected Result:**
- ? Dropdown shows "Default" as selected
- ? Opening dropdown shows "Default" at top of list
- ? Other presets sorted alphabetically below

**Pass Criteria:**
- [ ] "Default" is pre-selected on startup
- [ ] "Default" appears first in list
- [ ] Other presets are alphabetically sorted

**Console Should Show:**
```
[TopBarUI] Found 'Default' at index: X
[TopBarUI] ? Moved 'Default' to front of preset list
[TopBarUI] Final preset order: Default, ...
```

---

## 2?? **Preset System**

### **Test 2.1: Load Default Preset**

**What to Test:** Default preset loads and initializes correctly.

**Steps:**
1. Press Play
2. Select "Default" from dropdown
3. Click "Load" button
4. Click "Play" button
5. Watch simulation for 100 ticks

**Expected Result:**
- ? Console shows: `[SimulationController] Loading preset: Default`
- ? Console shows: `[SimulationController] ? Preset loaded: Default`
- ? Simulation starts from tick 0
- ? Grid shows 5×5 yellow seed region in center
- ? Seed region gradually expands (stable growth)

**Pass Criteria:**
- [ ] Console confirms preset loaded
- [ ] Tick counter resets to 0
- [ ] Grid resets to 5×5 seed
- [ ] Simulation shows stable expansion

**Expected Behavior:**
- Moderate expansion rate
- Few sinks (< 10 by tick 100)
- Yellow frontier gradually spreading
- No rapid collapse

---

### **Test 2.2: Switch to Resource Stress Preset**

**What to Test:** Different preset produces **DIFFERENT** behavior.

**Steps:**
1. **Baseline:** Load "Default" ? Play ? Note behavior (100 ticks)
2. **Test:** Load "Resource Stress" ? Play ? Compare behavior (100 ticks)

**Expected Result:**
- ? **Default:** Stable expansion, few sinks, gradual growth
- ? **Resource Stress:** Faster collapse, more sinks, cells die quickly

**Pass Criteria:**
- [ ] Preset loads successfully
- [ ] Tick resets to 0
- [ ] **Behavior is VISIBLY DIFFERENT from Default**
- [ ] More magenta sinks appear
- [ ] Cells die faster (yellow ? dark)

**If Behaviors Look Identical:**
- ? **CRITICAL BUG** - Presets not affecting simulation!
- Check: Does SimulationController.LoadPreset() call InitializeSimulation()?
- Check: Does preset have different parameter values?

---

### **Test 2.3: Switch to Rapid Expansion Preset**

**What to Test:** Fast expansion preset expands faster than default.

**Steps:**
1. Load "Rapid Expansion"
2. Click "Load" button
3. Click "Play" button
4. Watch first 50 ticks

**Expected Result:**
- ? **Much faster expansion** than Default
- ? Frontier advances quickly
- ? Large active region by tick 50
- ? Few or no sinks (high resources sustain growth)

**Pass Criteria:**
- [ ] Expansion is **noticeably faster** than Default
- [ ] Large yellow/green region quickly
- [ ] Behavior is DISTINCT from other presets

---

### **Test 2.4: Multiple Preset Switches**

**What to Test:** Can switch between presets multiple times without errors.

**Steps:**
1. Load "Default" ? Play ? 50 ticks ? Pause
2. Load "Resource Stress" ? Play ? 50 ticks ? Pause
3. Load "Rapid Expansion" ? Play ? 50 ticks ? Pause
4. Load "Default" ? Play ? 50 ticks ? Pause

**Expected Result:**
- ? Each preset loads successfully
- ? No errors in Console
- ? Tick resets to 0 each time
- ? Behavior matches expected preset each time

**Pass Criteria:**
- [ ] All 4 loads succeed
- [ ] No NullReferenceException
- [ ] No "Could not find preset" errors
- [ ] Each preset produces expected behavior

---

## 3?? **Playback Controls**

### **Test 3.1: Play/Pause Toggle**

**What to Test:** Play/Pause button controls simulation.

**Steps:**
1. Press Play (Unity)
2. Click "Play" button (TopBar)
3. Wait 20 ticks
4. Click "Pause" button
5. Wait 5 seconds
6. Click "Play" again

**Expected Result:**
- ? "Play" starts simulation ticking
- ? Tick counter increases
- ? "Pause" **stops** tick counter
- ? Tick counter **stays frozen** while paused
- ? "Play" again resumes from where it paused

**Pass Criteria:**
- [ ] Simulation starts when Play clicked
- [ ] Simulation pauses when Pause clicked
- [ ] Tick counter doesn't increase while paused
- [ ] Resume continues from paused tick

**Console Should Show:**
```
Tick 1 | ...
Tick 2 | ...
... (paused - no more tick logs)
... (resumed)
Tick 21 | ...
```

---

### **Test 3.2: Step Button**

**What to Test:** Step button advances simulation by 1 tick while paused.

**Steps:**
1. Start simulation
2. Click "Pause"
3. Click "Step" button 5 times
4. Check tick counter each time

**Expected Result:**
- ? Tick increases by exactly 1 per Step click
- ? Simulation updates (visual changes)
- ? Console shows new tick log
- ? No auto-advance (stays paused after Step)

**Pass Criteria:**
- [ ] Tick counter: 0 ? 1 ? 2 ? 3 ? 4 ? 5
- [ ] Grid updates after each Step
- [ ] Console shows tick logs
- [ ] Simulation doesn't auto-continue

---

### **Test 3.3: Restart Button**

**What to Test:** Restart resets simulation to tick 0.

**Steps:**
1. Start simulation
2. Click "Play" ? Run to tick 50
3. Click "Restart" button
4. Check tick counter
5. Check grid visual

**Expected Result:**
- ? Tick counter resets to 0
- ? Grid resets to 5×5 seed region
- ? All cells cleared except seed
- ? Simulation ready to run again

**Pass Criteria:**
- [ ] Tick shows 0 after restart
- [ ] Grid shows only 5×5 seed in center
- [ ] Console shows: `[TopBarUI] Restart`
- [ ] Can click Play to run again

---

## 4?? **Export System**

### **Test 4.1: Export After Running**

**What to Test:** Export creates files with simulation data.

**Steps:**
1. Start simulation
2. Run for 100 ticks
3. Click "Export" button
4. Check Console for path
5. Open File Explorer to that path

**Expected Result:**
- ? Console shows: `[TopBarUI] ? Export complete: <path>`
- ? Export folder exists: `VIABLE_Run_YYYY-MM-DDTHHMMSSZ_xxxxxx/`
- ? Folder contains 6 files:
  - `scenario.json`
  - `request.json`
  - `metrics.csv`
  - `summary.md`
  - `manifest.json`
  - `checksums.txt`

**Pass Criteria:**
- [ ] Console shows export path
- [ ] Export folder created
- [ ] 6 files present
- [ ] No errors in Console

**Console Should Show:**
```
[TopBarUI] Exporting current run...
[RunExporter] Exporting to: C:\Users\...\Exports\VIABLE_Run_...
[RunExporter] Export complete: <path>
[TopBarUI] ? Export complete: <path>
```

---

### **Test 4.2: Metrics CSV Content**

**What to Test:** CSV contains actual simulation data.

**Steps:**
1. After Test 4.1, navigate to export folder
2. Open `metrics.csv` in Excel or text editor
3. Check content

**Expected Result:**
- ? CSV has header row: `Tick,Time,ViableCount,ActiveCount,SinkCount,...`
- ? Multiple data rows (1 per 10 ticks)
- ? Numbers change over time (not all zeros)
- ? Tick column shows 0, 10, 20, 30, ...

**Pass Criteria:**
- [ ] Header row present
- [ ] ~10 data rows (for 100 tick run)
- [ ] ViableCount/ActiveCount > 0
- [ ] Values change between rows

**Example:**
```csv
Tick,Time,ViableCount,ActiveCount,SinkCount,...
0,0.0,25,25,0,...
10,10.0,150,200,2,...
20,20.0,380,450,5,...
```

---

### **Test 4.3: Summary MD Content**

**What to Test:** Summary has human-readable report.

**Steps:**
1. Open `summary.md` in text editor
2. Check content

**Expected Result:**
- ? Shows run metadata (ID, date, time)
- ? Shows configuration (grid size, seed, parameters)
- ? Shows results (viable cells, sinks, execution time)
- ? Readable format (Markdown)

**Pass Criteria:**
- [ ] File opens correctly
- [ ] Contains run information
- [ ] Contains configuration
- [ ] Contains results

---

## 5?? **Build Verification**

### **Test 5.1: Build with Presets**

**What to Test:** Standalone build has working presets.

**Steps:**
1. Unity ? File ? Build Settings
2. Build and Run (or Build then run .exe)
3. Check preset dropdown in built app

**Expected Result:**
- ? Dropdown shows all 5 presets
- ? "Default" is first
- ? Load button works
- ? Presets affect simulation

**Pass Criteria:**
- [ ] Dropdown populated (not empty)
- [ ] 5 presets visible
- [ ] Can load presets
- [ ] Different presets produce different behaviors

**If Dropdown Empty:**
- ? **CRITICAL** - Presets not in Resources folder
- Check: `Assets/Viable/Core.Unity/Resources/Presets/Examples/` contains .asset files
- Rebuild after copying presets to Resources

---

### **Test 5.2: Build UI Feedback**

**What to Test:** Button feedback works in build.

**Steps:**
1. In built app, hover over buttons
2. Click buttons

**Expected Result:**
- ? Buttons brighten on hover
- ? Buttons darken on click
- ? Play/Pause pulses
- ? Dropdowns close on selection

**Pass Criteria:**
- [ ] Hover feedback visible
- [ ] Click feedback visible
- [ ] All controls work

---

### **Test 5.3: Build Export**

**What to Test:** Export works in standalone build.

**Steps:**
1. In built app, run simulation for 50 ticks
2. Click "Export"
3. Check export location

**Expected Result:**
- ? Export completes (no errors)
- ? Files created in: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\VacuumNegationSim\Exports\`
- ? 6 files present in export folder

**Pass Criteria:**
- [ ] Export succeeds
- [ ] Files created
- [ ] No crashes

---

## ?? **Summary Scorecard**

### **UI Feedback (5 tests)**
- [ ] Button hover feedback works
- [ ] Button click feedback works
- [ ] Play/Pause pulse works
- [ ] Dropdowns auto-close
- [ ] Default preset first

**Score: ___ / 5**

---

### **Preset System (4 tests)**
- [ ] Default preset loads
- [ ] Resource Stress preset DIFFERENT from Default
- [ ] Rapid Expansion preset DIFFERENT from Default
- [ ] Multiple switches work

**Score: ___ / 4**

---

### **Playback Controls (3 tests)**
- [ ] Play/Pause toggle works
- [ ] Step advances by 1 tick
- [ ] Restart resets to tick 0

**Score: ___ / 3**

---

### **Export System (3 tests)**
- [ ] Export creates files
- [ ] Metrics CSV contains data
- [ ] Summary MD readable

**Score: ___ / 3**

---

### **Build Verification (3 tests)**
- [ ] Build has presets
- [ ] Build UI feedback works
- [ ] Build export works

**Score: ___ / 3**

---

## ?? **Overall Pass/Fail**

**Total Score: ___ / 18**

**Status:**
- ? **PASS (16-18):** Everything works great!
- ?? **NEEDS WORK (12-15):** Some issues, but mostly functional
- ? **FAIL (< 12):** Major issues, needs debugging

---

## ?? **Common Issues & Fixes**

### **Issue: Presets Don't Affect Simulation**

**Symptoms:**
- All presets look the same
- No behavior differences

**Fix:**
1. Check SimulationController.LoadPreset() calls InitializeSimulation()
2. Check InitializeSimulation() uses preset parameters
3. Check preset files have different parameter values

---

### **Issue: Dropdowns Stay Open**

**Symptoms:**
- Need double-click to close
- Dropdown doesn't close after selection

**Fix:**
1. Check CloseDropdownDelayed() is called
2. Check dropdown.onValueChanged has listener
3. Try increasing delay: `yield return new WaitForSeconds(0.1f);`

---

### **Issue: Build Has No Presets**

**Symptoms:**
- Dropdown empty in build
- Console: "No presets found"

**Fix:**
1. Copy presets to: `Assets/Viable/Core.Unity/Resources/Presets/Examples/`
2. Rebuild application
3. Check build includes Resources folder

---

### **Issue: Export Fails**

**Symptoms:**
- Console: "No run to export"
- Empty export path

**Fix:**
1. Run simulation for at least 10 ticks first
2. Check lastResult is updated in SimulationController
3. Check export path is writable

---

## ?? **Next Steps After Verification**

**If Tests Pass:**
1. ? Commit current working state
2. ? Update documentation with verification results
3. ? Move to Stage 13 remaining tasks:
   - Apply & Restart button
   - RightDock dropdown integration
   - Sink controls

**If Tests Fail:**
1. ? Document which tests failed
2. ? Use "Common Issues & Fixes" section
3. ? Debug specific failures
4. ? Re-run verification after fixes

---

**Created:** 2024  
**Last Updated:** 2024  
**Status:** ?? Ready for Verification  
**Use:** Run through all tests systematically to ensure everything works
                                             