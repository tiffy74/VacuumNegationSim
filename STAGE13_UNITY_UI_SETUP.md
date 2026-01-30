# Stage 13 Unity UI Setup - Mechanism Controls

**Status:** ?? **INSTRUCTIONS**  
**Goal:** Add UI panels for Stage 13 mechanisms (to complement Stage 12 base UI)

---

## ?? **Overview**

Stage 12 gave you the **base UI**:
- PresetSelectorPanel (load presets)
- SimulationControlsPanel (play/pause/stop)
- InfoDisplayPanel (live metrics)
- ExportPanel (export button)

**Stage 13 adds mechanism-specific controls:**
- MechanismSelectorPanel (choose which mechanisms to enable)
- PointSourcesPanel (add/edit point sources)
- BoundaryModePanel (set boundary behavior)
- DiffusionModePanel (choose diffusion type)
- HysteresisPanel (configure threshold hysteresis)
- MaskShapePanel (configure domain masks)
- RefinementPanel (future - stubs only)

---

## ?? **UI Layout**

```
???????????????????????????????????????????????????????????????
? PresetSelector ? SimControls ? InfoDisplay ? [Mechanisms?] ? Top
???????????????????????????????????????????????????????????????
?                                                             ?
?                    Simulation Grid View                     ? Middle
?                                                             ?
???????????????????????????????????????????????????????????????
? [Mechanism Panels - One Visible at a Time]      ? Export   ? Bottom
? (PointSources / Boundaries / Diffusion / etc.)  ?          ?
???????????????????????????????????????????????????????????????
```

**Layout Strategy:**
- **Top bar:** Existing Stage 12 controls + new Mechanism dropdown
- **Bottom bar:** Mechanism-specific panels (swap based on dropdown selection)
- **Right:** Export panel (from Stage 12, unchanged)

---

## ?? **Step 1: Create MechanismSelectorPanel** (10 minutes)

### **1.1 Create Panel**

1. **Right-click UICanvas ? UI ? Panel**
   - Name: `MechanismSelectorPanel`
   - **Rect Transform:**
     - Anchor: Top-Right (below InfoDisplay)
     - Position: (-10, -180, 0)
     - Width: 250, Height: 80

2. **Panel Background:**
   - Color: Semi-transparent black (alpha 0.8)

### **1.2 Add Dropdown**

1. **Right-click MechanismSelectorPanel ? UI ? Dropdown - TextMeshPro**
   - Name: `MechanismDropdown`
   - **Rect Transform:**
     - Anchor: Center-top
     - Position: (0, -20, 0)
     - Width: 220, Height: 30

2. **Dropdown Options:**
   - Default: "No Mechanisms"
   - Option 1: "Point Sources"
   - Option 2: "Boundary Mode"
   - Option 3: "Diffusion Mode"
   - Option 4: "Hysteresis"
   - Option 5: "Mask Shape"
   - Option 6: "Refinement (Future)"

3. **Add Label Above:**
   - Right-click MechanismSelectorPanel ? UI ? Text - TextMeshPro
   - Name: `MechanismLabel`
   - Text: "Stage 13 Mechanisms"
   - Font: Bold, 14pt
   - Position: Above dropdown

### **1.3 Create MechanismSelectorUI Script**

**File:** `Assets/Viable/Core.Unity/UI/MechanismSelectorUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for selecting which Stage 13 mechanism to configure.
    /// Swaps active mechanism panel based on dropdown selection.
    /// </summary>
    public class MechanismSelectorUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown mechanismDropdown;

        [Header("Mechanism Panels")]
        [SerializeField] private GameObject pointSourcesPanel;
        [SerializeField] private GameObject boundaryModePanel;
        [SerializeField] private GameObject diffusionModePanel;
        [SerializeField] private GameObject hysteresisPanel;
        [SerializeField] private GameObject maskShapePanel;
        [SerializeField] private GameObject refinementPanel;

        public event Action<int> OnMechanismChanged;

        public void Initialize()
        {
            if (mechanismDropdown != null)
            {
                mechanismDropdown.onValueChanged.AddListener(OnMechanismSelected);
            }

            // Hide all panels initially
            HideAllPanels();
        }

        private void OnMechanismSelected(int index)
        {
            HideAllPanels();

            // Show selected panel
            switch (index)
            {
                case 0: // No Mechanisms
                    break;
                case 1: // Point Sources
                    if (pointSourcesPanel != null) pointSourcesPanel.SetActive(true);
                    break;
                case 2: // Boundary Mode
                    if (boundaryModePanel != null) boundaryModePanel.SetActive(true);
                    break;
                case 3: // Diffusion Mode
                    if (diffusionModePanel != null) diffusionModePanel.SetActive(true);
                    break;
                case 4: // Hysteresis
                    if (hysteresisPanel != null) hysteresisPanel.SetActive(true);
                    break;
                case 5: // Mask Shape
                    if (maskShapePanel != null) maskShapePanel.SetActive(true);
                    break;
                case 6: // Refinement
                    if (refinementPanel != null) refinementPanel.SetActive(true);
                    break;
            }

            OnMechanismChanged?.Invoke(index);
            Debug.Log($"[MechanismSelectorUI] Selected mechanism: {index}");
        }

        private void HideAllPanels()
        {
            if (pointSourcesPanel != null) pointSourcesPanel.SetActive(false);
            if (boundaryModePanel != null) boundaryModePanel.SetActive(false);
            if (diffusionModePanel != null) diffusionModePanel.SetActive(false);
            if (hysteresisPanel != null) hysteresisPanel.SetActive(false);
            if (maskShapePanel != null) maskShapePanel.SetActive(false);
            if (refinementPanel != null) refinementPanel.SetActive(false);
        }
    }
}
```

### **1.4 Wire Components**

1. Select `MechanismSelectorPanel`
2. Add Component ? `MechanismSelectorUI`
3. Drag `MechanismDropdown` to script field
4. Leave panel fields empty (will assign after creating panels)

---

## ?? **Step 2: Create PointSourcesPanel** (15 minutes)

### **2.1 Create Panel**

1. **Right-click UICanvas ? UI ? Panel**
   - Name: `PointSourcesPanel`
   - **Rect Transform:**
     - Anchor: Bottom-Left
     - Position: (10, 10, 0)
     - Width: 400, Height: 200
   - **Initially:** SetActive = false

2. **Panel Title:**
   - Add Text: "Point Sources Configuration"
   - Font: Bold, 14pt

### **2.2 Add Controls**

**Layout (Top to Bottom):**

1. **Add Point Source Button:**
   - Button: "Add Point Source"
   - Position: Top-center

2. **Point Source List (ScrollView):**
   - Right-click Panel ? UI ? Scroll View
   - Name: `PointSourcesScrollView`
   - Content: Will hold dynamically created point source entries

3. **Point Source Entry Prefab (create separately):**
   - Name: `PointSourceEntry`
   - Contains:
     - Label: "Source #1"
     - InputField: "X" (int)
     - InputField: "Y" (int)
     - InputField: "Strength" (double)
     - Button: "Remove" (red)

### **2.3 Create PointSourcesUI Script**

**File:** `Assets/Viable/Core.Unity/UI/PointSourcesUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for configuring point sources (Stage 13.4).
    /// Allows adding/removing/editing point source locations and strengths.
    /// </summary>
    public class PointSourcesUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button addSourceButton;
        [SerializeField] private Transform sourcesContainer;
        [SerializeField] private GameObject sourceEntryPrefab;

        [Header("Current Configuration")]
        private List<PointSourceEntry> sourceEntries = new List<PointSourceEntry>();

        private Controllers.SimulationController simulationController;

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;

            if (addSourceButton != null)
            {
                addSourceButton.onClick.AddListener(OnAddSourceClicked);
            }

            // Load existing sources from current preset (if any)
            LoadCurrentSources();
        }

        private void OnAddSourceClicked()
        {
            if (sourceEntryPrefab == null || sourcesContainer == null)
            {
                Debug.LogError("[PointSourcesUI] Missing prefab or container!");
                return;
            }

            // Create new source entry
            GameObject entryObj = Instantiate(sourceEntryPrefab, sourcesContainer);
            PointSourceEntry entry = entryObj.GetComponent<PointSourceEntry>();

            if (entry != null)
            {
                entry.Initialize(sourceEntries.Count, OnRemoveSource);
                sourceEntries.Add(entry);
                Debug.Log($"[PointSourcesUI] Added source #{sourceEntries.Count}");
            }
        }

        private void OnRemoveSource(int index)
        {
            if (index >= 0 && index < sourceEntries.Count)
            {
                Destroy(sourceEntries[index].gameObject);
                sourceEntries.RemoveAt(index);

                // Renumber remaining entries
                for (int i = 0; i < sourceEntries.Count; i++)
                {
                    sourceEntries[i].SetIndex(i);
                }

                Debug.Log($"[PointSourcesUI] Removed source, now {sourceEntries.Count} sources");
            }
        }

        private void LoadCurrentSources()
        {
            // TODO: Load from SimulationController's current preset
            Debug.Log("[PointSourcesUI] Loading existing sources (not yet implemented)");
        }

        /// <summary>
        /// Apply current point sources to simulation.
        /// </summary>
        public void ApplySources()
        {
            // TODO: Update SimulationController with new sources
            List<PointSourceData> sources = new List<PointSourceData>();

            foreach (var entry in sourceEntries)
            {
                sources.Add(new PointSourceData
                {
                    X = entry.GetX(),
                    Y = entry.GetY(),
                    Strength = entry.GetStrength()
                });
            }

            Debug.Log($"[PointSourcesUI] Applying {sources.Count} point sources");
            // simulationController.UpdatePointSources(sources);
        }
    }

    /// <summary>
    /// Individual point source entry in the list.
    /// </summary>
    public class PointSourceEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI indexLabel;
        [SerializeField] private TMP_InputField xInput;
        [SerializeField] private TMP_InputField yInput;
        [SerializeField] private TMP_InputField strengthInput;
        [SerializeField] private Button removeButton;

        private int index;
        private System.Action<int> onRemove;

        public void Initialize(int idx, System.Action<int> removeCallback)
        {
            index = idx;
            onRemove = removeCallback;

            if (indexLabel != null)
                indexLabel.text = $"Source #{idx + 1}";

            if (removeButton != null)
                removeButton.onClick.AddListener(() => onRemove?.Invoke(index));

            // Set defaults
            if (xInput != null) xInput.text = "32";
            if (yInput != null) yInput.text = "32";
            if (strengthInput != null) strengthInput.text = "100.0";
        }

        public void SetIndex(int idx)
        {
            index = idx;
            if (indexLabel != null)
                indexLabel.text = $"Source #{idx + 1}";
        }

        public int GetX() => int.TryParse(xInput.text, out int x) ? x : 0;
        public int GetY() => int.TryParse(yInput.text, out int y) ? y : 0;
        public double GetStrength() => double.TryParse(strengthInput.text, out double s) ? s : 0.0;
    }

    [System.Serializable]
    public class PointSourceData
    {
        public int X;
        public int Y;
        public double Strength;
    }
}
```

### **2.4 Create Point Source Entry Prefab**

1. **Create empty GameObject in scene:**
   - Name: `PointSourceEntry`

2. **Add components:**
   - **Background:** Image (semi-transparent)
   - **Label:** TextMeshProUGUI "Source #1"
   - **X Input:** TMP_InputField (int)
   - **Y Input:** TMP_InputField (int)
   - **Strength Input:** TMP_InputField (double)
   - **Remove Button:** Button (red) with "X" text

3. **Layout horizontally:**
   ```
   [Source #1] X:[32] Y:[32] Strength:[100.0] [X]
   ```

4. **Add PointSourceEntry script** to prefab

5. **Wire components** to script fields

6. **Save as prefab:** Drag to `Assets/Viable/Core.Unity/UI/Prefabs/`

---

## ?? **Step 3: Create BoundaryModePanel** (10 minutes)

### **3.1 Create Panel**

1. **Right-click UICanvas ? UI ? Panel**
   - Name: `BoundaryModePanel`
   - Same position as PointSourcesPanel
   - **Initially:** SetActive = false

### **3.2 Add Controls**

1. **Title:** "Boundary Mode Configuration"

2. **Dropdown:**
   - Name: `BoundaryModeDropdown`
   - Options:
     - "Closed (Reflective)"
     - "Open (Absorbing)"
     - "Periodic Wrap"

3. **Description Text:**
   - Shows explanation of selected mode
   - Example: "Periodic: Wrap around edges (torus topology)"

### **3.3 Create BoundaryModeUI Script**

**File:** `Assets/Viable/Core.Unity/UI/BoundaryModeUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for configuring boundary mode (Stage 13.5).
    /// </summary>
    public class BoundaryModeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown boundaryDropdown;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private Controllers.SimulationController simulationController;

        private readonly string[] descriptions = new string[]
        {
            "Closed (Reflective): Resources reflect at boundaries",
            "Open (Absorbing): Resources lost at boundaries",
            "Periodic Wrap: Torus topology, wrap around edges"
        };

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;

            if (boundaryDropdown != null)
            {
                boundaryDropdown.onValueChanged.AddListener(OnBoundaryModeChanged);
                // Set initial description
                OnBoundaryModeChanged(0);
            }
        }

        private void OnBoundaryModeChanged(int index)
        {
            if (descriptionText != null && index >= 0 && index < descriptions.Length)
            {
                descriptionText.text = descriptions[index];
            }

            Debug.Log($"[BoundaryModeUI] Selected mode: {index}");
            // TODO: Apply to simulation
            // simulationController.UpdateBoundaryMode((BoundaryMode)index);
        }
    }
}
```

---

## ?? **Step 4: Create DiffusionModePanel** (10 minutes)

### **4.1 Create Panel**

Similar structure to BoundaryModePanel:

1. **Panel:** `DiffusionModePanel`
2. **Dropdown options:**
   - "Von Neumann (4-neighbor)"
   - "Moore (8-neighbor)"
3. **Visual diagram** (optional):
   - Show grid diagram illustrating 4 vs 8 neighbors

### **4.2 Create DiffusionModeUI Script**

**File:** `Assets/Viable/Core.Unity/UI/DiffusionModeUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for configuring diffusion mode (Stage 13.6).
    /// </summary>
    public class DiffusionModeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown diffusionDropdown;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image neighborhoodDiagram; // Optional

        private Controllers.SimulationController simulationController;

        private readonly string[] descriptions = new string[]
        {
            "Von Neumann (4-neighbor): Diffusion to N/S/E/W",
            "Moore (8-neighbor): Diffusion to all 8 surrounding cells"
        };

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;

            if (diffusionDropdown != null)
            {
                diffusionDropdown.onValueChanged.AddListener(OnDiffusionModeChanged);
                OnDiffusionModeChanged(0);
            }
        }

        private void OnDiffusionModeChanged(int index)
        {
            if (descriptionText != null && index >= 0 && index < descriptions.Length)
            {
                descriptionText.text = descriptions[index];
            }

            // Update diagram if present
            if (neighborhoodDiagram != null)
            {
                // TODO: Swap sprite to show 4 vs 8 neighbors
            }

            Debug.Log($"[DiffusionModeUI] Selected mode: {index}");
            // TODO: Apply to simulation
        }
    }
}
```

---

## ?? **Step 5: Create HysteresisPanel** (15 minutes)

### **5.1 Create Panel**

1. **Panel:** `HysteresisPanel`
2. **Controls:**
   - **Enable Toggle:** "Enable Hysteresis"
   - **ON Threshold Slider:** Range [-1, 1], default 0.5
   - **OFF Threshold Slider:** Range [-1, 1], default -0.5
   - **Visual Diagram:** Show threshold gap

### **5.2 Add Visual Feedback**

**Diagram showing:**
```
 V (Viability)
  |
1 |     ???????? ON (activate)
  |     ?
  |     ?  Gap
  |     ?
0 |????????????? OFF (deactivate)
  |
-1|________________ time
```

### **5.3 Create HysteresisUI Script**

**File:** `Assets/Viable/Core.Unity/UI/HysteresisUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for configuring viability hysteresis (Stage 13.7).
    /// </summary>
    public class HysteresisUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Toggle enableToggle;
        [SerializeField] private Slider onThresholdSlider;
        [SerializeField] private Slider offThresholdSlider;
        [SerializeField] private TextMeshProUGUI onThresholdText;
        [SerializeField] private TextMeshProUGUI offThresholdText;
        [SerializeField] private TextMeshProUGUI gapSizeText;
        [SerializeField] private TextMeshProUGUI warningText;

        private Controllers.SimulationController simulationController;

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;

            if (enableToggle != null)
                enableToggle.onValueChanged.AddListener(OnEnableChanged);

            if (onThresholdSlider != null)
                onThresholdSlider.onValueChanged.AddListener(OnOnThresholdChanged);

            if (offThresholdSlider != null)
                offThresholdSlider.onValueChanged.AddListener(OnOffThresholdChanged);

            // Set defaults
            if (onThresholdSlider != null) onThresholdSlider.value = 0.5f;
            if (offThresholdSlider != null) offThresholdSlider.value = -0.5f;

            UpdateDisplay();
        }

        private void OnEnableChanged(bool enabled)
        {
            // Enable/disable sliders
            if (onThresholdSlider != null) onThresholdSlider.interactable = enabled;
            if (offThresholdSlider != null) offThresholdSlider.interactable = enabled;

            Debug.Log($"[HysteresisUI] Hysteresis {(enabled ? "enabled" : "disabled")}");
            // TODO: Apply to simulation
        }

        private void OnOnThresholdChanged(float value)
        {
            UpdateDisplay();
            ValidateThresholds();
        }

        private void OnOffThresholdChanged(float value)
        {
            UpdateDisplay();
            ValidateThresholds();
        }

        private void UpdateDisplay()
        {
            if (onThresholdText != null && onThresholdSlider != null)
                onThresholdText.text = $"ON: {onThresholdSlider.value:F2}";

            if (offThresholdText != null && offThresholdSlider != null)
                offThresholdText.text = $"OFF: {offThresholdSlider.value:F2}";

            if (gapSizeText != null && onThresholdSlider != null && offThresholdSlider != null)
            {
                float gap = onThresholdSlider.value - offThresholdSlider.value;
                gapSizeText.text = $"Gap: {gap:F2}";
            }
        }

        private void ValidateThresholds()
        {
            if (onThresholdSlider == null || offThresholdSlider == null || warningText == null)
                return;

            if (offThresholdSlider.value >= onThresholdSlider.value)
            {
                warningText.text = "? OFF threshold must be < ON threshold!";
                warningText.color = Color.red;
            }
            else
            {
                warningText.text = "";
            }
        }
    }
}
```

---

## ?? **Step 6: Create MaskShapePanel** (15 minutes)

### **6.1 Create Panel**

1. **Panel:** `MaskShapePanel`
2. **Controls:**
   - **Shape Dropdown:**
     - Rectangle (no mask)
     - Circle
     - Ring (donut)
     - Corridor
     - Percolation Holes
   - **Shape-specific parameters** (show/hide based on selection):
     - Circle: Radius slider
     - Ring: Outer radius + Inner radius
     - Corridor: Width slider
     - Percolation: Hole probability slider

### **6.2 Add Visual Preview**

**Preview image showing mask shape** (optional but recommended):
- Small 64×64 grid preview
- Black = masked out
- White = active domain

### **6.3 Create MaskShapeUI Script**

**File:** `Assets/Viable/Core.Unity/UI/MaskShapeUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for configuring domain masks (Stage 13.8).
    /// </summary>
    public class MaskShapeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown shapeDropdown;
        [SerializeField] private GameObject circleParameters;
        [SerializeField] private GameObject ringParameters;
        [SerializeField] private GameObject corridorParameters;
        [SerializeField] private GameObject percolationParameters;
        [SerializeField] private Slider radiusSlider;
        [SerializeField] private Slider innerRadiusSlider;
        [SerializeField] private Slider outerRadiusSlider;
        [SerializeField] private Slider corridorWidthSlider;
        [SerializeField] private Slider holeProbabilitySlider;
        [SerializeField] private Image previewImage; // Optional

        private Controllers.SimulationController simulationController;

        public void Initialize(Controllers.SimulationController controller)
        {
            simulationController = controller;

            if (shapeDropdown != null)
            {
                shapeDropdown.onValueChanged.AddListener(OnShapeChanged);
                OnShapeChanged(0); // Initialize
            }
        }

        private void OnShapeChanged(int index)
        {
            // Hide all parameter panels
            if (circleParameters != null) circleParameters.SetActive(false);
            if (ringParameters != null) ringParameters.SetActive(false);
            if (corridorParameters != null) corridorParameters.SetActive(false);
            if (percolationParameters != null) percolationParameters.SetActive(false);

            // Show relevant parameters
            switch (index)
            {
                case 0: // Rectangle (no mask)
                    break;
                case 1: // Circle
                    if (circleParameters != null) circleParameters.SetActive(true);
                    break;
                case 2: // Ring
                    if (ringParameters != null) ringParameters.SetActive(true);
                    break;
                case 3: // Corridor
                    if (corridorParameters != null) corridorParameters.SetActive(true);
                    break;
                case 4: // Percolation Holes
                    if (percolationParameters != null) percolationParameters.SetActive(true);
                    break;
            }

            UpdatePreview();
            Debug.Log($"[MaskShapeUI] Selected shape: {index}");
        }

        private void UpdatePreview()
        {
            // TODO: Generate preview texture showing mask
            if (previewImage != null)
            {
                // Create 64×64 texture showing mask shape
            }
        }

        public void ApplyMask()
        {
            // TODO: Apply to simulation
            Debug.Log("[MaskShapeUI] Applying mask configuration");
        }
    }
}
```

---

## ?? **Step 7: Create RefinementPanel** (10 minutes)

### **7.1 Create Panel**

1. **Panel:** `RefinementPanel`
2. **Info Banner:** "? Refinement is not yet functional (stubs only)"
3. **Controls (disabled):**
   - Enable toggle
   - Disorder threshold slider
   - Max depth input

### **7.2 Create RefinementUI Script**

**File:** `Assets/Viable/Core.Unity/UI/RefinementUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// UI panel for refinement configuration (Stage 13.9 - stubs only).
    /// </summary>
    public class RefinementUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Toggle enableToggle;
        [SerializeField] private Slider thresholdSlider;
        [SerializeField] private TMP_InputField maxDepthInput;
        [SerializeField] private TextMeshProUGUI warningText;

        public void Initialize(Controllers.SimulationController controller)
        {
            // Disable all controls (not functional yet)
            if (enableToggle != null) enableToggle.interactable = false;
            if (thresholdSlider != null) thresholdSlider.interactable = false;
            if (maxDepthInput != null) maxDepthInput.interactable = false;

            // Show warning
            if (warningText != null)
            {
                warningText.text = "? Refinement mechanism is not yet implemented.\nConfiguration stored for future use.";
                warningText.color = Color.yellow;
            }
        }
    }
}
```

---

## ?? **Step 8: Wire Everything to UIManager** (10 minutes)

### **8.1 Update UIManager.cs**

Add references to new panels:

```csharp
[Header("Stage 13 Mechanism Panels")]
[SerializeField] private MechanismSelectorUI mechanismSelector;
[SerializeField] private PointSourcesUI pointSourcesUI;
[SerializeField] private BoundaryModeUI boundaryModeUI;
[SerializeField] private DiffusionModeUI diffusionModeUI;
[SerializeField] private HysteresisUI hysteresisUI;
[SerializeField] private MaskShapeUI maskShapeUI;
[SerializeField] private RefinementUI refinementUI;
```

In `InitializeUI()`:

```csharp
// Initialize mechanism panels
if (mechanismSelector != null)
    mechanismSelector.Initialize();

if (pointSourcesUI != null)
    pointSourcesUI.Initialize(simulationController);

if (boundaryModeUI != null)
    boundaryModeUI.Initialize(simulationController);

// ... etc for other panels
```

### **8.2 Wire Mechanism Selector to Panels**

In Unity Inspector:
1. Select `MechanismSelectorPanel`
2. Drag each mechanism panel to corresponding field:
   - PointSourcesPanel ? pointSourcesPanel field
   - BoundaryModePanel ? boundaryModePanel field
   - etc.

---

## ?? **Step 9: Testing Checklist**

After setup:

- [ ] Mechanism dropdown shows all 7 options
- [ ] Selecting "Point Sources" shows PointSourcesPanel
- [ ] Selecting "Boundary Mode" shows BoundaryModePanel
- [ ] Can add/remove point sources
- [ ] Sliders update text labels correctly
- [ ] Hysteresis validates ON > OFF threshold
- [ ] Mask shape shows appropriate parameters
- [ ] Refinement panel shows warning message
- [ ] Only one mechanism panel visible at a time
- [ ] No console errors

---

## ?? **Visual Polish** (Optional)

### **Colors:**
- Mechanism panels: Dark blue tint (differentiate from base UI)
- Active mechanism: Blue highlight
- Warnings: Yellow/red text
- Disabled controls: Gray

### **Icons:**
- Add icons next to dropdown options
- Point Sources: ??
- Boundaries: ??
- Diffusion: ??
- Hysteresis: ?
- Masks: ??
- Refinement: ??

---

## ?? **Time Estimates**

| Panel | Time |
|-------|------|
| MechanismSelectorPanel | 10 min |
| PointSourcesPanel | 15 min |
| BoundaryModePanel | 10 min |
| DiffusionModePanel | 10 min |
| HysteresisPanel | 15 min |
| MaskShapePanel | 15 min |
| RefinementPanel | 10 min |
| Wiring + Testing | 15 min |
| **Total** | **~2 hours** |

---

## ?? **Priority Order**

**Essential (do first):**
1. MechanismSelectorPanel (enables everything else)
2. HysteresisPanel (most impactful mechanism)
3. BoundaryModePanel (simple, good test case)

**Important:**
4. DiffusionModePanel
5. MaskShapePanel

**Nice-to-have:**
6. PointSourcesPanel (more complex, dynamic list)
7. RefinementPanel (not functional yet)

---

**Ready to build Stage 13 UI!** Start with MechanismSelectorPanel, then add panels one at a time. ??
