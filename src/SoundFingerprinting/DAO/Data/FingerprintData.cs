namespace SoundFingerprinting.DAO.Data
{
    using System;

    using DAO;

    [Serializable]
    public class FingerprintData
    {
        public FingerprintData(bool[] signature, IModelReference trackReference) : this()
        {
            Signature = signature;
            TrackReference = trackReference;
        }

        public FingerprintData(bool[] signature, IModelReference fingerprintReference, IModelReference trackReference)
        {
            Signature = signature;
            FingerprintReference = fingerprintReference;
            TrackReference = trackReference;
        }

        public FingerprintData()
        {
            // no op
        }

        [IgnoreBinding]
        public bool[] Signature { get; internal set; }

        [IgnoreBinding]
        public IModelReference FingerprintReference { get; internal set; }

        [IgnoreBinding]
        public IModelReference TrackReference { get; internal set; }

        public override bool Equals(object obj)
        {
            if (!(obj is FingerprintData))
            {
                return false;
            }

            return ((FingerprintData)obj).FingerprintReference.Equals(FingerprintReference);
        }

        public override int GetHashCode()
        {
            return FingerprintReference.GetHashCode();
        }
    }
}
