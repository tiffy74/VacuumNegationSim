# Viable Tests

This folder contains **Unity integration tests** for the Viable simulation framework.

---

## Test Structure Overview

The Viable project has **two separate test suites**:

### 1. **Engine Tests** (EditMode - NUnit)
**Location:** `Assets/Viable/Engine.Tests/`  
**Assembly:** `Viable.Engine.Tests.dll`

**Tests:**
- `GridStateTests.cs` - Grid state data structure
- `ViabilityCalculatorTests.cs` - Core viability calculations
- `SinkLogicTests.cs` - Sink formation and behavior
- `DeterminismTests.cs` - Reproducibility verification ?
- `PerformanceTests.cs` - Performance benchmarks

**How to Run:**
1. Open Unity Test Runner: `Window > General > Test Runner`
2. Select **EditMode** tab
3. Click "Run All"

**Purpose:** Unit tests for pure C# engine logic (no Unity dependencies)

---

### 2. **Unity Integration Tests** (PlayMode - This Folder!)
**Location:** `Assets/Scripts/Tests/`  
**Assembly:** `Viable.Tests.dll`

**Tests:**
- `TestPresetLoadingPlayMode.cs` - Automated NUnit tests for Resources.Load
- `TestPresetLoading.cs` - Manual MonoBehaviour test for visual verification

**How to Run:**

#### Option A: Automated Tests (Recommended)
1. Open Unity Test Runner: `Window > General > Test Runner`
2. Select **PlayMode** tab
3. Click "Run All"
4. View results in Test Runner

#### Option B: Manual Verification
1. Create empty GameObject in scene
2. Add `TestPresetLoading` component
3. Press Play
4. Check Console for results
5. OR: Right-click component ? "Run Preset Loading Test"

---

## Test Coverage

| Component | Engine Tests | Unity Tests |
|-----------|-------------|-------------|
| Grid State | ? | - |
| Viability Calc | ? | - |
| Sink Logic | ? | - |
| Determinism | ? | - |
| Performance | ? | - |
| **Preset Loading** | - | **?** |
| Resources Folder | - | **?** |
| UI Integration | - | ? (manual) |

---

## Preset Loading Tests Explained

### TestPresetLoadingPlayMode.cs
**Type:** NUnit PlayMode tests  
**Purpose:** Automated verification that runs in Unity Test Runner

**Tests:**
1. `AllPresets_CanLoadFromResources()` - Verifies all 5 presets load
2. `BalancedPersistence_LoadsWithCorrectProperties()` - Validates preset 01
3. `ResourceStress_LoadsWithCorrectProperties()` - Validates preset 02
4. `RapidExpansion_LoadsWithCorrectProperties()` - Validates preset 03
5. `CompetingRegions_LoadsWithCorrectProperties()` - Validates preset 04
6. `StochasticDynamics_LoadsWithCorrectProperties()` - Validates preset 05
7. `AllPresets_HaveValidDescriptions()` - Ensures no empty descriptions
8. `AllPresets_HaveValidGridSizes()` - Validates grid size ranges (32-256)
9. `AllPresets_HaveValidTicksPerSecond()` - Validates TicksPerSecond > 0
10. `PresetSelector_CanLoadAllPresetsAtRuntime()` - Tests runtime loading

**Expected Result:** ? 10/10 tests passing

### TestPresetLoading.cs
**Type:** MonoBehaviour manual test  
**Purpose:** Quick visual verification during development

**Features:**
- Runs automatically on Start (configurable)
- Context menu: Right-click ? "Run Preset Loading Test"
- Verbose logging option (shows all preset details)
- Color-coded Console output (? = success, ? = failure)

**Use Case:** Quick sanity check without opening Test Runner

---

## Prerequisites

### Required Setup

1. **Resources Folder Structure:**
   ```
   Assets/Resources/Presets/Examples/
   ?? 01_BalancedPersistence.asset
   ?? 02_ResourceStress.asset
   ?? 03_RapidExpansion.asset
   ?? 04_CompetingRegions.asset
   ?? 05_StochasticDynamics.asset
   ```

2. **Unity Test Framework Package:**
   - Should be auto-installed
   - If missing: `Window > Package Manager > Unity Registry > Test Framework > Install`

3. **Assembly Definition:**
   - `Viable.Tests.asmdef` (this folder)
   - References: `Viable.Core.Unity`, `UnityEngine.TestRunner`, `nunit.framework.dll`

---

## Running All Tests

### Recommended Workflow

**Step 1: Engine Tests (Fast)**
```
Test Runner > EditMode > Run All
Expected: 25/25 passing
Time: ~5 seconds
```

**Step 2: Unity Tests (Slower)**
```
Test Runner > PlayMode > Run All
Expected: 10/10 passing
Time: ~15 seconds (enters Play mode)
```

**Result:** ? 35/35 total tests passing

---

## Troubleshooting

### ? Tests Fail: "Could not load preset"

**Cause:** Presets not in Resources folder

**Fix:**
1. Verify folder structure: `Assets/Resources/Presets/Examples/`
2. Copy presets from `Assets/Viable/Core.Unity/Presets/Examples/`
3. Right-click Resources folder ? Reimport

### ? Test Runner shows no PlayMode tests

**Cause:** Assembly definition issue or Test Framework not installed

**Fix:**
1. Check `Viable.Tests.asmdef` exists in this folder
2. Install Unity Test Framework: `Package Manager > Test Framework`
3. Restart Unity

### ? Manual test doesn't run

**Cause:** Component not added or runOnStart = false

**Fix:**
1. Verify `TestPresetLoading` component is attached
2. Check Inspector: "Run On Start" should be ?
3. OR: Use context menu "Run Preset Loading Test"

### ? Compilation errors about missing properties

**Cause:** Test code out of sync with ScenarioPreset structure

**Fix:**
1. `ScenarioPreset` has: `GridWidth`, `GridHeight`, `TicksPerSecond`, `Seed`
2. Does NOT have: `GridSize`, `MaxTicks`
3. Update tests to use correct properties

---

## Adding New Tests

### For Resources/Presets

Add to `TestPresetLoadingPlayMode.cs`:

```csharp
[Test]
public void NewPreset_LoadsCorrectly()
{
    // Act
    ScenarioPreset preset = Resources.Load<ScenarioPreset>("Presets/Examples/06_NewPreset");

    // Assert
    Assert.IsNotNull(preset, "06_NewPreset should load");
    Assert.AreEqual("New Preset", preset.PresetName);
    Assert.IsTrue(preset.GridWidth > 0);
    Assert.IsTrue(preset.GridHeight > 0);
}
```

### For UI Components

Create new PlayMode test file:

```csharp
using NUnit.Framework;
using UnityEngine;
using Viable.Tests.PlayMode;

public class TestUIManager
{
    [Test]
    public void UIManager_Initializes()
    {
        // Test code
    }
}
```

---

## Continuous Integration

These tests can be integrated into CI/CD pipelines:

**GitHub Actions Example:**
```yaml
- name: Run Unity Tests
  run: |
    unity-editor -runTests -testPlatform EditMode
    unity-editor -runTests -testPlatform PlayMode
```

---

## Best Practices

### DO ?
- Run EditMode tests first (faster feedback)
- Run PlayMode tests before committing UI changes
- Use manual test for quick visual checks
- Keep tests deterministic (no random values)
- Use actual ScenarioPreset properties in tests

### DON'T ?
- Skip PlayMode tests (they catch Resources.Load issues!)
- Modify Resources folder structure without updating tests
- Add tests that depend on scene setup (use code-based setup instead)
- Reference non-existent properties (GridSize, MaxTicks)

---

## ScenarioPreset Properties Reference

**Available properties for testing:**
- `PresetId` (string)
- `PresetName` (string)
- `Description` (string)
- `GridWidth` (int)
- `GridHeight` (int)
- `Seed` (int?)
- `InitialResourceGlobal` (float)
- `ScaleFactor` (float)
- `DeltaTime` (float)
- `Parameters` (List<ParameterEntry>)
- `InactiveColor` (Color)
- `DormantRegionColor` (Color)
- `ShowComplexityTint` (bool)
- `TicksPerSecond` (float)

**Methods:**
- `GetParameter(key, defaultValue)`
- `SetParameter(key, value)`
- `GetParametersDictionary()`

---

## Related Documentation

- **[Engine Tests README](../../Viable/Engine.Tests/README.md)** - EditMode test suite
- **[Stage 12 Setup Guide](../../../STAGE12_UNITY_SETUP.md)** - UI setup instructions
- **[Preset Examples README](../../Viable/Core.Unity/Presets/Examples/README_EXAMPLES.md)** - Preset documentation
- **[ScenarioPreset.cs](../../Viable/Core.Unity/ScenarioPreset.cs)** - Preset class definition

---

**Status:** ? 10 PlayMode tests | 25 EditMode tests | 35 total tests passing

**Last Updated:** Stage 12 (Unity UI Integration)
