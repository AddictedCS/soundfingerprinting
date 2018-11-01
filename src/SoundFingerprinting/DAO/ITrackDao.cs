namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface ITrackDao
    {
        int Count { get; }

        TrackData InsertTrack(TrackInfo track);

        void InsertTrack(TrackData track);

        TrackData ReadTrack(IModelReference trackReference);

        IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references);

        int DeleteTrack(IModelReference trackReference);

        IEnumerable<TrackData> ReadTrackByTitle(string title);

        TrackData ReadTrackById(string id);

        IEnumerable<TrackData> ReadAll();
    }
}