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

        IEnumerable<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, QueryConfiguration config);

        IEnumerable<TrackData> ReadAllTracks();

        IEnumerable<TrackData> ReadTrackByTitle(string title);

        TrackData ReadTrackById(string id);

        TrackData ReadTrackByReference(IModelReference reference);

        IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references);

        int DeleteTrack(IModelReference trackReference);
    }
}
