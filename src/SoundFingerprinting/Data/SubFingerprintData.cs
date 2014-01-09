namespace SoundFingerprinting.Data
{
    public class SubFingerprintData
    {
        public byte[] Signature { get; set; }

        public ISubFingerprintReference SubFingerprintReference { get; set; }

        public ITrackReference TrackReference { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is SubFingerprintData))
            {
                return false;
            }

            return ((SubFingerprintData)obj).SubFingerprintReference.GetHashCode() == SubFingerprintReference.GetHashCode();
        }

        public override int GetHashCode()
        {
            return SubFingerprintReference.HashCode.GetHashCode();
        }
    }
}
