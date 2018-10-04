namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IModelService
    {
        ModelServiceInfo Info { get; }

        IModelReference Insert(TrackInfo trackInfo, IEnumerable<HashedFingerprint> hashedFingerprints);

        FingerprintsQueryResponse ReadSubFingerprints(IEnumerable<QueryHash> hashes, QueryConfiguration config);

        IEnumerable<TrackData> ReadAllTracks();

        // TODO add a fuzzy search
        IEnumerable<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        // TODO rename read by ID
        TrackData ReadTrackByISRC(string isrc);

        IEnumerable<TrackData> ReadTracksByReferences(params IModelReference[] ids);

        int DeleteTrack(IModelReference trackReference);

        bool ContainsTrack(string isrc, string artist, string title);
    }
}
