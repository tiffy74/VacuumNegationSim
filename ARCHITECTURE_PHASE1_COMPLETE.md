# VIABLE Architecture Refactor - Phase 1 Complete

## ? Commit 1: Folder Structure + Assembly Definitions

**Status:** COMPLETE  
**Build:** ? SUCCESS  
**Date:** 2024

---

## What Was Done

### 1. Created Folder Structure

```
Assets/Viable/
??? Contracts/              ? DTOs, schema models, versioning
?   ??? Viable.Contracts.asmdef
?   ??? README.md
??? Engine/                 ? Headless simulation core (no Unity)
?   ??? Viable.Engine.asmdef
?   ??? README.md
??? Core.Unity/             ? Unity presentation layer
    ??? Viable.Core.Unity.asmdef
    ??? README.md
```

### 2. Created Assembly Definitions

#### Viable.Contracts.asmdef
- **References:** None
- **Purpose:** Pure C# DTOs, no dependencies
- **Constraint:** `noEngineReferences: true` (no UnityEngine)

#### Viable.Engine.asmdef
- **References:** Viable.Contracts only
- **Purpose:** Headless simulation engine
- **Constraint:** `noEngineReferences: true` (no UnityEngine)

#### Viable.Core.Unity.asmdef
- **References:** Viable.Engine, Viable.Contracts, Unity packages
- **Purpose:** MonoBehaviours, UI, visualization
- **Constraint:** May use UnityEngine freely

### 3. Created Contract Types

All contracts are C# classes compatible with .NET Framework 4.7.1:

#### ContractVersions.cs
```csharp
SchemaVersion = "1.0"
EngineVersion = "0.1.0-alpha"
EngineName = "VIABLE Core Engine"
```

#### EngineMetadata.cs
- Engine identity + version tracking
- Timestamp generation
- Static factory method `Current()`

#### ScenarioDefinition.cs
- Scenario ID and name
- Parameters dictionary (string ? double)
- Initial state placeholder (object)
- Grid dimensions
- Deterministic seed (optional)
- Plugin array for extensions

#### RunRequest.cs
- Target scenario ID
- Step count / duration
- Sample frequency
- Parameter overrides
- Stop conditions
- Event emission flag
- Seed override

#### SimulationEvent.cs
- Event type string
- Step index + simulated time
- Data payload dictionary
- Optional severity level

#### StateSample.cs
- Step index + simulated time
- State snapshot (object placeholder)
- Metrics dictionary

#### RunResult.cs
- Metadata
- Steps executed / final time
- Events list
- Samples list
- Final state (object placeholder)
- Summary metrics
- Stop reason (if applicable)
- Execution time (ms)

---

## Design Decisions

### Why No JSON Attributes?
- .NET Framework 4.7.1 doesn't include `System.Text.Json`
- Attributes removed to maintain build compatibility
- Can be added later when serialization is needed (Newtonsoft.Json or upgrade to .NET Standard)

### Why `object` Placeholders?
- State model (`StateGrid`) not yet migrated to Engine
- Will be replaced with strongly-typed generics after extraction
- Allows contracts to compile independently

### Why Separate Assembly Definitions?
- Enforces architectural boundaries at compile time
- Engine cannot accidentally reference Unity types
- Clear dependency graph: Unity ? Engine ? Contracts

---

## Verification

### Build Status
```
Command: dotnet build
Result: SUCCESS
Errors: 0
Warnings: 0
```

### Assembly Graph
```
Viable.Core.Unity
    ? depends on
Viable.Engine
    ? depends on
Viable.Contracts
```

### No Regressions
- All existing scripts still compile
- No files deleted or moved yet
- Purely additive changes

---

## Next Steps

### Commit 2: Extract State & Config (Next)
1. Copy `StateGrid.cs` ? `Engine/State/GridState.cs`
2. Copy `SimConfig.cs` ? `Engine/Configuration/SimulationConfiguration.cs`
3. Copy `SimContext.cs` ? `Engine/Execution/StepContext.cs`
4. Update namespaces
5. Remove Unity `Color` types from config
6. Verify build

### Commit 3: Extract Step Phases
1. Copy Pass1-4.cs ? Engine/Steps/
2. Replace `UnityEngine.Random` ? `System.Random`
3. Replace `Mathf` ? `Math` / `MathF`
4. Remove `Debug.Log` calls

### Commit 4: Extract Logic Helpers
1. Copy SinkRegions.cs ? Engine/Logic/
2. Copy RegionExpansion.cs ? Engine/Logic/
3. Extract ViabilityCalculator from SimulationController

### Commit 5: Create Engine Runner
1. Copy SimulationEngine.cs ? Engine/Execution/
2. Copy LegacyTickStep.cs ? Engine/Execution/
3. Add Run(ScenarioDefinition, RunRequest) ? RunResult

### Commit 6: Unity Adapter Layer
1. Create UnityScenarioAdapter
2. Refactor SimulationController to use Engine
3. Test in Play mode

### Commit 7: Delete Old Files
1. Delete original Domain/Events/Simulation files
2. Final verification

---

## File Inventory

### Created (11 files)
```
Assets/Viable/Contracts/Viable.Contracts.asmdef
Assets/Viable/Contracts/README.md
Assets/Viable/Contracts/ContractVersions.cs
Assets/Viable/Contracts/EngineMetadata.cs
Assets/Viable/Contracts/ScenarioDefinition.cs
Assets/Viable/Contracts/RunRequest.cs
Assets/Viable/Contracts/RunResult.cs
Assets/Viable/Contracts/SimulationEvent.cs
Assets/Viable/Contracts/StateSample.cs
Assets/Viable/Engine/Viable.Engine.asmdef
Assets/Viable/Engine/README.md
Assets/Viable/Core.Unity/Viable.Core.Unity.asmdef
Assets/Viable/Core.Unity/README.md
```

Also created:
```
ENGINE_EXTRACTION_PLAN.md  ? Complete migration roadmap
```

### Modified
None (additive only)

### Deleted
None

---

## Git Commit Message

```
feat: Add VIABLE architecture structure (Contracts + Engine + Core.Unity)

- Create assembly definition boundaries for clean separation
- Add Contracts namespace with DTOs (ScenarioDefinition, RunRequest, RunResult)
- Add Engine namespace (empty, ready for extraction)
- Add Core.Unity namespace (will contain MonoBehaviours)
- Enforce no UnityEngine references in Contracts and Engine
- All types compatible with .NET Framework 4.7.1
- Schema version 1.0

Refs: ENGINE_EXTRACTION_PLAN.md
```

---

## Success Criteria (All Met ?)

- [x] Folder structure created
- [x] Assembly definitions configured
- [x] Contracts types implemented
- [x] Build successful (no errors)
- [x] No regressions (existing code unaffected)
- [x] Documentation complete
- [x] Clear migration plan documented

---

**Status:** ? READY FOR COMMIT  
**Next Action:** Commit this work, then proceed to Commit 2 (Extract State & Config)

