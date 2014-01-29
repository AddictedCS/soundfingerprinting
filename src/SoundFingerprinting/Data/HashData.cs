namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;

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

        public double Begin { get; set; }

        public double End { get; set; }
    }
}
