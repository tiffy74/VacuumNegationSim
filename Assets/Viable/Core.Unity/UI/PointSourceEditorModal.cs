using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Modal dialog for editing point sources in PointSources inflow mode.
    /// Allows adding, editing, and removing individual point sources.
    /// </summary>
    public class PointSourceEditorModal : MonoBehaviour
    {
        [Header("Modal UI")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button addSourceButton;
        [SerializeField] private Button applyButton;

        [Header("Source List Container")]
        [SerializeField] private Transform sourceListContainer;
        [SerializeField] private GameObject sourceRowPrefab;

        [Header("Grid Constraints")]
        [SerializeField] private TMP_InputField gridWidthInput;
        [SerializeField] private TMP_InputField gridHeightInput;

        private Configuration.WorkingScenarioConfig currentConfig;
        private List<PointSourceEditorRow> sourceRows = new List<PointSourceEditorRow>();

        private void Start()
        {
            // Wire buttons
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            if (addSourceButton != null)
                addSourceButton.onClick.AddListener(OnAddSourceClicked);

            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClicked);

            // Start hidden
            if (modalPanel != null)
                modalPanel.SetActive(false);
        }

        /// <summary>
        /// Open the modal with the current configuration.
        /// </summary>
        public void Open(Configuration.WorkingScenarioConfig config)
        {
            currentConfig = config;

            // Show grid constraints
            if (gridWidthInput != null)
                gridWidthInput.text = config.GridWidth.ToString();

            if (gridHeightInput != null)
                gridHeightInput.text = config.GridHeight.ToString();

            // Clear existing rows
            ClearSourceRows();

            // Create rows for each existing source
            foreach (var source in config.PointSources)
            {
                AddSourceRow(source);
            }

            // Show modal
            if (modalPanel != null)
                modalPanel.SetActive(true);
        }

        /// <summary>
        /// Close the modal without applying changes.
        /// </summary>
        public void Close()
        {
            if (modalPanel != null)
                modalPanel.SetActive(false);
        }

        private void OnCloseClicked()
        {
            Close();
        }

        private void OnAddSourceClicked()
        {
            if (currentConfig == null) return;

            // Create a new point source at center of grid
            int defaultX = currentConfig.GridWidth / 2;
            int defaultY = currentConfig.GridHeight / 2;
            double defaultStrength = 1e5;

            var newSource = new Configuration.PointSourceData(defaultX, defaultY, defaultStrength);
            
            // Add to config
            currentConfig.PointSources.Add(newSource);

            // Add UI row
            AddSourceRow(newSource);

            Debug.Log($"[PointSourceEditorModal] Added new source at ({defaultX}, {defaultY})");
        }

        private void OnApplyClicked()
        {
            if (currentConfig == null) return;

            // Clear existing sources
            currentConfig.PointSources.Clear();

            // Collect data from all rows
            foreach (var row in sourceRows)
            {
                if (row != null && row.gameObject.activeSelf)
                {
                    var sourceData = row.GetSourceData();
                    currentConfig.PointSources.Add(sourceData);
                }
            }

            Debug.Log($"[PointSourceEditorModal] Applied {currentConfig.PointSources.Count} point sources");

            // Close modal
            Close();
        }

        private void AddSourceRow(Configuration.PointSourceData sourceData)
        {
            if (sourceRowPrefab == null || sourceListContainer == null)
            {
                Debug.LogError("[PointSourceEditorModal] sourceRowPrefab or sourceListContainer is null!");
                return;
            }

            // Instantiate row
            GameObject rowObj = Instantiate(sourceRowPrefab, sourceListContainer);
            PointSourceEditorRow row = rowObj.GetComponent<PointSourceEditorRow>();

            if (row == null)
            {
                Debug.LogError("[PointSourceEditorModal] sourceRowPrefab does not have PointSourceEditorRow component!");
                Destroy(rowObj);
                return;
            }

            // Initialize row
            row.Initialize(sourceData, () => RemoveSourceRow(row));
            sourceRows.Add(row);
        }

        private void RemoveSourceRow(PointSourceEditorRow row)
        {
            if (row != null)
            {
                sourceRows.Remove(row);
                Destroy(row.gameObject);
                Debug.Log("[PointSourceEditorModal] Removed source row");
            }
        }

        private void ClearSourceRows()
        {
            foreach (var row in sourceRows)
            {
                if (row != null)
                    Destroy(row.gameObject);
            }
            sourceRows.Clear();
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();

            if (addSourceButton != null)
                addSourceButton.onClick.RemoveAllListeners();

            if (applyButton != null)
                applyButton.onClick.RemoveAllListeners();
        }
    }
}
