namespace SoundFingerprinting.Data
{
    public class HashedFingerprint 
    {
        public HashedFingerprint(byte[] subFingerprint, long[] hashBins, int sequenceNumber, double sequenceAt)
        {
            SubFingerprint = subFingerprint;
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            Timestamp = sequenceAt;
        }

        public byte[] SubFingerprint { get; set; }

        public long[] HashBins { get; set; }

        public int SequenceNumber { get; set; }

        public double Timestamp { get; set; }
    }
}
