# Simulation Module

This module defines the simulation engine architecture, orchestrating the execution of simulation steps in a pure, Unity-independent manner.

## Overview

The Simulation module implements a step-based architecture for executing complex multi-pass simulation logic. It provides the framework for tick-based updates, allowing different simulation strategies to be plugged in without modifying the core engine.

## Files

### SimulationEngine.cs

**Purpose**: Core orchestrator that executes simulation steps in sequence each tick.

**Key Responsibilities**:
- Tick-based simulation execution
- Step sequencing and coordination
- Context management (passing SimContext between steps)

#### Constructor
public SimulationEngine(ISimulationStep[] steps)


**Parameters**:
- `initialState`: Reference to the simulation state (modified in-place)
- `steps`: Array of simulation steps to execute each tick

**Purpose**: Initializes engine with state and step sequence

#### Core Methods

##### `void Tick(SimContext ctx)`

**Parameters**:
- `ctx`: Simulation context (contains tick counter, global energy, etc.)

**Purpose**: Executes one complete simulation tick

**Logic**:
1. Increments `ctx.Tick` counter
2. Iterates through all steps in `_steps` array
3. Calls `step.Execute(state, ctx)` for each step
4. Steps execute in order (Pass1 ? Pass2 ? Pass3 ? Pass4 ? Post-processing)

**Design Notes**:
- Steps share same `GridState` reference (modified in-place for performance)
- Steps are stateless - all state lives in `GridState` or `SimContext`
- No return value - effects are side-effects on state

#### Private Fields

##### `readonly GridState _state`
- Simulation grid state (per-cell arrays)
- Modified by steps during execution

##### `readonly ISimStep[] _steps`
- Ordered array of simulation steps
- Executed sequentially each tick

---

### ISimStep.cs

**Purpose**: Interface defining the contract for simulation steps.

**Interface Definition**:
public interface ISimStep { void Execute(GridState state, SimContext ctx); }


**Purpose**: Abstraction for any simulation logic that operates on grid state

**Design Pattern**: Strategy Pattern
- Different implementations can be swapped at runtime
- Engine doesn't need to know step implementation details

**Contract**:
- Must be stateless (no instance fields for simulation data)
- Must modify `state` in-place
- Can read/write `ctx` for global simulation parameters
- Must be side-effect-only (no return value)

---

### LegacyTickStep.cs

**Purpose**: Concrete implementation of ISimStep that wraps the original 4-pass simulation logic.

**Key Responsibilities**:
- Executes Pass1 (energy propagation)
- Executes Pass2 (energy application & viability)
- Executes Pass3 (global energy replenishment)
- Executes Pass4 (entropy diffusion)
- Executes post-processing callbacks (field wave, black hole growth)

#### Constructor
public LegacyTickStep( Func<float, float, float, float> viabilityFunc, Func<int, int> persistenceFunc, Action postProcessing)


**Parameters**:
- `viabilityFunc`: Computes viability from (inflow, local energy, entropy)
- `persistenceFunc`: Counts persistence configurations for a cell index
- `postProcessing`: Callback for field wave propagation & BH growth

**Purpose**: Injects dependencies from SimulationController into step logic

**Design**: Dependency Injection via Constructor
- Allows controller to maintain ownership of viability/persistence logic
- Enables testing with mock functions

#### Core Methods

##### `void Execute(GridState state, SimContext ctx)`

**Parameters**:
- `state`: Grid state to modify
- `ctx`: Simulation context

**Purpose**: Executes the complete 4-pass + post-processing simulation tick

**Execution Sequence**:

1. **Pass 1: Energy Outflow** (`Pass1.GatherOutflow`)
   - Computes energy propagation from active cells
   - Applies black hole attraction (mass-weighted)
   - Handles boundary void leakage ? black hole formation
   - Reflects blocked energy back to source
   - **Output**: `state.Incoming[]` filled with energy destined for each cell

2. **Pass 1.5: Energy Inflow** (`Pass1.GatherInflow`)
   - Adds continuous energy pulse to central seed region
   - Ensures seed remains active
   - **Output**: Additional energy added to `state.Incoming[]`

3. **Pass 2: Apply Energy & Compute Viability** (`Pass2.ApplyAndViability`)
   - Applies `Incoming[]` to `Nlocal[]`
   - Computes entropy from configuration count + gradients
   - Computes viability using injected `_viabilityFunc`
   - Updates `Active[]` state based on viability
   - Applies baseline decay
   - **Output**: Updated `Nlocal[]`, `Entropy[]`, `V[]`, `Active[]`

4. **Pass 3: Global Energy Replenishment** (`Pass3.GlobalRecharge`)
   - Replenishes global energy pool
   - **Output**: Updated `ctx.NGlobal`

5. **Pass 4: Entropy Diffusion** (`Pass4.EntropyDiffuse`)
   - Diffuses entropy using Laplacian operator
   - Applies entropy decay
   - **Output**: Smoothed `Entropy[]`

6. **Post-Processing** (Injected callback)
   - Field wave propagation (configuration space expansion)
   - Black hole growth (geometric collapse propagation)
   - **Output**: Updated `FieldPresent[]`, `IsBlackHole[]`, merged BH entities

#### Private Fields

##### `readonly Func<float, float, float, float> _viabilityFunc`
- Delegate for viability computation
- Signature: `(inflow, localEnergy, entropy) ? viability`
- Injected from SimulationController

##### `readonly Func<int, int> _persistenceFunc`
- Delegate for persistence configuration counting
- Signature: `(cellIndex) ? configCount`
- Injected from SimulationController

##### `readonly Action _postProcessing`
- Delegate for post-processing operations
- Executes field wave and black hole growth
- Injected from SimulationController

## Design Patterns

### Strategy Pattern (ISimStep)
- Defines family of algorithms (simulation steps)
- Makes them interchangeable
- Enables runtime step swapping

### Template Method (SimulationEngine)
- Defines skeleton of tick execution
- Delegates implementation to steps
- Fixed sequence, variable step implementations

### Dependency Injection (LegacyTickStep)
- Dependencies passed via constructor
- Enables testing with mocks
- Decouples step logic from controller

### Command Pattern (Implicit)
- Each step is a command object
- Encapsulates simulation operation
- Can be queued, logged, or replayed

## Integration Points

### Dependencies
- **Domain**: `GridState`, `SimContext`, `SimConfig`
- **Events**: `Pass1`, `Pass2`, `Pass3`, `Pass4`, `BlackHoles`, `FieldWave`

### Used By
- **SimulationController**: Creates engine, calls `Tick()` each frame

### Communication Flow
SimulationController.TickSimulation() ? SimulationEngine.Tick(ctx) ? LegacyTickStep.Execute(state, ctx) ? Pass1.GatherOutflow() ? Pass1.GatherInflow() ? Pass2.ApplyAndViability() ? Pass3.GlobalRecharge() ? Pass4.EntropyDiffuse() ? PostProcessing()


## Performance Considerations

### Per-Tick Operations
- **Pass1**: O(N) grid iteration + O(N) BH attraction field computation = O(2N)
- **Pass2**: O(N) grid iteration
- **Pass3**: O(1) global energy update
- **Pass4**: O(N) grid iteration + O(N) double-buffer copy = O(2N)
- **Post-Processing**: O(N) field wave + O(N) BH growth = O(2N)
- **Total**: O(8N) where N = width × height

### Memory Allocation
- **Zero Allocation per Tick**: All arrays pre-allocated in GridState
- **Function Delegates**: Cached in constructor, no allocation
- **Step Array**: Fixed-size, no reallocation

### Optimization Opportunities
1. **Parallel Steps**: Pass1-4 could potentially run in parallel if dependencies resolved
2. **SIMD Operations**: Vectorize Pass4 Laplacian computation
3. **Compute Shaders**: Offload entire tick to GPU
4. **Incremental Updates**: Only process "dirty" regions with recent changes

## Future Enhancement Opportunities

1. **Multi-Step Support**: Allow multiple step implementations (e.g., different physics models)
2. **Step Dependencies**: DAG-based execution for parallel step processing
3. **Step Profiling**: Built-in performance measurement per step
4. **Step Checkpointing**: Save/load state between specific steps
5. **Step Debugging**: Step-through debugger with state inspection
6. **Hot-Swapping**: Change step implementations at runtime without restart

## Testing Strategy

### Unit Testing Steps

[Test] public void LegacyTickStep_Execute_CallsAllPasses() { var state = new GridState(10, 10); var ctx = new SimContext(config, 1000f, 1f); var viabilityMock = (float i, float n, float e) => 0.5f; var persistenceMock = (int idx) => 8; bool postProcessingCalled = false; var postProcessingMock = () => postProcessingCalled = true;
var step = new LegacyTickStep(viabilityMock, persistenceMock, postProcessingMock);

step.Execute(state, ctx);

Assert.IsTrue(postProcessingCalled);
// ... additional assertions
}


### Integration Testing
[Test] public void SimulationEngine_Tick_IncrementsContext() { var state = new GridState(10, 10); var ctx = new SimContext(config, 1000f, 1f); var step = new LegacyTickStep(...); var engine = new SimulationEngine(state, new[] { step });
int initialTick = ctx.Tick;
engine.Tick(ctx);

Assert.AreEqual(initialTick + 1, ctx.Tick);
}


## Code Quality Notes

- **Pure Functions**: Steps are stateless, operate only on parameters
- **Clear Separation**: Engine doesn't know pass implementation details
- **Testable Design**: Dependency injection enables easy mocking
- **Performance-Oriented**: Zero allocation per tick, in-place modifications
- **Extensible**: New steps can be added without modifying engine
- **Documentation**: Execution sequence clearly documented in comments