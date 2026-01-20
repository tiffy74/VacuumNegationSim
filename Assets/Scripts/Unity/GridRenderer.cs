using UnityEngine;
using Viable.Engine.State;
using Viable.Engine.Execution;

namespace Assets.Scripts.Unity
{
    public enum RenderMode
    {
        Viability,
        Resource,
        Complexity
    }

    public sealed class GridRenderer
    {
        private readonly int _w;
        private readonly int _h;
        private readonly CellVisualiser[,] _views;

        private readonly Color _inactiveColor;
        private readonly Color _dormantRegionColor;
        private readonly Color _sinkColor = new Color(0.85f, 0f, 0.85f); // vivid magenta

        public float ViabilityColorScale = 40f;
        public bool ShowComplexityTint = false;
        
        public GridRenderer(int w, int h, CellVisualiser[,] views,
                            Color inactiveColor, Color dormantRegionColor,
                            bool showComplexityTint = false)
        {
            _w = w;
            _h = h;
            _views = views;
            _inactiveColor = inactiveColor;
            _dormantRegionColor = dormantRegionColor;
            ShowComplexityTint = showComplexityTint;
        }

        /// <summary>
        /// Rendering logic:
        /// - Sink regions: magenta
        /// - No active region: inactiveColor
        /// - Region arrived this tick: yellow (frontier)
        /// - Viable+active: viability coloring
        /// - Region but not viable: dormant color
        /// </summary>
        public void Render(GridState s, StepContext ctx, RenderMode mode)
        {
            // Compute per-frame maxima
            float vMax = 0f;
            float rMax = 0f;
            float complexityMax = 0f;
            for (int i = 0; i < s.V.Length; i++)
            {
                if (s.V[i] > vMax) vMax = s.V[i];
                if (s.ResourceLocal[i] > rMax) rMax = s.ResourceLocal[i];
                if (s.ComplexityMetric[i] > complexityMax) complexityMax = s.ComplexityMetric[i];
            }
            float invVmax = vMax > 0f ? 1f / vMax : 0f;
            float invRmax = rMax > 0f ? 1f / rMax : 0f;
            float invComplexityMax = complexityMax > 0f ? 1f / complexityMax : 0f;

            for (int y = 0; y < _h; y++)
            {
                for (int x = 0; x < _w; x++)
                {
                    var vis = _views[x, y];
                    if (vis == null) continue;

                    int i = s.Idx(x, y);

                    if (s.IsSink[i])
                    {
                        vis.SetColor(_sinkColor);
                        continue;
                    }

                    if (!s.ActiveRegion[i])
                    {
                        vis.SetColor(_inactiveColor);
                        continue;
                    }

                    // Frontier: arrival this tick or last tick
                    bool isArrival = s.RegionActivationTick[i] == ctx.Tick || s.RegionActivationTick[i] == ctx.Tick - 1;

                    // Frontier: has active region and at least one neighbor without active region (but not grid boundary)
                    bool isFrontier = false;
                    if (s.ActiveRegion[i])
                    {
                        // Check left neighbor (not at edge)
                        if (x > 0 && !s.ActiveRegion[s.Idx(x - 1, y)]) isFrontier = true;
                        // Check right neighbor (not at edge)
                        else if (x < _w - 1 && !s.ActiveRegion[s.Idx(x + 1, y)]) isFrontier = true;
                        // Check bottom neighbor (not at edge)
                        else if (y > 0 && !s.ActiveRegion[s.Idx(x, y - 1)]) isFrontier = true;
                        // Check top neighbor (not at edge)
                        else if (y < _h - 1 && !s.ActiveRegion[s.Idx(x, y + 1)]) isFrontier = true;
                    }

                    if (isArrival || isFrontier)
                    {
                        vis.SetColor(Color.yellow);
                        continue;
                    }

                    if (s.IsInactive[i])
                    {
                        vis.SetColor(_dormantRegionColor);
                        continue;
                    }

                    switch (mode)
                    {
                        case RenderMode.Resource:
                            {
                                float rNorm = Mathf.Clamp01(s.ResourceLocal[i] * invRmax);
                                Color resourceColor = Color.Lerp(_dormantRegionColor, Color.red, rNorm);
                                vis.SetColor(resourceColor);
                                break;
                            }
                        case RenderMode.Complexity:
                            {
                                float complexityNorm = Mathf.Clamp01(s.ComplexityMetric[i] * invComplexityMax);
                                Color complexityColor = Color.Lerp(Color.blue, Color.magenta, complexityNorm);
                                vis.SetColor(complexityColor);
                                break;
                            }
                        default:
                            {
                                if (s.Active[i] == 1 && s.V[i] > 0f)
                                {
                                    float vNorm = Mathf.Clamp01(s.V[i] * invVmax);
                                    if (ShowComplexityTint)
                                        vis.SetViabilityWithComplexity(vNorm, s.ComplexityMetric[i]);
                                    else
                                        vis.SetViability(vNorm);
                                }
                                else
                                {
                                    // Active region present but not viable/active
                                    vis.SetColor(_dormantRegionColor);
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}
