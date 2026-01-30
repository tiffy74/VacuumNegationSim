using UnityEngine;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Right dock container.
    /// Panel switching is now handled by DockModeController (dropdown-based).
    /// This component only holds references to panels for external access.
    /// </summary>
    public class RightDockUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject setupPanel;
        [SerializeField] private GameObject inspectPanel;
        [SerializeField] private GameObject exportPanel;

        [Header("Controller")]
        [SerializeField] private DockModeController dockModeController;

        // Public accessors for panels
        public GameObject SetupPanel => setupPanel;
        public GameObject InspectPanel => inspectPanel;
        public GameObject ExportPanel => exportPanel;

        /// <summary>
        /// Initialize dock (if needed for future setup).
        /// Panel switching is handled by DockModeController now.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[RightDockUI] Initialized (panel switching via DockModeController)");
        }

        /// <summary>
        /// Programmatic panel switching (delegates to DockModeController).
        /// </summary>
        public void SwitchToSetup()
        {
            if (dockModeController != null)
                dockModeController.SwitchToSetup();
        }

        public void SwitchToInspect()
        {
            if (dockModeController != null)
                dockModeController.SwitchToInspect();
        }

        public void SwitchToExport()
        {
            if (dockModeController != null)
                dockModeController.SwitchToExport();
        }
    }
}
