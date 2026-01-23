# ?? BUILD SHOWS OLD VERSION - QUICK FIX

## TL;DR - Most Likely Issue

**You added the UI to the wrong scene.**

**Build uses:** `Main.unity`  
**You edited:** `Viable.unity` (probably)

---

## ? Immediate Action

### 1. Run Diagnostic (2 minutes)

**In Unity:**
```
1. Open Main.unity
2. Add empty GameObject
3. Add Component: Stage12DiagnosticCheck
4. Press Play
5. Check Console
```

**If you see ? MISSING:**
? Main.unity doesn't have UI
? **Fix below**

---

## ? Quick Fix (5 minutes)

### Option A: Copy UI to Main.unity

```
1. Open Viable.unity (or wherever you added UI)
2. Select:
   - UIManager GameObject
   - UICanvas GameObject (with all children)
   - SimulationManager GameObject (if not in Main.unity)
3. Ctrl+C (Copy)
4. Open Main.unity
5. Ctrl+V (Paste)
6. Save (Ctrl+S)
7. File > Build Settings > Build
```

### Option B: Change Build Scene

```
1. File > Build Settings
2. Remove Main.unity
3. Add Viable.unity (drag from Project window)
4. Ensure Viable.unity is at index 0 (top of list)
5. Build
```

---

## ? Verify Fix

**After fix:**
```
1. Open the scene you're building (Main.unity or Viable.unity)
2. Press Play in Editor
3. You should see:
   - Preset dropdown (top-left)
   - Play/Pause buttons (top-center)
   - Info display (top-right)
   - Export button (bottom-right)
```

**If you see UI in Editor:** ? Scene is correct

**Then rebuild:**
```
1. Delete entire Build folder
2. File > Build Settings > Build
3. Run executable
4. UI should now appear in built version
```

---

## ?? Root Cause

**What happened:**
1. You created all the UI scripts ?
2. You followed Stage 12 setup in **some scene**
3. But Unity builds **Main.unity** (Build Settings)
4. Main.unity doesn't have the UI components yet
5. Build = old version

**Solution:** Get UI into the scene that's building (Main.unity)

---

## ?? Still Broken?

**Try this:**
1. Close Unity completely
2. Delete entire `Build` folder
3. Delete `Library/PlayerDataCache` folder
4. Reopen Unity
5. Open Main.unity
6. Verify UI is there (Play mode)
7. Build again

**If STILL broken:**
- Share screenshot of Build Settings window
- Share screenshot of Main.unity Hierarchy
- I'll debug further

---

## ?? Full Documentation

See: `STAGE12_BUILD_TROUBLESHOOTING.md` for complete guide

---

**Bottom line:** The UI exists in code, but not in the scene Unity is building. Copy it over!
