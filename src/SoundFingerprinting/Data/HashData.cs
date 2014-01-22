namespace SoundFingerprinting.Data
{
    using System;

    [Serializable]
    public class HashData
    {
        public HashData()
        {
            // no op
        }

        public HashData(byte[] subFingerprint, long[] hashBins)
        {
            SubFingerprint = subFingerprint;
            HashBins = hashBins;
        }

        public byte[] SubFingerprint { get; set; }

        public long[] HashBins { get; set; }
    }
}
