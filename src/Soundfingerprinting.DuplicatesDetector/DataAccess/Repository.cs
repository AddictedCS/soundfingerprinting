// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Linq;
using Soundfingerprinting.AudioProxies.Strides;
using Soundfingerprinting.DuplicatesDetector.Model;
using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.Hashing;

namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
    /// <summary>
    ///   Singleton class for repository container
    /// </summary>
    public class Repository
    {
        /// <summary>
        ///   Min hasher
        /// </summary>
        private readonly MinHash _hasher;

        /// <summary>
        ///   Creates fingerprints according to the theoretical constructs
        /// </summary>
        private readonly FingerprintManager _manager;

        /// <summary>
        ///   Storage for min-hash permutations
        /// </summary>
        private readonly IPermutations _permutations;

        /// <summary>
        ///   Storage for hash signatures and tracks
        /// </summary>
        private readonly IStorage _storage;

        /// <summary>
        ///   Each repository should have storage for permutations and for tracks/fingerprints
        /// </summary>
        /// <param name = "storage">Track/Signatures storage</param>
        /// <param name = "permutations">Permutations storage</param>
        public Repository(IStorage storage, IPermutations permutations)
        {
            _permutations = permutations;
            _storage = storage;
            _manager = new FingerprintManager();
            _hasher = new MinHash(_permutations);
        }

        /// <summary>
        ///   Create fingerprints out of down sampled samples
        /// </summary>
        /// <param name = "samples">Down sampled to 5512 samples </param>
        /// <param name = "track">Track</param>
        /// <param name = "stride">Stride</param>
        /// <param name = "hashTables">Number of hash tables</param>
        /// <param name = "hashKeys">Number of hash keys</param>
        public void CreateInsertFingerprints(float[] samples,
                                             Track track,
                                             IStride stride,
                                             int hashTables,
                                             int hashKeys)
        {
            if (track == null) return; /*track is not eligible*/
            /*Create fingerprints that will be used as initial fingerprints to be queried*/
            List<bool[]> dbFingers = _manager.CreateFingerprints(samples, stride);
            _storage.InsertTrack(track); /*Insert track into the storage*/
            /*Get fingerprint's hash signature, and associate it to a specific track*/
            List<HashSignature> creationalsignatures = GetSignatures(dbFingers, track, hashTables, hashKeys);
            foreach (HashSignature hash in creationalsignatures)
            {
                _storage.InsertHash(hash, HashType.Creational);
                /*Set this hashes as also the query hashes*/
                _storage.InsertHash(hash, HashType.Query);
            }
            return;
        }


// ReSharper disable ReturnTypeCanBeEnumerable.Local
        private List<HashSignature> GetSignatures(IEnumerable<bool[]> fingerprints, Track track, int hashTables, int hashKeys)
// ReSharper restore ReturnTypeCanBeEnumerable.Local
        {
            List<HashSignature> signatures = new List<HashSignature>();
            foreach (bool[] fingerprint in fingerprints)
            {
                int[] signature = _hasher.ComputeMinHashSignature(fingerprint); /*Compute min-hash signature out of fingerprint*/
                Dictionary<int, long> buckets = _hasher.GroupMinHashToLSHBuckets(signature, hashTables, hashKeys); /*Group Min-Hash signature into LSH buckets*/
                int[] hashSignature = new int[buckets.Count];
                foreach (KeyValuePair<int, long> bucket in buckets)
                    hashSignature[bucket.Key] = (int) bucket.Value;
                HashSignature hash = new HashSignature(track, hashSignature); /*associate track to hash-signature*/
                signatures.Add(hash);
            }
            return signatures; /*Return the signatures*/
        }

        /// <summary>
        ///   Find duplicates between existing tracks in the database
        /// </summary>
        /// <param name = "tracks">Tracks to be processed (this list should contain only tracks that have been inserted previously)</param>
        /// <param name = "threshold">Number of threshold tables</param>
        /// <param name = "percentageThreshold">Percentage of fingerprints threshold</param>
        /// <param name = "callback">Callback invoked at each processed track</param>
        /// <returns>Sets of duplicates</returns>
        public HashSet<Track>[] FindDuplicates(List<Track> tracks, int threshold, double percentageThreshold, Action<Track, int, int> callback)
        {
            List<HashSet<Track>> duplicates = new List<HashSet<Track>>();
            int total = tracks.Count, current = 0;
            foreach (Track track in tracks)
            {
                Dictionary<Track, int> trackDuplicates = new Dictionary<Track, int>(); /*this will be a set with duplicates*/
                HashSet<HashSignature> fingerprints = _storage.GetHashSignatures(track, HashType.Query); /*get all existing signatures for a specific track*/
                int fingerthreshold = (int) ((float) fingerprints.Count/100*percentageThreshold);
                foreach (HashSignature fingerprint in fingerprints)
                {
                    Dictionary<Track, int> results = _storage.GetTracks(fingerprint.Signature, threshold); /*get all duplicate track including the original track*/
                    foreach (KeyValuePair<Track, int> result in results)
                    {
                        if (!trackDuplicates.ContainsKey(result.Key))
                            trackDuplicates.Add(result.Key, 1);
                        else
                            trackDuplicates[result.Key]++;
                    }
                }
                if (trackDuplicates.Count > 1)
                {
                    IEnumerable<KeyValuePair<Track, int>> d = trackDuplicates.Where((pair) => pair.Value > fingerthreshold);
                    if (d.Count() > 1)
                        duplicates.Add(new HashSet<Track>(d.Select((pair) => pair.Key)));
                }
                if (callback != null)
                    callback.Invoke(track, total, ++current);
            }

            for (int i = 0; i < duplicates.Count - 1; i++)
            {
                HashSet<Track> set = duplicates[i];
                for (int j = i + 1; j < duplicates.Count; j++)
                {
                    IEnumerable<Track> result = set.Intersect(duplicates[j]);
                    if (result.Count() > 0)
                    {
                        duplicates.RemoveAt(j); /*Remove the duplicate set*/
                        i = -1; /*Start iterating from the beginning*/
                        break;
                    }
                }
            }
            return duplicates.ToArray();
        }

        /// <summary>
        ///   Clear current storage
        /// </summary>
        public void ClearStorage()
        {
            _storage.ClearAll();
        }
    }
}