# Stage 12 - Unity Setup Instructions

**Status:** ?? **SETUP REQUIRED**  
**Goal:** Create UI in Unity and wire to scripts

---

## ? **Scripts Created**

All C# scripts have been created:
1. ? `UIManager.cs` - Central coordinator
2. ? `PresetSelectorUI.cs` - Preset selector
3. ? `SimulationControlsUI.cs` - Play/pause controls
4. ? `InfoDisplayUI.cs` - Live metrics
5. ? `ExportUI.cs` - Export button
6. ? `ParameterEditorUI.cs` - Optional parameter editor

---

## ?? **Required SimulationController Changes**

The UI scripts need these public methods added to `SimulationController.cs`:

```csharp
// Add to SimulationController.cs:

/// <summary>
/// Get current simulation tick.
/// </summary>
public int GetCurrentTick()
{
    return context != null ? context.Tick : 0;
}

/// <summary>
/// Get current grid state (for UI metrics).
/// </summary>
public GridState GetCurrentState()
{
    return state;
}

/// <summary>
/// Get current step context.
/// </summary>
public StepContext GetCurrentContext()
{
    return context;
}

/// <summary>
/// Load a preset at runtime and restart simulation.
/// </summary>
public void LoadPreset(ScenarioPreset preset)
{
    if (preset == null)
    {
        Debug.LogWarning("[SimulationController] Cannot load null preset");
        return;
    }

    scenarioPreset = preset;
    Debug.Log($"[SimulationController] Loading preset: {preset.PresetName}");

    // Pause current simulation
    Pause();

    // Reinitialize with new preset
    InitializeSimulation();

    Debug.Log($"[SimulationController] Preset loaded: {preset.PresetName}");
}

/// <summary>
/// Export and return the export path.
/// </summary>
public string ExportLastRunWithPath()
{
    return ExportLastRunWithPath(RunExportOptions.ForLevel(ExportLevel.Publication));
}

/// <summary>
/// Export with options and return path.
/// </summary>
public string ExportLastRunWithPath(RunExportOptions options)
{
    if (lastScenario == null || lastResult == null)
    {
        Debug.LogWarning("[SimulationController] No run to export");
        return null;
    }

    try
    {
        var exporter = new RunExporter();
        string exportPath = exporter.Export(lastScenario, lastRequest, lastResult, options);
        Debug.Log($"[SimulationController] ? Export complete: {exportPath}");
        return exportPath;
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"[SimulationController] ? Export failed: {ex.Message}");
        return null;
    }
}
```

---

## ?? **Unity UI Setup**

### **Step 1: Create Canvas** (5 minutes)

1. **In Unity Hierarchy:**
   - Right-click ? UI ? Canvas
   - Name: `UICanvas`

2. **Canvas Settings:**
   - **Canvas Scaler:**
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920×1080
     - Match: 0.5 (Width/Height)

3. **Add EventSystem** (auto-created, but verify it exists)

---

### **Step 2: Create Preset Selector Panel** (10 minutes)

1. **Create Panel:**
   - Right-click UICanvas ? UI ? Panel
   - Name: `PresetSelectorPanel`
   - **Rect Transform:**
     - Anchor: Top-Left
     - Position: (10, -10, 0)
     - Width: 350, Height: 200

2. **Add Dropdown:**
   - Right-click PresetSelectorPanel ? UI ? Dropdown - TextMeshPro
   - Name: `PresetDropdown`
   - Position: top of panel

3. **Add Description Text:**
   - Right-click PresetSelectorPanel ? UI ? Text - TextMeshPro
   - Name: `DescriptionText`
   - Position: middle of panel
   - Enable Rich Text, Word Wrapping

4. **Add Load Button:**
   - Right-click PresetSelectorPanel ? UI ? Button - TextMeshPro
   - Name: `LoadButton`
   - Text: "Load Preset"
   - Position: bottom of panel

5. **Add Feedback Text:**
   - Right-click PresetSelectorPanel ? UI ? Text - TextMeshPro
   - Name: `FeedbackText`
   - Position: below button
   - Small font, initially empty

6. **Add PresetSelectorUI Component:**
   - Select PresetSelectorPanel
   - Add Component ? PresetSelectorUI
   - Drag UI elements to script fields

---

### **Step 3: Create Simulation Controls Panel** (10 minutes)

1. **Create Panel:**
   - Right-click UICanvas ? UI ? Panel
   - Name: `SimulationControlsPanel`
   - **Rect Transform:**
     - Anchor: Top-Center
     - Position: (0, -10, 0)
     - Width: 400, Height: 100

2. **Add Play Button:**
   - Add Button, Name: `PlayButton`
   - Text: "?" (or "Play")

3. **Add Pause Button:**
   - Add Button, Name: `PauseButton`
   - Text: "?" (or "Pause")

4. **Add Stop Button:**
   - Add Button, Name: `StopButton`
   - Text: "?" (or "Stop")

5. **Add Speed Slider:**
   - Add Slider, Name: `SpeedSlider`
   - Min: 1, Max: 10, Value: 1

6. **Add Speed Text:**
   - Add Text, Name: `SpeedText`
   - Text: "1.0x"

7. **Add Tick Text:**
   - Add Text, Name: `TickText`
   - Text: "Tick: 0"

8. **Add SimulationControlsUI Component:**
   - Wire all elements to script

---

### **Step 4: Create Info Display Panel** (5 minutes)

1. **Create Panel:**
   - Right-click UICanvas ? UI ? Panel
   - Name: `InfoDisplayPanel`
   - **Rect Transform:**
     - Anchor: Top-Right
     - Position: (-10, -10, 0)
     - Width: 250, Height: 150

2. **Add Text Labels:**
   - `TickText` - "Tick: 0"
   - `ViableCountText` - "Viable: 0"
   - `ActiveCountText` - "Active: 0"
   - `SinkCountText` - "Sinks: 0"
   - `ResourceGlobalText` - "Resource: 0"

3. **Add InfoDisplayUI Component:**
   - Wire text elements

---

### **Step 5: Create Export Panel** (5 minutes)

1. **Create Panel:**
   - Right-click UICanvas ? UI ? Panel
   - Name: `ExportPanel`
   - **Rect Transform:**
     - Anchor: Bottom-Right
     - Position: (-10, 10, 0)
     - Width: 300, Height: 100

2. **Add Export Button:**
   - Add Button, Name: `ExportButton`
   - Text: "Export Run" (large, prominent!)

3. **Add Path Text:**
   - Add Text, Name: `ExportPathText`
   - Text: "No exports yet"
   - Small font

4. **Add Feedback Text:**
   - Add Text, Name: `FeedbackText`
   - Initially empty

5. **Add Open Folder Button (Optional):**
   - Add Button, Name: `OpenFolderButton`
   - Text: "Open Folder"
   - Initially hidden

6. **Add ExportUI Component:**
   - Wire elements

---

### **Step 6: Create UIManager GameObject** (2 minutes)

1. **Create Empty GameObject:**
   - Hierarchy ? Create Empty
   - Name: `UIManager`

2. **Add UIManager Component:**
   - Add Component ? UIManager

3. **Wire References:**
   - Drag SimulationManager to Simulation Controller field
   - Drag PresetSelectorPanel to Preset Selector field
   - Drag SimulationControlsPanel to Simulation Controls field
   - Drag InfoDisplayPanel to Info Display field
   - Drag ExportPanel to Export UI field

---

## ?? **Resources Folder Setup**

**Critical:** Presets must be in a `Resources` folder to load at runtime!

1. **Create Folder:**
   - `Assets/Resources/Presets/Examples/`

2. **Copy Presets:**
   - Copy all 5 presets from `Assets/Viable/Core.Unity/Presets/Examples/`
   - To: `Assets/Resources/Presets/Examples/`

3. **Verify:**
   - Presets should now be loadable via `Resources.Load<ScenarioPreset>()`

---

## ? **Testing Checklist**

After setup:

- [ ] Press Play in Unity
- [ ] UI panels visible
- [ ] Preset dropdown populated
- [ ] Play button works
- [ ] Pause button works
- [ ] Stop button works
- [ ] Speed slider works
- [ ] Tick counter updates
- [ ] Export button works
- [ ] No console errors

---

## ?? **Visual Polish (Optional)**

### **Colors:**
- Panel background: Semi-transparent black (alpha 0.8)
- Text: White
- Buttons: Blue tint
- Play button: Green tint
- Stop button: Red tint

### **Fonts:**
- Title text: Bold, 16pt
- Body text: Regular, 12pt
- Button text: Bold, 14pt

---

## ?? **Known Issues to Fix**

1. **InfoDisplayUI shows [N/A]**
   - Fix: Add getter methods to SimulationController (see above)

2. **PresetSelectorUI can't load presets**
   - Fix: Add LoadPreset() method to SimulationController

3. **ExportUI doesn't show path**
   - Fix: Make Export return path

4. **Tick counter doesn't update**
   - Fix: Add GetCurrentTick() to SimulationController

---

## ?? **After Setup**

Once Unity setup is complete:
1. Test all UI functions
2. Fix any issues
3. Commit Stage 12
4. Framework is now user-friendly!

---

**Estimated Time:** ~45 minutes for full UI setup in Unity

