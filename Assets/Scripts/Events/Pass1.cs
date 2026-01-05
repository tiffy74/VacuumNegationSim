using System;
using UnityEngine;

namespace Assets.Scripts.Events
{
    /// <summary>
    /// Pass 1: Energy Propagation and Black Hole Attraction
    /// 
    /// Handles energy flow between cells with mass-weighted black hole attraction.
    /// Energy preferentially flows toward cells adjacent to black holes (stable dependencies).
    /// Creates new black holes when energy leaks into boundary voids after charge accumulation.
    /// </summary>
    public static class Pass1
    {
        // ============================================================================
        // PRIVATE CONSTANTS
        // ============================================================================
        
        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

        // ============================================================================
        // PUBLIC API: ENERGY PROPAGATION
        // ============================================================================

        /// <summary>
        /// Main energy propagation pass with black hole attraction.
        /// Energy flows from active cells to neighbors, weighted by proximity to black holes.
        /// Creates black holes at boundary voids when charge threshold is exceeded.
        /// </summary>
        /// <param name="blockedIntoBH">Accumulator for energy blocked by black holes (diagnostic)</param>
        /// <returns>Number of new black holes created this tick</returns>
        public static int GatherOutflow(
            int width, int height,
            float[] Nlocal, float[] V, byte[] Active, bool[] IsVacuum, float[] incoming,
            float MinBudgetToPropagate, float PropagateFrac,
            bool[] FieldPresent, bool[] IsBlackHole, float[] BlackHoleCharge, float blackHoleThreshold, float MatterAheadThreshold,
            int[] FieldFirstTick, int[] EnergyFirstTick,
            int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass, ref int NextBlackHoleId,
            int tick, out int boundaryHitsThisTick, out float maxChargeThisTick,
            ref float blockedIntoBH,
            ref int leakAttempts,
            ref float leakEnergy,
            bool debugForceBHOnFirstBoundaryHit = false)
        {
            InitializeOutflowBuffers(incoming, out int newBhCount, out boundaryHitsThisTick, out maxChargeThisTick);

            int Idx(int x, int y) => y * width + x;

            // Phase 1: Calculate black hole gravitational influence field
            float[] bhAttractionWeight = ComputeBlackHoleAttractionField(
                width, height, Idx, FieldPresent, IsBlackHole, BlackHoleId, BlackHoleParent, BlackHoleMass);

            // Phase 2: Propagate energy from all active cells
            PropagateEnergyFromActiveCells(
                width, height, Idx, Nlocal, V, Active, IsVacuum, incoming,
                MinBudgetToPropagate, PropagateFrac, FieldPresent, IsBlackHole,
                BlackHoleCharge, blackHoleThreshold, BlackHoleId, BlackHoleParent, BlackHoleMass,
                ref NextBlackHoleId, tick, bhAttractionWeight,
                ref newBhCount, ref boundaryHitsThisTick, ref maxChargeThisTick,
                ref blockedIntoBH,
                ref leakAttempts,
                ref leakEnergy,
                debugForceBHOnFirstBoundaryHit);

            return newBhCount;
        }

        /// <summary>
        /// Adds continuous energy inflow to the central seed region.
        /// Ensures seed cells remain active and maintain field presence.
        /// </summary>
        public static void GatherInflow(int width, int height, Func<int, int, int> Idx,
            float[] Nlocal, int[] FieldFirstTick, int[] EnergyFirstTick,
            bool[] FieldPresent, byte[] Active, float[] incoming, int tick)
        {
            int seedRadius = 2;
            int cx = width / 2;
            int cy = height / 2;

            ApplyEnergyPulseToSeedRegion(
                seedRadius, cx, cy, width, height, Idx,
                Nlocal, FieldFirstTick, EnergyFirstTick, FieldPresent, Active, incoming, tick);
        }

        // ============================================================================
        // PRIVATE: INITIALIZATION
        // ============================================================================

        /// <summary>
        /// Clears output buffers and initializes counters.
        /// </summary>
        private static void InitializeOutflowBuffers(float[] incoming, out int newBhCount, out int boundaryHitsThisTick, out float maxChargeThisTick)
        {
            System.Array.Clear(incoming, 0, incoming.Length);
            newBhCount = 0;
            boundaryHitsThisTick = 0;
            maxChargeThisTick = 0f;
        }

        // ============================================================================
        // PRIVATE: BLACK HOLE ATTRACTION FIELD
        // ============================================================================

        /// <summary>
        /// Computes the gravitational attraction field around all black holes.
        /// Each cell's weight = 1.0 + 0.1 × (sum of adjacent BH masses).
        /// Larger merged black holes exert stronger influence (analogous to F ∝ M in gravity).
        /// </summary>
        private static float[] ComputeBlackHoleAttractionField(
            int width, int height, Func<int, int, int> Idx,
            bool[] FieldPresent, bool[] IsBlackHole,
            int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass)
        {
            float[] bhAttractionWeight = new float[width * height];

            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    int pi = Idx(px, py);
                    if (!FieldPresent[pi]) continue;

                    float totalBHInfluence = SumAdjacentBlackHoleMasses(
                        px, py, width, height, Idx, IsBlackHole, BlackHoleId, BlackHoleParent, BlackHoleMass);

                    // Scale factor 0.1 keeps weights in reasonable range
                    bhAttractionWeight[pi] = (totalBHInfluence > 0f) 
                        ? 1f + (totalBHInfluence * 0.1f) 
                        : 0f;
                }
            }

            return bhAttractionWeight;
        }

        /// <summary>
        /// Sums the total mass of all black holes adjacent to a given cell.
        /// Uses union-find to get root mass of merged black holes.
        /// </summary>
        private static float SumAdjacentBlackHoleMasses(
            int px, int py, int width, int height, Func<int, int, int> Idx,
            bool[] IsBlackHole, int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass)
        {
            float totalMass = 0f;

            for (int d = 0; d < 4; d++)
            {
                int nx = px + dx[d];
                int ny = py + dy[d];
                if (!IsInBounds(nx, ny, width, height)) continue;

                int nIdx = Idx(nx, ny);
                if (!IsBlackHole[nIdx]) continue;

                float bhMass = GetBlackHoleMassAtCell(nIdx, BlackHoleId, BlackHoleParent, BlackHoleMass);
                totalMass += bhMass;
            }

            return totalMass;
        }

        /// <summary>
        /// Gets the total mass of the black hole entity at a given cell.
        /// </summary>
        private static float GetBlackHoleMassAtCell(int idx, int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass)
        {
            int root = BlackHoles.GetRootAtCell(idx, BlackHoleId, BlackHoleParent);
            
            if (root > 0 && root < BlackHoleMass.Length)
                return Mathf.Max(1f, BlackHoleMass[root]);
            
            return 1f; // Default mass
        }

        // ============================================================================
        // PRIVATE: ENERGY PROPAGATION
        // ============================================================================

        /// <summary>
        /// Propagates energy from all active cells that meet outflow criteria.
        /// </summary>
        private static void PropagateEnergyFromActiveCells(
            int width, int height, Func<int, int, int> Idx,
            float[] Nlocal, float[] V, byte[] Active, bool[] IsVacuum, float[] incoming,
            float MinBudgetToPropagate, float PropagateFrac,
            bool[] FieldPresent, bool[] IsBlackHole, float[] BlackHoleCharge, float blackHoleThreshold,
            int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass, ref int NextBlackHoleId,
            int tick, float[] bhAttractionWeight,
            ref int newBhCount, ref int boundaryHitsThisTick, ref float maxChargeThisTick,
            ref float blockedIntoBH,
            ref int leakAttempts,
            ref float leakEnergy,
            bool debugForceBHOnFirstBoundaryHit)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    if (!CanCellPropagateEnergy(i, FieldPresent, IsBlackHole, Nlocal, Active, V, MinBudgetToPropagate))
                        continue;

                    float available = CalculateAvailableEnergy(Nlocal[i], PropagateFrac);
                    if (available <= 0f) continue;

                    PropagateEnergyFromCell(
                        i, x, y, width, height, Idx, available,
                        Nlocal, IsVacuum, incoming, FieldPresent, IsBlackHole,
                        BlackHoleCharge, blackHoleThreshold, BlackHoleId, BlackHoleParent, BlackHoleMass,
                        ref NextBlackHoleId, tick, bhAttractionWeight,
                        ref newBhCount, ref boundaryHitsThisTick, ref maxChargeThisTick,
                        ref blockedIntoBH,
                        ref leakAttempts,
                        ref leakEnergy,
                        debugForceBHOnFirstBoundaryHit);
                }
            }
        }

        /// <summary>
        /// Checks if a cell meets the criteria to propagate energy.
        /// </summary>
        private static bool CanCellPropagateEnergy(
            int i, bool[] FieldPresent, bool[] IsBlackHole,
            float[] Nlocal, byte[] Active, float[] V, float MinBudgetToPropagate)
        {
            if (FieldPresent == null || !FieldPresent[i]) return false;
            if (IsBlackHole[i]) return false;
            if (Nlocal[i] <= MinBudgetToPropagate) return false;
            if (!(Active[i] == 1 || V[i] > -1e-3f)) return false; // Frontier ignition

            return true;
        }

        /// <summary>
        /// Calculates the amount of energy available for propagation.
        /// </summary>
        private static float CalculateAvailableEnergy(float localEnergy, float propagateFrac)
        {
            float frac = Mathf.Clamp01(propagateFrac);
            return localEnergy * frac;
        }

        /// <summary>
        /// Propagates energy from a single cell to its neighbors.
        /// Handles three cases: field-present neighbors, boundary voids, and black hole neighbors.
        /// </summary>
        private static void PropagateEnergyFromCell(
            int i, int x, int y, int width, int height, Func<int, int, int> Idx,
            float available, float[] Nlocal, bool[] IsVacuum, float[] incoming,
            bool[] FieldPresent, bool[] IsBlackHole, float[] BlackHoleCharge, float blackHoleThreshold,
            int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass, ref int NextBlackHoleId,
            int tick, float[] bhAttractionWeight,
            ref int newBhCount, ref int boundaryHitsThisTick, ref float maxChargeThisTick,
            ref float blockedIntoBH,
            ref int leakAttempts,
            ref float leakEnergy,
            bool debugForceBHOnFirstBoundaryHit)
        {
            // Analyze all 4 neighbors
            var neighborAnalysis = AnalyzeNeighbors(
                x, y, width, height, Idx, IsVacuum, IsBlackHole, FieldPresent, bhAttractionWeight,
                ref leakAttempts, ref leakEnergy, available);

            // Distribute energy based on neighbor types
            float sentTotal = DistributeEnergyToFieldNeighbors(
                available, neighborAnalysis, incoming);

            float reflectedEnergy = HandleBoundaryVoidLeakage(
                available, neighborAnalysis, width, height, IsBlackHole, BlackHoleCharge, blackHoleThreshold,
                BlackHoleId, BlackHoleParent, BlackHoleMass, ref NextBlackHoleId, tick,
                ref newBhCount, ref boundaryHitsThisTick, ref maxChargeThisTick,
                debugForceBHOnFirstBoundaryHit);

            // CRITICAL FIX: Blocked energy is reflected back to source cell (Option 1)
            reflectedEnergy += HandleBlockedBlackHoleNeighbors(available, neighborAnalysis, ref blockedIntoBH);

            // Update source cell energy: deduct sent energy, add reflected energy
            ApplyEnergyTransfers(i, Nlocal, incoming, sentTotal, reflectedEnergy);
        }

        // ============================================================================
        // PRIVATE: NEIGHBOR ANALYSIS
        // ============================================================================

        /// <summary>
        /// Represents the analysis of a cell's 4 neighbors for energy propagation.
        /// </summary>
        private struct NeighborAnalysis
        {
            public float[] Weights;
            public int[] Indices;
            public bool[] IsBoundaryVoid;
            public bool[] IsBlackHoleNeighbor;
            public float TotalWeight;
            public int ValidNeighborCount;
        }

        /// <summary>
        /// Analyzes all 4 neighbors of a cell to determine energy propagation weights.
        /// </summary>
        private static NeighborAnalysis AnalyzeNeighbors(
            int x, int y, int width, int height, Func<int, int, int> Idx,
            bool[] IsVacuum, bool[] IsBlackHole, bool[] FieldPresent, float[] bhAttractionWeight,
            ref int leakAttempts, ref float leakEnergy, float available)
        {
            var analysis = new NeighborAnalysis
            {
                Weights = new float[4],
                Indices = new int[4],
                IsBoundaryVoid = new bool[4],
                IsBlackHoleNeighbor = new bool[4],
                TotalWeight = 0f,
                ValidNeighborCount = 0
            };

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, width, height))
                {
                    analysis.Weights[d] = 0f;
                    continue;
                }

                int neighborIdx = Idx(nx, ny);
                analysis.Indices[d] = neighborIdx;

                ClassifyNeighbor(
                    neighborIdx, nx, ny, width, height, IsVacuum, IsBlackHole, FieldPresent,
                    bhAttractionWeight, ref analysis, d, ref leakAttempts, ref leakEnergy, available);
            }

            return analysis;
        }

        /// <summary>
        /// Classifies a single neighbor into one of: vacuum, black hole, field-present, or boundary void.
        /// </summary>
        private static void ClassifyNeighbor(
            int neighborIdx, int nx, int ny, int width, int height,
            bool[] IsVacuum, bool[] IsBlackHole, bool[] FieldPresent,
            float[] bhAttractionWeight, ref NeighborAnalysis analysis, int direction,
            ref int leakAttempts, ref float leakEnergy, float available)
        {
            // Case 1: Vacuum (impassable)
            if (IsVacuum[neighborIdx])
            {
                analysis.Weights[direction] = 0f;
                return;
            }

            // Case 2: Black hole (geometric constraint)
            if (IsBlackHole[neighborIdx])
            {
                analysis.Weights[direction] = 0f;
                analysis.IsBlackHoleNeighbor[direction] = true;
                return;
            }

            // Case 3: Field-present (normal propagation with BH attraction)
            if (FieldPresent[neighborIdx])
            {
                analysis.Weights[direction] = 1f + bhAttractionWeight[neighborIdx];
                analysis.TotalWeight += analysis.Weights[direction];
                analysis.ValidNeighborCount++;
                return;
            }

            // Case 4: Boundary void (potential BH formation site)
            if (IsBoundaryVoid(neighborIdx, width, height, FieldPresent))
            {
                analysis.IsBoundaryVoid[direction] = true;
                
                // DIAGNOSTIC: Track leak attempt
                leakAttempts++;
                leakEnergy += available * 0.25f; // Portion that would leak
                return;
            }

            // Case 5: Isolated void (no propagation, but still counts as leak attempt)
            analysis.Weights[direction] = 0f;
            
            // DIAGNOSTIC: Track leak attempt to isolated void
            leakAttempts++;
            leakEnergy += available * 0.25f;
        }

        // ============================================================================
        // PRIVATE: ENERGY DISTRIBUTION
        // ============================================================================

        /// <summary>
        /// Distributes energy to field-present neighbors proportionally to their weights.
        /// Cells adjacent to black holes receive more energy (stable dependency effect).
        /// </summary>
        private static float DistributeEnergyToFieldNeighbors(
            float available, NeighborAnalysis analysis, float[] incoming)
        {
            if (analysis.ValidNeighborCount == 0)
                return 0f;

            float sentTotal = 0f;

            for (int d = 0; d < 4; d++)
            {
                if (analysis.Weights[d] <= 0f) continue;

                float portion = available * (analysis.Weights[d] / analysis.TotalWeight);
                int neighborIdx = analysis.Indices[d];

                incoming[neighborIdx] += portion;
                sentTotal += portion;
            }

            return sentTotal;
        }

        /// <summary>
        /// Handles energy leaking into boundary voids.
        /// Accumulates charge and creates black holes when threshold exceeded (after tick 10).
        /// </summary>
        private static float HandleBoundaryVoidLeakage(
            float available, NeighborAnalysis analysis, int width, int height,
            bool[] IsBlackHole, float[] BlackHoleCharge, float blackHoleThreshold,
            int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass, ref int NextBlackHoleId,
            int tick, ref int newBhCount, ref int boundaryHitsThisTick, ref float maxChargeThisTick,
            bool debugForceBHOnFirstBoundaryHit)
        {
            float totalReflectedEnergy = 0f;

            for (int d = 0; d < 4; d++)
            {
                if (!analysis.IsBoundaryVoid[d]) continue;

                int neighborIdx = analysis.Indices[d];
                float leakPortion = available * 0.25f;

                AccumulateBlackHoleCharge(
                    neighborIdx, leakPortion, BlackHoleCharge,
                    ref boundaryHitsThisTick, ref maxChargeThisTick);

                bool blackHoleCreated = TryCreateBlackHole(
                    neighborIdx, width, height, tick, BlackHoleCharge, blackHoleThreshold,
                    IsBlackHole, BlackHoleId, BlackHoleParent, BlackHoleMass,
                    ref NextBlackHoleId, ref newBhCount, debugForceBHOnFirstBoundaryHit);

                totalReflectedEnergy += leakPortion;
            }

            return totalReflectedEnergy;
        }

        /// <summary>
        /// Accumulates charge at a boundary void cell.
        /// </summary>
        private static void AccumulateBlackHoleCharge(
            int idx, float leakAmount, float[] BlackHoleCharge,
            ref int boundaryHitsThisTick, ref float maxChargeThisTick)
        {
            BlackHoleCharge[idx] += leakAmount;
            boundaryHitsThisTick++;
            maxChargeThisTick = Mathf.Max(maxChargeThisTick, BlackHoleCharge[idx]);
        }

        /// <summary>
        /// Attempts to create a black hole at a boundary void if conditions are met.
        /// Requires: tick >= 10 AND charge >= threshold.
        /// </summary>
        private static bool TryCreateBlackHole(
            int idx, int width, int height, int tick,
            float[] BlackHoleCharge, float blackHoleThreshold,
            bool[] IsBlackHole, int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass,
            ref int NextBlackHoleId, ref int newBhCount, bool debugForce)
        {
            bool allowBH = tick >= 10; // Protect initial seed
            bool shouldCreate = allowBH && (BlackHoleCharge[idx] >= blackHoleThreshold || debugForce);

            if (!shouldCreate)
                return false;

            BlackHoles.AssignOrMergeAtCell(
                idx, width, height,
                IsBlackHole, BlackHoleId, BlackHoleParent, BlackHoleMass, ref NextBlackHoleId);

            BlackHoleCharge[idx] = 0f;
            newBhCount++;

            return true;
        }

        /// <summary>
        /// Handles energy that tries to propagate into black hole neighbors.
        /// Energy is reflected back (cannot penetrate collapsed configuration space).
        /// 
        /// DIAGNOSTIC: Accumulates blocked energy flux for halo analysis.
        /// </summary>
        private static float HandleBlockedBlackHoleNeighbors(
            float available, NeighborAnalysis analysis, ref float blockedIntoBH)
        {
            float totalBlocked = 0f;

            for (int d = 0; d < 4; d++)
            {
                if (!analysis.IsBlackHoleNeighbor[d]) continue;

                float blockedPortion = available * 0.25f;
                totalBlocked += blockedPortion;

                // DIAGNOSTIC: Accumulate blocked flux (measurement only, no behavior change)
                blockedIntoBH += blockedPortion;
            }

            return totalBlocked;
        }

        /// <summary>
        /// Applies energy transfers: deducts sent energy, accumulates reflected energy.
        /// </summary>
        private static void ApplyEnergyTransfers(
            int sourceIdx, float[] Nlocal, float[] incoming,
            float sentTotal, float reflectedEnergy)
        {
            if (sentTotal > 0f)
                Nlocal[sourceIdx] = Mathf.Max(0f, Nlocal[sourceIdx] - sentTotal);

            if (reflectedEnergy > 0f)
                incoming[sourceIdx] += reflectedEnergy;
        }

        // =========================================================================
        // PRIVATE: SEED REGION INFLOW
        // ============================================================================

        /// <summary>
        /// Applies exponentially decaying energy pulse to the central seed region.
        /// Ensures seed cells remain active during early simulation phase.
        /// </summary>
        private static void ApplyEnergyPulseToSeedRegion(
            int seedRadius, int cx, int cy, int width, int height, Func<int, int, int> Idx,
            float[] Nlocal, int[] FieldFirstTick, int[] EnergyFirstTick,
            bool[] FieldPresent, byte[] Active, float[] incoming, int tick)
        {
            for (int dy = -seedRadius; dy <= seedRadius; dy++)
            {
                for (int dx = -seedRadius; dx <= seedRadius; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (!IsInBounds(x, y, width, height)) continue;

                    int i = Idx(x, y);

                    ApplySeedPulse(i, incoming, tick);
                    EnsureSeedCellStability(i, FieldPresent, Active, FieldFirstTick, EnergyFirstTick, tick);
                }
            }
        }

        /// <summary>
        /// Applies exponentially decaying energy pulse to a seed cell.
        /// </summary>
        private static void ApplySeedPulse(int idx, float[] incoming, int tick)
        {
            float pulse = 0.1f * Mathf.Exp(-tick / 80f);
            incoming[idx] += pulse;

            // Extra boost during first 5 ticks
            if (tick < 5)
                incoming[idx] += 0.1f;
        }

        /// <summary>
        /// Ensures seed cells maintain field presence and active state.
        /// </summary>
        private static void EnsureSeedCellStability(
            int idx, bool[] FieldPresent, byte[] Active,
            int[] FieldFirstTick, int[] EnergyFirstTick, int tick)
        {
            Active[idx] = 1;
            FieldPresent[idx] = true;

            if (FieldFirstTick[idx] == -1)
                FieldFirstTick[idx] = tick;

            if (EnergyFirstTick[idx] == -1)
                EnergyFirstTick[idx] = tick;
        }

        // ============================================================================
        // PRIVATE: BOUNDARY DETECTION
        // ============================================================================

        /// <summary>
        /// Checks if a void cell is adjacent to at least one field-present cell.
        /// These are "boundary voids" where black holes can form.
        /// </summary>
        private static bool IsBoundaryVoid(int idx, int width, int height, bool[] fieldPresent)
        {
            int x = idx % width;
            int y = idx / width;

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, width, height)) continue;

                int nIdx = ny * width + nx;
                if (fieldPresent[nIdx])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a cell is adjacent to at least one field-present cell.
        /// Used for field boundary detection.
        /// </summary>
        private static bool IsFieldBoundary(int idx, bool[] FieldPresent, int width, int height, Func<int, int, int> Idx)
        {
            int x = idx % width;
            int y = idx / width;

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, width, height)) continue;

                int nIdx = Idx(nx, ny);
                if (FieldPresent[nIdx])
                    return true;
            }

            return false;
        }

        // ============================================================================
        // PRIVATE: UTILITIES
        // ============================================================================

        /// <summary>
        /// Checks if coordinates are within grid bounds.
        /// </summary>
        private static bool IsInBounds(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
}