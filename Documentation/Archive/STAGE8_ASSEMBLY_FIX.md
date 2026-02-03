# Stage 8 - Assembly Definition Fix

**Issue:** Core.Unity can't find UnityEngine types  
**Cause:** Unity hasn't regenerated project files yet  
**Status:** ?? **REQUIRES UNITY IMPORT**

---

## ?? **The Problem**

Visual Studio is trying to build `Viable.Core.Unity` but:
1. Unity hasn't imported the new `.asmdef` file yet
2. `.csproj` files don't have UnityEngine references
3. Visual Studio doesn't know about Unity types

This is **expected** and **normal** when creating new assemblies.

---

## ? **The Solution**

**You MUST open Unity to fix this:**

### **Step 1: Close Visual Studio**
```
File ? Exit
(Don't try to fix errors in VS - won't work!)
```

### **Step 2: Open Unity**
```
Double-click your Unity project
Wait for import (2-3 minutes)
```

### **Step 3: Unity Will:**
- ? Detect new `Viable.Core.Unity.asmdef`
- ? Generate `Viable.Core.Unity.csproj` with UnityEngine references
- ? Link all assemblies properly
- ? Compile Core.Unity assembly

### **Step 4: Check Unity Console**
**Expected:** Compilation errors (scene references broken - normal!)

**Warnings like:**
```
"The referenced script (SimulationController) on this Behaviour is missing!"
```

**This is OKAY!** The scripts moved - you just need to update scene references.

### **Step 5: Regenerate VS Solution**
```
In Unity: Assets ? Open C# Project
```

### **Step 6: Verify in Visual Studio**
```
Solution Explorer should now show:
- Viable.Contracts
- Viable.Engine  
- Viable.Engine.Tests
- Viable.Core.Unity  ? NEW!
- Assembly-CSharp

Build ? Rebuild Solution
Should compile successfully
```

---

## ?? **Why This Happens**

Unity uses `.asmdef` files to generate `.csproj` files.  
Visual Studio can't build Unity code without these generated files.

**Process:**
```
1. Create .asmdef file (done ?)
2. Unity imports it (NEEDS DOING ?)
3. Unity generates .csproj (automatic)
4. VS can now build (works after step 2)
```

---

## ?? **DO NOT**

- ? Try to fix errors in Visual Studio
- ? Delete Core.Unity files
- ? Manually edit `.csproj` files
- ? Add Unity DLL references manually

**Just open Unity and let it do its thing!**

---

## ?? **After Unity Import**

You'll still need to:
1. Fix scene references (old scripts ? new Core.Unity scripts)
2. Assign preset to SimulationController
3. Test simulation runs
4. Run 25 Engine tests

See `STAGE8_TESTING_INSTRUCTIONS.md` for details.

---

**Current Action:** Close VS, Open Unity, Wait for Import

