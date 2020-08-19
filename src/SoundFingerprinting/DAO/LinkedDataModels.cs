namespace SoundFingerprinting.DAO
{
    using System.Collections;
    using System.Collections.Generic;
    using SoundFingerprinting.DAO.Data;

    public class LinkedDataModels
    {
        public LinkedDataModels(TrackData track, IEnumerable<SubFingerprintData> subFingerprints)
        {
            Track = track;
            SubFingerprints = subFingerprints;
        }
        
        public TrackData Track { get; }
        
        public IEnumerable<SubFingerprintData> SubFingerprints { get; }

        public void Deconstruct(out TrackData track, out IEnumerable<SubFingerprintData> subFingerprints)
        {
            track = Track;
            subFingerprints = SubFingerprints;
        }
    }
}