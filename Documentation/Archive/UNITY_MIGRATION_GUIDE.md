# VIABLE Core: Unity Editor Migration Guide

## Overview

The VIABLE Core refactoring renamed **all theory-loaded terminology** to domain-neutral language. This affects Unity's serialization system, which means **inspector fields will lose their connections** when you first open the project after the refactoring.

This guide provides step-by-step instructions to restore functionality in the Unity Editor.

---

## ?? CRITICAL: Before Opening Unity

1. **Backup your project** (copy the entire folder)
2. **Close Unity** if it's currently open
3. **Complete the refactoring** in your IDE first
4. **Commit the code changes** to version control

---

## Expected Issues When Opening Unity

When you first open the Unity project after refactoring, you will see:

1. **Missing Script References** - MonoBehaviours may show as "Missing (Mono Script)"
2. **Null Inspector Fields** - All serialized fields will be empty/null
3. **Console Errors** - Missing component warnings
4. **Scene Objects** - May appear broken in the scene

**This is expected and fixable!**

---

## Step-by-Step Fix Instructions

### Phase 1: Fix Script Meta Files (5-10 minutes)

Unity tracks scripts by GUID in `.meta` files. Renamed files need meta file updates.

#### 1.1: Verify New Files Have Meta Files

Check that these new files have corresponding `.meta` files:
- `Assets/Scripts/Domain/Gridstate.cs.meta` (Note: Renamed class but same file)
- `Assets/Scripts/Events/SinkRegions.cs.meta`
- `Assets/Scripts/Events/RegionExpansion.cs.meta`

**If missing:**
1. In Unity, right-click each file ? "Reimport"
2. Unity will auto-generate `.meta` files

#### 1.2: Delete Old Meta Files

Delete these `.meta` files (their scripts were removed):
- `Assets/Scripts/Events/BlackHoles.cs.meta` ? DELETED
- `Assets/Scripts/Events/FieldWave.cs.meta` ? DELETED

**How to find them:**
```
Project window ? Show in Explorer ? Delete .meta files for deleted scripts
```

### Phase 2: Fix Scene GameObjects (10-15 minutes)

#### 2.1: Open Your Main Scene

1. `File ? Open Scene`
2. Select your main simulation scene (usually in `Assets/Scenes/`)

#### 2.2: Fix SimulationController Component

**The SimulationController lost ALL its inspector values.**

##### Find the GameObject:
- Look for an object named "SimulationController" or "GameManager" in the Hierarchy

##### Re-assign Inspector Values:

Open the Inspector for SimulationController and set these values:

**[Header: Global Resource Pool]**
```
ResourceGlobalMax = 50000000  (5e7)
ResourceGlobal = 10000000  (1e7)
GlobalReplenishPerTick = 200
MinResourceForPersistence = 5
```

**[Header: Viability / Threshold]**
```
EthreshBase = 0.18
GlobalScarcityK = 0.3
ComplexityPenalty = 0.02
DecayLoss = 0.003
ComplexityViabilityGainA = 0.5
ComplexityViabilityGainK = 1.0
```

**[Header: Propagation]**
```
PropagateFrac = 0.25
MinBudgetToPropagate = 0.1
ActivationCost = 0.25
```

**[Header: Complexity Dynamics]**
```
ComplexityGainPerUse = 0.2
ComplexityDiffusionRate = 0.2
ComplexityDecay = 0.02
ComplexityGainFromGradient = 0.02
ComplexityGainNearSink = 0.05
```

**[Header: Local Limits]**
```
ResourceLocalMax = 50000  (5e4)
PerturbationProbability = 0.0002
PerturbationComplexity = 0.5
ExpansionRate = 1.0
```

**[Header: Region Expansion]**
```
RegionExpansionChance = 0.25
RegionExpansionCost = 0.05
RegionExpansionMinSource = 0.1
RequireViabilityForRegion = false (unchecked)
SeedResourceOnRegionExpansion = true (checked)
RegionSeedResource = 0.1
```

**[Header: Colors]**
```
InactiveColor = RGB(13, 13, 20) / Hex #0D0D14
ShowComplexityTint = false (unchecked)
PerturbationColor = RGB(122, 64, 133) / Hex #7A4085
DormantRegionColor = RGB(38, 0, 64) / Hex #260040
ScaleFactor = 1.0
SpatialThreshK = -0.6
SpatialDecayK = 0.5
```

**[Header: Render Mode]**
```
renderMode = Viability (dropdown)
ticksPerSecond = 10
```

**[Header: Sink Regions]**
```
SinkFormationThreshold = 0.5
SinkDrainFraction = 0
SinkRecoilFraction = 0
```

**[Component Reference]**
```
Grid = (drag the SimulationGrid component from the same GameObject)
```

#### 2.3: Fix SimulationGrid Component

If your SimulationGrid is on the same GameObject:

```
Width = 64
Height = 64
CellPrefab = (drag your cell prefab from Project window)
CellSize = 0.1
```

#### 2.4: Fix Camera Controller (if present)

```
Grid = (drag the SimulationGrid component)
```

### Phase 3: Fix Prefabs (5 minutes)

#### 3.1: Cell Prefab

If you have a `CellPrefab` in `Assets/Prefabs/`:

1. Double-click to open Prefab mode
2. Verify `CellVisualiser` component is attached
3. Verify `SpriteRenderer` component is attached
4. No field changes needed (CellVisualiser has no inspector fields)

### Phase 4: Verify Functionality (5-10 minutes)

#### 4.1: Enter Play Mode

1. Click the Play button
2. **Check Console** for errors
3. **Watch the simulation** - you should see:
   - Yellow frontier expanding from center
   - Viability colors (blue ? cyan ? green ? yellow)
   - Magenta sink regions forming at boundaries
   - Debug logs showing tick progress

#### 4.2: Expected Behavior

**What you SHOULD see:**
- Simulation starts from center (5x5 seed)
- Yellow boundary expands outward
- Interior changes to viability colors
- Magenta sinks form where expansion hits inactive regions
- Console logs: "Tick X | RegionCount=... | Sinks=..."

**What you SHOULD NOT see:**
- NullReferenceExceptions
- Missing component errors
- Black screen (means no rendering)
- Immediate crash

### Phase 5: Save Everything

1. `File ? Save Scene`
2. `File ? Save Project`
3. **Commit to version control**

---

## Common Issues & Fixes

### Issue 1: "Missing (Mono Script)" on SimulationController

**Cause:** Unity can't find the class because namespace or file structure changed

**Fix:**
1. Remove the component (gear icon ? Remove Component)
2. Add it back (Add Component ? search "SimulationController")
3. Re-enter all inspector values (see Phase 2.2)

### Issue 2: Console Error - "NullReferenceException: Object reference not set"

**Cause:** Inspector field not assigned

**Fix:**
1. Read the error carefully - it will name the null field
2. Find that field in the Inspector
3. Assign the correct value/reference

### Issue 3: Simulation Doesn't Start

**Causes:**
- `Grid` reference is null
- `ticksPerSecond` is 0
- Play button not actually starting

**Fix:**
1. Check `Grid` is assigned in SimulationController
2. Set `ticksPerSecond = 10`
3. Click Stop, then Play again

### Issue 4: Black Screen / No Visuals

**Causes:**
- Camera not positioned correctly
- CellPrefab not assigned
- Missing SpriteRenderer on prefab

**Fix:**
1. Check Camera position (should be centered on grid)
2. Assign `CellPrefab` in SimulationGrid
3. Verify prefab has SpriteRenderer component

### Issue 5: Colors Look Wrong

**Cause:** Color values reset to default (usually white/black)

**Fix:**
1. Re-enter exact RGB values from Phase 2.2
2. Pay attention to alpha channel (usually 1.0 = 255)

---

## Automated Fix (Advanced - Optional)

If you have many scenes or prefabs to fix, you can use Unity's scripting API.

**Create this Editor script** at `Assets/Editor/RefactoringFixer.cs`:

```csharp
using UnityEngine;
using UnityEditor;

public class RefactoringFixer
{
    [MenuItem("Tools/VIABLE/Fix Inspector Values")]
    static void FixInspectorValues()
    {
        // Find all SimulationController instances
        var controllers = Object.FindObjectsOfType<SimulationController>();
        
        foreach (var ctrl in controllers)
        {
            // Set default values programmatically
            ctrl.ResourceGlobalMax = 5e7f;
            ctrl.ResourceGlobal = 1e7f;
            ctrl.GlobalReplenishPerTick = 200f;
            // ... etc (add all fields)
            
            EditorUtility.SetDirty(ctrl);
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Fixed {controllers.Length} SimulationController(s)");
    }
}
```

**Run it:**
1. In Unity: `Tools ? VIABLE ? Fix Inspector Values`
2. Check Console for confirmation

---

## Verification Checklist

After completing all phases, verify:

- [ ] No console errors on scene load
- [ ] No missing script warnings
- [ ] SimulationController inspector fully populated
- [ ] Simulation runs in Play mode
- [ ] Visual output looks correct (colors, expansion, sinks)
- [ ] Debug logs appear in console
- [ ] Frame rate is smooth (~60fps)
- [ ] Can stop and restart simulation

---

## File Rename Summary (For Reference)

### Deleted Files
```
Assets/Scripts/Events/BlackHoles.cs ? DELETED (replaced by SinkRegions.cs)
Assets/Scripts/Events/FieldWave.cs ? DELETED (replaced by RegionExpansion.cs)
```

### Class Renames (Same File, New Class Name)
```
GridState ? StateGrid  (in Gridstate.cs)
BlackHoles ? SinkRegions  (in new SinkRegions.cs)
FieldWave ? RegionExpansion  (in new RegionExpansion.cs)
```

### Inspector Field Renames (in SimulationController)

| Old Name | New Name |
|----------|----------|
| NGlobalMax | ResourceGlobalMax |
| NGlobal | ResourceGlobal |
| MinEnergyForPersistence | MinResourceForPersistence |
| EntropyPenalty | ComplexityPenalty |
| EntropyViabilityGainA | ComplexityViabilityGainA |
| EntropyViabilityGainK | ComplexityViabilityGainK |
| EntropyGainPerUse | ComplexityGainPerUse |
| EntropyDiffuseRate | ComplexityDiffusionRate |
| EntropyDecay | ComplexityDecay |
| EntropyGainFromGradient | ComplexityGainFromGradient |
| EntropyGainNearBH | ComplexityGainNearSink |
| NlocalMax | ResourceLocalMax |
| VacuumEventProbability | PerturbationProbability |
| VacuumEventEntropy | PerturbationComplexity |
| BlackHoleFormThreshold | SinkFormationThreshold |
| BlackHoleDrainFrac | SinkDrainFraction |
| BlackHoleRecoilFrac | SinkRecoilFraction |
| FieldAdvanceChance | RegionExpansionChance |
| FieldAdvanceCost | RegionExpansionCost |
| FieldAdvanceMinSource | RegionExpansionMinSource |
| RequireViabilityForField | RequireViabilityForRegion |
| SeedEnergyOnFieldAdvance | SeedResourceOnRegionExpansion |
| FieldSeedEnergy | RegionSeedResource |
| VoidColor | InactiveColor |
| VacuumEnergyColor | PerturbationColor |
| NullspaceColor | DormantRegionColor |
| ShowEntropyTint | ShowComplexityTint |

---

## Time Estimate

- **Minimum (single scene, no custom prefabs):** 30 minutes
- **Typical (multiple scenes, custom settings):** 1-2 hours
- **Maximum (many scenes, complex hierarchy):** 2-4 hours

---

## Support

If you encounter issues not covered here:

1. Check the Console for specific error messages
2. Verify the file rename summary matches your changes
3. Compare inspector values against the defaults in Phase 2.2
4. Check that all deleted files and their `.meta` files are gone
5. Restart Unity (sometimes fixes meta file issues)

**Still stuck?**
- Post the Console error message
- Screenshot the Inspector for the broken component
- List which phase you're on

---

**End of Migration Guide**
