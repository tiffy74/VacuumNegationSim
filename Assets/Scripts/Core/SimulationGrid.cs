using System.Collections.Generic;
using UnityEngine;

public class SimulationGrid : MonoBehaviour
{
    public int Width = 64;
    public int Height = 64;
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
        // SpawnVisualCells();
    }

    public void SpawnVisualCells(CellVisualiser[,] views)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 pos = new Vector3(x * CellSize, y * CellSize, 0f);
                GameObject cellGO = Instantiate(CellPrefab, pos, Quaternion.identity, transform);
                cellGO.name = $"Cell_{x}_{y}";
                var visualiser = cellGO.GetComponent<CellVisualiser>();
                views[x, y] = visualiser;
                visualiser.Initialize(Color.red); // or any default color
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
