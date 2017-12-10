namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IModelService
    {
        IList<SubFingerprintData> ReadSubFingerprints(int[] hashBins, QueryConfiguration config);

        ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, QueryConfiguration config);

        IModelReference InsertTrack(TrackData track);

        void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference);

        IList<HashedFingerprint> ReadHashedFingerprintsByTrack(IModelReference trackReference);
            
        IList<TrackData> ReadAllTracks();

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByReference(IModelReference trackReference);

        TrackData ReadTrackByISRC(string isrc);

        List<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> ids);

        int DeleteTrack(IModelReference trackReference);

        bool ContainsTrack(string isrc, string artist, string title);

        bool SupportsBatchedSubFingerprintQuery { get; }
    }
}
