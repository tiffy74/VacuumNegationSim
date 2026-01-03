using UnityEngine;
using Assets.Scripts.Domain;

namespace Assets.Scripts.Unity
{
    /// <summary>
    /// Grid-Wide Rendering Orchestrator
    /// 
    /// Translates simulation state (GridState) into visual representation by coordinating
    /// color updates across all cell visualizers. Implements multiple render modes and
    /// handles special visual states (boundaries, frontiers, black holes).
    /// 
    /// Design: Adapter pattern bridging pure simulation data with Unity's rendering system.
    /// </summary>
    public sealed class GridRenderer
    {
        // ============================================================================
        // PRIVATE CONSTANTS
        // ============================================================================
        
        private static readonly Color BlackHoleColor = new Color(0.85f, 0f, 0.85f); // Vivid magenta

        // ============================================================================
        // PRIVATE FIELDS: GRID CONFIGURATION
        // ============================================================================
        
        private readonly int _w;
        private readonly int _h;
        private readonly CellVisualiser[,] _views;

        // ============================================================================
        // PRIVATE FIELDS: COLOR SCHEME
        // ============================================================================
        
        private readonly Color _voidColor;         // Cells without configuration space
        private readonly Color _fieldDimColor;     // Inactive field cells

        // ============================================================================
        // PUBLIC PROPERTIES: RENDER SETTINGS
        // ============================================================================
        
        /// <summary>
        /// Multiplier for viability display (legacy - currently unused due to normalization).
        /// </summary>
        public float ViabilityColorScale = 40f;
        
        /// <summary>
        /// Enable entropy-based color tinting in Viability mode.
        /// </summary>
        public bool ShowEntropyTint = false;

        // ============================================================================
        // CONSTRUCTOR
        // ============================================================================
        
        /// <summary>
        /// Initializes grid renderer with dimensions, cell references, and color scheme.
        /// </summary>
        /// <param name="w">Grid width</param>
        /// <param name="h">Grid height</param>
        /// <param name="views">2D array of CellVisualiser components (one per cell)</param>
        /// <param name="voidColor">Color for void regions (no configuration space)</param>
        /// <param name="fieldDimColor">Color for inactive field cells</param>
        /// <param name="showEntropyTint">Enable entropy overlay in Viability mode</param>
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

        // ============================================================================
        // PUBLIC API: RENDERING
        // ============================================================================

        /// <summary>
        /// Main rendering method - updates all cell visuals based on current simulation state.
        /// 
        /// Rendering Priority (high to low):
        /// 1. Black holes (magenta) - collapsed configuration space
        /// 2. Void regions (dark purple) - no field present
        /// 3. Arrival ring (yellow) - field expansion wavefront
        /// 4. Frontier cells (yellow) - field boundary
        /// 5. Vacuum cells (dim purple) - permanent vacuum state
        /// 6. Mode-specific rendering (viability/energy/entropy)
        /// 
        /// Uses per-frame normalization for adaptive contrast.
        /// </summary>
        /// <param name="s">Current simulation state</param>
        /// <param name="ctx">Simulation context (tick counter, etc.)</param>
        /// <param name="mode">Visualization mode</param>
        public void Render(GridState s, SimContext ctx, RenderMode mode)
        {
            // Phase 1: Compute per-frame normalization factors
            var normalization = ComputeNormalizationFactors(s);

            // Phase 2: Render each cell based on priority rules
            RenderAllCells(s, ctx, mode, normalization);
        }

        // ============================================================================
        // PRIVATE: NORMALIZATION
        // ============================================================================

        /// <summary>
        /// Container for per-frame normalization factors.
        /// Enables adaptive contrast as simulation evolves.
        /// </summary>
        private struct NormalizationFactors
        {
            public float InvViabilityMax;
            public float InvEnergyMax;
            public float InvEntropyMax;
        }

        /// <summary>
        /// Computes inverse of maximum values for normalization.
        /// Prevents color saturation by adapting to current dynamic range.
        /// </summary>
        private NormalizationFactors ComputeNormalizationFactors(GridState s)
        {
            float vMax = FindMaximumValue(s.V);
            float eMax = FindMaximumValue(s.Nlocal);
            float entropyMax = FindMaximumValue(s.Entropy);

            return new NormalizationFactors
            {
                InvViabilityMax = vMax > 0f ? 1f / vMax : 0f,
                InvEnergyMax = eMax > 0f ? 1f / eMax : 0f,
                InvEntropyMax = entropyMax > 0f ? 1f / entropyMax : 0f
            };
        }

        /// <summary>
        /// Finds maximum value in an array (simple linear scan).
        /// </summary>
        private float FindMaximumValue(float[] array)
        {
            float max = 0f;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > max)
                    max = array[i];
            }
            return max;
        }

        // ============================================================================
        // PRIVATE: CELL RENDERING
        // ============================================================================

        /// <summary>
        /// Renders all cells in the grid according to priority rules.
        /// </summary>
        private void RenderAllCells(GridState s, SimContext ctx, RenderMode mode, NormalizationFactors norm)
        {
            for (int y = 0; y < _h; y++)
            {
                for (int x = 0; x < _w; x++)
                {
                    RenderSingleCell(x, y, s, ctx, mode, norm);
                }
            }
        }

        /// <summary>
        /// Renders a single cell based on priority rules and render mode.
        /// </summary>
        private void RenderSingleCell(int x, int y, GridState s, SimContext ctx, RenderMode mode, NormalizationFactors norm)
        {
            var vis = _views[x, y];
            if (vis == null) return;

            int i = s.Idx(x, y);

            // Priority 1: Black Holes (always magenta, regardless of mode)
            if (s.IsBlackHole[i])
            {
                vis.SetColor(BlackHoleColor);
                return;
            }

            // Priority 2: Void (no configuration space)
            if (!s.FieldPresent[i])
            {
                vis.SetColor(_voidColor);
                return;
            }

            // Priority 3: Arrival Ring (field expansion wavefront)
            if (IsArrivalRing(i, s, ctx))
            {
                vis.SetColor(Color.yellow);
                return;
            }

            // Priority 4: Frontier (field boundary)
            if (IsFrontierCell(x, y, i, s))
            {
                vis.SetColor(Color.yellow);
                return;
            }

            // Priority 5: Vacuum (permanent vacuum state)
            if (s.IsVacuum[i])
            {
                vis.SetColor(_fieldDimColor);
                return;
            }

            // Priority 6: Mode-specific rendering
            RenderByMode(i, vis, s, mode, norm);
        }

        // ============================================================================
        // PRIVATE: SPECIAL STATE DETECTION
        // ============================================================================

        /// <summary>
        /// Checks if cell is part of the arrival ring (field expansion wavefront).
        /// Creates visible 2-tick ring as field propagates.
        /// </summary>
        private bool IsArrivalRing(int idx, GridState s, SimContext ctx)
        {
            int arrivalTick = s.FieldFirstTick[idx];
            return arrivalTick == ctx.Tick || arrivalTick == ctx.Tick - 1;
        }

        /// <summary>
        /// Checks if cell is on the frontier (has field AND at least one neighbor without field).
        /// Forms the boundary between active configuration space and void.
        /// </summary>
        private bool IsFrontierCell(int x, int y, int idx, GridState s)
        {
            if (!s.FieldPresent[idx])
                return false;

            // Check 4-way neighbors (N, S, E, W)
            if (IsAtGridEdge(x, y))
                return true;

            if (!s.FieldPresent[s.Idx(x - 1, y)]) return true; // West
            if (!s.FieldPresent[s.Idx(x + 1, y)]) return true; // East
            if (!s.FieldPresent[s.Idx(x, y - 1)]) return true; // North
            if (!s.FieldPresent[s.Idx(x, y + 1)]) return true; // South

            return false;
        }

        /// <summary>
        /// Checks if cell is at the grid edge (boundary condition).
        /// </summary>
        private bool IsAtGridEdge(int x, int y)
        {
            return x == 0 || x == _w - 1 || y == 0 || y == _h - 1;
        }

        // ============================================================================
        // PRIVATE: MODE-SPECIFIC RENDERING
        // ============================================================================

        /// <summary>
        /// Renders cell according to selected visualization mode.
        /// </summary>
        private void RenderByMode(int idx, CellVisualiser vis, GridState s, RenderMode mode, NormalizationFactors norm)
        {
            switch (mode)
            {
                case RenderMode.Energy:
                    RenderEnergyMode(idx, vis, s, norm);
                    break;

                case RenderMode.Entropy:
                    RenderEntropyMode(idx, vis, s, norm);
                    break;

                default: // Viability (default)
                    RenderViabilityMode(idx, vis, s, norm);
                    break;
            }
        }

        /// <summary>
        /// Renders cell in Energy mode (heat map from dim purple to red).
        /// Shows local energy density distribution.
        /// </summary>
        private void RenderEnergyMode(int idx, CellVisualiser vis, GridState s, NormalizationFactors norm)
        {
            float energyNormalized = Mathf.Clamp01(s.Nlocal[idx] * norm.InvEnergyMax);
            Color energyColor = Color.Lerp(_fieldDimColor, Color.red, energyNormalized);
            vis.SetColor(energyColor);
        }

        /// <summary>
        /// Renders cell in Entropy mode (gradient from blue to magenta).
        /// Shows entropy/complexity distribution.
        /// </summary>
        private void RenderEntropyMode(int idx, CellVisualiser vis, GridState s, NormalizationFactors norm)
        {
            float entropyNormalized = Mathf.Clamp01(s.Entropy[idx] * norm.InvEntropyMax);
            Color entropyColor = Color.Lerp(Color.blue, Color.magenta, entropyNormalized);
            vis.SetColor(entropyColor);
        }

        /// <summary>
        /// Renders cell in Viability mode (default - shows persistence criterion).
        /// Active cells use viability gradient (black → blue → cyan → green → yellow → amber).
        /// Inactive cells show dim field color.
        /// Optional entropy tinting available.
        /// </summary>
        private void RenderViabilityMode(int idx, CellVisualiser vis, GridState s, NormalizationFactors norm)
        {
            bool isActive = s.Active[idx] == 1;
            bool isViable = s.V[idx] > 0f;

            if (isActive && isViable)
            {
                RenderViableCell(idx, vis, s, norm);
            }
            else
            {
                // Field present but not viable/active - show dim field
                vis.SetColor(_fieldDimColor);
            }
        }

        /// <summary>
        /// Renders an active, viable cell with viability gradient.
        /// Optionally applies entropy tinting if enabled.
        /// </summary>
        private void RenderViableCell(int idx, CellVisualiser vis, GridState s, NormalizationFactors norm)
        {
            float viabilityNormalized = Mathf.Clamp01(s.V[idx] * norm.InvViabilityMax);

            if (ShowEntropyTint)
            {
                vis.SetViabilityWithEntropy(viabilityNormalized, s.Entropy[idx]);
            }
            else
            {
                vis.SetViability(viabilityNormalized);
            }
        }
    }

    // ============================================================================
    // ENUMS
    // ============================================================================

    /// <summary>
    /// Available visualization modes for grid rendering.
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        /// Shows viability (persistence criterion) as color gradient.
        /// Default mode - most informative for understanding simulation dynamics.
        /// </summary>
        Viability,

        /// <summary>
        /// Shows local energy density as heat map (dim purple → red).
        /// Useful for tracking energy distribution and flow.
        /// </summary>
        Energy,

        /// <summary>
        /// Shows entropy/complexity as gradient (blue → magenta).
        /// Useful for understanding information-theoretic aspects.
        /// </summary>
        Entropy
    }
}
