namespace Soundfingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public class SubFingerprint
    {
        public SubFingerprint()
        {
            Id = long.MinValue;
        }

        public SubFingerprint(byte[] signature, int trackId) : this()
        {
            Signature = signature;
            TrackId = trackId;
        }

        public SubFingerprint(long id, byte[] signature, int trackId) : this(signature, trackId) 
        {
            Id = id;
        }

        public long Id { get; set; }

        public byte[] Signature { get; set; }

        public int TrackId { get; set; }
    }
}
