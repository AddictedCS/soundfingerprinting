namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface ITrackDao
    {
        IModelReference Insert(TrackData track);

        IList<TrackData> ReadAll();

        TrackData ReadById(IModelReference trackReference);

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByISRC(string isrc);

        int DeleteTrack(IModelReference trackReference);
    }
}