using UnityEngine;

public class CameraController : MonoBehaviour
{
    public SimulationGrid Grid;

    void LateUpdate()
    {
        if (Grid == null) return;

        // Calculate the actual world size of the grid  
        float worldWidth = Grid.Width * Grid.CellSize;
        float worldHeight = Grid.Height * Grid.CellSize;

        // Center the camera on the grid in world space
        Vector3 center = new Vector3(worldWidth / 2f, worldHeight / 2f, -10f);
        transform.position = center;

        Camera cam = GetComponent<Camera>();
        if (cam.orthographic)
        {
            // Set orthographic size to fit the entire grid with a small margin
            float maxDimension = Mathf.Max(worldWidth, worldHeight);
            cam.orthographicSize = (maxDimension / 2f); // 1.1f adds 10% margin
        }
    }
}