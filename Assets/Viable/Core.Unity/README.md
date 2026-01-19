# Viable.Core.Unity

**Purpose:** Unity presentation layer - MonoBehaviours, UI, visualization, and scene management.

## Design Principles
- **Thin Layer:** Minimal logic, mostly orchestration and display
- **Adapter Pattern:** Convert between Unity types and Engine contracts
- **Reactive:** Respond to engine state changes for visualization
- **Scene-Driven:** Configure scenarios via Unity Inspector/ScriptableObjects

## Folder Structure
```
/Adapters/           - Unity ? Engine conversion logic
/UI/                 - Canvas UI, buttons, panels, input fields
/Visualization/      - Grid rendering, cell colors, camera control
/Controllers/        - MonoBehaviours that orchestrate engine calls
/ScriptableObjects/  - Scenario presets, configuration assets
```

## What Belongs Here
- MonoBehaviours (SimulationController, CameraController, etc.)
- UI input/output (SimulationUIController)
- Visual rendering (GridRenderer, CellVisualiser)
- Unity-specific utilities (coroutines, Inspector attributes)
- Scene setup and lifecycle management

## Adapter Layer Responsibilities
- `UnityScenarioAdapter` - Build ScenarioDefinition from Unity scene/ScriptableObject
- `UnityRunnerController` - Drive SimulationRunner from Update loop
- `StateVisualizer` - Map engine state to Unity visuals
- `UnityTypeConverter` - Convert Math ? Mathf, System.Numerics ? UnityEngine

## Current Scripts to Keep Here (After Refactor)
- `SimulationController.cs` ? Becomes thin orchestrator
- `SimulationGrid.cs` ? Visual grid spawning only
- `CellVisualiser.cs` ? Sprite rendering
- `GridRenderer.cs` ? Color mapping and display
- `CameraController.cs` ? Camera positioning
- `SimulationUIController.cs` ? UI input/output

## Current Scripts to Migrate OUT
- Core logic from `SimulationController` ? Move to Engine
- State management from `Cell.cs` ? Superseded by Engine state
