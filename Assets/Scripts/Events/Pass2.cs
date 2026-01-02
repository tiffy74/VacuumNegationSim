using System;
using UnityEngine;

namespace Assets.Scripts.Events
{
    /// <summary>
    /// Pass 2: Energy Application, Viability Computation, and Entropy Dynamics
    /// 
    /// Applies incoming energy to cells, computes viability (persistence criterion),
    /// calculates entropy from configuration complexity and spatial gradients,
    /// and enforces geometric constraints (black holes, field presence).
    /// </summary>
    public static class Pass2
    {
        // ============================================================================
        // PRIVATE CONSTANTS
        // ============================================================================
        
        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };
        private const int TotalConfigurations = 16; // 2^4 for 4-way neighbors

        // ============================================================================
        // PUBLIC API: APPLY ENERGY & COMPUTE VIABILITY
        // ============================================================================

        /// <summary>
        /// Main pass that applies incoming energy, computes viability and entropy,
        /// updates active state, and enforces black hole/field constraints.
        /// </summary>
        public static void ApplyAndViability(
            Func<int, int, int> Idx,
            int width, int height,
            float[] Nlocal, float[] Entropy, float[] V, byte[] Active, bool[] IsVacuum,
            float[] incoming, int[] zeroEnergyTicks,
            float MinBudgetToPropagate, float ActivationCost,
            ref float NGlobal, float NlocalMax,
            float EntropyGainPerUse, float DecayLoss, float EntropyPenalty,
            float VacuumEventProbability, float VacuumEventEntropy,
            Func<float, float, float, float> ComputeViability,
            Func<int, int> CountPersistenceConfigurations,
            int GridWidth, float PropagateFrac,
            bool[] FieldPresent, bool[] IsBlackHole,
            int[] FieldFirstTick, int[] EnergyFirstTick,
            int tick)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    // Enforce geometric constraints first
                    if (IsBlackHole[i])
                    {
                        EnforceBlackHoleInvariants(i, Nlocal, Entropy, V, Active, incoming);
                        continue;
                    }

                    if (!FieldPresent[i])
                    {
                        EnforceNoFieldInvariants(i, Nlocal, Entropy, V, Active, incoming);
                        continue;
                    }

                    if (IsVacuum[i])
                    {
                        EnforceVacuumInvariants(i, incoming, V, Active);
                        continue;
                    }

                    // Normal cell processing
                    ProcessNormalCell(
                        i, x, y, width, height, Idx,
                        Nlocal, Entropy, V, Active, incoming, zeroEnergyTicks,
                        MinBudgetToPropagate, ActivationCost, ref NGlobal, NlocalMax,
                        EntropyGainPerUse, DecayLoss, VacuumEventProbability, VacuumEventEntropy,
                        ComputeViability, CountPersistenceConfigurations,
                        PropagateFrac, FieldPresent, IsBlackHole, EnergyFirstTick, tick);
                }
            }
        }

        // ============================================================================
        // PRIVATE: GEOMETRIC CONSTRAINT ENFORCEMENT
        // ============================================================================

        /// <summary>
        /// Enforces black hole invariants: no energy, maximum entropy, inactive.
        /// Black holes represent collapsed configuration space where energy cannot persist.
        /// </summary>
        private static void EnforceBlackHoleInvariants(
            int idx, float[] Nlocal, float[] Entropy, float[] V, byte[] Active, float[] incoming)
        {
            Nlocal[idx] = 0f;
            Entropy[idx] = 1f; // Maximum entropy (Bekenstein-Hawking analogue)
            V[idx] = 0f;
            Active[idx] = 0;
            incoming[idx] = 0f;
        }

        /// <summary>
        /// Enforces constraints on cells without active configuration space.
        /// Energy cannot persist without field presence.
        /// </summary>
        private static void EnforceNoFieldInvariants(
            int idx, float[] Nlocal, float[] Entropy, float[] V, byte[] Active, float[] incoming)
        {
            Nlocal[idx] = 0f;
            V[idx] = 0f;
            Active[idx] = 0;
            incoming[idx] = 0f;
        }

        /// <summary>
        /// Enforces vacuum state: no incoming energy, zero viability, inactive.
        /// </summary>
        private static void EnforceVacuumInvariants(
            int idx, float[] incoming, float[] V, byte[] Active)
        {
            incoming[idx] = 0f;
            V[idx] = 0f;
            Active[idx] = 0;
        }

        // ============================================================================
        // PRIVATE: NORMAL CELL PROCESSING
        // ============================================================================

        /// <summary>
        /// Processes a normal cell: applies energy, computes entropy and viability,
        /// updates active state, applies decay.
        /// </summary>
        private static void ProcessNormalCell(
            int i, int x, int y, int width, int height, Func<int, int, int> Idx,
            float[] Nlocal, float[] Entropy, float[] V, byte[] Active, float[] incoming, int[] zeroEnergyTicks,
            float MinBudgetToPropagate, float ActivationCost, ref float NGlobal, float NlocalMax,
            float EntropyGainPerUse, float DecayLoss, float VacuumEventProbability, float VacuumEventEntropy,
            Func<float, float, float, float> ComputeViability,
            Func<int, int> CountPersistenceConfigurations,
            float PropagateFrac, bool[] FieldPresent, bool[] IsBlackHole, int[] EnergyFirstTick, int tick)
        {
            float inFlow = incoming[i];

            // Step 1: Apply incoming energy
            ApplyIncomingEnergy(i, inFlow, Nlocal, NlocalMax, EnergyFirstTick, tick);
            incoming[i] = 0f;

            // Step 2: Pay activation cost if cell is turning on
            PayActivationCostIfNeeded(i, inFlow, Active, ref NGlobal, ActivationCost, Nlocal, NlocalMax);

            // Step 3: Compute entropy from complexity and gradients
            float complexityEntropy = ComputeComplexityEntropy(
                i, x, y, width, height, Idx, Nlocal, FieldPresent, IsBlackHole, CountPersistenceConfigurations);

            Entropy[i] = ApplyEntropyDynamics(complexityEntropy, inFlow, EntropyGainPerUse);

            // Step 4: Handle random vacuum events
            if (TryTriggerVacuumEvent(VacuumEventProbability))
            {
                ApplyVacuumEventCollapse(i, Nlocal, Entropy, V, Active, VacuumEventEntropy);
                return;
            }

            // Step 5: Compute viability
            V[i] = ComputeViability(inFlow, Nlocal[i], Entropy[i]);

            // Step 6: Update active state
            UpdateActiveState(i, V, Nlocal, Active, MinBudgetToPropagate);

            // Step 7: Apply baseline decay
            ApplyBaselineDecay(i, Nlocal, DecayLoss);

            // Step 8: Track zero-energy duration
            UpdateZeroEnergyCounter(i, Nlocal, zeroEnergyTicks);
        }

        // ============================================================================
        // PRIVATE: ENERGY APPLICATION
        // ============================================================================

        /// <summary>
        /// Applies incoming energy to a cell, capped at local maximum.
        /// </summary>
        private static void ApplyIncomingEnergy(
            int idx, float inFlow, float[] Nlocal, float NlocalMax, int[] EnergyFirstTick, int tick)
        {
            if (inFlow > 0f)
            {
                Nlocal[idx] = Mathf.Min(NlocalMax, Nlocal[idx] + inFlow);
                
                if (EnergyFirstTick[idx] == -1)
                    EnergyFirstTick[idx] = tick;
            }
        }

        /// <summary>
        /// Draws energy from global pool when a cell activates.
        /// Optional: treat drawn energy as additional local usable energy.
        /// </summary>
        private static void PayActivationCostIfNeeded(
            int idx, float inFlow, byte[] Active, ref float NGlobal,
            float ActivationCost, float[] Nlocal, float NlocalMax)
        {
            if (Active[idx] == 0 && inFlow > 0f && NGlobal > 0f)
            {
                float draw = Mathf.Min(ActivationCost, NGlobal);
                NGlobal -= draw;

                // Optional: add drawn energy to local pool
                Nlocal[idx] = Mathf.Min(NlocalMax, Nlocal[idx] + draw);
            }
        }

        // ============================================================================
        // PRIVATE: ENTROPY COMPUTATION
        // ============================================================================

        /// <summary>
        /// Computes entropy from two components:
        /// 1. Configuration entropy (information-theoretic)
        /// 2. Gradient entropy (spatial organization)
        /// </summary>
        private static float ComputeComplexityEntropy(
            int i, int x, int y, int width, int height, Func<int, int, int> Idx,
            float[] Nlocal, bool[] FieldPresent, bool[] IsBlackHole,
            Func<int, int> CountPersistenceConfigurations)
        {
            // Part A: Configuration entropy (how many neighbor states sustain energy)
            float configEntropy = ComputeConfigurationEntropy(i, CountPersistenceConfigurations);

            // Part B: Gradient entropy (energy contrast with neighbors)
            float gradEntropy = ComputeGradientEntropy(
                i, x, y, width, height, Idx, Nlocal, FieldPresent, IsBlackHole);

            // Combine: configuration complexity dominates, gradient adds spatial structure info
            return Mathf.Clamp01(0.7f * configEntropy + 0.3f * gradEntropy);
        }

        /// <summary>
        /// Computes configuration-based entropy using persistence configuration count.
        /// Higher count = more resilient structure = higher entropy.
        /// </summary>
        private static float ComputeConfigurationEntropy(int idx, Func<int, int> CountPersistenceConfigurations)
        {
            int persistenceConfigs = CountPersistenceConfigurations(idx);
            return Mathf.Log(1 + persistenceConfigs) / Mathf.Log(1 + TotalConfigurations);
        }

        /// <summary>
        /// Computes gradient-based entropy from energy contrast with neighbors.
        /// Higher gradients = more spatial organization.
        /// </summary>
        private static float ComputeGradientEntropy(
            int i, int x, int y, int width, int height, Func<int, int, int> Idx,
            float[] Nlocal, bool[] FieldPresent, bool[] IsBlackHole)
        {
            float gradSum = 0f;
            int gradCount = 0;
            float nHere = Nlocal[i];

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (!IsInBounds(nx, ny, width, height)) continue;

                int ni = Idx(nx, ny);
                if (!FieldPresent[ni] || IsBlackHole[ni]) continue;

                gradSum += Mathf.Abs(nHere - Nlocal[ni]);
                gradCount++;
            }

            if (gradCount == 0)
                return 0f;

            float grad = gradSum / gradCount;
            
            // Soft saturation: grad / (grad + 1)
            return grad / (grad + 1f);
        }

        /// <summary>
        /// Applies entropy dynamics: base complexity + activity-scaled gain.
        /// </summary>
        private static float ApplyEntropyDynamics(float complexityEntropy, float inFlow, float EntropyGainPerUse)
        {
            float activity = Mathf.Clamp01(inFlow);
            return Mathf.Clamp01(complexityEntropy + EntropyGainPerUse * activity);
        }

        // ============================================================================
        // PRIVATE: VACUUM EVENTS
        // ============================================================================

        /// <summary>
        /// Checks if a random vacuum event occurs (rare stochastic collapse).
        /// </summary>
        private static bool TryTriggerVacuumEvent(float probability)
        {
            return probability > 0f && UnityEngine.Random.value < probability;
        }

        /// <summary>
        /// Applies vacuum event: collapse energy, spike entropy, deactivate cell.
        /// </summary>
        private static void ApplyVacuumEventCollapse(
            int idx, float[] Nlocal, float[] Entropy, float[] V, byte[] Active, float VacuumEventEntropy)
        {
            Nlocal[idx] = 0f;
            Entropy[idx] = Mathf.Clamp01(Entropy[idx] + VacuumEventEntropy);
            Active[idx] = 0;
            V[idx] = 0f;
        }

        // ============================================================================
        // PRIVATE: VIABILITY & ACTIVE STATE
        // ============================================================================

        /// <summary>
        /// Updates active state based on viability and energy criteria.
        /// Active = (viable AND has sufficient energy).
        /// </summary>
        private static void UpdateActiveState(
            int idx, float[] V, float[] Nlocal, byte[] Active, float MinBudgetToPropagate)
        {
            if (V[idx] > 0f && Nlocal[idx] > MinBudgetToPropagate)
                Active[idx] = 1;
            else
                Active[idx] = 0;
        }

        // ============================================================================
        // PRIVATE: DECAY & COUNTERS
        // ============================================================================

        /// <summary>
        /// Applies constant energy decay (dissipation/thermalization).
        /// </summary>
        private static void ApplyBaselineDecay(int idx, float[] Nlocal, float DecayLoss)
        {
            if (DecayLoss > 0f)
                Nlocal[idx] = Mathf.Max(0f, Nlocal[idx] - DecayLoss);
        }

        /// <summary>
        /// Tracks how many consecutive ticks a cell has had zero energy.
        /// Can be used for vacuum promotion logic.
        /// </summary>
        private static void UpdateZeroEnergyCounter(int idx, float[] Nlocal, int[] zeroEnergyTicks)
        {
            if (Nlocal[idx] <= 0f)
                zeroEnergyTicks[idx]++;
            else
                zeroEnergyTicks[idx] = 0;
        }

        // ============================================================================
        // PRIVATE: UTILITIES
        // ============================================================================

        /// <summary>
        /// Checks if coordinates are within grid bounds.
        /// </summary>
        private static bool IsInBounds(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
}
