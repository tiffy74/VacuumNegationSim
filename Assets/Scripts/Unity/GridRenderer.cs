using Assets.Scripts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Unity
{
    public sealed class GridRenderer
    {
        private readonly int width;
        private readonly int height;
        private readonly CellVisualiser[,] views;

        // Optional colour constants (can later move to a config asset)
        private readonly Color voidColor = new Color(0.15f, 0.0f, 0.25f, 1f);
        private readonly Color fieldFrontColor = Color.yellow;

        public GridRenderer(int width, int height, CellVisualiser[,] views)
        {
            this.width = width;
            this.height = height;
            this.views = views;
        }

        public void Render(GridState s)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = s.Idx(x, y);
                    var vis = views[x, y];
                    if (vis == null) continue;

                    // --- Rendering rules only ---
                    // These must NOT modify state.

                    // 1) No field → nullspace / void
                    if (!s.FieldPresent[i])
                    {
                        vis.SetColor(voidColor);
                        continue;
                    }

                    // 2) Field present but not yet viable → wave/front highlight
                    if (s.FieldPresent[i] && (s.Active[i] == 0 || s.V[i] <= 0f))
                    {
                        vis.SetColor(fieldFrontColor);
                        continue;
                    }

                    // 3) Active + viable → colour by viability + entropy
                    vis.SetViabilityWithEntropy(s.V[i], s.Entropy[i]);
                }
            }
        }
    }
}
