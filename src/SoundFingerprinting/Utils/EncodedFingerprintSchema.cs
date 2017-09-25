namespace SoundFingerprinting.Utils
{
    using System.Collections;

    public class EncodedFingerprintSchema : IEncodedFingerprintSchema
    {
        private readonly BitArray bitArray;
        private bool isSilence = true;

        public EncodedFingerprintSchema(int length)
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

        public void SetTrueAt(int index)
        {
            isSilence = false;
            bitArray[index] = true;
        }

        public EncodedFingerprintSchema SetTrueAt(params int[] indexes)
        {
            isSilence = false;
            foreach (int index in indexes)
            {
                bitArray[index] = true;
            }

            return this;
        }

        public int AgreeOn(EncodedFingerprintSchema other)
        {
            BitArray copy = (BitArray)this.bitArray.Clone();
            var result = copy.And(other.bitArray);
            return TrueCounts(result);
        }


        public int TrueCounts()
        {
            return TrueCounts(this.bitArray);
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
