namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IModelService
    {
        IList<SubFingerprintData> ReadSubFingerprints(long[] hashBins, QueryConfiguration config);

        ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<long[]> hashes, QueryConfiguration config);

        IModelReference InsertTrack(TrackData track);

        void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference);

        IList<HashedFingerprint> ReadHashedFingerprintsByTrack(IModelReference trackReference);
            
        IList<TrackData> ReadAllTracks();

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByReference(IModelReference trackReference);

        TrackData ReadTrackByISRC(string isrc);

        int DeleteTrack(IModelReference trackReference);

        bool ContainsTrack(string isrc, string artist, string title);
    }
}
