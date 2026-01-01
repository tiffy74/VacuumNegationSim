using UnityEngine;
using Assets.Scripts.Domain;

namespace Assets.Scripts.Events
{
    public static class FieldWave
    {
        /// <summary>
        /// Configuration space pervades everywhere but is only observable at boundaries.
        /// Maintains configuration space around black holes (event horizons) and at the energy front.
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

                    // Black holes themselves never have FieldPresent
                    if (s.IsBlackHole[i])
                    {
                        nextField[i] = false;
                        continue;
                    }

                    // If cell already has field, check if it should keep it
                    if (s.FieldPresent[i])
                    {
                        // Keep field if:
                        // 1. It has energy, OR
                        // 2. It's adjacent to a black hole (event horizon boundary), OR
                        // 3. It's adjacent to another field cell
                        bool shouldKeepField = s.Nlocal[i] > 0f;
                        
                        if (!shouldKeepField)
                        {
                            for (int d = 0; d < 4; d++)
                            {
                                int nx = x + dx[d];
                                int ny = y + dy[d];
                                if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H) continue;
                                int ni = s.Idx(nx, ny);
                                
                                // Keep field if adjacent to BH (event horizon) or another field cell
                                if (s.IsBlackHole[ni] || s.FieldPresent[ni])
                                {
                                    shouldKeepField = true;
                                    break;
                                }
                            }
                        }
                        
                        if (!shouldKeepField)
                            nextField[i] = false; // Allow field to decay if isolated
                        
                        continue; // Don't try to re-create field that already exists
                    }

                    // From here: cell doesn't have field yet
                    // Configuration space forms at boundaries (field-void interface)
                    bool adjacentToField = false;
                    bool adjacentToVoidOrBH = false;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                        {
                            adjacentToVoidOrBH = true;
                            continue;
                        }

                        int ni = s.Idx(nx, ny);
                        
                        if (s.FieldPresent[ni])
                            adjacentToField = true;
                        else if (!s.FieldPresent[ni]) // Void or BH
                            adjacentToVoidOrBH = true;
                    }

                    // Only create config space at the boundary
                    if (!adjacentToField || !adjacentToVoidOrBH)
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
