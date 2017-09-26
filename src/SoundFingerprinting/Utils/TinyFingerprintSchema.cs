namespace SoundFingerprinting.Utils
{
    using System.Collections;

    internal class TinyFingerprintSchema : IEncodedFingerprintSchema
    {
        private readonly BitArray bitArray;
        private bool isSilence = true;

        public TinyFingerprintSchema(int length)
        {
            bitArray = new BitArray(length);
        }

        public bool IsTrueAt(int index)
        {
            return bitArray[index];
        }

        public bool[] ToBools()
        {
            bool[] concatenated = new bool[bitArray.Length];
            bitArray.CopyTo(concatenated, 0);
            return concatenated;
        }

        public bool IsSilence()
        {
            return isSilence;
        }

        public TinyFingerprintSchema SetTrueAt(int index)
        {
            isSilence = false;
            bitArray[index] = true;
            return this;
        }

        public TinyFingerprintSchema SetTrueAt(params int[] indexes)
        {
            isSilence = false;
            foreach (int index in indexes)
            {
                bitArray[index] = true;
            }

            return this;
        }

        public int AgreeOn(TinyFingerprintSchema other)
        {
            BitArray copy = (BitArray)bitArray.Clone();
            var result = copy.And(other.bitArray);
            return TrueCounts(result);
        }

        public int TrueCounts()
        {
            return TrueCounts(bitArray);
        }

        private int TrueCounts(BitArray result)
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
