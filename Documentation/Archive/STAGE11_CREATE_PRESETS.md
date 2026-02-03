# Stage 11 - Creating Preset Assets in Unity

**Status:** ?? **INSTRUCTIONS**  
**Goal:** Create 5 neutral example preset assets

---

## ?? **How to Create Presets in Unity**

Since preset `.asset` files are Unity-specific binary files, you'll need to create them manually in Unity. Here's the exact step-by-step process:

---

## ?? **Step-by-Step Instructions**

### **1. Create Preset Asset**

**For each preset:**

1. **In Unity Project window:**
   - Navigate to: `Assets/Viable/Core.Unity/Presets/Examples/`
   - Right-click ? **Create ? Scenario Preset**
   - Name it exactly as shown below

2. **Select the new asset**
3. **Fill in parameters** in Inspector (copy from tables below)
4. **Save** (Ctrl+S)

---

## ?? **Preset 1: Balanced Persistence**

**File Name:** `01_BalancedPersistence`

### **Identification:**
- **Preset Id:** `balanced-persistence`
- **Preset Name:** `Balanced Persistence`
- **Description:**
```
Stable propagation with minimal collapse. Demonstrates baseline system behavior with moderate decay and balanced resource replenishment. Good starting point for learning the system.
```

### **Grid:**
- **Width:** 128
- **Height:** 128

### **Execution:**
- **Seed:** ? Check box, value: `42`
- **Initial Resource Global:** `10000000` (1e7)
- **Scale Factor:** `1`
- **Delta Time:** `1`

### **Parameters:** (Click + to add each)
| Key | Value |
|-----|-------|
| `decayLoss` | `0.003` |
| `resourceGlobalMax` | `50000000` |
| `globalReplenishPerTick` | `200` |
| `ethreshBase` | `0.18` |
| `globalScarcityK` | `0.3` |
| `propagateFrac` | `0.25` |
| `regionExpansionChance` | `0.25` |
| `complexityGainPerUse` | `0.2` |
| `complexityDiffusionRate` | `0.2` |
| `complexityDecay` | `0.02` |
| `perturbationProbability` | `0.0002` |

### **Visualization:**
- **Inactive Color:** RGB `(0.05, 0.05, 0.08)` Alpha `1`
- **Dormant Region Color:** RGB `(0.15, 0, 0.25)` Alpha `1`
- **Show Complexity Tint:** `false`
- **Ticks Per Second:** `10`

---

## ?? **Preset 2: Resource Stress**

**File Name:** `02_ResourceStress`

### **Identification:**
- **Preset Id:** `resource-stress`
- **Preset Name:** `Resource Stress`
- **Description:**
```
High decay creates cascading failures. Demonstrates system behavior under resource scarcity with rapid collapse dynamics. Useful for studying resilience and network vulnerability.
```

### **Grid:**
- Same as Preset 1

### **Execution:**
- Same as Preset 1

### **Parameters:**
Copy all from Preset 1, **then change these:**
| Key | Value (CHANGED) |
|-----|-----------------|
| `decayLoss` | `0.01` ? 3× higher! |
| `globalReplenishPerTick` | `100` ? Half! |
| `resourceGlobalMax` | `30000000` ? Lower |

### **Visualization:**
- Same as Preset 1

---

## ?? **Preset 3: Rapid Expansion**

**File Name:** `03_RapidExpansion`

### **Identification:**
- **Preset Id:** `rapid-expansion`
- **Preset Name:** `Rapid Expansion`
- **Description:**
```
Fast propagation with abundant resources. Demonstrates growth-dominated regime where expansion overwhelms decay. Shows spatial spread dynamics and coverage patterns.
```

### **Grid:**
- Same as Preset 1

### **Execution:**
- Same as Preset 1

### **Parameters:**
Copy all from Preset 1, **then change these:**
| Key | Value (CHANGED) |
|-----|-----------------|
| `decayLoss` | `0.001` ? 3× lower! |
| `globalReplenishPerTick` | `500` ? 2.5× higher! |
| `resourceGlobalMax` | `100000000` ? 2× higher |
| `regionExpansionChance` | `0.5` ? 2× higher! |
| `propagateFrac` | `0.35` ? Higher flow |

### **Visualization:**
- Same as Preset 1

---

## ?? **Preset 4: Competing Regions**

**File Name:** `04_CompetingRegions`

### **Identification:**
- **Preset Id:** `competing-regions`
- **Preset Name:** `Competing Regions`
- **Description:**
```
Multiple seeds compete for shared resources. Demonstrates multi-agent dynamics with territory formation and resource partitioning. Shows how regions coexist or compete for dominance.

NOTE: Requires manual scene modification to add multiple initial seeds. See IMPLEMENTATION.md for details.
```

### **Grid:**
- Same as Preset 1

### **Execution:**
- Same as Preset 1

### **Parameters:**
Copy all from Preset 1, **then change these:**
| Key | Value (CHANGED) |
|-----|-----------------|
| `globalReplenishPerTick` | `150` ? Lower (creates scarcity) |

### **Visualization:**
- Same as Preset 1

**?? Special Note:**  
This preset requires modifying `InitStateInto()` in SimulationController to seed 4 regions instead of 1. This is an **advanced preset** - document as "requires code modification" for now.

---

## ?? **Preset 5: Stochastic Dynamics**

**File Name:** `05_StochasticDynamics`

### **Identification:**
- **Preset Id:** `stochastic-dynamics`
- **Preset Name:** `Stochastic Dynamics`
- **Description:**
```
High perturbation creates unpredictable patterns. Demonstrates deterministic chaos where small changes cascade. Despite appearing random, results are 100% reproducible with same seed.
```

### **Grid:**
- Same as Preset 1

### **Execution:**
- Same as Preset 1

### **Parameters:**
Copy all from Preset 1, **then change these:**
| Key | Value (CHANGED) |
|-----|-----------------|
| `perturbationProbability` | `0.002` ? 10× higher! |
| `perturbationComplexity` | `0.5` |

### **Visualization:**
- Same as Preset 1

---

## ? **Verification Checklist**

After creating all presets:

- [ ] **01_BalancedPersistence.asset** created
- [ ] **02_ResourceStress.asset** created
- [ ] **03_RapidExpansion.asset** created
- [ ] **04_CompetingRegions.asset** created
- [ ] **05_StochasticDynamics.asset** created
- [ ] All presets have seed = 42
- [ ] All presets have correct grid size (128×128)
- [ ] README_EXAMPLES.md exists in same folder
- [ ] Test each preset (loads without errors)

---

## ?? **Testing Each Preset**

**For each preset:**

1. **Load preset** in SimulationController
2. **Press Play**
3. **Verify behavior** matches description
4. **Run ~100-200 ticks**
5. **Export run** (right-click ? Export Current Run)
6. **Check CSV** has data

**Expected differences:**
- **Balanced:** Smooth expansion, few sinks
- **Stress:** Early collapses, many sinks
- **Rapid:** Fast coverage, late sinks
- **Competing:** (If implemented) Multiple regions
- **Stochastic:** Chaotic patterns, but reproducible

---

## ?? **After Creation**

Once all presets are created and tested:

1. **Commit to git:**
```sh
git add Assets/Viable/Core.Unity/Presets/Examples/
git add STAGE11_*.md
git commit -m "feat: Stage 11 complete - Neutral example presets

Created 5 domain-neutral example presets:
- Balanced Persistence (baseline)
- Resource Stress (cascading failures)
- Rapid Expansion (growth-dominated)
- Competing Regions (multi-agent)
- Stochastic Dynamics (deterministic chaos)

Each preset demonstrates distinct emergent behavior.
Documented with usage guide in README_EXAMPLES.md.

Ready for: User onboarding, tutorials, documentation"
```

2. **Update main README** (if needed)
3. **Test export** with each preset
4. **Mark Stage 11 complete**

---

## ?? **Quick Creation Script**

**To speed up creation:**

1. Create first preset (Balanced Persistence) completely
2. **Duplicate it** 4 times (Ctrl+D in Unity)
3. Rename each duplicate
4. **Modify only the changed parameters** (saves time!)
5. Update Identification fields

---

**Ready to create! Start with Balanced Persistence first.** ??

