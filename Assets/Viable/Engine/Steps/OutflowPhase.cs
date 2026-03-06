using System;
using Viable.Engine.State;
using Viable.Engine.Execution;
using Viable.Engine.Logic;
using Viable.Engine.ExpansionModels; // Stage 14: For IExpansionModel
using Viable.Contracts;

namespace Viable.Engine.Steps
{
    /// <summary>
    /// Phase 1: Gather outflow from active cells and distribute to neighbors.
    /// Handles sink formation at boundaries via charge accumulation.
    /// Updated for multi-topology support (Triangle/Rectangle/Hexagon).
    /// Stage 14: Uses IExpansionModel for sink formation decisions.
    /// </summary>
    public static class OutflowPhase
    {
        static bool IsBoundaryInactive(int idx, int width, int height, bool[] activeRegion, TopologyMode topology, AdjacencyMode adjacency)
        {
            int x = idx % width;
            int y = idx / width;
            
            // Use NeighborProvider for correct neighbor offsets
            NeighborProvider.GetNeighborOffsets(x, y, topology, out int[] dx, out int[] dy, adjacency);

            bool hasActiveNeighbor = false;
            for (int i = 0; i < dx.Length; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                int nIdx = ny * width + nx;
                if (activeRegion[nIdx]) { hasActiveNeighbor = true; break; }
            }
            return hasActiveNeighbor;
        }

        /// <summary>
        /// Gather outflow from active cells and distribute to neighbors.
        /// Stage 14: Now accepts GridState and StepContext for expansion model integration.
        /// </summary>
        public static int GatherOutflow(
            GridState state,
            StepContext context,
            float MinBudgetToPropagate, float PropagateFrac,
            float sinkThreshold, float MatterAheadThreshold,
            out int boundaryHitsThisTick, out float maxChargeThisTick,
            bool debugForceSinkOnFirstBoundaryHit = false)
        {
            int width = state.W;
            int height = state.H;
            var topology = context.Topology;
            var adjacency = context.Adjacency;
            int tick = context.Tick;
            var expansionModel = context.ExpansionModel;
            
            // Array aliases for cleaner code
            float[] ResourceLocal = state.ResourceLocal;
            float[] V = state.V;
            byte[] Active = state.Active;
            bool[] IsInactive = state.IsInactive;
            float[] incoming = state.Incoming;
            bool[] ActiveRegion = state.ActiveRegion;
            bool[] IsSink = state.IsSink;
            float[] SinkCharge = state.SinkCharge;
            int[] RegionActivationTick = state.RegionActivationTick;
            int[] ResourceFirstTick = state.ResourceFirstTick;
            int[] SinkId = state.SinkId;
            int[] SinkParent = state.SinkParent;
            float[] SinkMass = state.SinkMass;
            
            Array.Clear(incoming, 0, incoming.Length);
            int newSinkCount = 0;
            boundaryHitsThisTick = 0;
            maxChargeThisTick = 0f;

            int Idx(int x, int y) => y * width + x;

            // FIRST: Calculate sink attraction weights for each cell based on merged sink mass
            float[] sinkAttractionWeight = new float[width * height];
            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    int pi = Idx(px, py);
                    if (!ActiveRegion[pi]) continue;
                    
                    // Get neighbors using NeighborProvider
                    NeighborProvider.GetNeighborOffsets(px, py, topology, out int[] dx, out int[] dy, adjacency);
                    
                    // Sum the influence of ALL adjacent sinks (by their root mass)
                    float totalSinkInfluence = 0f;
                    
                    for (int d = 0; d < dx.Length; d++)
                    {
                        int nx = px + dx[d];
                        int ny = py + dy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                        int nIdx = Idx(nx, ny);
                        
                        if (IsSink[nIdx])
                        {
                            // Get the ROOT of this sink (handles merged sinks)
                            int root = SinkLogic.GetRootAtCell(nIdx, SinkId, SinkParent);
                            
                            // Get the total mass of this merged sink
                            float sinkMass = 1f; // Default mass
                            if (root > 0 && root < SinkMass.Length)
                                sinkMass = Math.Max(1f, SinkMass[root]);
                            
                            // Influence scales with mass
                            totalSinkInfluence += sinkMass;
                        }
                    }
            
                    // Attraction weight: base 1.0 + mass-weighted influence
                    if (totalSinkInfluence > 0f)
                        sinkAttractionWeight[pi] = 1f + (totalSinkInfluence * 0.1f);
                    else
                        sinkAttractionWeight[pi] = 0f;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Idx(x, y);

                    if (ActiveRegion == null || !ActiveRegion[i]) continue;
                    if (IsSink[i]) continue;
                    if (ResourceLocal[i] <= MinBudgetToPropagate) continue;
                    if (!(Active[i] == 1 || V[i] > -1e-3f)) continue;

                    float frac = Math.Clamp(PropagateFrac, 0f, 1f);
                    float available = ResourceLocal[i] * frac;
                    if (available <= 0f) continue;

                    // Get neighbors using NeighborProvider
                    NeighborProvider.GetNeighborOffsets(x, y, topology, out int[] nbDx, out int[] nbDy, adjacency);
                    int neighborCount = nbDx.Length;

                    // Calculate weighted distribution based on sink attraction
                    float[] weights = new float[neighborCount];
                    int[] neighborIndices = new int[neighborCount];
                    bool[] isBoundaryInactive = new bool[neighborCount];
                    bool[] isSinkNeighbor = new bool[neighborCount];
                    float totalWeight = 0f;
                    int validNeighbors = 0;

                    for (int d = 0; d < neighborCount; d++)
                    {
                        int nx = x + nbDx[d];
                        int ny = y + nbDy[d];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        {
                            weights[d] = 0f;
                            isBoundaryInactive[d] = false;
                            isSinkNeighbor[d] = false;
                            continue;
                        }

                        int neighborIdx = Idx(nx, ny);
                        neighborIndices[d] = neighborIdx;

                        // Skip permanently inactive
                        if (IsInactive[neighborIdx])
                        {
                            weights[d] = 0f;
                            isBoundaryInactive[d] = false;
                            isSinkNeighbor[d] = false;
                            continue;
                        }

                        // Sink neighbor: mark but don't transfer
                        if (IsSink[neighborIdx])
                        {
                            weights[d] = 0f;
                            isBoundaryInactive[d] = false;
                            isSinkNeighbor[d] = true;
                            continue;
                        }

                        // Active region neighbors: apply mass-weighted sink attraction
                        if (ActiveRegion[neighborIdx])
                        {
                            // Base weight = 1.0, boost by mass-weighted sink influence
                            weights[d] = 1f + sinkAttractionWeight[neighborIdx];
                            totalWeight += weights[d];
                            validNeighbors++;
                            isBoundaryInactive[d] = false;
                            isSinkNeighbor[d] = false;
                        }
                        // Boundary inactive: potential sink formation site
                        else if (IsBoundaryInactive(neighborIdx, width, height, ActiveRegion, topology, adjacency))
                        {
                            weights[d] = 0f;
                            isBoundaryInactive[d] = true;
                            isSinkNeighbor[d] = false;
                        }
                        else
                        {
                            weights[d] = 0f;
                            isBoundaryInactive[d] = false;
                            isSinkNeighbor[d] = false;
                        }
                    }

                    float sentTotal = 0f;
                    float wouldHaveSentIntoNonActive = 0f;

                    // PART 1: Distribute to active neighbors (with mass-weighted sink attraction)
                    if (validNeighbors > 0)
                    {
                        for (int di = 0; di < neighborCount; di++)
                        {
                            if (weights[di] <= 0f) continue;

                            // Proportional distribution based on mass-weighted sink attraction
                            float portion = available * (weights[di] / totalWeight);
                            int neighborIdx = neighborIndices[di];

                            incoming[neighborIdx] += portion;
                            sentTotal += portion;
                        }
                    }

                    // PART 2: Handle boundary inactive leakage (creates sinks)
                    // Stage 14: Sink formation now delegated to expansion model plugin
                    float leakPerNeighbor = available / neighborCount; // Equal split across all possible neighbors
                    for (int di = 0; di < neighborCount; di++)
                    {
                        if (!isBoundaryInactive[di]) continue;

                        int neighborIdx = neighborIndices[di];
                        int nx = neighborIdx % width;
                        int ny = neighborIdx / width;

                        // Resource leaking to boundary inactive charges sink formation
                        SinkCharge[neighborIdx] += leakPerNeighbor;
                        boundaryHitsThisTick++;

                        maxChargeThisTick = Math.Max(maxChargeThisTick, SinkCharge[neighborIdx]);

                        // Stage 14: Delegate sink formation decision to expansion model
                        bool shouldCreate = expansionModel.ShouldFormSink(
                            neighborIdx, nx, ny, state, context, SinkCharge[neighborIdx]);
                        
                        // Debug override for testing
                        if (debugForceSinkOnFirstBoundaryHit && !shouldCreate)
                            shouldCreate = true;

                        if (shouldCreate)
                        {
                            // This automatically merges with adjacent sinks via union-find
                            int sinkRoot = SinkLogic.AssignOrMergeAtCell(
                                neighborIdx, width, height,
                                IsSink, SinkId, SinkParent, SinkMass, ref state.NextSinkId);
                            
                            SinkCharge[neighborIdx] = 0f;
                            newSinkCount++;
                            
                            // Stage 14: Notify expansion model of sink formation
                            expansionModel.OnSinkFormed(neighborIdx, nx, ny, state, context);
                            
                            // Get the total mass of the (possibly merged) sink
                            float totalMass = 1f;
                            if (sinkRoot > 0 && sinkRoot < SinkMass.Length)
                                totalMass = SinkMass[sinkRoot];

                            if (debugForceSinkOnFirstBoundaryHit)
                                debugForceSinkOnFirstBoundaryHit = false;
                        }

                        // No actual transfer; resource stays at source
                        wouldHaveSentIntoNonActive += leakPerNeighbor;
                    }

                    // PART 3: Handle blocked sink neighbors (resource reflects back)
                    for (int di = 0; di < neighborCount; di++)
                    {
                        if (!isSinkNeighbor[di]) continue;

                        float blockedPortion = available / neighborCount;
                        wouldHaveSentIntoNonActive += blockedPortion;
                    }

                    // Deduct successfully transferred resource
                    if (sentTotal > 0f)
                        ResourceLocal[i] = Math.Max(0f, ResourceLocal[i] - sentTotal);

                    // Accumulate blocked/leaked resource back to source (creates pressure buildup)
                    if (wouldHaveSentIntoNonActive > 0f)
                        incoming[i] += wouldHaveSentIntoNonActive;
                }
            }

            return newSinkCount;
        }

        public static void GatherInflow(int width, int height, Func<int, int, int> Idx,
            float[] ResourceLocal, int[] RegionActivationTick, int[] ResourceFirstTick,
            bool[] ActiveRegion, byte[] Active, float[] incoming, int tick)
        {
            // Add inflow to all seeded cells  
            int cx = width / 2, cy = height / 2;
            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        int i = Idx(x, y);

                        float pulse = 0.1f * MathF.Exp(-tick / 80f);
                        // continuous inflow to the central patch
                        incoming[i] += pulse;

                        // ensure these stay as seeded/active cells
                        if (tick < 5) incoming[i] += 0.1f;
                        Active[i] = 1;
                        ActiveRegion[i] = true;
                        if (RegionActivationTick[i] == -1) RegionActivationTick[i] = tick;
                        if (ResourceFirstTick[i] == -1) ResourceFirstTick[i] = tick;
                    }
                }
            }
        }

        private static bool IsRegionBoundary(int idx, bool[] ActiveRegion, int width, int height, Func<int, int, int> Idx)
        {
            int x = idx % width;
            int y = idx / width;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;
                int nIdx = Idx(nx, ny);
                if (ActiveRegion[nIdx])
                    return true;
            }
            return false;
        }
    }
}
