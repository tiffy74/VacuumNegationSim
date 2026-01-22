# Example Presets - Quick Start Guide

**Purpose:** ?? **USER-FRIENDLY STARTING POINTS**

These presets demonstrate different emergent behaviors in domain-neutral terms. Pick one, load it, and explore!

---

## ?? **Available Presets**

### **1. Balanced Persistence** ??
**File:** `01_BalancedPersistence.asset`

**Theme:** Stable propagation with minimal collapse

**What to expect:**
- Steady expansion from central seed
- Stable yellow frontier propagates outward
- Few magenta sinks form (low stress)
- Final state: Large stable region with occasional sinks at boundaries

**Parameters:**
- Moderate decay (0.003)
- Balanced replenishment (200/tick)
- Standard expansion chance (0.25)
- Grid: 128×128
- Seed: 42 (reproducible)

**Good for:**
- **Learning the system** - Demonstrates all basic mechanics clearly
- **Baseline comparisons** - Compare other presets against this
- **Understanding viability** - See how cells persist or fail
- **Export testing** - Generate clean CSV data

**Duration:** ~200 ticks for full propagation

---

### **2. Resource Stress** ??
**File:** `02_ResourceStress.asset`

**Theme:** High decay creates cascading failures

**What to expect:**
- Rapid initial expansion (yellow frontier moves fast)
- Early collapses (regions lose viability and fail)
- Multiple magenta sinks form (resource accumulation points)
- Final state: Fragmented network with scattered surviving regions

**Parameters:**
- High decay (0.01 - 3× normal!)
- Low replenishment (100/tick)
- Standard expansion (0.25)
- Grid: 128×128
- Seed: 42

**Good for:**
- **Resilience studies** - What survives under stress?
- **Cascading failures** - Watch propagation of collapse
- **Network vulnerability** - Identify critical regions
- **Stress testing** - Push system to limits

**Duration:** ~150 ticks (faster collapse)

---

### **3. Rapid Expansion** ??
**File:** `03_RapidExpansion.asset`

**Theme:** Fast propagation with abundant resources

**What to expect:**
- Very fast frontier expansion (fills grid quickly)
- Large stable regions (high viability everywhere)
- Delayed sink formation (late-stage only)
- Final state: Nearly complete coverage, few sinks

**Parameters:**
- Very low decay (0.001 - 1/3 normal)
- High replenishment (500/tick)
- High expansion chance (0.5 - 2× normal)
- High propagation (0.35)
- Grid: 128×128
- Seed: 42

**Good for:**
- **Invasion dynamics** - Rapid colonization
- **Growth-dominated regimes** - When expansion > decay
- **Spatial spread** - Study propagation speed
- **Coverage problems** - How long to fill space?

**Duration:** ~100 ticks (fast!)

---

### **4. Competing Regions** ??
**File:** `04_CompetingRegions.asset`

**Theme:** Multiple seeds compete for shared resources

**What to expect:**
- Four initial seeds (corners of grid)
- Expanding regions collide at boundaries
- Competition for global resource pool
- Final state: Coexistence or winner-take-all?

**Parameters:**
- Moderate decay (0.003)
- Limited replenishment (150/tick - creates scarcity)
- Standard expansion (0.25)
- Grid: 128×128
- Seed: 42
- **Special:** 4 initial seeds instead of 1

**Good for:**
- **Multi-agent competition** - Territory formation
- **Resource partitioning** - How do regions share?
- **Spatial games** - Competitive dynamics
- **Coexistence** - Can multiple regions survive?

**Duration:** ~250 ticks (longer due to competition)

---

### **5. Stochastic Dynamics** ??
**File:** `05_StochasticDynamics.asset`

**Theme:** High perturbation creates unpredictable patterns

**What to expect:**
- Chaotic-looking spatial patterns
- Unpredictable local collapses
- Seed-dependent outcomes (deterministic chaos!)
- Numerically reproducible (same seed = same result)

**Parameters:**
- Moderate decay (0.003)
- Moderate replenishment (200/tick)
- **High perturbation** (0.002 - 10× normal!)
- Standard expansion (0.25)
- Grid: 128×128
- Seed: 42

**Good for:**
- **Noise effects** - How does randomness affect outcomes?
- **Sensitivity analysis** - Small perturbations ? large effects?
- **Stochastic modeling** - Exploring probabilistic dynamics
- **Deterministic chaos** - Reproducible but unpredictable-looking

**Duration:** ~200 ticks

**Note:** Visually looks random, but it's 100% deterministic! Same seed = identical pattern.

---

## ?? **How to Use**

### **Load a Preset:**
1. **In Unity:** Select SimulationManager GameObject
2. **In Inspector:** Find SimulationController component
3. **Scenario Preset field:** Drag a preset from `Presets/Examples/`
4. **Press Play** ??

### **Export Results:**
1. **Run simulation** (~100-250 ticks depending on preset)
2. **Right-click** SimulationController component
3. **Click** "Export Current Run"
4. **Analyze** CSV data in Excel/R/Python

### **Modify Parameters:**
1. **Load preset** as starting point
2. **Enable** "Allow Inspector Override"
3. **Adjust** parameters in Inspector
4. **Press Play** to see effects

---

## ?? **Preset Comparison**

| Preset | Decay | Replenish | Expansion | Grid | Expected Outcome |
|--------|-------|-----------|-----------|------|------------------|
| **Balanced** | 0.003 | 200 | 0.25 | 128² | Stable growth |
| **Stress** | 0.01 | 100 | 0.25 | 128² | Fragmented |
| **Rapid** | 0.001 | 500 | 0.5 | 128² | Full coverage |
| **Competing** | 0.003 | 150 | 0.25 | 128² | Multi-region |
| **Stochastic** | 0.003 | 200 | 0.25 | 128² | Chaotic patterns |

---

## ?? **Suggested Experiments**

### **Experiment 1: Parameter Sensitivity**
1. Load **Balanced Persistence**
2. Export baseline
3. Increase decay to 0.006 (2×)
4. Export again
5. Compare: How does doubling decay affect coverage?

### **Experiment 2: Reproducibility**
1. Load **Stochastic Dynamics**
2. Run to tick 200, export
3. Restart, run again to tick 200, export
4. Compare checksums ? Should be IDENTICAL!

### **Experiment 3: Resilience**
1. Load **Resource Stress**
2. Identify which regions survive
3. Why? Check resource availability, sink proximity
4. Can you predict survival from initial conditions?

---

## ?? **Creating Your Own Presets**

1. **Start with an example** (copy one)
2. **Adjust parameters** (see parameter guide below)
3. **Test behavior** (does it do what you expect?)
4. **Document it** (description, use cases)
5. **Export verification** (confirm reproducibility)

### **Key Parameters to Tune:**

**For stability:**
- Decrease `decayLoss`
- Increase `globalReplenishPerTick`

**For stress:**
- Increase `decayLoss`
- Decrease `globalReplenishPerTick`

**For speed:**
- Increase `regionExpansionChance`
- Increase `propagateFrac`

**For chaos:**
- Increase `perturbationProbability`

---

## ?? **Common Pitfalls**

### **"Nothing happens!"**
- Check seed is set (or uncheck for random)
- Verify replenishment > 0
- Ensure expansion chance > 0

### **"Instant collapse!"**
- Decay too high relative to replenishment
- Try **Balanced Persistence** first

### **"It looks the same every time!"**
- That's correct! Determinism is the goal
- Change seed for different initial conditions
- Or uncheck seed box for random behavior

---

## ?? **Learning Path**

**New users:**
1. **Balanced Persistence** ? Learn basics
2. **Rapid Expansion** ? See growth-dominated regime
3. **Resource Stress** ? Understand failure

**Research users:**
1. **Competing Regions** ? Multi-agent dynamics
2. **Stochastic Dynamics** ? Noise effects
3. **Custom preset** ? Your specific question

---

## ?? **Expected Performance**

All presets run efficiently:
- **128×128 grid:** ~5-10 seconds per 100 ticks
- **Export time:** <1 second
- **Memory:** <500 MB

---

## ?? **Contributing**

Have a preset idea? Consider submitting!

**Good preset criteria:**
- Demonstrates unique behavior
- Domain-neutral description
- Clear use cases
- Well-documented parameters

---

**Ready to explore!** Pick a preset and press Play. ??

