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
        public static void GatherOutflow(
            int width, int height,
            float[] Nlocal, float[] V, byte[] Active, bool[] IsVacuum, float[] incoming,
            float MinBudgetToPropagate, float PropagateFrac,
            bool[] FieldPresent, bool[] IsBlackHole, float[] BlackHoleCharge, float blackHoleThreshold, float MatterAheadThreshold,
            int[] FieldFirstTick, int[] EnergyFirstTick )
            {
            System.Array.Clear(incoming, 0, incoming.Length);

            int Idx(int x, int y) => y * width + x;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    // Only field-present cells may push energy outward
                    if (FieldPresent == null || !FieldPresent[i]) continue;

                    if (IsBlackHole[i]) continue;
                    if (Nlocal[i] <= MinBudgetToPropagate) continue;
                    // allow outflow if active or mildly viable
                    if (!(Active[i] == 1 || V[i] > -1e-3f)) continue; // frontier ignition

                    float frac = Mathf.Clamp01(PropagateFrac); // treat as fraction of current energy
                    float available = Nlocal[i] * frac;
                    if (available <= 0f) continue;

                    float portion = available * 0.25f; // 4-neighbour split
                    int sentCount = 0;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            continue;

                        int neighborIdx = Idx(nx, ny);
                        if (IsVacuum[neighborIdx]) continue;

                        if (FieldPresent[neighborIdx])
                        {
                            incoming[neighborIdx] += portion;
                            sentCount++;
                        }
                        else if (!IsBlackHole[neighborIdx] &&
                                 IsFieldBoundary(neighborIdx, FieldPresent, width, height, Idx))
                        {
                            BlackHoleCharge[neighborIdx] += portion;
                            sentCount++;

                            if (BlackHoleCharge[neighborIdx] > blackHoleThreshold)
                            {
                                IsBlackHole[neighborIdx] = true;
                                BlackHoleCharge[neighborIdx] = 0f;
                            }
                        }
                    }

                    // Subtract only the energy actually sent
                    float sentTotal = portion * sentCount;
                    if (sentTotal > 0f)
                        Nlocal[i] = Mathf.Max(0f, Nlocal[i] - sentTotal);
                }
            }
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