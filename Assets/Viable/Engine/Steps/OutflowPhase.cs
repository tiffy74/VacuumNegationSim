using System;
using Viable.Engine.State;
using Viable.Engine.Execution;
using Viable.Engine.Logic;

namespace Viable.Engine.Steps
{
    /// <summary>
    /// Phase 1: Gather outflow from active cells and distribute to neighbors.
    /// Handles sink formation at boundaries via charge accumulation.
    /// </summary>
    public static class OutflowPhase
    {
        static bool IsBoundaryInactive(int idx, int width, int height, bool[] activeRegion)
        {
            int x = idx % width;
            int y = idx / width;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            bool hasActiveNeighbor = false;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                int nIdx = ny * width + nx;
                if (activeRegion[nIdx]) { hasActiveNeighbor = true; break; }
            }
            return hasActiveNeighbor;
        }

        public static int GatherOutflow(
            int width, int height,
            float[] ResourceLocal, float[] V, byte[] Active, bool[] IsInactive, float[] incoming,
            float MinBudgetToPropagate, float PropagateFrac,
            bool[] ActiveRegion, bool[] IsSink, float[] SinkCharge, float sinkThreshold, float MatterAheadThreshold,
            int[] RegionActivationTick, int[] ResourceFirstTick,
            int[] SinkId, int[] SinkParent, float[] SinkMass, ref int NextSinkId,
            int tick, out int boundaryHitsThisTick, out float maxChargeThisTick,
            bool debugForceSinkOnFirstBoundaryHit = false)
        {
            Array.Clear(incoming, 0, incoming.Length);
            int newSinkCount = 0;
            boundaryHitsThisTick = 0;
            maxChargeThisTick = 0f;

            int Idx(int x, int y) => y * width + x;
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            // FIRST: Calculate sink attraction weights for each cell based on merged sink mass
            float[] sinkAttractionWeight = new float[width * height];
            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    int pi = Idx(px, py);
                    if (!ActiveRegion[pi]) continue;
                    
                    // Sum the influence of ALL adjacent sinks (by their root mass)
                    float totalSinkInfluence = 0f;
                    
                    for (int d = 0; d < 4; d++)
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

                    // Calculate weighted distribution based on sink attraction
                    float[] weights = new float[4];
                    int[] neighborIndices = new int[4];
                    bool[] isBoundaryInactive = new bool[4];
                    bool[] isSinkNeighbor = new bool[4];
                    float totalWeight = 0f;
                    int validNeighbors = 0;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
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
                        else if (IsBoundaryInactive(neighborIdx, width, height, ActiveRegion))
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
                        for (int d = 0; d < 4; d++)
                        {
                            if (weights[d] <= 0f) continue;

                            // Proportional distribution based on mass-weighted sink attraction
                            float portion = available * (weights[d] / totalWeight);
                            int neighborIdx = neighborIndices[d];

                            incoming[neighborIdx] += portion;
                            sentTotal += portion;
                        }
                    }

                    // PART 2: Handle boundary inactive leakage (creates sinks after tick 10)
                    for (int d = 0; d < 4; d++)
                    {
                        if (!isBoundaryInactive[d]) continue;

                        int neighborIdx = neighborIndices[d];

                        // Resource leaking to boundary inactive charges sink formation
                        float leakPortion = available * 0.25f;
                        SinkCharge[neighborIdx] += leakPortion;
                        boundaryHitsThisTick++;

                        maxChargeThisTick = Math.Max(maxChargeThisTick, SinkCharge[neighborIdx]);

                        // Do NOT allow sink formation before tick 10 (prevents seed destruction)
                        bool allowSink = tick >= 10;

                        // Require both: sufficient time elapsed AND threshold reached
                        bool shouldCreate = allowSink && (SinkCharge[neighborIdx] >= sinkThreshold || debugForceSinkOnFirstBoundaryHit);
                        if (shouldCreate)
                        {
                            // This automatically merges with adjacent sinks via union-find
                            int sinkRoot = SinkLogic.AssignOrMergeAtCell(
                                neighborIdx, width, height,
                                IsSink, SinkId, SinkParent, SinkMass, ref NextSinkId);
                            
                            SinkCharge[neighborIdx] = 0f;
                            newSinkCount++;
                            
                            // Get the total mass of the (possibly merged) sink
                            float totalMass = 1f;
                            if (sinkRoot > 0 && sinkRoot < SinkMass.Length)
                                totalMass = SinkMass[sinkRoot];

                            // Event emission instead of Debug.Log (handled by caller)

                            if (debugForceSinkOnFirstBoundaryHit)
                                debugForceSinkOnFirstBoundaryHit = false;
                        }

                        // No actual transfer; resource stays at source
                        wouldHaveSentIntoNonActive += leakPortion;
                    }

                    // PART 3: Handle blocked sink neighbors (resource reflects back)
                    for (int d = 0; d < 4; d++)
                    {
                        if (!isSinkNeighbor[d]) continue;

                        float blockedPortion = available * 0.25f;
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
