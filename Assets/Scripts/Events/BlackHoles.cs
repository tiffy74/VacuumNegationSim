using Assets.Scripts.Domain;
using System;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public static class BlackHoles
    {
        public static float LastDrained { get; private set; }
        public static float LastDrainedThisTick { get; private set; }
        public static int LastDrainedEdges { get; private set; }
        public static float LastRecoilThisTick { get; private set; }

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

        public static void ComputePotential(GridState s, int radius, float scale)
        {
            // Clear potential
            Array.Clear(s.BlackHolePotential, 0, s.BlackHolePotential.Length);
            if (radius <= 0 || scale <= 0f) return;

            // We treat each BH cell's root mass as the "source strength"
            // and spread it out with a simple 1/(1+dist) kernel (Manhattan dist).
            for (int by = 0; by < s.H; by++)
            {
                for (int bx = 0; bx < s.W; bx++)
                {
                    int bIdx = s.Idx(bx, by);
                    if (!s.IsBlackHole[bIdx]) continue;

                    int root = GetRootAtCell(bIdx, s.BlackHoleId, s.BlackHoleParent);
                    float mass = 1f;
                    if (root > 0 && root < s.BlackHoleMass.Length)
                        mass = Mathf.Max(1f, s.BlackHoleMass[root]);

                    // Spread influence within radius
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int y = by + dy;
                        if (y < 0 || y >= s.H) continue;

                        int rem = radius - Mathf.Abs(dy);
                        for (int dx = -rem; dx <= rem; dx++)
                        {
                            int x = bx + dx;
                            if (x < 0 || x >= s.W) continue;

                            int dist = Mathf.Abs(dx) + Mathf.Abs(dy);
                            float contrib = (mass / (1f + dist)) * scale;
                            s.BlackHolePotential[s.Idx(x, y)] += contrib;
                        }
                    }
                }
            }
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
                            //if (!next[ni])
                            //    Debug.LogError($"BH formation failed at {ni} charge={s.BlackHoleCharge[ni]}");
                        }
                    }
                }
            }

            s.IsBlackHole = next;

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    // If this is a BH cell without an ID (newly grown), assign/merge it
                    if (s.IsBlackHole[i] && s.BlackHoleId[i] == 0)
                    {
                        AssignOrMergeAtCell(i, s.W, s.H,
                            s.IsBlackHole, s.BlackHoleId, s.BlackHoleParent, s.BlackHoleMass,
                            ref s.NextBlackHoleId);
                    }
                }
            }
        }
        public static int Find(int id, int[] parent)
        {
            if (id <= 0) return 0;
            while (parent[id] != id)
            {
                parent[id] = parent[parent[id]];
                id = parent[id];
            }
            return id;
        }

        public static int GetRootAtCell(int idx, int[] blackHoleId, int[] blackHoleParent)
        {
            int id = (idx >= 0 && idx < blackHoleId.Length) ? blackHoleId[idx] : 0;
            return Find(id, blackHoleParent);
        }

        public static int AssignOrMergeAtCell(int idx, int width, int height,
            bool[] isBlackHole, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass, ref int nextBlackHoleId)
        {
            int EnsureIdForCell(int idx, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass, ref int nextBlackHoleId)
            {
                if (blackHoleId[idx] > 0)
                {
                    return Find(blackHoleId[idx], blackHoleParent);
                }

                int newId = nextBlackHoleId;
                nextBlackHoleId++;
                blackHoleParent[newId] = newId;
                blackHoleMass[newId] = blackHoleMass[newId]; // no-op but explicit
                blackHoleId[idx] = newId;
                return newId;
            }

            int x = idx % width;
            int y = idx / width;

            // If already BH, ensure it has an id and merge with adjacent BHs
            if (isBlackHole[idx])
            {
                int root = EnsureIdForCell(idx, blackHoleId, blackHoleParent, blackHoleMass, ref nextBlackHoleId);

                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    int nIdx = ny * width + nx;
                    if (!isBlackHole[nIdx]) continue;

                    int nRoot = Find(blackHoleId[nIdx], blackHoleParent);
                    if (nRoot == 0) continue;
                    root = Union(blackHoleParent, blackHoleMass, root, nRoot);
                }

                blackHoleId[idx] = root;
                return root;
            }

            // Not a BH yet: create and merge with adjacent BHs if any
            int chosenRoot = 0;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                int nIdx = ny * width + nx;
                if (!isBlackHole[nIdx]) continue;

                int nRoot = Find(blackHoleId[nIdx], blackHoleParent);
                if (nRoot == 0) continue;

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
            return chosenRoot;
        }

        public static float BlackHoleAttractEnergy(GridState s, float absorbFracPerTick = 0f, bool fieldOnly = false, float blackHoleRecoilFrac = 0f)
        {
            absorbFracPerTick = Mathf.Clamp01(absorbFracPerTick); 
            float drainedTotal = 0f;
            int drainedEdges = 0;
            float recoilTotal = 0f;

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);
                    if (!s.IsBlackHole[i]) continue;

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
                        drainedEdges++;
                        if (root > 0)
                            s.BlackHoleMass[root] += absorbed;
                    }
                }
            }

            if (blackHoleRecoilFrac > 0f && drainedTotal > 0f)
            {
                float recoilShare = drainedTotal * blackHoleRecoilFrac / 4f;
                for (int y = 0; y < s.H; y++)
                {
                    for (int x = 0; x < s.W; x++)
                    {
                        int i = s.Idx(x, y);
                        if (!s.IsBlackHole[i]) continue;

                        for (int d = 0; d < 4; d++)
                        {
                            int nx = x + dx[d];
                            int ny = y + dy[d];
                            if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                                continue;

                            int ni = s.Idx(nx, ny);
                            if (s.IsBlackHole[ni]) continue;
                            if (!s.FieldPresent[ni]) continue;

                            s.Incoming[ni] += recoilShare;
                            recoilTotal += recoilShare;
                        }
                    }
                }
            }

            LastDrainedThisTick = drainedTotal;
            LastDrainedEdges = drainedEdges;
            LastDrained = drainedTotal;
            LastRecoilThisTick = recoilTotal;
            return drainedTotal;
        }
    }
}
