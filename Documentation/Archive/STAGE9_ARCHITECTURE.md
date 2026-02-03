# Stage 9 - Export System Architecture

**Status:** ?? IN PROGRESS  
**Current Phase:** Phase 2 - Core Export Infrastructure

---

## ??? **Architecture Overview**

### **Export Flow:**
```
SimulationController.ExportLastRun()
    ?
RunExporter.Export(scenario, request, result, options)
    ?
?? Create export folder (timestamped)
?? ManifestWriter.Write()
?? MetricsCsvWriter.Write()
?? EventsCsvWriter.Write()
?? SummaryWriter.Write()
?? ChecksumWriter.Write()
    ?
Return export path
```

---

## ?? **Component Responsibilities**

### **RunExporter** (Orchestrator)
- Creates export directory
- Coordinates all writers
- Handles errors
- Returns export path

### **ManifestWriter**
- Writes `manifest.json`
- Includes run metadata, engine version, checksums
- JSON format

### **MetricsCsvWriter**
- Writes `metrics.csv`
- Time-series data (tick, viability, resources, etc.)
- CSV format for Excel/Python/R

### **EventsCsvWriter**
- Writes `events.csv`
- Simulation events (expansions, collapses, etc.)
- CSV format

### **SummaryWriter**
- Writes `summary.md`
- Human-readable markdown report
- Includes key findings, parameters, results

### **ChecksumWriter**
- Writes `checksums.txt`
- SHA-256 hashes for all exported files
- Enables reproducibility verification

---

## ?? **Export Directory Naming**

```
VIABLE_Run_{UTC_ISO_NO_COLONS}_{runIdShort}/
```

**Example:**
```
VIABLE_Run_2024-01-21T140355Z_a1b2c3/
```

**Components:**
- `VIABLE` - Engine name (constant)
- `2024-01-21T140355Z` - UTC timestamp (sortable)
- `a1b2c3` - First 6 chars of run GUID (uniqueness)

---

## ?? **Fil

e Writing Order**

1. **scenario.json** - Scenario definition
2. **request.json** - Run request
3. **result.json** - Run result (if including full state)
4. **metrics.csv** - Time-series metrics
5. **events.csv** - Simulation events
6. **summary.md** - Human-readable summary
7. **manifest.json** - Metadata + file list
8. **checksums.txt** - SHA-256 hashes (computed last)

---

## ? **Phase 1 Complete**

- [x] Created `RunExportOptions.cs`
- [x] Defined export levels (Study, Publication, Debug)
- [x] Added export directory configuration

---

## ?? **Phase 2 Next Steps**

1. Create `RunExporter.cs` (orchestrator)
2. Create writer classes (Manifest, CSV, Summary, Checksum)
3. Implement SHA-256 checksum computation
4. Test export with ConstraintsExpansionDemo

---

**Current File:** Creating RunExporter.cs...

