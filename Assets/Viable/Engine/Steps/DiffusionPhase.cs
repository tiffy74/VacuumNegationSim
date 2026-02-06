using System;
using Viable.Contracts;

namespace Viable.Engine.Steps
{
    /// <summary>
    /// Phase 4: Diffuse complexity metric across the grid using Laplacian operator.
    /// Stage 13.5: Supports periodic wrap boundary conditions.
    /// Stage 13.6: Supports different diffusion neighborhoods (VonNeumann4, Moore8).
    /// </summary>
    public static class DiffusionPhase
    {
        /// <summary>
        /// Diffuses complexity metric across the grid using a simple Laplacian, then applies decay and clamps.
        /// Stage 13.5: Supports BoundaryMode for periodic wrap.
        /// Stage 13.6: Supports DiffusionMode for different neighborhood sizes.
        /// Stage 13.7: Supports TopologyMode for proper neighbor connectivity (4 vs 6 neighbors).
        /// Stage 13.8: Supports domain masks for constrained propagation.
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="ComplexityMetric">Current complexity array</param>
        /// <param name="complexityNext">Buffer for next complexity state</param>
        /// <param name="ComplexityDiffusionRate">Diffusion rate</param>
        /// <param name="ComplexityDecay">Complexity decay per tick</param>
        /// <param name="IsSink">Sink mask (sinks have fixed complexity = 1.0)</param>
        /// <param name="boundaryMode">Boundary condition mode (default: Absorbing)</param>
        /// <param name="diffusionMode">Diffusion neighborhood mode (default: VonNeumann4)</param>
        /// <param name="domainMask">Optional domain mask (Stage 13.8)</param>
        /// <param name="topology">Grid topology mode (Stage 13.7 - default: RectGrid)</param>
        public static void ComplexityDiffuse(
            int width, int height,
            float[] ComplexityMetric, float[] complexityNext,
            float ComplexityDiffusionRate, float ComplexityDecay,
            bool[] IsSink,
            BoundaryMode boundaryMode = BoundaryMode.Absorbing,
            DiffusionMode diffusionMode = DiffusionMode.VonNeumann4,
            bool[] domainMask = null,
            TopologyMode topology = TopologyMode.RectGrid)  // ADDED: topology parameter
        {
            int Idx(int x, int y) => y * width + x;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    // Stage 13.8: Skip masked-out cells
                    if (domainMask != null && !MaskGenerator.IsCellValid(x, y, width, domainMask))
                    {
                        complexityNext[i] = 0f;  // Masked cells have no complexity
                        continue;
                    }

                    if (IsSink[i])
                    {
                        complexityNext[i] = 1f;
                        continue;
                    }

                    float c = ComplexityMetric[i];

                    // Stage 13.6/13.7: Get neighbors based on diffusion mode and topology
                    int[] nx, ny;
                    int neighborCount;
                    TopologyProvider.GetNeighbors(x, y, width, height, diffusionMode, boundaryMode, out nx, out ny, out neighborCount, domainMask, topology);

                    // Compute Laplacian: sum(neighbors) - N*center
                    float neighborSum = 0f;
                    int validNeighbors = 0;

                    for (int n = 0; n < neighborCount; n++)
                    {
                        int nIdx = Idx(nx[n], ny[n]);
                        if (!IsSink[nIdx])
                        {
                            neighborSum += ComplexityMetric[nIdx];
                            validNeighbors++;
                        }
                        else
                        {
                            // Sinks act as barriers (don't contribute to diffusion)
                            neighborSum += c; // Use center value instead
                            validNeighbors++;
                        }
                    }

                    // Laplacian = (sum - N*center)
                    float lap = (neighborSum - validNeighbors * c);
                    float diffused = c + ComplexityDiffusionRate * lap;
                    diffused = Math.Max(0f, diffused - ComplexityDecay);
                    complexityNext[i] = Math.Clamp(diffused, 0f, 1f);
                }
            }

            // Commit next state to current
            for (int i = 0; i < complexityNext.Length; i++)
                ComplexityMetric[i] = complexityNext[i];
        }
    }
}
