using Viable.Contracts;
using System;

namespace Viable.Engine
{
    /// <summary>
    /// Provides neighbor offsets for different grid topologies.
    /// Stage 13.7: Support for Rectangular, Triangular, and Hexagonal grids.
    /// Stage 13.9: Support for both edge-only and edge+vertex adjacency.
    /// </summary>
    public static class NeighborProvider
    {
        /// <summary>
        /// Get neighbor count for topology and adjacency mode.
        /// </summary>
        public static int GetNeighborCount(TopologyMode topology, AdjacencyMode adjacency = AdjacencyMode.EdgeOnly)
        {
            switch (topology)
            {
                case TopologyMode.RectGrid:
                    return adjacency == AdjacencyMode.EdgeOnly ? 4 : 8; // Von Neumann 4 or Moore 8

                case TopologyMode.TriGrid:
                    return adjacency == AdjacencyMode.EdgeOnly ? 3 : 6; // Edge-only or edge+vertex

                case TopologyMode.HexGrid:
                    return 6; // Hexagon always has 6 (same for both modes)

                default:
                    return 4; // Fallback to rectangular edge-only
            }
        }

        /// <summary>
        /// Get neighbor offsets for cell at (x, y) with given topology and adjacency mode.
        /// Returns arrays of dx, dy offsets.
        /// </summary>
        public static void GetNeighborOffsets(
            int x, int y, 
            TopologyMode topology, 
            out int[] dx, 
            out int[] dy,
            AdjacencyMode adjacency = AdjacencyMode.EdgeOnly)
        {
            switch (topology)
            {
                case TopologyMode.RectGrid:
                    if (adjacency == AdjacencyMode.EdgeOnly)
                    {
                        // Von Neumann 4-neighborhood (edge-adjacent only)
                        dx = new int[] { 0, 0, -1, 1 };
                        dy = new int[] { -1, 1, 0, 0 };
                    }
                    else
                    {
                        // Moore 8-neighborhood (edge + corner adjacent)
                        dx = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
                        dy = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };
                    }
                    break;

                case TopologyMode.TriGrid:
                    if (adjacency == AdjacencyMode.EdgeOnly)
                    {
                        // Edge-only: 3 neighbors (the 3 triangles sharing an edge)
                        // Triangular grid alternates up/down triangles
                        // For simplicity, we determine orientation by (x+y) parity
                        bool isUpTriangle = (x + y) % 2 == 0;
                        
                        if (isUpTriangle)
                        {
                            // Up-pointing triangle (?): shares edges with 3 down-triangles
                            // Left, Right, Bottom
                            dx = new int[] { -1, 1, 0 };
                            dy = new int[] { 0, 0, 1 };
                        }
                        else
                        {
                            // Down-pointing triangle (?): shares edges with 3 up-triangles
                            // Left, Right, Top
                            dx = new int[] { -1, 1, 0 };
                            dy = new int[] { 0, 0, -1 };
                        }
                    }
                    else
                    {
                        // Edge + Vertex: 6 neighbors (hexagonal vertex lattice)
                        // Offset coordinates depend on row parity
                        if (y % 2 == 0)
                        {
                            // Even row
                            dx = new int[] { -1, 0, 1, 1, 0, -1 };
                            dy = new int[] { -1, -1, -1, 0, 1, 1 };
                        }
                        else
                        {
                            // Odd row
                            dx = new int[] { -1, 0, 1, 1, 0, -1 };
                            dy = new int[] { 0, 0, 0, 1, 1, 1 };
                        }
                    }
                    break;

                case TopologyMode.HexGrid:
                    // Hexagonal grid (flat-top) - always 6 edge neighbors
                    // Offset coordinates depend on row parity
                    if (y % 2 == 0)
                    {
                        // Even row
                        dx = new int[] { -1, 0, 1, 0, -1, 1 };
                        dy = new int[] { 0, -1, 0, 1, 1, 1 };
                    }
                    else
                    {
                        // Odd row
                        dx = new int[] { -1, 0, 1, 0, 1, -1 };
                        dy = new int[] { -1, -1, -1, 0, 0, 0 };
                    }
                    break;

                default:
                    // Fallback to rectangular edge-only
                    dx = new int[] { 0, 0, -1, 1 };
                    dy = new int[] { -1, 1, 0, 0 };
                    break;
            }
        }

        /// <summary>
        /// Iterate over neighbors of cell (x, y) and call action for each valid neighbor.
        /// </summary>
        public static void ForEachNeighbor(
            int x, int y, int gridWidth, int gridHeight,
            TopologyMode topology,
            Action<int, int> action,
            AdjacencyMode adjacency = AdjacencyMode.EdgeOnly)
        {
            GetNeighborOffsets(x, y, topology, out int[] dx, out int[] dy, adjacency);

            for (int i = 0; i < dx.Length; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                // Bounds check
                if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight)
                    continue;

                action(nx, ny);
            }
        }

        /// <summary>
        /// Count neighbors of cell (x, y) that satisfy a condition.
        /// </summary>
        public static int CountNeighbors(
            int x, int y, int gridWidth, int gridHeight,
            TopologyMode topology,
            Func<int, int, bool> predicate,
            AdjacencyMode adjacency = AdjacencyMode.EdgeOnly)
        {
            int count = 0;
            ForEachNeighbor(x, y, gridWidth, gridHeight, topology, (nx, ny) =>
            {
                if (predicate(nx, ny))
                    count++;
            }, adjacency);
            return count;
        }
    }
}
