using System;
using System.Collections.Generic;
using Viable.Contracts;

namespace Viable.Engine
{
    /// <summary>
    /// Topology provider for grid-based simulations.
    /// Stage 13.5: Enables different boundary conditions (absorbing, periodic, etc.)
    /// Stage 13.6: Supports different neighborhood types (VonNeumann4, Moore8)
    /// Stage 13.7: Supports grid topology (Rectangular, Triangular, Hexagonal) for proper neighbor connectivity.
    /// </summary>
    public static class TopologyProvider
    {
        /// <summary>
        /// Get neighbors based on grid topology, diffusion mode, and boundary handling.
        /// Stage 13.7: Now topology-aware - uses 4 neighbors for Rectangular, 6 for Triangular/Hexagonal.
        /// </summary>
        public static void GetNeighbors(
            int x, int y, int width, int height,
            DiffusionMode diffusionMode,
            BoundaryMode boundaryMode,
            out int[] nx, out int[] ny, out int count,
            bool[] domainMask = null,
            TopologyMode topology = TopologyMode.RectGrid)  // ADDED: Grid topology parameter
        {
            // Stage 13.7: If topology is Triangular or Hexagonal, override diffusion mode
            // These grids always have 6 neighbors regardless of diffusion mode
            if (topology == TopologyMode.TriGrid || topology == TopologyMode.HexGrid)
            {
                GetNeighborsTopology(x, y, width, height, topology, boundaryMode, out nx, out ny, out count, domainMask);
                return;
            }

            // Original behavior for Rectangular grid
            switch (diffusionMode)
            {
                case DiffusionMode.VonNeumann4:
                    GetNeighbors4(x, y, width, height, boundaryMode, out nx, out ny, out count, domainMask);
                    break;

                case DiffusionMode.Moore8:
                    GetNeighbors8(x, y, width, height, boundaryMode, out nx, out ny, out count, domainMask);
                    break;

                default:
                    // Default to VonNeumann4 (current behavior)
                    GetNeighbors4(x, y, width, height, boundaryMode, out nx, out ny, out count, domainMask);
                    break;
            }
        }

        /// <summary>
        /// Get neighbors based on grid topology (Triangular or Hexagonal).
        /// Stage 13.7: Handles offset coordinates for triangular and hexagonal grids.
        /// </summary>
        private static void GetNeighborsTopology(
            int x, int y, int width, int height,
            TopologyMode topology,
            BoundaryMode boundaryMode,
            out int[] nx, out int[] ny, out int count,
            bool[] domainMask = null)
        {
            // Allocate arrays for up to 6 neighbors (triangular/hexagonal)
            nx = new int[6];
            ny = new int[6];
            count = 0;

            // Get topology-specific offsets
            int[] dx, dy;
            NeighborProvider.GetNeighborOffsets(x, y, topology, out dx, out dy, AdjacencyMode.EdgeOnly); // ADDED: adjacency parameter

            for (int d = 0; d < dx.Length; d++)
            {
                int candidateX = x + dx[d];
                int candidateY = y + dy[d];

                // Apply boundary conditions
                switch (boundaryMode)
                {
                    case BoundaryMode.Absorbing:
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;

                    case BoundaryMode.PeriodicWrap:
                        candidateX = (candidateX + width) % width;
                        candidateY = (candidateY + height) % height;
                        break;

                    case BoundaryMode.Reflecting:
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;

                    default:
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;
                }

                // Check domain mask
                if (domainMask != null)
                {
                    if (!MaskGenerator.IsCellValid(candidateX, candidateY, width, domainMask))
                        continue;
                }

                nx[count] = candidateX;
                ny[count] = candidateY;
                count++;
            }
        }

        /// <summary>
        /// Get 4-way neighbors (Von Neumann) with boundary handling.
        /// Stage 13.8: Optionally respects domain mask.
        /// </summary>
        public static void GetNeighbors4(
            int x, int y, int width, int height,
            BoundaryMode boundaryMode,
            out int[] nx, out int[] ny, out int count,
            bool[] domainMask = null)  // Stage 13.8
        {
            // Allocate arrays for up to 4 neighbors
            nx = new int[4];
            ny = new int[4];
            count = 0;

            // Offsets: up, down, left, right
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int d = 0; d < 4; d++)
            {
                int candidateX = x + dx[d];
                int candidateY = y + dy[d];

                switch (boundaryMode)
                {
                    case BoundaryMode.Absorbing:
                        // Default: out-of-bounds neighbors are skipped
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;

                    case BoundaryMode.PeriodicWrap:
                        // Stage 13.5: Wrap coordinates (toroidal topology)
                        candidateX = (candidateX + width) % width;
                        candidateY = (candidateY + height) % height;
                        break;

                    case BoundaryMode.Reflecting:
                        // Future: Mirror coordinates at boundaries
                        // For now, treat as absorbing
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;

                    default:
                        // Unknown mode: treat as absorbing
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;
                }

                // Stage 13.8: Check domain mask
                if (domainMask != null)
                {
                    if (!MaskGenerator.IsCellValid(candidateX, candidateY, width, domainMask))
                        continue;  // Skip masked-out cells
                }

                nx[count] = candidateX;
                ny[count] = candidateY;
                count++;
            }
        }

        /// <summary>
        /// Get 8-way neighbors (Moore) with boundary handling.
        /// Stage 13.6: Includes diagonal neighbors for smoother diffusion.
        /// Stage 13.8: Optionally respects domain mask.
        /// </summary>
        public static void GetNeighbors8(
            int x, int y, int width, int height,
            BoundaryMode boundaryMode,
            out int[] nx, out int[] ny, out int count,
            bool[] domainMask = null)  // Stage 13.8
        {
            // Allocate arrays for up to 8 neighbors
            nx = new int[8];
            ny = new int[8];
            count = 0;

            // Offsets: N, S, W, E, NW, NE, SW, SE
            int[] dx = { 0, 0, -1, 1, -1, 1, -1, 1 };
            int[] dy = { -1, 1, 0, 0, -1, -1, 1, 1 };

            for (int d = 0; d < 8; d++)
            {
                int candidateX = x + dx[d];
                int candidateY = y + dy[d];

                switch (boundaryMode)
                {
                    case BoundaryMode.Absorbing:
                        // Default: out-of-bounds neighbors are skipped
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;

                    case BoundaryMode.PeriodicWrap:
                        // Wrap coordinates (toroidal topology)
                        candidateX = (candidateX + width) % width;
                        candidateY = (candidateY + height) % height;
                        break;

                    case BoundaryMode.Reflecting:
                        // Future: Mirror coordinates at boundaries
                        // For now, treat as absorbing
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;

                    default:
                        // Unknown mode: treat as absorbing
                        if (candidateX < 0 || candidateX >= width || candidateY < 0 || candidateY >= height)
                            continue;
                        break;
                }

                // Stage 13.8: Check domain mask
                if (domainMask != null)
                {
                    if (!MaskGenerator.IsCellValid(candidateX, candidateY, width, domainMask))
                        continue;  // Skip masked-out cells
                }

                nx[count] = candidateX;
                ny[count] = candidateY;
                count++;
            }
        }

        /// <summary>
        /// Check if a cell is valid within the topology.
        /// For periodic wrap, all cells within grid dimensions are valid.
        /// </summary>
        public static bool IsCellValid(int x, int y, int width, int height, BoundaryMode boundaryMode)
        {
            switch (boundaryMode)
            {
                case BoundaryMode.PeriodicWrap:
                    // All cells within grid are valid (wrap handles out-of-bounds)
                    return x >= 0 && x < width && y >= 0 && y < height;

                case BoundaryMode.Absorbing:
                case BoundaryMode.Reflecting:
                default:
                    // Standard bounds check
                    return x >= 0 && x < width && y >= 0 && y < height;
            }
        }
    }
}
