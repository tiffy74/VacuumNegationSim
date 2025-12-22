using UnityEngine;
using Assets.Scripts.Domain; // adjust if your GridState/SimContext live elsewhere

namespace Assets.Scripts.Events
{
    public static class FieldWave
    {
        /// <summary>
        /// Energy-coupled field propagation.
        /// A field cell can create field in a neighbor only by paying FieldAdvanceCost from its Nlocal.
        /// Optionally requires the source neighbor to be viable.
        ///
        /// This prevents the field/geometry front from racing far ahead of the energy wave.
        /// </summary>
        public static void PropagateFieldWave(
            GridState s,
            int tick,
            float fieldAdvanceChance,
            float fieldAdvanceCost,
            float fieldAdvanceMinSource,
            bool requireViability = false,
            bool seedEnergyOnAdvance = false,
            float seedEnergy = 0.1f)
        {
            bool[] nextField = (bool[])s.FieldPresent.Clone();

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    if (s.FieldPresent[i]) continue;     // already has field
                    if (s.IsBlackHole[i]) continue;      // optional: BH blocks field
                    if (s.IsVacuum[i]) continue;         // optional: vacuum blocks field

                    // Look for any neighbor with field that can "pay" to expand
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H) continue;

                        int ni = s.Idx(nx, ny);
                        if (!s.FieldPresent[ni]) continue;

                        // Optional: require source neighbor to be viable & active
                        if (requireViability)
                        {
                            if (!(s.V[ni] > 0f && s.Active[ni] == 1))
                                continue;
                        }

                        // Must have enough energy to advance field
                        if (s.Nlocal[ni] < fieldAdvanceMinSource)
                            continue;

                        // Chance gate
                        if (Random.value > fieldAdvanceChance)
                            continue;

                        // Pay energy
                        s.Nlocal[ni] = Mathf.Max(0f, s.Nlocal[ni] - fieldAdvanceCost);

                        // Create field
                        nextField[i] = true;

                        // mark arrival tick (used by renderer to show thin yellow ring)
                        if (s.FieldFirstTick[i] == -1)
                            s.FieldFirstTick[i] = tick;

                        // Optional: seed minimal energy so energy doesn't lag absurdly
                        if (seedEnergyOnAdvance)
                            s.Nlocal[i] = Mathf.Max(s.Nlocal[i], seedEnergy);

                        break;
                    }
                }
            }

            s.FieldPresent = nextField;
        }
    }
}
