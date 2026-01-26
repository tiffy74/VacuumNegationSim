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

---

## Stage 13.9: Adaptive Refinement (Future Feature - Stubs Only)

### Concept

**Hierarchical Refinement:** When a cell exhibits high disorder (steep gradients, heterogeneity), subdivide it into a finer subgrid for better resolution.

**Key Design Decisions:**
- **Hierarchical:** Cell contains subgrid, not global grid rewriting
- **Bounded Growth:** Max depth prevents infinite recursion
- **Trigger-based:** Disorder index threshold determines when to refine

### Intended Behavior

```
Base Grid (Level 0):
?????????????
? A ? B ? C ?  B has high disorder ? refine B
?????????????
? D ? E ? F ?
?????????????

After Refinement (B ? 2×2 subgrid):
?????????????????
? A ? b? b? ? C ?  B now contains 4 subcells: b?, b?, b?, b?
?   ?????????   ?
?   ? b?? b??   ?
?????????????????
? D ?   E   ? F ?
?????????????????
```

### Configuration Parameters

**EngineConfig Fields:**
- `RefinementMode`: None (default) | ThresholdRefinement
- `RefinementDisorderThreshold`: Trigger value (default: 0.0 = no refinement)
- `MaxRefinementDepth`: Maximum hierarchy levels (default: 0)
- `MaxSubgridSize`: Maximum subcells per refined region (default: 0)
- `BaseSubcells`: Initial subgrid size (default: 2 ? 2×2)
- `AlphaSubcellsPerLineage`: Growth rate per level (default: 0.0 = constant)

### Event Type: "RefinementCreated"

**Emitted When:** Cell is refined into subgrid

**Data Payload:**
```csharp
{
    "parentId": int,          // ID of parent cell
    "x": int,                 // Parent X coordinate
    "y": int,                 // Parent Y coordinate
    "disorderIndex": double,  // Disorder that triggered refinement
    "level": int,             // Depth in hierarchy (0 = base)
    "subgridSide": int,       // Subgrid size (e.g., 2, 4, 8)
    "lineageDepth": int       // Lineage depth (for growth calculations)
}
```

### Implementation Status (Stage 13.9)

**? Complete (Stubs Only):**
- Configuration parameters added to `EngineConfig`
- Event type documented in `SimulationEvent`
- `DisorderIndexCalculator` stub created (always returns 0.0)
- Documentation complete

**? Not Implemented Yet:**
- Actual disorder index computation
- Refinement trigger logic
- Subgrid creation and management
- Hierarchical state evolution

**?? No Behavior Change:**
- Default `RefinementMode = None`
- All refinement parameters default to 0
- No refinement occurs in any simulation
- Backward compatible with all existing presets

### Future Implementation Notes

**When implementing functional refinement:**

1. **State Management:** Track subgrid hierarchy (tree structure), map subcell IDs to parents
2. **Upscaling/Downscaling:** Distribute parent state to subcells, aggregate subcells to parent
3. **Neighbor Queries:** Subcells at boundary interact with adjacent cells/subcells
4. **Performance:** Spatial hash for subcell lookup, cache subgrid metadata

---

**Engine Status:** ? Production Ready (Stage 13.9 stubs complete)
