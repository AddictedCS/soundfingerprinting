namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IModelService
    {
        IModelReference InsertTrack(TrackData track);

        void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference);

        IList<HashedFingerprint> ReadHashedFingerprintsByTrack(IModelReference trackReference);
            
        IList<TrackData> ReadAllTracks();

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByReference(IModelReference trackReference);

        TrackData ReadTrackByISRC(string isrc);

        int DeleteTrack(IModelReference trackReference);

        IList<SubFingerprintData> ReadSubFingerprints(long[] hashBins, QueryConfiguration config);

        ISet<SubFingerprintData> ReadAllSubFingerprintCandidatesWithThreshold(IEnumerable<HashedFingerprint> hashes, int threshold);

        bool ContainsTrack(string isrc, string artist, string title);
    }
}
