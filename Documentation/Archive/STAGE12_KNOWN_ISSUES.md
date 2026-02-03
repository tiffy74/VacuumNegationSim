# Stage 12 - Known Issues & Future Work

**Stage Status:** ? Complete  
**Date:** 2024-01-21

---

## ?? Known Issues (Non-Critical)

### **Issue #1: Export Path Display Not Working**
**Priority:** Low  
**Status:** Deferred to Future Stage

#### Description
The Export button successfully exports run data to disk, but the ExportUI panel doesn't display the export path in the UI.

#### Current Behavior
- ? Export button works correctly
- ? Files are created in expected location
- ? Console shows export path
- ?? UI `ExportPathText` field doesn't update
- ?? "Open Folder" button not implemented

#### Expected Behavior
- Export path should appear in `ExportPathText` field
- "Open Folder" button should open the export directory
- Success feedback should be more prominent

#### Impact
- **Functionality:** ? Working - Export creates files successfully
- **User Experience:** ?? Reduced - User must check Console for path
- **Severity:** Low - Does not prevent usage

#### Root Cause
Likely one of:
1. `ExportUI.OnExportClicked()` not using `ExportLastRunWithPath()` return value
2. `ExportPathText` field not wired in Inspector
3. Path not being stored/displayed correctly

#### Workaround
Users can:
1. Check Unity Console for export path (shown in Debug.Log)
2. Navigate to default export location manually
3. Export still creates all required files

#### Fix Required (Future)
```csharp
// In ExportUI.cs, update OnExportClicked():
private void OnExportClicked()
{
    if (simulationController == null) return;

    if (feedbackText != null)
        feedbackText.text = "Exporting...";

    // Get export path from controller
    string exportPath = simulationController.ExportLastRunWithPath();

    if (exportPath != null)
    {
        // Display path in UI
        if (exportPathText != null)
            exportPathText.text = $"Exported to: {exportPath}";

        if (feedbackText != null)
            feedbackText.text = "Export successful!";

        // Enable "Open Folder" button
        if (openFolderButton != null)
            openFolderButton.gameObject.SetActive(true);
    }
    else
    {
        if (feedbackText != null)
            feedbackText.text = "Export failed - no run data";
    }
}

// Add Open Folder functionality:
private void OnOpenFolderClicked()
{
    if (lastExportPath != null && System.IO.Directory.Exists(lastExportPath))
    {
        System.Diagnostics.Process.Start("explorer.exe", lastExportPath);
    }
}
```

#### Testing Required
1. Verify path display updates
2. Verify "Open Folder" button works
3. Test on Windows, Mac, Linux (if applicable)

#### Estimated Fix Time
30 minutes - 1 hour

---

## ?? Deferred Features (Stage 12 Scope)

### **Feature #1: Parameter Editor UI**
**Status:** Script exists, UI not implemented  
**Priority:** Optional

#### Description
`ParameterEditorUI.cs` was created but the Unity UI wasn't built.

#### Current State
- ? Script created
- ? UI panel not created
- ? Inspector fields not wired
- ? Not tested

#### If Implementing (Future)
1. Create `ParameterEditorPanel` (bottom-left)
2. Add text fields for key parameters
3. Add "Apply" and "Reset" buttons
4. Wire to UIManager
5. Test parameter changes at runtime

#### User Impact
None - Feature is optional and wasn't required for Stage 12 completion.

---

### **Feature #2: Keyboard Shortcuts**
**Status:** Not implemented  
**Priority:** Polish

#### Suggested Shortcuts
- **Space:** Play/Pause
- **R:** Restart simulation
- **E:** Export
- **1-5:** Load presets 1-5
- **+/-:** Increase/decrease speed

#### Implementation
Add Input handling to UIManager or individual UI components.

---

### **Feature #3: Tooltips**
**Status:** Not implemented  
**Priority:** Polish

#### Suggested Tooltips
- Hover over buttons to see keyboard shortcuts
- Hover over metrics to see explanations
- Hover over presets to see detailed descriptions

#### Implementation
Use Unity's built-in Tooltip system or custom tooltip UI.

---

### **Feature #4: Standalone Build Verification**
**Status:** Not tested  
**Priority:** Medium

#### Current State
- ? UI works in Unity Editor
- ? Build scene configured (Viable.unity)
- ? Standalone .exe not tested

#### Testing Required
1. Build Windows standalone
2. Verify UI appears
3. Verify all buttons work
4. Verify export works (path may differ in build)
5. Test on target platforms (Windows/Mac/Linux)

---

## ?? Minor Issues (Non-Blocking)

### **Issue #2: Update Frequency Configurable**
**Current:** InfoDisplayUI updates every frame (frequency = 1)  
**Optimization:** Could be increased to 5-10 for built executables  
**Impact:** Minor performance overhead  
**Fix:** Expose in Inspector, default to 5

---

### **Issue #3: Speed Slider Affects Unity Time.timeScale**
**Current:** Speed slider changes `Time.timeScale`  
**Potential Issue:** May affect non-simulation Unity systems  
**Alternative:** Pass speed to SimulationController directly  
**Impact:** None currently, but could affect future features

---

### **Issue #4: Verbose Debug Logging**
**Current:** Many Debug.Log statements for diagnostics  
**Future:** Add log level control (Debug/Info/Warning/Error)  
**Impact:** Console clutter in built executables  
**Fix:** Conditional compilation or log level system

---

## ?? Technical Debt

### **1. Inspector Wiring Required**
**Issue:** UI components must be manually wired in Unity Inspector  
**Alternative:** Auto-find components by name/tag  
**Pro:** More flexible, clearer relationships  
**Con:** Requires manual setup, error-prone  
**Status:** Acceptable for current scope

---

### **2. Hard-Coded UI Layout**
**Issue:** Panel positions hard-coded in setup docs  
**Alternative:** Responsive layout system  
**Pro:** Easier to set up initially  
**Con:** Not responsive to different screen sizes  
**Mitigation:** Canvas Scaler handles basic scaling  
**Status:** Acceptable for current scope

---

### **3. No UI Prefab**
**Issue:** UI must be rebuilt for new scenes  
**Alternative:** Create `UICanvas.prefab` for reuse  
**Pro:** Faster setup in new scenes  
**Con:** One-time setup cost  
**Priority:** Low - single scene project

---

## ?? Future Enhancements (Beyond Stage 12)

### **Enhancement #1: Advanced Metrics Display**
- Graphs/charts for metrics over time
- Heatmap visualization toggle
- Metric export to clipboard
- Customizable metric panels

### **Enhancement #2: Preset Management**
- Create new presets from UI
- Edit existing presets
- Save/load from custom locations
- Preset comparison view

### **Enhancement #3: Session Management**
- Save/load simulation state
- Resume from checkpoint
- Replay simulation
- Compare runs side-by-side

### **Enhancement #4: Accessibility**
- Screen reader support
- High contrast mode
- Adjustable font sizes
- Colorblind-friendly palette

### **Enhancement #5: Multi-Language Support**
- Localization system
- Language dropdown
- Translated documentation

---

## ?? Issue Priority Matrix

| Issue | Impact | Effort | Priority | Stage |
|-------|--------|--------|----------|-------|
| Export path display | Low | Low | Low | 13 (Polish) |
| Standalone build test | Medium | Medium | Medium | 13 (Deployment) |
| Parameter Editor UI | Low | High | Optional | 14+ |
| Keyboard shortcuts | Low | Low | Low | 13 (Polish) |
| Tooltips | Low | Low | Low | 13 (Polish) |
| Update frequency | Very Low | Very Low | Very Low | Anytime |
| Log level control | Low | Low | Low | 13 (Polish) |
| UI Prefab | Low | Low | Low | If multi-scene |

---

## ? Acceptance Criteria for "Fixed"

### **Export Path Display:**
- [ ] Path displays in ExportPathText field
- [ ] "Open Folder" button appears after export
- [ ] Clicking "Open Folder" opens directory
- [ ] Works on Windows, Mac, Linux
- [ ] Handles invalid paths gracefully

### **Standalone Build:**
- [ ] UI visible in built executable
- [ ] All buttons functional
- [ ] Metrics update correctly
- [ ] Export creates files in correct location
- [ ] No runtime errors

---

## ?? Review Checklist (Before Stage 13)

Before starting Stage 13 (Polish & Deployment):

- [ ] Review all known issues
- [ ] Prioritize fixes based on user feedback
- [ ] Test standalone build thoroughly
- [ ] Document any new issues found
- [ ] Update this document with findings

---

## ?? Reporting New Issues

**For future developers/users:**

When reporting issues, include:
1. **Description:** What's not working?
2. **Steps to Reproduce:** How to trigger the issue?
3. **Expected Behavior:** What should happen?
4. **Actual Behavior:** What actually happens?
5. **Impact:** How does it affect usage?
6. **Workaround:** Any temporary solutions?
7. **Environment:** Unity version, OS, build settings

---

## ?? Developer Notes

### **Why Export Path Display Was Deferred:**

1. **Core functionality works** - Export creates files successfully
2. **Console provides path** - Workaround available
3. **Time constraint** - Prioritized getting metrics display working
4. **Low user impact** - Doesn't prevent tool usage
5. **Easy fix** - Can be addressed quickly in future

### **Decision Rationale:**

Stage 12 goal was "Essential UI for non-programmers."

**Essential:** ? Achieved
- Preset selector works
- Play/Pause/Stop works
- Metrics display works
- Export works

**Polish:** ?? Deferred
- Export path display (nice-to-have)
- Open folder button (convenience)
- Parameter editor (advanced feature)

**Verdict:** Stage 12 goals met. Polish can wait for Stage 13.

---

**Document Status:** Living document - update as issues discovered/resolved  
**Last Updated:** 2024-01-21  
**Next Review:** Before Stage 13 kickoff
