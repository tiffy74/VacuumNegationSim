using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Domain
{
    public sealed class GridState
    {
        public readonly int W, H, Len;

        public float[] Nlocal, Entropy, V, Incoming, EntropyNext, BlackHoleCharge, BlackHoleMass;
        public byte[] Active;
        public bool[] IsVacuum, FieldPresent, IsBlackHole;
        public int[] ZeroEnergyTicks, FieldFirstTick, EnergyFirstTick, BlackHoleId, BlackHoleParent;
        public int NextBlackHoleId;
        public GridState(int w, int h)
        {
            W = w; H = h; Len = w * h;

            Nlocal = new float[Len];
            Entropy = new float[Len];
            V = new float[Len];
            Active = new byte[Len];

            Incoming = new float[Len];
            EntropyNext = new float[Len];

            IsVacuum = new bool[Len];
            FieldPresent = new bool[Len];
            IsBlackHole = new bool[Len];

            BlackHoleCharge = new float[Len];

            ZeroEnergyTicks = new int[Len];
            FieldFirstTick = new int[Len];
            EnergyFirstTick = new int[Len];

            // Black hole connected-component tracking and mass per component
            BlackHoleId = new int[Len];
            BlackHoleParent = new int[Len + 1]; // ids start at 1
            BlackHoleMass = new float[Len + 1];
            NextBlackHoleId = 1;
            for (int i = 0; i < BlackHoleParent.Length; i++)
                BlackHoleParent[i] = i;
        }

        public int Idx(int x, int y) => y * W + x;
    }

}
