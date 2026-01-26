using System;
using Viable.Contracts;

namespace Viable.Engine
{
    /// <summary>
    /// Generates domain masks for constrained propagation.
    /// Stage 13.8: Implements various mask shapes (Circle, Ring, etc.)
    /// </summary>
    public static class MaskGenerator
    {
        /// <summary>
        /// Generate a mask array based on MaskShape and parameters.
        /// Stage 13.8: Supports Circle, Ring, Rectangle (no mask), and future shapes.
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="maskShape">Shape type</param>
        /// <param name="maskRadius">Outer radius for Circle/Ring</param>
        /// <param name="maskInnerRadius">Inner radius for Ring</param>
        /// <param name="corridorWidth">Width for Corridor shape</param>
        /// <param name="holeProbability">Probability of holes for PercolationHoles</param>
        /// <param name="seed">Random seed for deterministic hole generation</param>
        /// <returns>Boolean array where true = cell is valid (inside mask)</returns>
        public static bool[] GenerateMask(
            int width,
            int height,
            MaskShape maskShape,
            double maskRadius = 0.0,
            double maskInnerRadius = 0.0,
            double corridorWidth = 0.0,
            double holeProbability = 0.0,
            int seed = 0)
        {
            bool[] mask = new bool[width * height];
            int Idx(int x, int y) => y * width + x;

            // Default center (can be parameterized later if needed)
            double centerX = width / 2.0;
            double centerY = height / 2.0;

            switch (maskShape)
            {
                case MaskShape.Rectangle:
                    // No mask - all cells valid (default behavior)
                    for (int i = 0; i < mask.Length; i++)
                        mask[i] = true;
                    break;

                case MaskShape.Circle:
                    // Circular mask: distance from center <= radius
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double dx = x - centerX;
                            double dy = y - centerY;
                            double distance = Math.Sqrt(dx * dx + dy * dy);
                            mask[Idx(x, y)] = distance <= maskRadius;
                        }
                    }
                    break;

                case MaskShape.Ring:
                    // Ring mask: innerRadius < distance <= outerRadius
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double dx = x - centerX;
                            double dy = y - centerY;
                            double distance = Math.Sqrt(dx * dx + dy * dy);
                            mask[Idx(x, y)] = distance > maskInnerRadius && distance <= maskRadius;
                        }
                    }
                    break;

                case MaskShape.Corridor:
                    // Vertical corridor: |x - centerX| <= corridorWidth/2
                    double halfWidth = corridorWidth / 2.0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double distanceFromCenter = Math.Abs(x - centerX);
                            mask[Idx(x, y)] = distanceFromCenter <= halfWidth;
                        }
                    }
                    break;

                case MaskShape.PercolationHoles:
                    // Start with all valid, then punch deterministic holes
                    var rng = new Random(seed);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // Each cell has holeProbability chance of being invalid
                            mask[Idx(x, y)] = rng.NextDouble() >= holeProbability;
                        }
                    }
                    break;

                default:
                    // Unknown shape: default to Rectangle (no mask)
                    for (int i = 0; i < mask.Length; i++)
                        mask[i] = true;
                    break;
            }

            return mask;
        }

        /// <summary>
        /// Check if a cell is valid based on mask.
        /// Stage 13.8: Used by topology provider to skip masked-out cells.
        /// </summary>
        public static bool IsCellValid(int x, int y, int width, bool[] mask)
        {
            if (mask == null) return true;  // No mask = all valid
            if (x < 0 || x >= width || y < 0 || y >= mask.Length / width) return false;
            
            int idx = y * width + x;
            return mask[idx];
        }
    }
}
