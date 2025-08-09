# Vacuum Negation Simulator

**A Unity-based simulation of the theoretical framework _Energy as Vacuum Negation_.**  
This project visualizes how structure, energy, and persistence emerge from an infinite true vacuum — not through design, but through thermodynamic viability conditions.

---

## 🧠 Conceptual Overview

This simulation explores the idea that:

- **The true vacuum** is an infinite, geometry-less state with maximum entropy and perfect symmetry.
- **Negation** is the emergence of structure from this nullstate — measured by deviation from entropy and symmetry.
- **Energy** is not a substance, but the **thermodynamic cost of persisting structure**.
- **Persistence** is governed by a viability equation:

- - **Negation clusters (e.g., black holes)** can become over-viable, causing inflationary propagation into the surrounding vacuum.
- **Negation cannot propagate infinitely**; it thins out, eventually reverting to vacuum — naturally creating pockets of persistent structure and voids.
- **Persistent structures act as attractors**, increasing local viability and enabling further structure formation.

---

## 🎯 Simulation Goals

- Demonstrate structure emerging **spontaneously** from overcompensated negation.
- Simulate **self-terminating inflation**, not through explicit boundaries but through natural viability failure.
- Show how **pockets of persistent negation** act as **viability attractors**.
- Let **voids re-form naturally** where viability falls below threshold.
- Provide a **modular framework** for theoretical experimentation and visual education.

---

## 🗂️ Core Modules

### 1. Simulation Grid Manager
- Manages a 2D (or optional 3D) array of `Cells`.
- Each cell holds:
- `bool IsVacuum`
- `float Entropy`
- `float Symmetry`
- `float Energy`
- `float Viability`
- `bool IsNegationSource`
- `List<Cell> Neighbors`

### 2. Negation Engine
- Computes viability `V(C, t)` per cell.
- Applies viability rules:
- If `V > 1`: structure persists/emerges.
- If `V < 1`: structure collapses (returns to vacuum).
- Factors:
- `E_in`: influenced by nearby structures.
- `E_loss`: entropy, distance, dissipation.
- `E_thresh`: minimum energy needed to persist.

### 3. Entropy Manager
- Modifies local entropy based on:
- Neighboring structure presence/absence.
- Environmental noise.
- Collapse events (increase local entropy).

### 4. Negation Propagation System
- Spreads negation to viable neighbors.
- No hardcoded range — viability governs spread.
- Decay handled naturally through entropy and distance.

### 5. Attractor Field System
- Persistent negation emits viability bias.
- Affects neighbor cells’ `E_in`, `Entropy`, or `E_thresh`.
- Can result in:
- Cluster formation.
- Structure chain reactions.
- Void reinforcement when bias is absent.

### 6. Inflation Trigger
- Special initial configuration with:
- Extremely high `E_in`
- Low `Entropy`
- High asymmetry
- Results in rapid negation expansion.
- Automatically halts when `V < 1` in outer cells.

### 7. Collapse Manager
- Monitors structure persistence.
- Collapses any structure that falls below viability threshold for N frames.
- Collapse increases entropy in local region.

### 8. Visualizer
- Visual cues:
- **Viability**: color intensity (red = high, blue = low, gray = vacuum)
- **Entropy**: noise grain overlay or distortion field
- **Attractors**: field lines or glow effects
- **Negation Type**: wavelet, particle, ripple, or collapsed zone
- Toggle views:
- Entropy map
- Viability field
- Negation source view

---

## 🔄 Simulation Loop

Every frame:
1. Update entropy and symmetry metrics.
2. Compute viability for each cell.
3. Propagate or decay negation accordingly.
4. Apply attractor influence.
5. Visualize updated system.

---

## 🧪 Testable Phenomena

| Scenario                    | Expected Outcome                                      |
|----------------------------|-------------------------------------------------------|
| Vacuum state               | Nothing happens — stable nullstate                   |
| Negation burst             | Inflation spreads until viability drops              |
| Persistent pocket          | Becomes attractor, enabling more local negation      |
| Excess entropy injection   | Structure decays; voids reform                       |
| Competing attractors       | Stronger one dominates, weaker one collapses         |
| Multi-source interaction   | Complex clustering or destructive interference        |

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
