# Stage 12 Build Troubleshooting

## ?? Build Reverts to Old Version - Fix Guide

### Issue
Built standalone shows old simulation instead of new UI system.

---

## ?? **IMMEDIATE ACTION: Diagnose Which Scene Has UI**

### Step 1: Run Diagnostic Check

1. **Open Unity**
2. **Open scene:** `Assets/Scenes/Main.unity`
3. **Create empty GameObject** (Right-click Hierarchy ? Create Empty)
4. **Add Component:** `Stage12DiagnosticCheck`
5. **Press Play**
6. **Check Console** for results

**Expected output:**
```
=== Stage 12 Scene Diagnostic ===
SimulationController: ? FOUND
UIManager: ? FOUND
PresetSelectorUI: ? FOUND
SimulationControlsUI: ? FOUND
InfoDisplayUI: ? FOUND
ExportUI: ? FOUND
Current Scene: Main
=== ? Stage 12 Setup Complete in This Scene ===
```

**If you see ? MISSING:**
? **Main.unity doesn't have Stage 12 setup!**
? **Solution:** Follow instructions below to add UI to Main.unity

---

## ? Fix Step 1: Verify Build Scene

**In Unity:**
1. Go to `File > Build Settings`
2. Check **Scenes In Build** list
3. **Current scene should be:** `Assets/Scenes/Main.unity` ? (confirmed in your project)

**Status:** ? Build scene is correct!

**Problem:** Main.unity might not have the UI components yet.

---

## ? Fix Step 2: Check Which Scene Has UI

You have multiple scenes. **Which one did you add the UI to?**

**Scenes in project:**
- `Assets/Scenes/Main.unity` ? **This is what builds**
- `Assets/Viable/Viable.unity` ? Old scene?
- `Assets/Pixel UI Grey Kit/Scenes/Demo Scene.unity` ? UI kit demo

**Run the diagnostic in EACH scene:**

### Check Main.unity
1. Open `Main.unity`
2. Add diagnostic GameObject
3. Press Play
4. ? or ??

### Check Viable.unity  
1. Open `Viable.unity`
2. Add diagnostic GameObject
3. Press Play
4. ? or ??

**Result:**
- **If Main.unity has ?** and **Viable.unity has ?**:
  ? You added UI to the wrong scene!
  ? **Fix:** Copy UI from Viable.unity to Main.unity OR change build scene

---

## ? Fix Step 3: Copy UI to Correct Scene

**If UI is in wrong scene:**

### Option A: Copy UI GameObjects (Quick)
1. Open scene with UI (e.g., `Viable.unity`)
2. Select all UI GameObjects:
   - `UIManager`
   - `UICanvas` (with all children)
3. **Copy:** Ctrl+C
4. Open `Main.unity`
5. **Paste:** Ctrl+V
6. **Save:** Ctrl+S
7. **Rebuild**

### Option B: Change Build Scene (Quick)
1. `File > Build Settings`
2. **Remove Main.unity** from Scenes In Build
3. **Add Viable.unity** to Scenes In Build
4. Ensure it's at **index 0**
5. **Rebuild**

---

## ? Fix Step 4: Clear Build Cache

Unity may be using cached data. Clear it:

**Option A: Delete Build Folder**
```
1. Delete entire "Build" or "Builds" folder
2. Rebuild from scratch
```

**Option B: Clean Build**
```
1. File > Build Settings
2. Click "Clean Build" (if available)
3. OR: Delete /Library/PlayerDataCache/
```

---

## ? Fix Step 5: Verify Scene Setup

**Open your main scene** (`Main.unity` or scene you're building)

**Required hierarchy:**

```
Hierarchy:
?? SimulationManager
?  ?? SimulationController (script)
?? UIManager (script)
?? UICanvas
?  ?? PresetSelectorPanel (PresetSelectorUI script)
?  ?? SimulationControlsPanel (SimulationControlsUI script)
?  ?? InfoDisplayPanel (InfoDisplayUI script)
?  ?? ExportPanel (ExportUI script)
?? Main Camera
?? EventSystem
```

**If this hierarchy doesn't exist in Main.unity:**
? **You need to add it!** Follow `STAGE12_UNITY_SETUP.md`

---

## ? Fix Step 6: Force Reimport

Sometimes Unity doesn't detect script changes:

```
1. Right-click Assets/Viable/Core.Unity folder
2. Select "Reimport"
3. Wait for reimport to complete
4. Try building again
```

---

## ? Fix Step 7: Development Build

To debug built executable:

```
File > Build Settings
? Development Build
? Script Debugging (optional)

Build ? Run ? Check Console logs
```

---

## ?? How to Identify Which Scene Is Building

**In your built exe:**
- **Old version**: No UI panels, old visualization
- **New version**: UI panels visible (top-left, top-right, etc.)

**Quick test:**
1. Run executable
2. Look for **dropdown in top-left** (Preset Selector)
3. Look for **Play/Pause buttons** at top-center
4. **If missing** ? Wrong scene or scene not set up

---

## ?? Most Likely Causes (In Order)

### 1. **UI Added to Wrong Scene** (70% probability)
**Symptoms:**
- Editor Play mode shows UI
- Built exe shows old version
- You edited `Viable.unity` but `Main.unity` is building

**Fix:**
- Copy UI from Viable.unity to Main.unity
- OR: Change build scene to Viable.unity

### 2. **Scene Not Saved** (15%)
**Fix:** Press Ctrl+S before building!

### 3. **Cached Assets** (10%)
**Fix:** Delete Build folder, rebuild from scratch

### 4. **Wrong Scene Index** (5%)
**Fix:** Ensure correct scene at index 0 in Build Settings

---

## ? Verification Checklist

Before building again:

- [ ] Run `Stage12DiagnosticCheck` in scene
- [ ] All components found (?)
- [ ] Correct scene open in Unity
- [ ] Scene saved (Ctrl+S)
- [ ] Build Settings ? Correct scene at index 0
- [ ] Build folder deleted (fresh build)
- [ ] Test in Editor first (Press Play, see UI?)

---

## ?? Quick Test

### In Unity Editor:
1. Open `Main.unity`
2. Press Play
3. **Do you see UI panels?**
   - **YES**: Scene is correct, just rebuild
   - **NO**: UI not in this scene! Add it or switch build scene

### In Built Executable:
1. Run the .exe
2. **Do you see UI panels?**
   - **YES**: Fixed! ?
   - **NO**: Wrong scene building OR cache issue

---

## ?? Stage 12 Scene Setup Status

**Main.unity must have:**
1. ? All UI scripts exist in project
2. ? UICanvas with panels in scene
3. ? UIManager in scene
4. ? SimulationController in scene
5. ? All references wired in Inspector
6. ? Resources folder has presets
7. ? Scene saved before build

**If any ?:** Follow `STAGE12_UNITY_SETUP.md` to add missing components

---

## ?? Still Not Working?

**Share these screenshots:**
1. `File > Build Settings` window
2. Main.unity Hierarchy view
3. Console output from `Stage12DiagnosticCheck`

**Then try:**
- Delete entire Build folder
- Close Unity
- Reopen Unity
- Open Main.unity
- Verify UI is there (Play mode)
- Build again

---

## ?? Prevention for Future

**Workflow:**
1. Work in ONE scene (e.g., Main.unity)
2. Save often (Ctrl+S)
3. Test in Editor before building (Press Play)
4. Only build after confirming Editor works
5. Keep Build Settings pointing to that one scene

**Avoid:**
- Switching between multiple scenes without checking build settings
- Building without testing in Editor first
- Forgetting to save scene

---

## ?? Understanding the Issue

**What happened:**
- You created UI components (scripts exist ?)
- You added them to **A scene** (maybe Viable.unity)
- But **Main.unity** is building (Build Settings)
- Main.unity doesn't have the UI yet
- Build uses Main.unity ? old version

**Solution:**
- Get UI into Main.unity
- OR: Make Viable.unity the build scene

---

**Run the diagnostic first, then we'll know exactly what's wrong!**
