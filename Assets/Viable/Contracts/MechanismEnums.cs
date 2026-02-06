namespace Viable.Contracts
{
    /// <summary>
    /// Adjacency mode for neighbor connectivity.
    /// Stage 13.9: Determines how neighbors are defined (edge-only vs edge+vertex).
    /// </summary>
    public enum AdjacencyMode
    {
        /// <summary>
        /// Only neighbors sharing an edge (true topology connectivity).
        /// Triangle: 3, Rectangle: 4, Hexagon: 6
        /// </summary>
        EdgeOnly = 0,

        /// <summary>
        /// Neighbors sharing an edge OR vertex (extended connectivity).
        /// Triangle: 6, Rectangle: 8, Hexagon: 6
        /// </summary>
        EdgeAndVertex = 1
    }

    /// <summary>
    /// Selects the inflow mechanism for resource delivery to the grid.
    /// Stage 13: Behavioral configuration.
    /// </summary>
    public enum InflowMode
    {
        /// <summary>
        /// Current default: uniform inflow across active cells.
        /// </summary>
        Uniform = 0,

        /// <summary>
        /// Inflow only at grid boundaries.
        /// </summary>
        Boundary = 1,

        /// <summary>
        /// Fixed point sources at specified locations.
        /// </summary>
        PointSources = 2,

        /// <summary>
        /// Moving sources (future extension).
        /// </summary>
        MovingSources = 3,

        /// <summary>
        /// Regional clusters (future extension).
        /// </summary>
        RegionalClusters = 4
    }

    /// <summary>
    /// Selects the outflow/loss mechanism.
    /// Stage 13: Behavioral configuration.
    /// </summary>
    public enum OutflowMode
    {
        /// <summary>
        /// Current default: uniform decay/loss.
        /// </summary>
        Uniform = 0,

        /// <summary>
        /// Loss at boundaries only.
        /// </summary>
        BoundaryLeak = 1,

        /// <summary>
        /// Loss biased toward sink regions.
        /// </summary>
        SinkBiased = 2
    }

    /// <summary>
    /// Selects the diffusion neighborhood and pattern.
    /// Stage 13: Behavioral configuration.
    /// </summary>
    public enum DiffusionMode
    {
        /// <summary>
        /// Current default: Von Neumann 4-neighbor.
        /// </summary>
        VonNeumann4 = 0,

        /// <summary>
        /// Moore 8-neighbor (includes diagonals).
        /// </summary>
        Moore8 = 1,

        /// <summary>
        /// Radius-2 neighborhood (future extension).
        /// </summary>
        Radius2 = 2,

        /// <summary>
        /// Anisotropic diffusion with directional bias.
        /// </summary>
        Anisotropic = 3
    }

    /// <summary>
    /// Selects boundary behavior for cells at grid edges.
    /// Stage 13: Behavioral configuration.
    /// </summary>
    public enum BoundaryMode
    {
        /// <summary>
        /// Current default: absorbing boundaries (loss at edges).
        /// </summary>
        Absorbing = 0,

        /// <summary>
        /// Reflecting boundaries (no loss).
        /// </summary>
        Reflecting = 1,

        /// <summary>
        /// Periodic wrap (toroidal topology).
        /// </summary>
        PeriodicWrap = 2
    }

    /// <summary>
    /// Selects the viability evaluation rule.
    /// Stage 13: Behavioral configuration.
    /// </summary>
    public enum ViabilityRule
    {
        /// <summary>
        /// Current default: hard threshold (on/off).
        /// </summary>
        HardThreshold = 0,

        /// <summary>
        /// Soft threshold with sigmoid transition.
        /// </summary>
        SoftThresholdSigmoid = 1,

        /// <summary>
        /// Hysteresis with separate on/off thresholds.
        /// </summary>
        Hysteresis = 2,

        /// <summary>
        /// Local plus regional contribution (future extension).
        /// </summary>
        LocalPlusRegional = 3
    }

    /// <summary>
    /// Selects region management behavior (if region logic is active).
    /// Stage 13: Behavioral configuration.
    /// </summary>
    public enum RegionMode
    {
        /// <summary>
        /// Current default region logic.
        /// </summary>
        CurrentDefault = 0,

        /// <summary>
        /// Merge regions by similarity threshold.
        /// </summary>
        MergeBySimilarity = 1,

        /// <summary>
        /// Aggressive merging.
        /// </summary>
        MergeAggressive = 2,

        /// <summary>
        /// Fragment under stress (future extension).
        /// </summary>
        FragmentUnderStress = 3
    }

    /// <summary>
    /// Selects the grid topology type - the 3 fundamental regular tessellations.
    /// Stage 13: Basic shape groups for comprehensive spatial behavior comparison.
    /// </summary>
    public enum TopologyMode
    {
        /// <summary>
        /// Rectangular (square) grid - 4 neighbors, Manhattan distance.
        /// Current default: implemented and well-tested.
        /// </summary>
        RectGrid = 0,

        /// <summary>
        /// Triangular grid - 3 or 6 neighbors depending on connectivity.
        /// Stage 13.7: Fundamental triangular tessellation.
        /// </summary>
        TriGrid = 1,

        /// <summary>
        /// Hexagonal grid - 6 neighbors, uniform distance, isotropic diffusion.
        /// Stage 13.8: Most symmetric regular tessellation.
        /// </summary>
        HexGrid = 2,

        /// <summary>
        /// Future: Masked domain with shaped constraints (circular, corridor, etc.)
        /// Stage 13.9: Constrained geometries within rectangular grid.
        /// </summary>
        MaskedDomain = 3,

        /// <summary>
        /// Future: Graph-based domain for non-spatial networks.
        /// Stage 14: Arbitrary connectivity graphs.
        /// </summary>
        GraphDomain = 4
    }

    /// <summary>
    /// Selects the mask shape when TopologyMode = MaskedDomain.
    /// Stage 13: Behavioral configuration.
    /// </summary>
    public enum MaskShape
    {
        /// <summary>
        /// No mask (full rectangular domain).
        /// </summary>
        Rectangle = 0,

        /// <summary>
        /// Circular domain.
        /// </summary>
        Circle = 1,

        /// <summary>
        /// Ring/annulus shape.
        /// </summary>
        Ring = 2,

        /// <summary>
        /// Corridor/channel shape.
        /// </summary>
        Corridor = 3,

        /// <summary>
        /// Maze-like pattern (future extension).
        /// </summary>
        MazeLike = 4,

        /// <summary>
        /// Percolation holes (random connectivity).
        /// </summary>
        PercolationHoles = 5
    }

    /// <summary>
    /// Selects adaptive refinement behavior.
    /// Stage 13: Behavioral configuration.
    /// </summary>
    public enum RefinementMode
    {
        /// <summary>
        /// No adaptive refinement.
        /// </summary>
        None = 0,

        /// <summary>
        /// Threshold-based hierarchical refinement.
        /// </summary>
        ThresholdRefinement = 1,

        /// <summary>
        /// Dynamic graph growth (future extension).
        /// </summary>
        DynamicGraphGrowth = 2
    }
}
