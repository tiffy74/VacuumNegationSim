using System;

namespace Assets.Scripts.Domain
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
        public float[] Nlocal;        // local energy/budget
        public float[] Entropy;       // 0..1 (your "complexity proxy")
        public float[] V;             // viability
        public byte[] Active;         // 0/1

        public float[] Incoming;      // Pass1 -> Pass2 buffer
        public float[] EntropyNext;   // Pass4 diffusion ping-pong
        public bool[] IsVacuum;       // permanent/semipermanent vacuum cell

        // ---- Field/geometry presence ----
        public bool[] FieldPresent;   // configuration/geometry available
        public int[] FieldFirstTick;  // -1 until field arrives
        public int[] EnergyFirstTick; // -1 until energy arrives

        // ---- Black hole per-cell mask ----
        public bool[] IsBlackHole;    // true if this cell belongs to a BH region

        // ---- Black hole charging (boundary attempt accumulation) ----
        public float[] BlackHoleCharge; // charge accumulated on non-field boundary cells

        // ---- Black hole union-find identity / merging ----
        // If a cell is a BH cell, BlackHoleId[cell] is the BH entity id (>0).
        public int[] BlackHoleId;       // per cell: BH entity id (0 = none)
        public int[] BlackHoleParent;   // union-find parent pointers, indexed by BH entity id
        public float[] BlackHoleMass;   // mass per BH entity id (index by root id)

        // Next ID to assign (starts at 1)
        public int NextBlackHoleId = 1;

        // ---- Optional helper field (if you later use it) ----
        // Not required for correctness, but often useful for diagnostics/forces.
        public float[] BlackHolePotential;

        // ---- Other book-keeping ----
        public int[] ZeroEnergyTicks;

        public GridState(int width, int height, int initialBlackHoleCapacity = 8192)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            W = width;
            H = height;
            Len = W * H;

            // Per-cell arrays
            Nlocal = new float[Len];
            Entropy = new float[Len];
            V = new float[Len];
            Active = new byte[Len];

            Incoming = new float[Len];
            EntropyNext = new float[Len];
            IsVacuum = new bool[Len];

            FieldPresent = new bool[Len];
            FieldFirstTick = new int[Len];
            EnergyFirstTick = new int[Len];

            IsBlackHole = new bool[Len];
            BlackHoleCharge = new float[Len];

            BlackHoleId = new int[Len];

            ZeroEnergyTicks = new int[Len];

            BlackHolePotential = new float[Len];

            // Union-find arrays are indexed by BH entity id (not cell index),
            // so allocate a separate capacity. We grow them if needed.
            if (initialBlackHoleCapacity < 16) initialBlackHoleCapacity = 16;
            BlackHoleParent = new int[initialBlackHoleCapacity];
            BlackHoleMass = new float[initialBlackHoleCapacity];

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
            Array.Clear(Nlocal, 0, Len);
            Array.Clear(Entropy, 0, Len);
            Array.Clear(V, 0, Len);
            Array.Clear(Active, 0, Len);

            Array.Clear(Incoming, 0, Len);
            Array.Clear(EntropyNext, 0, Len);
            Array.Clear(IsVacuum, 0, Len);

            Array.Clear(FieldPresent, 0, Len);
            Array.Clear(IsBlackHole, 0, Len);
            Array.Clear(BlackHoleCharge, 0, Len);
            Array.Clear(BlackHoleId, 0, Len);

            Array.Clear(ZeroEnergyTicks, 0, Len);
            Array.Clear(BlackHolePotential, 0, Len);

            for (int i = 0; i < Len; i++)
            {
                FieldFirstTick[i] = -1;
                EnergyFirstTick[i] = -1;
            }

            // Reset BH entities
            NextBlackHoleId = 1;
            Array.Clear(BlackHoleParent, 0, BlackHoleParent.Length);
            Array.Clear(BlackHoleMass, 0, BlackHoleMass.Length);
        }

        /// <summary>
        /// Ensure the union-find arrays can hold a BH entity id of at least 'requiredId'.
        /// </summary>
        public void EnsureBlackHoleCapacity(int requiredId)
        {
            if (requiredId < BlackHoleParent.Length) return;

            int newCap = BlackHoleParent.Length;
            while (newCap <= requiredId) newCap *= 2;

            Array.Resize(ref BlackHoleParent, newCap);
            Array.Resize(ref BlackHoleMass, newCap);
        }

        /// <summary>
        /// Convenience: create a new BH entity id (root), with initial mass.
        /// Caller still needs to mark IsBlackHole[cell]=true and BlackHoleId[cell]=id.
        /// </summary>
        public int CreateBlackHoleEntity(float initialMass = 1f)
        {
            int id = NextBlackHoleId++;
            EnsureBlackHoleCapacity(id + 1);

            BlackHoleParent[id] = id;                 // root
            BlackHoleMass[id] = Math.Max(1f, initialMass);

            return id;
        }
    }
}
