using System;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public static class Pass2
    {
        /// <summary>
        /// Apply incoming energy, update entropy/viability/active flags, and enforce hard constraints.
        /// IMPORTANT: This pass must NOT destroy black holes or allow normal physics to operate outside FieldPresent.
        /// </summary>
        public static void ApplyAndViability(
            Func<int, int, int> Idx,
            int width, int height,
            float[] Nlocal, float[] Entropy, float[] V, byte[] Active, bool[] IsVacuum,
            float[] incoming, int[] zeroEnergyTicks,
            float MinBudgetToPropagate, float ActivationCost,
            ref float NGlobal, float NlocalMax,
            float EntropyGainPerUse, float DecayLoss, float EntropyPenalty, // EntropyPenalty kept for compatibility; not used as "disorder" here
            float VacuumEventProbability, float VacuumEventEntropy,
            Func<float, float, float, float> ComputeViability,
            Func<int, int> CountPersistenceConfigurations,
            int GridWidth, float PropagateFrac,
            bool[] FieldPresent, bool[] IsBlackHole,
            int[] FieldFirstTick, int[] EnergyFirstTick,
            int tick
        )
        {
            // Neighbour offsets (4-way)
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            const int totalConfigs = 16; // 2^4

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    // ---- BLACK HOLE: hard boundary / static marker ----
                    // Do NOT allow normal energy/entropy/viability evolution inside.
                    // Keep them "dead" cells (or whatever you want to render as BH).
                    if (IsBlackHole[i])
                    {
                        Nlocal[i] = 0f;
                        Entropy[i] = 1f;
                        V[i] = 0f;
                        Active[i] = 0;

                        // ensure no accumulation inside
                        incoming[i] = 0f;
                        continue;
                    }

                    // ---- FIELD GATING ----
                    // If no field/configuration space, no normal evolution.
                    // If energy ends up here (should be rare), drop it to 0 and optionally count it.
                    if (!FieldPresent[i])
                    {
                        // Hard rule: energy cannot persist without field space.
                        Nlocal[i] = 0f;
                        V[i] = 0f;
                        Active[i] = 0;
                        incoming[i] = 0f;
                        // Entropy can be left unchanged or decayed; simplest: keep as-is
                        continue;
                    }

                    // ---- VACUUM ----
                    if (IsVacuum[i])
                    {
                        incoming[i] = 0f;
                        V[i] = 0f;
                        Active[i] = 0;
                        continue;
                    }

                    // ---- APPLY INFLOW (always) ----
                    float inFlow = incoming[i];

                    if (inFlow > 0f)
                    {
                        Nlocal[i] = Mathf.Min(NlocalMax, Nlocal[i] + inFlow);
                        if (EnergyFirstTick[i] == -1) EnergyFirstTick[i] = tick;
                    }

                    // Clear incoming buffer for next tick usage (optional; safe)
                    incoming[i] = 0f;

                    // ---- ACTIVATION COST (global pool) ----
                    // Only charge global pool when turning on from inactive due to positive inflow.
                    if (Active[i] == 0 && inFlow > 0f && NGlobal > 0f)
                    {
                        float draw = Mathf.Min(ActivationCost, NGlobal);
                        NGlobal -= draw;

                        // Optional: treat draw as additional local usable energy.
                        // This makes activation meaningful. If you don't want this, comment it out.
                        Nlocal[i] = Mathf.Min(NlocalMax, Nlocal[i] + draw);
                    }

                    // ---- "ENTROPY AS COMPLEXITY" ----
                    // Part A: persistence configuration count -> normalized [0..1]
                    int persistenceConfigs = CountPersistenceConfigurations(i);
                    float configEntropy = Mathf.Log(1 + persistenceConfigs) / Mathf.Log(1 + totalConfigs);

                    // Part B: local gradient proxy (energy contrast with neighbours)
                    float gradSum = 0f;
                    int gradCount = 0;
                    float nHere = Nlocal[i];

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                        int ni = Idx(nx, ny);
                        if (!FieldPresent[ni]) continue;
                        if (IsBlackHole[ni]) continue;

                        gradSum += Mathf.Abs(nHere - Nlocal[ni]);
                        gradCount++;
                    }

                    float grad = (gradCount > 0) ? gradSum / gradCount : 0f;
                    // Scale gradient into [0..1] using a soft saturation curve
                    float gradEntropy = grad / (grad + 1f); // behaves nicely without tuning

                    // Combine into "complexity entropy"
                    float complexityEntropy = Mathf.Clamp01(0.7f * configEntropy + 0.3f * gradEntropy);

                    // Activity-scaled gain (you asked entropy should rise with complex utilisation)
                    float activity = Mathf.Clamp01(inFlow); // inflow as activity proxy in [0..1] if small numbers
                    Entropy[i] = Mathf.Clamp01(complexityEntropy + EntropyGainPerUse * activity);

                    // ---- RANDOM VACUUM EVENTS (optional) ----
                    // If you keep this, make it rare and meaningful.
                    if (VacuumEventProbability > 0f && UnityEngine.Random.value < VacuumEventProbability)
                    {
                        // Vacuum event forces local energy collapse and entropy spike
                        Nlocal[i] = 0f;
                        Entropy[i] = Mathf.Clamp01(Entropy[i] + VacuumEventEntropy);
                        Active[i] = 0;
                        V[i] = 0f;
                        continue;
                    }
                    int blocked = 0;
                    float bhConsumption = 0f;
                    const float BH_CONSUMPTION_RATE = 0.05f; // Energy consumed per adjacent BH per tick

                    if (x > 0 && IsBlackHole[i - 1]) 
                    { 
                        blocked++; 
                        bhConsumption += BH_CONSUMPTION_RATE;
                    }
                    if (x < width - 1 && IsBlackHole[i + 1]) 
                    { 
                        blocked++; 
                        bhConsumption += BH_CONSUMPTION_RATE;
                    }
                    if (y > 0 && IsBlackHole[i - width]) 
                    { 
                        blocked++; 
                        bhConsumption += BH_CONSUMPTION_RATE;
                    }
                    if (y < height - 1 && IsBlackHole[i + width]) 
                    { 
                        blocked++; 
                        bhConsumption += BH_CONSUMPTION_RATE;
                    }

                    // Apply BH consumption BEFORE viability calculation
                    if (bhConsumption > 0f && Nlocal[i] > 0f)
                    {
                        float consumed = Mathf.Min(Nlocal[i] * bhConsumption, Nlocal[i]);
                        Nlocal[i] -= consumed;
                        
                        // Optional: feed consumed energy to global pool or BH mass
                        // NGlobal += consumed * 0.1f; // Some energy returns to global pool
                    }

                    // ---- VIABILITY ----
                    // Compute viability from inflow, current energy, and entropy.
                    // If you want entropy to BOOST viability (your conceptual preference),
                    // encode that inside ComputeViability, not here, to keep the system clean.
                    float constraintBoost = 1f + 0.25f * blocked;
                    V[i] = ComputeViability(inFlow * constraintBoost, Nlocal[i], Entropy[i]);
                    Debug.Log($"[Blocked: {blocked}");
                    // ---- ACTIVE STATE ----
                    // Your current practice: active if viable enough and sufficient energy.
                    if (V[i] > 0f && Nlocal[i] > MinBudgetToPropagate)
                        Active[i] = 1;
                    else
                        Active[i] = 0;

                    // ---- BASELINE DECAY ----
                    // Decay happens after viability (so viability sees current Nlocal).
                    if (DecayLoss > 0f)
                        Nlocal[i] = Mathf.Max(0f, Nlocal[i] - DecayLoss);

                    // ---- ZERO ENERGY TICKS (vacuum eligibility) ----
                    if (Nlocal[i] <= 0f)
                        zeroEnergyTicks[i]++;
                    else
                        zeroEnergyTicks[i] = 0;

                    // Optional: promote to vacuum after sustained zero
                    // if (zeroEnergyTicks[i] > 20) IsVacuum[i] = true;
                }
            }
        }
    }
}
