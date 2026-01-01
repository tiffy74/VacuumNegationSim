using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Events
{
    public static class Pass1
    {
        
        static bool IsBoundaryVoid(int idx, int width, int height, bool[] fieldPresent)
        {
            int x = idx % width;
            int y = idx / width;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            bool hasFieldNeighbor = false;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                int nIdx = ny * width + nx;
                if (fieldPresent[nIdx]) { hasFieldNeighbor = true; break; }
            }
            return hasFieldNeighbor;
        }
        //public static void GatherOutflow(
        //    int width, int height,
        //    float[] Nlocal, float[] V, byte[] Active, bool[] IsVacuum, float[] incoming,
        //    float MinBudgetToPropagate, float PropagateFrac,
        //    bool[] FieldPresent, bool[] IsBlackHole, float[] BlackHoleCharge, float blackHoleThreshold, float MatterAheadThreshold,
        //    int[] FieldFirstTick, int[] EnergyFirstTick,
        //    float[] blackHolePotential, float blackHoleFlowBias)
        //    {
        //    System.Array.Clear(incoming, 0, incoming.Length);

        //    int Idx(int x, int y) => y * width + x;

        //    int[] dx = { 0, 0, -1, 1 };
        //    int[] dy = { -1, 1, 0, 0 };

        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            int i = Idx(x, y);

        //            // Only field-present cells may push energy outward
        //            if (FieldPresent == null || !FieldPresent[i]) continue;

                    
        //            if (Nlocal[i] <= MinBudgetToPropagate) continue;
        //            // allow outflow if active or mildly viable
        //            if (!(Active[i] == 1 || V[i] > -1e-3f)) continue; // frontier ignition

        //            float frac = Mathf.Clamp01(PropagateFrac); // treat as fraction of current energy
        //            float available = Nlocal[i] * frac;
        //            if (available <= 0f) continue;

        //            // collect candidate neighbors
        //            int candCount = 0;
        //            int[] candIdx = new int[4];
        //            float[] candW = new float[4];



        //            float portion = available * 0.25f; // 4-neighbour split
        //            int sentCount = 0;

        //            for (int d = 0; d < 4; d++)
        //            {
        //                int nx = x + dx[d];
        //                int ny = y + dy[d];
        //                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
        //                    continue;

        //                int neighborIdx = Idx(nx, ny);
        //                if (IsVacuum[neighborIdx]) continue;
        //                if (IsBlackHole[neighborIdx])
        //                    continue; // HARD WALL: do not send, do not count
        //                if (FieldPresent[neighborIdx])
        //                {
        //                    incoming[neighborIdx] += portion;
        //                    sentCount++;
        //                }
        //                else if (!IsBlackHole[neighborIdx])
        //                {
        //                    if (!IsBoundaryVoid(neighborIdx, width, height, FieldPresent))
        //                        continue;
        //                    // Energy leaked into a cell without field (void or vacuum): charge a black hole
        //                    BlackHoleCharge[neighborIdx] += portion;
                            
        //                    if (BlackHoleCharge[neighborIdx] > blackHoleThreshold)
        //                    {
        //                        IsBlackHole[neighborIdx] = true;
        //                        BlackHoleCharge[neighborIdx] = 0f;
        //                        Debug.Log($"BH created at {neighborIdx} from void-leak; charge reset");
        //                    }
        //                }
        //            }

        //            // Subtract only the energy actually sent
        //            float sentTotal = portion * sentCount;
        //            if (sentTotal > 0f)
        //                Nlocal[i] = Mathf.Max(0f, Nlocal[i] - sentTotal);
        //        }
        //    }
        //}
        public static int GatherOutflow(
            int width, int height,
            float[] Nlocal, float[] V, byte[] Active, bool[] IsVacuum, float[] incoming,
            float MinBudgetToPropagate, float PropagateFrac,
            bool[] FieldPresent, bool[] IsBlackHole, float[] BlackHoleCharge, float blackHoleThreshold, float MatterAheadThreshold,
            int[] FieldFirstTick, int[] EnergyFirstTick,
            int[] BlackHoleId, int[] BlackHoleParent, float[] BlackHoleMass, ref int NextBlackHoleId,
            int tick, out int boundaryHitsThisTick, out float maxChargeThisTick,
            bool debugForceBHOnFirstBoundaryHit = false)
        {
            System.Array.Clear(incoming, 0, incoming.Length);
            int newBhCount = 0;
            boundaryHitsThisTick = 0;
            maxChargeThisTick = 0f;

            int Idx(int x, int y) => y * width + x;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            // FIRST: Calculate black hole attraction weights for each cell
            // This now considers the TOTAL MASS of merged black holes
            float[] bhAttractionWeight = new float[width * height];
            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    int pi = Idx(px, py);
                    if (!FieldPresent[pi]) continue;
                    
                    // Sum the influence of ALL adjacent black holes (by their root mass)
                    float totalBHInfluence = 0f;
                    
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = px + dx[d];
                        int ny = py + dy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                        int nIdx = Idx(nx, ny);
                        
                        if (IsBlackHole[nIdx])
                        {
                            // Get the ROOT of this BH (handles merged BHs)
                            int root = BlackHoles.GetRootAtCell(nIdx, BlackHoleId, BlackHoleParent);
                            
                            // Get the total mass of this merged BH
                            float bhMass = 1f; // Default mass
                            if (root > 0 && root < BlackHoleMass.Length)
                                bhMass = Mathf.Max(1f, BlackHoleMass[root]);
                            
                            // Influence scales with mass (like gravitational force ∝ M)
                            // Larger merged BHs have stronger pull
                            totalBHInfluence += bhMass;
                        }
                    }
            
                    // Attraction weight: base 1.0 + mass-weighted influence
                    // Scale factor 0.1 to keep weights reasonable (tune as needed)
                    if (totalBHInfluence > 0f)
                        bhAttractionWeight[pi] = 1f + (totalBHInfluence * 0.1f);
                    else
                        bhAttractionWeight[pi] = 0f;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    if (FieldPresent == null || !FieldPresent[i]) continue;
                    if (IsBlackHole[i]) continue;
                    if (Nlocal[i] <= MinBudgetToPropagate) continue;
                    if (!(Active[i] == 1 || V[i] > -1e-3f)) continue;

                    float frac = Mathf.Clamp01(PropagateFrac);
                    float available = Nlocal[i] * frac;
                    if (available <= 0f) continue;

                    // Calculate weighted distribution based on BH attraction
                    float[] weights = new float[4];
                    int[] neighborIndices = new int[4];
                    bool[] isBoundaryVoid = new bool[4];
                    bool[] isBlackHoleNeighbor = new bool[4];
                    float totalWeight = 0f;
                    int validNeighbors = 0;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        {
                            weights[d] = 0f;
                            isBoundaryVoid[d] = false;
                            isBlackHoleNeighbor[d] = false;
                            continue;
                        }

                        int neighborIdx = Idx(nx, ny);
                        neighborIndices[d] = neighborIdx;

                        // Skip vacuum
                        if (IsVacuum[neighborIdx])
                        {
                            weights[d] = 0f;
                            isBoundaryVoid[d] = false;
                            isBlackHoleNeighbor[d] = false;
                            continue;
                        }

                        // Black hole neighbor: mark but don't transfer
                        if (IsBlackHole[neighborIdx])
                        {
                            weights[d] = 0f;
                            isBoundaryVoid[d] = false;
                            isBlackHoleNeighbor[d] = true;
                            continue;
                        }

                        // Field-present neighbors: apply mass-weighted BH attraction
                        if (FieldPresent[neighborIdx])
                        {
                            // Base weight = 1.0, boost by mass-weighted BH influence
                            weights[d] = 1f + bhAttractionWeight[neighborIdx];
                            totalWeight += weights[d];
                            validNeighbors++;
                            isBoundaryVoid[d] = false;
                            isBlackHoleNeighbor[d] = false;
                        }
                        // Boundary void: potential BH formation site
                        else if (IsBoundaryVoid(neighborIdx, width, height, FieldPresent))
                        {
                            weights[d] = 0f;
                            isBoundaryVoid[d] = true;
                            isBlackHoleNeighbor[d] = false;
                        }
                        else
                        {
                            weights[d] = 0f;
                            isBoundaryVoid[d] = false;
                            isBlackHoleNeighbor[d] = false;
                        }
                    }

                    float sentTotal = 0f;
                    float wouldHaveSentIntoNoConfig = 0f;

                    // PART 1: Distribute to field-present neighbors (with mass-weighted BH attraction)
                    if (validNeighbors > 0)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            if (weights[d] <= 0f) continue;

                            // Proportional distribution based on mass-weighted BH attraction
                            float portion = available * (weights[d] / totalWeight);
                            int neighborIdx = neighborIndices[d];

                            incoming[neighborIdx] += portion;
                            sentTotal += portion;
                        }
                    }

                    // PART 2: Handle boundary void leakage (creates black holes after tick 10)
                    for (int d = 0; d < 4; d++)
                    {
                        if (!isBoundaryVoid[d]) continue;

                        int neighborIdx = neighborIndices[d];

                        // Energy leaking to boundary void charges black hole formation
                        float leakPortion = available * 0.25f;
                        BlackHoleCharge[neighborIdx] += leakPortion;
                        boundaryHitsThisTick++;

                        maxChargeThisTick = Mathf.Max(maxChargeThisTick, BlackHoleCharge[neighborIdx]);

                        // CRITICAL: Do NOT allow BH formation before tick 10 (prevents seed destruction)
                        bool allowBH = tick >= 10;

                        // Require both: sufficient time elapsed AND threshold reached
                        bool shouldCreate = allowBH && (BlackHoleCharge[neighborIdx] >= blackHoleThreshold || debugForceBHOnFirstBoundaryHit);
                        if (shouldCreate)
                        {
                            // This automatically merges with adjacent BHs via union-find
                            int bhRoot = BlackHoles.AssignOrMergeAtCell(
                                neighborIdx, width, height,
                                IsBlackHole, BlackHoleId, BlackHoleParent, BlackHoleMass, ref NextBlackHoleId);
                            
                            BlackHoleCharge[neighborIdx] = 0f;
                            newBhCount++;
                            
                            // Get the total mass of the (possibly merged) BH
                            float totalMass = 1f;
                            if (bhRoot > 0 && bhRoot < BlackHoleMass.Length)
                                totalMass = BlackHoleMass[bhRoot];
                            

                            Debug.Log($"BH created/merged at ({neighborIdx % width}, {neighborIdx / width}) tick={tick}, root={bhRoot}, totalMass={totalMass:F2}");

                            if (debugForceBHOnFirstBoundaryHit)
                                debugForceBHOnFirstBoundaryHit = false;
                        }

                        // No actual transfer; energy stays at source
                        wouldHaveSentIntoNoConfig += leakPortion;
                    }

                    // PART 3: Handle blocked BH neighbors (energy reflects back)
                    for (int d = 0; d < 4; d++)
                    {
                        if (!isBlackHoleNeighbor[d]) continue;

                        float blockedPortion = available * 0.25f;
                        wouldHaveSentIntoNoConfig += blockedPortion;
                    }

                    // Deduct successfully transferred energy
                    if (sentTotal > 0f)
                        Nlocal[i] = Mathf.Max(0f, Nlocal[i] - sentTotal);

                    // Accumulate blocked/leaked energy back to source (creates pressure buildup)
                    if (wouldHaveSentIntoNoConfig > 0f)
                        incoming[i] += wouldHaveSentIntoNoConfig;
                }
            }

            if (tick % 20 == 0)
            {
                Debug.Log($"[Tick {tick}] BoundaryHits={boundaryHitsThisTick} MaxCharge={maxChargeThisTick:F4} NewBH={newBhCount}");
            }

            return newBhCount;
        }

        public static void GatherInflow(int width, int height, Func<int, int, int> Idx,
            float[] Nlocal, int[] FieldFirstTick, int[] EnergyFirstTick,
            bool[] FieldPresent, byte[] Active, float[] incoming, int tick)
        {
            // Add inflow to all seeded cells  
            int cx = width / 2, cy = height / 2;
            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        int i = Idx(x, y);

                        float pulse = 0.1f * Mathf.Exp(-tick / 80f);
                        // continuous inflow to the central patch
                        incoming[i] += pulse;

                        // ensure these stay as seeded/field cells (optional but consistent)
                        if (tick < 5) incoming[i] += 0.1f;
                        Active[i] = 1;
                        FieldPresent[i] = true;
                        if (FieldFirstTick[i] == -1) FieldFirstTick[i] = tick;
                        if (EnergyFirstTick[i] == -1) EnergyFirstTick[i] = tick;
                    }
                }
            }
        }
        private static bool IsFieldBoundary(int idx, bool[] FieldPresent, int width, int height, Func<int, int, int> Idx)
        {
            int x = idx % width;
            int y = idx / width;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;
                int nIdx = Idx(nx, ny);
                if (FieldPresent[nIdx])
                    return true;
            }
            return false;
        }
    }
}