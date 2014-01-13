namespace SoundFingerprinting.Dao.InMemory
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Data;

    internal class TrackStorageDao : ITrackDao
    {
        private static int counter;

        private readonly object lockObject = new object();

        private readonly RAMStorage storage;
        
        public TrackStorageDao()
        {
            storage = RAMStorage.Instance;
        }

        public int Insert(TrackData track)
        {
            lock (lockObject)
            {
                counter++;
                storage.Tracks[counter] = track;
                track.TrackReference = new ModelReference<int>(counter);
                return counter;
            }
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

        public TrackData ReadById(int id)
        {
            if (storage.Tracks.ContainsKey(id))
            {
                return storage.Tracks[id];
            }

            return null;
        }

        public int DeleteTrack(int trackId)
        {
            int count = 0;
            if (storage.Tracks.Remove(trackId))
            {
                count++;
                if (storage.Fingerprints.ContainsKey(trackId))
                {
                    count += storage.Fingerprints[trackId].Count;
                    storage.Fingerprints.Remove(trackId);
                }

                var subFingerprintIds = storage.SubFingerprints
                                 .Where(pair => pair.Value.TrackReference.Equals(trackId))
                                 .Select(pair => pair.Key)
                                 .ToList();

                count += subFingerprintIds.Count;
                foreach (var id in subFingerprintIds)
                {
                    storage.SubFingerprints.Remove(id);
                }

                foreach (var hashTable in storage.HashTables)
                {
                    foreach (var hashBins in hashTable.Value)
                    {
                        foreach (var id in subFingerprintIds)
                        {
                            lock (((ICollection)hashBins.Value).SyncRoot)
                            {
                                if (hashBins.Value.Remove(id))
                                {
                                    count++;
                                }
                            }
                        }
                    }
                }
            }

            return count;
        }
    }
}
