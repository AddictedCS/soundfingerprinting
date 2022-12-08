namespace SoundFingerprinting.Utils
{
    using System.Collections;

    internal class TinyFingerprintSchema : IEncodedFingerprintSchema
    {
        private readonly BitArray bitArray;

        public TinyFingerprintSchema(int length)
        {
            bitArray = new BitArray(length);
        }

        public bool this[int index] => bitArray[index];

        public bool[] ConvertToBooleans()
        {
            bool[] concatenated = new bool[bitArray.Length];
            bitArray.CopyTo(concatenated, 0);
            return concatenated;
        }

        public bool IsSilence { get; private set; } = true;

        public TinyFingerprintSchema SetTrueAt(int index)
        {
            IsSilence = false;
            bitArray[index] = true;
            return this;
        }

        public TinyFingerprintSchema SetTrueAt(params int[] indexes)
        {
            IsSilence = false;
            foreach (int index in indexes)
            {
                bitArray[index] = true;
            }

            return this;
        }

        public int AgreeOn(TinyFingerprintSchema other)
        {
            var copy = (BitArray)bitArray.Clone();
            var result = copy.And(other.bitArray);
            return TrueCounts(result);
        }

        public int TrueCounts()
        {
            return TrueCounts(bitArray);
        }

        private static int TrueCounts(BitArray result)
        {
            int count = 0;
            for (int i = 0; i < result.Length; ++i)
            {
                if (result[i])
                {
                    count++;
                }
            }

            return count;
        }
    }
}