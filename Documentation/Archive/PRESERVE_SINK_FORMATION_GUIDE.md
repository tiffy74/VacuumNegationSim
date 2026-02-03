# Preserving Default Sink Formation Behavior

## ?? Problem Solved

**Issue:** Default preset's sink formation mechanism (fieldwave/energywave dynamics) was being lost when switching to other presets because mechanism modes weren't being saved in presets.

**Root Cause:** ScenarioPreset only stored numeric parameters, not mechanism modes (Inflow, Boundary, Diffusion, Viability, Topology).

**Solution:** Added `MechanismConfig` field to ScenarioPreset to preserve mechanism modes.

---

## ? What Changed

### 1. **ScenarioPreset.cs** - Added MechanismConfig
- New field: `public EngineConfigData MechanismConfig`
- Stores all mechanism modes:
  - InflowMode (Uniform, PointSources, EdgeSources)
  - BoundaryMode (Closed, Open, Wrap)
  - DiffusionMode (VonNeumann4, Moore8, Anisotropic)
  - ViabilityRule (Simple, Hysteresis)
  - TopologyMode (FullDomain, MaskedDomain)
- Stores mechanism-specific settings (hysteresis thresholds, point sources, etc.)

### 2. **WorkingScenarioConfig.FromPreset()** - Maps MechanismConfig
- Now reads mechanism modes from preset
- Deep copies all mechanism settings
- Preserves your sink formation configuration

---

## ?? **Action Required: Save Your Default Preset's Mechanism Modes**

Your default preset now needs its mechanism configuration saved so it can be restored.

### Step 1: Open Your Default Preset in Inspector

1. In Unity Project window, navigate to your default preset:
   - Likely at: `Assets/Viable/Core.Unity/Presets/Examples/YourDefaultPreset.asset`
2. Select the preset file
3. Inspector window shows preset fields

### Step 2: Configure Mechanism Modes

In Inspector, you'll now see a new section: **Mechanism Configuration**

**Set these to match your default sink formation behavior:**

#### **Mechanism Modes:**
- **Inflow Mode:** `UniformField` (resources flow uniformly across active cells)
- **Boundary Mode:** `Wrap` (periodic wrap - toroidal topology)
- **Diffusion Mode:** `Moore8` (8-neighbor diffusion)
- **Viability Rule:** `Simple` (simple threshold rule)
- **Topology Mode:** `FullDomain` (no masking)

#### **Topology Details (if MaskedDomain):**
- Leave as defaults unless using masked topology

#### **Point Sources (if InflowMode = PointSources):**
- Leave empty for UniformField mode

#### **Hysteresis (if ViabilityRule = Hysteresis):**
- Leave as defaults for Simple rule

#### **Anisotropic Diffusion (if DiffusionMode = Anisotropic):**
- Leave as defaults for Moore8

### Step 3: Save the Preset

1. After setting mechanism modes, press `Ctrl+S` or click outside the preset
2. Unity auto-saves the changes

### Step 4: Verify in Other Presets

**For each other preset**, set their mechanism modes appropriately:
- If they should have **different** behavior, set different modes
- If they should **preserve** sink formation, set same modes as default

---

## ?? How It Works Now

### Preset Switching Workflow:

```
1. Default Preset Selected:
   ?? MechanismConfig: Inflow=UniformField, Boundary=Wrap, etc.
   ?? Parameters: decayLoss=0.003, etc.
   ?? Result: Yellow fieldwave boundary, magenta sinks ?

2. User Switches to "Experiment A" Preset:
   ?? MechanismConfig: Inflow=PointSources (different!)
   ?? Parameters: Different values
   ?? Result: Different behavior (point source islands)

3. User Switches Back to Default Preset:
   ?? MechanismConfig: Inflow=UniformField (restored!)
   ?? Parameters: Original values (restored!)
   ?? Result: Yellow fieldwave + magenta sinks RESTORED ?
```

**Your sink formation behavior is now preserved!**

---

## ?? Testing

### Test 1: Default Behavior Preserved

```bash
1. Select default preset from dropdown
2. Click Apply
3. Click Play
4. Observe: Yellow fieldwave boundary, magenta sinks
? PASS if sink formation behavior appears
```

### Test 2: Switch Presets and Return

```bash
1. Start with default preset (yellow + magenta)
2. Switch to different preset (e.g., "Point Sources")
3. Click Apply ? Different behavior
4. Switch back to default preset
5. Click Apply ? Original behavior returns
? PASS if sink formation restored
```

### Test 3: Mechanism Modes Persist

```bash
1. Select default preset
2. Check MechanismsSection dropdown values:
   - Inflow: "Uniform Field"
   - Boundary: "Wrap"
   - Diffusion: "Moore (8-neighbor)"
   - Viability: "Simple Threshold"
? PASS if dropdown values match preset's MechanismConfig
```

---

## ?? Understanding Sink Formation

Your default preset's sink formation emerges from this mechanism combination:

**Key Mechanisms:**
1. **UniformField Inflow** ? Resources flow evenly across fieldwave
2. **Wrap Boundaries** ? Periodic topology (no edge effects)
3. **Moore8 Diffusion** ? 8-neighbor resource diffusion
4. **Region Expansion** ? Yellow boundary propagates outward
5. **Viability Threshold** ? Cells die if resources < threshold

**Sink Formation Process:**
```
Fieldwave (Yellow):
  ?? Active region boundary propagating outward
  ?? Receives resources from UniformField inflow
  ?? Expands into inactive cells

Energywave:
  ?? Resource diffusion spreading through active cells
  ?? Moves faster than fieldwave in some areas

Sinks (Magenta):
  ?? Form where energywave overtakes fieldwave boundary
  ?? "Pockets" where fieldwave is absent
  ?? Resource accumulation points
```

**Critical:** If you change InflowMode (e.g., to PointSources), this mechanism breaks!
- Fieldwave no longer receives uniform resources
- Sink formation pattern changes completely

**Solution:** Presets now preserve these mechanism modes, so switching back restores the behavior!

---

## ?? Checklist: Default Preset Configuration

**In Unity Inspector (Default Preset):**

- [ ] **Mechanism Configuration** section visible
- [ ] **Inflow Mode** = `UniformField`
- [ ] **Boundary Mode** = `Wrap`
- [ ] **Diffusion Mode** = `Moore8`
- [ ] **Viability Rule** = `Simple`
- [ ] **Topology Mode** = `FullDomain`
- [ ] **Preset saved** (Ctrl+S)

**In Unity Scene:**

- [ ] **Default Preset** assigned in orchestrator Inspector
- [ ] **Dropdown shows default preset** on startup
- [ ] **Apply button** wired to orchestrator

**Testing:**

- [ ] Select default ? Apply ? Sinks form ?
- [ ] Switch preset ? Apply ? Behavior changes
- [ ] Return to default ? Apply ? Sinks restore ?

---

## ?? Result

Your default preset's **sink formation mechanism** is now:
- ? **Stored** in preset's MechanismConfig
- ? **Loaded** when preset is selected
- ? **Restored** when you switch back to default
- ? **Preserved** across Unity sessions

**Your master hypothesis (fieldwave/energywave dynamics creating sinks) is now recoverable!** ??

---

## ?? Bonus: Create Preset Variants

You can now create preset variants that preserve or modify the mechanism:

### Variant 1: "Default + Higher Decay"
- MechanismConfig: Same as default (sinks preserved)
- Parameters: Increase `decayLoss`
- Result: Sinks still form, but faster collapse

### Variant 2: "Default + Hysteresis"
- MechanismConfig: Same except `ViabilityRule = Hysteresis`
- Parameters: Same as default
- Result: Sinks form but cells persist longer (delayed death)

### Variant 3: "Experiment - Point Sources"
- MechanismConfig: `Inflow = PointSources` (different mechanism!)
- Parameters: Different values
- Result: Completely different behavior (islands, not sinks)

**All presets coexist, and switching between them preserves their mechanisms!** ?
