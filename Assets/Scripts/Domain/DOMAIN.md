# Domain Module

This module contains pure data structures representing simulation state and configuration. No Unity dependencies, no behavior - just data.

## Overview

The Domain module defines the "what" of the simulation: what data exists, how it's structured, and what parameters control behavior. These are Plain Old C# Objects (POCOs) that can be serialized, tested, and used independently of Unity.

## Files

### GridState.cs

**Purpose**: Encapsulates all per-cell and per-entity simulation state for a 2D grid.

**Key Responsibilities**:
- Stores all per-cell arrays (energy, entropy, viability, field presence, etc.)
- Manages black hole entity tracking (union-find data structures)
- Provides grid indexing utilities
- Handles state reset

#### Constructor
public GridState(int width, int height, int initialBlackHoleCapacity = 8192)

**Parameters**:
- `width`, `height`: Grid dimensions
- `initialBlackHoleCapacity`: Initial size for BH entity arrays (grows as needed)

**Purpose**: Allocates all state arrays based on grid size

#### Per-Cell State Arrays

All indexed by `[i]` where `i = y * width + x`:

##### Core State
- `float[] Nlocal`: Local energy per cell
- `float[] Entropy`: Entropy/complexity [0, 1]
- `float[] V`: Viability (persistence criterion)
- `byte[] Active`: Active state (0 or 1)

##### Buffers
- `float[] Incoming`: Energy destined for cell (Pass1 ? Pass2 buffer)
- `float[] EntropyNext`: Entropy double-buffer for diffusion

##### Geometry
- `bool[] FieldPresent`: Configuration space active at cell
- `bool[] IsVacuum`: Permanent vacuum state
- `bool[] IsBlackHole`: Collapsed configuration space

##### Black Hole Formation
- `float[] BlackHoleCharge`: Charge accumulated at boundary voids
- `float[] BlackHolePotential`: Gravitational potential field (optional)

##### Black Hole Identity
- `int[] BlackHoleId`: BH entity ID per cell (0 = no BH)

##### Tracking
- `int[] FieldFirstTick`: Tick when field arrived (-1 = not yet)
- `int[] EnergyFirstTick`: Tick when energy first arrived (-1 = not yet)
- `int[] ZeroEnergyTicks`: Consecutive ticks with zero energy

#### Black Hole Entity Arrays

Indexed by BH entity ID (not cell index):

- `int[] BlackHoleParent`: Union-find parent pointers
- `float[] BlackHoleMass`: Total mass per BH entity
- `int NextBlackHoleId`: Next available BH entity ID

#### Public Properties

##### `readonly int W`, `readonly int H`, `readonly int Len`
- Grid dimensions and total cell count
- Immutable after construction

#### Core Methods

##### `int Idx(int x, int y)`
- **Purpose**: Converts 2D coordinates to 1D array index
- **Formula**: `y * W + x`
- **Usage**: All array access uses this indexer

##### `void Reset()`
- **Purpose**: Clears all state arrays to default values
- **Logic**:
  - Zeros all float/byte arrays
  - Sets FieldFirstTick/EnergyFirstTick to -1
  - Resets BH entity tracking
  - Resets NextBlackHoleId to 1

##### `void EnsureBlackHoleCapacity(int requiredId)`
- **Purpose**: Grows BH entity arrays if needed
- **Logic**: Doubles capacity until `requiredId` fits
- **Note**: Called automatically when creating new BH entities

##### `int CreateBlackHoleEntity(float initialMass = 1f)`
- **Purpose**: Allocates a new BH entity ID
- **Returns**: New BH entity ID
- **Side Effects**:
  - Increments `NextBlackHoleId`
  - Initializes `BlackHoleParent[id] = id` (union-find root)
  - Sets `BlackHoleMass[id] = initialMass`

---

### SimContext.cs

**Purpose**: Holds global simulation state that changes each tick (context variables).

**Structure**:
```csharp
public sealed class SimContext
{
	public readonly SimConfig Config;  // Configuration parameters (immutable)
	public float NGlobal;              // Global energy pool (mutable)
	public float ScaleFactor;          // Spatial scale factor (mutable)
	public int Tick;                   // Current simulation tick (mutable)
}
```

#### Constructor
```csharp
public SimContext(SimConfig config, float initialNGlobal, float initialScale)
```


**Purpose**: Initializes context with configuration and initial values

#### Fields

##### `SimConfig Config`
- **Type**: `readonly` reference to configuration
- **Purpose**: Provides access to all simulation parameters
- **Mutability**: Config itself is immutable; this is a readonly reference

##### `float NGlobal`
- **Purpose**: Global energy pool for activation costs
- **Mutability**: Modified by Pass3 (replenishment) and Pass2 (activation)
- **Range**: [0, Config.NGlobalMax]

##### `float ScaleFactor`
- **Purpose**: Spatial expansion factor (currently unused)
- **Future Use**: Could scale grid dynamically

##### `int Tick`
- **Purpose**: Current simulation tick counter
- **Mutability**: Incremented by SimulationEngine each tick
- **Usage**: Time-based logic, arrival tracking, diagnostics

---

### SimConfig.cs

**Purpose**: Immutable configuration container for all simulation parameters.

**Structure**: Plain C# class with public fields (no properties needed - treated as data transfer object)

#### Parameter Categories

##### Global Budget
- `float NGlobalMax`: Maximum global energy capacity
- `float GlobalReplenishPerTick`: Energy added to global pool per tick
- `float MinEnergyForPersistence`: Energy threshold for persistence configurations

##### Viability / Threshold
- `float EthreshBase`: Base energy threshold
- `float GlobalScarcityK`: Scarcity adjustment factor
- `float EntropyPenalty`: Entropy penalty (currently unused)
- `float DecayLoss`: Baseline energy decay per tick

##### Propagation
- `float PropagateFrac`: Fraction of energy sent per tick
- `float MinBudgetToPropagate`: Minimum energy to propagate
- `float ActivationCost`: Global energy cost to activate cell

##### Entropy Dynamics
- `float EntropyGainPerUse`: Entropy gain per active use
- `float EntropyDiffuseRate`: Diffusion rate (Laplacian coefficient)
- `float EntropyDecay`: Entropy decay per tick
- `float EntropyGainFromGradient`: Entropy gain from energy gradients
- `float EntropyGainNearBH`: Entropy gain near black holes
- `float EntropyViabilityGainA`: Entropy boost to viability (A parameter)
- `float EntropyViabilityGainK`: Entropy boost to viability (K parameter)

##### Local Limits
- `float NlocalMax`: Maximum energy per cell
- `float VacuumEventProbability`: Probability of random vacuum event
- `float VacuumEventEntropy`: Entropy spike from vacuum event
- `float ExpansionRate`: Spatial expansion rate (currently unused)

##### Black Holes
- `float BlackHoleFormThreshold`: Charge threshold for BH formation
- `float BlackHoleDrainFrac`: Energy drained per tick (currently disabled)
- `float BlackHoleRecoilFrac`: Energy recoil fraction (Hawking-like radiation)

##### Field Propagation
- `float FieldAdvanceChance`: Probability of field expansion per tick
- `float FieldAdvanceCost`: Energy cost to activate configuration space
- `float FieldAdvanceMinSource`: Min energy in source to expand field
- `bool FieldAdvanceRequiresViability`: Require source viability for expansion
- `bool FieldAdvanceSeedsEnergy`: Seed energy when field expands
- `float FieldSeedEnergy`: Energy seeded at new field locations

##### Visualization
- `Color VoidColor`: Color for void regions
- `Color NullspaceColor`: Color for inactive field
- `bool ShowEntropyTint`: Enable entropy color tinting
- `float ViabilityColorScale`: Viability display multiplier

## Design Patterns

### Value Objects
- `GridState`, `SimContext`, `SimConfig` are value-semantic objects
- No behavior beyond data storage and basic utilities
- Enables easy serialization, testing, and debugging

### Separation of Concerns
- State (GridState) separate from context (SimContext) separate from config (SimConfig)
- Clear ownership: GridState = per-cell, SimContext = per-tick, SimConfig = immutable

### Data-Oriented Design
- Flat arrays (Structure of Arrays) for cache efficiency
- Per-cell data stored in separate arrays, not as Cell objects
- Better CPU cache locality for grid iteration

## Integration Points

### Used By
- **Simulation**: SimulationEngine, LegacyTickStep
- **Events**: Pass1, Pass2, Pass3, Pass4, BlackHoles, FieldWave
- **Core**: SimulationController
- **Unity**: GridRenderer

### Dependencies
- **None** - pure C# with only System and Unity.Mathematics (for Color)
- Can be tested independently of Unity

## Memory Layout

### Cache Efficiency
- Per-cell arrays stored contiguously:

```csharp
 Nlocal:  [cell0, cell1, cell2, ...]
 Entropy: [cell0, cell1, cell2, ...]
 V:       [cell0, cell1, cell2, ...]
```
**Benefits**:
- Sequential access patterns
- Better CPU cache utilization
- SIMD-friendly layout

### Alternative (Worse for Performance)

```csharp
 cells: [Cell{N, E, V, ...},
 Cell{N, E, V, ...}, ...]
```

**Problems**:
- Random memory layout
- Cache misses when accessing specific fields
- Harder to vectorize

## Serialization Support

All three classes can be easily serialized:
```csharp
// JSON serialization string json = JsonUtility.ToJson(config); SimConfig loaded = JsonUtility.FromJson<SimConfig>(json);
// Binary serialization // (requires [Serializable] attribute)
```

## Testing Strategy

### Unit Testing GridState
```csharp
[Test] public void GridState_Idx_ConvertsCoordinatesCorrectly() { var state = new GridState(10, 10); int idx = state.Idx(3, 5); Assert.AreEqual(5 * 10 + 3, idx); }
[Test] public void GridState_Reset_ClearsAllArrays() { var state = new GridState(10, 10); state.Nlocal[0] = 100f; state.Reset(); Assert.AreEqual(0f, state.Nlocal[0]); }
```
### Unit Testing SimContext
```csharp
[Test] public void SimContext_Tick_StartsAtZero() { var config = new SimConfig(); var ctx = new SimContext(config, 1000f, 1f); Assert.AreEqual(0, ctx.Tick); }
```

## Code Quality Notes

- **Immutability**: Config is immutable by convention (no setters)
- **Clear Naming**: Field names self-documenting
- **No Magic Numbers**: All thresholds externalized to Config
- **Memory Efficiency**: Struct-of-arrays layout
- **Type Safety**: Strong typing prevents mistakes
