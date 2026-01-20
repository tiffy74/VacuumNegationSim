using UnityEngine;

public class SimulationGrid : MonoBehaviour
{
    public int Width = 64;
    public int Height = 64;
    public GameObject CellPrefab;
    public float CellSize = 0.1f;

    public void SpawnVisualCells(CellVisualiser[,] views)
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
                
                var visualiser = cellGO.GetComponent<CellVisualiser>();
                views[x, y] = visualiser;
                visualiser.Initialize(Color.black);
            }
        }
        
        Debug.Log($"Spawned {Width}x{Height} cells. Grid world size: {Width * CellSize}x{Height * CellSize} units");
    }
}