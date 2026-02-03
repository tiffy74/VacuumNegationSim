# Stage 13.2: Value Semantics for Interpretable Outputs

## ? COMPLETE

**Date:** 2025-01-21  
**Status:** Implementation complete, tests passing

---

## ?? Goal

Add value semantics to make numeric outputs interpretable without assigning physical units. Use role-based semantics (Quantity, Rate, Cost, Index, Count) to describe what metrics mean.

---

## ? What Was Implemented

### 1. ValueSemantics Dictionary in RunResult ?

**File:** `Assets/Viable/Contracts/RunResult.cs`

Added `ValueSemantics` dictionary to describe metric meanings:
```csharp
/// <summary>
/// Value semantics describing the meaning of metric keys.
/// Stage 13.2: Enables interpretable outputs without physical units.
/// Format: metric_name -> semantic_type
/// Semantic types: "Quantity (Q)", "Rate (Q/step)", "Cost (Q/step)", 
///                 "Index (dimensionless)", "Count"
/// </summary>
public Dictionary<string, string> ValueSemantics { get; set; } = new Dictionary<string, string>();
```

### 2. Semantic Population in SimulationRunner ?

**File:** `Assets/Viable/Engine/SimulationRunner.cs`

Added `PopulateValueSemantics()` method that populates semantics for all current metrics:

```csharp
private void PopulateValueSemantics(RunResult result)
{
    result.ValueSemantics["viableCount"] = "Count";
    result.ValueSemantics["activeCount"] = "Count";
    result.ValueSemantics["sinkCount"] = "Count";
    result.ValueSemantics["avgResource"] = "Quantity (Q)";
    result.ValueSemantics["avgComplexity"] = "Index (dimensionless)";
    result.ValueSemantics["resourceGlobal"] = "Quantity (Q)";
    
    // Add semantics for common scenario parameters
    result.ValueSemantics["decayLoss"] = "Rate (Q/step)";
    result.ValueSemantics["maintenanceCost"] = "Cost (Q/step)";
    result.ValueSemantics["inflow"] = "Rate (Q/step)";
    result.ValueSemantics["outflow"] = "Rate (Q/step)";
    result.ValueSemantics["viability"] = "Index (dimensionless)";
}
```

### 3. CSV Headers with Semantic Suffixes ?

**File:** `Assets/Viable/Core.Unity/Export/MetricsCsvWriter.cs`

Updated CSV headers to include semantic suffixes:
- `_Q` = Quantity (abstract resource units)
- `_Q_per_step` = Rate or Cost (quantity per simulation step)
- `_index` = Index (dimensionless, normalized metric)
- `_count` = Count (integer quantity)

**Example CSV header:**
```
Tick,Time,ViableCount_count,ActiveCount_count,SinkCount_count,AvgResource_Q,AvgComplexity_index,ResourceGlobal_Q
```

Added `GetSemanticSuffix()` helper method to convert semantic types to suffixes.

### 4. Semantics Legend in Summary ?

**File:** `Assets/Viable/Core.Unity/Export/SummaryWriter.cs`

Added value semantics legend to `summary.md`:

```markdown
### Value Semantics

Metric suffixes in `metrics.csv` indicate value types:

- `_Q` = Quantity (abstract resource units)
- `_Q_per_step` = Rate or Cost (quantity per simulation step)
- `_index` = Index (dimensionless, normalized metric)
- `_count` = Count (integer quantity)

**Note:** Q is an abstract quantity unit without physical dimensions.
Time basis is per simulation step (not physical time).
```

### 5. Test Coverage ?

**File:** `Assets/Viable/Engine.Tests/MechanismConfigTests.cs`

Added test to verify ValueSemantics functionality:
```csharp
[Test]
public void RunResult_ValueSemantics_IncludesStandardMetrics()
{
    // Verifies that ValueSemantics dictionary works correctly
    // and contains expected semantic type strings
}
```

---

## ?? Semantic Type Mapping

| Metric | Semantic Type | Suffix | Meaning |
|--------|---------------|--------|---------|
| viableCount | Count | `_count` | Integer count of cells |
| activeCount | Count | `_count` | Integer count of cells |
| sinkCount | Count | `_count` | Integer count of cells |
| avgResource | Quantity (Q) | `_Q` | Average resource quantity |
| avgComplexity | Index (dimensionless) | `_index` | Normalized complexity metric |
| resourceGlobal | Quantity (Q) | `_Q` | Global resource quantity |
| decayLoss | Rate (Q/step) | `_Q_per_step` | Resource loss per step |
| maintenanceCost | Cost (Q/step) | `_Q_per_step` | Maintenance cost per step |
| inflow | Rate (Q/step) | `_Q_per_step` | Resource inflow per step |
| outflow | Rate (Q/step) | `_Q_per_step` | Resource outflow per step |
| viability | Index (dimensionless) | `_index` | Viability score |

---

## ?? Key Design Decisions

### 1. **Role-Based Semantics (Not Physical Units)**
- Used abstract semantic types (Quantity, Rate, Cost, Index, Count)
- Avoided physical units (joules, seconds, etc.)
- Keeps framework domain-neutral

### 2. **Suffix Convention**
- Short, clear suffixes that don't clutter CSV files
- Easy to parse programmatically: `split("_")[1]`
- Human-readable without excessive verbosity

### 3. **Backward Compatibility**
- ValueSemantics dictionary defaults to empty (no breaking changes)
- MetricsCsvWriter accepts optional `valueSemantics` parameter
- Existing code continues to work unchanged

### 4. **Time Basis: Simulation Steps**
- Rates and costs are "per step", not "per second"
- Keeps framework independent of physical time scaling
- Clear separation: simulation time ? wall-clock time

### 5. **Q as Abstract Quantity**
- "Q" represents an abstract quantity unit
- No physical dimensions (not joules, liters, dollars, etc.)
- Allows framework to be used across domains

---

## ? Verification

### Tests Passing ?
- Build successful
- All existing tests pass (no behavior change)
- New test `RunResult_ValueSemantics_IncludesStandardMetrics()` passes

### Exported Files Updated ?
- `metrics.csv` headers include semantic suffixes
- `summary.md` includes value semantics legend
- `manifest.json` unchanged (backward compatible)

### Determinism Preserved ?
- No changes to simulation logic
- Same seed ? same results
- ValueSemantics is metadata only

---

## ?? Example Export Output

### metrics.csv (with semantic headers)
```csv
Tick,Time,ViableCount_count,ActiveCount_count,SinkCount_count,AvgResource_Q,AvgComplexity_index,ResourceGlobal_Q
0,0.0,25,25,0,1000.00,1.50,5.00E+07
10,10.0,45,45,2,980.25,1.65,4.95E+07
20,20.0,67,67,3,955.80,1.82,4.89E+07
```

### summary.md (with semantics legend)
```markdown
## Data

**Samples Collected:** 100
**Sample Interval:** Every 10 steps

### Value Semantics

Metric suffixes in `metrics.csv` indicate value types:

- `_Q` = Quantity (abstract resource units)
- `_Q_per_step` = Rate or Cost (quantity per simulation step)
- `_index` = Index (dimensionless, normalized metric)
- `_count` = Count (integer quantity)

**Note:** Q is an abstract quantity unit without physical dimensions.
Time basis is per simulation step (not physical time).
```

---

## ?? Interpretation Guide for Users

### For Researchers
- **Quantity (Q):** Treat as a resource budget or capacity
- **Rate (Q/step):** How much Q changes per simulation step
- **Cost (Q/step):** Maintenance or consumption per step
- **Index:** Normalized metric (0-1 or other fixed range)
- **Count:** Simple integer count

### For Data Analysis
```python
# Python example: Parse semantic suffixes
import pandas as pd

df = pd.read_csv("metrics.csv")

# Extract suffix from column names
def get_semantic(col):
    if "_Q_per_step" in col:
        return "rate"
    elif "_Q" in col:
        return "quantity"
    elif "_index" in col:
        return "index"
    elif "_count" in col:
        return "count"
    return "other"

# Group columns by semantic type
for col in df.columns:
    semantic = get_semantic(col)
    print(f"{col}: {semantic}")
```

---

## ?? Next Steps (Future Stages)

### Stage 13.3: Wire Mechanism Behaviors
- Implement actual mechanism selection logic in engine phases
- Current state: Defaults read, but all behavior is still uniform

### Stage 13.4: Create Preset Diversity
- Build preset library showcasing different mechanisms
- Examples: PointSource inflow, Moore8 diffusion, Hysteresis viability

### Stage 13.5: UI Mechanism Controls
- Add dropdown selectors for mechanisms in Unity UI
- Allow runtime mechanism switching

---

## ?? Notes

- **No behavior change:** Simulation results are identical to Stage 13.1
- **Metadata only:** ValueSemantics is purely descriptive
- **Backward compatible:** Null/empty ValueSemantics is fine
- **Schema version stable:** ContractVersions unchanged (still 1.0)

---

## ? Stage 13.2 Complete!

**Ready for:** Stage 13.3 (Wire mechanism behaviors to engine logic)

**Checklist:**
- [x] ValueSemantics added to RunResult
- [x] Semantics populated in SimulationRunner
- [x] CSV headers include semantic suffixes
- [x] Summary includes semantics legend
- [x] Tests pass
- [x] Build successful
- [x] Documentation complete

---

**Stage 13.2 Status: ? COMPLETE**
