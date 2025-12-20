using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public static class Pass2
    {
        public static void ApplyAndViability(
            Func<int, int, int> Idx,
            int width, int height,
            float[] Nlocal, float[] Entropy, float[] V, byte[] Active, bool[] IsVacuum,
            float[] incoming, int[] zeroEnergyTicks, float MinBudgetToPropagate, float ActivationCost,
            ref float NGlobal, float NlocalMax, float EntropyGainPerUse, float DecayLoss, float EntropyPenalty,
            float VacuumEventProbability, float VacuumEventEntropy,
            Func<float, float, float, float> ComputeViability, Func<int, int> CountPersistenceConfigurations,
            int GridWidth, float PropagateFrac, bool[] FieldPresent, bool[] IsBlackHole,
            int[] FieldFirstTick, int[] EnergyFirstTick, int tick)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int i = Idx(x, y);

                        // Black hole: skip all normal logic
                        if (IsBlackHole[i])
                        {
                            Nlocal[i] = 0f;
                            Entropy[i] = 1f;
                            Active[i] = 0;
                            continue;
                        }

                        // If the field wave hasn't reached this cell, don't allow energy/viability
                        if (!FieldPresent[i])
                        {
                            if (Nlocal[i] > 0f)
                            {
                                // Energy in a non-field cell: create a black hole
                                IsBlackHole[i] = true;
                                Nlocal[i] = 0f;
                                Entropy[i] = 1f;
                                Active[i] = 0;
                            }
                            continue;
                        }


                    if (IsVacuum[i]) continue;

                            int persistenceConfigs = CountPersistenceConfigurations(i);
                            int totalConfigs = 16; // 2^4 for 4 neighbors
                            float entropy = Mathf.Log(1 + persistenceConfigs) / Mathf.Log(1 + totalConfigs);

                            // Occasionally, force high entropy and low energy
                            if (UnityEngine.Random.value < 0.01f)
                            {
                                entropy = 10f;
                                Nlocal[i] = 0f; // cell becomes vacuum
                            }

                            float inFlow = incoming[i];

                            // Always deposit inflow into local budget (cap)
                            if (inFlow > 0f)
                                Nlocal[i] = Mathf.Min(NlocalMax, Nlocal[i] + inFlow);

                            // Activation cost from global pool if toggling on
                            if (Active[i] == 0 && inFlow > 0f && NGlobal > 0f)
                            {
                                float draw = Mathf.Min(ActivationCost, NGlobal);
                                NGlobal -= draw;
                                // Optional: treat draw as extra usable budget:
                                // Nlocal[i] = Mathf.Min(NlocalMax, Nlocal[i] + draw);
                                // Nlocal[i] = Mathf.Min(NlocalMax, Nlocal[i] + inFlow);
                            }

                            // Entropy grows with activity (use cappedActivity)
                            float cappedActivity = Mathf.Min(inFlow, 1.0f);
                            Entropy[i] = Mathf.Clamp01(entropy + EntropyGainPerUse * cappedActivity);

                            // Viability
                            V[i] = ComputeViability(inFlow, Nlocal[i], Entropy[i]);

                            // State
                            Active[i] = (V[i] > 0f && Nlocal[i] > MinBudgetToPropagate) ? (byte)1 : (byte)0;
                            //else if (V[i] <= 0f) Active[i] = 0;

                            // Baseline local decay
                            Nlocal[i] -= DecayLoss;

                            // Attraction bonus for non-viable, non-vacuum cells
                            int activeNeighbors = 0;
                            if (x > 0 && Active[i - 1] == 1) activeNeighbors++;
                            if (x < width - 1 && Active[i + 1] == 1) activeNeighbors++;
                            if (y > 0 && Active[i - width] == 1) activeNeighbors++;
                            if (y < height - 1 && Active[i + width] == 1) activeNeighbors++;

                            // if (!IsVacuum[i] && V[i] <= 0f && Nlocal[i] > 0f)
                            //    Nlocal[i] = Mathf.Min(0.5f, Nlocal[i] + 0.05f * activeNeighbors); // cap at 0.5f for static cells

                            // Final clamp to ensure non-negative energy
                            Nlocal[i] = Mathf.Max(0f, Nlocal[i]);
                            if (EnergyFirstTick[i] == -1 && (incoming[i] > 0f || Nlocal[i] > 0f))
                            {
                            EnergyFirstTick[i] = tick;   // pass tick in as parameter, or use a ref }
                            }
                        // Set vacuum status after all updates
                        if (Nlocal[i] <= 0f && incoming[i] <= 0f)
                                zeroEnergyTicks[i]++;
                            else
                                zeroEnergyTicks[i] = 0;

                            //if (zeroEnergyTicks[i] > 5) // e.g., 5 ticks of zero energy
                            //    IsVacuum[i] = true;
                        }
                    }
                }
    }
}
