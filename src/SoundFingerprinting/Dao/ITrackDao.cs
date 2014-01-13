namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal interface ITrackDao
    {
        int Insert(TrackData track);

        IList<TrackData> ReadAll();

        TrackData ReadById(int id);

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByISRC(string isrc);

        int DeleteTrack(int trackId);
    }
}