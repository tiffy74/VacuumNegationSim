# Context-Sensitive Detail Sections Setup Guide

**?? For complete setup instructions, see `MASTER_UI_SETUP_GUIDE.md` (Part 4)**

This document provides additional context about the detail sections.

---

## Overview

These sections appear/disappear based on mechanism selections in MechanismsSection.

**Visibility Logic:**
- TopologyDetailsSection ? Shows when Topology = "Masked Domain"
- InflowDetailsSection ? Shows when Inflow = "Point Sources"  
- DiffusionDetailsSection ? Shows when Diffusion = "Anisotropic"
- ViabilityDetailsSection ? Shows when ViabilityRule = "Hysteresis"

All sections implement `RefreshVisibility()` which is called:
1. When a preset is loaded
2. When mechanisms change (via MechanismsSection.OnMechanismChanged event)

---

## Section Details

### 1. TopologyDetailsSection

**Purpose:** Configure mask shape and parameters for masked domains

**Field Visibility Rules:**
- **Rectangle:** No parameters (all hidden)
- **Circle:** Outer Radius only
- **Ring:** Outer Radius + Inner Radius
- **Corridor:** Corridor Width only
- **Percolation Holes:** Hole Probability only

**Script:** `TopologyDetailsSection.cs`

---

### 2. InflowDetailsSection

**Purpose:** Display and edit point source locations/strengths

**Features:**
- Shows count of point sources
- Lists first few sources (x, y, strength)
- "Edit Sources..." button opens modal (Step 12 implementation)

**Script:** `InflowDetailsSection.cs`

**Note:** Currently adds test sources on button click (placeholder)

---

### 3. DiffusionDetailsSection

**Purpose:** Configure anisotropic diffusion direction and bias

**Features:**
- Direction dropdown: North, East, South, West
- Bias slider: 0.0 to 1.0 (how strongly diffusion favors the direction)
- Real-time bias value display

**Script:** `DiffusionDetailsSection.cs`

---

### 4. ViabilityDetailsSection

**Purpose:** Configure hysteresis on/off thresholds

**Features:**
- ON threshold input
- OFF threshold input
- Explanation text about hysteresis behavior

**Important:** ON threshold should be > OFF threshold to prevent flickering.

**Script:** `ViabilityDetailsSection.cs`

---

## Complete Setup Instructions

**See `MASTER_UI_SETUP_GUIDE.md` Part 4 for:**
- Step-by-step GameObject creation
- UI element layouts
- Inspector wiring details
- CollapsibleSection setup

**Quick Reference:** See `QUICK_REFERENCE_HIERARCHY.md` for complete hierarchy tree.

---

## Testing Checklist

### TopologyDetailsSection:
- [ ] Hidden when Topology = "Full Domain"
- [ ] Visible when Topology = "Masked Domain"
- [ ] Mask shape dropdown works
- [ ] Correct fields show for each shape
- [ ] Input fields update config

### InflowDetailsSection:
- [ ] Hidden when Inflow = "Uniform Field"
- [ ] Visible when Inflow = "Point Sources"
- [ ] Point source list displays correctly
- [ ] Edit button works (adds test source for now)

### DiffusionDetailsSection:
- [ ] Hidden when Diffusion = "Von Neumann" or "Moore"
- [ ] Visible when Diffusion = "Anisotropic"
- [ ] Direction dropdown works
- [ ] Bias slider updates value text
- [ ] Config updates on changes

### ViabilityDetailsSection:
- [ ] Hidden when ViabilityRule = "Simple"
- [ ] Visible when ViabilityRule = "Hysteresis"
- [ ] ON/OFF threshold inputs work
- [ ] Explanation text displays

---

## Implementation Notes

- All sections inherit from `CollapsibleSection`
- All sections implement `IConfigSection` interface
- Field visibility is dynamic based on sub-mode selections
- Changes apply immediately to `WorkingScenarioConfig`
- Final application to Engine happens on "Apply & Restart"

---

**For complete setup, see: `MASTER_UI_SETUP_GUIDE.md`**
