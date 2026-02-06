using UnityEngine;
using Viable.Contracts; // ADDED: For TopologyMode enum
using Viable.Core.Unity.Rendering; // ADDED: For GridSpriteGenerator

namespace Viable.Core.Unity.Controllers
{
    /// <summary>
    /// Manages the Unity visual grid (cell GameObjects).
    /// Spawns and positions cell visualizers based on grid dimensions.
    /// Stage 8: Moved from Assets.Scripts.Core to Viable.Core.Unity.Controllers.
    /// Stage 13.7: Added support for 3 tessellations (Rectangular, Triangular, Hexagonal).
    /// </summary>
    public class SimulationGrid : MonoBehaviour
    {
        [Tooltip("Grid width in cells")]
        public int Width = 128;

        [Tooltip("Grid height in cells")]
        public int Height = 128;

        [Tooltip("Prefab for individual cell visualization")]
        public GameObject CellPrefab;

        [Tooltip("World size of each cell (Unity units)")]
        public float CellSize = 0.01f;

        // ADDED: Cached sprites for 3 tessellations
        private Sprite rectangularSprite;
        private Sprite triangularSprite;
        private Sprite hexagonalSprite;

        /// <summary>
        /// Spawns visual cells as GameObjects and initializes their CellVisualiser components.
        /// Stage 13.7: Now supports topology-aware sprite switching and positioning.
        /// </summary>
        /// <param name="views">2D array to populate with CellVisualiser references</param>
        /// <param name="topology">Grid topology mode (Rectangular, Triangular, Hexagonal)</param>
        public void SpawnVisualCells(Visuals.CellVisualiser[,] views, TopologyMode topology = TopologyMode.RectGrid)
        {
            // Generate sprites if not already cached
            if (rectangularSprite == null)
                rectangularSprite = GridSpriteGenerator.GenerateSquareSprite();
            if (triangularSprite == null)
                triangularSprite = GridSpriteGenerator.GenerateTriangleSprite();
            if (hexagonalSprite == null)
                hexagonalSprite = GridSpriteGenerator.GenerateHexagonSprite();

            Debug.Log($"[SimulationGrid] Spawning {Width}×{Height} cells with topology: {topology}");

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Get position based on topology
                    Vector3 pos = GetCellPosition(x, y, topology);
                    
                    GameObject cellGO = Instantiate(CellPrefab, pos, Quaternion.identity, transform);
                    cellGO.name = $"Cell_{x}_{y}";
                    
                    // Scale the cell to match CellSize
                    cellGO.transform.localScale = new Vector3(CellSize, CellSize, 1f);
                    
                    var visualiser = cellGO.GetComponent<Visuals.CellVisualiser>();
                    if (visualiser == null)
                    {
                        Debug.LogError($"Cell prefab missing CellVisualiser component at ({x},{y})!");
                        continue;
                    }

                    // ADDED: Set sprite based on topology
                    var renderer = cellGO.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.sprite = GetSpriteForTopology(topology);
                    }
                    else
                    {
                        Debug.LogError($"Cell prefab missing SpriteRenderer component at ({x},{y})!");
                    }

                    views[x, y] = visualiser;
                    visualiser.Initialize(Color.black);
                }
            }
            
            Debug.Log($"[SimulationGrid] Spawned {Width}×{Height} cells. Grid world size: {GetGridWorldSize(topology)}");
        }

        /// <summary>
        /// Clear all visual cells (destroy GameObjects).
        /// Called before respawning cells with new topology/size.
        /// </summary>
        public void ClearAllCells()
        {
            Debug.Log("[SimulationGrid] Clearing all visual cells...");
            
            // Destroy all child GameObjects (all cell instances)
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            
            Debug.Log($"[SimulationGrid] Cleared {childCount} visual cells");
        }

        /// <summary>
        /// Get appropriate sprite for current topology.
        /// </summary>
        private Sprite GetSpriteForTopology(TopologyMode topology)
        {
            switch (topology)
            {
                case TopologyMode.RectGrid:
                    return rectangularSprite;
                case TopologyMode.TriGrid:
                    return triangularSprite;
                case TopologyMode.HexGrid:
                    return hexagonalSprite;
                default:
                    Debug.LogWarning($"[SimulationGrid] Unknown topology: {topology}, defaulting to Rectangular");
                    return rectangularSprite;
            }
        }

        /// <summary>
        /// Calculate cell position based on topology.
        /// Different tessellations have different spacing patterns.
        /// </summary>
        private Vector3 GetCellPosition(int x, int y, TopologyMode topology)
        {
            switch (topology)
            {
                case TopologyMode.RectGrid:
                    return GetRectangularPosition(x, y);
                
                case TopologyMode.TriGrid:
                    return GetTriangularPosition(x, y);
                
                case TopologyMode.HexGrid:
                    return GetHexagonalPosition(x, y);
                
                default:
                    return GetRectangularPosition(x, y);
            }
        }

        /// <summary>
        /// Rectangular grid: Simple 1:1 spacing.
        /// </summary>
        private Vector3 GetRectangularPosition(int x, int y)
        {
            return new Vector3(x * CellSize, y * CellSize, 0f);
        }

        /// <summary>
        /// Triangular grid: Offset every other row.
        /// Uses "flat-side-down" orientation with alternating offset.
        /// </summary>
        private Vector3 GetTriangularPosition(int x, int y)
        {
            float cellWidth = CellSize;
            float cellHeight = CellSize * 0.866f; // sqrt(3)/2 for equilateral triangles
            
            // Offset every other row
            float xOffset = (y % 2 == 0) ? 0 : cellWidth * 0.5f;
            
            return new Vector3(
                x * cellWidth + xOffset,
                y * cellHeight,
                0f
            );
        }

        /// <summary>
        /// Hexagonal grid: Flat-top orientation with offset rows.
        /// Uses axial coordinate system.
        /// </summary>
        private Vector3 GetHexagonalPosition(int x, int y)
        {
            // Flat-top hexagon dimensions
            float hexWidth = CellSize;
            float hexHeight = CellSize * 0.866f; // sqrt(3)/2
            
            // Offset every other row
            float xOffset = (y % 2 == 0) ? 0 : hexWidth * 0.5f;
            
            return new Vector3(
                x * hexWidth * 0.75f + xOffset, // 0.75 = 3/4 for flat-top hex spacing
                y * hexHeight,
                0f
            );
        }

        /// <summary>
        /// Calculate grid world size based on topology.
        /// Useful for camera framing.
        /// </summary>
        private string GetGridWorldSize(TopologyMode topology)
        {
            Vector3 minPos = GetCellPosition(0, 0, topology);
            Vector3 maxPos = GetCellPosition(Width - 1, Height - 1, topology);
            
            float worldWidth = Mathf.Abs(maxPos.x - minPos.x) + CellSize;
            float worldHeight = Mathf.Abs(maxPos.y - minPos.y) + CellSize;
            
            return $"{worldWidth:F2}×{worldHeight:F2} units";
        }
    }
}
