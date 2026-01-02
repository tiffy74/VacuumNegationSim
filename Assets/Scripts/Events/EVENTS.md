# Events Module

This module contains the core simulation logic organized into distinct passes and specialized physics systems.

## Overview

The Events module implements the "how" of the simulation: the algorithms that transform state each tick. It's organized into 4 main passes plus specialized systems for field propagation and black hole dynamics.

## Files

### Pass1.cs - Energy Propagation & Black Hole Attraction

**Purpose**: Handles energy flow between cells with mass-weighted black hole gravitational attraction.

**Key Methods**:

##### `int GatherOutflow(...)`
- **Purpose**: Main energy propagation with BH attraction
- **Logic**:
  1. Computes BH attraction field (mass-weighted, O(N))
  2. For each active cell:
     - Calculates weighted distribution to 4 neighbors
     - Cells near BHs receive more energy (stable dependency)
     - Energy blocked by BHs/voids reflects back
     - Boundary void leakage accumulates BH charge
     - Creates BHs when charge exceeds threshold (after tick 10)
- **Returns**: Count of new black holes created
- **Key Features**:
  - Mass-proportional BH attraction (F ? M analogue)
  - Charge-based BH formation (prevents instant collapse)
  - Stable dependency pressure buildup
  - Union-find BH merging

##### `void GatherInflow(...)`
- **Purpose**: Adds continuous energy to central seed region
- **Logic**: Exponentially decaying pulse to 5×5 center block
- **Formula**: `pulse = 0.1 * exp(-tick / 80)`

**Helper Methods**:
- `bool IsBoundaryVoid(...)`: Checks if void cell is adjacent to field
- `bool IsFieldBoundary(...)`: Checks if field cell has void neighbors

---

### Pass2.cs - Energy Application & Viability

**Purpose**: Applies incoming energy, computes viability and entropy, enforces geometric constraints.

**Key Method**:

##### `void ApplyAndViability(...)`
- **Purpose**: Main pass for energy application and state updates
- **Logic** (per cell):
  1. **Constraint Enforcement**:
     - Black holes: N=0, S=1, V=0, Active=0
     - No field: N=0, V=0, Active=0
     - Vacuum: In=0, V=0, Active=0
  2. **Energy Application**: `Nlocal += Incoming`
  3. **Activation Cost**: Draw from global pool when cell activates
  4. **Entropy Computation**:
     - Configuration entropy: `log(1 + persistenceConfigs) / log(17)`
     - Gradient entropy: `grad / (grad + 1)`
     - Combined: `0.7 * config + 0.3 * grad`
     - Activity-scaled gain
  5. **Vacuum Events**: Rare stochastic collapse
  6. **Viability Computation**: Calls injected `ComputeViability(inflow, N, S)`
  7. **Active State Update**: `Active = (V > 0 && N > minBudget)`
  8. **Baseline Decay**: `N -= DecayLoss`
  9. **Zero Energy Tracking**: Increment counter if N=0

**Entropy Philosophy**:
- Configuration entropy: Information-theoretic (# of sustainable states)
- Gradient entropy: Spatial organization measure
- Higher entropy BOOSTS viability (resilience through complexity)

---

### Pass3.cs - Global Energy Replenishment

**Purpose**: Replenishes global energy pool at constant rate.

**Key Method**:

##### `void GlobalRecharge(ref float NGlobal, float NGlobalMax, float GlobalReplenishPerTick)`
- **Purpose**: Adds energy to global pool, capped at max
- **Formula**: `NGlobal = min(NGlobalMax, NGlobal + replenishRate)`
- **Physical Analogue**: Vacuum energy / cosmological constant

---

### Pass4.cs - Entropy Diffusion

**Purpose**: Smooths entropy gradients across grid using discrete Laplacian.

**Key Method**:

##### `void EntropyDiffuse(...)`
- **Purpose**: Implements heat-equation-style entropy diffusion
- **Logic**:
  1. For each cell (non-BH):
     - Sample 4 neighbors (Neumann boundary conditions)
     - Compute discrete Laplacian: `?²S = (N+S+E+W - 4*C)`
     - Apply diffusion: `S_new = S + rate * ?²S`
     - Apply decay: `S_new -= decay`
     - Clamp to [0, 1]
  2. Black holes: Fixed `S = 1` (maximum entropy)
  3. Commit double-buffered state
- **Physical Analogue**: Thermodynamic smoothing (2nd law)
- **Mathematical Form**: `?S/?t = k?²S - ?S`

---

### BlackHoles.cs - Black Hole Dynamics

**Purpose**: Manages BH formation, growth, merging, and energy interactions.

**Key Methods**:

##### `void GrowBlackHoles(GridState s)`
- **Purpose**: Expands BH regions via geometric trapping
- **Logic**:
  1. Mark cells with 2+ BH neighbors for conversion
  2. Apply conversions
  3. Assign/merge IDs for new BH cells via union-find
- **Physical Analogue**: Event horizon expansion

##### `int AssignOrMergeAtCell(...)`
- **Purpose**: Assigns BH entity ID, merges with adjacent BHs
- **Logic**: Union-find algorithm with mass accumulation
- **Returns**: Root ID of (possibly merged) BH entity

##### `float BlackHoleAttractEnergy(...)`
- **Purpose**: Drains energy from cells adjacent to BHs
- **Logic**:
  1. For each BH cell: drain `absorbFrac` from neighbors
  2. Add drained energy to BH mass
  3. Optional: apply recoil (Hawking-like radiation)
- **Returns**: Total energy drained

##### `void ComputePotential(...)`
- **Purpose**: Calculates gravitational potential field
- **Logic**: Spreads influence using `1/(1+dist)` kernel
- **Usage**: Could bias energy flow (currently unused in main propagation)

**Helper Methods**:
- `int Find(...)`: Union-find root finding with path compression
- `int Union(...)`: Merges two BH entities, combines masses
- `int GetRootAtCell(...)`: Gets root BH ID for a cell

---

### FieldWave.cs - Configuration Space Propagation

**Purpose**: Manages expansion of active configuration space (FieldPresent) at boundaries.

**Key Method**:

##### `void PropagateFieldWave(...)`
- **Purpose**: Expands field at energy-void boundaries
- **Logic**:
  1. **Phase 1**: Maintain event horizons around BHs
     - Keep field if: has energy OR adjacent to BH OR adjacent to field
     - Clear field if isolated
  2. **Phase 2**: Expand at boundaries
     - Identify boundary cells (adjacent to both field AND void)
     - For each boundary cell:
       - Find viable neighbor with sufficient energy
       - Probabilistic expansion (FieldAdvanceChance)
       - Neighbor pays energy cost
       - Mark field arrival tick
       - Optional: seed minimal energy

**Physical Analogue**: Spacetime geometry becoming "observable" where it mediates interactions

**Design Philosophy**:
- Configuration space pervades everywhere (like spacetime)
- FieldPresent marks where it's "active" (analogous to curved spacetime)
- Black holes = collapsed config space (event horizons)
- Field boundaries = regions where geometry is being "activated"

## Cross-Cutting Concerns

### Energy Conservation
- Pass1: Energy sent = energy deducted (+ reflected energy)
- Pass2: Energy applied = energy removed from Incoming buffer
- Pass3: Global pool capped at max
- No energy created except via GatherInflow (seed pulse)

### Geometric Constraints
- Black holes enforce: No energy, max entropy, inactive
- No field enforces: No energy, no activity
- Vacuum enforces: No incoming energy

### Union-Find Efficiency
- BlackHoles.cs uses path compression for O(?(N)) amortized
- Merges by ID stability (lower ID becomes root)
- Mass accumulation during Union

### Performance Characteristics
- **Pass1**: O(N) + O(N) for BH field = O(2N)
- **Pass2**: O(N)
- **Pass3**: O(1)
- **Pass4**: O(N) + O(N) for copy = O(2N)
- **BlackHoles.GrowBlackHoles**: O(N) mark + O(N) assign = O(2N)
- **FieldWave**: O(N)
- **Total per Tick**: O(8N)

## Testing Recommendations
```Csharp
[Test] public void Pass1_EnergyConservation() { // Setup grid with energy float totalBefore = SumEnergy(state);
Pass1.GatherOutflow(...);
Pass2.ApplyAndViability(...);

float totalAfter = SumEnergy(state) + state.NGlobal;
Assert.AreApproximatelyEqual(totalBefore, totalAfter, 1e-3f);
}
[Test] public void BlackHoles_MergeAdjacent() { state.IsBlackHole[0] = true; state.IsBlackHole[1] = true; // Adjacent cells
BlackHoles.AssignOrMergeAtCell(0, ...);
BlackHoles.AssignOrMergeAtCell(1, ...);

int root0 = BlackHoles.GetRootAtCell(0, ...);
int root1 = BlackHoles.GetRootAtCell(1, ...);

Assert.AreEqual(root0, root1); // Same root = merged
}
```


## Code Quality Notes

- **Static Classes**: All passes are stateless static classes
- **Pure Functions**: No side effects except on passed-in state
- **Descriptive Names**: Method names explain purpose
- **Separation**: Each pass has single responsibility
- **Documentation**: XML comments on public APIs
- **Performance**: Zero allocation per tick, in-place modifications
- 