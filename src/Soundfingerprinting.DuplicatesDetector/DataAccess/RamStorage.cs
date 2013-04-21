// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Linq;
using Soundfingerprinting.DuplicatesDetector.Model;

namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
    /// <summary>
    ///   Class for RAM storage of hashes
    /// </summary>
    public class RamStorage : IStorage
    {
        private static readonly object LockObject = new object();

        /// <summary>
        ///   Number of hash tables
        /// </summary>
        private readonly int _numberOfHashTables;

        /// <summary>
        ///   Fingerprints that correspond to the specific tracks
        /// </summary>
        /// <remarks>
        ///   Each track has a set of fingerprints which in turn has 25 hash buckets
        /// </remarks>
        private Dictionary<Track, Hashes> _fingerprints;

        /// <summary>
        ///   Hash tables
        /// </summary>
        /// <remarks>
        ///   Key - hash value (hash bucket) / Value - Set of track objects (unique set)
        /// </remarks>
        private Dictionary<Int32, HashSet<Track>>[] _hashTables;

        /// <summary>
        ///   RAM storage constructor
        /// </summary>
        /// <param name = "numberOfHashTables">Number of hash tables in the RAM Storage</param>
        public RamStorage(int numberOfHashTables)
        {
            _numberOfHashTables = numberOfHashTables;
            _hashTables = new Dictionary<int, HashSet<Track>>[_numberOfHashTables];
            for (int i = 0; i < _numberOfHashTables; i++)
                _hashTables[i] = new Dictionary<int, HashSet<Track>>();
            _fingerprints = new Dictionary<Track, Hashes>();
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
                if (!_fingerprints.ContainsKey(track))
                    _fingerprints[track] = new Hashes();
            }
        }

        /// <summary>
        ///   Remove track from the RAM storage
        /// </summary>
        /// <param name = "track"></param>
        public void RemoveTrack(Track track)
        {
            if (_fingerprints.ContainsKey(track))
                _fingerprints.Remove(track);
        }

        /// <summary>
        ///   Clear all data from the storage
        /// </summary>
        public void ClearAll()
        {
            _hashTables = new Dictionary<int, HashSet<Track>>[_numberOfHashTables];
            for (int i = 0; i < _numberOfHashTables; i++)
                _hashTables[i] = new Dictionary<int, HashSet<Track>>();
            _fingerprints = new Dictionary<Track, Hashes>();
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
                    _fingerprints[hash.Track].Query.Add(hash);
                    break;
                case HashType.Creational:
                {
                    _fingerprints[hash.Track].Creational.Add(hash);
                    int[] signature = hash.Signature;
                    lock (_hashTables.SyncRoot) /*Lock insertion in the hash-tables as it keys are verified*/
                    {
                        for (int i = 0; i < _numberOfHashTables; i++)
                        {
                            if (!_hashTables[i].ContainsKey(signature[i]))
                                _hashTables[i][signature[i]] = new HashSet<Track>();
                            _hashTables[i][signature[i]].Add(hash.Track);
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
        public Dictionary<Track, int> GetTracks(int[] hashSignature, int hashTableThreshold)
        {
            Dictionary<Track, int> result = new Dictionary<Track, int>();
            HashSet<Track> voted = new HashSet<Track>();
            for (int i = 0; i < _numberOfHashTables; i++) //loop through all 25 hash tables
            {
                voted.Clear();
                //for (int j = 0; j < _numberOfHashTables; j++) //lookup all 25 hash buckets in all 25 tables
                {
                    if (_hashTables[i].ContainsKey(hashSignature[i]))
                    {
                        HashSet<Track> tracks = _hashTables[i][hashSignature[i]]; //get the set of tracks that map to a specific hash signature
                        foreach (Track track in tracks.Where(track => !voted.Contains(track))) //select those that hasn't been voted by this table yet
                        {
                            if (!result.ContainsKey(track))
                                result[track] = 1;
                            else
                                result[track]++;
                            voted.Add(track); //this specific table voted for this track, do not allow it doing twice
                        }
                    }
                }
            }

            //select only those tracks that passed 8 votes
            Dictionary<Track, int> filteredResult = result.Where(item => item.Value >= hashTableThreshold)
                .ToDictionary(item => item.Key, item => item.Value);
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
            if (_fingerprints.ContainsKey(track))
            {
                switch (type)
                {
                    case HashType.Creational:
                        return _fingerprints[track].Creational;
                    case HashType.Query:
                        return _fingerprints[track].Query;
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
            return _fingerprints.Keys.ToList();
        }

        #endregion
    }
}