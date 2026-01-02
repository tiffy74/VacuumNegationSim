# Visuals Module

This module handles all visual rendering and cell color management for the simulation grid.

## Overview

The Visuals module is responsible for translating simulation state (viability, entropy, energy) into visual representations using Unity's sprite rendering system. It provides color gradient mapping and visual effects to make the abstract simulation data observable.

## Files

### CellVisualiser.cs

**Purpose**: Manages the visual appearance of individual grid cells by controlling their sprite renderer color based on simulation state.

**Key Responsibilities**:
- Maps viability values to color gradients
- Applies entropy-based color tinting
- Provides radial shading effects
- Manages sprite renderer component lifecycle

#### Core Methods

##### `void Awake()`
- **Purpose**: Unity lifecycle method that initializes the sprite renderer reference
- **Logic**: Retrieves and caches the SpriteRenderer component, logs error if not found

##### `void Initialize(Color color)`
- **Parameters**: 
  - `color`: Initial color to set for the cell
- **Purpose**: Sets up the cell visualizer with an initial color
- **Logic**: Ensures renderer exists, applies color, logs initialization

##### `Color GetViabilityColor(float viability)`
- **Parameters**: 
  - `viability`: Normalized viability value (0-1)
- **Returns**: Color representing the viability level
- **Purpose**: Converts viability value to color using multi-stage gradient
- **Color Mapping**:
  - `0.0 - 0.1`: Black ? Blue (dead/inactive)
  - `0.1 - 0.25`: Blue ? Cyan (low viability)
  - `0.25 - 0.5`: Cyan ? Green (moderate viability)
  - `0.5 - 0.75`: Green ? Yellow (good viability)
  - `0.75 - 0.9`: Yellow ? Amber (high viability)
  - `0.9 - 1.0`: Amber ? Light Peach (maximum viability)

##### `void SetViabilityColor(float viability)`
- **Parameters**: 
  - `viability`: Raw viability value (normalized internally)
- **Purpose**: Applies viability-based color directly to sprite renderer
- **Logic**: 
  1. Normalizes viability using `NormalizeViability()`
  2. Maps to color using gradient stages
  3. Applies to sprite renderer

##### `float NormalizeViability(float v)`
- **Parameters**: 
  - `v`: Raw viability value
- **Returns**: Normalized viability in [0, 1] range
- **Purpose**: Scales viability to visual range to prevent color saturation
- **Logic**: Clamps `v / VmaxVisual` where `VmaxVisual = 2.0`

##### `void SetViabilityWithEntropy(float viability, float entropy)`
- **Parameters**: 
  - `viability`: Viability value to visualize
  - `entropy`: Entropy value for tinting
- **Purpose**: Renders viability with entropy-based color overlay
- **Logic**:
  1. Normalizes both viability and entropy to [0, 1]
  2. Gets base color from viability
  3. Applies subtle entropy overlay (40% blend, power-scaled)
  4. Special case: very low viability fades toward blue
  5. Applies final color to renderer

##### `void SetViability(float v)`
- **Parameters**: 
  - `v`: Viability value
- **Purpose**: Convenience method to set viability without entropy
- **Logic**: Calls `SetViabilityWithEntropy(v, 0f)`

##### `Color ApplyRadialShading(Color baseColor, float rNorm)`
- **Parameters**: 
  - `baseColor`: Base color to shade
  - `rNorm`: Normalized radial distance from center (0 = center, 1 = edge)
- **Returns**: Shaded color
- **Purpose**: Applies vignette-style shading based on distance from grid center
- **Logic**: Lerps shade factor from 1.1 (center, brightened) to 0.7 (edge, darkened)
- **Note**: Currently defined but not actively used in rendering pipeline

##### `void SetCombinedColor(Color viabilityColor)`
- **Parameters**: 
  - `viabilityColor`: Pre-computed color to apply
- **Purpose**: Directly sets the sprite renderer color
- **Logic**: Ensures renderer exists, sets alpha to 1.0, applies color

##### `void SetColor(Color c)`
- **Parameters**: 
  - `c`: Color to apply
- **Purpose**: Simple color setter for direct color assignment
- **Logic**: Gets sprite renderer and applies color directly

#### Properties

##### `Color LastColor { get; private set; }`
- **Purpose**: Stores the last color applied to this cell
- **Usage**: Can be used for color interpolation or debugging
- **Note**: Currently stored but not actively used in logic

## Design Patterns

### Component Pattern
- Relies on Unity's component system (requires SpriteRenderer)
- Uses `GetComponent<T>()` with caching for performance

### Gradient Mapping
- Multi-stage color gradients provide intuitive viability visualization
- Avoids extreme reds (perceptually alarming) in favor of amber/peach for high values

### Separation of Concerns
- Pure visualization logic - no simulation state management
- Receives processed values, renders them appropriately
- Color mapping isolated in dedicated methods

## Integration Points

### Used By
- `GridRenderer.cs` (Unity module) - calls visualizer methods to update cell colors based on render mode

### Dependencies
- **Unity Engine**: `MonoBehaviour`, `SpriteRenderer`, `Color`
- **None from simulation** - receives only primitive types (float, Color)

## Performance Considerations

- **Renderer Caching**: Stores SpriteRenderer reference in `_renderer` to avoid repeated `GetComponent` calls
- **Inline Color Calculation**: Color gradients computed directly without lookup tables (acceptable for 100x100 grid)
- **No Per-Frame Updates**: Only updated when simulation state changes, not every Unity Update()

## Future Enhancement Opportunities

1. **Radial Shading**: `ApplyRadialShading()` exists but unused - could add depth perception
2. **Color Interpolation**: `LastColor` property could enable smooth color transitions
3. **Custom Gradient Profiles**: Could externalize color mappings to ScriptableObjects
4. **HDR Support**: Could use HDR colors for glow effects around high-energy regions
5. **Shader-Based Rendering**: Could move color computation to GPU shaders for large grids

## Visual Design Philosophy

The color scheme follows these principles:
- **Intuitive Temperature Mapping**: Cool colors (blue) = inactive/dead, warm colors (amber) = active/viable
- **Avoid Alarm Colors**: No deep reds (perceptually stressful), uses amber/peach instead
- **High Dynamic Range**: 10 gradient stages provide smooth visual transitions
- **Entropy Subtlety**: Entropy tinting is subdued (40% blend) to avoid overwhelming viability signal
- **Accessibility Consideration**: Avoids pure red-green transitions (colorblind-friendly design)

## Code Quality Notes

- **Defensive Programming**: Null checks on renderer, graceful fallback behavior
- **Clear Naming**: Method names clearly indicate their purpose
- **Magic Numbers**: Some constants (e.g., 0.1f thresholds) could be extracted to named constants
- **Logging**: Initialization logging helps with debugging visual issues
- **Consistent API**: Multiple convenience methods (`SetViability`, `SetViabilityWithEntropy`, `SetColor`) for different use cases