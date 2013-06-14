namespace SoundFingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public class Fingerprint
    {
        public Fingerprint()
        {
            Id = int.MinValue;
        }

        public Fingerprint(bool[] signature, int trackId, int songOrder) : this()
        {
            Signature = signature;
            TrackId = trackId;
            SongOrder = songOrder;
        }

        public Fingerprint(int id, bool[] signature, int trackId, int songOrder)
            : this(signature, trackId, songOrder)
        {
            Id = id;
        }

        public Fingerprint(int id, bool[] signature, int trackId, int songOrder, int totalFingerprints)
            : this(id, signature, trackId, songOrder)
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