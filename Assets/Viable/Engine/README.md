# Viable.Engine

**Purpose:** Headless, Unity-independent simulation engine core.

## Design Principles
- **Unity-Free:** No references to UnityEngine (use System.Numerics or custom types)
- **Deterministic:** Same seed + same scenario = same results
- **Stateless:** Engine classes are pure functions where possible
- **Testable:** All logic can be unit tested without Unity

## Architecture
```
IModel<TState>           - Defines state transitions
IStepper<TState>         - Manages step-by-step execution
SimulationRunner         - Orchestrates full runs
ViabilityCalculator      - Core viability computation
StepContext              - Input context for each step
StepResult<TState>       - Output from each step
```

## What Belongs Here
- State update logic
- Viability calculations
- Configuration validation
- Event generation
- Metric computation
- Deterministic random number usage

## What Does NOT Belong Here
- Unity MonoBehaviours
- Visual rendering logic
- User input handling
- Scene management
- Inspector field serialization

## Migration Strategy
1. Extract pure computational logic from existing scripts
2. Replace Unity types (Mathf ? Math, Vector3 ? System.Numerics.Vector3)
3. Pass time delta explicitly (no Time.deltaTime)
4. Use System.Random with explicit seed (no UnityEngine.Random)

## Current Simulation "Heart" Candidates for Migration
From existing codebase:
- `StateGrid.cs` ? `GridState.cs` (pure state container)
- `SimConfig.cs` ? `ModelConfiguration.cs` (parameters)
- `Pass1-4.cs` ? `StepPhases.cs` (state update logic)
- `ViabilityCalculator` ? Already extracted method in SimulationController
- `SinkRegions.cs` ? `SinkLogic.cs` (pure algorithms)
- `RegionExpansion.cs` ? `RegionLogic.cs` (pure algorithms)
