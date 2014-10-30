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

        public HashData(byte[] subFingerprint, long[] hashBins, int sequenceNumber)
            : this(subFingerprint, hashBins)
        {
            SequenceNumber = sequenceNumber;
        }

        public byte[] SubFingerprint { get; set; }

        public long[] HashBins { get; set; }

        public int SequenceNumber { get; set; }
    }
}
