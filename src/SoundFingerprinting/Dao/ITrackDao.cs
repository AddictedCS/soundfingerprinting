namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

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