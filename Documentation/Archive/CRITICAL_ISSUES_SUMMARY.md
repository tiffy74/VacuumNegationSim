# Critical Issues Summary - Action Required

**Date:** January 2025  
**Status:** ?? **IDENTIFIED - AWAITING IMPLEMENTATION**

---

## ?? **Quick Summary**

Two critical generalization gaps identified that limit cross-domain applicability:

1. **Visualization System** - Single hardcoded color scheme limits use
2. **Sink Formation** - Only one mechanism, `SinkFormationThreshold` doesn't work

---

## ?? **Documents Created**

### **Design Documents:**
1. ? `SINK_FORMATION_DESIGN.md` - Complete sink strategy system design
2. ? `POST_STAGE11_ROADMAP.md` - Phased implementation plan
3. ? Color system design (in progress - see plan)

### **What They Contain:**
- Problem statements
- Multiple solution approaches
- Domain-specific examples
- Implementation phases
- Acceptance criteria

---

## ? **Immediate Actions Needed**

### **Priority 1: Fix Sink Threshold Bug** (30 minutes)
**Problem:** `SinkFormationThreshold` parameter exists but doesn't affect sink formation  
**Impact:** Users can't control when sinks form  
**Location:** Needs investigation in `SimulationStepper` or step phases  

**Action:**
1. Search codebase for where sinks are created
2. Find why threshold isn't checked
3. Add threshold check before `AssignOrMergeAtCell` call
4. Test with different threshold values

---

### **Priority 2: Add FixedCountStrategy** (1 hour)
**Problem:** No way to specify exact number of sinks  
**Impact:** Can't create controlled experiments  

**Action:**
1. Create `SinkFormationStrategy` abstract base
2. Extract current logic ? `BoundaryAccumulationStrategy`
3. Implement `FixedCountStrategy`:
   - User specifies count
   - Places at resource maxima (or random)
   - Places at specific tick
4. Add strategy selection to configuration

---

### **Priority 3: Color Scheme System** (2-3 hours)
**Problem:** Single hardcoded color limits scientific use  
**Impact:** Can't match domain conventions, accessibility issues  

**Action:**
1. Create `ColorScheme` ScriptableObject
2. Implement gradient types
3. Create 5 preset schemes
4. Update `GridRenderer` to use schemes
5. Add scheme reference to presets

---

## ?? **Recommended Implementation Order**

### **Option A: Quick Fixes First** ? RECOMMENDED
```
Day 1 Morning (2 hrs):
  1. Fix SinkFormationThreshold bug
  2. Add FixedCountStrategy
  ? Unblocks Stage 11 testing

Day 1 Afternoon (2-3 hrs):
  3. Color scheme system
  ? Makes visualization flexible

Day 2 (4-5 hrs):
  4. Additional sink strategies
  5. Domain-specific extensions
  ? Cross-domain ready!
```

**Total:** ~2 days focused work

---

### **Option B: Complete Stage 11 First**
```
Now (20 min):
  - Create 5 preset assets in Unity
  - Test what works now
  ? Get Stage 11 done

Later (when time permits):
  - Implement Stages 12-14
  ? Full generalization
```

---

## ?? **What Each Fix Enables**

### **After Sink Fixes:**
- ? `SinkFormationThreshold` actually works
- ? Users can specify exact sink count
- ? Can test controlled scenarios
- ? Multiple formation mechanisms available

### **After Color System:**
- ? Scientific field-specific palettes
- ? Colorblind-friendly options
- ? Publication-ready visuals
- ? Domain-appropriate colors

### **After Both:**
- ? Framework truly domain-agnostic
- ? Usable by any scientific field
- ? Publication-ready
- ? User-configurable

---

## ?? **Current Stage 11 Status**

**Documentation:** ? Complete  
**Unity Assets:** ? Awaiting creation (20 minutes)

**Can complete Stage 11 now with:**
- Current sink mechanism (boundary accumulation)
- Current colors (hardcoded)
- Document known limitations

**Or wait for fixes:**
- Better sink control
- Flexible colors
- More complete system

---

## ?? **My Recommendation**

**Do this NOW:**
1. ? Create Stage 11 presets in Unity (20 min)
   - Use current system (it works, just limited)
   - Document limitations in README
   - Commit Stage 11 as "complete with known limitations"

**Do this NEXT SESSION:**
2. ? Stage 13 (Sink fixes) - 2 hours
3. ?? Stage 12 (Colors) - 2-3 hours
4. ?? Stage 14 (Extensions) - 4 hours

**Why:**
- Stage 11 provides value NOW (example presets)
- Fixes can wait for dedicated session
- Progress > perfection
- You'll have working examples to test fixes against

---

## ?? **Reference Documents**

When ready to implement:

**Sink Formation:**
- Design: `SINK_FORMATION_DESIGN.md`
- Roadmap: `POST_STAGE11_ROADMAP.md`
- Current code: `Assets/Viable/Engine/Logic/SinkLogic.cs`

**Color System:**
- Roadmap: `POST_STAGE11_ROADMAP.md` (Stage 12 section)
- Current code: `Assets/Viable/Core.Unity/Rendering/GridRenderer.cs`

**Integration:**
- Configuration: `Assets/Viable/Engine/Configuration/SimulationConfiguration.cs`
- Presets: `Assets/Viable/Core.Unity/ScenarioPreset.cs`

---

## ? **Success Criteria (After All Fixes)**

### **Visualization:**
- [ ] 5+ color schemes available
- [ ] Runtime scheme switching
- [ ] Domain-appropriate palettes
- [ ] Accessibility options

### **Sink Formation:**
- [ ] `SinkFormationThreshold` works correctly
- [ ] User can specify exact sink count
- [ ] 5+ formation strategies
- [ ] Domain-specific mechanisms

### **Cross-Domain:**
- [ ] Usable by physicists, biologists, economists, etc.
- [ ] No single-domain assumptions
- [ ] Flexible configuration
- [ ] Well-documented analogies

---

## ?? **Next Steps**

**Right now:**
- Create Stage 11 presets (20 min)
- Commit with "known limitations" note
- Mark Stage 11 complete

**Next session:**
- Start with `SINK_FORMATION_DESIGN.md`
- Implement Phase 1 (fix threshold bug)
- Implement Phase 2 (FixedCountStrategy)
- Test thoroughly

---

**These issues don't block Stage 11 - proceed with preset creation!** ??

