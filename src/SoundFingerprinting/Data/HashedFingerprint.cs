namespace SoundFingerprinting.Data
{
    public class HashedFingerprint 
    {
        public HashedFingerprint(byte[] subFingerprint, long[] hashBins, int sequenceNumber, double startsAt)
        {
            SubFingerprint = subFingerprint;
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            StartsAt = startsAt;
        }

        public byte[] SubFingerprint { get; set; }

        public long[] HashBins { get; set; }

        public int SequenceNumber { get; set; }

        public double StartsAt { get; set; }

        public double SourceDuration { get; set; }
    }
}
