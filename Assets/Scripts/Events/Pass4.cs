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
        /// Diffuses complexity metric across the grid using a simple Laplacian, then applies decay and clamps.
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="ComplexityMetric">Current complexity array</param>
        /// <param name="complexityNext">Buffer for next complexity state</param>
        /// <param name="ComplexityDiffusionRate">Diffusion rate</param>
        /// <param name="ComplexityDecay">Complexity decay per tick</param>
        public static void ComplexityDiffuse(
            int width, int height,
            float[] ComplexityMetric, float[] complexityNext,
            float ComplexityDiffusionRate, float ComplexityDecay,
            bool[] IsSink)
        {
            int Idx(int x, int y) => y * width + x;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    if (IsSink[i])
                    {
                        complexityNext[i] = 1f;
                        continue;
                    }

                    float c = ComplexityMetric[i];
                    float n = (y > 0 && !IsSink[i - width]) ? ComplexityMetric[i - width] : c;
                    float s = (y < height - 1 && !IsSink[i + width]) ? ComplexityMetric[i + width] : c;
                    float w = (x > 0 && !IsSink[i - 1]) ? ComplexityMetric[i - 1] : c;
                    float e = (x < width - 1 && !IsSink[i + 1]) ? ComplexityMetric[i + 1] : c;

                    float lap = (n + s + w + e - 4f * c);
                    float diffused = c + ComplexityDiffusionRate * lap;
                    diffused = Mathf.Max(0f, diffused - ComplexityDecay);
                    complexityNext[i] = Mathf.Clamp01(diffused);
                }
            }

            // Commit next state to current
            for (int i = 0; i < complexityNext.Length; i++)
                ComplexityMetric[i] = complexityNext[i];
        }
    }
}
