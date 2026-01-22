# Post-Stage 11 Roadmap - Critical Gaps

**Status:** ?? **PLANNING**  
**Goal:** Address missing generalization features before publication

---

## ?? **Critical Issues Identified**

### **Issue 1: Visualization Limitations** ??
**Problem:** Single hardcoded color scheme limits domain applicability

**Impact:**
- Scientists in different fields use different color conventions
- Current colors may not be perceptually uniform
- No accessibility considerations (colorblind-friendly)
- Limits use in publications (journal-specific palettes)

**Solution:** Color scheme system with presets

---

### **Issue 2: Sink Formation Rigidity** ???
**Problem:** Only one formation mechanism (boundary accumulation)

**Impact:**
- Limits cross-domain applicability
- `SinkFormationThreshold` parameter doesn't work as expected
- No user control over sink count/distribution
- Can't model other sink-like phenomena (metabolic sinks, market crashes, etc.)

**Solution:** Strategy pattern with multiple formation mechanisms

---

## ?? **Proposed Stages**

### **Stage 12: Color System** ??
**Duration:** 2-3 hours  
**Priority:** HIGH (affects all visualizations)

**Tasks:**
1. Create `ColorScheme` ScriptableObject
   - Gradient types (linear, perceptual)
   - Color relationships (complementary, analogous, triadic)
   - n-point gradients
2. Create preset schemes
   - Scientific (blue-red, viridis)
   - Monochrome (grayscale, accessibility)
   - Domain-specific (heat maps, terrain)
3. Update `GridRenderer` to use schemes
4. Add scheme selector to `ScenarioPreset`

**Acceptance:**
- [ ] 5+ preset color schemes
- [ ] Runtime scheme switching works
- [ ] All color relationships supported
- [ ] Colorblind-friendly options available

---

### **Stage 13: Sink Formation Strategies** ???
**Duration:** 4-5 hours  
**Priority:** CRITICAL (core functionality broken)

**Tasks:**
1. Create `SinkFormationStrategy` abstract base
2. Refactor current logic ? `BoundaryAccumulationStrategy`
3. Implement core strategies:
   - `LocalMaximumStrategy` (forms at resource peaks)
   - `FixedCountStrategy` (user specifies count)
   - `PoissonProcessStrategy` (stochastic)
4. Add strategy configuration to `SimulationConfiguration`
5. Update presets to use appropriate strategies
6. **Fix `SinkFormationThreshold` bug**

**Acceptance:**
- [ ] 3+ formation strategies working
- [ ] `SinkFormationThreshold` works correctly
- [ ] User can specify exact sink count
- [ ] Strategy selectable in presets
- [ ] All tests pass

---

### **Stage 14: Domain-Specific Extensions** ??
**Duration:** 3-4 hours  
**Priority:** MEDIUM (nice-to-have for broad appeal)

**Tasks:**
1. Implement domain-specific sink strategies:
   - `JeansInstabilityStrategy` (astrophysics)
   - `MetabolicSinkStrategy` (biology)
   - `MarketCrashStrategy` (economics)
   - `ErosionBasinStrategy` (geology)
2. Document analogies for each domain
3. Create example presets demonstrating each
4. Update README with domain examples

**Acceptance:**
- [ ] 4+ domain-specific strategies
- [ ] Each documented with analogies
- [ ] Example presets for each domain
- [ ] README updated with use cases

---

## ?? **Recommended Order**

### **Option A: Complete Current Stage First** (Recommended)
1. ? **Finish Stage 11** (create preset assets in Unity) [20 min]
2. ? **Stage 12** (Color system) [2-3 hrs]
3. ? **Stage 13** (Sink strategies) [4-5 hrs]
4. ? **Stage 14** (Domain extensions) [3-4 hrs]
5. ? Publication ready!

**Total:** ~1-2 days of focused work

---

### **Option B: Fix Critical Bugs Immediately**
1. ? **Stage 13 (Partial)** - Fix `SinkFormationThreshold` bug [1 hr]
2. ? **Stage 13 (Partial)** - Add `FixedCountStrategy` [1 hr]
3. ? **Finish Stage 11** (presets) [20 min]
4. ? **Stage 12** (Colors) [2-3 hrs]
5. ? **Stage 13 (Complete)** [2 hrs remaining]
6. ? **Stage 14** (Extensions) [3-4 hrs]

**Total:** Same time, but gets bugfixes done first

---

## ?? **Detailed Breakdown**

### **Stage 12 Tasks (Color System)**

#### **Phase 1: ScriptableObject** (30 min)
- [ ] Create `ColorScheme.cs` ScriptableObject
- [ ] Define color relationship types (enum)
- [ ] Add gradient point configuration
- [ ] Support n-point gradients

#### **Phase 2: Presets** (45 min)
- [ ] Scientific preset (blue-white-red)
- [ ] Viridis preset (perceptually uniform)
- [ ] Grayscale preset (accessibility)
- [ ] Heat map preset (yellow-orange-red)
- [ ] Terrain preset (green-yellow-brown)

#### **Phase 3: Integration** (1 hr)
- [ ] Update `GridRenderer` to use `ColorScheme`
- [ ] Add scheme reference to `ScenarioPreset`
- [ ] Support runtime switching
- [ ] Test with all presets

#### **Phase 4: Documentation** (30 min)
- [ ] Document color theory basics
- [ ] Explain when to use each scheme
- [ ] Accessibility guidelines

---

### **Stage 13 Tasks (Sink Strategies)**

#### **Phase 1: Strategy Pattern** (1 hr)
- [ ] Create `SinkFormationStrategy` abstract base
- [ ] Define `EvaluateFormation()` interface
- [ ] Add strategy selection to configuration

#### **Phase 2: Refactor Current** (1 hr)
- [ ] Extract current logic ? `BoundaryAccumulationStrategy`
- [ ] Fix `SinkFormationThreshold` bug
- [ ] Test backward compatibility

#### **Phase 3: Core Strategies** (2 hrs)
- [ ] `LocalMaximumStrategy` implementation
- [ ] `FixedCountStrategy` implementation
- [ ] `PoissonProcessStrategy` implementation
- [ ] Test each strategy

#### **Phase 4: Integration** (1 hr)
- [ ] Update `SimulationStepper` to use strategies
- [ ] Add strategy parameters to presets
- [ ] Update example presets
- [ ] Verify all tests pass

---

### **Stage 14 Tasks (Domain Extensions)**

#### **Phase 1: Astrophysics** (45 min)
- [ ] `JeansInstabilityStrategy` implementation
- [ ] Documentation with analogies
- [ ] Example preset

#### **Phase 2: Biology** (45 min)
- [ ] `MetabolicSinkStrategy` implementation
- [ ] Documentation
- [ ] Example preset

#### **Phase 3: Economics** (45 min)
- [ ] `MarketCrashStrategy` implementation
- [ ] Documentation
- [ ] Example preset

#### **Phase 4: Geology** (45 min)
- [ ] `ErosionBasinStrategy` implementation
- [ ] Documentation
- [ ] Example preset

#### **Phase 5: Documentation** (30 min)
- [ ] Update README with domain examples
- [ ] Cross-domain comparison table
- [ ] When to use which strategy

---

## ? **Success Criteria**

**After Stages 12-14:**

### **Visualization:**
- [ ] 5+ color schemes available
- [ ] Scientists can choose domain-appropriate colors
- [ ] Accessibility options (colorblind-friendly)
- [ ] Publications can use custom palettes

### **Sink Formation:**
- [ ] `SinkFormationThreshold` works correctly
- [ ] User can control sink count exactly
- [ ] 7+ formation strategies available
- [ ] Each strategy documented with domain analogies
- [ ] Presets demonstrate diverse mechanisms

### **Cross-Domain:**
- [ ] Framework usable by astrophysicists
- [ ] Framework usable by biologists
- [ ] Framework usable by economists
- [ ] Framework usable by geologists
- [ ] Framework usable by network scientists

---

## ?? **My Recommendation**

**Do this order:**

1. **Stage 13 (Bugfix Priority)** - 1 hour
   - Fix `SinkFormationThreshold` bug FIRST
   - Add `FixedCountStrategy` (user control)
   - This unblocks Stage 11 testing

2. **Stage 11 (Complete)** - 20 minutes
   - Create 5 preset assets in Unity
   - Test with fixed sink strategies
   - Commit

3. **Stage 12 (Color System)** - 2-3 hours
   - Full color scheme system
   - 5 preset schemes
   - Test with all presets

4. **Stage 13 (Complete)** - 2 hours
   - Remaining strategies
   - Full integration
   - Documentation

5. **Stage 14 (Extensions)** - 3-4 hours
   - Domain-specific strategies
   - Cross-domain documentation
   - Publication ready!

**Total time:** ~1 focused work day

---

**Should I start with Stage 13 (fix sink bugs) or Stage 12 (colors) first?** ??

