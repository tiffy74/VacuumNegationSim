# Bugfix: Grid Boundary Yellow Border

**Issue:** When the simulation reaches the grid edges, a thick yellow border forms around the entire boundary.

**Cause:** Frontier detection logic in `GridRenderer` treated grid edges as "frontier" cells.

---

## ?? **The Problem**

### Old Frontier Detection (Broken):
```csharp
// Marked grid edges as frontier!
if (x == 0 || !s.ActiveRegion[s.Idx(x - 1, y)]) isFrontier = true;
else if (x == _w - 1 || !s.ActiveRegion[s.Idx(x + 1, y)]) isFrontier = true;
else if (y == 0 || !s.ActiveRegion[s.Idx(x, y - 1)]) isFrontier = true;
else if (y == _h - 1 || !s.ActiveRegion[s.Idx(x, y + 1)]) isFrontier = true;
```

**Logic flaw:**
- `x == 0` ? True at left edge ? **Always frontier!** ??
- `x == _w - 1` ? True at right edge ? **Always frontier!** ??
- Same for top/bottom edges

**Result:** Entire grid boundary marked yellow (thick border).

---

## ? **The Fix**

### New Frontier Detection (Correct):
```csharp
// Only check INTERNAL neighbors, ignore grid edges
if (x > 0 && !s.ActiveRegion[s.Idx(x - 1, y)]) isFrontier = true;
else if (x < _w - 1 && !s.ActiveRegion[s.Idx(x + 1, y)]) isFrontier = true;
else if (y > 0 && !s.ActiveRegion[s.Idx(x, y - 1)]) isFrontier = true;
else if (y < _h - 1 && !s.ActiveRegion[s.Idx(x, y + 1)]) isFrontier = true;
```

**Fixed logic:**
- `x > 0 && ...` ? Only check left if not at edge ?
- `x < _w - 1 && ...` ? Only check right if not at edge ?
- Same for top/bottom

**Result:** Only **actual expansion frontiers** marked yellow (no border).

---

## ?? **Visual Comparison**

### Before Fix:
```
???????????????????????  ? Yellow border (bad!)
? Y Y Y Y Y Y Y Y Y Y ?
? Y ???????????????? Y ?
? Y ???????????????? Y ?
? Y ???????????????? Y ?
? Y Y Y Y Y Y Y Y Y Y ?
???????????????????????  ? Yellow border (bad!)
```

### After Fix:
```
???????????????????????  ? No yellow border ?
?   ????????????????   ?
?   ???Y??????????Y??  ?  ? Only internal frontier yellow
?   ????????????????   ?
?   ????????????????   ?
???????????????????????  ? No yellow border ?
```

---

## ?? **Test Results**

### Expected Behavior:
1. ? Simulation starts at center
2. ? Yellow frontier expands outward
3. ? When reaching grid edge, yellow disappears
4. ? No thick border around grid
5. ? Only active expansion fronts show yellow

### Actual Result After Fix:
- ? Yellow appears only at active-inactive boundaries
- ? No yellow at grid edges
- ? Clean visualization

---

## ?? **Files Modified**

- `Assets/Scripts/Unity/GridRenderer.cs`
  - Fixed `isFrontier` detection logic
  - Changed `x == 0` ? `x > 0`
  - Changed `x == _w - 1` ? `x < _w - 1`
  - Changed `y == 0` ? `y > 0`
  - Changed `y == _h - 1` ? `y < _h - 1`

---

## ?? **Why This Matters**

**Visual clarity:**
- Old: Can't distinguish grid edge from real frontier
- New: Clear visualization of expansion fronts

**Scientifically:**
- Grid edges are **artificial boundaries** (simulation artifact)
- Real frontiers are **expansion boundaries** (simulation feature)
- Should only visualize real frontiers, not artifacts

---

## ?? **Root Cause Analysis**

The original code was likely trying to detect:
> "Cells at the edge OR cells next to inactive regions"

But the `||` operator caused:
> "Cells at the edge are ALWAYS frontier, regardless of neighbors"

The fix explicitly requires:
> "Cells must have an inactive neighbor WITHIN the grid to be frontier"

---

## ? **Verification Checklist**

- [x] Build succeeds
- [x] No yellow border at grid edges
- [x] Yellow still appears at expansion fronts
- [x] Behavior otherwise unchanged
- [x] No performance impact

---

**Status:** ? FIXED  
**Commit with:** Phase 7 cleanup

