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

        public float[] Nlocal, Entropy, V, Incoming, EntropyNext, BlackHoleCharge;
        public byte[] Active;
        public bool[] IsVacuum, FieldPresent, IsBlackHole;
        public int[] ZeroEnergyTicks, FieldFirstTick, EnergyFirstTick;
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

        }

        public int Idx(int x, int y) => y * W + x;
    }

}
