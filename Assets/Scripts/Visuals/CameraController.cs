using UnityEngine;

public class CameraController : MonoBehaviour
{
    public SimulationGrid Grid;

    void LateUpdate()
    {
        if (Grid == null) return;

        Vector3 center = new Vector3(Grid.Width / 2f, Grid.Height / 2f, -10f);
        transform.position = center;

        Camera cam = GetComponent<Camera>();
        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Max(Grid.Width, Grid.Height) / 2f;
        }
    }
}