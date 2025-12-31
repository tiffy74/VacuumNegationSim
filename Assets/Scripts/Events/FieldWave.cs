using UnityEngine;
using Assets.Scripts.Domain;

namespace Assets.Scripts.Events
{
    public static class FieldWave
    {
        /// <summary>
        /// Configuration space (FieldPresent) exists ONLY at the boundary between energy field and void.
        /// This is where possibilities/propagation paths exist.
        /// 
        /// Rule: A void cell becomes configuration space if:
        /// 1. It is NOT black hole
        /// 2. It is adjacent to at least one energy field cell
        /// 3. It is adjacent to void (the "leading edge")
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

                    // Skip if already has field or is a black hole
                    if (s.FieldPresent[i]) continue;
                    if (s.IsBlackHole[i]) continue;

                    // Configuration space can only form at the boundary:
                    // Must be adjacent to both existing field AND void
                    bool adjacentToField = false;
                    bool adjacentToVoid = false;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                        {
                            adjacentToVoid = true; // grid edge counts as void
                            continue;
                        }

                        int ni = s.Idx(nx, ny);
                        
                        if (s.FieldPresent[ni])
                            adjacentToField = true;
                        else if (!s.IsBlackHole[ni])
                            adjacentToVoid = true;
                    }

                    // Only create config space at the boundary
                    if (!adjacentToField || !adjacentToVoid)
                        continue;

                    // Now check if a neighboring field cell can pay to expand
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H) continue;

                        int ni = s.Idx(nx, ny);
                        if (!s.FieldPresent[ni]) continue;

                        // Optional: require source to be viable
                        if (requireViability && !(s.V[ni] > 0f && s.Active[ni] == 1))
                            continue;

                        // Must have enough energy
                        if (s.Nlocal[ni] < fieldAdvanceMinSource)
                            continue;

                        // Probabilistic expansion
                        if (Random.value > fieldAdvanceChance)
                            continue;

                        // Pay energy cost
                        s.Nlocal[ni] = Mathf.Max(0f, s.Nlocal[ni] - fieldAdvanceCost);

                        // Create configuration space
                        nextField[i] = true;

                        if (s.FieldFirstTick[i] == -1)
                            s.FieldFirstTick[i] = tick;

                        // Optional: seed minimal energy
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
