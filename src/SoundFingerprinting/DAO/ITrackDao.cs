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

        List<TrackData> ReadTracks(IEnumerable<IModelReference> ids);

        int DeleteTrack(IModelReference trackReference);

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByISRC(string isrc);

        IList<TrackData> ReadAll();
    }
}