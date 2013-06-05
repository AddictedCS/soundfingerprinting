namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Soundfingerprinting.DuplicatesDetector.Model;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Strides;

    /// <summary>
    ///   Singleton class for repository container
    /// </summary>
    public class Repository
    {
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;

        private readonly ICombinedHashingAlgoritm combinedHashingAlgorithm;

        /// <summary>
        ///   Storage for hash signatures and tracks
        /// </summary>
        private readonly IStorage storage;

        public Repository(IFingerprintUnitBuilder fingerprintUnitBuilder, IStorage storage, ICombinedHashingAlgoritm combinedHashingAlgorithm)
        {
            this.combinedHashingAlgorithm = combinedHashingAlgorithm;
            this.storage = storage;
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
        }

        /// <summary>
        ///   Create fingerprints out of down sampled samples
        /// </summary>
        /// <param name = "samples">Down sampled to 5512 samples</param>
        /// <param name = "track">Track</param>
        /// <param name = "stride">Stride</param>
        /// <param name = "hashTables">Number of hash tables</param>
        /// <param name = "hashKeys">Number of hash keys</param>
        public void CreateInsertFingerprints(float[] samples, Track track, IStride stride, int hashTables, int hashKeys)
        {
            if (track == null)
            {
                return; /*track is not eligible*/
            }

            /*Create fingerprints that will be used as initial fingerprints to be queried*/
            List<bool[]> fingerprints = fingerprintUnitBuilder.BuildFingerprints()
                                                       .On(samples)
                                                       .WithCustomConfiguration(config => config.Stride = stride)
                                                       .RunAlgorithm()
                                                       .Result;

            storage.InsertTrack(track); /*Insert track into the storage*/
            /*Get signature's hash signature, and associate it to a specific track*/
            IEnumerable<HashSignature> creationalsignatures = GetSignatures(fingerprints, track, hashTables, hashKeys);
            foreach (HashSignature hash in creationalsignatures)
            {
                storage.InsertHash(hash);
            }
        }

        /// <summary>
        ///   Find duplicates between existing tracks in the database
        /// </summary>
        /// <param name = "tracks">Tracks to be processed (this list should contain only tracks that have been inserted previously)</param>
        /// <param name = "threshold">Number of threshold tables</param>
        /// <param name = "numberOfFingerprintThreshold">Number of fingerprints threshold</param>
        /// <param name = "callback">Callback invoked at each processed track</param>
        /// <returns>Sets of duplicates</returns>
        public HashSet<Track>[] FindDuplicates(List<Track> tracks, int threshold, int numberOfFingerprintThreshold, Action<Track, int, int> callback)
        {
            List<HashSet<Track>> duplicates = new List<HashSet<Track>>();
            int total = tracks.Count, current = 0;
            foreach (Track track in tracks)
            {
                Dictionary<Track, int> trackDuplicates = new Dictionary<Track, int>(); /*this will be a set with duplicates*/
                HashSet<HashSignature> fingerprints = storage.GetHashSignatures(track); /*get all existing signatures for a specific track*/
                foreach (HashSignature fingerprint in fingerprints)
                {
                    Dictionary<Track, int> results = storage.GetTracks(fingerprint, threshold); /*get all duplicate track without the original track*/
                    foreach (KeyValuePair<Track, int> result in results)
                    {
                        if (!trackDuplicates.ContainsKey(result.Key))
                        {
                            trackDuplicates.Add(result.Key, 1);
                        }
                        else
                        {
                            trackDuplicates[result.Key]++;
                        }
                    }
                }

                if (trackDuplicates.Any())
                {
                    var actualDuplicates = trackDuplicates.Where(pair => pair.Value > numberOfFingerprintThreshold).ToList();
                    if (actualDuplicates.Any())
                    {
                        HashSet<Track> duplicatePair = new HashSet<Track>(actualDuplicates.Select(pair => pair.Key)) { track };
                        duplicates.Add(duplicatePair);
                    }
                }

                if (callback != null)
                {
                    callback.Invoke(track, total, ++current);
                }
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

        private IEnumerable<HashSignature> GetSignatures(IEnumerable<bool[]> fingerprints, Track track, int hashTables, int hashKeys)
        {
            List<HashSignature> signatures = new List<HashSignature>();
            foreach (bool[] fingerprint in fingerprints)
            {
                long[] buckets = combinedHashingAlgorithm.Hash(fingerprint, hashTables, hashKeys).Item2;
                long[] hashSignature = new long[buckets.Length];
                int tableCount = 0;
                foreach (long bucket in buckets)
                {
                    hashSignature[tableCount++] = bucket;
                }

                HashSignature hash = new HashSignature(track, hashSignature); /*associate track to hash-signature*/
                signatures.Add(hash);
            }

            return signatures; /*Return the signatures*/
        }
    }
}