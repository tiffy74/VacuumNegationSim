using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Base class for collapsible UI sections with header/content pattern.
    /// Provides expand/collapse functionality and implements IConfigSection.
    /// 
    /// USAGE:
    /// 1. Inherit from this class
    /// 2. Create HeaderPanel (with Button) and ContentPanel in Unity
    /// 3. Wire headerObject, contentObject, toggleButton, headerText in Inspector
    /// 4. Implement Bind(), RefreshVisibility(), ApplyEdits()
    /// 5. Click header to expand/collapse content
    /// </summary>
    public abstract class CollapsibleSection : MonoBehaviour, IConfigSection
    {
        [Header("Collapsible Section Components")]
        [Tooltip("Header panel GameObject (contains button + text)")]
        [SerializeField] protected GameObject headerObject;

        [Tooltip("Content panel GameObject (contains controls)")]
        [SerializeField] protected GameObject contentObject;

        [Tooltip("Button component on header (for click to expand/collapse)")]
        [SerializeField] protected Button toggleButton;

        [Tooltip("Header text component (optional, for visual feedback)")]
        [SerializeField] protected TextMeshProUGUI headerText;

        [Header("Collapsible State")]
        [Tooltip("Is section expanded by default?")]
        [SerializeField] protected bool isExpandedByDefault = true;

        private bool isExpanded;

        protected virtual void Start()
        {
            // Set initial collapsed state
            isExpanded = isExpandedByDefault;
            UpdateContentVisibility();

            // Wire toggle button
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleExpanded);
            }
        }

        /// <summary>
        /// Toggle section expanded/collapsed state.
        /// </summary>
        public void ToggleExpanded()
        {
            isExpanded = !isExpanded;
            UpdateContentVisibility();
        }

        /// <summary>
        /// Set expanded state explicitly.
        /// </summary>
        public void SetExpanded(bool expanded)
        {
            isExpanded = expanded;
            UpdateContentVisibility();
        }

        /// <summary>
        /// Update content panel visibility based on isExpanded state.
        /// </summary>
        private void UpdateContentVisibility()
        {
            if (contentObject != null)
            {
                contentObject.SetActive(isExpanded);
            }

            // Optional: Update header text to show expand/collapse indicator
            if (headerText != null)
            {
                string originalText = headerText.text.TrimEnd(' ', '?', '?');
                headerText.text = isExpanded ? $"{originalText} ?" : $"{originalText} ?";
            }
        }

        #region IConfigSection Implementation (Abstract)

        /// <summary>
        /// Bind UI controls to config values.
        /// Override in derived classes to implement section-specific binding.
        /// </summary>
        public abstract void Bind(Configuration.WorkingScenarioConfig config);

        /// <summary>
        /// Update section visibility based on config state.
        /// Override in derived classes to implement conditional visibility.
        /// </summary>
        public abstract void RefreshVisibility(Configuration.WorkingScenarioConfig config);

        /// <summary>
        /// Apply current UI values to config.
        /// Override in derived classes to implement section-specific editing.
        /// </summary>
        public abstract void ApplyEdits(Configuration.WorkingScenarioConfig config);

        #endregion

        #region Unity Lifecycle

        protected virtual void OnDestroy()
        {
            // Clean up button listener
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(ToggleExpanded);
            }
        }

        #endregion
    }
}
