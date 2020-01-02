using System;

// ReSharper disable InconsistentNaming

namespace D2Evil
{
    /// <summary>
    /// Trash (Java) Random
    /// </summary>
    ///REF:https://stackoverflow.com/a/2147782
    [Serializable]
    public class LJRandom
    {
        public LJRandom(UInt64 seed)
        {
            this.seed = (seed ^ 0x5DEECE66DUL) & ((1UL << 48) - 1);
        }

        public int NextInt()
        {
            return unchecked((int)Next(32));
        }

        public int NextInt(int n)
        {
            if (n <= 0) throw new ArgumentException("n must be positive");

            if ((n & -n) == n)  // i.e., n is a power of 2
                return (int)((n * (long)Next(31)) >> 31);

            long bits, val;
            do
            {
                bits = Next(31);
                val = bits % (uint)n;
            }
            while (bits - val + (n - 1) < 0);

            return (int)val;
        }

        protected uint Next(int bits)
        {
            seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);

            return (uint)(seed >> (48 - bits));
        }

        private ulong seed;
    }
}
