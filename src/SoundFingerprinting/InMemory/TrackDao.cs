namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class TrackDao : ITrackDao
    {
        private static int counter;

        private readonly IRAMStorage storage;

        public TrackDao()
            : this(DependencyResolver.Current.Get<IRAMStorage>())
        {
            // no op   
        }

        public TrackDao(IRAMStorage storage)
        {
            this.storage = storage;
        }

        public IModelReference InsertTrack(TrackData track)
        {
            var trackReference = new ModelReference<int>(Interlocked.Increment(ref counter));
            storage.Tracks[trackReference] = track;
            return track.TrackReference = trackReference;
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
            if (storage.Tracks.ContainsKey(trackReference))
            {
                return storage.Tracks[trackReference];
            }

            return null;
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            int count = 0;
            if (storage.Tracks.Remove(trackReference))
            {
                count++;
                if (storage.Fingerprints.ContainsKey(trackReference))
                {
                    count += storage.Fingerprints[trackReference].Count;
                    storage.Fingerprints.Remove(trackReference);
                }

                var subFingerprintReferences = storage.SubFingerprints
                                 .Where(pair => pair.Value.TrackReference.Equals(trackReference))
                                 .Select(pair => pair.Key)
                                 .ToList();

                count += subFingerprintReferences.Count;
                foreach (var subFingerprintReference in subFingerprintReferences)
                {
                    storage.SubFingerprints.Remove(subFingerprintReference);
                }

                foreach (var hashTable in storage.HashTables)
                {
                    foreach (var hashBins in hashTable)
                    {
                        foreach (var subFingerprintReference in subFingerprintReferences)
                        {
                            if (hashBins.Value.Remove(subFingerprintReference))
                            {
                                count++;
                            }
                        }
                    }
                }

                if (storage.TracksHashes.ContainsKey(trackReference))
                {
                    storage.TracksHashes.Remove(trackReference);
                }
            }

            return count;
        }
    }
}
