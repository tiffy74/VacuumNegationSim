# Scripts Directory

**Path:** `Assets/Scripts/`  
**Purpose:** Root directory containing all C# source code for the Vacuum Negation Simulation

---

## ?? Directory Overview

The `Scripts/` directory is organized into logical subdirectories representing architectural layers of the simulation. This structure enforces separation of concerns and maintains clean dependencies between layers.

Assets/Scripts/
??? Core/           - Unity MonoBehaviour controllers and legacy types
??? Domain/         - Pure data structures (simulation state)
??? Simulation/     - Simulation engine and tick orchestration
??? Events/         - Pure simulation logic (4-pass system + extensions)
??? Unity/          - Unity-specific utilities (rendering, enums)
??? Visuals/        - Visual representation components


---

## ??? Architecture Layers

### **Layer 1: Core (Unity Entry Points)**
- **Directory:** `Core/`
- **Purpose:** Unity MonoBehaviour controllers that bridge Unity lifecycle with simulation engine
- **Dependencies:** All other layers
- **Key Files:** `SimulationController.cs`, `SimulationGrid.cs`, `SimulationUIController.cs`
- **Documentation:** [CORE.md](Core/CORE.md)

### **Layer 2: Domain (Data Structures)**
- **Directory:** `Domain/`
- **Purpose:** Pure data types for simulation state and configuration (no Unity dependencies)
- **Dependencies:** None
- **Key Files:** `GridState.cs`, `SimContext.cs`, `SimConfig.cs`
- **Documentation:** [DOMAIN.md](Domain/DOMAIN.md)

### **Layer 3: Simulation (Engine)**
- **Directory:** `Simulation/`
- **Purpose:** Tick orchestration and simulation engine infrastructure
- **Dependencies:** Domain
- **Key Files:** `SimulationEngine.cs`, `ISimStep.cs`, `LegacyTickStep.cs`
- **Documentation:** [SIMULATION.md](Simulation/SIMULATION.md)

### **Layer 4: Events (Simulation Logic)**
- **Directory:** `Events/`
- **Purpose:** Pure simulation algorithms (4-pass viability system + extensions)
- **Dependencies:** Domain
- **Key Files:** `Pass1.cs`, `Pass2.cs`, `Pass3.cs`, `Pass4.cs`, `BlackHoles.cs`, `FieldWave.cs`
- **Documentation:** [EVENTS.md](Events/EVENTS.md)

### **Layer 5: Unity (Utilities)**
- **Directory:** `Unity/`
- **Purpose:** Unity-specific helper types (rendering, enums, utilities)
- **Dependencies:** Domain, Visuals
- **Key Files:** `GridRenderer.cs`, `RenderMode.cs`
- **Documentation:** [UNITY.md](Unity/UNITY.md)

### **Layer 6: Visuals (Presentation)**
- **Directory:** `Visuals/`
- **Purpose:** Visual representation components for grid cells and camera control
- **Dependencies:** None (Unity only)
- **Key Files:** `CellVisualiser.cs`, `CameraController.cs`
- **Documentation:** [VISUALS.md](Visuals/VISUALS.md)

---

## ?? Dependency Graph
```plaintext
                ???????????????????????????????
                ?         Core/               ?
                ?  (Unity Controllers)        ?
                ?  • SimulationController     ?
                ?  • SimulationGrid           ?
                ?  • SimulationUIController   ?
                ???????????????????????????????
                               ?
                               ? depends on
                               ?
                ??????????????????????????????
                ?                            ?
     ???????????????????????      ???????????????????????
     ?   Simulation/       ?      ?     Unity/          ?
     ?   (Engine)          ?      ?   (Utilities)       ?
     ? • SimulationEngine  ?      ? • GridRenderer      ?
     ? • LegacyTickStep    ?      ? • RenderMode        ?
     ???????????????????????      ???????????????????????
                ?                            ?
                ? depends on                 ? depends on
                ?                            ?
     ???????????????????????      ???????????????????????
     ?    Events/          ?      ?    Visuals/         ?
     ?    (Logic)          ?      ?  (Presentation)     ?
     ?  • Pass1-4          ?      ? • CellVisualiser    ?
     ?  • BlackHoles       ?      ? • CameraController  ?
     ?  • FieldWave        ?      ?                     ?
     ???????????????????????      ???????????????????????
                ?
                ? depends on
                ?
     ????????????????????????????????????????
     ?           Domain/                    ?
     ?    (Pure Data - No Dependencies)     ?
     ?        • GridState                   ?
     ?        • SimContext                  ?
     ?        • SimConfig                   ?
     ????????????????????????????????????????

```


## ?? Data Flow

### **Initialization (Startup)**
1. SimulationController.Start()
   ??> Initialize GridState (Domain)
   ??> Initialize SimContext (Domain)
   ??> Create SimulationEngine (Simulation)
   ??> Spawn Visual Cells via SimulationGrid (Core)
   ??> Initialize GridRenderer (Unity)

2. SimulationGrid.SpawnVisualCells()
   ??> Instantiate cell GameObjects
   ??> Store CellVisualiser references

3. CameraController.Awake()
   ??> Center camera on grid

### **User Interaction**
SimulationUIController (Core)
   ??> Play/Pause/Restart buttons
   ??> SimulationController.Play() / Pause() / RestartSimulation()


## ?? File Naming Conventions

### **Controllers (MonoBehaviour)**
- Pattern: `{Feature}Controller.cs`
- Examples: `SimulationController.cs`, `SimulationUIController.cs`
- Location: `Core/`

### **Data Structures**
- Pattern: `{Concept}.cs` (singular noun)
- Examples: `GridState.cs`, `SimContext.cs`, `SimConfig.cs`
- Location: `Domain/`

### **Engine Components**
- Pattern: `{Feature}Engine.cs` or `{Feature}Step.cs`
- Examples: `SimulationEngine.cs`, `LegacyTickStep.cs`
- Location: `Simulation/`

### **Simulation Algorithms**
- Pattern: `Pass{N}.cs` for main passes, `{Feature}.cs` for extensions
- Examples: `Pass1.cs`, `BlackHoles.cs`, `FieldWave.cs`
- Location: `Events/`

### **Visual Components**
- Pattern: `{Feature}Visualiser.cs` or `{Feature}Controller.cs`
- Examples: `CellVisualiser.cs`, `CameraController.cs`
- Location: `Visuals/`

### **Utilities**
- Pattern: `{Feature}Renderer.cs` or `{Concept}.cs`
- Examples: `GridRenderer.cs`, `RenderMode.cs`
- Location: `Unity/`

---

## ?? Code Quality Standards

### **Documentation Requirements**
- ? All public classes have XML summary documentation
- ? All public methods have XML parameter/return documentation
- ? Banner comments separate logical sections
- ? Complex algorithms have inline explanatory comments

### **Architecture Principles**
- ? **Single Responsibility**: Each class has one clear purpose
- ? **Dependency Inversion**: High-level modules don't depend on low-level details
- ? **Pure Functions**: Simulation logic is stateless (operates on GridState)
- ? **Unity Isolation**: Unity dependencies isolated to `Core/` and `Visuals/`

### **Performance Considerations**
- ? Hot paths use flat arrays instead of 2D arrays
- ? Minimal allocations in per-tick logic (no `new` in tight loops)
- ? Cached component references (no repeated `GetComponent` calls)
- ? Early exits for null/disabled states

### **Code Style**
- ? C# 9.0 language features (.NET Framework 4.7.1 compatible)
- ? 4-space indentation (Unity standard)
- ? Banner comments use `// ====` format
- ? Namespaces match directory structure (`Assets.Scripts.{Folder}`)

---

## ?? Migration Status

### **Completed ?**
- Migration from `Cell[,]` to `GridState` flat arrays
- Separation of simulation logic from Unity lifecycle
- 4-pass system refactored into pure `Events/` layer
- Documentation coverage for all major components

### **In Progress ??**
- Removal of deprecated `Cell.cs` type (still referenced but unused)
- Statistics display refactoring in `SimulationUIController`
- Parameter input system (currently disabled)

### **Planned ??**
- Unit tests for simulation logic (`Events/` layer)
- Performance profiling for large grids (500×500+)
- Runtime parameter tuning UI
- Save/load system for simulation state

---

## ?? Related Documentation

- [Root README](../../README.md) - Project overview and setup instructions
- [ASSETS.md](../ASSETS.md) - Assets directory structure
- [CORE.md](Core/CORE.md) - Unity controllers and entry points
- [DOMAIN.md](Domain/DOMAIN.md) - Data structures and state management
- [SIMULATION.md](Simulation/SIMULATION.md) - Engine architecture
- [EVENTS.md](Events/EVENTS.md) - Simulation algorithms (4-pass system)
- [UNITY.md](Unity/UNITY.md) - Unity utilities and rendering
- [VISUALS.md](Visuals/VISUALS.md) - Visual representation components

---

## ?? Key Entry Points for Developers

### **"Where do I start?"**
1. **Understanding Simulation Logic:** Start with [EVENTS.md](Events/EVENTS.md) and read `Pass1-4.cs`
2. **Modifying Parameters:** Edit inspector fields in `SimulationController.cs` or `SimConfig.cs`
3. **Changing Visuals:** Modify `GridRenderer.cs` or `CellVisualiser.cs`
4. **Adding New Features:** Follow the layer structure - logic goes in `Events/`, data in `Domain/`

### **"How do I debug?"**
- **Simulation State:** Check `SimulationController.LogTickSummary()` console output
- **Visual Rendering:** Inspect `GridRenderer.Render()` color calculations
- **Performance:** Use Unity Profiler on `SimulationEngine.Tick()`
- **Grid State:** Add breakpoints in `Pass1-4.cs` and inspect `GridState` arrays

### **"How do I extend the simulation?"**
1. Add new fields to `GridState.cs` (e.g., `public float[] NewField`)
2. Add parameters to `SimConfig.cs` (e.g., `public float NewParameter`)
3. Create new algorithm file in `Events/` (e.g., `NewFeature.cs`)
4. Call from `LegacyTickStep.cs` or create new `ISimStep` implementation
5. Update visualization in `GridRenderer.cs` if needed

---

**Last Updated:** January 2026  
**Target .NET:** Framework 4.7.1  
**Unity Version:** 2021.3+ (LTS)  
**C# Version:** 9.0