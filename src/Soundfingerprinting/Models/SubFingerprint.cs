namespace Soundfingerprinting.Models
{
    public class SubFingerprint
    {
        public int StartAtSecond { get; set; }

        public int EndAtSecond { get; set; }

        public byte[] Signature { get; set; }
    }
}
