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
            float[] bhAttractionWeight = new float[width * height];
            
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

                    float portion = available * 0.25f;
                    int sentCount = 0;
                    float sentTotal = 0f; // <-- Declare sentTotal before use
                    float wouldHaveSentIntoNoConfig = 0f;
                    // Count adjacent black holes
                    int adjacentBH = 0;
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                        int nIdx = Idx(nx, ny);
                        int neighborIdx = Idx(nx, ny);
                        if (IsBlackHole[nIdx]) adjacentBH++;
                        if (IsVacuum[neighborIdx]) continue;

                        // Hard wall: BH blocks transfer
                        if (IsBlackHole[neighborIdx])
                        {
                            wouldHaveSentIntoNoConfig += portion;
                            continue;
                        }
                        ;

                        // Successful transfer inside geometry
                        if (FieldPresent[neighborIdx])
                        {
                            incoming[neighborIdx] += portion;
                            sentTotal += portion;               // <-- only successful sends spend energy
                            continue;
                        }
                        else 
                        {
                            if (!IsBlackHole[neighborIdx])
                            {
                                boundaryHitsThisTick++;
                                BlackHoleCharge[neighborIdx] += portion;
                                maxChargeThisTick = Mathf.Max(maxChargeThisTick, BlackHoleCharge[neighborIdx]);

                                // Do NOT allow BH formation in first few ticks (prevents seed getting punched out)
                                bool allowBH = tick >= 10;

                                // Require accumulation across multiple hits
                                bool shouldCreate = allowBH && BlackHoleCharge[neighborIdx] >= blackHoleThreshold;
                                if (shouldCreate)
                                {
                                    BlackHoles.AssignOrMergeAtCell(
                                        neighborIdx, width, height,
                                        IsBlackHole, BlackHoleId, BlackHoleParent, BlackHoleMass, ref NextBlackHoleId);
                                    BlackHoleCharge[neighborIdx] = 0f;
                                    newBhCount++;
                                    if (debugForceBHOnFirstBoundaryHit)
                                        debugForceBHOnFirstBoundaryHit = false; // only first hit
                                }
                            }
                            // No transfer into void; accumulation occurs on the source side
                            wouldHaveSentIntoNoConfig += portion;
                        }
                    }
                    // Cells next to BH have strong attraction bias (more BH neighbors = stronger)
                    bhAttractionWeight[i] = adjacentBH > 0 ? 1f + (adjacentBH * 0.5f) : 0f;
                    // float sentTotal = portion * sentCount;
                    if (sentTotal > 0f)
                        Nlocal[i] = Mathf.Max(0f, Nlocal[i] - sentTotal);
                    // Accumulate build-up pressure locally (this is your “stable dependency” effect)
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