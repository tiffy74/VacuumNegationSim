using Assets.Scripts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events
{
    static class FieldWave
    {
        // 4-neighbour connectivity
        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

        /// <summary>
        /// Expands FieldPresent outward. Records FieldFirstTick the first time field appears in a cell.
        /// </summary>
        public static void PropagateFieldWave(GridState s, int tick, float spreadProbability)
        {
            // If you want "do nothing" probabilities to be cheap:
            if (spreadProbability <= 0f) return;

            // We'll build a nextField buffer (bool[]) but reuse it rather than Clone() if you later add a buffer.
            bool[] nextField = (bool[])s.FieldPresent.Clone();

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    // Already has field -> nothing to do here
                    if (s.FieldPresent[i]) continue;

                    // Optional: don't expand into permanent vacuum cells if you use them as sinks
                    if (s.IsVacuum[i]) continue;

                    // If any neighbor has field, we may acquire it this tick
                    bool neighborHasField = false;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];

                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                            continue;

                        int ni = s.Idx(nx, ny);

                        if (s.FieldPresent[ni])
                        {
                            neighborHasField = true;
                            break;
                        }
                    }

                    if (!neighborHasField) continue;

                    // Probabilistic spread
                    if (UnityEngine.Random.value < spreadProbability)
                    {
                        nextField[i] = true;

                        // Mark first tick we gained field
                        if (s.FieldFirstTick[i] == -1)
                            s.FieldFirstTick[i] = tick;
                    }
                }
            }

            s.FieldPresent = nextField;
        }
    }
}