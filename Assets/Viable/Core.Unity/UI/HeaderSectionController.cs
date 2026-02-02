using UnityEngine;
using UnityEngine.UI;
using Viable.Core.Unity.Controllers;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Controls the HeaderSection buttons to ensure only one section is expanded at a time.
    /// Manages the visibility of MechanismsSection, CoreParametersSection, and detail sections.
    /// </summary>
    public class HeaderSectionController : MonoBehaviour
    {
        [Header("Header Buttons")]
        [SerializeField] private Button mechanismsButton;
        [SerializeField] private Button coreParametersButton;
        [SerializeField] private Button topologyDetailsButton;
        [SerializeField] private Button inflowDetailsButton;
        [SerializeField] private Button diffusionDetailsButton;
        [SerializeField] private Button viabilityDetailsButton;

        [Header("Content Sections")]
        [SerializeField] private GameObject mechanismsSection;
        [SerializeField] private GameObject coreParametersSection;
        [SerializeField] private GameObject topologyDetailsSection;
        [SerializeField] private GameObject inflowDetailsSection;
        [SerializeField] private GameObject diffusionDetailsSection;
        [SerializeField] private GameObject viabilityDetailsSection;

        [Header("Orchestrator")]
        [SerializeField] private SimulationUIOrchestrator orchestrator;

        private GameObject currentlyActiveSection;

        private void Start()
        {
            // Wire button click events
            if (mechanismsButton != null)
                mechanismsButton.onClick.AddListener(() => ShowSection(mechanismsSection));
            
            if (coreParametersButton != null)
                coreParametersButton.onClick.AddListener(() => ShowSection(coreParametersSection));
            
            if (topologyDetailsButton != null)
                topologyDetailsButton.onClick.AddListener(() => ShowSection(topologyDetailsSection));
            
            if (inflowDetailsButton != null)
                inflowDetailsButton.onClick.AddListener(() => ShowSection(inflowDetailsSection));
            
            if (diffusionDetailsButton != null)
                diffusionDetailsButton.onClick.AddListener(() => ShowSection(diffusionDetailsSection));
            
            if (viabilityDetailsButton != null)
                viabilityDetailsButton.onClick.AddListener(() => ShowSection(viabilityDetailsSection));

            // Show mechanisms section by default
            ShowSection(mechanismsSection);
        }

        /// <summary>
        /// Shows the specified section and hides all others.
        /// </summary>
        private void ShowSection(GameObject sectionToShow)
        {
            if (sectionToShow == null) return;

            // Hide all sections
            HideAllSections();

            // Show the requested section
            sectionToShow.SetActive(true);
            currentlyActiveSection = sectionToShow;

            Debug.Log($"HeaderSectionController: Showing {sectionToShow.name}");
        }

        /// <summary>
        /// Hides all content sections.
        /// </summary>
        private void HideAllSections()
        {
            SetSectionActive(mechanismsSection, false);
            SetSectionActive(coreParametersSection, false);
            SetSectionActive(topologyDetailsSection, false);
            SetSectionActive(inflowDetailsSection, false);
            SetSectionActive(diffusionDetailsSection, false);
            SetSectionActive(viabilityDetailsSection, false);
        }

        private void SetSectionActive(GameObject section, bool active)
        {
            if (section != null)
            {
                section.SetActive(active);
            }
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (mechanismsButton != null)
                mechanismsButton.onClick.RemoveAllListeners();
            
            if (coreParametersButton != null)
                coreParametersButton.onClick.RemoveAllListeners();
            
            if (topologyDetailsButton != null)
                topologyDetailsButton.onClick.RemoveAllListeners();
            
            if (inflowDetailsButton != null)
                inflowDetailsButton.onClick.RemoveAllListeners();
            
            if (diffusionDetailsButton != null)
                diffusionDetailsButton.onClick.RemoveAllListeners();
            
            if (viabilityDetailsButton != null)
                viabilityDetailsButton.onClick.RemoveAllListeners();
        }
    }
}
