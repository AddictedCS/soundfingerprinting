namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.DuplicatesDetector.Model;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.Hashing;

    /// <summary>
    ///   Singleton class for repository container
    /// </summary>
    public class Repository
    {
        /// <summary>
        ///   Min hasher
        /// </summary>
        private readonly MinHash hasher;

        /// <summary>
        ///   Creates fingerprints according to the theoretical constructs
        /// </summary>
        private readonly IFingerprintService service;

        private readonly IWorkUnitBuilder workUnitBuilder;

        /// <summary>
        ///   Storage for min-hash permutations
        /// </summary>
        private readonly IPermutations permutations;

        /// <summary>
        ///   Storage for hash signatures and tracks
        /// </summary>
        private readonly IStorage storage;

        public Repository(IFingerprintService fingerprintService, IWorkUnitBuilder workUnitBuilder, IStorage storage, IPermutations permutations)
        {
            this.permutations = permutations;
            this.storage = storage;
            service = fingerprintService;
            this.workUnitBuilder = workUnitBuilder;
            hasher = new MinHash(this.permutations);
        }

        /// <summary>
        ///   Create fingerprints out of down sampled samples
        /// </summary>
        /// <param name = "samples">Down sampled to 5512 samples </param>
        /// <param name = "track">Track</param>
        /// <param name = "stride">Stride</param>
        /// <param name = "hashTables">Number of hash tables</param>
        /// <param name = "hashKeys">Number of hash keys</param>
        public void CreateInsertFingerprints(
            float[] samples,
            Track track,
            IStride stride,
                                             int hashTables,
                                             int hashKeys)
        {
            if (track == null)
            {
                return; /*track is not eligible*/
            }

            /*Create fingerprints that will be used as initial fingerprints to be queried*/
            List<bool[]> fingerprints = workUnitBuilder.BuildWorkUnit()
                                                       .On(samples)
                                                       .WithCustomConfiguration(config => config.Stride = stride)
                                                       .GetFingerprintsUsingService(service)
                                                       .Result;

            storage.InsertTrack(track); /*Insert track into the storage*/
            /*Get signature's hash signature, and associate it to a specific track*/
            List<HashSignature> creationalsignatures = GetSignatures(fingerprints, track, hashTables, hashKeys);
            foreach (HashSignature hash in creationalsignatures)
            {
                storage.InsertHash(hash, HashType.Creational);
                /*Set this hashes as also the query hashes*/
                storage.InsertHash(hash, HashType.Query);
            }
        }


        // ReSharper disable ReturnTypeCanBeEnumerable.Local
        private List<HashSignature> GetSignatures(IEnumerable<bool[]> fingerprints, Track track, int hashTables, int hashKeys)
// ReSharper restore ReturnTypeCanBeEnumerable.Local
        {
            List<HashSignature> signatures = new List<HashSignature>();
            foreach (bool[] fingerprint in fingerprints)
            {
                int[] signature = hasher.ComputeMinHashSignature(fingerprint); /*Compute min-hash signature out of signature*/
                Dictionary<int, long> buckets = hasher.GroupMinHashToLSHBuckets(signature, hashTables, hashKeys); /*Group Min-Hash signature into LSH buckets*/
                int[] hashSignature = new int[buckets.Count];
                foreach (KeyValuePair<int, long> bucket in buckets)
                {
                    hashSignature[bucket.Key] = (int)bucket.Value;
                }

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
                HashSet<HashSignature> fingerprints = storage.GetHashSignatures(track, HashType.Query); /*get all existing signatures for a specific track*/
                int fingerthreshold = (int) ((float) fingerprints.Count/100*percentageThreshold);
                foreach (HashSignature fingerprint in fingerprints)
                {
                    Dictionary<Track, int> results = storage.GetTracks(fingerprint.Signature, threshold); /*get all duplicate track including the original track*/
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
                    if (result.Any())
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
            storage.ClearAll();
        }
    }
}