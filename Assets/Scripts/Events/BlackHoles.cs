using Assets.Scripts.Domain;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public static class BlackHoles
    {
        public static float LastDrained { get; private set; }

        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

        private static int FindRoot(int[] parent, int id)
        {
            if (id <= 0) return 0;
            while (parent[id] != id)
            {
                parent[id] = parent[parent[id]];
                id = parent[id];
            }
            return id;
        }

        private static int Union(int[] parent, float[] mass, int a, int b)
        {
            int ra = FindRoot(parent, a);
            int rb = FindRoot(parent, b);
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
        public static void GrowBlackHoles(GridState s)
        {
            // Next-state copy (simple, but allocates each tick)
            bool[] next = (bool[])s.IsBlackHole.Clone();

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);
                    if (!s.IsBlackHole[i]) continue;

                    // For each neighbor of this black hole:
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                            continue;

                        int ni = s.Idx(nx, ny);
                        if (s.IsBlackHole[ni]) continue;

                        // Count black-hole neighbors around ni
                        int count = 0;
                        for (int dd = 0; dd < 4; dd++)
                        {
                            int nnx = nx + dx[dd];
                            int nny = ny + dy[dd];
                            if (nnx < 0 || nnx >= s.W || nny < 0 || nny >= s.H)
                                continue;

                            int nni = s.Idx(nnx, nny);
                            if (s.IsBlackHole[nni]) count++;
                        }

                        // Convert if adjacent to >=2 black holes
                        if (count >= 2)
                        {
                            next[ni] = true;
                            if (!next[ni])
                                Debug.LogError($"BH formation failed at {ni} charge={s.BlackHoleCharge[ni]}");
                        }
                    }
                }
            }

            s.IsBlackHole = next;
        }
        public static int AssignOrMergeAtCell(int idx, int width, int height,
            bool[] isBlackHole, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass, ref int nextBlackHoleId)
        {
            if (isBlackHole[idx])
            {
                int existingRoot = FindRoot(blackHoleParent, blackHoleId[idx]);
                Debug.LogError($"BH overwrite attempt at {idx} root={existingRoot} mass={blackHoleMass[existingRoot]}");
                return existingRoot;
            }

            int x = idx % width;
            int y = idx / width;
            int chosenRoot = 0;

            // Prefer an existing neighbour id if present
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                int nIdx = ny * width + nx;
                if (!isBlackHole[nIdx]) continue;

                int nRoot = FindRoot(blackHoleParent, blackHoleId[nIdx]);
                if (chosenRoot == 0) chosenRoot = nRoot;
                else chosenRoot = Union(blackHoleParent, blackHoleMass, chosenRoot, nRoot);
            }

            if (chosenRoot == 0)
            {
                chosenRoot = nextBlackHoleId++;
                blackHoleParent[chosenRoot] = chosenRoot;
            }

            isBlackHole[idx] = true;
            blackHoleId[idx] = chosenRoot;
            if (!isBlackHole[idx])
                Debug.LogError($"BH formation failed at {idx} mass={blackHoleMass[chosenRoot]}");
            return chosenRoot;
        }

        public static float BlackHoleAttractEnergy(GridState s, float absorbFracPerTick = 0.5f, bool fieldOnly = false)
        {
            absorbFracPerTick = Mathf.Clamp01(absorbFracPerTick);
            float drainedTotal = 0f;

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);
                    if (!s.IsBlackHole[i]) continue;

                    // Enforce dead state
                    s.Nlocal[i] = 0f;
                    s.Entropy[i] = 1f;
                    s.Active[i] = 0;

                    int root = FindRoot(s.BlackHoleParent, s.BlackHoleId[i]);

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                            continue;

                        int ni = s.Idx(nx, ny);
                        if (s.IsBlackHole[ni]) continue;
                        if (fieldOnly && !s.FieldPresent[ni]) continue;

                        float neighbourEnergy = s.Nlocal[ni];
                        if (neighbourEnergy <= 0f) continue;

                        float absorbed = neighbourEnergy * absorbFracPerTick;
                        s.Nlocal[ni] = neighbourEnergy - absorbed;
                        drainedTotal += absorbed;
                        if (root > 0)
                            s.BlackHoleMass[root] += absorbed;
                    }
                }
            }

            LastDrained = drainedTotal;
            return drainedTotal;
        }
    }
}
