namespace SoundFingerprinting.Data
{
    using System.Collections.Generic;

    public class HashedFingerprint 
    {
        public HashedFingerprint(byte[] subFingerprint, long[] hashBins, int sequenceNumber, double startsAt, IEnumerable<string> clusters)
        {
            SubFingerprint = subFingerprint;
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            StartsAt = startsAt;
            Clusters = clusters;
        }

        public byte[] SubFingerprint { get; private set; }

        public long[] HashBins { get; private set; }

        public int SequenceNumber { get; private set; }

        public double StartsAt { get; private set; }

        public IEnumerable<string> Clusters { get; private set; }
    }
}
