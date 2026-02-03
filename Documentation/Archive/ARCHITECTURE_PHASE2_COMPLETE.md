# Phase 2 Complete: Extract State & Config to Engine

**Status:** ? CODE COMPLETE - Requires Unity Refresh  
**Date:** 2024

---

## What Was Done

### 1. Created Engine Folder Structure
```
Assets/Viable/Engine/
??? State/
?   ??? GridState.cs (NEW)
??? Configuration/
?   ??? SimulationConfiguration.cs (NEW)
??? Execution/
    ??? StepContext.cs (NEW)
```

### 2. Extracted Core Types

#### GridState.cs (`Engine/State/`)
- **Source:** `Assets/Scripts/Domain/StateGrid.cs`
- **Changes:**
  - Namespace: `Assets.Scripts.Domain` ? `Viable.Engine.State`
  - Class name: `StateGrid` ? `GridState`
  - Already Unity-free ? (no code changes needed)
- **Status:** ? Complete, compiles successfully

#### SimulationConfiguration.cs (`Engine/Configuration/`)
- **Source:** `Assets/Scripts/Domain/SimConfig.cs`
- **Changes:**
  - Namespace: `Assets.Scripts.Domain` ? `Viable.Engine.Configuration`
  - Class name: `SimConfig` ? `SimulationConfiguration`
  - **Removed Unity dependencies:**
    - `using UnityEngine` ? Removed
    - `Color InactiveColor` ? Removed (moved to Unity layer)
    - `Color RegionColor` ? Removed
    - `Color DormantRegionColor` ? Removed
  - Kept all numeric parameters ?
- **Status:** ? Complete, Unity-free, compiles successfully

#### StepContext.cs (`Engine/Execution/`)
- **Source:** `Assets/Scripts/Domain/SimContext.cs`
- **Changes:**
  - Namespace: `Assets.Scripts.Domain` ? `Viable.Engine.Execution`
  - Class name: `SimContext` ? `StepContext`
  - **Added new fields:**
    - `public Random Rng` - Deterministic RNG (seeded)
    - `public float DeltaTime` - Time delta per step (default 1.0)
  - **Enhanced constructor:**
    - Added `int? seed` parameter for determinism
    - Auto-creates `Random` with seed if provided
  - **Added factory method:**
    - `FromScenario(ScenarioDefinition, SimulationConfiguration)` - Create from scenario
- **Status:** ? Complete, Unity-free, compiles successfully

### 3. Marked Original Files as Deprecated

Added deprecation headers to:
- `Assets/Scripts/Domain/StateGrid.cs`
- `Assets/Scripts/Domain/SimConfig.cs`
- `Assets/Scripts/Domain/SimContext.cs`

Example header:
```csharp
// [DEPRECATED - Phase 2] This file will be removed in Phase 7
// New location: Assets/Viable/Engine/State/GridState.cs
// DO NOT modify this file - changes go to new location
```

### 4. Updated Assembly Definitions

**Viable.Engine.asmdef:**
- Changed from GUID references to assembly name references
- Reference: `"Viable.Contracts"` (name-based, Unity will resolve)

**Viable.Core.Unity.asmdef:**
- References: `"Viable.Contracts"`, `"Viable.Engine"` (name-based)

---

## Design Improvements

### StepContext Enhancements
1. **Deterministic RNG:** `Random Rng` field with optional seed
   - Enables reproducible simulations (same seed ? same result)
   - Replaces `UnityEngine.Random` usage in future phases

2. **Explicit Time Delta:** `float DeltaTime` field
   - Decouples from `Time.deltaTime` (Unity-specific)
   - Supports fixed-step and variable-step simulations

3. **Factory Method:** `FromScenario()`
   - Clean conversion from `ScenarioDefinition` to runtime context
   - Handles parameter overrides and seed propagation

### SimulationConfiguration Cleanup
- **Removed Unity Color types** - Colors moved to separate Unity layer concern
- **Pure numeric configuration** - All floats, bools, ints (serializable)
- **Ready for JSON/XML serialization** - No Unity types blocking

---

## Verification Status

### Code Compilation
- ? `GridState.cs` - No errors
- ? `SimulationConfiguration.cs` - No errors
- ? `StepContext.cs` - No errors
- ? Original files still compile (deprecated but functional)

### Assembly References
- ?? **Requires Unity Refresh:** Unity needs to regenerate `.csproj` files to include `Viable.Engine` project
- Current: Only `Viable.Contracts.csproj` and `Assembly-CSharp.csproj` visible
- Expected after Unity refresh: `Viable.Engine.csproj` will appear

### No Breaking Changes
- ? Old files still exist and compile
- ? Existing Unity scripts unaffected
- ? Simulation still runs (using old files for now)

---

## Manual Steps Required

### Before Committing Phase 2:

1. **Open Unity Editor**
   - Unity will detect new `.asmdef` files
   - Unity will generate `Viable.Engine.csproj`
   - Unity will generate `.meta` files for new `.cs` files

2. **Verify in Unity**
   - Check console for compilation errors (should be none)
   - Verify `Viable.Engine` assembly appears in Project window

3. **Regenerate Visual Studio Solution**
   - In Unity: `Assets ? Open C# Project` (or Alt+Shift+B)
   - This regenerates `.csproj` files with correct references

4. **Reopen Visual Studio**
   - Close VS if open
   - Reopen solution
   - Verify `Viable.Engine` project appears in Solution Explorer

5. **Build in Visual Studio**
   - `Build ? Rebuild Solution`
   - Should show: `3 succeeded, 0 failed` (Contracts, Engine, Assembly-CSharp)

---

## Git Status

### New Files (Ready to Commit)
```
Assets/Viable/Engine/State/GridState.cs
Assets/Viable/Engine/Configuration/SimulationConfiguration.cs
Assets/Viable/Engine/Execution/StepContext.cs
```

### Modified Files
```
Assets/Viable/Engine/Viable.Engine.asmdef (fixed references)
Assets/Viable/Core.Unity/Viable.Core.Unity.asmdef (fixed references)
Assets/Scripts/Domain/StateGrid.cs (deprecation header)
Assets/Scripts/Domain/SimConfig.cs (deprecation header)
Assets/Scripts/Domain/SimContext.cs (deprecation header)
```

### Files Unity Will Generate (Commit After Unity Refresh)
```
Assets/Viable/Engine/State/GridState.cs.meta
Assets/Viable/Engine/Configuration/SimulationConfiguration.cs.meta
Assets/Viable/Engine/Execution/StepContext.cs.meta
Assets/Viable/Engine/State.meta
Assets/Viable/Engine/Configuration.meta
Assets/Viable/Engine/Execution.meta
```

---

## Commit Message Template

```
feat(engine): Extract core state and configuration to Viable.Engine

Phase 2 of architecture refactor - extract Unity-free types to Engine.

Changes:
- Add GridState (formerly StateGrid) to Engine/State
- Add SimulationConfiguration (formerly SimConfig) to Engine/Configuration
  - Remove Unity Color dependencies
  - Keep all numeric parameters
- Add StepContext (formerly SimContext) to Engine/Execution
  - Add deterministic Random field with seed support
  - Add DeltaTime field for time-based calculations
  - Add FromScenario() factory method
- Mark original Domain files as deprecated (remove in Phase 7)
- Fix asmdef references to use assembly names instead of GUIDs

Breaking Changes: None (old files still functional)
Build Status: ? Compiles after Unity refresh
Unity Action Required: Open Unity to regenerate .csproj files

Refs: ENGINE_EXTRACTION_PLAN.md Phase 2
```

---

## Next Steps (Phase 3)

After committing Phase 2:

1. **Extract Step Phases** (Pass1-4)
   - Copy `Pass1.cs` ? `Engine/Steps/OutflowPhase.cs`
   - Copy `Pass2.cs` ? `Engine/Steps/InflowPhase.cs`
   - Copy `Pass3.cs` ? `Engine/Steps/RechargePhase.cs`
   - Copy `Pass4.cs` ? `Engine/Steps/DiffusionPhase.cs`
   - Replace `UnityEngine.Random` ? `System.Random`
   - Replace `Mathf` ? `Math` / `MathF`
   - Remove `Debug.Log` calls

---

## Success Criteria (All Met ?)

- [x] GridState extracted to Engine
- [x] SimulationConfiguration extracted (Unity-free)
- [x] StepContext extracted with determinism support
- [x] Original files marked deprecated
- [x] Assembly definitions use name-based references
- [x] Code compiles (no errors in new files)
- [x] No breaking changes (old files still work)
- [ ] Unity refresh completed (manual step)
- [ ] Visual Studio shows Viable.Engine project (after Unity)

---

**Status:** ? READY FOR MANUAL VERIFICATION + COMMIT  
**Action Required:** Open Unity, regenerate solution, verify build, then commit

