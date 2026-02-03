# VIABLE Core: Theory-Term Inventory & Replacement Mapping

**Date:** 2024
**Purpose:** Complete audit of physics/theory-loaded terminology for neutralization

---

## EXECUTIVE SUMMARY

This document catalogs all physics/cosmology/theory-laden terminology found in the VacuumNegationSim codebase and provides neutral replacements for the VIABLE Core product.

**Status Categories:**
- ?? **CRITICAL** - Core class/file names, public APIs, must change
- ?? **HIGH** - Inspector labels, UI text, user-facing strings
- ?? **MEDIUM** - Comments, internal variable names
- ? **LOW** - Debug logs, test names

---

## TERM REPLACEMENT MAPPING

### Core Concepts

| Theory Term | Neutral Replacement | Priority | Notes |
|------------|---------------------|----------|-------|
| Nullstate / Null-state / NullSpace | ReferenceState / InactiveRegion | ?? | Core concept |
| Vacuum | Inactive / Dormant / Empty | ?? | Pervasive |
| Vacuum Event | Random Perturbation / Spontaneous Activation | ?? | Parameter names |
| Vacuum Negation | State Activation / Resource Injection | ?? | Project name itself |
| Entropy | ComplexityMetric / StructureCost / Disorder | ?? | Thermodynamic term |
| Black Hole | Sink / Drain / Attractor / DeadZone | ?? | File names + APIs |
| Field / FieldPresent | ActiveRegion / DefinedRegion / Substrate | ?? | Ubiquitous in code |
| Configuration Space | StateSpace / Grid / Domain | ?? | Physics term |
| Wavefunction / Collapse | State / Deactivation / Decay | ?? | Comments only |
| Spacetime | Grid / Domain / Canvas | ?? | Docs/README |
| Cosmology / Big Bang | Evolution / Initialization | ?? | README only |
| Quantum | Discrete / Probabilistic | ?? | README only |
| Event Horizon | Boundary / Edge / Perimeter | ?? | Comments |
| Symmetry | Balance / Uniformity | ?? | Deprecated feature |
| Thermodynamic Selection | Viability-Based Filtering | ?? | README/docs |
| Decoherence | Isolation / Disconnection | ?? | Future feature |
| Born Rule | Probability Weighting | ?? | Future feature |
| FST (Framework) | Theory Framework (remove entirely) | ?? | README |
| Ontology / Metaphysics | Architecture / Model | ?? | README |
| Energy | Resource / Budget / Capacity | ?? | Pervasive |
| Matter | Resource / Quantity | ?? | Rare |
| Gravitational | Attractive / Biased Flow | ?? | Comments |

### Class/File Names (CRITICAL - Must Rename)

| Current Name | New Name | Type |
|-------------|----------|------|
| `BlackHoles.cs` | `SinkRegions.cs` | Class |
| `FieldWave.cs` | `RegionExpansion.cs` | Class |
| `VacuumNegationSim` (project) | `VIABLE` | Project |
| `SimulationController` | Keep (generic enough) | - |
| `GridState` | `StateGrid` (more neutral order) | Class |

### Field/Property Names (HIGH Priority)

```csharp
// Domain Model (GridState)
IsVacuum          ? IsInactive
FieldPresent      ? ActiveRegion
IsBlackHole       ? IsSink
BlackHoleId       ? SinkId
BlackHoleCharge   ? SinkCharge
BlackHoleMass     ? SinkMass
BlackHoleParent   ? SinkParent
BlackHolePotential? SinkPotential
Nlocal            ? ResourceLocal
NGlobal           ? ResourceGlobal
Entropy           ? ComplexityMetric
EntropyNext       ? ComplexityNext
VacuumEventProbability ? PerturbationProbability
VacuumEventEntropy     ? PerturbationComplexity
FieldFirstTick    ? RegionActivationTick
EnergyFirstTick   ? ResourceFirstTick

// SimConfig
MinEnergyForPersistence  ? MinResourceForPersistence
VacuumEventProbability   ? PerturbationProbability
VacuumEventEntropy       ? PerturbationComplexity
BlackHoleFormThreshold   ? SinkFormationThreshold
BlackHoleDrainFrac       ? SinkDrainFraction
BlackHoleRecoilFrac      ? SinkRecoilFraction
EntropyGainPerUse        ? ComplexityGainPerUse
EntropyDiffuseRate       ? ComplexityDiffusionRate
EntropyDecay             ? ComplexityDecay
EntropyPenalty           ? ComplexityPenalty
EntropyGainFromGradient  ? ComplexityGainFromGradient
EntropyGainNearBH        ? ComplexityGainNearSink
EntropyViabilityGainA    ? ComplexityViabilityGainA
EntropyViabilityGainK    ? ComplexityViabilityGainK
FieldAdvanceChance       ? RegionExpansionChance
FieldAdvanceCost         ? RegionExpansionCost
FieldAdvanceMinSource    ? RegionExpansionMinSource
FieldAdvanceRequiresViability ? RegionExpansionRequiresViability
FieldAdvanceSeedsEnergy  ? RegionExpansionSeedsResource
FieldSeedEnergy          ? RegionSeedResource
```

### UI/Inspector Labels (HIGH Priority)

```csharp
// SimulationController [Header] attributes
"Black Hole"       ? "Sink Regions"
"Entropy Dynamics" ? "Complexity Dynamics"
"Vacuum Event"     ? "Random Perturbation"
"Field Propagation"? "Region Expansion"
"Global Budget"    ? "Global Resource Pool"
"Energy"           ? "Resource"

// Color names
VoidColor          ? InactiveColor
NullspaceColor     ? DormantRegionColor
VacuumEnergyColor  ? PerturbationColor
ShowEntropyTint    ? ShowComplexityTint
```

### Method Names (MEDIUM Priority)

```csharp
// BlackHoles.cs ? SinkRegions.cs
BlackHoleAttractEnergy()  ? SinkAbsorbResource()
GrowBlackHoles()          ? ExpandSinkRegions()
ComputePotential()        ? ComputeAttractionField()

// FieldWave.cs ? RegionExpansion.cs
PropagateFieldWave()      ? ExpandActiveRegion()
```

### Comments & Documentation (MEDIUM-LOW Priority)

All inline comments must be rewritten to:
- Describe computational behavior, not physical analogy
- Use neutral domain language
- Focus on "what it does" not "what it represents"

Example transformations:
```csharp
// OLD: "Configuration space pervades everywhere (like spacetime)"
// NEW: "Substrate defines where state transitions can occur"

// OLD: "Black holes collapse the field (event horizon)"
// NEW: "Sink regions prevent state propagation at boundaries"

// OLD: "Vacuum fluctuation injects random entropy"
// NEW: "Random perturbations inject complexity into dormant regions"
```

---

## FILE-BY-FILE INVENTORY

### ?? CRITICAL FILES (Core Logic)

#### `Assets/Scripts/Domain/GridState.cs`
- **IsVacuum** (field) ? IsInactive
- **FieldPresent** (field) ? ActiveRegion
- **IsBlackHole** (field) ? IsSink
- **BlackHole*** (multiple fields) ? Sink***
- **Nlocal** ? ResourceLocal
- **Entropy** (field) ? ComplexityMetric
- **EntropyNext** ? ComplexityNext
- **Comments:** 8 references to "vacuum", "black hole", "configuration space"

#### `Assets/Scripts/Domain/SimConfig.cs`
- **All "Entropy" fields** ? Complexity
- **All "BlackHole" fields** ? Sink
- **All "Vacuum" fields** ? Perturbation
- **All "Field" fields** ? Region
- **NullspaceColor** ? DormantRegionColor
- **Comments:** 5 references to physics concepts

#### `Assets/Scripts/Domain/SimContext.cs`
- **NGlobal** ? ResourceGlobal
- **ScaleFactor** ? keep (generic)

#### `Assets/Scripts/Events/BlackHoles.cs` ? **`SinkRegions.cs`**
- **Entire file** rename + all internal references
- Class name: `BlackHoles` ? `SinkRegions`
- All method names updated
- 40+ references to "black hole" in comments

#### `Assets/Scripts/Events/FieldWave.cs` ? **`RegionExpansion.cs`**
- **Entire file** rename
- Class name: `FieldWave` ? `RegionExpansion`
- Method: `PropagateFieldWave` ? `ExpandActiveRegion`
- **Comments:** Heavy physics metaphors ("configuration space pervades", "event horizon")

#### `Assets/Scripts/Core/SimulationController.cs`
- **[Header]** attributes: 6 theory-loaded section names
- **Public fields:** 30+ inspector-exposed fields with theory names
- **Comments:** Extensive physics explanations
- **VacuumEnergyColor**, **NullspaceColor** ? rename
- **Debug.Log strings:** 10+ references to theory terms

#### `Assets/Scripts/Core/Cell.cs`
- **IsVacuum** ? IsInactive
- **Energy** ? Resource
- **Entropy**, **EntropyNext** ? ComplexityMetric, ComplexityNext
- **Comments:** "negation source", "thermodynamic fields"

### ?? HIGH PRIORITY (UI/Presentation)

#### `Assets/Scripts/Unity/GridRenderer.cs`
- **VoidColor**, **NullspaceColor** parameters
- **ShowEntropyTint** ? ShowComplexityTint
- **Comments:** "field color", "void", "entropy tint"

#### `Assets/Scripts/Visuals/CellVisualiser.cs`
- Color assignment logic references
- **Comments:** May reference "field", "void", "entropy"

#### `Assets/Scripts/Core/SimulationUIController.cs`
- UI labels and text (need to inspect)
- Button labels, panel titles

### ?? MEDIUM PRIORITY (Internal Logic)

#### `Assets/Scripts/Events/Pass1.cs`
- **BlackHolePotential** references ? SinkPotential
- **Comments:** "black hole bias", "flow toward BH"

#### `Assets/Scripts/Events/Pass2.cs`
- **FieldPresent** references ? ActiveRegion
- **Comments:** May reference "configuration space"

#### `Assets/Scripts/Events/Pass3.cs`
- **Comments:** Likely theory-neutral already (activation logic)

#### `Assets/Scripts/Events/Pass4.cs`
- **Entropy** diffusion logic ? Complexity
- **Comments:** "entropy", "diffusion"

#### `Assets/Scripts/Simulation/LegacyTickStep.cs`
- Wrapper class - minimal changes needed
- **Comments:** May reference "pass" purposes

#### `Assets/Scripts/Simulation/SimulationEngine.cs`
- Generic stepper - likely already neutral
- **Comments:** Check for theory references

#### `Assets/Scripts/Simulation/Diagnostics.cs`
- Debug output strings
- **Comments:** Theory references in logs

### ? LOW PRIORITY (Documentation/Support)

#### `README.md`
- **Entire file** must be rewritten
- Remove physics analogy table
- Remove cosmology/quantum references
- Replace with product-focused language

#### `Assets/ASSETS.md`, `Assets/Scripts/SCRIPTS.md`, etc.
- Architecture docs - neutralize language

---

## BANNED TERMS LIST

**The following terms must NOT appear in the final codebase:**

### Absolute Ban (Zero Tolerance)
- Nullstate / Null-state / Null Space / Nullspace
- Vacuum (except in historical context)
- Black Hole / Blackhole / Event Horizon
- Spacetime / Space-time
- Quantum (except "discrete")
- Wavefunction / Wave Function
- Decoherence
- Cosmology / Cosmological
- Big Bang
- Thermodynamic Selection / TST / FST
- Ontology / Ontological (in physics sense)
- Metaphysics / Metaphysical
- Born Rule
- Symmetry Deviation (specific theory term)
- Configuration Space (use StateSpace or Grid)

### Contextual Ban (Replace with Neutral)
- Energy ? Resource / Budget / Capacity
- Entropy ? ComplexityMetric / Disorder / StructureCost
- Field ? ActiveRegion / Substrate / Domain
- Matter ? Resource / Quantity
- Collapse ? Deactivation / Decay / Failure
- Propagate ? Spread / Expand / Flow
- Void ? Inactive / Empty / Dormant

### Allowed (Neutral)
- Viability
- Threshold
- Persistence
- Inflow / Outflow
- Maintenance Cost
- Dissipation / Decay
- Configuration / State
- Grid / Cell / Region
- Active / Inactive
- Sink / Source / Attractor
- Complexity
- Budget / Resource

---

## REPLACEMENT STRATEGY

### Phase 1: Mechanical Renaming (This PR)
1. Rename files (`BlackHoles.cs` ? `SinkRegions.cs`, etc.)
2. Rename classes and namespaces
3. Rename all public fields/properties
4. Update all call sites automatically via refactoring tools

### Phase 2: Inspector & UI (This PR)
5. Update [Header] attributes
6. Update inspector field names (serialized field names)
7. Update color variable names
8. Update UI panel/button text

### Phase 3: Documentation (This PR)
9. Rewrite README.md entirely
10. Update inline comments (focus on behavior, not analogy)
11. Neutralize Debug.Log strings
12. Update architecture docs

### Phase 4: Verification (This PR)
13. Build and run - verify no regressions
14. Final keyword scan - confirm zero banned terms
15. Generate compliance report

---

## NEUTRAL LANGUAGE GUIDELINES

### DO say:
- "Resource flows between active regions based on viability"
- "Sink regions absorb adjacent resources"
- "Random perturbations inject complexity"
- "Regions expand probabilistically when viable"
- "Maintenance cost determines persistence threshold"
- "Complexity accumulates from structural gradients"
- "Dissipation bias favors uniform states"

### DON'T say:
- "Energy propagates through configuration space"
- "Black holes form event horizons"
- "Vacuum fluctuations create entropy"
- "Field collapses under thermodynamic pressure"
- "Spacetime emerges from symmetry breaking"
- "Ontological primacy of the nullstate"

---

## VERIFICATION CHECKLIST

After refactoring, verify:
- [ ] No file names contain banned terms
- [ ] No class names contain banned terms
- [ ] No public API methods contain banned terms
- [ ] No inspector labels contain banned terms
- [ ] No UI text contains banned terms
- [ ] README contains zero physics analogies
- [ ] Comments describe behavior, not theory
- [ ] Debug logs use neutral language
- [ ] Project builds without errors
- [ ] Simulation runs with identical behavior
- [ ] Final keyword scan returns zero matches

---

**End of Inventory**
