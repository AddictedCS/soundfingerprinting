namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO.Data;

    /// <summary>
    ///  Class that holds linked data models between track and sub-fingerprints
    /// </summary>
    public class LinkedDataModels
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedDataModels"/> class.
        /// </summary>
        /// <param name="track">Track.</param>
        /// <param name="subFingerprints">Fingerprints.</param>
        public LinkedDataModels(TrackData track, IEnumerable<SubFingerprintData> subFingerprints)
        {
            Track = track;
            SubFingerprints = subFingerprints;
        }

        /// <summary>
        /// Gets track.
        /// </summary>
        public TrackData Track { get; }
        
        /// <summary>
        /// Gets sub-fingerprints.
        /// </summary>
        public IEnumerable<SubFingerprintData> SubFingerprints { get; }

        /// <summary>
        ///  Deconstruct track and sub-fingerprints.
        /// </summary>
        /// <param name="track">Track to set.</param>
        /// <param name="subFingerprints">Sub-fingerprints to set.</param>
        public void Deconstruct(out TrackData track, out IEnumerable<SubFingerprintData> subFingerprints)
        {
            track = Track;
            subFingerprints = SubFingerprints;
        }
    }
}