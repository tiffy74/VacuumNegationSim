using System.Collections.Generic;

namespace Viable.Contracts
{
    /// <summary>
    /// Configuration for engine mechanisms and behavioral modes.
    /// Stage 13: Enables presets to change behavior, not just numeric parameters.
    /// Defaults preserve current behavior exactly.
    /// </summary>
    public sealed class EngineConfig
    {
        /// <summary>
        /// Model/rule family identifier. Default: "default" (current behavior).
        /// </summary>
        public string ModelId { get; set; } = "default";

        /// <summary>
        /// Phase pipeline identifier. Default: "default" (current phase order).
        /// </summary>
        public string PhaseSetId { get; set; } = "default";

        // ===== Mechanism Selectors =====

        /// <summary>
        /// Inflow mechanism. Default: Uniform (current behavior).
        /// </summary>
        public InflowMode InflowMode { get; set; } = InflowMode.Uniform;

        /// <summary>
        /// Outflow mechanism. Default: Uniform (current behavior).
        /// </summary>
        public OutflowMode OutflowMode { get; set; } = OutflowMode.Uniform;

        /// <summary>
        /// Diffusion neighborhood. Default: VonNeumann4 (current behavior).
        /// </summary>
        public DiffusionMode DiffusionMode { get; set; } = DiffusionMode.VonNeumann4;

        /// <summary>
        /// Boundary behavior. Default: Absorbing (current behavior).
        /// </summary>
        public BoundaryMode BoundaryMode { get; set; } = BoundaryMode.Absorbing;

        /// <summary>
        /// Viability evaluation rule. Default: HardThreshold (current behavior).
        /// </summary>
        public ViabilityRule ViabilityRule { get; set; } = ViabilityRule.HardThreshold;

        /// <summary>
        /// Region management mode. Default: CurrentDefault.
        /// </summary>
        public RegionMode RegionMode { get; set; } = RegionMode.CurrentDefault;

        /// <summary>
        /// Grid topology type. Default: RectGrid (current behavior).
        /// </summary>
        public TopologyMode TopologyMode { get; set; } = TopologyMode.RectGrid;

        /// <summary>
        /// Mask shape when TopologyMode = MaskedDomain. Default: Rectangle (no mask).
        /// </summary>
        public MaskShape MaskShape { get; set; } = MaskShape.Rectangle;

        /// <summary>
        /// Adaptive refinement mode. Default: None (current behavior - no refinement).
        /// </summary>
        public RefinementMode RefinementMode { get; set; } = RefinementMode.None;

        // ===== Mechanism Parameters =====

        /// <summary>
        /// Point source locations and strengths for InflowMode.PointSources.
        /// Format: { x, y, strength }
        /// Empty list = no point sources.
        /// </summary>
        public List<PointSourceConfig> PointSources { get; set; } = new List<PointSourceConfig>();

        /// <summary>
        /// Anisotropic diffusion direction (unit vector) for DiffusionMode.Anisotropic.
        /// Default: (0, 0) = isotropic fallback.
        /// </summary>
        public double AnisotropyDirectionX { get; set; } = 0.0;
        public double AnisotropyDirectionY { get; set; } = 0.0;

        /// <summary>
        /// Anisotropic diffusion bias strength [0, 1]. Default: 0 (isotropic).
        /// </summary>
        public double AnisotropyBias { get; set; } = 0.0;

        /// <summary>
        /// Hysteresis ON threshold (ViabilityRule.Hysteresis). Default: 0 (uses HardThreshold).
        /// </summary>
        public double HysteresisOnThreshold { get; set; } = 0.0;

        /// <summary>
        /// Hysteresis OFF threshold (ViabilityRule.Hysteresis). Default: 0 (uses HardThreshold).
        /// </summary>
        public double HysteresisOffThreshold { get; set; } = 0.0;

        // ===== Mask Parameters =====

        /// <summary>
        /// Mask radius for Circle/Ring shapes (in grid units). Default: 0 (no mask).
        /// </summary>
        public double MaskRadius { get; set; } = 0.0;

        /// <summary>
        /// Inner radius for Ring shape (in grid units). Default: 0.
        /// </summary>
        public double MaskInnerRadius { get; set; } = 0.0;

        /// <summary>
        /// Corridor width for Corridor shape (in grid units). Default: 0.
        /// </summary>
        public double CorridorWidth { get; set; } = 0.0;

        /// <summary>
        /// Hole probability for PercolationHoles [0, 1]. Default: 0 (no holes).
        /// </summary>
        public double HoleProbability { get; set; } = 0.0;

        // ===== Refinement Parameters =====

        /// <summary>
        /// Disorder threshold for triggering refinement. Default: 0 (no refinement).
        /// Stage 13.9: Stub for future adaptive refinement.
        /// </summary>
        public double RefinementDisorderThreshold { get; set; } = 0.0;

        /// <summary>
        /// Maximum refinement depth (hierarchy levels). Default: 0 (no refinement).
        /// Stage 13.9: Stub for future adaptive refinement.
        /// </summary>
        public int MaxRefinementDepth { get; set; } = 0;

        /// <summary>
        /// Maximum subgrid size (cells per refined region). Default: 0.
        /// Stage 13.9: Stub for future adaptive refinement.
        /// </summary>
        public int MaxSubgridSize { get; set; } = 0;

        /// <summary>
        /// Base number of subcells per dimension for initial refinement. Default: 2.
        /// Stage 13.9: Stub for future adaptive refinement.
        /// When a cell is refined, it's subdivided into BaseSubcells × BaseSubcells subgrid.
        /// </summary>
        public int BaseSubcells { get; set; } = 2;

        /// <summary>
        /// Growth rate for subcell count based on lineage depth. Default: 0.0 (constant).
        /// Stage 13.9: Stub for future adaptive refinement.
        /// Actual subcells = BaseSubcells + (AlphaSubcellsPerLineage × lineageDepth).
        /// </summary>
        public double AlphaSubcellsPerLineage { get; set; } = 0.0;
    }

    /// <summary>
    /// Configuration for a single point source (InflowMode.PointSources).
    /// </summary>
    public sealed class PointSourceConfig
    {
        /// <summary>
        /// X coordinate (grid cell).
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y coordinate (grid cell).
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Resource inflow strength (per step).
        /// </summary>
        public double Strength { get; set; }
    }
}
