namespace SoundFingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    internal class SubFingerprint
    {
        public SubFingerprint()
        {
            // no op
        }

        public SubFingerprint(byte[] signature, int trackId)
            : this()
        {
            Signature = signature;
            TrackId = trackId;
        }

        public long Id { get; set; }

        public byte[] Signature { get; set; }

        public int TrackId { get; set; }
    }
}
