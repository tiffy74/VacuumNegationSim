using System.Collections.Generic;
using UnityEngine;

public class SimulationGrid : MonoBehaviour
{
    public int Width = 1200;
    public int Height = 1200;
    public GameObject CellPrefab;
    public float CellSize = 0.1f;
    public Cell[,] Grid;
    private List<Vector2Int> activeCells = new List<Vector2Int>();

    private void Awake()
    {
        Grid = new Cell[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Grid[x, y] = new Cell(new Vector2Int(x, y));
            }
        }

        AssignNeighbors();
        SpawnVisualCells();
    }

    private void SpawnVisualCells()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var worldPos = new Vector3(x * CellSize, y * CellSize, 0);
                GameObject cellObj = Instantiate(CellPrefab, worldPos, Quaternion.identity, this.transform);
                cellObj.name = $"Cell_{x}_{y}";

                var visual = cellObj.GetComponent<CellVisualiser>();
                visual?.Initialize(Color.gray); // initial vacuum color

                // Connect logic to visuals + init state
                var cell = Grid[x, y];
                cell.GridPosition = new Vector2Int(x, y);
                cell.IsVacuum = true;
                cell.Visualiser = visual;
                cell.BaseEntropy = 0f;     // keep flat for now (we’ll add dynamic later)
                cell.EntropyDyn = 0f;

                if ((x == Width / 2 && y == Height / 2) && Time.frameCount == 0)
                    Debug.Log($"[GRID] Wired visualiser for center at {x},{y}");
            }
        }
    }


    private void AssignNeighbors()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var neighbors = new System.Collections.Generic.List<Cell>();

                Vector2Int[] directions = {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1)
                };


                foreach (var dir in directions)
                {
                    var neighbor = GetCell(new Vector2Int(x, y) + dir);
                    if (neighbor != null)
                        neighbors.Add(neighbor);
                }

                Grid[x, y].Neighbors = neighbors.ToArray();
            }
        }
    }

    public Cell GetCell(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= Width || pos.y >= Height)
            return null;

        return Grid[pos.x, pos.y];
    }
}
