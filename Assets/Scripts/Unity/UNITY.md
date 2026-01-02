# Unity Integration Module

This module bridges Unity's rendering system with the pure simulation logic, handling all Unity-specific functionality.

## Overview

The Unity module serves as the adapter layer between Unity's component-based architecture and the pure C# simulation engine. It handles visualization, color management, and render mode switching without polluting the simulation logic with Unity dependencies.

## Files

### GridRenderer.cs

**Purpose**: Orchestrates the rendering of the entire simulation grid, translating simulation state into visual representation via CellVisualiser components.

**Key Responsibilities**:
- Per-frame color updates for all grid cells
- Render mode switching (Viability/Energy/Entropy)
- Special visual handling for boundaries, frontiers, black holes
- Frame-based normalization for consistent visualization

#### Enums

##### `RenderMode`
Defines available visualization modes:
- `Viability`: Shows persistence criterion (default)
- `Energy`: Shows local energy density
- `Entropy`: Shows entropy/complexity

#### Constructor
public GridRenderer(int w, int h, CellVisualiser[,] views, Color voidColor, Color fieldDimColor, bool showEntropyTint = false)

- **Parameters**:
  - `w`, `h`: Grid dimensions
  - `views`: 2D array of CellVisualiser components for each cell
  - `voidColor`: Color for void cells
  - `fieldDimColor`: Color for low-energy field cells
  - `showEntropyTint`: Whether to apply entropy-based tinting

**Purpose**: Initializes renderer with grid dimensions, cell references, and color scheme

#### Core Methods

##### `void Render(GridState s, SimContext ctx, RenderMode mode)`

**Parameters**:
- `s`: Current simulation state (GridState)
- `ctx`: Simulation context (SimContext)
- `mode`: Visualization mode (RenderMode enum)

**Purpose**: Main rendering method - updates all cell visuals based on current simulation state

**Rendering Logic (Priority Order)**:

1. **Black Holes** (Highest Priority)
   - Color: Vivid magenta `(0.85, 0, 0.85)`
   - Represents collapsed configuration space
   - Always rendered regardless of mode

2. **Void Cells** (No Field Present)
   - Color: `_voidColor` (dark purple/black)
   - Represents regions without active configuration space

3. **Arrival Ring** (New Field Expansion)
   - Color: Yellow
   - Condition: `FieldFirstTick[i] == currentTick OR currentTick-1`
   - Visualizes the "wave front" of field propagation
   - 2-tick window creates visible ring

4. **Frontier Cells** (Field Boundary)
   - Color: Yellow
   - Condition: Cell has field AND at least one neighbor without field
   - Shows the edge between active field and void
   - 4-way neighbor check (N, S, E, W)

5. **Vacuum Cells**
   - Color: `_fieldDimColor` (dim purple)
   - Special state for permanent vacuum regions

6. **Mode-Specific Rendering**:

   **Viability Mode** (Default):
   - Active & Viable cells: Color gradient based on viability
     - Normalized per-frame: `vNorm = V[i] / max(V)`
     - Optional entropy tint if `ShowEntropyTint == true`
     - Uses `CellVisualiser.SetViability()` or `SetViabilityWithEntropy()`
   - Inactive/Non-viable: `_fieldDimColor`

   **Energy Mode**:
   - Color: Lerp from `fieldDimColor` to red
   - Normalized: `eNorm = Nlocal[i] / max(Nlocal)`
   - Shows energy density distribution

   **Entropy Mode**:
   - Color: Lerp from blue to magenta
   - Normalized: `entropyNorm = Entropy[i] / max(Entropy)`
   - Shows complexity/disorder distribution

**Frame-Based Normalization**:
// Compute maxima each frame for dynamic range adjustment float vMax = max(all V[i]); float eMax = max(all Nlocal[i]); float entropyMax = max(all Entropy[i]);
- Prevents color saturation as simulation evolves
- Ensures visual contrast at all simulation stages
- Each frame recalculates for adaptive visualization

#### Properties

##### `float ViabilityColorScale = 40f`
- **Purpose**: Multiplier for viability display (currently unused in normalized rendering)
- **Note**: Legacy property from pre-normalization rendering

##### `bool ShowEntropyTint`
- **Purpose**: Toggle entropy overlay in Viability mode
- **Default**: `false`
- **Effect**: When true, adds subtle blue tint to high-entropy regions

#### Private Fields

##### `readonly Color _blackHoleColor`
- **Value**: `(0.85, 0, 0.85)` - vivid magenta
- **Purpose**: Distinctive color for collapsed configuration space (black holes)
- **Design Choice**: High saturation magenta is visually distinct from all other states

## Design Patterns

### Adapter Pattern
- Bridges pure simulation (GridState) with Unity visualization (CellVisualiser)
- Isolates Unity-specific code from simulation logic

### Strategy Pattern
- `RenderMode` enum enables runtime switching between visualization strategies
- Same simulation state, different visual interpretations

### Immutable Configuration
- Constructor parameters stored as `readonly` fields
- Prevents accidental modification during rendering

## Integration Points

### Dependencies
- **Domain**: `GridState`, `SimContext` (pure data structures)
- **Visuals**: `CellVisualiser` (Unity MonoBehaviour)
- **Unity Engine**: `Color`, `Mathf`

### Used By
- **SimulationController**: Calls `Render()` each tick after simulation update

### Communication Flow
SimulationController ? (calls Render()) GridRenderer ? (calls SetColor/SetViability on each cell) CellVisualiser ? (updates sprite) Unity Rendering System


## Performance Considerations

### Per-Frame Operations
- **Grid Iteration**: O(width × height) every render call
- **Max-Finding**: 3 full passes (viability, energy, entropy maxima)
  - Could be optimized by tracking in simulation or using parallel max
- **Color Updates**: One `SetColor()` call per cell per frame

### Optimization Opportunities
1. **Incremental Max Tracking**: Track maxima during simulation, not rendering
2. **Dirty Rectangles**: Only update changed regions
3. **LOD (Level of Detail)**: Reduce update frequency for distant cells
4. **GPU Instancing**: Batch similar color updates
5. **Compute Shader**: Offload color computation to GPU

### Current Performance
- Acceptable for 100×100 grid (10,000 cells) at 10 FPS
- May struggle at 500×500 (250,000 cells) without optimization

## Visual Design Philosophy

### Color Coding Strategy
- **Magenta**: Singularities (black holes) - impossible states
- **Yellow**: Boundaries/frontiers - active expansion
- **Dark Purple**: Void - absence of field
- **Dim Purple**: Inactive field - potential but dormant
- **Heat Map**: Energy mode - red = high energy
- **Cool-Warm Gradient**: Viability mode - blue (dead) to amber (viable)

### Rendering Priority
The priority order ensures critical features are never obscured:
1. Black holes (always visible)
2. Void regions (no field)
3. Expansion fronts (yellow ring)
4. Mode-specific data (viability/energy/entropy)

### Adaptive Contrast
Per-frame normalization ensures:
- Early simulation (low values) still has visible gradients
- Late simulation (high values) doesn't saturate to single color
- Dynamic range adjustment maintains visual information density

## Future Enhancement Opportunities

1. **HDR Rendering**: Use HDR colors for glow effects around high-energy regions
2. **Shader-Based Rendering**: Move color computation to GPU fragment shader
3. **Particle Effects**: Add particle emitters at frontiers for "spreading" visual
4. **Camera Effects**: Post-processing effects (bloom, glow) for dramatic presentation
5. **Heatmap Improvements**: Smooth heatmap interpolation between cells
6. **Multi-Layer Rendering**: Overlay multiple modes simultaneously (e.g., viability + entropy)
7. **Custom Gradients**: Expose color ramps as inspector-editable gradients

## Code Quality Notes

- **Defensive Null Checks**: `if (vis == null) continue;` prevents crashes
- **Clear State Machine**: Priority-based if-else chain is easy to follow
- **Separation of Concerns**: Pure rendering logic, no simulation state modification
- **Magic Numbers**: Some thresholds (e.g., 40f scale) could be named constants
- **Performance Trade-off**: Clarity over micro-optimization (acceptable for current scale)
