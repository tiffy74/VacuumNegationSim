using System;

namespace Viable.Engine.Steps
{
    /// <summary>
    /// Phase 2: Apply incoming resource flow, update complexity/viability/active flags, and enforce constraints.
    /// </summary>
    public static class InflowPhase
    {
        /// <summary>
        /// Apply incoming resource flow, update complexity/viability/active flags, and enforce hard constraints.
        /// Uses RNG from StepContext for deterministic perturbations.
        /// </summary>
        public static void ApplyAndViability(
            Func<int, int, int> Idx,
            int width, int height,
            float[] ResourceLocal, float[] ComplexityMetric, float[] V, byte[] Active, bool[] IsInactive,
            float[] incoming, int[] zeroResourceTicks,
            float MinBudgetToPropagate, float ActivationCost,
            ref float ResourceGlobal, float ResourceLocalMax,
            float ComplexityGainPerUse, float DecayLoss, float ComplexityPenalty,
            float PerturbationProbability, float PerturbationComplexity,
            Func<float, float, float, float> ComputeViability,
            Func<int, int> CountPersistenceConfigurations,
            int GridWidth, float PropagateFrac,
            bool[] ActiveRegion, bool[] IsSink,
            int[] RegionActivationTick, int[] ResourceFirstTick,
            int tick,
            Random rng)
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

                    // ---- SINK REGIONS: no state transitions possible ----
                    if (IsSink[i])
                    {
                        ResourceLocal[i] = 0f;
                        ComplexityMetric[i] = 1f; // maximum complexity (no viable configurations)
                        V[i] = 0f;
                        Active[i] = 0;
                        incoming[i] = 0f;
                        continue;
                    }

                    // ---- REGION GATING ----
                    // If no active region (no state transition substrate), no evolution.
                    if (!ActiveRegion[i])
                    {
                        // Hard rule: resource cannot persist without active region.
                        ResourceLocal[i] = 0f;
                        V[i] = 0f;
                        Active[i] = 0;
                        incoming[i] = 0f;
                        continue;
                    }

                    // ---- PERMANENTLY INACTIVE ----
                    if (IsInactive[i])
                    {
                        incoming[i] = 0f;
                        V[i] = 0f;
                        Active[i] = 0;
                        continue;
                    }

                    // ---- APPLY INFLOW ----
                    float inFlow = incoming[i];

                    if (inFlow > 0f)
                    {
                        ResourceLocal[i] = Math.Min(ResourceLocalMax, ResourceLocal[i] + inFlow);
                        if (ResourceFirstTick[i] == -1) ResourceFirstTick[i] = tick;
                    }

                    // Clear incoming buffer for next tick
                    incoming[i] = 0f;

                    // ---- ACTIVATION COST (global pool) ----
                    // Charge global pool when turning on from inactive due to positive inflow.
                    if (Active[i] == 0 && inFlow > 0f && ResourceGlobal > 0f)
                    {
                        float draw = Math.Min(ActivationCost, ResourceGlobal);
                        ResourceGlobal -= draw;

                        // Treat draw as additional local usable resource
                        ResourceLocal[i] = Math.Min(ResourceLocalMax, ResourceLocal[i] + draw);
                    }

                    // ---- COMPLEXITY METRIC ----
                    // Part A: persistence configuration count -> normalized [0..1]
                    int persistenceConfigs = CountPersistenceConfigurations(i);
                    float configComplexity = MathF.Log(1 + persistenceConfigs) / MathF.Log(1 + totalConfigs);

                    // Part B: local gradient proxy (resource contrast with neighbours)
                    float gradSum = 0f;
                    int gradCount = 0;
                    float rHere = ResourceLocal[i];

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                        int ni = Idx(nx, ny);
                        if (!ActiveRegion[ni]) continue;
                        if (IsSink[ni]) continue;

                        gradSum += Math.Abs(rHere - ResourceLocal[ni]);
                        gradCount++;
                    }

                    float grad = (gradCount > 0) ? gradSum / gradCount : 0f;
                    // Scale gradient into [0..1] using soft saturation
                    float gradComplexity = grad / (grad + 1f);

                    // Combine into structural complexity metric
                    float structuralComplexity = Math.Clamp(0.7f * configComplexity + 0.3f * gradComplexity, 0f, 1f);

                    // Activity-scaled gain
                    float activity = Math.Clamp(inFlow, 0f, 1f);
                    ComplexityMetric[i] = Math.Clamp(structuralComplexity + ComplexityGainPerUse * activity, 0f, 1f);

                    // ---- RANDOM PERTURBATIONS (optional) ----
                    if (PerturbationProbability > 0f && rng.NextDouble() < PerturbationProbability)
                    {
                        // Random perturbation forces local resource collapse and complexity spike
                        ResourceLocal[i] = 0f;
                        ComplexityMetric[i] = Math.Clamp(ComplexityMetric[i] + PerturbationComplexity, 0f, 1f);
                        Active[i] = 0;
                        V[i] = 0f;
                        continue;
                    }

                    // ---- VIABILITY ----
                    float constraintBoost = 1f;
                    V[i] = ComputeViability(inFlow * constraintBoost, ResourceLocal[i], ComplexityMetric[i]);
                    
                    // ---- ACTIVE STATE ----
                    if (V[i] > 0f && ResourceLocal[i] > MinBudgetToPropagate)
                        Active[i] = 1;
                    else
                        Active[i] = 0;

                    // ---- BASELINE DECAY ----
                    if (DecayLoss > 0f)
                        ResourceLocal[i] = Math.Max(0f, ResourceLocal[i] - DecayLoss);

                    // ---- ZERO RESOURCE TICKS ----
                    if (ResourceLocal[i] <= 0f)
                        zeroResourceTicks[i]++;
                    else
                        zeroResourceTicks[i] = 0;
                }
            }
        }
    }
}
