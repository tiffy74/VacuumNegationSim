using System;

namespace Viable.Engine.State
{
    /// <summary>
    /// Pure simulation state for a 2D grid. No Unity objects, no rendering, no behaviour.
    /// All per-cell fields are flattened arrays of length W*H.
    /// </summary>
    public sealed class GridState
    {
        public readonly int W;
        public readonly int H;
        public readonly int Len;

        // ---- Per-cell fields ----
        public float[] ResourceLocal;     // local resource budget
        public float[] ComplexityMetric;  // 0..1 structural complexity proxy
        public float[] V;                 // viability score
        public byte[] Active;             // 0/1

        public float[] Incoming;          // Pass1 -> Pass2 buffer
        public float[] ComplexityNext;    // Pass4 diffusion ping-pong
        public bool[] IsInactive;         // permanent/semipermanent inactive cell

        // ---- Region/geometry presence ----
        public bool[] ActiveRegion;       // region where state transitions are defined
        public int[] RegionActivationTick;  // -1 until region becomes active
        public int[] ResourceFirstTick;   // -1 until resource arrives

        // ---- Sink region per-cell mask ----
        public bool[] IsSink;             // true if this cell belongs to a sink region

        // ---- Sink boundary charging (accumulation) ----
        public float[] SinkCharge;        // charge accumulated on inactive boundary cells

        // ---- Sink union-find identity / merging ----
        // If a cell is a sink cell, SinkId[cell] is the sink entity id (>0).
        public int[] SinkId;              // per cell: sink entity id (0 = none)
        public int[] SinkParent;          // union-find parent pointers, indexed by sink entity id
        public float[] SinkMass;          // mass per sink entity id (index by root id)

        // Next ID to assign (starts at 1)
        public int NextSinkId = 1;

        // ---- Optional helper field for diagnostics/forces ----
        public float[] SinkPotential;

        // ---- Other book-keeping ----
        public int[] ZeroResourceTicks;

        public GridState(int width, int height, int initialSinkCapacity = 8192)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            W = width;
            H = height;
            Len = W * H;

            // Per-cell arrays
            ResourceLocal = new float[Len];
            ComplexityMetric = new float[Len];
            V = new float[Len];
            Active = new byte[Len];

            Incoming = new float[Len];
            ComplexityNext = new float[Len];
            IsInactive = new bool[Len];

            ActiveRegion = new bool[Len];
            RegionActivationTick = new int[Len];
            ResourceFirstTick = new int[Len];

            IsSink = new bool[Len];
            SinkCharge = new float[Len];

            SinkId = new int[Len];

            ZeroResourceTicks = new int[Len];

            SinkPotential = new float[Len];

            // Union-find arrays are indexed by sink entity id (not cell index),
            // so allocate a separate capacity. We grow them if needed.
            if (initialSinkCapacity < 16) initialSinkCapacity = 16;
            SinkParent = new int[initialSinkCapacity];
            SinkMass = new float[initialSinkCapacity];

            Reset();
        }

        /// <summary>
        /// 2D -> 1D index.
        /// </summary>
        public int Idx(int x, int y) => (y * W) + x;

        /// <summary>
        /// Clear the whole state back to defaults.
        /// </summary>
        public void Reset()
        {
            Array.Clear(ResourceLocal, 0, Len);
            Array.Clear(ComplexityMetric, 0, Len);
            Array.Clear(V, 0, Len);
            Array.Clear(Active, 0, Len);

            Array.Clear(Incoming, 0, Len);
            Array.Clear(ComplexityNext, 0, Len);
            Array.Clear(IsInactive, 0, Len);

            Array.Clear(ActiveRegion, 0, Len);
            Array.Clear(IsSink, 0, Len);
            Array.Clear(SinkCharge, 0, Len);
            Array.Clear(SinkId, 0, Len);

            Array.Clear(ZeroResourceTicks, 0, Len);
            Array.Clear(SinkPotential, 0, Len);

            for (int i = 0; i < Len; i++)
            {
                RegionActivationTick[i] = -1;
                ResourceFirstTick[i] = -1;
            }

            // Reset sink entities
            NextSinkId = 1;
            Array.Clear(SinkParent, 0, SinkParent.Length);
            Array.Clear(SinkMass, 0, SinkMass.Length);
        }

        /// <summary>
        /// Ensure the union-find arrays can hold a sink entity id of at least 'requiredId'.
        /// </summary>
        public void EnsureSinkCapacity(int requiredId)
        {
            if (requiredId < SinkParent.Length) return;

            int newCap = SinkParent.Length;
            while (newCap <= requiredId) newCap *= 2;

            Array.Resize(ref SinkParent, newCap);
            Array.Resize(ref SinkMass, newCap);
        }

        /// <summary>
        /// Convenience: create a new sink entity id (root), with initial mass.
        /// Caller still needs to mark IsSink[cell]=true and SinkId[cell]=id.
        /// </summary>
        public int CreateSinkEntity(float initialMass = 1f)
        {
            int id = NextSinkId++;
            EnsureSinkCapacity(id + 1);

            SinkParent[id] = id;                 // root
            SinkMass[id] = Math.Max(1f, initialMass);

            return id;
        }
    }
}
