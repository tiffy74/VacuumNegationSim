using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Viable.Core.Unity.Configuration;
using Viable.Core.Unity.Controllers;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Inflow details section - shows when Inflow = PointSources.
    /// Shows list of point sources and button to edit them.
    /// </summary>
    public class InflowDetailsSection : CollapsibleSection
    {
        [Header("Point Source Display")]
        [SerializeField] private TextMeshProUGUI pointSourceCountText;
        [SerializeField] private TextMeshProUGUI pointSourceListText;
        [SerializeField] private Button editPointSourcesButton;

        [Header("Orchestrator")]
        [SerializeField] private SimulationUIOrchestrator orchestrator;

        [Header("Point Source Editor Modal")]
        [SerializeField] private PointSourceEditorModal pointSourceEditorModal;

        private Configuration.WorkingScenarioConfig currentConfig;

        protected override void Start()
        {
            base.Start();

            // Wire button
            if (editPointSourcesButton != null)
            {
                editPointSourcesButton.onClick.AddListener(OnEditPointSourcesClicked);
            }

            // Initialize with a default config if none provided
            if (currentConfig == null)
            {
                Debug.Log("[InflowDetailsSection] No config bound on Start, creating default config");
                currentConfig = new Configuration.WorkingScenarioConfig();
            }
        }
        /// <summary>
        /// Refresh UI controls from WorkingScenarioConfig.
        /// Called by orchestrator after preset load.
        /// </summary>
        public void Refresh(WorkingScenarioConfig cfg)
        {
            if (orchestrator != null && orchestrator.IsRefreshing())
                return;

            // Update the display using the existing UpdateDisplay logic
            currentConfig = cfg;
            UpdateDisplay();
        }

        
        public override void Bind(Configuration.WorkingScenarioConfig config)
        {
            currentConfig = config;
            UpdateDisplay();
        }

        public override void RefreshVisibility(Configuration.WorkingScenarioConfig config)
        {
            // Show only when Inflow = PointSources
            bool relevant = (config.Inflow == Configuration.InflowMode.PointSources);
            gameObject.SetActive(relevant);

            if (relevant)
            {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Unity-event compatible wrapper for RefreshVisibility.
        /// Uses cached currentConfig.
        /// </summary>
        public void RefreshVisibility()
        {
            if (currentConfig != null)
            {
                RefreshVisibility(currentConfig);
            }
            else
            {
                // If no config bound yet, hide by default
                gameObject.SetActive(false);
            }
        }

        public override void ApplyEdits(Configuration.WorkingScenarioConfig config)
        {
            // Point sources are edited via modal, already applied to config
            // Nothing to do here
        }

        private void UpdateDisplay()
        {
            if (currentConfig == null) return;

            int count = currentConfig.PointSources.Count;

            // Update count text
            if (pointSourceCountText != null)
            {
                pointSourceCountText.text = $"Point Sources: {count}";
            }

            // Update list text
            if (pointSourceListText != null)
            {
                if (count == 0)
                {
                    pointSourceListText.text = "No point sources defined. Click 'Edit Sources' to add.";
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < count; i++)
                    {
                        var src = currentConfig.PointSources[i];
                        sb.AppendLine($"Source {i + 1}: ({src.X}, {src.Y}) strength {src.Strength:F0}");
                    }
                    pointSourceListText.text = sb.ToString();
                }
            }
        }

        private void OnEditPointSourcesClicked()
        {
            Debug.Log("[InflowDetailsSection] Edit Point Sources button clicked");
            
            if (pointSourceEditorModal == null)
            {
                Debug.LogError("[InflowDetailsSection] PointSourceEditorModal reference is null! Please wire it in the Inspector.");
                return;
            }

            if (currentConfig == null)
            {
                Debug.LogError("[InflowDetailsSection] No config bound!");
                return;
            }

            // Open the modal
            pointSourceEditorModal.Open(currentConfig);
            
            // Refresh display when modal closes (poll for now, can be improved with callback)
            StartCoroutine(RefreshAfterModalCloses());
        }

        private System.Collections.IEnumerator RefreshAfterModalCloses()
        {
            // Wait a frame
            yield return null;

            // Check if modal is closed (poll every frame)
            while (pointSourceEditorModal != null && pointSourceEditorModal.gameObject.activeSelf)
            {
                yield return null;
            }

            // Modal closed, refresh display
            UpdateDisplay();
            Debug.Log("[InflowDetailsSection] Refreshed display after modal closed");
        }
    }
}
