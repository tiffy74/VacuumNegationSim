# Vacuum Negation Simulator

**A Unity-based simulation exploring emergent spacetime geometry, black hole formation, and energy propagation through configuration space.**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Unity Version](https://img.shields.io/badge/Unity-2022%2B-blue.svg)](https://unity.com/)
[![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.1-purple.svg)](https://dotnet.microsoft.com/)

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

## 🏗️ Architecture

### Core Components

- `Program.cs`: Entry point, CLI argument parsing, and simulation initialization.
- `Simulation/`, `Components/`, `Systems/`: Organized folders for ECS architecture (entities, components, systems).
- `Shaders/`: Contains shader graphs for visual effects (e.g., entropy visualization).
- `Prefabs/`: Placeholder for any prefabricated GameObjects (e.g., camera setups, UI elements).
- `Materials/`: Adjustable materials for different cell types/states.
- `Tests/`: Unit and integration tests for critical systems (e.g., viability calculations).

### Experimental Features

- **3D Support**: Initial setups for converting grid-based systems to 3D (optional, experimental)
- **Attractor Mechanism**: Early-stage implementation of viability-based attractors

---

## 🔄 Simulation Loop

Every frame:
1. Update entropy and symmetry metrics.
2. Compute viability for each cell.
3. Propagate or decay negation accordingly.
4. Apply attractor influence.
5. Visualize updated system.

---

## 🔧 Technical Stack

| Component             | Purpose                                              |
|----------------------|------------------------------------------------------|
| Unity (2022+)        | Game engine                                          |
| ECS (DOTS) (optional)| High-performance entity/grid updates                 |
| Shader Graph         | Dynamic visual overlays for entropy, viability       |
| Burst + Jobs System  | Parallelize updates across large cell fields         |
| ScriptableObjects    | Parameter tuning for entropy, energy, thresholds     |
| C# Unit Tests        | Validate propagation, decay, and viability logic     |

---

## 🚀 Future Extensions

- **Quantum collapse**: Simulate viability-based wavefunction reduction.
- **Multi-universe inflation**: Test branching spacetime events.
- **Viability-weighted path integrals**: Model thermodynamic versions of quantum dynamics.
- **3D geometry support**: Model foam-like cosmological structure in real-time.

---

## 🏁 Getting Started

Coming soon...

- Starter Unity project
- Core C# scripts for viability and entropy systems
- Visualizer components
- Example configuration presets

---

## 📜 License

MIT License. Use freely with attribution to the theoretical framework of *Energy as Vacuum Negation* by T. M. Prosser.

---
