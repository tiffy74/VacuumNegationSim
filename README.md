# Vacuum Negation Simulator

**A Unity-based simulation exploring emergent spacetime geometry, black hole formation, and energy propagation through configuration space.**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Unity Version](https://img.shields.io/badge/Unity-2022%2B-blue.svg)](https://unity.com/)
[![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.1-purple.svg)](https://dotnet.microsoft.com/)

---

## 📚 Documentation Navigation

### Quick Start
- [Main README (you are here)](README.md) - Overview, setup, conceptual framework
- [Assets Overview](Assets/ASSETS.md) - Asset organization and structure

### Code Documentation by Module

#### 🎮 Unity Integration Layer
- [**Core Module**](Assets/Scripts/Core/CORE.md) - Controllers, initialization, UI
  - `SimulationController.cs` - Main orchestrator
  - `SimulationGrid.cs` - Grid spawning
  - `SimulationUIController.cs` - UI buttons
  - `Cell.cs` - Cell marker component

- [**Visuals Module**](Assets/Scripts/Visuals/VISUALS.md) - Rendering and visualization
  - `CellVisualiser.cs` - Cell color management

- [**Unity Module**](Assets/Scripts/Unity/UNITY.md) - Unity-specific adapters
  - `GridRenderer.cs` - Grid-wide rendering orchestration
  - `RenderMode` enum - Visualization modes

#### 🧮 Pure Simulation Layer

- [**Domain Module**](Assets/Scripts/Domain/DOMAIN.md) - Data structures
  - `GridState.cs` - Per-cell state arrays
  - `SimContext.cs` - Global simulation context
  - `SimConfig.cs` - Configuration parameters

- [**Simulation Module**](Assets/Scripts/Simulation/SIMULATION.md) - Engine architecture
  - `SimulationEngine.cs` - Tick orchestrator
  - `ISimStep.cs` - Step interface
  - `LegacyTickStep.cs` - 4-pass implementation

- [**Events Module**](Assets/Scripts/Events/EVENTS.md) - Physics simulation
  - `Pass1.cs` - Energy propagation & BH attraction
  - `Pass2.cs` - Viability & entropy computation
  - `Pass3.cs` - Global energy replenishment
  - `Pass4.cs` - Entropy diffusion
  - `BlackHoles.cs` - BH formation, growth, merging
  - `FieldWave.cs` - Configuration space propagation

#### 📂 Additional Documentation
- [**Scripts Overview**](Assets/Scripts/SCRIPTS.md) - High-level script organization

---

## 🧠 Conceptual Overview

This simulation tests a theoretical framework where:

- **Configuration Space** is a fundamental substrate that pervades everywhere (analogous to spacetime in General Relativity)
- **Energy** propagates through active configuration space, creating observable field structures
- **Black Holes** represent regions where configuration space has collapsed—energy cannot exist there
- **Stable Dependencies** form when energy encounters geometric constraints, creating pressure buildups (visible as "halos")
- **Emergence** drives the formation of complex structures through local thermodynamic rules, not top-down design

### Physical Analogies

| **Your Framework** | **General Relativity Analogue** | **Implementation** |
|-------------------|--------------------------------|-------------------|
| Configuration Space | Spacetime manifold | `FieldPresent[i]` boolean array |
| Energy Field | Matter/energy distribution | `Nlocal[i]` float array |
| Black Holes | Event horizons (collapsed geometry) | `IsBlackHole[i]` boolean array |
| Mass-weighted attraction | Gravitational force ∝ M | `bhAttractionWeight` in Pass1 |
| Viability | Thermodynamic persistence criterion | `V[i]` computed each tick |
| Entropy | Information-theoretic complexity | `Entropy[i]` from config count + gradients |

---

## 🎯 What You'll See

When you run the simulation:

1. **Initial Seed** (tick 0-10): Small central energy patch (bright white) establishes itself
2. **Field Expansion** (tick 10+): Yellow boundaries propagate outward as configuration space activates
3. **Black Hole Formation** (tick 10+): Magenta regions appear where energy leaks into void without config space
4. **Stable Halos**: Bright regions form around black holes as energy accumulates (cannot propagate into collapsed space)
5. **Merging**: Adjacent black holes combine into single entities with increased mass/influence

### Color Legend

- 🟪 **Dark Purple/Black**: Void (no configuration space, no energy)
- 🟨 **Yellow**: Configuration space boundary (field active but low energy)
- ⬜ **White/Bright**: High-energy regions (active field with abundant energy)
- 🟣 **Magenta**: Black holes (collapsed configuration space)
- 🔵 **Blue tint**: Entropy visualization (when enabled)

---

## 🏗️ Architecture Overview


````````markdown
VacuumNegationSim/ ├── Assets/ │   ├── Scenes/                    # Unity scenes │   ├── Prefabs/                   # GameObject prefabs │   └── Scripts/ │       ├── Core/                  # Unity controllers & UI │       ├── Domain/                # Pure data structures │       ├── Simulation/            # Engine architecture │       ├── Events/                # Physics passes │       ├── Unity/                 # Unity-specific adapters │       └── Visuals/               # Rendering logic └── README.md (this file)
````````

### Dependency Flow
````````markdown
Unity Layer (MonoBehaviour) ↓ Core (SimulationController) ↓ Simulation (SimulationEngine) ↓ Events (Pass1-4, BlackHoles, FieldWave) ↓ Domain (GridState, SimContext, SimConfig)
````````

**Key Design Principle**: Pure simulation logic (Domain, Simulation, Events) has zero Unity dependencies and can be unit tested independently.

---

## 🔬 Scientific Framework

### 1. Configuration Space Theory

**Hypothesis**: A fundamental geometric substrate exists everywhere, but only becomes "observable" where it mediates energy propagation.

- **Latent Config Space**: Exists in void regions (like flat spacetime far from mass)
- **Active Config Space** (`FieldPresent=true`): Where energy can propagate (like curved spacetime near matter)
- **Collapsed Config Space** (`IsBlackHole=true`): Impossible configurations (like singularities)

**Implementation**: See [FieldWave.cs documentation](Assets/Scripts/Events/EVENTS.md#fieldwavecs---configuration-space-propagation)

### 2. Energy Propagation

**Principle**: Energy flows through active configuration space, biased toward regions of lower entropy (more stable configurations).

**Key Equation** (Pass1.cs):
```cSharp
float portion = available * (weights[d] / totalWeight);
```

Where `weights[d]` is influenced by:
- Base weight = 1.0
- Black hole attraction: +0.1 × (adjacent BH mass)
- Result: Energy flows preferentially toward BH boundaries

**Physics Analogue**: Geodesic motion in curved spacetime (particles follow paths of least action)

### 3. Black Hole Formation & Dynamics

**Formation Criterion** (Pass1.cs):
- Energy leaks into void without active configuration space
- After tick 10 (protects initial seed)
- Charge accumulates until threshold reached
- Instant collapse when threshold exceeded

**Growth Mechanism** (BlackHoles.cs):
- Cells with 2+ BH neighbors become trapped (geometric collapse)
- Union-find data structure tracks merged entities
- Total mass = sum of merged BH masses

**Physics Analogue**: 
- Formation ≈ Trapped surface formation (Penrose criterion)
- Growth ≈ Event horizon expansion (area increase law)
- Mass ∝ energy absorbed (conservation of mass-energy)

### 4. Viability & Persistence

**Definition** (SimulationController.cs):
```cSharp
float ComputeViability(float incomingFlow, float nlocal, float entropy)
{ 
	float gain = 1f + EntropyViabilityGainA * (1f - Exp(-EntropyViabilityGainK * entropy));
	return (incomingFlow * gain - DecayLoss) / EthreshEff;
}
```

**Interpretation**:
- `V > 0`: Structure persists (energy input exceeds decay)
- `V < 0`: Structure decays (insufficient energy to maintain)
- Entropy **boosts** viability (counter-intuitive but represents configuration richness)

**Physics Analogue**: Thermodynamic stability criterion (Gibbs free energy)

---

## 🎮 How to Use

### Building & Running

**Requirements**:
- Unity 2022.3 LTS or later
- .NET Framework 4.7.1
- Git LFS (for assets, if applicable)

**Setup**:
````bash
git clone https://github.com/tiffy74/VacuumNegationSim.git cd VacuumNegationSim
````

**Run**:
1. Open scene: `Assets/Scenes/Main.unity`
2. Press Play
3. Use UI buttons: **Play** / **Pause** / **Restart** / **Exit**

### Inspector Parameters

All simulation parameters are exposed in the **SimulationController** inspector:

- **Global Budget**: Energy pool settings
- **Viability/Threshold**: Persistence criteria
- **Propagation**: Energy flow rules
- **Entropy Dynamics**: Complexity evolution
- **Field Propagation**: Configuration space expansion
- **Black Holes**: Collapse thresholds
- **Colors**: Visualization scheme

**Tip**: Pause simulation, tweak parameter, restart to see effect.

---

## 📊 Performance

### Current Complexity
- **Grid Size**: N = width × height
- **Per Tick**: O(8N) - linear scaling
  - Pass1: O(2N)
  - Pass2: O(N)
  - Pass3: O(1)
  - Pass4: O(2N)
  - Post: O(2N)
  - Render: O(N)

### Tested Configurations
- ✅ **100×100 grid** (10k cells) @ 10 FPS - Smooth
- ⚠️ **500×500 grid** (250k cells) @ 10 FPS - Laggy
- ❌ **1000×1000 grid** (1M cells) - Too slow without optimization

### Optimization Opportunities
See individual module documentation for specific optimization recommendations.

---

## 🐛 Known Issues & TODOs

### Critical
- [ ] **Energy conservation not enforced**: Add diagnostic tracking
- [ ] **No global entropy tracking**: Implement in SimContext
- [ ] **BH charge accumulation is ad-hoc**: Replace with density criterion

### Enhancements
- [ ] **Hawking radiation**: Mass-dependent energy emission
- [ ] **Gravitational waves**: Energy radiation during BH mergers
- [ ] **3D grid support**: Extend to 3D space
- [ ] **Multi-threading**: Parallelize passes
- [ ] **Save/Load**: Serialize GridState

### Visualization
- [ ] **Heatmap mode**: Energy density coloring
- [ ] **Vector field**: Energy flow direction arrows
- [ ] **BH labels**: Display mass/ID on hover
- [ ] **Timeline scrubbing**: Rewind/replay

---

## 📚 Further Reading

### Theoretical Background
- **General Relativity**: Einstein's field equations, Schwarzschild metric
- **Black Hole Thermodynamics**: Bekenstein-Hawking entropy, no-hair theorem
- **Statistical Mechanics**: Partition functions, entropy as phase space volume
- **Cellular Automata**: Conway's Game of Life, emergent complexity

### Relevant Papers
- Penrose, R. (1965). "Gravitational Collapse and Space-Time Singularities"
- Hawking, S. (1975). "Particle Creation by Black Holes"
- Verlinde, E. (2011). "On the Origin of Gravity and the Laws of Newton" (entropic gravity)

---

## 🤝 Contributing

We welcome contributions from:
- **Physicists**: Suggest more accurate physical models
- **Developers**: Optimize code, add features, fix bugs
- **Theorists**: Propose experiments to test framework

**Process**:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/hawking-radiation`)
3. Commit changes (`git commit -m 'Add Hawking radiation'`)
4. Push to branch (`git push origin feature/hawking-radiation`)
5. Open Pull Request

**Code Standards**:
- Follow existing naming conventions
- Add XML documentation to public methods
- Include unit tests for new physics
- Update relevant `.md` files if adding features

---

## 📜 License

MIT License - see [LICENSE](LICENSE) file

---

## 👤 Author

**T. M. Prosser**  
Theoretical Framework: *Energy as Vacuum Negation*

## 🙏 Acknowledgments

- Unity Technologies for the game engine
- Inspiration from General Relativity, Quantum Field Theory, and Cellular Automata research
- The open-source scientific computing community

---

## 📞 Contact

- **Issues**: [GitHub Issues](https://github.com/tiffy74