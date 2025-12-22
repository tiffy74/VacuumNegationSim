using Assets.Scripts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public static class BlackHoles
    {
        public static float LastDrained { get; private set; }
        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

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
                            next[ni] = true;
                    }
                }
            }

            s.IsBlackHole = next;
        }
        /// <summary>
        /// Drains a fraction of neighbouring cell energy into black-hole cells.
        /// This is a direct state-driven port of your SimulationController.BlackHoleAttractEnergy().
        /// </summary>
        public static float BlackHoleAttractEnergy(GridState s, float absorbFracPerTick = 0.2f, bool fieldOnly = true)
        {
            // Clamp absorb fraction to a safe range
            absorbFracPerTick = UnityEngine.Mathf.Clamp01(absorbFracPerTick);

            float drainedTotal = 0f;

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

                        // Only drain from non-black-hole neighbours
                        if (s.IsBlackHole[ni]) continue;
                        if (fieldOnly && !s.FieldPresent[ni]) continue;

                        float neighbourEnergy = s.Nlocal[ni];
                        if (neighbourEnergy <= 0f) continue;

                        float absorbed = neighbourEnergy * absorbFracPerTick;

                        // Remove from neighbour
                        s.Nlocal[ni] = neighbourEnergy - absorbed;
                        drainedTotal += absorbed;
                    }
                }
            }

            LastDrained = drainedTotal;
            return drainedTotal;
        }
    }
}
