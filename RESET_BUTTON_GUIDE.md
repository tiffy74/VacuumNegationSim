# ?? Reset Button - Quick Setup Guide

## **Overview**
The **Reset** button provides a one-click way to:
- ? Load default preset back into UI
- ? Clear all mechanism tweaks
- ? Restart simulation at **Tick 0**
- ? Show notification feedback

---

## **?? Unity Editor Setup**

### **Step 1: Create Button Container**

The Reset and Apply buttons should be in a horizontal row at the bottom of the Setup Panel.

**Hierarchy:**
```
SetupPanel/
??? PresetControlsSection
??? MechanismsSection  
??? CoreParametersSection
??? BottomButtonRow (NEW)
    ??? ResetButton (NEW)
    ??? ApplyButton (existing - move here)
```

### **Step 2: Create BottomButtonRow**

1. Right-click `SetupPanel` (at the **END** of children list)
2. Create Empty
3. Rename to `BottomButtonRow`
4. Add **HorizontalLayoutGroup** component:
   - Padding: `Left: 10, Right: 10, Top: 5, Bottom: 10`
   - Spacing: `10`
   - Child Alignment: **Middle Center**
   - Child Force Expand: **Width ?**, Height ?
   - Child Control Size: **Width ?**, **Height ?**

### **Step 3: Add Reset Button**

1. Right-click `BottomButtonRow` ? UI ? Button - TextMeshPro
2. Rename to `ResetButton`
3. Set button text: `"Reset"`
4. Add **LayoutElement** component:
   - **Min Width:** `120`
   - **Preferred Height:** `45`
5. Button **Colors** (ColorBlock):
   - **Normal:** `#E74C3C` (Red - indicates destructive action)
   - **Highlighted:** `#EC7063` (Lighter red on hover)
   - **Pressed:** `#C0392B` (Darker red on click)
   - **Disabled:** `#95A5A6` (Gray)
6. Text styling:
   - **Font Size:** `16`
   - **Style:** Bold
   - **Color:** White `#FFFFFF`

### **Step 4: Add ResetButton Script**

1. Select `ResetButton` GameObject
2. **Add Component** ? Search for `ResetButton`
3. Assign references in Inspector:
   - **Reset Button** ? Drag the `Button` component from this same GameObject
   - **Default Preset** ? (Optional) Drag your default preset ScriptableObject
     - If left empty, it auto-finds the first "Default" or "00_" preset

### **Step 5: Move Apply Button (if needed)**

If `ApplyButton` is already created directly under `SetupPanel`:

1. Select `ApplyButton` GameObject
2. Drag it **into** `BottomButtonRow` (as a child)
3. Verify it appears **after** `ResetButton` in the hierarchy
4. Add **LayoutElement** component (if not present):
   - **Min Width:** `120`
   - **Preferred Height:** `45`

---

## **?? Visual Result**

### **Before (Apply only):**
```
???????????????????????????????????
?  MECHANISMS                     ?
?  ...                            ?
???????????????????????????????????
?  [          Apply          ]    ? ? Full width
???????????????????????????????????
```

### **After (Reset + Apply):**
```
???????????????????????????????????
?  MECHANISMS                     ?
?  ...                            ?
???????????????????????????????????
?    [   Reset   ]  [   Apply   ] ? ? Two buttons, centered
???????????????????????????????????
      Red/Orange      Green
```

---

## **?? Testing**

### **Test 1: Reset from Default**
1. Load "Default" preset
2. Change Grid Topology to TriGrid
3. Change Inflow to PointSources
4. Run simulation for 50 ticks
5. Click **"Reset"**
   - ? Notification: "? Reset to defaults (Tick 0)"
   - ? UI reverts to Default preset values
   - ? Simulation restarts at Tick 0
   - ? Grid Topology back to RectGrid
   - ? Inflow back to Uniform

### **Test 2: Reset from Custom Config**
1. Manually change multiple parameters (no preset)
2. Run simulation for 100 ticks
3. Click **"Reset"**
   - ? All parameters reset to default preset
   - ? Tick counter = 0
   - ? Notification shows success

### **Test 3: Reset Multiple Times**
1. Click **"Reset"**
2. Wait for restart
3. Click **"Reset"** again
   - ? Should work every time
   - ? Always resets to Tick 0

---

## **?? Behavior Comparison**

| Button | Action | Tick Reset | Notification |
|--------|--------|------------|--------------|
| **Load Preset** | Loads preset into UI only | ? No | "? Preset loaded: [Name]" |
| **Apply** | Restarts sim with current UI config | ? No (continues from current tick) | "? Simulation restarted" |
| **Reset** | Loads default preset + restarts | ? **Yes (Tick 0)** | "? Reset to defaults (Tick 0)" |

---

## **?? Troubleshooting**

### **Reset button doesn't appear:**
- Check `BottomButtonRow` has **HorizontalLayoutGroup** component
- Verify `ResetButton` is a child of `BottomButtonRow`
- Check GameObject is **Active** (checkbox in Inspector)

### **Reset doesn't work:**
- Check Console for errors: `[ResetButton] ...`
- Verify `SimulationUIOrchestrator` is in scene
- Verify `SimulationController` is in scene
- Check that a default preset exists

### **Tick doesn't reset to 0:**
- This is expected if `RestartSimulation()` is called instead of `RestartWithScenario()`
- The `ResetButton` properly calls `ApplyAndRestart()` which should reset ticks
- If ticks persist, check `SimulationController.RestartWithScenario()` implementation

### **Notification doesn't show:**
- Check `NotificationUI` GameObject exists and is active
- Verify `NotificationUI` script is attached
- Check Canvas has `CanvasGroup` component

---

## **?? Code Overview**

### **ResetButton.cs Flow:**

```
OnReset() called
   ?
Load default preset into WorkingConfig
   ?
orchestrator.LoadPreset(defaultPreset)
   ?
orchestrator.ApplyAndRestart()
   ?
SimulationController.RestartWithScenario()
   ?
Tick counter = 0
   ?
Show notification: "? Reset to defaults (Tick 0)"
```

---

## **? Checklist**

- [ ] Created `BottomButtonRow` container with HorizontalLayoutGroup
- [ ] Created `ResetButton` with proper styling (red/orange)
- [ ] Added `ResetButton` script and wired references
- [ ] Moved `ApplyButton` into `BottomButtonRow` (if needed)
- [ ] Both buttons have LayoutElement with Min Width = 120
- [ ] Tested Reset button (UI reverts + Tick = 0)
- [ ] Tested notification appears on reset
- [ ] Verified default preset auto-loads if not assigned

---

## **?? Color Reference**

| Button | Normal | Highlighted | Pressed | Purpose |
|--------|--------|-------------|---------|---------|
| **Reset** | `#E74C3C` | `#EC7063` | `#C0392B` | Destructive (red/orange) |
| **Apply** | `#4CAF50` | `#66BB6A` | `#3C9F40` | Confirmation (green) |
| **Load Preset** | `#4A9FD8` | `#60B5EE` | `#3A8FC8` | Information (blue) |

---

**Implementation complete! The Reset button is now ready to use!** ??
