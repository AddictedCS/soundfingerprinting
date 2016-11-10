namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;

    public interface ITrackDao
    {
        IModelReference InsertTrack(TrackData track);

        TrackData ReadTrack(IModelReference trackReference);

        int DeleteTrack(IModelReference trackReference);

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByISRC(string isrc);

        IList<TrackData> ReadAll();
    }
}