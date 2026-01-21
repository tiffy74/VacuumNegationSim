# Creating the ConstraintsExpansionDemo Preset

**Issue:** Preset file is YAML placeholder, not a real Unity asset  
**Solution:** Create it properly in Unity Editor

---

## ?? **How to Create the Preset**

### **Step 1: Create the Asset**

**In Unity Project window:**

1. **Navigate to:** `Assets/Viable/Core.Unity/Presets/Internal/`
2. **Right-click** in the folder
3. **Create ? Viable ? Scenario Preset**
   - If "Scenario Preset" doesn't appear, try **Create ? ScriptableObject** and search for `ScenarioPreset`
4. **Name it:** `ConstraintsExpansionDemo`

---

### **Step 2: Configure the Preset**

**Select the newly created asset, then in Inspector:**

#### **Identification:**
- **Preset Id:** `constraints-expansion-demo`
- **Preset Name:** `Constraints-Based Expansion (Internal Demo)`
- **Description:**
```
Original simulation demonstrating configuration space expansion with constraint-based dynamics.

This preset preserves the exact behavior of the simulation used for research validation and manuscript preparation.

DO NOT MODIFY - Used for reproducibility and export verification.
```

#### **Grid Configuration:**
- **Grid Width:** `64`
- **Grid Height:** `64`

#### **Execution:**
- **Seed:** Check the box, set to `42`
- **Initial Resource Global:** `10000000` (1e7)
- **Scale Factor:** `1`
- **Delta Time:** `1`

#### **Simulation Parameters:**

Click the **"+"** button for each parameter and add:

| Key | Value |
|-----|-------|
| `resourceGlobalMax` | `50000000` |
| `globalReplenishPerTick` | `200` |
| `minResourceForPersistence` | `5` |
| `ethreshBase` | `0.18` |
| `globalScarcityK` | `0.3` |
| `complexityPenalty` | `0.02` |
| `decayLoss` | `0.003` |
| `propagateFrac` | `0.25` |
| `minBudgetToPropagate` | `0.1` |
| `activationCost` | `0.25` |
| `complexityGainPerUse` | `0.2` |
| `complexityDiffusionRate` | `0.2` |
| `complexityDecay` | `0.02` |
| `resourceLocalMax` | `50000` |
| `perturbationProbability` | `0.0002` |
| `perturbationComplexity` | `0.5` |
| `expansionRate` | `1` |
| `matterAheadThreshold` | `0` |
| `sinkFormationThreshold` | `0.5` |
| `sinkDrainFraction` | `0` |
| `sinkRecoilFraction` | `0` |
| `regionExpansionChance` | `0.25` |
| `regionExpansionCost` | `0.05` |
| `regionExpansionMinSource` | `0.1` |
| `regionExpansionRequiresViability` | `0` |
| `regionExpansionSeedsResource` | `1` |
| `regionSeedResource` | `0.1` |
| `complexityGainFromGradient` | `0.02` |
| `complexityGainNearSink` | `0.05` |
| `complexityViabilityGainA` | `0.5` |
| `complexityViabilityGainK` | `1` |

#### **Visualization:**
- **Inactive Color:** RGB `(0.05, 0.05, 0.08)` Alpha `1`
- **Dormant Region Color:** RGB `(0.15, 0, 0.25)` Alpha `1`
- **Show Complexity Tint:** `false`
- **Ticks Per Second:** `10`

**Save** (Ctrl+S)

---

## ?? **Quick Alternative: Run Without Preset**

**If creating the preset is too tedious right now:**

1. **In SimulationController component:**
   - **Scenario Preset:** Leave as `None`
   - This will use **Legacy Mode** (Inspector settings)

2. **Configure Inspector values:**
   - All the parameters are already set to correct defaults
   - Should work identically!

3. **Press Play** - Should work fine!

**You can create the preset later** when you need to export runs (Stage 9).

---

## ? **After Creating Preset**

1. **Drag preset** to `SimulationController` ? `Scenario Preset` field
2. **Console should show:** `"[SimulationController] Loading from preset: Constraints-Based Expansion (Internal Demo)"`
3. **Press Play** ? Simulation runs from preset

---

## ?? **Recommendation**

**For now:** Just leave the preset as `None` and run in Legacy Mode.

**Preset is only needed for:**
- Stage 9 (Export system)
- Stage 10 (Neutralization)
- Publishing reproducible results

You can create it later when you actually need it!

---

**TL;DR: Leave preset field empty, press Play, it will work!** ??

