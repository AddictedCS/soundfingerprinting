namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal class TrackDao : ITrackDao
    {
        private readonly IRAMStorage storage;

        public TrackDao(IRAMStorage storage)
        {
            this.storage = storage;
        }

        public int Count
        {
            get
            {
                return storage.Tracks.Count;
            }
        }

        public TrackData InsertTrack(TrackInfo track)
        {
            return storage.AddTrack(track);
        }

        public void InsertTrack(TrackData track)
        {
            storage.AddTrack(track);
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return storage.Tracks.FirstOrDefault(pair => pair.Value.ISRC == isrc).Value;
        }

        public IList<TrackData> ReadAll()
        {
            return storage.Tracks.Values.ToList();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return storage.Tracks.Where(pair => pair.Value.Artist == artist && pair.Value.Title == title)
                          .Select(pair => pair.Value)
                          .ToList();
        }

        public TrackData ReadTrack(IModelReference trackReference)
        {
            if (storage.Tracks.ContainsKey((int)trackReference.Id))
            {
                return storage.Tracks[(int)trackReference.Id];
            }

            return null;
        }

        public List<TrackData> ReadTracks(IEnumerable<IModelReference> ids)
        {
            return ids.Select(ReadTrack).Where(track => track != null).ToList();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            return storage.DeleteTrack(trackReference);
        }
    }
}
