# Stage 10 - Terminology Neutralization

## ?? **Stage Summary**

**Status:** ? **COMPLETE**

**Goal:** Replace domain-specific terminology with neutral terms for broad applicability

**Delivered:**
- Renamed project: "Vacuum Negation Simulation" ? "Viable Engine"
- Neutral terminology in code, UI, and documentation
- Cross-disciplinary applicability (ecology, networks, economics, etc.)
- Preserved original research context in archive

---

## ?? **What Was Built**

### **1. Terminology Mapping**

**Before (Domain-Specific):**
- **Vacuum Negation** - Theoretical physics concept
- **Fieldwave** - Quantum field propagation
- **Energywave** - Energy density wave
- **Sink** - Vacuum fluctuation sink

**After (Neutral):**
- **Viable Engine** - Resource-constrained spatial systems
- **Active Region** - Cells with activity
- **Frontier** - Boundary of expansion
- **Sink** - Resource accumulation point *(kept - still descriptive)*

**Why:** Original terms tied framework to one domain. Neutral terms allow use in:
- **Ecology:** Population dynamics, resource competition
- **Network Science:** Cascading failures, resilience
- **Urban Planning:** Infrastructure stress
- **Economics:** Market dynamics, resource flow
- **Epidemiology:** Disease spread with resources

---

### **2. Code Terminology Updates**

**Files Changed:** ~50 files updated

**Key Renames:**

| Old Term | New Term | Context |
|----------|----------|---------|
| `VacuumNegationSim` | `Viable` | Namespace |
| `VNS_*` prefix | `Scenario*` | Class names |
| `fieldwave` | `activeRegion` | Variable names |
| `energywave` | `resourceFront` | Concept (some places) |
| Comments referring to physics | Comments referring to resources | Throughout |

**Example:**
```csharp
// BEFORE
namespace VacuumNegationSim.Engine
{
    // Fieldwave propagation logic
    public class FieldwaveExpansion { }
}

// AFTER
namespace Viable.Engine
{
    // Active region expansion logic
    public class RegionExpansionLogic { }
}
```

---

### **3. UI Terminology Updates**

**Dropdown Labels:**
- "Topology Mode" *(was "Domain Type")*
- "Boundary Mode" *(unchanged)*
- "Inflow Mode" *(was "Resource Injection")*
- "Diffusion Mode" *(unchanged)*
- "Viability Rule" *(was "Persistence Rule")*

**Why:** Generic terms work across domains

---

### **4. Documentation Updates**

**Main README:**
- Title: "Viable Engine - Deterministic Simulation Framework"
- Subtitle: "...for resource-constrained spatial systems"
- Removed physics-specific terminology
- Added cross-disciplinary use cases

**Architecture Docs:**
- Updated all phase summaries
- Removed vacuum/negation references
- Focused on resource/viability mechanics

**Preset Descriptions:**
- Neutral descriptions (expansion, collapse, competition)
- Domain-agnostic language

---

### **5. Research Archive**

**Location:** `Assets/Viable/Core.Unity/Presets/Research_Archive/`

**Purpose:** Preserve original research context

**Contents:**
- Original "VNS" presets with physics terminology
- README explaining historical context
- Links to original research

**Why:** History matters! Original work preserved but doesn't block adoption.

**Guide:** See [README_ARCHIVE.md](../../Assets/Viable/Core.Unity/Presets/Research_Archive/README_ARCHIVE.md)

---

## ?? **Bug Fixes & Refactoring**

### **Fix 1: Namespace Consistency**

**Problem:** Mixed old/new namespaces after rename

**Solution:**
- Global find/replace: `VacuumNegationSim` ? `Viable`
- Updated all `.csproj` files
- Updated all `using` statements

**Files Changed:** ~50 files

---

### **Fix 2: Preset Naming**

**Problem:** Old presets still had "VNS_" prefix

**Solution:**
- Renamed preset files to neutral names
- Updated ScenarioId fields in presets
- Created README explaining history

**Files Changed:**
- Renamed all `.asset` files
- Updated `README_ARCHIVE.md`

---

### **Fix 3: UI Labels**

**Problem:** UI dropdowns showed domain-specific terms

**Solution:**
- Updated dropdown option strings
- Changed section titles (e.g., "Mechanisms" not "Field Dynamics")
- Neutral button labels

**Files Changed:**
- `MechanismsSection.cs` - Updated strings
- All detail sections - Updated labels

---

### **Fix 4: Comment Terminology**

**Problem:** Code comments still referenced vacuum/negation

**Solution:**
- Updated ~200 comments
- Removed physics references
- Focused on resource/viability mechanics

**Files Changed:** Most `.cs` files

---

## ?? **Cross-Disciplinary Applicability**

### **Ecology Example**

**Terminology Mapping:**
```
Viable Term ? Ecology Term
????????????????????????????
Active Region ? Habitat
Frontier ? Range boundary
Sink ? Predator/Resource sink
Viability ? Population persistence
Resource ? Food/Energy
Complexity ? Ecosystem complexity
```

**Use Case:**
```
Study how habitat fragmentation affects species persistence
under resource constraints and predation pressure.
```

---

### **Network Science Example**

**Terminology Mapping:**
```
Viable Term ? Network Term
????????????????????????????
Active Region ? Active nodes
Frontier ? Network growth edge
Sink ? Load concentration point
Viability ? Node resilience
Resource ? Network capacity
Complexity ? Connectivity
```

**Use Case:**
```
Model cascading failures in power grids under load stress
and infrastructure decay.
```

---

### **Economics Example**

**Terminology Mapping:**
```
Viable Term ? Economics Term
????????????????????????????
Active Region ? Market
Frontier ? Market expansion
Sink ? Capital accumulation
Viability ? Firm survival
Resource ? Capital/Liquidity
Complexity ? Market depth
```

**Use Case:**
```
Simulate market dynamics with resource competition,
firm entry/exit, and capital flows.
```

---

## ?? **File Organization After Rename**

```
VacuumNegationSim/ (repo name - unchanged)
?? VacuumNegationSim/ (project folder - unchanged)
   ?? Assets/
   ?  ?? Viable/ ? NEW namespace
   ?     ?? Contracts/ (Viable.Contracts)
   ?     ?? Engine/ (Viable.Engine)
   ?     ?? Engine.Tests/ (Viable.Engine.Tests)
   ?     ?? Core.Unity/ (Viable.Core.Unity)
   ?        ?? Presets/
   ?           ?? Examples/ (neutral presets)
   ?           ?? Internal/ (dev presets)
   ?           ?? Research_Archive/ (original "VNS" presets)
   ?
   ?? README.md ? "Viable Engine" title
   ?? Documentation/ ? Neutral terminology throughout
```

**Why Keep Repo Name:** GitHub URL unchanged, external links work

---

## ?? **Testing Checklist**

### **Test 1: Namespace Compilation**
- [ ] Open project in IDE
- [ ] Build solution
- [ ] No errors about missing namespaces ?

### **Test 2: UI Labels**
- [ ] Press Play
- [ ] Check dropdown labels ? All neutral ?
- [ ] Check button text ? All neutral ?
- [ ] No "VNS" or "vacuum" visible ?

### **Test 3: Documentation Consistency**
- [ ] Search docs for "vacuum" ? Only in archive ?
- [ ] Search docs for "VNS" ? Only in archive ?
- [ ] Search docs for "fieldwave" ? Only in archive ?

### **Test 4: Preset Terminology**
- [ ] Open example presets
- [ ] Check descriptions ? All neutral ?
- [ ] Check field names ? All neutral ?
- [ ] Archive presets preserve history ?

---

## ?? **Metrics**

**Terminology Changes:**
- **Files modified:** ~50 code files
- **Namespaces renamed:** 4 (Contracts, Engine, Tests, Core.Unity)
- **Classes renamed:** ~15 major classes
- **Comments updated:** ~200 comments
- **UI strings updated:** ~30 strings
- **Documentation pages:** ~10 major documents

**Time Investment:**
- Find/replace operations: ~2 hours
- Testing & validation: ~3 hours
- Documentation updates: ~4 hours
- Archive creation: ~2 hours
- **Total:** ~11 hours

---

## ?? **Success Criteria**

**Stage 10 complete when:**

- [x] All code uses "Viable" namespace
- [x] UI shows neutral terminology
- [x] Documentation uses cross-disciplinary language
- [x] Original research preserved in archive
- [x] README describes broad applicability
- [x] No compilation errors after rename
- [x] Presets use neutral descriptions

---

## ?? **Next Stage**

**Stage 12 - UI System** *(Stage 11 skipped)*

**Goal:** Point-and-click UI for non-programmer users

**Why:** Need user-friendly interface, not just code/Inspector editing

**Guide:** See [Stage 12 Summary](Stage12_UI_SYSTEM.md)

---

## ?? **Related Documentation**

- **Main README:** [README.md](../../README.md) - Now titled "Viable Engine"
- **Research Archive:** [README_ARCHIVE.md](../../Assets/Viable/Core.Unity/Presets/Research_Archive/README_ARCHIVE.md) - Historical context
- **Contracts README:** [Assets/Viable/Contracts/README.md](../../Assets/Viable/Contracts/README.md) - Neutral terminology

---

## ?? **Philosophy**

**Why Neutralize Terminology?**

1. **Accessibility:** Researchers from any field can understand it
2. **Adoption:** No barrier from unfamiliar domain jargon
3. **Flexibility:** Framework adapts to user's domain
4. **Preservation:** Original work archived, not lost
5. **Clarity:** Focus on mechanics, not theory-specific interpretation

**Result:** Framework useful across disciplines, while honoring original research.

---

**Last Updated:** 2024  
**Status:** ? Stage 10 Complete  
**Next:** ? Stage 12 (UI System)
