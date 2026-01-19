# Viable.Contracts

**Purpose:** Data Transfer Objects (DTOs), JSON schema models, and versioning contracts.

## Design Principles
- **No Unity Dependencies:** All types are pure C# (no UnityEngine references)
- **Serialization-Ready:** Compatible with System.Text.Json
- **Version Controlled:** Explicit schema versioning (SchemaVersion)
- **Theory-Neutral:** All names and documentation use domain-neutral terminology

## Contents
- `EngineMetadata.cs` - Engine identity and version information
- `ScenarioDefinition.cs` - Scenario configuration and initial state
- `RunRequest.cs` - Simulation execution request parameters
- `RunResult.cs` - Simulation execution results
- `SimulationEvent.cs` - Discrete events during simulation
- `StateSample.cs` - State snapshots at specific time points
- `ContractVersions.cs` - Schema versioning constants

## Usage
Contracts serve as the boundary between:
- Engine (producer of results)
- Unity UI (consumer of results, producer of requests)
- Future API layer (serialization target)

## Rules
1. All types must be serializable with System.Text.Json
2. No circular references
3. Use C# records for immutability where appropriate
4. Maintain backward compatibility within major schema versions
