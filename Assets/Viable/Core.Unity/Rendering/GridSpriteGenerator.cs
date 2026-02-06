using UnityEngine;

namespace Viable.Core.Unity.Rendering
{
    /// <summary>
    /// Procedurally generates sprites for different grid topologies.
    /// Stage 13.6: Support for Rectangular, Triangular, and Hexagonal tessellations.
    /// </summary>
    public static class GridSpriteGenerator
    {
        /// <summary>
        /// Generate a square sprite (current default).
        /// Used for Rectangular topology.
        /// </summary>
        public static Sprite GenerateSquareSprite(int resolution = 64)
        {
            Texture2D texture = new Texture2D(resolution, resolution);
            Color[] pixels = new Color[resolution * resolution];
            
            // Fill with white (will be tinted by cell color)
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(
                texture,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f), // Pivot at center
                resolution
            );
        }
        
        /// <summary>
        /// Generate an equilateral triangle sprite.
        /// Pointing UP for triangular tessellation.
        /// Used for Triangular topology.
        /// </summary>
        public static Sprite GenerateTriangleSprite(int resolution = 64)
        {
            Texture2D texture = new Texture2D(resolution, resolution);
            Color[] pixels = new Color[resolution * resolution];
            
            // Initialize to transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;
            
            // Draw equilateral triangle
            float height = resolution * 0.866f; // sqrt(3)/2 for equilateral
            
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // Convert to centered coordinates
                    float px = x - resolution / 2f;
                    float py = y - resolution / 4f; // Shift up 1/4 for better centering
                    
                    // Equilateral triangle bounds
                    // Top vertex: (0, height/2)
                    // Bottom-left: (-width/2, -height/2)
                    // Bottom-right: (width/2, -height/2)
                    
                    bool insideTriangle = 
                        py <= height / 2f && // Below top vertex
                        py >= -height / 2f && // Above bottom edge
                        Mathf.Abs(px) <= (height / 2f - py) / Mathf.Sqrt(3); // Within angled sides
                    
                    if (insideTriangle)
                        pixels[y * resolution + x] = Color.white;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(
                texture,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f), // Pivot at center
                resolution
            );
        }
        
        /// <summary>
        /// Generate a regular hexagon sprite.
        /// Flat-top orientation (common for hexagonal grids).
        /// Used for Hexagonal topology.
        /// </summary>
        public static Sprite GenerateHexagonSprite(int resolution = 64)
        {
            Texture2D texture = new Texture2D(resolution, resolution);
            Color[] pixels = new Color[resolution * resolution];
            
            // Initialize to transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;
            
            // Draw regular hexagon (flat-top)
            float radius = resolution / 2f - 2; // Padding
            float centerX = resolution / 2f;
            float centerY = resolution / 2f;
            
            // Hexagon vertices (flat-top orientation)
            Vector2[] hexVertices = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.PI / 3f * i; // 60° increments
                hexVertices[i] = new Vector2(
                    centerX + radius * Mathf.Cos(angle),
                    centerY + radius * Mathf.Sin(angle)
                );
            }
            
            // Fill hexagon using point-in-polygon test
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    if (IsPointInPolygon(new Vector2(x, y), hexVertices))
                        pixels[y * resolution + x] = Color.white;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(
                texture,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f), // Pivot at center
                resolution
            );
        }
        
        /// <summary>
        /// Point-in-polygon test (ray casting algorithm).
        /// Determines if a point is inside a polygon defined by vertices.
        /// </summary>
        private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            int j = polygon.Length - 1;
            
            for (int i = 0; i < polygon.Length; i++)
            {
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / 
                               (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
                j = i;
            }
            
            return inside;
        }
    }
}
