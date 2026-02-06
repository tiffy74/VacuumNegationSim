# ?? CRITICAL: Fix Neighbor Connectivity for Triangular/Hexagonal Grids

## **Problem Identified** ?

**You're absolutely right!** The visuals changed but the simulation logic is still using **4 neighbors** (rectangular grid) instead of **6 neighbors** for triangular and hexagonal grids.

### **Current Behavior:**
```
Visual:  ? ? ? ? ?  (Triangular sprites)
Logic:   4 neighbors (N, S, E, W only)
Result:  MISMATCH! ?
```

### **Expected Behavior:**
```
Visual:  ? ? ? ? ?  (Triangular sprites)
Logic:   6 neighbors (hexagonal vertex lattice)
Result:  MATCH! ?
```

---

## **?? Where the Problem Is**

The neighbor calculations are hardcoded in multiple places:

| File | Location | Current Code |
|------|----------|--------------|
| **SimulationStepper.cs** | `ExpandSinkRegions()` | `int[] dx = { 0, 0, -1, 1 };` |
| **OutflowPhase.cs** | Neighbor loops | 4-direction offsets |
| **DiffusionPhase.cs** | Complexity diffusion | 4-direction offsets |
| **InflowPhase.cs** | Resource propagation | 4-direction offsets |
| **SimulationController.cs** | `CountPersistenceConfigurations()` | `int[] dx = { 0, 0, -1, 1 };` |

**All of these are hardcoded to 4 neighbors (Von Neumann neighborhood).**

---

## **?? Neighbor Systems for Each Topology**

### **Rectangular (Current - 4 neighbors):**
```
     N
     ?
W ?? ? ?? E
     ?
     S

dx = { 0, 0, -1, 1 }
dy = {-1, 1,  0, 0 }

4 neighbors total
```

---

### **Triangular (NEW - 6 neighbors):**
```
Triangular grid is the DUAL of hexagonal grid:
- Each triangle vertex connects to 6 triangles
- Forms hexagonal vertex lattice

    ?
   ? ?
  ?   ?
 ? ? ? ?
?   ?   ?
 ? ? ? ?
  ?   ?
   ? ?
    ?

Neighbor offsets (offset coordinates):
Even rows (y % 2 == 0):
dx = {-1, 0, 1,  1,  0, -1}
dy = {-1,-1,-1,  0,  1,  1}

Odd rows (y % 2 == 1):
dx = {-1, 0, 1,  1,  0, -1}
dy = { 0, 0, 0,  1,  1,  1}

6 neighbors total
```

---

### **Hexagonal (NEW - 6 neighbors):**
```
Flat-top hexagonal grid:
  ___
 /   \___
 \___/   \
 /   \___/
 \___/

Neighbor offsets (offset coordinates):
Even rows (y % 2 == 0):
dx = {-1, 0, 1,  0, -1,  1}
dy = { 0,-1, 0,  1,  1,  1}

Odd rows (y % 2 == 1):
dx = {-1, 0, 1,  0,  1, -1}
dy = {-1,-1,-1,  0,  0,  0}

6 neighbors total
```

---

## **??? Implementation Strategy**

This is a **significant** change that touches the **core engine**. Here's the approach:

### **Option A: Quick Fix (Minimal Changes)**
- Create a `NeighborProvider` helper class
- Pass topology to engine phases
- Update neighbor loops to use provider

**Pros:** Smaller change, easier to test  
**Cons:** Still hardcoded in places, not fully flexible  
**Time:** 2-3 days  

---

### **Option B: Proper Refactor (Recommended)**
- Add `TopologyMode` to `StepContext`
- Create `ITopologyProvider` interface
- Refactor all neighbor calculations to use interface
- Update all phases (Diffusion, Outflow, Inflow, Sink expansion)

**Pros:** Clean architecture, fully flexible, future-proof  
**Cons:** Larger change, more testing needed  
**Time:** 1-2 weeks  

---

## **?? Quick Fix Implementation (Option A)**

I recommend **Option A** for now to get triangular/hexagonal working quickly, then refactor later if needed.

### **Step 1: Create NeighborProvider Helper**

**File:** `Assets/Viable/Engine/Helpers/NeighborProvider.cs`

```csharp
using Viable.Contracts;
using System.Collections.Generic;

namespace Viable.Engine.Helpers
{
    /// <summary>
    /// Provides neighbor offsets for different grid topologies.
    /// Stage 13.7: Support for Rectangular, Triangular, and Hexagonal grids.
    /// </summary>
    public static class NeighborProvider
    {
        /// <summary>
        /// Get neighbor count for topology.
        /// </summary>
        public static int GetNeighborCount(TopologyMode topology)
        {
            switch (topology)
            {
                case TopologyMode.RectGrid:
                    return 4;
                case TopologyMode.TriGrid:
                case TopologyMode.HexGrid:
                    return 6;
                default:
                    return 4;
            }
        }

        /// <summary>
        /// Get neighbor offsets for cell at (x, y) with given topology.
        /// Returns arrays of dx, dy offsets.
        /// </summary>
        public static void GetNeighborOffsets(int x, int y, TopologyMode topology, out int[] dx, out int[] dy)
        {
            switch (topology)
            {
                case TopologyMode.RectGrid:
                    // Von Neumann 4-neighborhood
                    dx = new int[] { 0, 0, -1, 1 };
                    dy = new int[] { -1, 1, 0, 0 };
                    break;

                case TopologyMode.TriGrid:
                    // Triangular grid (hexagonal vertex lattice)
                    // Offset coordinates depend on row parity
                    if (y % 2 == 0)
                    {
                        // Even row
                        dx = new int[] { -1, 0, 1, 1, 0, -1 };
                        dy = new int[] { -1, -1, -1, 0, 1, 1 };
                    }
                    else
                    {
                        // Odd row
                        dx = new int[] { -1, 0, 1, 1, 0, -1 };
                        dy = new int[] { 0, 0, 0, 1, 1, 1 };
                    }
                    break;

                case TopologyMode.HexGrid:
                    // Hexagonal grid (flat-top)
                    // Offset coordinates depend on row parity
                    if (y % 2 == 0)
                    {
                        // Even row
                        dx = new int[] { -1, 0, 1, 0, -1, 1 };
                        dy = new int[] { 0, -1, 0, 1, 1, 1 };
                    }
                    else
                    {
                        // Odd row
                        dx = new int[] { -1, 0, 1, 0, 1, -1 };
                        dy = new int[] { -1, -1, -1, 0, 0, 0 };
                    }
                    break;

                default:
                    // Fallback to rectangular
                    dx = new int[] { 0, 0, -1, 1 };
                    dy = new int[] { -1, 1, 0, 0 };
                    break;
            }
        }

        /// <summary>
        /// Iterate over neighbors of cell (x, y) and call action for each valid neighbor.
        /// </summary>
        public static void ForEachNeighbor(
            int x, int y, int gridWidth, int gridHeight,
            TopologyMode topology,
            System.Action<int, int> action)
        {
            GetNeighborOffsets(x, y, topology, out int[] dx, out int[] dy);

            for (int i = 0; i < dx.Length; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                // Bounds check
                if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight)
                    continue;

                action(nx, ny);
            }
        }
    }
}
```

---

### **Step 2: Pass Topology Through Engine**

**Modify `StepContext.cs`** to include topology:

```csharp
public sealed class StepContext
{
    public SimulationConfiguration Config { get; }
    public float ResourceGlobal;
    public float ScaleFactor;
    public Random Rng { get; }
    public int Tick;
    public float DeltaTime => 1f;
    
    // NEW: Add topology
    public TopologyMode Topology { get; }

    public StepContext(
        SimulationConfiguration config,
        float resourceGlobal,
        float scaleFactor,
        int? seed,
        TopologyMode topology = TopologyMode.RectGrid) // NEW parameter
    {
        Config = config;
        ResourceGlobal = resourceGlobal;
        ScaleFactor = scaleFactor;
        Tick = 0;
        Topology = topology; // NEW
        Rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }
}
```

---

### **Step 3: Update SimulationController**

Pass topology when creating `StepContext`:

```csharp
// In InitializeSimulation()
context = new StepContext(
    config,
    resourceGlobal,
    scaleFactor,
    seed,
    topology // NEW: Pass topology
);
```

---

### **Step 4: Update Each Phase**

This is the big part - updating all neighbor loops. I'll show one example:

**Example: Update `SimulationStepper.ExpandSinkRegions()`**

```csharp
private void ExpandSinkRegions(GridState state, TopologyMode topology)
{
    bool[] nextSink = (bool[])state.IsSink.Clone();

    for (int y = 0; y < state.H; y++)
    {
        for (int x = 0; x < state.W; x++)
        {
            int i = state.Idx(x, y);
            if (state.IsSink[i]) continue;
            if (state.ActiveRegion[i]) continue;

            // NEW: Use NeighborProvider instead of hardcoded dx/dy
            int sinkNeighbors = 0;
            NeighborProvider.ForEachNeighbor(x, y, state.W, state.H, topology,
                (nx, ny) =>
                {
                    int ni = state.Idx(nx, ny);
                    if (state.IsSink[ni]) sinkNeighbors++;
                });

            // For hexagonal/triangular: need 4+ neighbors (not 3)
            int threshold = topology == TopologyMode.RectGrid ? 3 : 4;
            if (sinkNeighbors >= threshold)
            {
                nextSink[i] = true;
                SinkLogic.AssignOrMergeAtCell(i, state.W, state.H, nextSink, state.SinkId, state.SinkParent, state.SinkMass, ref state.NextSinkId);
            }
        }
    }

    state.IsSink = nextSink;
}
```

---

## **?? Files That Need Updates**

### **Core Files (Create New):**
- [ ] `Assets/Viable/Engine/Helpers/NeighborProvider.cs` (NEW)

### **Context Files (Modify):**
- [ ] `Assets/Viable/Engine/Execution/StepContext.cs` (add `Topology` property)
- [ ] `Assets/Viable/Core.Unity/Controllers/SimulationController.cs` (pass topology to context)

### **Phase Files (Modify - use `NeighborProvider`):**
- [ ] `Assets/Viable/Engine/SimulationStepper.cs` (`ExpandSinkRegions`)
- [ ] `Assets/Viable/Engine/Steps/DiffusionPhase.cs` (complexity diffusion)
- [ ] `Assets/Viable/Engine/Steps/OutflowPhase.cs` (resource propagation)
- [ ] `Assets/Viable/Engine/Steps/InflowPhase.cs` (if needed)
- [ ] `Assets/Viable/Core.Unity/Controllers/SimulationController.cs` (`CountPersistenceConfigurations`)

---

## **?? Testing Strategy**

After implementing:

1. **Test Rectangular:**
   - Run existing simulations
   - **Expected:** No change (4 neighbors, current behavior)
   - **Goal:** No regression

2. **Test Triangular:**
   - Set topology = TriGrid
   - **Expected:** Expansion rate ~50% faster (6 neighbors vs 4)
   - **Expected:** Hexagonal expansion pattern (not diamond)

3. **Test Hexagonal:**
   - Set topology = HexGrid
   - **Expected:** Circular expansion (isotropic)
   - **Expected:** No axis bias

---

## **?? Expected Behavior Changes**

| Metric | Rectangular (4) | Triangular (6) | Hexagonal (6) |
|--------|-----------------|----------------|---------------|
| **Neighbors** | 4 | 6 | 6 |
| **Expansion Rate** | ~4 cells/tick | ~6 cells/tick | ~6 cells/tick |
| **Diffusion Speed** | Slower | Faster | Faster |
| **Symmetry** | 4-fold | 6-fold | 6-fold |
| **Isotropy** | Anisotropic | Semi-isotropic | **Isotropic** |

---

## **?? Priority**

**This is a CRITICAL fix** because:
- ? Visuals changed (working!)
- ? Logic unchanged (broken!)
- ?? **Users will be confused** (triangles behaving like squares)

**Recommendation:** Implement **Quick Fix (Option A)** ASAP, then refactor later if needed.

---

## **?? Next Steps**

1. **I can implement the Quick Fix for you** if you want
   - Create `NeighborProvider.cs`
   - Update `StepContext.cs`
   - Update key phases (Diffusion, Outflow, Sink expansion)
   - Test with all 3 topologies

2. **Or you can do it yourself** using this guide
   - Follow step-by-step instructions
   - Test incrementally
   - Report any issues

**Which would you prefer?** ??

---

**Estimated Time:**
- Implementation: 2-4 hours
- Testing: 1-2 hours
- Total: ~1 day of work

**Complexity:** Medium (touches core engine, but pattern is consistent)
