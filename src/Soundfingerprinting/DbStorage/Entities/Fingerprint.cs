namespace Soundfingerprinting.DbStorage.Entities
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Fingerprint
    {
        public Fingerprint()
        {
            Id = Int32.MinValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fingerprint"/> class. 
        /// </summary>
        /// <param name="id">
        /// Id of the fingerprint
        /// </param>
        /// <param name="fingerprint">
        /// Fingerprint image
        /// </param>
        /// <param name="trackId">
        /// Track identifier
        /// </param>
        /// <param name="songOrder">
        /// Order # in the corresponding song
        /// </param>
        public Fingerprint(int id, bool[] fingerprint, int trackId, int songOrder)
        {
            Id = id;
            Signature = fingerprint;
            TrackId = trackId;
            SongOrder = songOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fingerprint"/> class. 
        /// </summary>
        /// <param name="id">
        /// Id of the fingerprint
        /// </param>
        /// <param name="fingerprint">
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
        public Fingerprint(int id, bool[] fingerprint, int trackId, int songOrder, int totalFingerprints)
            : this(id, fingerprint, trackId, songOrder)
        {
            TotalFingerprintsPerTrack = totalFingerprints;
        }

        public bool[] Signature { get; set; }

        /// <summary>
        ///   Fingerprint Id
        /// </summary>
        /// <remarks>
        ///   Once inserted into the database the object will be given a unique identifier
        /// </remarks>
        public Int32 Id { get; set; }

        /// <summary>
        ///   Track Id to which the fingerprint belongs to
        /// </summary>
        public Int32 TrackId { get; set; }

        /// <summary>
        ///   Total fingerprints per track
        /// </summary>
        public int TotalFingerprintsPerTrack { get; set; }

        /// <summary>
        ///   Song order
        /// </summary>
        public int SongOrder { get; set; }

        /// <summary>
        ///   Associate fingerprint signatures with a specific track
        /// </summary>
        /// <param name = "fingerprintSignatures">Signatures built from one track</param>
        /// <param name = "trackId">Track id, which is the parent for this fingerprints</param>
        /// <returns>List of fingerprint entity objects</returns>
        public static List<Fingerprint> AssociateFingerprintsToTrack(IEnumerable<bool[]> fingerprintSignatures, Int32 trackId)
        {
            const int fakeId = -1;
            List<Fingerprint> fingers = new List<Fingerprint>();
            int c = 0;
            foreach (bool[] signature in fingerprintSignatures)
            {
                fingers.Add(new Fingerprint(fakeId, signature, trackId, c));
                c++;
            }
            return fingers;
        }
    }
}