namespace SoundFingerprinting.Data
{
    using System;

    using SoundFingerprinting.Dao;

    [Serializable]
    public class FingerprintData
    {
        public FingerprintData()
        {
            // no op
        }

        public FingerprintData(bool[] signature, ITrackReference trackReference)
        {
            Signature = signature;
            TrackReference = trackReference;
        }

        [IgnoreBinding]
        public bool[] Signature { get; set; }

        [IgnoreBinding]
        public IFingerprintReference FingerprintReference { get; set; }

        [IgnoreBinding]
        public ITrackReference TrackReference { get; set; }
    }
}
