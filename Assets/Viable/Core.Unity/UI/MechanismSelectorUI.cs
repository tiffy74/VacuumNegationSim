using UnityEngine;

namespace Viable.Core.Unity
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System;

    namespace Viable.Core.Unity.UI
    {
        /// <summary>
        /// UI panel for selecting which Stage 13 mechanism to configure.
        /// Swaps active mechanism panel based on dropdown selection.
        /// </summary>
        public class MechanismSelectorUI : MonoBehaviour
        {
            [Header("UI Elements")]
            [SerializeField] private TMP_Dropdown mechanismDropdown;

            [Header("Mechanism Panels")]
            [SerializeField] private GameObject pointSourcesPanel;
            [SerializeField] private GameObject boundaryModePanel;
            [SerializeField] private GameObject diffusionModePanel;
            [SerializeField] private GameObject hysteresisPanel;
            [SerializeField] private GameObject maskShapePanel;
            [SerializeField] private GameObject refinementPanel;

            public event Action<int> OnMechanismChanged;

            public void Initialize()
            {
                if (mechanismDropdown != null)
                {
                    mechanismDropdown.onValueChanged.AddListener(OnMechanismSelected);
                }

                // Hide all panels initially
                HideAllPanels();
            }

            private void OnMechanismSelected(int index)
            {
                HideAllPanels();

                // Show selected panel
                switch (index)
                {
                    case 0: // No Mechanisms
                        break;
                    case 1: // Point Sources
                        if (pointSourcesPanel != null) pointSourcesPanel.SetActive(true);
                        break;
                    case 2: // Boundary Mode
                        if (boundaryModePanel != null) boundaryModePanel.SetActive(true);
                        break;
                    case 3: // Diffusion Mode
                        if (diffusionModePanel != null) diffusionModePanel.SetActive(true);
                        break;
                    case 4: // Hysteresis
                        if (hysteresisPanel != null) hysteresisPanel.SetActive(true);
                        break;
                    case 5: // Mask Shape
                        if (maskShapePanel != null) maskShapePanel.SetActive(true);
                        break;
                    case 6: // Refinement
                        if (refinementPanel != null) refinementPanel.SetActive(true);
                        break;
                }

                OnMechanismChanged?.Invoke(index);
                Debug.Log($"[MechanismSelectorUI] Selected mechanism: {index}");
            }

            private void HideAllPanels()
            {
                if (pointSourcesPanel != null) pointSourcesPanel.SetActive(false);
                if (boundaryModePanel != null) boundaryModePanel.SetActive(false);
                if (diffusionModePanel != null) diffusionModePanel.SetActive(false);
                if (hysteresisPanel != null) hysteresisPanel.SetActive(false);
                if (maskShapePanel != null) maskShapePanel.SetActive(false);
                if (refinementPanel != null) refinementPanel.SetActive(false);
            }
        }
    }
}
