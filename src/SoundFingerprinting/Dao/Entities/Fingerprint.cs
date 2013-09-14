namespace SoundFingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public class Fingerprint
    {
        public Fingerprint()
        {
            // no op
        }

        public Fingerprint(bool[] signature, int trackId)
            : this()
        {
            Signature = signature;
            TrackId = trackId;
        }

        public Fingerprint(bool[] signature, int trackId, int songOrder)
            : this(signature, trackId)
        {
            SongOrder = songOrder;
        }

        public Fingerprint(bool[] signature, int trackId, int songOrder, int totalFingerprints)
            : this(signature, trackId, songOrder)
        {
            TotalFingerprintsPerTrack = totalFingerprints;
        }

        public bool[] Signature { get; set; }

        public int Id { get; set; }

        public int TrackId { get; set; }

        public int TotalFingerprintsPerTrack { get; set; }

        public int SongOrder { get; set; }
    }
}