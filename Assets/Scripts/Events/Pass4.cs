using System;
using UnityEngine;

namespace Assets.Scripts.Events
{
    /// <summary>
    /// Pass 4: Entropy Diffusion and Decay
    /// 
    /// Implements thermodynamic smoothing of entropy gradients across the grid.
    /// Uses discrete Laplacian operator for diffusion (analogous to heat equation).
    /// Enforces 2nd law: entropy spreads but never goes negative.
    /// Black holes maintain maximum entropy (Bekenstein-Hawking limit).
    /// </summary>
    public static class Pass4
    {
        // ============================================================================
        // PRIVATE CONSTANTS
        // ============================================================================

        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

        // ============================================================================
        // PUBLIC API: ENTROPY DIFFUSION
        // ============================================================================

        /// <summary>
        /// Diffuses entropy across the grid using discrete Laplacian operator.
        /// Applies decay and clamps to [0,1] range.
        /// 
        /// Physical analogy: Heat diffusion / thermalization.
        /// Mathematical form: ∂S/∂t = k∇²S - γS (diffusion - decay)
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="Entropy">Current entropy array (modified in place)</param>
        /// <param name="entropyNext">Buffer for next entropy state (work array)</param>
        /// <param name="EntropyDiffuseRate">Diffusion coefficient (k)</param>
        /// <param name="EntropyDecay">Decay rate per tick (γ)</param>
        /// <param name="IsBlackHole">Black hole mask (BHs have fixed entropy=1)</param>
        public static void EntropyDiffuse(
            int width, int height,
            float[] Entropy, float[] entropyNext,
            float EntropyDiffuseRate, float EntropyDecay,
            bool[] IsBlackHole)
        {
            int Idx(int x, int y) => y * width + x;

            // Phase 1: Compute diffused entropy for all cells (double buffering)
            ComputeDiffusedEntropy(
                width, height, Idx,
                Entropy, entropyNext, IsBlackHole,
                EntropyDiffuseRate, EntropyDecay);

            // Phase 2: Commit next state to current (swap buffers)
            CommitEntropyState(Entropy, entropyNext);
        }

        // ============================================================================
        // PRIVATE: ENTROPY DIFFUSION
        // ============================================================================

        /// <summary>
        /// Computes diffused entropy using 4-neighbor Laplacian operator.
        /// Black holes maintain maximum entropy (no diffusion).
        /// </summary>
        private static void ComputeDiffusedEntropy(
            int width, int height, Func<int, int, int> Idx,
            float[] Entropy, float[] entropyNext, bool[] IsBlackHole,
            float EntropyDiffuseRate, float EntropyDecay)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    // Black holes have fixed maximum entropy (Bekenstein-Hawking)
                    if (IsBlackHole[i])
                    {
                        entropyNext[i] = 1f;
                        continue;
                    }

                    // Compute Laplacian and apply diffusion
                    float diffusedEntropy = ComputeDiffusionAtCell(
                        i, x, y, width, height, Idx,
                        Entropy, IsBlackHole, EntropyDiffuseRate, EntropyDecay);

                    entropyNext[i] = diffusedEntropy;
                }
            }
        }

        /// <summary>
        /// Computes diffused entropy at a single cell using discrete Laplacian.
        /// ∇²S ≈ (S_north + S_south + S_west + S_east - 4*S_center)
        /// </summary>
        private static float ComputeDiffusionAtCell(
            int idx, int x, int y, int width, int height, Func<int, int, int> Idx,
            float[] Entropy, bool[] IsBlackHole,
            float EntropyDiffuseRate, float EntropyDecay)
        {
            float center = Entropy[idx];

            // Sample 4 neighbors (use center value if neighbor is out-of-bounds or BH)
            float north = SampleNeighborEntropy(x, y - 1, width, height, Idx, Entropy, IsBlackHole, center);
            float south = SampleNeighborEntropy(x, y + 1, width, height, Idx, Entropy, IsBlackHole, center);
            float west = SampleNeighborEntropy(x - 1, y, width, height, Idx, Entropy, IsBlackHole, center);
            float east = SampleNeighborEntropy(x + 1, y, width, height, Idx, Entropy, IsBlackHole, center);

            // Discrete Laplacian: ∇²S = (sum of neighbors) - 4*center
            float laplacian = (north + south + west + east - 4f * center);

            // Apply diffusion and decay
            float diffused = center + EntropyDiffuseRate * laplacian;
            diffused = Mathf.Max(0f, diffused - EntropyDecay);

            // Clamp to [0,1] (entropy is normalized)
            return Mathf.Clamp01(diffused);
        }

        /// <summary>
        /// Samples entropy at a neighbor location.
        /// Returns fallback value if neighbor is out-of-bounds or a black hole.
        /// </summary>
        private static float SampleNeighborEntropy(
            int nx, int ny, int width, int height, Func<int, int, int> Idx,
            float[] Entropy, bool[] IsBlackHole, float fallback)
        {
            // Out of bounds: use fallback (Neumann boundary condition)
            if (!IsInBounds(nx, ny, width, height))
                return fallback;

            int neighborIdx = Idx(nx, ny);

            // Black hole neighbor: use fallback (no diffusion across event horizon)
            if (IsBlackHole[neighborIdx])
                return fallback;

            return Entropy[neighborIdx];
        }

        // ============================================================================
        // PRIVATE: STATE COMMIT
        // ============================================================================

        /// <summary>
        /// Commits computed entropy state to main array (double-buffer swap).
        /// </summary>
        private static void CommitEntropyState(float[] Entropy, float[] entropyNext)
        {
            for (int i = 0; i < Entropy.Length; i++)
                Entropy[i] = entropyNext[i];
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
