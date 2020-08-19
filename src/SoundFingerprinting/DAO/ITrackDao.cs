namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface ITrackDao
    {
        int Count { get; }

        void InsertTrack(TrackData track);

        IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references);

        int DeleteTrack(IModelReference trackReference);

        TrackData? ReadTrackById(string id);

        IEnumerable<TrackData> ReadAll();
        
        IEnumerable<string> GetTrackIds();
    }
}