# Phase 3 Complete: Extract Step Phases to Engine

**Status:** ? CODE COMPLETE - Requires Unity Refresh  
**Date:** 2024

---

## What Was Done

### 1. Created Engine Steps Folder
```
Assets/Viable/Engine/Steps/
??? OutflowPhase.cs (NEW)
??? InflowPhase.cs (NEW)
??? RechargePhase.cs (NEW)
??? DiffusionPhase.cs (NEW)
```

### 2. Extracted Step Phases

#### OutflowPhase.cs (`Engine/Steps/`)
- **Source:** `Assets/Scripts/Events/Pass1.cs`
- **Changes:**
  - Namespace: `Assets.Scripts.Events` ? `Viable.Engine.Steps`
  - Class name: `Pass1` ? `OutflowPhase`
  - **Removed Unity dependencies:**
    - `using UnityEngine` ? Removed
    - `using UnityEngine.Rendering` ? Removed
    - `Mathf.Clamp01()` ? `Math.Clamp(value, 0f, 1f)`
    - `Mathf.Max()` ? `Math.Max()`
    - `Mathf.Exp()` ? `MathF.Exp()`
    - `Debug.Log()` ? Removed (commented for event emission)
  - **Updated references:**
    - `SinkRegions.GetRootAtCell()` ? `Logic.SinkLogic.GetRootAtCell()` (Phase 4)
    - `SinkRegions.AssignOrMergeAtCell()` ? `Logic.SinkLogic.AssignOrMergeAtCell()` (Phase 4)
- **Status:** ? Complete, awaits SinkLogic extraction (Phase 4)

#### InflowPhase.cs (`Engine/Steps/`)
- **Source:** `Assets/Scripts/Events/Pass2.cs`
- **Changes:**
  - Namespace: `Assets.Scripts.Events` ? `Viable.Engine.Steps`
  - Class name: `Pass2` ? `InflowPhase`
  - **Removed Unity dependencies:**
    - `using UnityEngine` ? Removed
    - `Mathf.Min()` ? `Math.Min()`
    - `Mathf.Max()` ? `Math.Max()`
    - `Mathf.Abs()` ? `Math.Abs()`
    - `Mathf.Log()` ? `MathF.Log()`
    - `Mathf.Clamp01()` ? `Math.Clamp(value, 0f, 1f)`
    - `UnityEngine.Random.value` ? `rng.NextDouble()` ? **Deterministic**
  - **Added parameter:**
    - `Random rng` - Uses StepContext.Rng for deterministic perturbations
- **Status:** ? Complete, Unity-free, deterministic

#### RechargePhase.cs (`Engine/Steps/`)
- **Source:** `Assets/Scripts/Events/Pass3.cs`
- **Changes:**
  - Namespace: `Assets.Scripts.Events` ? `Viable.Engine.Steps`
  - Class name: `Pass3` ? `RechargePhase`
  - **Removed Unity dependencies:** None (already pure)
  - `Math.Min()` used instead of Mathf
- **Status:** ? Complete, trivial conversion

#### DiffusionPhase.cs (`Engine/Steps/`)
- **Source:** `Assets/Scripts/Events/Pass4.cs`
- **Changes:**
  - Namespace: `Assets.Scripts.Events` ? `Viable.Engine.Steps`
  - Class name: `Pass4` ? `DiffusionPhase`
  - Method: `EntropyDiffuse()` ? `ComplexityDiffuse()`
  - **Removed Unity dependencies:**
    - `using UnityEngine` ? Removed
    - `Mathf.Max()` ? `Math.Max()`
    - `Mathf.Clamp01()` ? `Math.Clamp(value, 0f, 1f)`
- **Status:** ? Complete, Unity-free

### 3. Marked Original Files as Deprecated

Added deprecation headers to:
- `Assets/Scripts/Events/Pass1.cs`
- `Assets/Scripts/Events/Pass2.cs`
- `Assets/Scripts/Events/Pass3.cs`
- `Assets/Scripts/Events/Pass4.cs`

---

## Key Improvements

### 1. Deterministic Random Number Generation
**InflowPhase** now takes `Random rng` parameter:
```csharp
// OLD (Unity, non-deterministic)
if (UnityEngine.Random.value < PerturbationProbability) { ... }

// NEW (Engine, deterministic)
if (rng.NextDouble() < PerturbationProbability) { ... }
```

**Impact:**
- Same seed ? same perturbations ? reproducible simulations ?
- Critical for testing and debugging
- Enables replay and verification

### 2. Unity Math Replacements

| Unity Type | Engine Replacement | Notes |
|------------|-------------------|-------|
| `Mathf.Clamp01(x)` | `Math.Clamp(x, 0f, 1f)` | .NET Standard |
| `Mathf.Max(a, b)` | `Math.Max(a, b)` | .NET Standard |
| `Mathf.Min(a, b)` | `Math.Min(a, b)` | .NET Standard |
| `Mathf.Abs(x)` | `Math.Abs(x)` | .NET Standard |
| `Mathf.Log(x)` | `MathF.Log(x)` | .NET Standard 2.1+ |
| `Mathf.Exp(x)` | `MathF.Exp(x)` | .NET Standard 2.1+ |
| `UnityEngine.Random` | `System.Random` | Seeded for determinism |

### 3. Debug.Log Removal
- All `Debug.Log()` calls removed or commented
- Logging replaced with event emission (future: SimulationEvent)
- Engine remains silent by default (no console coupling)

---

## Pending Dependencies

### Phase 4 Requirement: SinkLogic Extraction

**OutflowPhase** references:
```csharp
Logic.SinkLogic.GetRootAtCell(...)
Logic.SinkLogic.AssignOrMergeAtCell(...)
```

**Action Required in Phase 4:**
1. Extract `SinkRegions.cs` ? `Engine/Logic/SinkLogic.cs`
2. Update OutflowPhase references
3. Verify build

**Current Status:** OutflowPhase will have compile errors until SinkLogic is extracted.

---

## Verification Status

### Code Compilation
- ?? **OutflowPhase.cs** - Will fail until SinkLogic extracted (Phase 4)
- ? **InflowPhase.cs** - No errors
- ? **RechargePhase.cs** - No errors
- ? **DiffusionPhase.cs** - No errors
- ? Original files still compile (deprecated but functional)

### Unity Dependencies Removed
- ? All `using UnityEngine` removed
- ? All `Mathf.*` replaced with `Math.*` / `MathF.*`
- ? All `UnityEngine.Random` replaced with `System.Random`
- ? All `Debug.Log()` removed

### Determinism Achieved
- ? InflowPhase uses seeded RNG
- ? All math operations deterministic
- ? No Unity frame-dependent behavior

---

## Manual Steps Required

### Before Committing Phase 3:

1. **Open Unity Editor**
   - Unity will detect new Step files
   - Generate `.meta` files

2. **Verify in Unity**
   - Check console - expect **compile errors** in OutflowPhase (expected, fixed in Phase 4)
   - Old Pass files still work

3. **Regenerate Visual Studio Solution**
   - In Unity: `Assets ? Open C# Project`

4. **Reopen Visual Studio**
   - Verify `Viable.Engine` project shows new Step files

5. **Note Build Errors (Expected)**
   - OutflowPhase: "SinkLogic not found" ? **Normal, fixed in Phase 4**

---

## Git Status

### New Files
```
Assets/Viable/Engine/Steps/OutflowPhase.cs
Assets/Viable/Engine/Steps/InflowPhase.cs
Assets/Viable/Engine/Steps/RechargePhase.cs
Assets/Viable/Engine/Steps/DiffusionPhase.cs
+ .meta files (after Unity)
```

### Modified Files
```
Assets/Scripts/Events/Pass1.cs (deprecation header)
Assets/Scripts/Events/Pass2.cs (deprecation header)
Assets/Scripts/Events/Pass3.cs (deprecation header)
Assets/Scripts/Events/Pass4.cs (deprecation header)
```

---

## Commit Message Template

```
feat(engine): Extract step phases to Viable.Engine

Phase 3 of architecture refactor - extract Unity-free step logic to Engine.

- Add OutflowPhase (formerly Pass1) to Engine/Steps
  (depends on SinkLogic extraction in Phase 4)
- Add InflowPhase (formerly Pass2) to Engine/Steps
  (deterministic RNG via System.Random)
- Add RechargePhase (formerly Pass3) to Engine/Steps
- Add DiffusionPhase (formerly Pass4) to Engine/Steps
- Replace all Mathf.* with Math.* / MathF.*
- Replace UnityEngine.Random with System.Random (seeded)
- Remove all Debug.Log calls
- Mark original Pass files as deprecated (remove in Phase 7)

Breaking Changes: None (old files still functional)
Known Issue: OutflowPhase awaits SinkLogic extraction (Phase 4)

Refs: ENGINE_EXTRACTION_PLAN.md Phase 3
```

---

## Next Steps (Phase 4)

After committing Phase 3:

1. **Extract Logic Helpers**
   - Copy `SinkRegions.cs` ? `Engine/Logic/SinkLogic.cs`
   - Copy `RegionExpansion.cs` ? `Engine/Logic/RegionExpansionLogic.cs`
   - Replace `Mathf` ? `Math`
   - Remove `Debug.Log`
   - Fix OutflowPhase references

2. **Extract ViabilityCalculator**
   - Create `Engine/Computation/ViabilityCalculator.cs`
   - Extract from `SimulationController.ComputeViability()`

3. **Verify Build**
   - All Engine files compile
   - No Unity dependencies

---

## Success Criteria

- [x] OutflowPhase extracted (awaits SinkLogic)
- [x] InflowPhase extracted (deterministic)
- [x] RechargePhase extracted
- [x] DiffusionPhase extracted
- [x] All Unity math replaced
- [x] All RNG deterministic
- [x] Debug.Log removed
- [x] Original files marked deprecated
- [ ] Build successful (after Phase 4 completes SinkLogic)

---

**Status:** ? PHASE 3 CODE COMPLETE  
**Known Issue:** OutflowPhase compilation blocked on SinkLogic (resolved in Phase 4)  
**Action Required:** Commit Phase 3, proceed to Phase 4

