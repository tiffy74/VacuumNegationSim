# Stage 12 Implementation - Essential UI System

**Status:** ?? **IN PROGRESS**  
**Goal:** Create basic UI to make framework usable without Unity Editor knowledge

---

## ?? **Stage 12 Goals**

Build minimal UI that enables users to:
1. Select and load presets without dragging in Inspector
2. Control simulation (Play/Pause/Stop) from runtime UI
3. Export results with visible button (not hidden context menu)
4. See live simulation info (tick, viable cells, etc.)
5. Optionally edit parameters (advanced users)

**Why:** Without UI, framework requires Unity Editor expertise. This makes it accessible to researchers.

---

## ?? **UI Components to Build**

### **Component 1: Preset Selector Panel** (1 hour)
**Location:** Top-left of screen

**Features:**
- Dropdown listing all presets in `Presets/Examples/`
- Preset description display
- "Load Preset" button
- Visual feedback when loaded

**Implementation:**
```csharp
PresetSelectorUI.cs
- Finds all ScenarioPreset assets
- Populates dropdown
- Loads selected preset into SimulationController
```

---

### **Component 2: Simulation Controls Panel** (1 hour)
**Location:** Top-center of screen

**Features:**
- Play button (??)
- Pause button (??)
- Stop/Restart button (??)
- Speed slider (1x - 10x)
- Current tick display

**Implementation:**
```csharp
SimulationControlsUI.cs
- Wires to SimulationController.Play()/Pause()
- Updates tick display each frame
- Controls simulation speed
```

---

### **Component 3: Info Display Panel** (30 min)
**Location:** Top-right of screen

**Features:**
- Current tick
- Viable cells count
- Active cells count
- Sink count
- Global resource level

**Implementation:**
```csharp
InfoDisplayUI.cs
- Reads state from SimulationController
- Updates every frame or every N ticks
- Simple text labels
```

---

### **Component 4: Export Panel** (30 min)
**Location:** Bottom-right of screen

**Features:**
- "Export Run" button (large, visible!)
- Shows last export path
- Success/error feedback

**Implementation:**
```csharp
ExportUI.cs
- Calls SimulationController.ExportLastRun()
- Displays result message
- Opens folder button (optional)
```

---

### **Component 5: Parameter Editor** (Optional - 1 hour)
**Location:** Bottom-left of screen (collapsible)

**Features:**
- Shows current parameter values
- Allows editing (text fields)
- "Apply" button
- "Reset to Preset" button

**Implementation:**
```csharp
ParameterEditorUI.cs
- Reads from current preset
- Allows runtime modification
- Updates SimulationConfiguration
```

---

## ??? **Implementation Plan**

### **Phase 1: UI Foundation** (30 min)
1. Create `UIManager.cs` - Central UI coordinator
2. Create Canvas with proper scaling
3. Setup UI panels (empty placeholders)
4. Wire to SimulationController

### **Phase 2: Core Controls** (1.5 hours)
1. Preset Selector UI
2. Simulation Controls UI
3. Test: Can load preset and start simulation from UI

### **Phase 3: Info & Export** (1 hour)
1. Info Display UI
2. Export Panel UI
3. Test: Can see live info and export with button

### **Phase 4: Polish** (1 hour)
1. Parameter Editor (optional)
2. Visual polish (colors, layout)
3. Keyboard shortcuts
4. Help text/tooltips

---

## ?? **UI Layout**

```
???????????????????????????????????????????????????
?  [Preset Selector ?]  ? [??] [??] [??] [Speed]  ? Tick: 0    ?
?  Description...       ?                        ? Viable: 0  ?
?                      ?                        ? Sinks: 0   ?
??????????????????????????????????????????????????????????????
?                                                            ?
?                   Simulation Grid View                     ?
?                                                            ?
?                                                            ?
??????????????????????????????????????????????????????????????
?  [Parameters ?]                              [Export Run] ?
?  (collapsible)                               Last: path   ?
??????????????????????????????????????????????????????????????
```

---

## ?? **Visual Design**

### **Style:**
- Semi-transparent black panels (alpha 0.8)
- White text for readability
- Blue accent for buttons
- Green for play, red for stop
- Minimal, functional, non-intrusive

### **Layout:**
- Canvas scaled with screen size
- Panels anchored to corners
- Responsive to resolution changes
- Doesn't obscure grid view

---

## ?? **File Structure**

```
Assets/Viable/Core.Unity/UI/
?? UIManager.cs                  ? Central coordinator
?? PresetSelectorUI.cs           ? Dropdown + load button
?? SimulationControlsUI.cs       ? Play/pause/stop controls
?? InfoDisplayUI.cs              ? Live metrics display
?? ExportUI.cs                   ? Export button + feedback
?? ParameterEditorUI.cs          ? Optional parameter tweaking
?? Prefabs/
   ?? UICanvas.prefab            ? Complete UI setup
```

---

## ? **Acceptance Criteria**

- [ ] User can select preset from dropdown (no Inspector dragging)
- [ ] User can start/pause/stop simulation with buttons
- [ ] User can see live tick count and metrics
- [ ] User can export with visible button (not hidden)
- [ ] UI doesn't require Unity Editor knowledge
- [ ] UI works in built standalone (not just editor)
- [ ] All existing tests still pass (25/25)

---

## ?? **Success Metrics**

**Stage 12 Complete When:**
1. ? Non-programmer can use the tool
2. ? All controls accessible from runtime UI
3. ? Export button visible and obvious
4. ? Preset switching works without Inspector
5. ? Build works standalone (not just in editor)

---

## ?? **Phased Rollout**

### **Minimal Viable UI** (2 hours)
- Preset selector
- Play/Pause buttons
- Export button
? Functional for basic use

### **Enhanced UI** (2 hours more)
- Info display
- Stop/Restart
- Speed control
? Better user experience

### **Full UI** (1 hour more)
- Parameter editor
- Keyboard shortcuts
- Polish
? Power user features

---

## ?? **Before vs After**

### **Before Stage 12:**
**To run simulation:**
1. Open Unity Editor
2. Find SimulationManager GameObject
3. Drag preset to Inspector slot
4. Press Unity Play button
5. Right-click component ? Export (hidden!)

**Requires:** Unity Editor expertise, Inspector knowledge

---

### **After Stage 12:**
**To run simulation:**
1. Launch application
2. Select preset from dropdown
3. Click Play button
4. Click Export button

**Requires:** Ability to click buttons

---

## ?? **Priority Order**

**Must Have:**
1. ? Preset selector
2. ? Play/Pause controls
3. ? Export button

**Should Have:**
4. Info display
5. Stop/Restart
6. Speed control

**Nice to Have:**
7. Parameter editor
8. Keyboard shortcuts
9. Tooltips

---

**Current Phase:** Phase 1 - UI Foundation

