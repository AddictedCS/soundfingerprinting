namespace SoundFingerprinting.Services.Track
{
    using System.Collections.Generic;
    using SoundFingerprinting.Dao.Entities;

    public interface ITrackService
    {
        void InsertTrack(Track track);

        void InsertTrack(IEnumerable<Track> collection);
    }
}