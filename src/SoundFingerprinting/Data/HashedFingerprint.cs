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

        public byte[] SubFingerprint { get; private set; }

        public long[] HashBins { get; private set; }

        public int SequenceNumber { get; private set; }

        public double StartsAt { get; private set; }
    }
}
