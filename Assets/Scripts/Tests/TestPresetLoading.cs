using UnityEngine;
using Viable.Core.Unity;

namespace Viable.Tests
{
    /// <summary>
    /// Manual runtime test to verify presets load correctly from Resources folder.
    /// Attach to any GameObject and check Console on Play.
    /// 
    /// For automated testing, see TestPresetLoadingPlayMode.cs
    /// </summary>
    public class TestPresetLoading : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Run test automatically on Start")]
        [SerializeField] private bool runOnStart = true;

        [Tooltip("Log individual preset details")]
        [SerializeField] private bool verboseLogging = true;

        private void Start()
        {
            if (runOnStart)
            {
                RunPresetLoadingTest();
            }
        }

        /// <summary>
        /// Run the preset loading test manually.
        /// Can be called from other scripts or Unity events.
        /// </summary>
        [ContextMenu("Run Preset Loading Test")]
        public void RunPresetLoadingTest()
        {
            Debug.Log("=== Testing Preset Loading ===");

            // Test loading all 5 presets
            string[] presetPaths = new string[]
            {
                "Presets/Examples/01_BalancedPersistence",
                "Presets/Examples/02_ResourceStress",
                "Presets/Examples/03_RapidExpansion",
                "Presets/Examples/04_CompetingRegions",
                "Presets/Examples/05_StochasticDynamics"
            };

            int successCount = 0;
            int failCount = 0;

            foreach (string path in presetPaths)
            {
                ScenarioPreset preset = Resources.Load<ScenarioPreset>(path);
                
                if (preset != null)
                {
                    successCount++;
                    
                    if (verboseLogging)
                    {
                        Debug.Log($"? Successfully loaded: {preset.PresetName}");
                        Debug.Log($"  - Description: {preset.Description}");
                        Debug.Log($"  - Grid Size: {preset.GridWidth}x{preset.GridHeight}");
                        Debug.Log($"  - Ticks/Second: {preset.TicksPerSecond}");
                        Debug.Log($"  - Seed: {(preset.Seed.HasValue ? preset.Seed.Value.ToString() : "Random")}");
                    }
                    else
                    {
                        Debug.Log($"? Loaded: {preset.PresetName}");
                    }
                }
                else
                {
                    Debug.LogError($"? Failed to load preset at path: {path}");
                    failCount++;
                }
            }

            Debug.Log($"\n=== Results: {successCount} successful, {failCount} failed ===");
            
            if (successCount == 5)
            {
                Debug.Log("? ALL PRESETS LOADED SUCCESSFULLY!");
            }
            else
            {
                Debug.LogError("? Some presets failed to load. Check paths and Resources folder setup.");
                Debug.LogError("Expected location: Assets/Resources/Presets/Examples/");
            }
        }
    }
}
