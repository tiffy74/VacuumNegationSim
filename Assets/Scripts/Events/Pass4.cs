using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public static class Pass4
    {
        /// <summary>
        /// Diffuses entropy across the grid using a simple Laplacian, then applies decay and clamps.
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="Entropy">Current entropy array</param>
        /// <param name="entropyNext">Buffer for next entropy state</param>
        /// <param name="EntropyDiffuseRate">Diffusion rate</param>
        /// <param name="EntropyDecay">Entropy decay per tick</param>
        public static void EntropyDiffuse(
            int width, int height,
            float[] Entropy, float[] entropyNext,
            float EntropyDiffuseRate, float EntropyDecay,
            bool[] IsBlackHole)
        {
            int Idx(int x, int y) => y * width + x;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    if (IsBlackHole[i])
                    {
                        entropyNext[i] = 1f;
                        continue;
                    }

                    float c = Entropy[i];
                    float n = (y > 0 && !IsBlackHole[i - width]) ? Entropy[i - width] : c;
                    float s = (y < height - 1 && !IsBlackHole[i + width]) ? Entropy[i + width] : c;
                    float w = (x > 0 && !IsBlackHole[i - 1]) ? Entropy[i - 1] : c;
                    float e = (x < width - 1 && !IsBlackHole[i + 1]) ? Entropy[i + 1] : c;

                    float lap = (n + s + w + e - 4f * c);
                    float diffused = c + EntropyDiffuseRate * lap;
                    diffused = Mathf.Max(0f, diffused - EntropyDecay);
                    entropyNext[i] = Mathf.Clamp01(diffused);
                }
            }

            // Commit next state to current
            for (int i = 0; i < entropyNext.Length; i++)
                Entropy[i] = entropyNext[i];
        }
    }
}
