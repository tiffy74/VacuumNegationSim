# Internal Presets - DO NOT MODIFY

**Status:** ?? **PROTECTED FOR REPRODUCIBILITY**

---

## ?? **Warning**

This folder contains presets used for:
1. **Research validation** - Verifying Engine behavior matches original simulation
2. **Manuscript preparation** - Generating figures and data for publication
3. **Determinism testing** - Ensuring reproducibility of results

**DO NOT:**
- ? Delete these presets
- ? Modify parameter values
- ? Rename preset files
- ? Move to public presets folder

**Until Stage 10 (Neutralization) is complete.**

---

## ?? **Presets in This Folder**

### **ConstraintsExpansionDemo.asset**

**Purpose:** Preserves exact behavior of original simulation for:
- Reproducing manuscript figures
- Validating Engine refactor (Phases 1-7)
- Exporting run artifacts (Stage 9)
- Determinism verification

**Parameters:** Frozen from pre-Stage 8 SimulationController Inspector settings

**Seed:** 42 (deterministic execution)

**Grid:** 64×64 cells

**Use Cases:**
- Running original simulation in Unity
- Exporting data for publication
- Regression testing after Engine changes
- Baseline for new presets

---

## ?? **Lifecycle**

### **Stage 8 (Current):**
- ? Preset created
- ? Reproduces original behavior
- ? Protected from accidental changes

### **Stage 9:**
- ? Used for export system validation
- ? Generates run artifacts for manuscript

### **Stage 10 (Future):**
- ?? Preset will be neutralized/renamed
- ?? Theory-specific language removed
- ?? Internal copy archived for reproducibility

### **Stage 11:**
- ?? New domain-neutral presets created
- ?? Internal preset moved to archive

---

## ?? **Validation Checklist**

Before using this preset in production:

- [ ] Verify visual output matches original simulation
- [ ] Run all 25 Engine tests
- [ ] Verify determinism (same seed = same result)
- [ ] Export run artifact and validate format
- [ ] Compare metrics to pre-refactor baseline

---

## ??? **Protection Mechanism**

1. **Folder Structure:** `Internal/` subfolder prevents accidental public use
2. **Documentation:** This README warns against modifications
3. **Version Control:** Git tracks all changes to preset
4. **Export Archive:** Stage 9 will create export artifacts as backup

---

## ?? **If You Need to Modify**

**Instead of modifying this preset:**

1. **Duplicate it:**
```
Right-click ? Duplicate
Rename to: "ConstraintsExpansionDemo_Tuned"
```

2. **Modify the duplicate:**
- Change parameters as needed
- Update description to note changes
- Use different seed to distinguish runs

3. **Keep original intact:**
- Original remains for reproducibility
- Can always revert to known-good state

---

## ?? **Related Documentation**

- [Stage 8 Implementation](../../../STAGE8_IMPLEMENTATION.md)
- [ScenarioPreset Usage](../../../README_PRESETS.md) (Stage 11)
- [Export System](../../../STAGE9_IMPLEMENTATION.md) (Stage 9)

---

**Last Updated:** Stage 8 (Productization)  
**Maintained By:** Architecture Team  
**Contact:** See main README for questions

