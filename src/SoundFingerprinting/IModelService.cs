namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IModelService
    {
        IEnumerable<ModelServiceInfo> Info { get; }

        void Insert(TrackInfo trackInfo, Hashes hashes);

        IEnumerable<SubFingerprintData> Query(IEnumerable<int[]> hashes, QueryConfiguration config);
        
        int DeleteTrack(string trackId);

        IEnumerable<TrackData> ReadAllTracks();

        IEnumerable<TrackData> ReadTrackByTitle(string title);

        TrackData ReadTrackById(string trackId);

        IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references);
    }
}
