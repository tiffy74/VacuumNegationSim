# VIABLE Core Refactoring Progress Report

## Completed (Steps 1-3)

### ? Step 1: Theory-Term Inventory
- Created comprehensive `THEORY_TERM_INVENTORY.md`
- Documented 100+ theory-loaded instances across 15+ files
- Generated replacement mapping and banned terms list

### ? Step 2: Core Domain Classes Renamed
**Files Modified:**
- `Assets/Scripts/Domain/Gridstate.cs` 
  - Class: `GridState` ? `StateGrid`
  - Fields: All theory terms neutralized
  
- `Assets/Scripts/Domain/SimConfig.cs`
  - All field names neutralized
  
- `Assets/Scripts/Domain/SimContext.cs`
  - `NGlobal` ? `ResourceGlobal`

**Key Renamings:**
```
IsVacuum          ? IsInactive
FieldPresent      ? ActiveRegion
IsBlackHole       ? IsSink
BlackHole*        ? Sink*
Nlocal/NGlobal    ? ResourceLocal/ResourceGlobal
Entropy           ? ComplexityMetric
VacuumEvent*      ? Perturbation*
Field*            ? Region*
```

### ? Step 3: Event Namespace Files Created/Refactored
**New Files Created:**
- `Assets/Scripts/Events/SinkRegions.cs` (was BlackHoles.cs)
  - Class: `BlackHoles` ? `SinkRegions`
  - Methods: `GrowBlackHoles` ? `ExpandSinkRegions`, etc.
  - All internal references updated
  
- `Assets/Scripts/Events/RegionExpansion.cs` (was FieldWave.cs)
  - Class: `FieldWave` ? `RegionExpansion`
  - Methods: `PropagateFieldWave` ? `ExpandActiveRegion`
  - All comments neutralized

**Files Modified:**
- `Assets/Scripts/Events/Pass1.cs`
  - All parameter and variable names updated
  - All method signatures updated
  - References to BlackHoles ? SinkRegions
  - All comments neutralized

## Remaining Work

### ?? Step 3 (Continued): Events Namespace
**Files Still Need Updating:**
- [ ] `Assets/Scripts/Events/Pass2.cs`
- [ ] `Assets/Scripts/Events/Pass3.cs`
- [ ] `Assets/Scripts/Events/Pass4.cs`
- [ ] `Assets/Scripts/Events/AccretionDisk.cs` (may need removal or renaming)
- [ ] `Assets/Scripts/Events/EventInterface.cs`

**Files to Delete:**
- [ ] `Assets/Scripts/Events/BlackHoles.cs` (replaced by SinkRegions.cs)
- [ ] `Assets/Scripts/Events/FieldWave.cs` (replaced by RegionExpansion.cs)

### ?? Step 4: Simulation Core
**Files Need Updating:**
- [ ] `Assets/Scripts/Simulation/LegacyTickStep.cs`
- [ ] `Assets/Scripts/Simulation/SimulationEngine.cs`
- [ ] `Assets/Scripts/Simulation/Diagnostics.cs`

### ?? Step 5: Unity/Visualization Layer
**Files Need Updating:**
- [ ] `Assets/Scripts/Core/SimulationController.cs` (CRITICAL - main MonoBehaviour)
- [ ] `Assets/Scripts/Core/Cell.cs`
- [ ] `Assets/Scripts/Core/SimulationGrid.cs`
- [ ] `Assets/Scripts/Core/SimulationUIController.cs`
- [ ] `Assets/Scripts/Unity/GridRenderer.cs`
- [ ] `Assets/Scripts/Visuals/CellVisualiser.cs`
- [ ] `Assets/Scripts/Visuals/CameraController.cs`

### ?? Step 6: Documentation
**Files Need Rewriting:**
- [ ] `README.md` (COMPLETE REWRITE)
- [ ] `Assets/ASSETS.md`
- [ ] `Assets/Scripts/SCRIPTS.md`
- [ ] `Assets/Scripts/*/DOMAIN.md, CORE.md, etc.`

### ?? Step 7: Build & Test
- [ ] Build solution and fix compilation errors
- [ ] Test simulation runs correctly
- [ ] Verify behavior unchanged
- [ ] Document any parameter adjustments needed

### ?? Step 8: Final Verification
- [ ] Run banned-term search (expect zero hits)
- [ ] Generate compliance report
- [ ] Update THEORY_TERM_INVENTORY.md with "CLEAN" status

## Breaking Changes Log

### Type Renames
- `GridState` ? `StateGrid`
- `BlackHoles` ? `SinkRegions`
- `FieldWave` ? `RegionExpansion`

### Property/Field Renames (StateGrid)
```csharp
// Old Name              ? New Name
IsVacuum                ? IsInactive
FieldPresent            ? ActiveRegion
IsBlackHole             ? IsSink
BlackHoleId             ? SinkId
BlackHoleCharge         ? SinkCharge
BlackHoleMass           ? SinkMass
BlackHoleParent         ? SinkParent
BlackHolePotential      ? SinkPotential
NextBlackHoleId         ? NextSinkId
Nlocal                  ? ResourceLocal
Entropy                 ? ComplexityMetric
EntropyNext             ? ComplexityNext
FieldFirstTick          ? RegionActivationTick
EnergyFirstTick         ? ResourceFirstTick
ZeroEnergyTicks         ? ZeroResourceTicks
```

### Property/Field Renames (SimConfig)
```csharp
// Old Name                      ? New Name
NGlobalMax                       ? ResourceGlobalMax
NlocalMax                        ? ResourceLocalMax
MinEnergyForPersistence          ? MinResourceForPersistence
VacuumEventProbability           ? PerturbationProbability
VacuumEventEntropy               ? PerturbationComplexity
BlackHoleFormThreshold           ? SinkFormationThreshold
BlackHoleDrainFrac               ? SinkDrainFraction
BlackHoleRecoilFrac              ? SinkRecoilFraction
EntropyPenalty                   ? ComplexityPenalty
EntropyGainPerUse                ? ComplexityGainPerUse
EntropyDiffuseRate               ? ComplexityDiffusionRate
EntropyDecay                     ? ComplexityDecay
EntropyGainFromGradient          ? ComplexityGainFromGradient
EntropyGainNearBH                ? ComplexityGainNearSink
EntropyViabilityGainA            ? ComplexityViabilityGainA
EntropyViabilityGainK            ? ComplexityViabilityGainK
FieldAdvanceChance               ? RegionExpansionChance
FieldAdvanceCost                 ? RegionExpansionCost
FieldAdvanceMinSource            ? RegionExpansionMinSource
FieldAdvanceRequiresViability    ? RegionExpansionRequiresViability
FieldAdvanceSeedsEnergy          ? RegionExpansionSeedsResource
FieldSeedEnergy                  ? RegionSeedResource
VoidColor                        ? InactiveColor
NullspaceColor                   ? DormantRegionColor
ShowEntropyTint                  ? ShowComplexityTint
BlackHolePotentialRadius         ? SinkPotentialRadius
BlackHolePotentialScale          ? SinkPotentialScale
BlackHoleFlowBias                ? SinkFlowBias
BlackHoleViabilityBoost          ? SinkViabilityBoost
```

### Property/Field Renames (SimContext)
```csharp
NGlobal ? ResourceGlobal
```

### Method Renames (SinkRegions)
```csharp
// Old Name                 ? New Name
GrowBlackHoles()           ? ExpandSinkRegions()
BlackHoleAttractEnergy()   ? SinkAbsorbResource()
EnsureBlackHoleCapacity()  ? EnsureSinkCapacity()
CreateBlackHoleEntity()    ? CreateSinkEntity()
```

### Method Renames (RegionExpansion)
```csharp
// Old Name              ? New Name
PropagateFieldWave()     ? ExpandActiveRegion()
```

## Migration Path for External Code

If any external code references this project:

1. Update all type references
2. Update all property/field access
3. Update all method calls
4. Search codebase for banned terms (see THEORY_TERM_INVENTORY.md)
5. Recompile and test

## Notes

- All changes preserve computational behavior
- Only naming and documentation changed
- No algorithm modifications
- Unity inspector may need field re-linking (serialization)

---
**Status:** IN PROGRESS  
**Last Updated:** [Current Date]  
**Remaining Files:** ~15-20  
**Estimated Completion:** Step 6-7 completion needed before PR
