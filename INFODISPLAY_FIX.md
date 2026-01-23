# InfoDisplay Not Updating - Diagnostic Guide

## Issue
InfoDisplay and SimulationControls panels not showing live tick/metrics.

---

## ? Changes Made

### 1. **Fixed SimulationControlsUI.cs**
- ? Was showing: `Tick: [N/A]`
- ? Now shows: `Tick: {actualValue}`
- Uses: `simulationController.GetCurrentTick()`

### 2. **Fixed InfoDisplayUI.cs**
- Changed `updateFrequency` from 10 to **1** (updates every frame)
- Added error handling
- Added debug logging
- Fixed metric retrieval

### 3. **Enhanced UIManager.cs**
- Added initialization logging
- Added null checks with warnings
- Better error reporting

---

## ?? Diagnostic Steps

### Step 1: Check Console on Play

**Press Play in Unity**, then check Console for these messages:

**Expected messages:**
```
[UIManager] SimulationController found automatically
[UIManager] PresetSelector initialized
[UIManager] SimulationControls initialized
[UIManager] InfoDisplay initialized
[UIManager] ExportUI initialized
[UIManager] UI initialization complete
[InfoDisplayUI] Initialized with SimulationController
```

**If you see warnings:**
```
[UIManager] SimulationController not found!
[UIManager] InfoDisplay is null!
```
? **Problem:** UI components not wired in Inspector

---

## ?? Fix: Wire UI Components in Inspector

### Step-by-Step Inspector Setup

#### 1. **Select UIManager GameObject** (in Hierarchy)

#### 2. **In Inspector, UIManager component:**

**Simulation Controller field:**
- Should point to: `SimulationManager` GameObject
- If null: Drag `SimulationManager` from Hierarchy to this field

**Preset Selector field:**
- Should point to: `PresetSelectorPanel` GameObject
- If null: Drag `PresetSelectorPanel` to this field

**Simulation Controls field:**
- Should point to: `SimulationControlsPanel` GameObject
- If null: Drag `SimulationControlsPanel` to this field

**Info Display field:**
- Should point to: `InfoDisplayPanel` GameObject
- If null: Drag `InfoDisplayPanel` to this field

**Export UI field:**
- Should point to: `ExportPanel` GameObject
- If null: Drag `ExportPanel` to this field

#### 3. **Select InfoDisplayPanel** (in Hierarchy)

**In Inspector, InfoDisplayUI component:**

Wire all text fields:
- **Tick Text** ? Drag the TextMeshProUGUI that shows tick
- **Viable Count Text** ? Drag the TextMeshProUGUI that shows viable count
- **Active Count Text** ? Drag the TextMeshProUGUI that shows active count
- **Sink Count Text** ? Drag the TextMeshProUGUI that shows sink count
- **Resource Global Text** ? Drag the TextMeshProUGUI that shows resource

**Set Update Frequency:**
- Change to **1** (updates every frame)

#### 4. **Select SimulationControlsPanel** (in Hierarchy)

**In Inspector, SimulationControlsUI component:**

Wire all elements:
- **Play Button** ? Drag the Button GameObject
- **Pause Button** ? Drag the Button GameObject
- **Stop Button** ? Drag the Button GameObject
- **Speed Slider** ? Drag the Slider GameObject
- **Speed Text** ? Drag the TextMeshProUGUI for speed
- **Tick Text** ? Drag the TextMeshProUGUI for tick

---

## ?? Quick Test

After wiring everything:

1. **Save Scene** (Ctrl+S)
2. **Press Play**
3. **Check Console** for initialization messages
4. **Watch InfoDisplay panel** (top-right)
   - Numbers should update
5. **Watch SimulationControls panel** (top-center)
   - Tick should increment

---

## ?? Common Issues

### Issue 1: "SimulationController not found!"

**Cause:** No GameObject with SimulationController component in scene

**Fix:**
1. Check if `SimulationManager` exists in Hierarchy
2. Check if it has `SimulationController` component
3. If missing: Add the component
4. Wire it to UIManager

---

### Issue 2: "InfoDisplay is null!"

**Cause:** InfoDisplayPanel not wired to UIManager

**Fix:**
1. Select UIManager GameObject
2. In Inspector, find "Info Display" field
3. Drag `InfoDisplayPanel` from Hierarchy to that field
4. Press Play again

---

### Issue 3: Numbers Show "0" and Never Update

**Cause:** Text fields not wired in InfoDisplayUI component

**Fix:**
1. Select `InfoDisplayPanel` in Hierarchy
2. Find `InfoDisplayUI` component in Inspector
3. Wire all 5 text fields (see Step 3 above)
4. Save and Play again

---

### Issue 4: Console Shows "GetCurrentTick() returned 0"

**Possible causes:**
- Simulation not started yet (normal at tick 0)
- SimulationController not initialized
- Context is null

**Check:**
1. Wait a few seconds after pressing Play
2. Tick should increment: 0 ? 1 ? 2 ? 3...
3. If stuck at 0: Simulation might be paused

---

### Issue 5: "Error updating metrics: NullReferenceException"

**Cause:** Metrics dictionary or context is null

**Check:**
1. SimulationController.GetCurrentMetrics() exists? ? (it does)
2. Simulation running? (Check if other simulation logs appear)
3. Context initialized? (Check [Tick X] logs)

---

## ? Success Criteria

**When working correctly, you should see:**

### In Console (while playing):
```
[Tick 0] TickSimulation start (Engine)
[Tick 0] Regions=25 Sinks=0 Viable=25 ...
[Tick 10] TickSimulation start (Engine)
[Tick 20] TickSimulation start (Engine)
...
```

### In InfoDisplay Panel:
```
Tick: 0 ? 1 ? 2 ? 3 ? 10 ? 20 ? 30...
Viable: 25
Active: 25
Sinks: 0
Resource: 10000000 (decreasing over time)
```

### In SimulationControls Panel:
```
Tick: 0 ? 1 ? 2 ? 3 ? 10 ? 20 ? 30...
```

---

## ?? Visual Checklist

**Take screenshots of:**
1. UIManager Inspector (showing all fields wired)
2. InfoDisplayPanel Inspector (showing all text fields wired)
3. SimulationControlsPanel Inspector (showing all fields wired)
4. Console during Play (showing initialization messages)

**If still not working, share these screenshots!**

---

## ?? Reset Steps (If Completely Broken)

1. **Close Unity**
2. **Delete** `Library` folder (Unity will regenerate)
3. **Reopen Unity**
4. **Open your scene**
5. **Rewire all Inspector references**
6. **Press Play**

---

## ?? Pro Tip

**Set Update Frequency = 1** in InfoDisplayUI for immediate feedback.

**After it works**, you can increase to 5 or 10 to reduce overhead.

---

**Next Step:** Follow the Inspector wiring steps above, then test again!
