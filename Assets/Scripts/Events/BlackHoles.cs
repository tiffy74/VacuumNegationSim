using Assets.Scripts.Domain;
using System;
using UnityEngine;

namespace Assets.Scripts.Events
{
    /// <summary>
    /// Manages black hole formation, growth, merging, and energy interactions.
    /// Black holes represent collapsed configuration space where energy cannot persist.
    /// Uses union-find data structure to track merged black hole entities.
    /// </summary>
    public static class BlackHoles
    {
        // ============================================================================
        // PUBLIC DIAGNOSTICS
        // ============================================================================
        
        public static float LastDrained { get; private set; }
        public static float LastDrainedThisTick { get; private set; }
        public static int LastDrainedEdges { get; private set; }
        public static float LastRecoilThisTick { get; private set; }

        // ============================================================================
        // PRIVATE CONSTANTS
        // ============================================================================
        
        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

        // ============================================================================
        // PUBLIC API: BLACK HOLE GROWTH & MERGING
        // ============================================================================

        /// <summary>
        /// Expands black hole regions when cells have 2+ BH neighbors.
        /// Then assigns/merges IDs for newly created BH cells.
        /// Implements geometric collapse criterion: trapped surfaces form when
        /// a cell is surrounded by existing collapsed regions.
        /// </summary>
        public static void GrowBlackHoles(GridState s)
        {
            bool[] nextBlackHoleState = IdentifyNewBlackHoleCells(s);
            s.IsBlackHole = nextBlackHoleState;
            AssignIdsToNewlyGrownBlackHoles(s);
        }

        /// <summary>
        /// Assigns a union-find ID to a cell becoming a black hole.
        /// Automatically merges with adjacent black holes to maintain
        /// the property that touching BH cells belong to the same entity.
        /// </summary>
        /// <returns>The root ID of the (possibly merged) black hole entity</returns>
        public static int AssignOrMergeAtCell(int idx, int width, int height,
            bool[] isBlackHole, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass, ref int nextBlackHoleId)
        {
            if (isBlackHole[idx])
                return MergeExistingBlackHoleCell(idx, width, height, isBlackHole, blackHoleId, blackHoleParent, blackHoleMass, ref nextBlackHoleId);
            else
                return CreateNewBlackHoleCell(idx, width, height, isBlackHole, blackHoleId, blackHoleParent, blackHoleMass, ref nextBlackHoleId);
        }

        // ============================================================================
        // PUBLIC API: ENERGY INTERACTIONS
        // ============================================================================

        /// <summary>
        /// Drains energy from cells adjacent to black holes.
        /// Energy absorbed increases the black hole's mass (analogous to accretion).
        /// Optional recoil effect radiates a fraction of drained energy back out.
        /// </summary>
        /// <returns>Total energy drained this tick</returns>
        public static float BlackHoleAttractEnergy(GridState s, float absorbFracPerTick = 0f, bool fieldOnly = false, float blackHoleRecoilFrac = 0f)
        {
            absorbFracPerTick = Mathf.Clamp01(absorbFracPerTick);
            
            float drainedTotal = DrainEnergyFromBlackHoleBoundaries(s, absorbFracPerTick, fieldOnly);
            float recoilTotal = ApplyEnergyRecoilEffect(s, drainedTotal, blackHoleRecoilFrac);

            UpdateDiagnostics(drainedTotal, recoilTotal);
            return drainedTotal;
        }

        /// <summary>
        /// Computes gravitational potential field around black holes.
        /// Potential falls off as 1/(1+distance) from each BH cell,
        /// weighted by the black hole's total mass.
        /// </summary>
        public static void ComputePotential(GridState s, int radius, float scale)
        {
            ClearPotentialField(s);
            if (radius <= 0 || scale <= 0f) return;

            SpreadBlackHoleInfluence(s, radius, scale);
        }

        // ============================================================================
        // PUBLIC API: UNION-FIND OPERATIONS
        // ============================================================================

        /// <summary>
        /// Finds the root ID of a black hole entity (union-find path compression).
        /// </summary>
        public static int Find(int id, int[] parent)
        {
            if (id <= 0) return 0;
            
            // Path compression: flatten tree during traversal
            while (parent[id] != id)
            {
                parent[id] = parent[parent[id]];
                id = parent[id];
            }
            return id;
        }

        /// <summary>
        /// Gets the root black hole ID for a specific cell.
        /// </summary>
        public static int GetRootAtCell(int idx, int[] blackHoleId, int[] blackHoleParent)
        {
            int id = (idx >= 0 && idx < blackHoleId.Length) ? blackHoleId[idx] : 0;
            return Find(id, blackHoleParent);
        }

        // ============================================================================
        // PRIVATE: BLACK HOLE GROWTH LOGIC
        // ============================================================================

        /// <summary>
        /// Determines which cells should become black holes based on neighbor count.
        /// A cell converts to BH if it has 2+ BH neighbors (geometric collapse criterion).
        /// </summary>
        private static bool[] IdentifyNewBlackHoleCells(GridState s)
        {
            bool[] next = (bool[])s.IsBlackHole.Clone();

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);
                    if (!s.IsBlackHole[i]) continue;

                    ExpandBlackHoleToNeighbors(s, x, y, next);
                }
            }

            return next;
        }

        /// <summary>
        /// Checks each neighbor of a BH cell and marks it for conversion if trapped.
        /// </summary>
        private static void ExpandBlackHoleToNeighbors(GridState s, int x, int y, bool[] next)
        {
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, s.W, s.H)) continue;

                int ni = s.Idx(nx, ny);
                if (s.IsBlackHole[ni]) continue;

                int bhNeighborCount = CountBlackHoleNeighbors(s, nx, ny);
                
                // Geometric collapse: cell is trapped if 2+ neighbors are collapsed
                if (bhNeighborCount >= 2)
                {
                    next[ni] = true;
                }
            }
        }

        /// <summary>
        /// Counts how many of a cell's 4 neighbors are black holes.
        /// </summary>
        private static int CountBlackHoleNeighbors(GridState s, int x, int y)
        {
            int count = 0;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, s.W, s.H)) continue;

                if (s.IsBlackHole[s.Idx(nx, ny)])
                    count++;
            }
            return count;
        }

        /// <summary>
        /// After growing black holes, assigns union-find IDs to newly created BH cells
        /// and merges them with adjacent black holes.
        /// </summary>
        private static void AssignIdsToNewlyGrownBlackHoles(GridState s)
        {
            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    // Newly grown BH cells have IsBlackHole=true but BlackHoleId=0
                    if (s.IsBlackHole[i] && s.BlackHoleId[i] == 0)
                    {
                        AssignOrMergeAtCell(i, s.W, s.H,
                            s.IsBlackHole, s.BlackHoleId, s.BlackHoleParent, s.BlackHoleMass,
                            ref s.NextBlackHoleId);
                    }
                }
            }
        }

        // ============================================================================
        // PRIVATE: BLACK HOLE CREATION & MERGING
        // ============================================================================

        /// <summary>
        /// Handles merging when a cell that's already a BH needs ID assignment/update.
        /// Merges with all adjacent BH cells to maintain connected component invariant.
        /// </summary>
        private static int MergeExistingBlackHoleCell(int idx, int width, int height,
            bool[] isBlackHole, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass, ref int nextBlackHoleId)
        {
            int x = idx % width;
            int y = idx / width;

            int root = EnsureBlackHoleCellHasId(idx, blackHoleId, blackHoleParent, blackHoleMass, ref nextBlackHoleId);
            root = MergeWithAdjacentBlackHoles(idx, x, y, width, height, isBlackHole, blackHoleId, blackHoleParent, blackHoleMass, root);

            blackHoleId[idx] = root;
            return root;
        }

        /// <summary>
        /// Creates a new black hole at a cell and merges it with any adjacent BHs.
        /// </summary>
        private static int CreateNewBlackHoleCell(int idx, int width, int height,
            bool[] isBlackHole, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass, ref int nextBlackHoleId)
        {
            int x = idx % width;
            int y = idx / width;

            int chosenRoot = FindAdjacentBlackHoleToMergeWith(idx, x, y, width, height, isBlackHole, blackHoleId, blackHoleParent, blackHoleMass);

            if (chosenRoot == 0)
            {
                chosenRoot = CreateNewBlackHoleEntity(blackHoleParent, ref nextBlackHoleId);
            }

            isBlackHole[idx] = true;
            blackHoleId[idx] = chosenRoot;
            return chosenRoot;
        }

        /// <summary>
        /// Ensures a black hole cell has a valid union-find ID.
        /// If it already has one, returns its root. Otherwise, creates new ID.
        /// </summary>
        private static int EnsureBlackHoleCellHasId(int idx, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass, ref int nextBlackHoleId)
        {
            if (blackHoleId[idx] > 0)
            {
                return Find(blackHoleId[idx], blackHoleParent);
            }

            int newId = nextBlackHoleId++;
            blackHoleParent[newId] = newId;
            blackHoleMass[newId] = 1f; // Initial mass
            blackHoleId[idx] = newId;
            return newId;
        }

        /// <summary>
        /// Merges a black hole cell with all adjacent black holes.
        /// </summary>
        private static int MergeWithAdjacentBlackHoles(int idx, int x, int y, int width, int height,
            bool[] isBlackHole, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass, int root)
        {
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, width, height)) continue;

                int nIdx = ny * width + nx;
                if (!isBlackHole[nIdx]) continue;

                int nRoot = Find(blackHoleId[nIdx], blackHoleParent);
                if (nRoot == 0) continue;

                root = UnionByMass(blackHoleParent, blackHoleMass, root, nRoot);
            }

            return root;
        }

        /// <summary>
        /// Searches for an adjacent black hole to merge with.
        /// Returns the merged root ID, or 0 if no adjacent BHs found.
        /// </summary>
        private static int FindAdjacentBlackHoleToMergeWith(int idx, int x, int y, int width, int height,
            bool[] isBlackHole, int[] blackHoleId, int[] blackHoleParent, float[] blackHoleMass)
        {
            int chosenRoot = 0;

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, width, height)) continue;

                int nIdx = ny * width + nx;
                if (!isBlackHole[nIdx]) continue;

                int nRoot = Find(blackHoleId[nIdx], blackHoleParent);
                if (nRoot == 0) continue;

                if (chosenRoot == 0)
                    chosenRoot = nRoot;
                else
                    chosenRoot = UnionByMass(blackHoleParent, blackHoleMass, chosenRoot, nRoot);
            }

            return chosenRoot;
        }

        /// <summary>
        /// Creates a new black hole entity with a fresh ID.
        /// </summary>
        private static int CreateNewBlackHoleEntity(int[] blackHoleParent, ref int nextBlackHoleId)
        {
            int newId = nextBlackHoleId++;
            blackHoleParent[newId] = newId;
            return newId;
        }

        // ============================================================================
        // PRIVATE: UNION-FIND OPERATIONS
        // ============================================================================

        /// <summary>
        /// Union-find operation with path compression.
        /// </summary>
        private static int FindRoot(int[] parent, int id)
        {
            if (id <= 0) return 0;
            
            while (parent[id] != id)
            {
                parent[id] = parent[parent[id]]; // Path compression
                id = parent[id];
            }
            return id;
        }

        /// <summary>
        /// Merges two black hole entities, combining their masses.
        /// Attaches higher ID to lower ID for stability.
        /// </summary>
        private static int UnionByMass(int[] parent, float[] mass, int a, int b)
        {
            int ra = FindRoot(parent, a);
            int rb = FindRoot(parent, b);
            if (ra == 0) return rb;
            if (rb == 0) return ra;
            if (ra == rb) return ra;

            // Attach higher ID to lower ID for deterministic behavior
            if (rb < ra)
            {
                int tmp = ra;
                ra = rb;
                rb = tmp;
            }

            parent[rb] = ra;
            mass[ra] += mass[rb]; // Combine masses (conservation of mass)
            mass[rb] = 0f;
            return ra;
        }

        // ============================================================================
        // PRIVATE: ENERGY DRAIN & RECOIL
        // ============================================================================

        /// <summary>
        /// Drains energy from cells adjacent to black holes.
        /// Energy is removed from neighbors and added to BH mass.
        /// </summary>
        private static float DrainEnergyFromBlackHoleBoundaries(GridState s, float absorbFracPerTick, bool fieldOnly)
        {
            float drainedTotal = 0f;
            int drainedEdges = 0;

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);
                    if (!s.IsBlackHole[i]) continue;

                    EnforceBlackHoleInvariants(s, i);
                    drainedTotal += DrainEnergyFromNeighbors(s, i, x, y, absorbFracPerTick, fieldOnly, ref drainedEdges);
                }
            }

            LastDrainedEdges = drainedEdges;
            return drainedTotal;
        }

        /// <summary>
        /// Enforces that black hole cells have zero energy, max entropy, inactive state.
        /// </summary>
        private static void EnforceBlackHoleInvariants(GridState s, int idx)
        {
            s.Nlocal[idx] = 0f;
            s.Entropy[idx] = 1f; // Maximum entropy (total collapse)
            s.Active[idx] = 0;
        }

        /// <summary>
        /// Drains energy from all valid neighbors of a black hole cell.
        /// </summary>
        private static float DrainEnergyFromNeighbors(GridState s, int bhIdx, int x, int y, float absorbFrac, bool fieldOnly, ref int drainedEdges)
        {
            int root = FindRoot(s.BlackHoleParent, s.BlackHoleId[bhIdx]);
            float totalDrained = 0f;

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, s.W, s.H)) continue;

                int ni = s.Idx(nx, ny);
                if (s.IsBlackHole[ni]) continue;
                if (fieldOnly && !s.FieldPresent[ni]) continue;

                float neighborEnergy = s.Nlocal[ni];
                if (neighborEnergy <= 0f) continue;

                float absorbed = neighborEnergy * absorbFrac;
                s.Nlocal[ni] = neighborEnergy - absorbed;
                totalDrained += absorbed;
                drainedEdges++;

                if (root > 0)
                    s.BlackHoleMass[root] += absorbed; // Accretion: mass increases
            }

            return totalDrained;
        }

        /// <summary>
        /// Applies energy recoil effect (Hawking-like radiation).
        /// A fraction of drained energy is radiated back to cells adjacent to BH boundaries.
        /// </summary>
        private static float ApplyEnergyRecoilEffect(GridState s, float drainedTotal, float blackHoleRecoilFrac)
        {
            if (blackHoleRecoilFrac <= 0f || drainedTotal <= 0f)
                return 0f;

            float recoilTotal = 0f;
            float recoilShare = drainedTotal * blackHoleRecoilFrac / 4f; // Split among 4 directions

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);
                    if (!s.IsBlackHole[i]) continue;

                    recoilTotal += RadiateEnergyToNeighbors(s, i, x, y, recoilShare);
                }
            }

            return recoilTotal;
        }

        /// <summary>
        /// Radiates energy to all valid neighbors of a black hole (recoil effect).
        /// </summary>
        private static float RadiateEnergyToNeighbors(GridState s, int bhIdx, int x, int y, float recoilShare)
        {
            float totalRadiated = 0f;

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, s.W, s.H)) continue;

                int ni = s.Idx(nx, ny);
                if (s.IsBlackHole[ni]) continue;
                if (!s.FieldPresent[ni]) continue;

                s.Incoming[ni] += recoilShare;
                totalRadiated += recoilShare;
            }

            return totalRadiated;
        }

        // ============================================================================
        // PRIVATE: POTENTIAL FIELD
        // ============================================================================

        /// <summary>
        /// Clears the gravitational potential field.
        /// </summary>
        private static void ClearPotentialField(GridState s)
        {
            Array.Clear(s.BlackHolePotential, 0, s.BlackHolePotential.Length);
        }

        /// <summary>
        /// Spreads gravitational influence from all black holes using inverse-distance kernel.
        /// </summary>
        private static void SpreadBlackHoleInfluence(GridState s, int radius, float scale)
        {
            for (int by = 0; by < s.H; by++)
            {
                for (int bx = 0; bx < s.W; bx++)
                {
                    int bIdx = s.Idx(bx, by);
                    if (!s.IsBlackHole[bIdx]) continue;

                    float bhMass = GetBlackHoleMass(s, bIdx);
                    SpreadInfluenceFromBlackHole(s, bx, by, bhMass, radius, scale);
                }
            }
        }

        /// <summary>
        /// Gets the total mass of the black hole entity that this cell belongs to.
        /// </summary>
        private static float GetBlackHoleMass(GridState s, int idx)
        {
            int root = GetRootAtCell(idx, s.BlackHoleId, s.BlackHoleParent);
            if (root > 0 && root < s.BlackHoleMass.Length)
                return Mathf.Max(1f, s.BlackHoleMass[root]);
            return 1f;
        }

        /// <summary>
        /// Spreads gravitational potential from a single black hole cell.
        /// Uses 1/(1+distance) falloff with Manhattan distance.
        /// </summary>
        private static void SpreadInfluenceFromBlackHole(GridState s, int bx, int by, float mass, int radius, float scale)
        {
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

        // ============================================================================
        // PRIVATE: UTILITIES
        // ============================================================================

        /// <summary>
        /// Checks if a coordinate is within grid bounds.
        /// </summary>
        private static bool IsInBounds(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        /// <summary>
        /// Updates diagnostic properties for monitoring.
        /// </summary>
        private static void UpdateDiagnostics(float drainedTotal, float recoilTotal)
        {
            LastDrainedThisTick = drainedTotal;
            LastDrained = drainedTotal;
            LastRecoilThisTick = recoilTotal;
        }
    }
}
