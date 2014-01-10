namespace SoundFingerprinting.Data
{
    using System;

    [Serializable]
    public class SubFingerprintData
    {
        public SubFingerprintData()
        {
            // no op
        }

        public SubFingerprintData(byte[] signature, ISubFingerprintReference subFingerprintReference, ITrackReference trackReference)
        {
            Signature = signature;
            SubFingerprintReference = subFingerprintReference;
            TrackReference = trackReference;
        }

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

            return ((SubFingerprintData)obj).SubFingerprintReference.HashCode == SubFingerprintReference.HashCode;
        }

        public override int GetHashCode()
        {
            return SubFingerprintReference.HashCode;
        }
    }
}
