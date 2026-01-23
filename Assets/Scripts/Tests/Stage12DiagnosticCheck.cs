using UnityEngine;

/// <summary>
/// Quick diagnostic to verify Stage 12 setup in current scene.
/// Attach to any GameObject and check Console on Play.
/// </summary>
public class Stage12DiagnosticCheck : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Stage 12 Scene Diagnostic ===");
        
        // Check for required components
        bool hasSimController = FindObjectOfType<Viable.Core.Unity.Controllers.SimulationController>() != null;
        bool hasUIManager = FindObjectOfType<Viable.Core.Unity.UI.UIManager>() != null;
        bool hasPresetSelector = FindObjectOfType<Viable.Core.Unity.UI.PresetSelectorUI>() != null;
        bool hasSimControls = FindObjectOfType<Viable.Core.Unity.UI.SimulationControlsUI>() != null;
        bool hasInfoDisplay = FindObjectOfType<Viable.Core.Unity.UI.InfoDisplayUI>() != null;
        bool hasExportUI = FindObjectOfType<Viable.Core.Unity.UI.ExportUI>() != null;
        
        // Report results
        Debug.Log($"SimulationController: {(hasSimController ? "? FOUND" : "? MISSING")}");
        Debug.Log($"UIManager: {(hasUIManager ? "? FOUND" : "? MISSING")}");
        Debug.Log($"PresetSelectorUI: {(hasPresetSelector ? "? FOUND" : "? MISSING")}");
        Debug.Log($"SimulationControlsUI: {(hasSimControls ? "? FOUND" : "? MISSING")}");
        Debug.Log($"InfoDisplayUI: {(hasInfoDisplay ? "? FOUND" : "? MISSING")}");
        Debug.Log($"ExportUI: {(hasExportUI ? "? FOUND" : "? MISSING")}");
        
        // Check scene name
        Debug.Log($"Current Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // Overall status
        bool isStage12Complete = hasSimController && hasUIManager && hasPresetSelector && 
                                  hasSimControls && hasInfoDisplay && hasExportUI;
        
        if (isStage12Complete)
        {
            Debug.Log("=== ? Stage 12 Setup Complete in This Scene ===");
        }
        else
        {
            Debug.LogWarning("=== ? Stage 12 Setup Incomplete ===");
            Debug.LogWarning("Missing components need to be added to the scene!");
            Debug.LogWarning("See STAGE12_UNITY_SETUP.md for setup instructions");
        }
    }
}
