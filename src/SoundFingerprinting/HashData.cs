namespace SoundFingerprinting
{
    public class HashData
    {
        public HashData(byte[] subFingerprint, long[] hashBins)
        {
            SubFingerprint = subFingerprint;
            HashBins = hashBins;
        }

        public byte[] SubFingerprint { get; private set; }

        public long[] HashBins { get; private set; }
    }
}
