# Engine Extraction Plan - File-by-File Migration

## Overview
This document identifies which existing files contain the simulation "heart" and should be migrated to `Viable.Engine`, versus which should remain in `Viable.Core.Unity` as presentation layer.

---

## ? MIGRATE TO VIABLE.ENGINE (Pure Logic)

### High Priority (Core State & Logic)

#### 1. **StateGrid.cs** ? `Engine/State/GridState.cs`
**Current Location:** `Assets/Scripts/Domain/Gridstate.cs`  
**Reason:** Pure state container with no Unity dependencies  
**Changes Required:**
- Already Unity-free ?
- Rename namespace to `Viable.Engine.State`
- No code changes needed

#### 2. **SimConfig.cs** ? `Engine/Configuration/SimulationConfiguration.cs`
**Current Location:** `Assets/Scripts/Domain/SimConfig.cs`  
**Reason:** Parameter container, Unity-free  
**Changes Required:**
- Remove Unity `Color` types ? Use RGB tuples or separate color config
- Rename to `SimulationConfiguration`
- Keep all numeric parameters

#### 3. **SimContext.cs** ? `Engine/Execution/StepContext.cs`
**Current Location:** `Assets/Scripts/Domain/SimContext.cs`  
**Reason:** Runtime context for stepping  
**Changes Required:**
- Rename to `StepContext`
- Add `Random` field for deterministic RNG
- Add `DeltaTime` field (instead of Unity Time.deltaTime)

#### 4. **Pass1.cs** ? `Engine/Steps/OutflowPhase.cs`
**Current Location:** `Assets/Scripts/Events/Pass1.cs`  
**Reason:** Pure computational logic, no Unity types  
**Changes Required:**
- Already Unity-free except `UnityEngine.Random` ? Use `System.Random`
- Rename to `OutflowPhase`

#### 5. **Pass2.cs** ? `Engine/Steps/InflowPhase.cs`
**Current Location:** `Assets/Scripts/Events/Pass2.cs`  
**Reason:** Pure computational logic  
**Changes Required:**
- Replace `UnityEngine.Random` ? `System.Random`
- Rename to `InflowPhase`

#### 6. **Pass3.cs** ? `Engine/Steps/RechargePhase.cs`
**Current Location:** `Assets/Scripts/Events/Pass3.cs`  
**Reason:** Trivial recharge logic, already Unity-free  
**Changes Required:**
- Rename to `RechargePhase`

#### 7. **Pass4.cs** ? `Engine/Steps/DiffusionPhase.cs`
**Current Location:** `Assets/Scripts/Events/Pass4.cs`  
**Reason:** Diffusion math, no Unity dependencies  
**Changes Required:**
- Replace `Mathf` ? `Math` / `MathF`
- Rename to `DiffusionPhase`

#### 8. **SinkRegions.cs** ? `Engine/Logic/SinkLogic.cs`
**Current Location:** `Assets/Scripts/Events/SinkRegions.cs`  
**Reason:** Pure sink expansion/merging algorithms  
**Changes Required:**
- Replace `Mathf` ? `Math` / `MathF`
- Replace `UnityEngine.Debug.Log` ? Remove or use abstraction

#### 9. **RegionExpansion.cs** ? `Engine/Logic/RegionExpansionLogic.cs`
**Current Location:** `Assets/Scripts/Events/RegionExpansion.cs`  
**Reason:** Pure region boundary expansion logic  
**Changes Required:**
- Replace `UnityEngine.Random` ? `System.Random`

#### 10. **LegacyTickStep.cs** ? `Engine/Execution/SimulationStepper.cs`
**Current Location:** `Assets/Scripts/Simulation/LegacyTickStep.cs`  
**Reason:** Orchestrates step phases, minimal Unity coupling  
**Changes Required:**
- Remove `UnityEngine.Debug.Log` ? Use event emission instead
- Decouple from Unity-specific callbacks
- Rename to `SimulationStepper`

#### 11. **SimulationEngine.cs** ? `Engine/Execution/SimulationRunner.cs`
**Current Location:** `Assets/Scripts/Simulation/SimulationEngine.cs`  
**Reason:** Main runner loop, already minimal Unity coupling  
**Changes Required:**
- Remove `UnityEngine.Debug.Log`
- Rename to `SimulationRunner`
- Add `Run(ScenarioDefinition, RunRequest)` method

#### 12. **ISimStep.cs** ? `Engine/Interfaces/IStepPhase.cs`
**Current Location:** `Assets/Scripts/Simulation/ISimStep.cs`  
**Reason:** Core interface for step phases  
**Changes Required:**
- Rename to `IStepPhase`
- Update signature to use `StepContext`

---

## ? KEEP IN VIABLE.CORE.UNITY (Presentation Layer)

### Unity-Specific Scripts

#### 1. **SimulationController.cs** ? REFACTOR to `Core.Unity/Controllers/UnitySimulationController.cs`
**Current Location:** `Assets/Scripts/Core/SimulationController.cs`  
**Reason:** MonoBehaviour orchestrator  
**Changes Required:**
- Remove all computation logic ? Call Engine instead
- Keep Unity lifecycle (Start, Update, Coroutine)
- Become thin adapter that:
  - Builds `ScenarioDefinition` from inspector fields
  - Calls `SimulationRunner.Step()` each Update
  - Updates visuals based on engine state

#### 2. **GridRenderer.cs** ? KEEP in `Core.Unity/Visualization/GridRenderer.cs`
**Current Location:** `Assets/Scripts/Unity/GridRenderer.cs`  
**Reason:** Visual rendering only, already separation-friendly  
**Changes Required:**
- None, already takes `StateGrid` as input

#### 3. **CellVisualiser.cs** ? KEEP in `Core.Unity/Visualization/CellVisualiser.cs`
**Current Location:** `Assets/Scripts/Visuals/CellVisualiser.cs`  
**Reason:** SpriteRenderer wrapper  
**Changes Required:** None

#### 4. **SimulationGrid.cs** ? KEEP in `Core.Unity/Controllers/SimulationGrid.cs`
**Current Location:** `Assets/Scripts/Core/SimulationGrid.cs`  
**Reason:** Spawns Unity GameObjects  
**Changes Required:**
- Remove `Cell` class references (superseded by engine state)
- Keep only visual cell spawning

#### 5. **Cell.cs** ? DELETE (superseded by engine StateGrid)
**Current Location:** `Assets/Scripts/Core/Cell.cs`  
**Reason:** Redundant with `StateGrid` arrays  
**Changes Required:** Remove entirely after confirming no dependencies

#### 6. **SimulationUIController.cs** ? KEEP in `Core.Unity/UI/SimulationUIController.cs`
**Current Location:** `Assets/Scripts/Core/SimulationUIController.cs`  
**Reason:** Unity UI bindings  
**Changes Required:**
- Update to read/write via `UnitySimulationController` adapter

#### 7. **CameraController.cs** ? KEEP in `Core.Unity/Visualization/CameraController.cs`
**Current Location:** `Assets/Scripts/Visuals/CameraController.cs`  
**Reason:** Camera positioning  
**Changes Required:** None

---

## ?? NEW FILES TO CREATE

### In Viable.Engine

#### 1. **Engine/Interfaces/IModel.cs**
```csharp
public interface IModel<TState>
{
    TState Initialize(ScenarioDefinition scenario);
    StepResult<TState> Step(TState currentState, StepContext context);
}
```

#### 2. **Engine/Execution/StepResult.cs**
```csharp
public sealed class StepResult<TState>
{
    public TState NextState { get; set; }
    public List<SimulationEvent> Events { get; set; }
    public Dictionary<string, double> Metrics { get; set; }
    public bool IsActive { get; set; }
}
```

#### 3. **Engine/Computation/ViabilityCalculator.cs**
Extract from `SimulationController.ComputeViability()`:
```csharp
public static class ViabilityCalculator
{
    public static float Compute(
        float incomingFlow, 
        float resource, 
        float complexity,
        float complexityGainA,
        float complexityGainK,
        float decayLoss,
        float thresholdEffective)
    {
        float gain = 1f + complexityGainA * (1f - MathF.Exp(-complexityGainK * Math.Max(0f, complexity)));
        return (incomingFlow * gain - decayLoss) / Math.Max(1e-6f, thresholdEffective);
    }
}
```

#### 4. **Engine/Utilities/MathHelpers.cs**
Replacements for Unity `Mathf`:
```csharp
public static class MathHelpers
{
    public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);
    public static float Max(float a, float b) => Math.Max(a, b);
    public static float Min(float a, float b) => Math.Min(a, b);
    // etc.
}
```

### In Viable.Core.Unity

#### 1. **Core.Unity/Adapters/UnityScenarioAdapter.cs**
Converts Unity inspector fields ? `ScenarioDefinition`:
```csharp
public static class UnityScenarioAdapter
{
    public static ScenarioDefinition FromInspector(SimulationController controller)
    {
        return new ScenarioDefinition
        {
            ScenarioId = "unity-scene",
            Parameters = new Dictionary<string, double>
            {
                ["resourceGlobalMax"] = controller.ResourceGlobalMax,
                ["decayLoss"] = controller.DecayLoss,
                // ... all parameters
            },
            GridWidth = controller.Grid.Width,
            GridHeight = controller.Grid.Height
        };
    }
}
```

#### 2. **Core.Unity/Adapters/StateVisualizer.cs**
Maps engine `StateGrid` to visual updates:
```csharp
public static class StateVisualizer
{
    public static void UpdateVisuals(StateGrid state, StepContext context, GridRenderer renderer, RenderMode mode)
    {
        renderer.Render(state, context, mode);
    }
}
```

---

## ?? MIGRATION ORDER (Commits)

### Commit 1: Structure ? COMPLETE
- [x] Create folders
- [x] Create asmdef files
- [x] Create Contracts types
- [x] Create README files

### Commit 2: Extract State & Config (Low Risk)
1. Copy `StateGrid.cs` ? `Engine/State/GridState.cs`
2. Copy `SimConfig.cs` ? `Engine/Configuration/SimulationConfiguration.cs`
3. Copy `SimContext.cs` ? `Engine/Execution/StepContext.cs`
4. Update namespaces, remove Unity Color types
5. Verify build still works (old files still exist)

### Commit 3: Extract Step Phases (Medium Risk)
1. Copy `Pass1.cs` ? `Engine/Steps/OutflowPhase.cs`
2. Copy `Pass2.cs` ? `Engine/Steps/InflowPhase.cs`
3. Copy `Pass3.cs` ? `Engine/Steps/RechargePhase.cs`
4. Copy `Pass4.cs` ? `Engine/Steps/DiffusionPhase.cs`
5. Replace `UnityEngine.Random` ? `System.Random`
6. Replace `Mathf` ? `Math`/`MathF`
7. Verify build

### Commit 4: Extract Logic Helpers
1. Copy `SinkRegions.cs` ? `Engine/Logic/SinkLogic.cs`
2. Copy `RegionExpansion.cs` ? `Engine/Logic/RegionExpansionLogic.cs`
3. Create `ViabilityCalculator.cs` (extract from SimulationController)
4. Remove Debug.Log calls
5. Verify build

### Commit 5: Create Engine Runner
1. Copy `SimulationEngine.cs` ? `Engine/Execution/SimulationRunner.cs`
2. Copy `LegacyTickStep.cs` ? `Engine/Execution/SimulationStepper.cs`
3. Copy `ISimStep.cs` ? `Engine/Interfaces/IStepPhase.cs`
4. Add `Run(ScenarioDefinition, RunRequest) ? RunResult` method
5. Verify build

### Commit 6: Unity Adapter Layer
1. Create `UnityScenarioAdapter.cs`
2. Create `StateVisualizer.cs`
3. Refactor `SimulationController` to use adapters + engine
4. Test in Play mode

### Commit 7: Delete Old Files
1. Delete original Domain/Events/Simulation files
2. Delete `Cell.cs`
3. Update all references
4. Final verification

---

## ?? VERIFICATION CHECKLIST

After each commit:
- [ ] Project builds without errors
- [ ] Assembly references are correct (no cycles)
- [ ] Engine assembly has no `UnityEngine` references
- [ ] Simulation runs and produces same behavior
- [ ] No duplicate code between Engine and Unity layer

---

## ?? ESTIMATED IMPACT

| Category | Files to Migrate | Files to Keep | Files to Create | Files to Delete |
|----------|------------------|---------------|-----------------|-----------------|
| Engine | 12 | 0 | 5 | 0 |
| Core.Unity | 0 | 6 | 2 | 1 (Cell.cs) |
| **Total** | **12** | **6** | **7** | **1** |

---

**Next Step:** Begin Commit 2 - Extract State & Config files to Engine.
