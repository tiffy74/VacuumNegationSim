# ?? Right Dock Setup - Unity Editor Guide

## **Overview**
Moving preset controls from TopBar to RightDock for cleaner UX:
- **Load Preset** button loads into UI (shows notification)
- **Apply** button at bottom applies ALL changes and restarts simulation

---

## **? Step 1: Create Preset Controls Section**

### **Hierarchy:**
```
Canvas/
??? RightDock/
    ??? SetupPanel/
        ??? PresetControlsSection (NEW)
        ?   ??? Title (TextMeshProUGUI: "PRESETS")
        ?   ??? PresetDropdown (TMP_Dropdown)
        ?   ??? LoadButton (Button: "Load Preset")
        ?
        ??? MechanismsSection (existing)
        ?   ??? ... (existing mechanisms)
        ?
        ??? ApplyButton (NEW - at bottom)
            ??? Button (Button: "Apply")
```

---

## **? Step 2: Setup PresetControlsSection GameObject**

### **Create GameObject:**
1. Right-click `SetupPanel` ? Create Empty
2. Rename to `PresetControlsSection`
3. Add **VerticalLayoutGroup** component:
   - Padding: `10, 10, 10, 10`
   - Spacing: `8`
   - Child Alignment: Upper Left
   - Child Force Expand: Width ?, Height ?

### **Add Title:**
1. Right-click `PresetControlsSection` ? UI ? Text - TextMeshPro
2. Rename to `Title`
3. Set text: `"PRESETS"`
4. Font size: `16`
5. Style: Bold
6. Color: Light gray `#CCCCCC`

### **Add Preset Dropdown:**
1. Right-click `PresetControlsSection` ? UI ? Dropdown - TextMeshPro
2. Rename to `PresetDropdown`
3. Set placeholder text: `"Select Preset..."`
4. Template position: Below
5. **Min Height:** `30`
6. **Items auto-populated** (topology/adjacency handled in MechanismsSection)

### **Add Load Button:**
1. Right-click `PresetControlsSection` ? UI ? Button - TextMeshPro
2. Rename to `LoadButton`
3. Set button text: `"Load Preset"`
4. Button size: `Width: 280, Height: 35`
5. Colors:
   - Normal: `#4A9FD8` (Blue)
   - Highlighted: `#60B5EE`
   - Pressed: `#3A8FC8`

### **Add PresetControlsSection Script:**
1. Select `PresetControlsSection` GameObject
2. Add Component ? `PresetControlsSection`
3. Assign references:
   - **Preset Dropdown** ? Drag `PresetDropdown`
   - **Load Preset Button** ? Drag `LoadButton`

---

## **? Step 3: Setup Apply Button (Bottom of Setup Panel)**

### **Create GameObject:**
1. Right-click `SetupPanel` (at the **END** of children list)
2. UI ? Button - TextMeshPro
3. Rename to `ApplyButton`

### **Configure Button:**
1. Set button text: `"Apply"`
2. Button size: `Width: 100%, Height: 45`
3. **Anchor:** Stretch bottom
4. **Position:** Bottom of panel
5. Colors:
   - Normal: `#4CAF50` (Green)
   - Highlighted: `#66BB6A`
   - Pressed: `#3C9F40`
6. Font size: `18`
7. Style: Bold

### **Add ApplyButton Script:**
1. Select `ApplyButton` GameObject
2. Add Component ? `ApplyButton`
3. Assign references:
   - **Apply Button** ? Drag `Button` component

---

## **? Step 4: Create Notification UI (Bottom-Right Corner)**

### **Create GameObject:**
1. Right-click `Canvas` (at ROOT level)
2. Create Empty
3. Rename to `NotificationUI`

### **Setup RectTransform:**
1. **Anchor Preset:** Bottom-Right
2. **Pivot:** `(1, 0)` (bottom-right corner)
3. **Position:** 
   - X: `-20` (20px from right edge)
   - Y: `20` (20px from bottom edge)
4. **Size:** `Width: 300, Height: 80`

### **Add Background Panel:**
1. Right-click `NotificationUI` ? UI ? Image
2. Rename to `Panel`
3. **RectTransform:** Stretch all (fill parent)
4. **Image:**
   - Sprite: UI Sprite (rounded corners if available)
   - Color: `#2C3E50` (Dark gray-blue)
   - Material: Default
5. Add **Shadow** component for depth

### **Add Text:**
1. Right-click `Panel` ? UI ? Text - TextMeshPro
2. Rename to `NotificationText`
3. **RectTransform:** Stretch all with padding `10, 10, 10, 10`
4. **Text:**
   - Font size: `16`
   - Alignment: Center
   - Wrapping: Enabled
   - Color: White `#FFFFFF`
   - Auto-size: Best Fit (Min: 12, Max: 16)

### **Add NotificationUI Script:**
1. Select `NotificationUI` GameObject
2. Add Component ? `NotificationUI`
3. Assign references:
   - **Notification Panel** ? Drag `Panel`
   - **Notification Text** ? Drag `NotificationText`
   - **Notification Background** ? Drag `Panel` Image component
4. Configure settings:
   - **Display Duration:** `2.5`
   - **Fade In Duration:** `0.3`
   - **Fade Out Duration:** `0.3`
5. Configure colors (optional - defaults are good):
   - **Info Color:** `#3498DB` (Blue)
   - **Success Color:** `#27AE60` (Green)
   - **Warning Color:** `#F39C12` (Orange)
   - **Error Color:** `#E74C3C` (Red)

---

## **? Step 5: Optional - Hide TopBar Preset Controls**

If you want to **disable** the old TopBar preset controls:

1. Select `TopBar/PresetDropdown`
   - Uncheck "Active" in Inspector (or delete)
2. Select `TopBar/LoadButton`
   - Uncheck "Active" in Inspector (or delete)
3. Select `TopBar/ApplyRestartButton`
   - Uncheck "Active" in Inspector (or delete)

**Keep in TopBar:**
- Play/Pause button ?
- Step button ?
- Restart button ?
- Speed dropdown ?
- Seed input ?
- Export button ?

---

## **? Step 6: Test the Flow**

### **Expected Workflow:**

1. **Select Preset** from dropdown
2. **Click "Load Preset"**
   - ? Notification appears: "? Preset loaded: [PresetName]"
   - ? UI updates with preset parameters
   - ? Simulation does NOT restart yet

3. **Optionally tweak mechanisms** (Grid Topology, Inflow, etc.)

4. **Click "Apply"**
   - ? Notification appears: "? Simulation restarted"
   - ? Simulation restarts with new config

---

## **?? Final Layout**

### **Right Dock ? Setup Panel:**
```
???????????????????????????????????
?  PRESETS                        ?
?  ????????????????????????????   ?
?  ? Select Preset: Default ? ?   ? ? Dropdown
?  ????????????????????????????   ?
?  [        Load Preset        ]  ? ? Button
???????????????????????????????????
?  MECHANISMS                     ?
?  Grid Topology:  Rect (edges)?
?                  Rect (edges+vertices)
?                  Tri (edges)
?                  Tri (edges+vertices)
?                  Hex (edges)
?                  Hex (edges+vertices)
?  Inflow:         Uniform     ?  ?
?  Boundary:       Absorbing   ?  ?
?  Diffusion:      VonNeumann4 ?  ?
?  Viability:      Simple      ?  ?
???????????????????????????????????
?  [          Apply          ]    ? ? Button (Green)
???????????????????????????????????
```

### **Notification (Bottom-Right):**
```
                    ????????????????????????
                    ?  ? Preset loaded:    ?
                    ?     Default          ?
                    ????????????????????????
                         ? Fades in/out
```

---

## **?? Visual Polish Tips**

### **Buttons:**
- Use **rounded corners** (9-slice sprites)
- Add **drop shadows** for depth
- Ensure **hover feedback** is visible (color change)

### **Notification:**
- Add **drop shadow** to panel
- Use **rounded corners**
- Ensure text is **readable** against all background colors

### **Spacing:**
- Use **VerticalLayoutGroup** with consistent spacing
- Add **padding** to panels (10-15px)
- Separate sections with **horizontal dividers** (thin lines)

---

## **?? Troubleshooting**

### **Notification doesn't appear:**
- Check `NotificationUI` GameObject is **Active**
- Verify Canvas has **GraphicRaycaster** enabled
- Check `Canvas Scaler` is set to **Scale With Screen Size**

### **Dropdown doesn't populate:**
- Ensure presets exist in `Assets/Viable/Core.Unity/Presets/Examples/`
- Check Console for logs: `[PresetControlsSection] Loaded X presets`
- Verify presets are **ScriptableObjects** of type `ScenarioPreset`

### **Apply doesn't restart simulation:**
- Check `SimulationUIOrchestrator` is in scene
- Verify `SimulationController` is assigned
- Check Console for errors: `[ApplyButton] Apply failed: ...`

---

## **? Checklist**

- [ ] Created `PresetControlsSection` GameObject
- [ ] Added preset dropdown
- [ ] Added "Load Preset" button
- [ ] Wired `PresetControlsSection` script
- [ ] Created `ApplyButton` at bottom of Setup Panel
- [ ] Wired `ApplyButton` script
- [ ] Created `NotificationUI` in bottom-right corner
- [ ] Wired `NotificationUI` script
- [ ] Tested preset loading (notification appears)
- [ ] Tested Apply button (simulation restarts)
- [ ] Optionally disabled old TopBar controls

---

**You're done! The new layout should now work perfectly!** ??
