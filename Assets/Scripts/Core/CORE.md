# Core Module

This module contains Unity MonoBehaviour controllers that manage simulation lifecycle, user interaction, and grid initialization.

## Overview

The Core module is the "glue" between Unity's component system and the pure simulation engine. It handles initialization, user input, coroutine-based simulation loops, and inspector parameter exposure.

## Files

### SimulationController.cs

**Purpose**: Main controller that orchestrates the entire simulation.

**Key Responsibilities**:
- Initializes grid, simulation state, and engine
- Manages simulation loop (play/pause/restart)
- Exposes all parameters via Unity Inspector
- Bridges Unity lifecycle with simulation ticks
- Provides viability/persistence computation delegates
- Handles diagnostics and logging

**See**: `SimulationController.cs` is extensively documented with inline comments. Key sections:

##### Initialization (Start)
1. `InitializeGridAndVisuals()`: Spawns grid cells
2. `InitializeSimulationState()`: Creates GridState, SimContext
3. `InitializeSimulationEngine()`: Creates SimulationEngine with LegacyTickStep
4. `InitializeRenderer()`: Creates GridRenderer

##### Simulation Loop
```CSharp
IEnumerator SimLoop() { var delay = new WaitForSeconds(1f / ticksPerSecond); while (running) { TickSimulation(); yield return delay; } }
```
- Coroutine-based for framerate independence
- Configurable ticks-per-second
- Can pause/resume/single-step

##### Tick Execution
```CSharp
void TickSimulation() { engine.Tick(ctx);  // Executes all passes LogDiagnostics();  // Debug output UpdateVisualsFromState(state);  // Render }
```

##### Inspector Parameters
- **80+ parameters** organized into sections:
  - Global Budget, Viability, Propagation, Entropy, Field, Black Holes, Colors
- **Tooltips** for each parameter
- **SerializeField** for runtime tweaking

##### Key Methods
- `ComputeViability(inflow, N, S)`: Viability formula with entropy boost
- `CountPersistenceConfigurations(cellIdx)`: Tests all 16 neighbor configurations
- `RestartSimulation()`: Resets state to initial conditions
- `Play/Pause/Step()`: Playback controls

**Design**: Fat Controller Pattern
- Owns all initialization logic
- Coordinates between engine, renderer, and Unity
- Could be refactored to thinner controller + services

---

### SimulationGrid.cs

**Purpose**: Manages grid dimensions and cell GameObject spawning.

**Key Responsibilities**:
- Defines grid width/height (Inspector parameters)
- Spawns Cell GameObjects in grid layout
- Attaches CellVisualiser components

**Key Methods**:

##### `void SpawnVisualCells(CellVisualiser[,] views)`
- **Purpose**: Instantiates all grid cell GameObjects
- **Logic**:
  1. Nested loop (x, y) from (0, 0) to (Width-1, Height-1)
  2. Instantiate Cell prefab at position (x, y, 0)
  3. Add CellVisualiser component
  4. Store reference in views[x, y] array
  5. Parent to SimulationGrid GameObject

**Inspector Parameters**:
- `int Width = 100`: Grid columns
- `int Height = 100`: Grid rows
- `GameObject CellPrefab`: Prefab for individual cells (requires SpriteRenderer)

**Design**: Simple factory pattern for cell creation

---

### Cell.cs

**Purpose**: Empty marker component for cell GameObjects.

**Structure**:
```CSharp
public class Cell : MonoBehaviour
{ 
	// Currently empty - placeholder for future cell-specific logic
}
```


**Purpose**: 
- Identifies GameObjects as cells
- Could hold cell-specific state in future (e.g., selected state for UI)
- Could implement cell-level interactions (e.g., click handlers)

**Design**: Marker pattern (empty component for tagging)

---

### SimulationUIController.cs

**Purpose**: Manages UI buttons for simulation control.

**Key Responsibilities**:
- Exposes Play/Pause/Restart/Exit buttons
- Calls SimulationController methods

**Key Methods**:

##### `void OnPlayButtonPressed()`
- Calls `simulationController.Play()`
- Resumes simulation loop

##### `void OnPauseButtonPressed()`
- Calls `simulationController.Pause()`
- Halts simulation loop (coroutine stops)

##### `void OnRestartButtonPressed()`
- Calls `simulationController.RestartSimulation()`
- Resets state to initial seed

##### `void OnExitButtonPressed()`
- Quits application
- **Editor**: Stops play mode
- **Build**: `Application.Quit()`

**Inspector Parameters**:
- `SimulationController simulationController`: Reference to main controller
- UI Button components (wired via Unity Inspector events)

**Design**: Simple mediator between UI and controller

## Integration Points

### Dependencies
- **Domain**: `GridState`, `SimContext`, `SimConfig`
- **Simulation**: `SimulationEngine`, `LegacyTickStep`, `ISimStep`
- **Events**: All passes, `BlackHoles`, `FieldWave`
- **Unity**: `GridRenderer`, `CellVisualiser`
- **Visuals**: `CellVisualiser`

### Unity Lifecycle Integration
Awake/Start (SimulationGrid) ? Start (SimulationController) ? Initialize Engine, State, Renderer ? StartCoroutine(SimLoop) ? Update (SimulationController) ? Handle input (render mode switching) ? SimLoop Coroutine ? TickSimulation() ? engine.Tick(ctx) ? UpdateVisuals() ? yield WaitForSeconds(1/fps) ? loop


### Communication Flow

User Input (Keyboard/UI) ? SimulationUIController / SimulationController.Update() ? SimulationController (orchestrates) ? SimulationEngine.Tick() ? LegacyTickStep.Execute() ? Pass1 ? Pass2 ? Pass3 ? Pass4 ? PostProcessing ? GridRenderer.Render() ? CellVisualiser.SetColor/SetViability() ? Unity Rendering System


## Performance Considerations

### Per-Frame Operations
- **Simulation Tick**: O(8N) where N = width × height
- **Rendering**: O(N) color updates
- **Diagnostics**: O(N) stats computation (only if logging enabled)

### Coroutine Overhead
- Minimal: single `WaitForSeconds` allocation per tick
- Amortized over simulation duration

### Inspector Parameters
- Stored in controller (MonoBehaviour serialization)
- Copied to SimConfig once at Start()
- Runtime changes require restart

## Testing Strategy

### Integration Testing
```CSharp
[Test] public void SimulationController_Restart_ResetsState() { var controller = GetComponent<SimulationController>(); controller.state.Nlocal[0] = 100f;
controller.RestartSimulation();

Assert.AreEqual(0f, controller.state.Nlocal[0]);
}
```

### Manual Testing Checklist
- [ ] Play button starts simulation
- [ ] Pause button stops simulation
- [ ] Restart button resets to initial seed
- [ ] Render mode switching works (1/2/3 keys)
- [ ] Parameters can be tweaked in Inspector
- [ ] Simulation runs at configured FPS

## Code Quality Notes

- **Fat Controller**: SimulationController owns too much logic (refactor candidate)
- **Inspector Overload**: 80+ parameters overwhelming (could use ScriptableObject)
- **Logging**: Extensive diagnostic logging (performance cost in builds)
- **Defensive Programming**: Null checks, graceful fallbacks
- **Clear Separation**: Core coordinates, doesn't implement algorithms

## Future Refactoring Opportunities

1. **Thin Controller**: Extract initialization into `SimulationBootstrapper`
2. **Parameter Object**: Move inspector params to `SimulationParameters` ScriptableObject
3. **Service Locator**: Extract renderer, engine as services
4. **Event System**: Use UnityEvents for Play/Pause/Restart instead of direct calls
5. **Dependency Injection**: Use Zenject/VContainer for cleaner dependency management