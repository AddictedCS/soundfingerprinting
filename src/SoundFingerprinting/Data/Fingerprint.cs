namespace SoundFingerprinting.Data
{
    public class Fingerprint
    {
        public bool[] Signature { get; set; }

        public int SequenceNumber { get; set; }

        public double Timestamp { get; set; }

        public int Length
        {
            get
            {
                return Signature == null ? 0 : Signature.Length;
            }
        }
    }
}
