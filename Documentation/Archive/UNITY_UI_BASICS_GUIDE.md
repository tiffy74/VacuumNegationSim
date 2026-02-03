# Unity UI Basics for RightDock - Beginner's Guide

## ?? Understanding Unity UI Components

Before building the sections, let's understand the building blocks:

---

## ?? Basic Unity UI Elements

### 1. **GameObject (Empty)**
- Think of this as an **invisible container**
- Used to organize other UI elements
- Like a `<div>` in HTML or a Panel in Windows Forms

**How to create:**
- Right-click in Hierarchy ? Create Empty

---

### 2. **Panel**
- A **visible rectangle** (has background)
- Used for headers, backgrounds, dividers
- Has an `Image` component (you can change its color)

**How to create:**
- Right-click in Hierarchy ? UI ? Panel

---

### 3. **Text - TextMeshPro (TMP)**
- Displays text (like a Label in Windows Forms)
- Can set Font Size, Color, Alignment, Wrapping

**How to create:**
- Right-click in Hierarchy ? UI ? Text - TextMeshPro

---

### 4. **Button - TextMeshPro**
- Clickable button
- Has a child Text (TMP) for the button label
- Can wire up `onClick` events

**How to create:**
- Right-click in Hierarchy ? UI ? Button - TextMeshPro

---

### 5. **Input Field - TextMeshPro**
- Text box for user input
- Can set Content Type (Integer, Decimal, Standard, etc.)
- Has Placeholder text

**How to create:**
- Right-click in Hierarchy ? UI ? Input Field - TextMeshPro

---

### 6. **Dropdown - TextMeshPro**
- Dropdown list (ComboBox in Windows Forms)
- Options are added in Inspector or via code

**How to create:**
- Right-click in Hierarchy ? UI ? Dropdown - TextMeshPro

---

### 7. **Slider**
- Horizontal or Vertical slider (like TrackBar in Windows Forms)
- Set Min/Max values, current Value

**How to create:**
- Right-click in Hierarchy ? UI ? Slider

---

## ?? Layout Components (Automatic Positioning)

Instead of manually positioning each element (X, Y coordinates), Unity uses **Layout Groups** to automatically arrange children.

### 1. **Horizontal Layout Group**
- Arranges children **left to right** (like a horizontal StackPanel)
- Set `Spacing` to add gaps between children
- Set `Padding` for margins inside the container

**Example:**
```
Row (Horizontal Layout Group)
??? Label (Text)
??? Input (InputField)
```
Result: Label on left, Input on right, automatically sized.

---

### 2. **Vertical Layout Group**
- Arranges children **top to bottom** (like a vertical StackPanel)
- Each child appears below the previous one

**Example:**
```
ContentPanel (Vertical Layout Group)
??? Row1
??? Row2
??? Row3
```
Result: Rows stack vertically.

---

### 3. **Layout Element**
- Controls **how much space** a child takes in a Layout Group
- Key properties:
  - **Preferred Width:** Fixed width (e.g., 100 pixels)
  - **Flexible Width:** Grows to fill remaining space (e.g., 1 = takes all remaining)
  - **Min/Max Width:** Constraints

**Example:**
```
Row (Horizontal Layout, Total Width: 300px)
??? Label (Preferred Width: 100px) ? Takes 100px
??? Input (Flexible Width: 1) ? Takes remaining 200px
```

---

## ??? How RightDock is Structured

Let's visualize your existing RightDock:

```
RightDock (Fixed width: 320px, no scroll)
?
??? TabButtonRow (Height: 40px, Horizontal Layout)
?   ??? SetupTabButton (Button, Flexible Width: 1)
?   ??? InspectTabButton (Button, Flexible Width: 1)
?   ??? ExportTabButton (Button, Flexible Width: 1)
?   ? All 3 buttons share space equally
?
??? ContentArea (Vertical Layout, fills remaining height)
    ?
    ??? SetupPanel (Active by default)
    ?   ??? MechanismSummary (Panel, Height: 50px, read-only text)
    ?   ??? SectionsContainer (Vertical Layout)
    ?       ??? MechanismsSection ?
    ?       ??? TopologyDetailsSection ?
    ?       ??? InflowDetailsSection ?
    ?       ??? DiffusionDetailsSection ? (You need to build this)
    ?       ??? ViabilityDetailsSection ? (You need to build this)
    ?       ??? CoreParametersSection ?
    ?
    ??? InspectPanel (Hidden by default)
    ?   ??? LiveMetricsText (Placeholder)
    ?
    ??? ExportPanel (Hidden by default)
        ??? ExportInfoText (Placeholder)
```

---

## ?? Step-by-Step: Building DiffusionDetailsSection

Let's build this **one step at a time** with screenshots of what to click:

### Step 1: Create the Section Container

1. **In Hierarchy panel**, find: `RightDock` ? `ContentArea` ? `SetupPanel`
2. **Right-click `SetupPanel`**
3. **Select:** `Create Empty`
4. **Name it:** `DiffusionDetailsSection`

**What you created:** An invisible container (like a `<div>`)

---

### Step 2: Add the Script

1. **Select `DiffusionDetailsSection`** (click on it in Hierarchy)
2. **In Inspector panel** (right side), click **"Add Component"**
3. **Type:** `DiffusionDetailsSection`
4. **Press Enter**

**What this does:** Attaches the C# script to the GameObject

---

### Step 3: Add Vertical Layout Group

1. **With `DiffusionDetailsSection` still selected**
2. **In Inspector**, click **"Add Component"**
3. **Type:** `Vertical Layout Group`
4. **Press Enter**
5. **In the Vertical Layout Group component:**
   - Set `Spacing`: 5
   - Check ? `Child Force Expand` ? Width only

**What this does:** Makes children stack vertically with 5px gaps

---

### Step 4: Create Header Panel

1. **Right-click `DiffusionDetailsSection`** (in Hierarchy)
2. **Select:** `UI` ? `Panel`
3. **Name it:** `HeaderPanel`
4. **With `HeaderPanel` selected:**
   - In Inspector, find **Layout Element** component
   - If not there, click "Add Component" ? Layout Element
   - Set `Min Height`: 30
   - Set `Preferred Height`: 30

**What you created:** A visible rectangle (the collapsible header background)

---

### Step 5: Add Button to Header (for collapse/expand)

1. **Select `HeaderPanel`**
2. **In Inspector**, click **"Add Component"**
3. **Type:** `Button`
4. **Press Enter**

**What this does:** Makes the header clickable to collapse/expand

---

### Step 6: Add Header Text

1. **Right-click `HeaderPanel`** (in Hierarchy)
2. **Select:** `UI` ? `Text - TextMeshPro`
3. **Name it:** `HeaderText`
4. **With `HeaderText` selected:**
   - In Inspector, find **TextMeshPro - Text (UI)** component
   - Set `Text`: "Diffusion Details"
   - Set `Font Size`: 16
   - Set `Font Style`: Bold (click **B** button)
   - Set `Color`: White

**What you created:** The header label

---

### Step 7: Create Content Panel (holds the controls)

1. **Right-click `DiffusionDetailsSection`** (in Hierarchy)
2. **Select:** `Create Empty`
3. **Name it:** `ContentPanel`
4. **With `ContentPanel` selected:**
   - In Inspector, click "Add Component"
   - Type: `Vertical Layout Group`
   - Set `Padding`: Left=10, Right=10, Top=10, Bottom=10
   - Set `Spacing`: 8

**What you created:** Container for all the input controls

---

### Step 8: Create Direction Row (Label + Dropdown)

1. **Right-click `ContentPanel`**
2. **Select:** `Create Empty`
3. **Name it:** `DirectionRow`
4. **With `DirectionRow` selected:**
   - Add Component ? `Horizontal Layout Group`
   - Set `Spacing`: 10
   - Check ? `Child Control Size` ? Width and Height

**What you created:** A horizontal container (label will be on left, dropdown on right)

---

### Step 9: Add Direction Label

1. **Right-click `DirectionRow`**
2. **Select:** `UI` ? `Text - TextMeshPro`
3. **Name it:** `DirectionLabel`
4. **With `DirectionLabel` selected:**
   - Set `Text`: "Direction:"
   - Add Component ? `Layout Element`
   - Set `Preferred Width`: 100

**What you created:** The label "Direction:" that takes 100px width

---

### Step 10: Add Direction Dropdown

1. **Right-click `DirectionRow`**
2. **Select:** `UI` ? `Dropdown - TextMeshPro`
3. **Name it:** `DirectionDropdown`
4. **With `DirectionDropdown` selected:**
   - Add Component ? `Layout Element`
   - Set `Flexible Width`: 1 (this makes it fill remaining space)
   - In Inspector, find **TMP_Dropdown** component
   - Expand **Options** list
   - Clear existing options (set Size: 0)
   - Set Size: 4
   - Element 0: "North (?)"
   - Element 1: "East (?)"
   - Element 2: "South (?)"
   - Element 3: "West (?)"

**What you created:** A dropdown that takes all remaining width in the row

---

### Step 11: Create Bias Row (Label + Slider + Value Text)

1. **Right-click `ContentPanel`**
2. **Select:** `Create Empty`
3. **Name it:** `BiasRow`
4. **Add:** Horizontal Layout Group (Spacing: 10)

---

### Step 12: Add Bias Label

1. **Right-click `BiasRow`**
2. **Select:** `UI` ? `Text - TextMeshPro`
3. **Name it:** `BiasLabel`
4. **Set:**
   - Text: "Bias:"
   - Layout Element ? Preferred Width: 100

---

### Step 13: Add Bias Slider

1. **Right-click `BiasRow`**
2. **Select:** `UI` ? `Slider`
3. **Name it:** `BiasSlider`
4. **With `BiasSlider` selected:**
   - In Inspector, find **Slider** component
   - Set `Min Value`: 0
   - Set `Max Value`: 1
   - Set `Value`: 0.5
   - **Uncheck** `Whole Numbers`
   - Add Component ? Layout Element
   - Set `Flexible Width`: 1

**What you created:** A slider that goes from 0.0 to 1.0

---

### Step 14: Add Bias Value Text

1. **Right-click `BiasRow`**
2. **Select:** `UI` ? `Text - TextMeshPro`
3. **Name it:** `BiasValueText`
4. **Set:**
   - Text: "Bias: 0.50"
   - Layout Element ? Preferred Width: 80

**What you created:** Text that shows the current slider value

---

### Step 15: Wire Everything in Inspector

**This is the most important step!** This connects the UI elements to the C# script.

1. **Select `DiffusionDetailsSection`** (the root GameObject)
2. **In Inspector**, find the **DiffusionDetailsSection (Script)** component
3. **You'll see empty fields (slots) like:**
   - Direction Dropdown
   - Bias Slider
   - Bias Value Text
   - Header Object
   - Content Object
   - Toggle Button
   - Header Text

4. **Drag and drop:**
   - Find `DirectionDropdown` in Hierarchy ? Drag to `directionDropdown` field
   - Find `BiasSlider` ? Drag to `biasSlider` field
   - Find `BiasValueText` ? Drag to `biasValueText` field
   - Find `HeaderPanel` ? Drag to `headerObject` field
   - Find `ContentPanel` ? Drag to `contentObject` field
   - Find `HeaderPanel` ? Drag to `toggleButton` field (yes, same object)
   - Find `HeaderText` ? Drag to `headerText` field

**What this does:** Connects the UI elements to the script variables so the code can access them.

---

## ?? Testing

1. **Press Play** (top center of Unity)
2. **Find the Mechanisms section**
3. **Change Diffusion dropdown to "Anisotropic"**
4. **Result:** DiffusionDetailsSection should appear!
5. **Change back to "Moore"** ? Section disappears

---

## ?? Key Concepts Summary

### Layout Groups = Automatic Positioning
- **Horizontal Layout Group:** Children go left-to-right
- **Vertical Layout Group:** Children go top-to-bottom
- **Spacing:** Gap between children
- **Padding:** Margin inside container

### Layout Element = Size Control
- **Preferred Width:** Fixed size (e.g., 100px)
- **Flexible Width:** Fill remaining space (e.g., 1)
- **Min/Max:** Constraints

### Wiring in Inspector = Connecting UI to Code
- Drag GameObject from Hierarchy ? Field in Inspector
- This lets the C# script access the UI element

---

## ?? Quick Reference: What You Need to Build

For **DiffusionDetailsSection:**
```
DiffusionDetailsSection (Empty, Vertical Layout)
??? HeaderPanel (Panel + Button, Height: 30)
?   ??? HeaderText (Text TMP, "Diffusion Details")
??? ContentPanel (Empty, Vertical Layout)
    ??? DirectionRow (Empty, Horizontal Layout)
    ?   ??? DirectionLabel (Text, "Direction:", Width: 100)
    ?   ??? DirectionDropdown (Dropdown, Flexible)
    ??? BiasRow (Empty, Horizontal Layout)
        ??? BiasLabel (Text, "Bias:", Width: 100)
        ??? BiasSlider (Slider, 0-1, Flexible)
        ??? BiasValueText (Text, "Bias: 0.50", Width: 80)
```

For **ViabilityDetailsSection:**
```
ViabilityDetailsSection (Empty, Vertical Layout)
??? HeaderPanel (Panel + Button, Height: 30)
?   ??? HeaderText (Text TMP, "Viability Details")
??? ContentPanel (Empty, Vertical Layout)
    ??? OnThresholdRow (Empty, Horizontal Layout)
    ?   ??? OnThresholdLabel (Text, "ON Threshold:", Width: 120)
    ?   ??? OnThresholdInput (InputField, Decimal, Flexible)
    ??? OffThresholdRow (Empty, Horizontal Layout)
    ?   ??? OffThresholdLabel (Text, "OFF Threshold:", Width: 120)
    ?   ??? OffThresholdInput (InputField, Decimal, Flexible)
    ??? ExplanationText (Text TMP, wrapping enabled)
```

---

## ?? Tips

1. **Save often:** Ctrl+S after each step
2. **Undo:** Ctrl+Z if you make a mistake
3. **Duplicate:** Ctrl+D to copy a GameObject (saves time!)
4. **Use Inspector Search:** Type component name in "Add Component" box
5. **Check Hierarchy:** Make sure things are nested correctly (indented under parent)

---

## ?? Common Mistakes

### Mistake 1: Wrong Parent
- **Problem:** Created GameObject in wrong place
- **Fix:** Drag it to correct parent in Hierarchy

### Mistake 2: Forgot Layout Element
- **Problem:** Element doesn't size correctly
- **Fix:** Select it ? Add Component ? Layout Element

### Mistake 3: Forgot to Wire Inspector
- **Problem:** Script can't find UI elements (NullReferenceException)
- **Fix:** Drag GameObjects to script fields in Inspector

### Mistake 4: Wrong Component Type
- **Problem:** Used regular Text instead of Text - TextMeshPro
- **Fix:** Delete it, create correct type

---

**Does this help clarify how Unity UI works?** Follow the steps above to build DiffusionDetailsSection, then I can help with ViabilityDetailsSection!
