using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Visuals
{
    /// <summary>
    /// Camera Controller - Automatic Grid-Centered Camera Positioning
    /// 
    /// Dynamically positions and sizes the main camera to frame the simulation grid.
    /// Ensures the entire grid is visible regardless of grid dimensions.
    /// 
    /// Design Philosophy:
    /// - Single Responsibility: Only handles camera positioning and sizing
    /// - Automatic Framing: Centers camera on grid and adjusts orthographic size
    /// - No User Input: Pure automatic behavior (no pan/zoom controls)
    /// - LateUpdate Timing: Runs after simulation updates to avoid frame lag
    /// 
    /// Lifecycle:
    /// 1. LateUpdate(): Every frame, recalculates camera position and size
    /// 2. Runtime: Continuously tracks grid dimensions (supports dynamic resizing)
    /// 
    /// Requirements:
    /// - Must be attached to GameObject with Camera component (orthographic mode)
    /// - Requires reference to SimulationGrid in Inspector
    /// - Camera depth should be negative (typically -10) to view grid
    /// 
    /// Camera Math:
    /// - Position: (Width/2, Height/2, -10) - centers on grid
    /// - Orthographic Size: max(Width, Height) / 2 - ensures full visibility
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ============================================================================
        // INSPECTOR PARAMETERS: GRID REFERENCE
        // ============================================================================

        /// <summary>
        /// Reference to the simulation grid to frame.
        /// Used to query grid dimensions (Width, Height) for camera positioning.
        /// Must be assigned in Inspector before runtime.
        /// </summary>
        [Header("Grid Reference")]
        [Tooltip("The simulation grid to center the camera on")]
        public SimulationGrid Grid;

        // ============================================================================
        // PRIVATE FIELDS: CACHED COMPONENTS
        // ============================================================================

        /// <summary>
        /// Cached camera component for performance.
        /// Retrieved once in Awake to avoid repeated GetComponent calls.
        /// </summary>
        private Camera mainCamera;

        /// <summary>
        /// Last known grid dimensions for change detection.
        /// Used to optimize camera updates when grid size is static.
        /// </summary>
        private Vector2Int lastGridDimensions;

        // ============================================================================
        // INSPECTOR PARAMETERS: CAMERA SETTINGS
        // ============================================================================

        /// <summary>
        /// Camera depth (Z position).
        /// Negative value positions camera "in front" of grid.
        /// Typical Value: -10 (Unity default orthographic depth)
        /// </summary>
        [Header("Camera Settings")]
        [Tooltip("Camera distance from grid (negative = in front)")]
        [Range(-50f, -1f)]
        [SerializeField] private float cameraDepth = -10f;

        /// <summary>
        /// Size multiplier for orthographic camera.
        /// Values > 1.0 zoom out (show more), < 1.0 zoom in (show less).
        /// Default: 1.0 (exact fit to grid)
        /// </summary>
        [Tooltip("Zoom factor (1.0 = exact fit, >1.0 = zoomed out)")]
        [Range(0.5f, 2.0f)]
        [SerializeField] private float sizeFactor = 1.0f;

        /// <summary>
        /// Padding around grid edges (as fraction of grid size).
        /// Adds visual breathing room around the simulation.
        /// 0.0 = tight fit, 0.1 = 10% padding on all sides
        /// </summary>
        [Tooltip("Padding around grid (0 = tight fit, 0.1 = 10% padding)")]
        [Range(0f, 0.3f)]
        [SerializeField] private float edgePadding = 0.05f;

        // ============================================================================
        // UNITY LIFECYCLE: INITIALIZATION
        // ============================================================================

        /// <summary>
        /// Unity Awake - Cache camera component and validate configuration.
        /// Called before Start(), ensures camera reference is available.
        /// </summary>
        private void Awake()
        {
            CacheComponents();
            ValidateConfiguration();
            InitializeLastDimensions();
        }

        /// <summary>
        /// Caches the Camera component for performance optimization.
        /// </summary>
        private void CacheComponents()
        {
            mainCamera = GetComponent<Camera>();
            
            if (mainCamera == null)
            {
                Debug.LogError("[CameraController] No Camera component found on GameObject! Adding one automatically.");
                mainCamera = gameObject.AddComponent<Camera>();
                mainCamera.orthographic = true;
            }
        }

        /// <summary>
        /// Validates Inspector configuration at startup.
        /// </summary>
        private void ValidateConfiguration()
        {
            if (Grid == null)
            {
                Debug.LogWarning("[CameraController] Grid reference not assigned in Inspector. Camera will not update until assigned.");
            }

            if (mainCamera != null && !mainCamera.orthographic)
            {
                Debug.LogWarning("[CameraController] Camera is not in orthographic mode. Forcing orthographic projection.");
                mainCamera.orthographic = true;
            }
        }

        /// <summary>
        /// Initializes last known dimensions to detect changes.
        /// </summary>
        private void InitializeLastDimensions()
        {
            if (Grid != null)
            {
                lastGridDimensions = new Vector2Int(Grid.Width, Grid.Height);
            }
            else
            {
                lastGridDimensions = Vector2Int.zero;
            }
        }

        // ============================================================================
        // UNITY LIFECYCLE: LATE UPDATE
        // ============================================================================

        /// <summary>
        /// Unity LateUpdate - Updates camera position and size every frame.
        /// 
        /// LateUpdate Timing: Runs after all Update() calls, ensuring grid
        /// dimensions are finalized before camera adjusts.
        /// 
        /// Performance: Early-exits if grid reference missing or unchanged.
        /// </summary>
        private void LateUpdate()
        {
            if (!CanUpdateCamera())
                return;

            UpdateCameraTransform();
            UpdateCameraSize();
            
            TrackDimensionChanges();
        }

        // ============================================================================
        // PRIVATE METHODS: CAMERA UPDATE LOGIC
        // ============================================================================

        /// <summary>
        /// Checks if camera can be updated (grid exists, camera valid).
        /// </summary>
        private bool CanUpdateCamera()
        {
            if (Grid == null)
            {
                // Suppress log spam - only warn once
                if (Time.frameCount % 300 == 0) // Log every 300 frames (~5 seconds at 60fps)
                    Debug.LogWarning("[CameraController] Grid reference is null. Assign in Inspector.");
                return false;
            }

            if (mainCamera == null)
            {
                Debug.LogError("[CameraController] Camera component lost at runtime!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates camera transform to center on grid.
        /// Formula: center = (Width/2, Height/2, cameraDepth)
        /// </summary>
        private void UpdateCameraTransform()
        {
            Vector3 gridCenter = CalculateGridCenter();
            transform.position = gridCenter;
        }

        /// <summary>
        /// Calculates world-space center position of grid.
        /// Accounts for CellSize to correctly position in world units.
        /// </summary>
        private Vector3 CalculateGridCenter()
        {
            float centerX = (Grid.Width * Grid.CellSize) / 2f;
            float centerY = (Grid.Height * Grid.CellSize) / 2f;
            
            return new Vector3(centerX, centerY, cameraDepth);
        }

        /// <summary>
        /// Updates orthographic camera size to frame entire grid.
        /// Formula: size = (max(Width, Height) * CellSize / 2) * (1 + padding) * sizeFactor
        /// </summary>
        private void UpdateCameraSize()
        {
            if (!mainCamera.orthographic)
            {
                Debug.LogWarning("[CameraController] Camera switched to perspective mode at runtime. Re-enabling orthographic.");
                mainCamera.orthographic = true;
            }

            mainCamera.orthographicSize = CalculateOrthographicSize();
        }

        /// <summary>
        /// Calculates required orthographic size to fit grid.
        /// Accounts for cell size, padding, and user-specified zoom factor.
        /// </summary>
        private float CalculateOrthographicSize()
        {
            float maxDimension = Mathf.Max(Grid.Width, Grid.Height);
            float baseSize = (maxDimension * Grid.CellSize) / 2f;
            float paddedSize = baseSize * (1f + edgePadding);
            
            return paddedSize * sizeFactor;
        }

        /// <summary>
        /// Tracks grid dimension changes for optimization and debugging.
        /// </summary>
        private void TrackDimensionChanges()
        {
            Vector2Int currentDimensions = new Vector2Int(Grid.Width, Grid.Height);
            
            if (currentDimensions != lastGridDimensions)
            {
                Debug.Log($"[CameraController] Grid dimensions changed: {lastGridDimensions} ? {currentDimensions}");
                lastGridDimensions = currentDimensions;
            }
        }

        // ============================================================================
        // PUBLIC API: MANUAL CONTROL
        // ============================================================================

        /// <summary>
        /// Manually forces camera update (useful for editor scripting).
        /// </summary>
        public void ForceUpdate()
        {
            if (CanUpdateCamera())
            {
                UpdateCameraTransform();
                UpdateCameraSize();
            }
        }

        /// <summary>
        /// Gets current camera framing bounds in world space.
        /// Useful for debugging or UI positioning.
        /// </summary>
        public Bounds GetViewBounds()
        {
            if (mainCamera == null || Grid == null)
                return new Bounds(Vector3.zero, Vector3.zero);

            float height = mainCamera.orthographicSize * 2f;
            float width = height * mainCamera.aspect;

            return new Bounds(
                CalculateGridCenter(),
                new Vector3(width, height, 0f)
            );
        }

        // ============================================================================
        // PUBLIC API: DIAGNOSTIC QUERIES
        // ============================================================================

        /// <summary>
        /// Gets formatted camera info string for debugging.
        /// </summary>
        public override string ToString()
        {
            if (Grid == null || mainCamera == null)
                return "CameraController[Invalid]";

            return $"CameraController[Grid={Grid.Width}×{Grid.Height}, " +
                   $"Size={mainCamera.orthographicSize:F1}, " +
                   $"Pos={transform.position}, " +
                   $"Depth={cameraDepth}]";
        }

        // ============================================================================
        // UNITY EDITOR: GIZMOS
        // ============================================================================

#if UNITY_EDITOR
        /// <summary>
        /// Draws debug visualization in Scene view (Editor only).
        /// Shows camera frustum and grid bounds.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (Grid == null || mainCamera == null)
                return;

            DrawGridBounds();
            DrawCameraFrustum();
        }

        /// <summary>
        /// Draws grid boundary rectangle in Scene view.
        /// </summary>
        private void DrawGridBounds()
        {
            Gizmos.color = Color.yellow;
            
            Vector3 center = CalculateGridCenter();
            Vector3 size = new Vector3(
                Grid.Width * Grid.CellSize,
                Grid.Height * Grid.CellSize,
                0.1f
            );
            
            Gizmos.DrawWireCube(center, size);
        }

        /// <summary>
        /// Draws camera view frustum in Scene view.
        /// </summary>
        private void DrawCameraFrustum()
        {
            Gizmos.color = Color.cyan;
            
            Bounds viewBounds = GetViewBounds();
            Gizmos.DrawWireCube(viewBounds.center, viewBounds.size);
        }
#endif
    }
}