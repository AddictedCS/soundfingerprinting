namespace SoundFingerprinting.Data
{
    public class HashData
    {
        public HashData(byte[] subFingerprint, long[] hashBins)
        {
            SubFingerprint = subFingerprint;
            HashBins = hashBins;
        }

        public byte[] SubFingerprint { get; set; }

        public long[] HashBins { get; set; }
    }
}
