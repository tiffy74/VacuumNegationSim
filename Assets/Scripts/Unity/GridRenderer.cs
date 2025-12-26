using UnityEngine;
using Assets.Scripts.Domain; // adjust if GridState/SimContext live elsewhere

namespace Assets.Scripts.Unity
{
    public sealed class GridRenderer
    {
        private readonly int _w;
        private readonly int _h;
        private readonly CellVisualiser[,] _views;

        // You can inject colours from SimulationController if you prefer.
        private readonly Color _voidColor;
        private readonly Color _fieldDimColor;
        private readonly Color _blackHoleColor = new Color(0.85f, 0f, 0.85f); // vivid magenta

        public float ViabilityColorScale = 40f; // because your V is ~0.01-0.04 typically
        public bool ShowEntropyTint = false;
        
        public GridRenderer(int w, int h, CellVisualiser[,] views,
                            Color voidColor, Color fieldDimColor,
                            bool showEntropyTint = false)
        {
            _w = w;
            _h = h;
            _views = views;
            _voidColor = voidColor;
            _fieldDimColor = fieldDimColor;
            ShowEntropyTint = showEntropyTint;
        }

        /// <summary>
        /// Rendering
        /// - Black hole: black
        /// - No field: voidColor
        /// - Field arrived this tick: yellow (thin front ring)
        /// - Viable+active: viability colouring
        /// - Field but not viable: dim field colour
        /// - Vacuum: dim field colour (or special colour if you prefer)
        /// </summary>
        public void Render(GridState s, SimContext ctx)
        {
            // Compute per-frame viability max (positive values only)
            float vMax = 0f;
            for (int i = 0; i < s.V.Length; i++)
            {
                if (s.V[i] > vMax) vMax = s.V[i];
            }
            float invVmax = vMax > 0f ? 1f / vMax : 0f;

            for (int y = 0; y < _h; y++)
            {
                for (int x = 0; x < _w; x++)
                {
                    var vis = _views[x, y];
                    if (vis == null) continue;

                    int i = s.Idx(x, y);

                    if (s.IsBlackHole[i])
                    {
                        vis.SetColor(_blackHoleColor);
                        continue;
                    }

                    if (!s.FieldPresent[i])
                    {
                        vis.SetColor(_voidColor);
                        continue;
                    }

                    // Thin ring: arrival this tick or last tick
                    bool isArrival = s.FieldFirstTick[i] == ctx.Tick || s.FieldFirstTick[i] == ctx.Tick - 1;

                    // Frontier: has the field and at least one neighbour without the field (or out of bounds)
                    bool isFrontier = false;
                    if (s.FieldPresent[i])
                    {
                        // 4-way neighbours
                        if (x == 0 || !s.FieldPresent[s.Idx(x - 1, y)]) isFrontier = true;
                        else if (x == _w - 1 || !s.FieldPresent[s.Idx(x + 1, y)]) isFrontier = true;
                        else if (y == 0 || !s.FieldPresent[s.Idx(x, y - 1)]) isFrontier = true;
                        else if (y == _h - 1 || !s.FieldPresent[s.Idx(x, y + 1)]) isFrontier = true;
                    }

                    if (isArrival || isFrontier)
                    {
                        vis.SetColor(Color.yellow);
                        continue;
                    }

                    //// Persist yellow once the field has ever arrived
                    //if (s.FieldPresent[i] && s.FieldFirstTick[i] >= 0)
                    //{
                    //    vis.SetColor(Color.yellow);
                    //    continue;
                    //}

                    if (s.IsVacuum[i])
                    {
                        vis.SetColor(_fieldDimColor);
                        continue;
                    }

                    if (s.Active[i] == 1 && s.V[i] > 0f)
                    {
                        float vNorm = Mathf.Clamp01(s.V[i] * invVmax); // per-frame normalization
                        if (ShowEntropyTint)
                            vis.SetViabilityWithEntropy(vNorm, s.Entropy[i]);
                        else
                            vis.SetViability(vNorm);
                        continue;
                    }

                    // Field present but not viable/active
                    vis.SetColor(_fieldDimColor);
                }
            }
        }
    }
}
