namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
    using System.Collections.Generic;
    using System.Linq;

    using Soundfingerprinting.DuplicatesDetector.Model;

    /// <summary>
    ///   Class for RAM storage of hashes
    /// </summary>
    public class RamStorage : IStorage
    {
        private static readonly object LockObject = new object();

        /// <summary>
        ///   Number of hash tables
        /// </summary>
        private readonly int numberOfHashTables;

        /// <summary>
        ///   Fingerprints that correspond to the specific tracks
        /// </summary>
        /// <remarks>
        ///   Each track has a set of fingerprints which in turn has 25 hash buckets
        /// </remarks>
        private Dictionary<Track, Hashes> fingerprints;

        /// <summary>
        ///   Hash tables
        /// </summary>
        /// <remarks>
        ///   Key - hash value (hash bucket) / Value - Set of track objects (unique set)
        /// </remarks>
        private Dictionary<int, HashSet<Track>>[] hashTables;

        /// <summary>
        /// Initializes a new instance of the <see cref="RamStorage"/> class. 
        /// </summary>
        /// <param name="numberOfHashTables">
        /// Number of hash tables in the RAM Storage
        /// </param>
        public RamStorage(int numberOfHashTables)
        {
            this.numberOfHashTables = numberOfHashTables;
            hashTables = new Dictionary<int, HashSet<Track>>[this.numberOfHashTables];
            for (int i = 0; i < this.numberOfHashTables; i++)
            {
                hashTables[i] = new Dictionary<int, HashSet<Track>>();
            }

            fingerprints = new Dictionary<Track, Hashes>();
        }

        #region IStorage Members

        /// <summary>
        ///   Insert a track into the RAM Storage
        /// </summary>
        /// <param name = "track">Track to be inserted</param>
        public void InsertTrack(Track track)
        {
            lock (LockObject)
            {
                if (!fingerprints.ContainsKey(track))
                {
                    fingerprints[track] = new Hashes();
                }
            }
        }

        /// <summary>
        ///   Remove track from the RAM storage
        /// </summary>
        /// <param name = "track">Track to be removed</param>
        public void RemoveTrack(Track track)
        {
            if (fingerprints.ContainsKey(track))
            {
                fingerprints.Remove(track);
            }
        }

        /// <summary>
        ///   Clear all data from the storage
        /// </summary>
        public void ClearAll()
        {
            hashTables = new Dictionary<int, HashSet<Track>>[numberOfHashTables];
            for (int i = 0; i < numberOfHashTables; i++)
            {
                hashTables[i] = new Dictionary<int, HashSet<Track>>();
            }

            fingerprints = new Dictionary<Track, Hashes>();
        }

        /// <summary>
        ///   Insert hash into the RAM Storage. Be careful, there should be a Track object already inserted into the Storage.
        /// </summary>
        /// <param name = "hash">Hash signature that corresponds to a specific track</param>
        /// <param name = "type">Type of the hash to be inserted</param>
        public void InsertHash(HashSignature hash, HashType type)
        {
            switch (type)
            {
                case HashType.Query:
                    fingerprints[hash.Track].Query.Add(hash);
                    break;
                case HashType.Creational:
                {
                    fingerprints[hash.Track].Creational.Add(hash);
                    int[] signature = hash.Signature;
                    /*Lock insertion in the hash-tables as it keys are verified*/
                    lock (hashTables.SyncRoot) 
                    {
                        for (int i = 0; i < numberOfHashTables; i++)
                        {
                            if (!hashTables[i].ContainsKey(signature[i]))
                            {
                                hashTables[i][signature[i]] = new HashSet<Track>();
                            }

                            hashTables[i][signature[i]].Add(hash.Track);
                        }
                    }
                }

                    break;
            }
        }

        /// <summary>
        ///   Get tracks that correspond to a specific hash signature and pass the threshold value
        /// </summary>
        /// <param name = "hashSignature">Hash signature of the track</param>
        /// <param name = "hashTableThreshold">Number of threshold tables</param>
        /// <returns>Possible candidates</returns>
        public Dictionary<Track, int> GetTracks(HashSignature hashSignature, int hashTableThreshold)
        {
            Dictionary<Track, int> result = new Dictionary<Track, int>();
            int[] signature = hashSignature.Signature;

            // loop through all 25 hash tables
            for (int i = 0; i < numberOfHashTables; i++)
            {
                if (hashTables[i].ContainsKey(signature[i]))
                {
                    HashSet<Track> tracks = hashTables[i][signature[i]]; // get the set of tracks that map to a specific hash signature

                    // select all tracks except the original one
                    foreach (Track track in tracks.Where(t => t.Id != hashSignature.Track.Id))
                    {
                        if (!result.ContainsKey(track))
                        {
                            result[track] = 1;
                        }
                        else
                        {
                            result[track]++;
                        }
                    }
                }
            }

            // select only those tracks that passed threshold votes
            Dictionary<Track, int> filteredResult = result.Where(item => item.Value >= hashTableThreshold).ToDictionary(item => item.Key, item => item.Value);
            return filteredResult;
        }

        /// <summary>
        ///   Gets the list of hash signatures that are available in the storage for a specific track
        /// </summary>
        /// <param name = "track">Requested track</param>
        /// <param name = "type">Type of the hashes toe gathered</param>
        /// <returns>A set of fingerprints (hash signatures) that correspond to a specific track id</returns>
        public HashSet<HashSignature> GetHashSignatures(Track track, HashType type)
        {
            if (fingerprints.ContainsKey(track))
            {
                switch (type)
                {
                    case HashType.Creational:
                        return fingerprints[track].Creational;
                    case HashType.Query:
                        return fingerprints[track].Query;
                }
            }

            return null;
        }

        /// <summary>
        ///   Get all tracks from the repository
        /// </summary>
        /// <returns>Tracks from the repository</returns>
        public List<Track> GetAllTracks()
        {
            return fingerprints.Keys.ToList();
        }

        #endregion
    }
}