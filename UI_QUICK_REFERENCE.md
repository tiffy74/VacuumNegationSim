# ?? Quick Reference Card - UI Buttons

## **Button Overview**

| Button | Location | Color | Hotkey | Action |
|--------|----------|-------|--------|--------|
| **Load Preset** | Right Dock (Top) | Blue | - | Load preset into UI only |
| **Apply** | Right Dock (Bottom) | Green | - | Restart sim with current config |
| **Reset** | Right Dock (Bottom) | Red | - | Load default + Reset to Tick 0 |
| **Play/Pause** | Top Bar | Default | Space | Start/stop simulation |
| **Step** | Top Bar | Default | ? | Advance 1 tick |
| **Restart** | Top Bar | Default | - | Restart from Tick 0 (keep config) |
| **Export** | Top Bar | Default | - | Export current run data |

---

## **Button Behavior Quick Guide**

### **Load Preset (Blue)**
```
Action:  Select preset ? Click "Load Preset"
Effect:  ? UI updates with preset values
         ? Simulation NOT restarted
         ? Tick counter unchanged
Notice:  "? Preset loaded: [PresetName]"
Use:     Preview preset before applying
```

### **Apply (Green)**
```
Action:  Make changes ? Click "Apply"
Effect:  ? Simulation restarts
         ? All UI changes applied
         ? Tick counter NOT reset
Notice:  "? Simulation restarted"
Use:     Apply tweaks and restart sim
```

### **Reset (Red)**
```
Action:  Click "Reset"
Effect:  ? Default preset loaded
         ? Simulation restarts
         ? Tick counter = 0
Notice:  "? Reset to defaults (Tick 0)"
Use:     Start fresh from scratch
```

---

## **Common Workflows**

### **Workflow 1: Load & Run Preset**
```
1. Select preset from dropdown
2. [Load Preset] ? Preview in UI
3. [Apply] ? Run simulation
```

### **Workflow 2: Tweak & Test**
```
1. Change Grid Topology
2. Change Inflow mode
3. [Apply] ? Test changes
4. Not satisfied? ? [Reset]
```

### **Workflow 3: Preset + Custom Changes**
```
1. Select preset
2. [Load Preset]
3. Adjust specific parameters
4. [Apply] ? Run with hybrid config
```

### **Workflow 4: Quick Reset**
```
1. [Reset] ? Done!
   (Back to defaults, Tick 0)
```

---

## **Visual Cues**

### **Button Colors:**
- ?? **Blue** = Information (preview, no restart)
- ?? **Green** = Confirmation (apply and restart)
- ?? **Red** = Destructive (reset everything)

### **Notification Colors:**
- ?? **Green** = Success (action completed)
- ?? **Red** = Error (action failed)
- ?? **Orange** = Warning (caution needed)
- ?? **Blue** = Info (general message)

---

## **Troubleshooting Quick Fixes**

| Problem | Quick Fix |
|---------|-----------|
| **Notification doesn't show** | Check `NotificationUI` GameObject is Active |
| **Preset dropdown empty** | Check presets exist in `Presets/Examples/` folder |
| **Reset doesn't work** | Verify default preset is assigned/found |
| **Apply doesn't restart** | Check `SimulationController` is in scene |
| **Tick doesn't reset to 0** | Use `Reset` button (not `Restart`) |

---

## **When to Use Which Button**

### **Use "Load Preset" when:**
- ? You want to preview preset values
- ? You want to check what a preset contains
- ? You want to start from a preset but make changes first
- ? You're comparing different presets

### **Use "Apply" when:**
- ? You've made changes and want to test them
- ? You want to restart with current configuration
- ? You've loaded a preset and are ready to run
- ? You've tweaked parameters and want to see results

### **Use "Reset" when:**
- ? You want to start completely fresh
- ? You want to clear all tweaks
- ? You want to reset the tick counter to 0
- ? You've made a mess and want to go back to default
- ? You're testing multiple scenarios from the same baseline

### **Use "Restart" (Top Bar) when:**
- ? You want to restart current config from Tick 0
- ? You want to rerun the same simulation
- ? You don't want to change any parameters

---

## **Key Differences**

| Feature | Load Preset | Apply | Reset | Restart (Top Bar) |
|---------|-------------|-------|-------|-------------------|
| **Restarts Sim** | ? No | ? Yes | ? Yes | ? Yes |
| **Resets Tick** | ? No | ? No | ? **Yes** | ? Yes |
| **Loads Preset** | ? Yes | ? No | ? Default | ? No |
| **Clears Tweaks** | ?? Overwrites | ? No | ? Yes | ? No |
| **Updates UI** | ? Yes | ? No | ? Yes | ? No |

---

## **Remember:**

- ?? **Load** = Preview (no restart)
- ?? **Apply** = Restart with changes (Tick continues)
- ?? **Reset** = Back to defaults at Tick 0
- ? **Restart** = Rerun current config from Tick 0

---

**Print this card and keep it handy while using the UI!** ??
