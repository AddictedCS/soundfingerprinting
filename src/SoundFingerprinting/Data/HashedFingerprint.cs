namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class HashedFingerprint 
    {
        public HashedFingerprint(byte[] subFingerprint, long[] hashBins, uint sequenceNumber, float startsAt, IEnumerable<string> clusters)
        {
            SubFingerprint = subFingerprint;
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            StartsAt = startsAt;
            Clusters = clusters;
        }

        public byte[] SubFingerprint { get; private set; }

        public long[] HashBins { get; private set; }

        public uint SequenceNumber { get; private set; }

        public float StartsAt { get; private set; }

        public IEnumerable<string> Clusters { get; private set; }
    }
}
