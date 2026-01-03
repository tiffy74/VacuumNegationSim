using UnityEngine;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Simulation Grid - Unity GameObject Factory for Grid Cells
    /// 
    /// Manages grid dimensions and spawns visual cell GameObjects in a grid layout.
    /// Acts as the bridge between Unity's scene hierarchy and the simulation's
    /// flat array representation (GridState).
    /// 
    /// Design Philosophy:
    /// - Single Responsibility: Only handles grid GameObject instantiation
    /// - Minimal Logic: No simulation logic, just visual cell creation
    /// - Clean Architecture: No legacy Cell[,] array - fully migrated to GridState
    /// - Inspector-Driven: Grid dimensions configurable in Unity Inspector
    /// 
    /// Lifecycle:
    /// 1. Awake(): Reserved for future initialization (currently empty)
    /// 2. SpawnVisualCells(): Called by SimulationController to create visual grid
    /// 3. Runtime: No further updates (visual cells managed by GridRenderer)
    /// 
    /// Migration Status:
    /// - COMPLETED: Migrated from Cell[,] Grid to GridState
    /// - All simulation state now managed by GridState
    /// - Visual representation managed by CellVisualiser[,]
    /// </summary>
    public class SimulationGrid : MonoBehaviour
    {
        // ============================================================================
        // INSPECTOR PARAMETERS: GRID CONFIGURATION
        // ============================================================================

        /// <summary>
        /// Grid width (number of columns).
        /// Determines horizontal extent of simulation space.
        /// Typical Values: 64 (small), 100 (default), 200 (large)
        /// Performance Impact: O(Width × Height) per tick
        /// </summary>
        [Header("Grid Dimensions")]
        [Tooltip("Number of columns in the grid")]
        [Range(10, 500)]
        public int Width = 100;

        /// <summary>
        /// Grid height (number of rows).
        /// Determines vertical extent of simulation space.
        /// Typical Values: 64 (small), 100 (default), 200 (large)
        /// Performance Impact: O(Width × Height) per tick
        /// </summary>
        [Tooltip("Number of rows in the grid")]
        [Range(10, 500)]
        public int Height = 100;

        /// <summary>
        /// Prefab template for individual cell GameObjects.
        /// Requirements: Must have SpriteRenderer component
        /// CellVisualiser component will be added automatically if missing
        /// </summary>
        [Header("Cell Prefab")]
        [Tooltip("Prefab for individual grid cells (must have SpriteRenderer)")]
        public GameObject CellPrefab;

        /// <summary>
        /// Visual spacing between cells (world space units).
        /// Controls grid density and overall grid size.
        /// Formula: GridWorldSize = Width × CellSize × Height × CellSize
        /// Typical Value: 0.1 (tightly packed) to 1.0 (spaced)
        /// </summary>
        [Tooltip("Size of each cell in world space")]
        [Range(0.01f, 1.0f)]
        public float CellSize = 0.1f;

        // ============================================================================
        // PUBLIC API: VISUAL GRID SPAWNING
        // ============================================================================

        /// <summary>
        /// Spawns visual cell GameObjects in grid layout.
        /// 
        /// Called by SimulationController.Start() to create visual representation.
        /// Each cell is instantiated from CellPrefab and positioned in world space.
        /// 
        /// Process:
        /// 1. Iterate through grid positions (x, y)
        /// 2. Calculate world position from grid coordinates
        /// 3. Instantiate prefab at position
        /// 4. Get/Add CellVisualiser component
        /// 5. Store reference in views[x, y] array
        /// 6. Initialize visualiser with default color
        /// 
        /// Performance: O(Width × Height) - called once at startup
        /// </summary>
        /// <param name="views">2D array to store CellVisualiser references (output parameter)</param>
        public void SpawnVisualCells(CellVisualiser[,] views)
        {
            ValidateViewsArray(views);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SpawnSingleVisualCell(x, y, views);
                }
            }
        }

        // ============================================================================
        // PRIVATE METHODS: VISUAL CELL SPAWNING
        // ============================================================================

        /// <summary>
        /// Validates that views array has correct dimensions.
        /// </summary>
        private void ValidateViewsArray(CellVisualiser[,] views)
        {
            if (views == null)
                throw new System.ArgumentNullException(nameof(views), "Views array cannot be null");

            if (views.GetLength(0) != Width || views.GetLength(1) != Height)
            {
                throw new System.ArgumentException(
                    $"Views array dimensions ({views.GetLength(0)}×{views.GetLength(1)}) " +
                    $"do not match grid dimensions ({Width}×{Height})",
                    nameof(views));
            }
        }

        /// <summary>
        /// Spawns a single visual cell GameObject at specified grid position.
        /// </summary>
        private void SpawnSingleVisualCell(int x, int y, CellVisualiser[,] views)
        {
            Vector3 worldPosition = CalculateWorldPosition(x, y);
            GameObject cellGO = InstantiateCellGameObject(x, y, worldPosition);
            CellVisualiser visualiser = GetOrAddCellVisualiser(cellGO);
            
            views[x, y] = visualiser;
            InitializeCellVisualiser(visualiser);
        }

        /// <summary>
        /// Converts grid coordinates to world space position.
        /// Formula: worldPos = (x * CellSize, y * CellSize, 0)
        /// </summary>
        private Vector3 CalculateWorldPosition(int x, int y)
        {
            return new Vector3(x * CellSize, y * CellSize, 0f);
        }

        /// <summary>
        /// Instantiates cell GameObject from prefab.
        /// </summary>
        private GameObject InstantiateCellGameObject(int x, int y, Vector3 position)
        {
            if (CellPrefab == null)
            {
                Debug.LogError("[SimulationGrid] CellPrefab is not assigned in Inspector!");
                return null;
            }

            GameObject cellGO = Instantiate(CellPrefab, position, Quaternion.identity, transform);
            cellGO.name = $"Cell_{x}_{y}";
            return cellGO;
        }

        /// <summary>
        /// Gets existing CellVisualiser component or adds one if missing.
        /// </summary>
        private CellVisualiser GetOrAddCellVisualiser(GameObject cellGO)
        {
            if (cellGO == null)
                return null;

            var visualiser = cellGO.GetComponent<CellVisualiser>();
            
            if (visualiser == null)
            {
                visualiser = cellGO.AddComponent<CellVisualiser>();
                Debug.LogWarning($"[SimulationGrid] CellVisualiser missing on {cellGO.name}, added automatically");
            }

            return visualiser;
        }

        /// <summary>
        /// Initializes cell visualiser with default black color.
        /// </summary>
        private void InitializeCellVisualiser(CellVisualiser visualiser)
        {
            if (visualiser != null)
            {
                visualiser.Initialize(Color.black);
            }
        }

        // ============================================================================
        // PRIVATE METHODS: VALIDATION
        // ============================================================================

        /// <summary>
        /// Checks if grid position is within bounds.
        /// </summary>
        private bool IsValidGridPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < Width && pos.y < Height;
        }

        // ============================================================================
        // PUBLIC METHODS: DIAGNOSTIC QUERIES
        // ============================================================================

        /// <summary>
        /// Gets total cell count (Width × Height).
        /// </summary>
        public int GetTotalCellCount() => Width * Height;

        /// <summary>
        /// Gets grid world size in Unity units.
        /// </summary>
        public Vector2 GetWorldSize() => new Vector2(Width * CellSize, Height * CellSize);

        /// <summary>
        /// Gets formatted grid info string for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"SimulationGrid[{Width}×{Height}, CellSize={CellSize}, Total={GetTotalCellCount()}]";
        }
    }
}