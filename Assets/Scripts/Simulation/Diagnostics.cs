using Assets.Scripts.Domain;
using Assets.Scripts.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    /// <summary>
    /// Centralized Diagnostic Logging System
    /// 
    /// Consolidates all simulation diagnostics into a single, comprehensive logging system.
    /// Provides multiple logging levels and formats for different analysis needs.
    /// 
    /// Design Principles:
    /// - Single Responsibility: All logging logic centralized here
    /// - Zero Unity Dependencies: Can be unit tested independently
    /// - Configurable Output: Support for detailed vs. compact logging
    /// - CSV Export Ready: Structured format for data analysis
    /// 
    /// Usage:
    ///   var diagnostics = new Diagnostics(state, ctx);
    ///   diagnostics.LogComprehensiveTick(boundaryHits, maxCharge, newBHs);
    /// </summary>
    public sealed class Diagnostics
    {
        // ============================================================================
        // PRIVATE FIELDS
        // ============================================================================

        private readonly GridState _state;
        private readonly SimContext _ctx;
        private readonly float _minBudgetToPropagate;

        // ============================================================================
        // CONSTRUCTOR
        // ============================================================================

        /// <summary>
        /// Initializes diagnostic system with simulation state references.
        /// </summary>
        /// <param name="state">Grid state to analyze</param>
        /// <param name="ctx">Simulation context for tick/global data</param>
        /// <param name="minBudgetToPropagate">Minimum energy threshold for propagation</param>
        public Diagnostics(GridState state, SimContext ctx, float minBudgetToPropagate)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _minBudgetToPropagate = minBudgetToPropagate;
        }

        // ============================================================================
        // PUBLIC API: COMPREHENSIVE LOGGING
        // ============================================================================

        /// <summary>
        /// Logs comprehensive single-line diagnostic for each tick.
        /// Consolidates all metrics from LegacyTickStep and SimulationController.
        /// 
        /// Format: [Sim-Diag Txxxx] BH: ... | Energy: ... | Field: ... | Ratios: ... | Pass1: ...
        /// </summary>
        /// <param name="boundaryHits">Number of boundary void hits (from Pass1)</param>
        /// <param name="maxCharge">Maximum BH charge accumulated (from Pass1)</param>
        /// <param name="newBHs">Number of new BHs created this tick (from Pass1)</param>
        public void LogComprehensiveTick(int boundaryHits, float maxCharge, int newBHs)
        {
            var bhStats = ComputeBlackHoleMassStats();

            Debug.Log(
                $"[Tick: {_ctx.Tick}] " +
                $"BH: Cells={bhStats.CellCount:D3} Ent={bhStats.EntityCount:D2} TotalMass={bhStats.TotalMass:F1} MaxMass={bhStats.MaxMass:F1} | " +
                $"Energy: Blk={_ctx.BlockedIntoBH:F3} Ring={_ctx.AdjEnergyNearBH:F3} Leak={_ctx.LeakEnergy:F3} LeakN={_ctx.LeakAttempts:D3} | " +
                $"Field: Avg={_ctx.AvgNField:F3} RingAvg={_ctx.AvgNRingBH:F3} | " +
                $"Ratios: Diff={_ctx.RingMinusField:F3} Ratio={_ctx.RingRatio:F2}x | " +
                $"Pass1: BndHits={boundaryHits:D3} MaxChg={maxCharge:F3} NewBH={newBHs:D2}"
            );
        }

        /// <summary>
        /// Logs detailed cell-level diagnostics (center + frontier).
        /// Useful for debugging specific propagation issues.
        /// </summary>
        public void LogCellDetails()
        {
            int cx = _state.W / 2;
            int cy = _state.H / 2;
            int frontX = Math.Min(_state.W - 1, cx + 5);
            int frontY = cy;
            int ci = _state.Idx(cx, cy);
            int fi = _state.Idx(frontX, frontY);

            Debug.Log(
                $"[Cell-Diag T{_ctx.Tick:D4}] " +
                $"Center({cx},{cy}): In={_state.Incoming[ci]:F4} N={_state.Nlocal[ci]:F4} Field={_state.FieldPresent[ci]} Active={_state.Active[ci]} V={_state.V[ci]:F4} | " +
                $"Front({frontX},{frontY}): In={_state.Incoming[fi]:F4} N={_state.Nlocal[fi]:F4} Field={_state.FieldPresent[fi]} Active={_state.Active[fi]} V={_state.V[fi]:F4}"
            );
        }

        /// <summary>
        /// Logs regional statistics (outer region + frontier).
        /// Should be called every N ticks (e.g., every 20 ticks).
        /// </summary>
        public void LogRegionalStatistics()
        {
            int cx = _state.W / 2;
            int cy = _state.H / 2;

            var outerStats = ComputeOuterRegionStats(cx, cy);
            var frontierStats = ComputeFrontierStats(cx, cy);

            Debug.Log(
                $"[Region-Diag T{_ctx.Tick:D4}] " +
                $"Outer: V+={outerStats.VPosCount:D4} In+={outerStats.InPosCount:D4} Vmin={outerStats.VMin:F4} Vmax={outerStats.VMax:F4} | " +
                $"Frontier: Field={frontierStats.FieldCount:D4} Arrivals={frontierStats.FieldArrivals:D3} Energy+={frontierStats.EnergyCount:D4} Viable={frontierStats.ViableCount:D4} " +
                $"FrontV={frontierStats.FrontierViable:D3} Vmin={frontierStats.FrontierVmin:F3} Vmax={frontierStats.FrontierVmax:F3}"
            );
        }

        /// <summary>
        /// Logs comprehensive tick summary with NaN detection.
        /// Includes averages, counts, and global energy state.
        /// </summary>
        public void LogTickSummary()
        {
            var stats = ComputeTickStatistics();

            float avgIncoming = stats.SumIncoming / _state.Len;
            float avgEntropy = stats.SumEntropy / _state.Len;
            float avgVposAllCells = stats.SumViabilityPos / _state.Len;

            string baseMsg =
                $"[Summary T{_ctx.Tick:D4}] " +
                $"AvgIn={avgIncoming:F3} AvgV+={avgVposAllCells:F3} AvgS={avgEntropy:F3} | " +
                $"Active={stats.ActiveCount:D4} V+={stats.VPosCount:D4} Vmin={stats.VMin:F3} Vmax={stats.VMax:F3} | " +
                $"NGlobal={_ctx.NGlobal:F1}";

            if (HasNaNValues(stats))
            {
                baseMsg += $" | NaN! V={stats.VNaN} In={stats.InNaN} N={stats.NNaN} E={stats.ENaN}";
            }

            Debug.Log(baseMsg);
        }

        /// <summary>
        /// Logs CSV-formatted line for data export and analysis.
        /// Format: Tick, BHCells, BHEntities, ... (no labels, comma-separated)
        /// </summary>
        public void LogCSVRow(int boundaryHits, float maxCharge, int newBHs)
        {
            var bhStats = ComputeBlackHoleMassStats();

            Debug.Log(
                $"CSV,{_ctx.Tick}," +
                $"{bhStats.CellCount},{bhStats.EntityCount},{bhStats.TotalMass:F2},{bhStats.MaxMass:F2}," +
                $"{_ctx.BlockedIntoBH:F4},{_ctx.AdjEnergyNearBH:F4},{_ctx.LeakEnergy:F4},{_ctx.LeakAttempts}," +
                $"{_ctx.AvgNField:F4},{_ctx.AvgNRingBH:F4},{_ctx.RingMinusField:F4},{_ctx.RingRatio:F4}," +
                $"{boundaryHits},{maxCharge:F4},{newBHs}"
            );
        }

        /// <summary>
        /// Logs CSV header (call once at simulation start).
        /// </summary>
        public static void LogCSVHeader()
        {
            Debug.Log(
                "CSV,Tick," +
                "BHCells,BHEntities,TotalBHMass,MaxBHMass," +
                "BlockedIntoBH,AdjEnergyNearBH,LeakEnergy,LeakAttempts," +
                "AvgNField,AvgNRingBH,RingMinusField,RingRatio," +
                "BoundaryHits,MaxCharge,NewBHs"
            );
        }

        // ============================================================================
        // PRIVATE: BLACK HOLE STATISTICS
        // ============================================================================

        private struct BlackHoleStats
        {
            public int CellCount;
            public int EntityCount;
            public float TotalMass;
            public float MaxMass;
        }

        private BlackHoleStats ComputeBlackHoleMassStats()
        {
            var stats = new BlackHoleStats();
            var countedRoots = new HashSet<int>();

            for (int i = 0; i < _state.Len; i++)
            {
                if (!_state.IsBlackHole[i]) continue;

                stats.CellCount++;

                int bhId = _state.BlackHoleId[i];
                if (bhId <= 0) continue;

                int root = BlackHoles.GetRootAtCell(i, _state.BlackHoleId, _state.BlackHoleParent);
                if (countedRoots.Contains(root)) continue;

                countedRoots.Add(root);

                float mass = (root > 0 && root < _state.BlackHoleMass.Length)
                    ? _state.BlackHoleMass[root]
                    : 1f;

                stats.TotalMass += mass;
                stats.EntityCount++;

                if (mass > stats.MaxMass)
                    stats.MaxMass = mass;
            }

            return stats;
        }

        // ============================================================================
        // PRIVATE: REGIONAL STATISTICS
        // ============================================================================

        private struct OuterRegionStats
        {
            public int VPosCount;
            public int InPosCount;
            public float VMin;
            public float VMax;
        }

        private OuterRegionStats ComputeOuterRegionStats(int cx, int cy)
        {
            var stats = new OuterRegionStats
            {
                VMin = float.PositiveInfinity,
                VMax = float.NegativeInfinity
            };

            for (int y = 0; y < _state.H; y++)
            {
                for (int x = 0; x < _state.W; x++)
                {
                    // Skip seed patch (5x5 center)
                    if (x >= cx - 2 && x <= cx + 2 && y >= cy - 2 && y <= cy + 2)
                        continue;

                    int idx = _state.Idx(x, y);

                    if (_state.V[idx] > 0f)
                    {
                        stats.VPosCount++;
                        if (_state.V[idx] < stats.VMin) stats.VMin = _state.V[idx];
                        if (_state.V[idx] > stats.VMax) stats.VMax = _state.V[idx];
                    }

                    if (_state.Incoming[idx] > 0f)
                        stats.InPosCount++;
                }
            }

            if (stats.VMin == float.PositiveInfinity) stats.VMin = 0f;
            if (stats.VMax == float.NegativeInfinity) stats.VMax = 0f;

            return stats;
        }

        private struct FrontierStats
        {
            public int FieldCount;
            public int FieldArrivals;
            public int EnergyCount;
            public int ViableCount;
            public int FrontierViable;
            public float FrontierVmin;
            public float FrontierVmax;
        }

        private FrontierStats ComputeFrontierStats(int cx, int cy)
        {
            var stats = new FrontierStats
            {
                FrontierVmin = float.PositiveInfinity,
                FrontierVmax = float.NegativeInfinity
            };

            int arrivalTick = _ctx.Tick - 1;

            for (int i = 0; i < _state.Len; i++)
            {
                if (_state.FieldPresent[i])
                {
                    stats.FieldCount++;
                    if (_state.FieldFirstTick[i] == arrivalTick || _state.FieldFirstTick[i] == _ctx.Tick)
                        stats.FieldArrivals++;
                }

                if (_state.Nlocal[i] > _minBudgetToPropagate)
                    stats.EnergyCount++;

                if (_state.V[i] > 0f)
                    stats.ViableCount++;

                // Frontier: recent field arrivals (last 5 ticks, excluding seed)
                if (_state.FieldPresent[i] && _state.FieldFirstTick[i] >= _ctx.Tick - 5)
                {
                    int fx = i % _state.W;
                    int fy = i / _state.W;

                    // Exclude seed region
                    if (fx >= cx - 2 && fx <= cx + 2 && fy >= cy - 2 && fy <= cy + 2)
                        continue;

                    if (_state.V[i] > 0f)
                    {
                        stats.FrontierViable++;
                        if (_state.V[i] < stats.FrontierVmin) stats.FrontierVmin = _state.V[i];
                        if (_state.V[i] > stats.FrontierVmax) stats.FrontierVmax = _state.V[i];
                    }
                }
            }

            if (stats.FrontierVmin == float.PositiveInfinity) stats.FrontierVmin = 0f;
            if (stats.FrontierVmax == float.NegativeInfinity) stats.FrontierVmax = 0f;

            return stats;
        }

        // ============================================================================
        // PRIVATE: TICK SUMMARY STATISTICS
        // ============================================================================

        private struct TickStatistics
        {
            public float SumIncoming;
            public float SumViabilityPos;
            public float SumEntropy;
            public int ActiveCount;
            public float VMin;
            public float VMax;
            public int VPosCount;
            public int VNaN;
            public int InNaN;
            public int NNaN;
            public int ENaN;
        }

        private TickStatistics ComputeTickStatistics()
        {
            var stats = new TickStatistics
            {
                VMin = float.PositiveInfinity,
                VMax = float.NegativeInfinity
            };

            for (int i = 0; i < _state.Len; i++)
            {
                float inc = _state.Incoming[i];
                float n = _state.Nlocal[i];
                float ent = _state.Entropy[i];
                float v = _state.V[i];

                stats.SumIncoming += inc;
                stats.SumEntropy += ent;

                if (_state.Active[i] == 1) stats.ActiveCount++;

                if (float.IsNaN(inc) || float.IsInfinity(inc)) stats.InNaN++;
                if (float.IsNaN(n) || float.IsInfinity(n)) stats.NNaN++;
                if (float.IsNaN(ent) || float.IsInfinity(ent)) stats.ENaN++;

                if (float.IsNaN(v) || float.IsInfinity(v))
                {
                    stats.VNaN++;
                    continue;
                }

                if (v > 0f)
                {
                    stats.VPosCount++;
                    stats.SumViabilityPos += v;
                }

                if (v < stats.VMin) stats.VMin = v;
                if (v > stats.VMax) stats.VMax = v;
            }

            if (stats.VMin == float.PositiveInfinity) stats.VMin = float.NaN;
            if (stats.VMax == float.NegativeInfinity) stats.VMax = float.NaN;

            return stats;
        }

        private bool HasNaNValues(TickStatistics stats)
        {
            return stats.VNaN > 0 || stats.InNaN > 0 || stats.NNaN > 0 || stats.ENaN > 0;
        }
    }
}
