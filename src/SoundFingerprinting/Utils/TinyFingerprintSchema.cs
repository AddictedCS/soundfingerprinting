namespace SoundFingerprinting.Utils
{
    using System;

    internal class TinyFingerprintSchema : IEncodedFingerprintSchema
    {
        /*
            Lookup table for the de Bruijn sequence based count of trailing zeros, used to enumerate set bits in ConvertToBooleans.
            See "Bit Twiddling Hacks" by Sean Eron Anderson.
        */
        private static readonly int[] DeBruijnTrailingZeros =
        {
            0, 1, 2, 53, 3, 7, 54, 27, 4, 38, 41, 8, 34, 55, 48, 28,
            62, 5, 39, 46, 44, 42, 22, 9, 24, 35, 59, 56, 49, 18, 29, 11,
            63, 52, 6, 26, 37, 40, 33, 47, 61, 45, 43, 21, 23, 58, 17, 10,
            51, 25, 36, 32, 60, 20, 57, 16, 50, 31, 19, 15, 30, 14, 13, 12
        };

        private readonly ulong[] words;
        private readonly int length;

        public TinyFingerprintSchema(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            this.length = length;
            words = new ulong[(length + 63) >> 6];
        }

        public bool this[int index]
        {
            get
            {
                if ((uint)index >= (uint)length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                // shift count on ulong is implicitly taken modulo 64
                return (words[index >> 6] & (1UL << index)) != 0;
            }
        }

        public bool IsSilence { get; private set; } = true;

        public bool[] ConvertToBooleans()
        {
            bool[] concatenated = new bool[length];
            for (int w = 0; w < words.Length; ++w)
            {
                ulong word = words[w];
                int baseIndex = w << 6;
                while (word != 0)
                {
                    ulong isolated = word & (0UL - word);
                    concatenated[baseIndex + DeBruijnTrailingZeros[(isolated * 0x022FDD63CC95386DUL) >> 58]] = true;
                    word &= word - 1;
                }
            }

            return concatenated;
        }

        public TinyFingerprintSchema SetTrueAt(int index)
        {
            if ((uint)index >= (uint)length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            IsSilence = false;
            words[index >> 6] |= 1UL << index;
            return this;
        }

        public TinyFingerprintSchema SetTrueAt(params int[] indexes)
        {
            IsSilence = false;
            foreach (int index in indexes)
            {
                SetTrueAt(index);
            }

            return this;
        }

        public int AgreeOn(TinyFingerprintSchema other)
        {
            if (length != other.length)
            {
                throw new ArgumentException("Schemas must be of the same length", nameof(other));
            }

            ulong[] otherWords = other.words;
            int count = 0;
            for (int i = 0; i < words.Length; ++i)
            {
                count += PopCount(words[i] & otherWords[i]);
            }

            return count;
        }

        public int TrueCounts()
        {
            int count = 0;
            for (int i = 0; i < words.Length; ++i)
            {
                count += PopCount(words[i]);
            }

            return count;
        }

        /*
            SWAR population count, netstandard2.0-compatible (System.Numerics.BitOperations is not available).
        */
        private static int PopCount(ulong x)
        {
            x -= (x >> 1) & 0x5555555555555555UL;
            x = (x & 0x3333333333333333UL) + ((x >> 2) & 0x3333333333333333UL);
            x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
            return (int)((x * 0x0101010101010101UL) >> 56);
        }
    }
}
