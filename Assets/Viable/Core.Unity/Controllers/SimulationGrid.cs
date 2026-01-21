using UnityEngine;

namespace Viable.Core.Unity.Controllers
{
    /// <summary>
    /// Manages the Unity visual grid (cell GameObjects).
    /// Spawns and positions cell visualizers based on grid dimensions.
    /// Stage 8: Moved from Assets.Scripts.Core to Viable.Core.Unity.Controllers.
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

        /// <summary>
        /// Spawns visual cells as GameObjects and initializes their CellVisualiser components.
        /// </summary>
        /// <param name="views">2D array to populate with CellVisualiser references</param>
        public void SpawnVisualCells(Visuals.CellVisualiser[,] views)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Vector3 pos = new Vector3(x * CellSize, y * CellSize, 0f);
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

                    views[x, y] = visualiser;
                    visualiser.Initialize(Color.black);
                }
            }
            
            Debug.Log($"[SimulationGrid] Spawned {Width}×{Height} cells. Grid world size: {Width * CellSize:F2}×{Height * CellSize:F2} units");
        }
    }
}
