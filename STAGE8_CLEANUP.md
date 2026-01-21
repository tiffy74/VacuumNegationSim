# Stage 8 - Cleanup Old Files

**Status:** ?? **CLEANUP REQUIRED**  
**Issue:** Old files still present after Stage 8 move

---

## ?? **Problem**

After moving files to `Assets/Viable/Core.Unity/`, the old files in `Assets/Scripts/` are still present and causing conflicts:

1. `Assembly-CSharp.asmdef` - Reserved name conflict (NOW DELETED ?)
2. Old script files - Duplicates of moved files

---

## ??? **Files to Delete**

### **Already Moved to Core.Unity (Safe to Delete):**

1. ? `Assets/Scripts/Core/SimulationController.cs`  
   ? ? Moved to `Assets/Viable/Core.Unity/Controllers/SimulationController.cs`

2. ? `Assets/Scripts/Core/SimulationGrid.cs`  
   ? ? Moved to `Assets/Viable/Core.Unity/Controllers/SimulationGrid.cs`

3. ? `Assets/Scripts/Unity/GridRenderer.cs`  
   ? ? Moved to `Assets/Viable/Core.Unity/Rendering/GridRenderer.cs`

4. ? `Assets/Scripts/Visuals/CameraController.cs`  
   ? ? Moved to `Assets/Viable/Core.Unity/Visuals/CameraController.cs`

5. ? `Assets/Scripts/Visuals/CellVisualiser.cs`  
   ? ? Moved to `Assets/Viable/Core.Unity/Visuals/CellVisualiser.cs`

### **Keep (Not Yet Moved):**

- ? `Assets/Scripts/Core/SimulationUIController.cs` (UI controls - Stage 12)
- ? `Assets/Scripts/Events/EventInterface.cs` (if still used)

---

## ?? **How to Clean Up**

### **Option 1: Delete in Unity (Recommended)**

1. **In Unity Project window:**
```
Assets/Scripts/Core/SimulationController.cs ? Delete
Assets/Scripts/Core/SimulationGrid.cs ? Delete
Assets/Scripts/Unity/GridRenderer.cs ? Delete
Assets/Scripts/Visuals/CameraController.cs ? Delete
Assets/Scripts/Visuals/CellVisualiser.cs ? Delete
```

2. **If folders are empty, delete them too:**
```
Assets/Scripts/Unity/ ? Delete (if empty)
Assets/Scripts/Visuals/ ? Delete (if empty)
Assets/Scripts/Core/ ? Keep (has SimulationUIController)
```

### **Option 2: Let Me Delete Via Code**

I can delete them for you right now if you want.

---

## ? **After Cleanup**

Your structure will be:

```
Assets/
?? Viable/
?  ?? Contracts/
?  ?? Engine/
?  ?? Engine.Tests/
?  ?? Core.Unity/          ? All Unity code here
?     ?? Controllers/
?     ?? Rendering/
?     ?? Visuals/
?     ?? Presets/
?
?? Scripts/
   ?? Core/
   ?  ?? SimulationUIController.cs  ? Keep (Stage 12)
   ?? Events/
      ?? EventInterface.cs           ? Keep (if used)
```

---

## ?? **Verification**

After cleanup:
1. Unity should compile with 0 errors
2. No duplicate class warnings
3. Scene references should use Core.Unity versions
4. Tests should still pass (25/25)

---

**Action Required:** Choose cleanup method and execute.

