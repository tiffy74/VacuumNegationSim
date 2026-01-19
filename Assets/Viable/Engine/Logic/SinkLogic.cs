using System;

namespace Viable.Engine.Logic
{
    /// <summary>
    /// Pure logic for sink region management using Union-Find algorithm.
    /// Extracted from Assets/Scripts/Events/SinkRegions.cs (Phase 4).
    /// </summary>
    public static class SinkLogic
    {
        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

        /// <summary>
        /// Find the root ID of a sink using path compression.
        /// </summary>
        private static int Find(int id, int[] parent)
        {
            if (id <= 0) return 0;
            while (parent[id] != id)
            {
                // Path compression: attach to grandparent
                parent[id] = parent[parent[id]];
                id = parent[id];
            }
            return id;
        }

        /// <summary>
        /// Get the root sink ID for a given cell index.
        /// </summary>
        /// <param name="cellIdx">Cell index in flattened array</param>
        /// <param name="sinkId">SinkId array</param>
        /// <param name="sinkParent">SinkParent union-find array</param>
        /// <returns>Root sink ID (0 if not a sink)</returns>
        public static int GetRootAtCell(int cellIdx, int[] sinkId, int[] sinkParent)
        {
            int id = (cellIdx >= 0 && cellIdx < sinkId.Length) ? sinkId[cellIdx] : 0;
            return Find(id, sinkParent);
        }

        /// <summary>
        /// Assign or merge a sink at the given cell index.
        /// If the cell is already a sink, merges with adjacent sinks.
        /// If the cell is not yet a sink, creates a new sink and merges with adjacent sinks.
        /// </summary>
        /// <param name="cellIdx">Cell index in flattened array</param>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="isSink">IsSink boolean array</param>
        /// <param name="sinkId">SinkId array</param>
        /// <param name="sinkParent">SinkParent union-find array</param>
        /// <param name="sinkMass">SinkMass array</param>
        /// <param name="nextSinkId">Next available sink ID (incremented by ref)</param>
        /// <returns>Root sink ID after merging</returns>
        public static int AssignOrMergeAtCell(
            int cellIdx, int width, int height,
            bool[] isSink, int[] sinkId, int[] sinkParent, float[] sinkMass, ref int nextSinkId)
        {
            int x = cellIdx % width;
            int y = cellIdx / width;

            // If already sink, ensure it has an id and merge with adjacent sinks
            if (isSink[cellIdx])
            {
                int root = EnsureIdForCell(cellIdx, sinkId, sinkParent, sinkMass, ref nextSinkId);

                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    int nIdx = ny * width + nx;
                    if (!isSink[nIdx]) continue;

                    int nRoot = Find(sinkId[nIdx], sinkParent);
                    if (nRoot == 0) continue;
                    root = Union(sinkParent, sinkMass, root, nRoot);
                }

                sinkId[cellIdx] = root;
                return root;
            }

            // Not a sink yet: create and merge with adjacent sinks if any
            int chosenRoot = 0;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                int nIdx = ny * width + nx;
                if (!isSink[nIdx]) continue;

                int nRoot = Find(sinkId[nIdx], sinkParent);
                if (nRoot == 0) continue;

                if (chosenRoot == 0) chosenRoot = nRoot;
                else chosenRoot = Union(sinkParent, sinkMass, chosenRoot, nRoot);
            }

            if (chosenRoot == 0)
            {
                chosenRoot = nextSinkId++;
                sinkParent[chosenRoot] = chosenRoot;
            }

            isSink[cellIdx] = true;
            sinkId[cellIdx] = chosenRoot;
            return chosenRoot;
        }

        /// <summary>
        /// Ensure a cell has a sink ID, creating one if needed.
        /// </summary>
        private static int EnsureIdForCell(
            int cellIdx, int[] sinkId, int[] sinkParent, float[] sinkMass, ref int nextSinkId)
        {
            if (sinkId[cellIdx] > 0)
            {
                return Find(sinkId[cellIdx], sinkParent);
            }

            int newId = nextSinkId;
            nextSinkId++;
            sinkParent[newId] = newId;
            // sinkMass[newId] remains as-is (default 0 or pre-allocated)
            sinkId[cellIdx] = newId;
            return newId;
        }

        /// <summary>
        /// Union two sink IDs, merging their masses and attaching higher ID to lower ID for stability.
        /// </summary>
        private static int Union(int[] parent, float[] mass, int a, int b)
        {
            int ra = Find(a, parent);
            int rb = Find(b, parent);
            if (ra == 0) return rb;
            if (rb == 0) return ra;
            if (ra == rb) return ra;

            // Attach higher id to lower id for stability
            if (rb < ra)
            {
                int tmp = ra;
                ra = rb;
                rb = tmp;
            }

            parent[rb] = ra;
            mass[ra] += mass[rb];
            mass[rb] = 0f;
            return ra;
        }
    }
}