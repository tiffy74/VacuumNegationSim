# Sink Formation System Design

**Status:** ?? **DESIGN DOCUMENT**  
**Goal:** Create flexible, domain-agnostic sink formation mechanisms

---

## ?? **Problem Statement**

Current sink formation:
- ? Single method: "forms at resource-rich boundaries"
- ? Theory-specific (analogous to gravitational collapse)
- ? `SinkFormationThreshold` not working as expected
- ? No control over sink count/distribution
- ? Doesn't match other domain models

**Need:** Multiple formation mechanisms for different domains

---

## ?? **Sink Formation Mechanisms**

### **Category 1: Threshold-Based (Current)**

#### **1A: Boundary Accumulation** ? CURRENT
**Theory:** Resource accumulates at expansion boundaries until threshold triggers collapse

**Parameters:**
- `sinkFormationThreshold`: Resource level to trigger
- `boundaryOnly`: Only form at active region boundaries

**Analogies:**
- Physics: Gravitational collapse at high-density points
- Networks: Congestion points in traffic flow
- Biology: Resource sinks in vascular systems

**Use when:** Modeling accumulation-driven collapse

---

#### **1B: Local Maximum**
**Theory:** Sinks form at local resource maxima when threshold exceeded

**Parameters:**
- `sinkFormationThreshold`: Resource level to trigger
- `localRadius`: Neighborhood size for "local" maximum
- `cooldownTicks`: Minimum time between formations

**Analogies:**
- Chemistry: Nucleation sites in crystallization
- Ecology: Predator concentration at prey hotspots
- Economics: Market concentration points

**Use when:** Modeling concentration-driven phenomena

---

#### **1C: Gradient Extrema**
**Theory:** Sinks form where resource gradients are steepest

**Parameters:**
- `gradientThreshold`: Steepness to trigger formation
- `requirePositiveFlow`: Must be converging (not diverging)

**Analogies:**
- Geology: Erosion sinks at steep terrain
- Fluid dynamics: Vortex formation
- Social: Opinion polarization points

**Use when:** Modeling flow-driven phenomena

---

### **Category 2: Count-Based (User Control)**

#### **2A: Fixed Count**
**Theory:** User specifies exact number, placed by algorithm

**Parameters:**
- `sinkCount`: Exact number to create
- `placementMethod`: Random, Maxima, Uniform, Custom
- `placementTick`: When to place (start, or specific tick)

**Analogies:**
- Network design: Fixed number of hubs
- Urban planning: Predetermined service centers
- Experimental: Controlled perturbation sites

**Use when:** Testing specific configurations, controlled experiments

---

#### **2B: Density-Based**
**Theory:** Sink density proportional to some metric (resource, complexity, etc.)

**Parameters:**
- `sinksPerUnit`: Density target (e.g., 1 per 1000 resource units)
- `metric`: What to base density on (resource, complexity, viability)
- `redistributionInterval`: How often to recompute

**Analogies:**
- Ecology: Predator-prey density relationships
- Chemistry: Catalyst concentration
- Sociology: Service provider density

**Use when:** Modeling density-dependent phenomena

---

### **Category 3: Stochastic (Probabilistic)**

#### **3A: Poisson Process**
**Theory:** Random formation with constant probability per cell per tick

**Parameters:**
- `formationProbability`: Base probability per tick
- `resourceModifier`: How resource level affects probability
- `spatialCorrelation`: Nearby sinks increase/decrease probability

**Analogies:**
- Radioactive decay (Poisson events)
- Mutation events in genetics
- Random failure in networks

**Use when:** Modeling random processes, exploring noise effects

---

#### **3B: Criticality-Triggered**
**Theory:** System-wide criticality triggers avalanche of formations

**Parameters:**
- `criticalityMetric`: What defines system criticality (e.g., avg resource)
- `criticalityThreshold`: Level that triggers avalanche
- `avalancheProbability`: Formation probability during avalanche

**Analogies:**
- Self-organized criticality (sandpile models)
- Cascading failures (power grids)
- Epidemic outbreak (critical density)

**Use when:** Modeling phase transitions, cascading phenomena

---

### **Category 4: Domain-Specific Models**

#### **4A: Black Hole (Astrophysics)**
**Current model:** Boundary accumulation (Schwarzschild radius analogue)

**Alternative:** Jeans instability
- Form when local mass exceeds Jeans mass
- `jeansLengthScale`: Gravitational collapse scale
- `densityThreshold`: Critical density

---

#### **4B: Metabolic Sink (Biology)**
**Theory:** Cells with high metabolic demand become sinks

**Parameters:**
- `metabolicDemandThreshold`: Activity level to trigger
- `nutrientFlowRequired`: Must have sufficient inflow
- `apoptosisProbability`: Random death removes sinks

**Analogies:**
- Tumor angiogenesis (blood vessel sinks)
- Root systems (nutrient sinks)
- Brain energy consumption (metabolic hotspots)

---

#### **4C: Market Crash (Economics)**
**Theory:** Liquidity crises create value sinks

**Parameters:**
- `liquidityThreshold`: Below this, forms sink
- `contagionRadius`: Nearby cells affected
- `recoveryProbability`: Sink can dissolve

**Analogies:**
- Financial crises (liquidity traps)
- Supply chain disruptions
- Information black holes (censorship)

---

#### **4D: Erosion Basin (Geology)**
**Theory:** Low points accumulate material, deepen over time

**Parameters:**
- `elevationInversion`: Low complexity = sink candidate
- `accumulationRate`: How fast sinks deepen
- `drainageArea`: Catchment size

**Analogies:**
- River basins
- Sediment deposition
- Weathering patterns

---

## ??? **Implementation Architecture**

### **SinkFormationStrategy (Abstract)**

```csharp
public abstract class SinkFormationStrategy
{
    public abstract void EvaluateFormation(GridState state, StepContext context);
    public abstract string GetStrategyName();
    public abstract Dictionary<string, double> GetParameters();
}
```

### **Concrete Strategies**

```csharp
// Category 1
- BoundaryAccumulationStrategy (current)
- LocalMaximumStrategy
- GradientExtremaStrategy

// Category 2
- FixedCountStrategy
- DensityBasedStrategy

// Category 3
- PoissonProcessStrategy
- CriticalityTriggeredStrategy

// Category 4
- JeansInstabilityStrategy (astrophysics)
- MetabolicSinkStrategy (biology)
- MarketCrashStrategy (economics)
- ErosionBasinStrategy (geology)
```

### **Configuration**

Add to `SimulationConfiguration`:

```csharp
public string SinkFormationStrategy { get; set; } = "BoundaryAccumulation";
public Dictionary<string, double> SinkStrategyParameters { get; set; }
```

---

## ?? **Preset Updates**

Each example preset should specify formation strategy:

**Balanced Persistence:**
- Strategy: `BoundaryAccumulation` (current)
- Threshold: 0.5

**Resource Stress:**
- Strategy: `LocalMaximum` (forms at resource peaks)
- Threshold: 0.4 (lower = more sinks)

**Rapid Expansion:**
- Strategy: `FixedCount` (user controls)
- Count: 5, placed at maxima after tick 100

**Competing Regions:**
- Strategy: `DensityBased` (proportional to resources)
- Density: 1 sink per 10,000 resource units

**Stochastic Dynamics:**
- Strategy: `PoissonProcess` (random formation)
- Probability: 0.0001 per cell per tick

---

## ? **Acceptance Criteria**

- [ ] Abstract `SinkFormationStrategy` base class
- [ ] 3-5 concrete strategies implemented
- [ ] Configuration system allows runtime strategy selection
- [ ] Each strategy documented with domain analogies
- [ ] Presets updated to use appropriate strategies
- [ ] `SinkFormationThreshold` works as expected
- [ ] All existing tests still pass

---

## ?? **Phased Implementation**

### **Phase 1: Foundation (Stage 12)**
- Abstract strategy interface
- Refactor current logic into `BoundaryAccumulationStrategy`
- Configuration system

### **Phase 2: Core Strategies (Stage 13)**
- `LocalMaximumStrategy`
- `FixedCountStrategy`
- `PoissonProcessStrategy`

### **Phase 3: Domain-Specific (Stage 14)**
- 4-5 domain-specific strategies
- Documentation with analogies
- Example presets for each

---

## ?? **Priority**

**Critical:**
1. ? Fix `SinkFormationThreshold` (works as expected)
2. ? `FixedCountStrategy` (user control)
3. ? Strategy selection in configuration

**Important:**
4. `LocalMaximumStrategy` (common pattern)
5. `PoissonProcessStrategy` (stochastic modeling)
6. Documentation updates

**Nice-to-Have:**
7. Domain-specific strategies (expand later)
8. Runtime strategy switching
9. Strategy presets library

---

**Next:** Implement Phase 1 (Strategy pattern + refactor current logic)

