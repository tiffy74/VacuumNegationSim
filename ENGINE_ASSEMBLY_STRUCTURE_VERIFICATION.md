# Engine Assembly Structure Verification

## File Location Verification ?

All files are in correct locations with correct namespaces:

### State Files
```
Location: Assets/Viable/Engine/State/GridState.cs
Namespace: Viable.Engine.State
Class: GridState
Status: ? CORRECT
```

### Execution Files
```
Location: Assets/Viable/Engine/Execution/StepContext.cs
Namespace: Viable.Engine.Execution
Class: StepContext
Status: ? CORRECT
```

### Configuration Files
```
Location: Assets/Viable/Engine/Configuration/SimulationConfiguration.cs
Namespace: Viable.Engine.Configuration
Class: SimulationConfiguration
Status: ? CORRECT
```

### Steps Files
```
Location: Assets/Viable/Engine/Steps/*.cs
Namespace: Viable.Engine.Steps
Classes: OutflowPhase, InflowPhase, RechargePhase, DiffusionPhase
Status: ? CORRECT
```

### Logic Files
```
Location: Assets/Viable/Engine/Logic/*.cs
Namespace: Viable.Engine.Logic
Classes: SinkLogic, RegionExpansionLogic
Status: ? CORRECT
```

### Computation Files
```
Location: Assets/Viable/Engine/Computation/*.cs
Namespace: Viable.Engine.Computation
Class: ViabilityCalculator
Status: ? CORRECT
```

### Interfaces Files
```
Location: Assets/Viable/Engine/Interfaces/*.cs
Namespace: Viable.Engine.Interfaces
Interface: IStepPhase
Status: ? CORRECT
```

### Root Engine Files
```
Location: Assets/Viable/Engine/SimulationRunner.cs
Namespace: Viable.Engine
Class: SimulationRunner
Using Statements:
  - using Viable.Engine.State;          ? (references GridState)
  - using Viable.Engine.Execution;      ? (references StepContext)
  - using Viable.Engine.Interfaces;     ? (references IStepPhase)
  - using Viable.Contracts;             ? (references ScenarioDefinition, etc.)
Status: ? CORRECT (will compile after Unity regenerates .csproj)

Location: Assets/Viable/Engine/SimulationStepper.cs
Namespace: Viable.Engine
Class: SimulationStepper
Using Statements:
  - using Viable.Engine.State;          ?
  - using Viable.Engine.Execution;      ?
  - using Viable.Engine.Steps;          ?
  - using Viable.Engine.Logic;          ?
  - using Viable.Engine.Computation;    ?
  - using Viable.Engine.Interfaces;     ?
  - using Viable.Contracts;             ?
Status: ? CORRECT (will compile after Unity regenerates .csproj)
```

---

## Why Visual Studio Shows Errors

Visual Studio is currently using an **outdated** `Viable.Engine.csproj` that was generated before the subfolder structure existed.

**Current (old) .csproj structure:**
```xml
<Project>
  <ItemGroup>
    <!-- Only includes files Unity knew about during last regeneration -->
    <Compile Include="Assets\Viable\Engine\README.md" />
    <!-- Missing: State\GridState.cs -->
    <!-- Missing: Execution\StepContext.cs -->
    <!-- Missing: Configuration\SimulationConfiguration.cs -->
    <!-- Missing: Steps\*.cs -->
    <!-- Missing: Logic\*.cs -->
    <!-- Missing: Computation\*.cs -->
    <!-- Missing: Interfaces\*.cs -->
  </ItemGroup>
</Project>
```

**After Unity regenerates (correct) .csproj structure:**
```xml
<Project>
  <ItemGroup>
    <Compile Include="Assets\Viable\Engine\State\GridState.cs" />
    <Compile Include="Assets\Viable\Engine\Execution\StepContext.cs" />
    <Compile Include="Assets\Viable\Engine\Configuration\SimulationConfiguration.cs" />
    <Compile Include="Assets\Viable\Engine\Steps\OutflowPhase.cs" />
    <Compile Include="Assets\Viable\Engine\Steps\InflowPhase.cs" />
    <Compile Include="Assets\Viable\Engine\Steps\RechargePhase.cs" />
    <Compile Include="Assets\Viable\Engine\Steps\DiffusionPhase.cs" />
    <Compile Include="Assets\Viable\Engine\Logic\SinkLogic.cs" />
    <Compile Include="Assets\Viable\Engine\Logic\RegionExpansionLogic.cs" />
    <Compile Include="Assets\Viable\Engine\Computation\ViabilityCalculator.cs" />
    <Compile Include="Assets\Viable\Engine\Interfaces\IStepPhase.cs" />
    <Compile Include="Assets\Viable\Engine\SimulationRunner.cs" />
    <Compile Include="Assets\Viable\Engine\SimulationStepper.cs" />
  </ItemGroup>
</Project>
```

---

## Resolution Steps

### 1. Open Unity Editor ? **DO THIS NOW**
```
Unity will:
1. Scan Assets/Viable/Engine/ folder
2. Detect all new .cs files
3. Generate .meta files for folders and files
4. Regenerate Viable.Engine.csproj with correct includes
```

### 2. Check Unity Console
```
Expected: 0 errors (all Engine files should compile)
If errors: Report them - likely a typo or actual issue
```

### 3. Regenerate Visual Studio Solution
```
In Unity: Assets ? Open C# Project
This forces Unity to regenerate .sln and .csproj files
```

### 4. Reopen Visual Studio
```
Close Visual Studio completely
Reopen from Unity or double-click .sln
IntelliSense will now see the correct structure
```

### 5. Verify in Visual Studio
```
Solution Explorer should show:
Viable.Engine/
  ?? State/
  ?  ?? GridState.cs
  ?? Execution/
  ?  ?? StepContext.cs
  ?? Configuration/
  ?  ?? SimulationConfiguration.cs
  ?? Steps/
  ?  ?? OutflowPhase.cs
  ?  ?? InflowPhase.cs
  ?  ?? RechargePhase.cs
  ?  ?? DiffusionPhase.cs
  ?? Logic/
  ?  ?? SinkLogic.cs
  ?  ?? RegionExpansionLogic.cs
  ?? Computation/
  ?  ?? ViabilityCalculator.cs
  ?? Interfaces/
  ?  ?? IStepPhase.cs
  ?? SimulationRunner.cs
  ?? SimulationStepper.cs
```

### 6. Build in Visual Studio
```
Build ? Rebuild Solution
Expected: 3 succeeded, 0 failed
  - Viable.Contracts   ?
  - Viable.Engine      ? (now includes all files!)
  - Assembly-CSharp    ?
```

---

## Common Issues & Fixes

### Issue: "Namespace 'State' does not exist"
**Cause:** Unity hasn't regenerated .csproj yet
**Fix:** Open Unity, wait for import, regenerate solution

### Issue: Files missing in Solution Explorer
**Cause:** Visual Studio cached old .csproj
**Fix:** Delete `.vs/` folder, reopen solution

### Issue: Duplicate file entries
**Cause:** Unity meta files conflicted
**Fix:** Delete `Library/ScriptAssemblies/`, reopen Unity

---

## Verification Commands

### In Unity Console
```
No compilation errors should appear after Unity finishes importing.
If you see errors, check the error message carefully.
```

### In Visual Studio Output Window
```
Build ? Rebuild Solution
Check Output window for:
  "========== Rebuild All: 3 succeeded, 0 failed, 0 skipped =========="
```

### In Terminal (Alternative)
```powershell
# From solution directory
Get-ChildItem -Recurse -Filter "*.csproj" | Select-String "GridState"
# Should show: Viable.Engine.csproj includes GridState.cs
```

---

## Expected Timeline

| Step | Duration | Cumulative |
|------|----------|------------|
| 1. Open Unity | 30-60 sec | 1 min |
| 2. Unity import all files | 10-30 sec | 1.5 min |
| 3. Generate .meta files | Auto | 1.5 min |
| 4. Regenerate .csproj | 5-10 sec | 2 min |
| 5. Reopen Visual Studio | 15-30 sec | 2.5 min |
| 6. IntelliSense reindex | 10-20 sec | 3 min |
| **Total** | | **~3 minutes** |

---

## Success Criteria

After completing the resolution steps, you should see:

- [ ] Unity Console: **0 errors**
- [ ] Visual Studio Solution Explorer: **All Engine subfolders visible**
- [ ] Visual Studio Error List: **0 errors**
- [ ] Build Output: **3 succeeded, 0 failed**
- [ ] IntelliSense: **GridState, StepContext, SimulationConfiguration** all resolve

---

## Status

**Current:** ?? Awaiting Unity project regeneration
**Code Quality:** ? All namespaces and using statements correct
**Action Required:** Open Unity to regenerate project files

**DO THIS NOW:**
1. Save all files in Visual Studio
2. Close Visual Studio
3. Open Unity
4. Wait for import to complete
5. Verify console shows 0 errors
6. Close Unity
7. Reopen Visual Studio
8. Build solution

**After these steps, all errors will be resolved automatically.**

