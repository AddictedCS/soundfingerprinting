namespace Soundfingerprinting.DbStorage.Entities
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Fingerprint
    {
        public Fingerprint()
        {
            Id = int.MinValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fingerprint"/> class. 
        /// </summary>
        /// <param name="id">
        /// Id of the signature
        /// </param>
        /// <param name="signature">
        /// Fingerprint image
        /// </param>
        /// <param name="trackId">
        /// Track identifier
        /// </param>
        /// <param name="songOrder">
        /// Order # in the corresponding song
        /// </param>
        public Fingerprint(int id, bool[] signature, int trackId, int songOrder)
        {
            Id = id;
            Signature = signature;
            TrackId = trackId;
            SongOrder = songOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fingerprint"/> class. 
        /// </summary>
        /// <param name="id">
        /// Id of the signature
        /// </param>
        /// <param name="signature">
        /// Fingerprint
        /// </param>
        /// <param name="trackId">
        /// Track identifier
        /// </param>
        /// <param name="songOrder">
        /// Order # in the corresponding song
        /// </param>
        /// <param name="totalFingerprints">
        /// Total fingerprints per track
        /// </param>
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