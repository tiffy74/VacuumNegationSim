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
            float[] Nlocal, float[] V, bool[] IsVacuum, float[] incoming,
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
                    if (V[i] <= 0f) continue; // only viable cells push outward

                    float available = Nlocal[i] * PropagateFrac;
                    if (available <= 0f) continue;

                    float portion = available * 0.25f; // 4-neighbour split

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            continue;

                        int neighborIdx = Idx(nx, ny);
                        if (IsVacuum[neighborIdx]) continue;

                        // Normal propagation into field-present neighbor
                        if (FieldPresent[neighborIdx])
                        {
                            incoming[neighborIdx] += portion;
                        }
                        else if (!IsBlackHole[neighborIdx] && Nlocal[neighborIdx] > MatterAheadThreshold &&
                                 IsFieldBoundary(neighborIdx, FieldPresent, width, height, Idx))
                        {
                            BlackHoleCharge[neighborIdx] += portion;
                            Debug.Log($"BH charge at {neighborIdx % width},{neighborIdx / width}: " +
                                      $"Nlocal={Nlocal[neighborIdx]}, charge={BlackHoleCharge[neighborIdx]}");

                            if (BlackHoleCharge[neighborIdx] > blackHoleThreshold)
                            {
                                IsBlackHole[neighborIdx] = true;
                                BlackHoleCharge[neighborIdx] = 0f;
                                Debug.Log($"Black hole formed at ({neighborIdx % width},{neighborIdx / width})");
                            }
                        }
                    }

                    //if (portion > 0f)
                    //    Debug.Log($"Cell ({x},{y}) outflow portion={portion} to neighbors");
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