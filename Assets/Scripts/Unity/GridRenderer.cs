using UnityEngine;
using Assets.Scripts.Domain; // adjust if GridState/SimContext live elsewhere

namespace Assets.Scripts.Unity
{
    public enum RenderMode
    {
        Viability,
        Energy,
        Entropy
    }

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
        public void Render(GridState s, SimContext ctx, RenderMode mode)
        {
            // Compute per-frame maxima
            float vMax = 0f;
            float eMax = 0f;
            float entropyMax = 0f;
            for (int i = 0; i < s.V.Length; i++)
            {
                if (s.V[i] > vMax) vMax = s.V[i];
                if (s.Nlocal[i] > eMax) eMax = s.Nlocal[i];
                if (s.Entropy[i] > entropyMax) entropyMax = s.Entropy[i];
            }
            float invVmax = vMax > 0f ? 1f / vMax : 0f;
            float invEmax = eMax > 0f ? 1f / eMax : 0f;
            float invEntropyMax = entropyMax > 0f ? 1f / entropyMax : 0f;

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

                    if (s.IsVacuum[i])
                    {
                        vis.SetColor(_fieldDimColor);
                        continue;
                    }

                    switch (mode)
                    {
                        case RenderMode.Energy:
                            {
                                float eNorm = Mathf.Clamp01(s.Nlocal[i] * invEmax);
                                Color energyColor = Color.Lerp(_fieldDimColor, Color.red, eNorm);
                                vis.SetColor(energyColor);
                                break;
                            }
                        case RenderMode.Entropy:
                            {
                                float entropyNorm = Mathf.Clamp01(s.Entropy[i] * invEntropyMax);
                                Color entropyColor = Color.Lerp(Color.blue, Color.magenta, entropyNorm);
                                vis.SetColor(entropyColor);
                                break;
                            }
                        default:
                            {
                                if (s.Active[i] == 1 && s.V[i] > 0f)
                                {
                                    float vNorm = Mathf.Clamp01(s.V[i] * invVmax); // per-frame normalization
                                    if (ShowEntropyTint)
                                        vis.SetViabilityWithEntropy(vNorm, s.Entropy[i]);
                                    else
                                        vis.SetViability(vNorm);
                                }
                                else
                                {
                                    // Field present but not viable/active
                                    vis.SetColor(_fieldDimColor);
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}
