namespace SoundFingerprinting.DAO.Data
{
    using System;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;

    [Serializable]
    public class FingerprintData
    {
        public FingerprintData()
        {
            // no op
        }

        public FingerprintData(bool[] signature, IModelReference trackReference)
        {
            Signature = signature;
            TrackReference = trackReference;
        }

        [IgnoreBinding]
        public bool[] Signature { get; set; }

        [IgnoreBinding]
        public IModelReference FingerprintReference { get; set; }

        [IgnoreBinding]
        public IModelReference TrackReference { get; set; }
    }
}
