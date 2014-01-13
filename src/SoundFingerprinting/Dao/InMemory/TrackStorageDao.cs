namespace SoundFingerprinting.Dao.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Data;

    internal class TrackStorageDao
    {
        private static int counter;

        private readonly object lockObject = new object();

        private readonly ConcurrentDictionary<int, TrackData> storage;

        public TrackStorageDao()
        {
            storage = new ConcurrentDictionary<int, TrackData>();
        }

        public int Insert(TrackData track)
        {
            lock (lockObject)
            {
                counter++;
                storage[counter] = track;
                IModelReference trackReference = new ModelReference<int>(counter);
                track.TrackReference = trackReference;
                return counter;
            }
        }

        public TrackData ReadByISRC(string isrc)
        {
            return storage.FirstOrDefault(pair => pair.Value.ISRC == isrc).Value;
        }

        public IList<TrackData> ReadAll()
        {
            return storage.Values.ToList();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return storage.Where(pair => pair.Value.Artist == artist && pair.Value.Title == title)
                          .Select(pair => pair.Value)
                          .ToList();
        }

        public TrackData ReadById(int id)
        {
            if (storage.ContainsKey(id))
            {
                return storage[id];
            }

            return null;
        }
    }
}
