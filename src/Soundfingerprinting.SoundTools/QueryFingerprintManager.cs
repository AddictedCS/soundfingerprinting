namespace Soundfingerprinting.SoundTools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.Hashing;

    public static class QueryFingerprintManager
    {
        private static readonly Random Random = new Random(unchecked((int) DateTime.Now.Ticks));

        /// <summary>
        ///   Query one specific song using MinHash algorithm. ConnectionString is set by the caller.
        /// </summary>
        /// <param name = "signatures">Fingerprint signatures from a song</param>
        /// <param name = "dalManager">DAL Manager used to query the underlying database</param>
        /// <param name = "permStorage">Permutation storage</param>
        /// <param name = "seconds">Fingerprints to consider as query points [1.4 sec * N]</param>
        /// <param name = "lshHashTables">Number of hash tables from the database</param>
        /// <param name = "lshGroupsPerKey">Number of groups per hash table</param>
        /// <param name = "thresholdTables">Threshold percentage [0.07 for 20 LHash Tables, 0.17 for 25 LHashTables]</param>
        /// <param name = "queryTime">Set but the method, representing the query length</param>
        /// <returns>Dictionary with Tracks ID's and the Query Statistics</returns>
        public static Dictionary<Int32, QueryStats> QueryOneSongMinHash(
            IEnumerable<bool[]> signatures,
            DaoGateway dalManager,
            IPermutations permStorage,
            int seconds,
            int lshHashTables,
            int lshGroupsPerKey,
            int thresholdTables,
            ref long queryTime)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Dictionary<int, QueryStats> stats = new Dictionary<int, QueryStats>();
            MinHash minHash = new MinHash(permStorage);
            foreach (bool[] signature in signatures)
            {
                if (signature == null)
                {
                    continue;
                }

                int[] bin = minHash.ComputeMinHashSignature(signature);
                    /*Compute Min Hash on randomly selected fingerprints*/
                Dictionary<int, long> hashes = minHash.GroupMinHashToLSHBuckets(bin, lshHashTables, lshGroupsPerKey); /*Find all candidates by querying the database*/
                long[] hashbuckets = hashes.Values.ToArray();
                Dictionary<int, List<HashBinMinHash>> candidates = dalManager.ReadFingerprintsByHashBucketLSH(hashbuckets);
                Dictionary<int, List<HashBinMinHash>> potentialCandidates = SelectPotentialMatchesOutOfEntireDataset(candidates, thresholdTables);
                if (potentialCandidates.Count > 0)
                {
                    List<Fingerprint> fingerprints = dalManager.ReadFingerprintById(potentialCandidates.Keys);
                    Dictionary<Fingerprint, int> finalCandidates = fingerprints.ToDictionary(finger => finger, finger => potentialCandidates[finger.Id].Count);
                    ArrangeCandidatesAccordingToFingerprints(
                        signature, finalCandidates, lshHashTables, lshGroupsPerKey, stats);
                }
            }
            stopWatch.Stop();
            queryTime = stopWatch.ElapsedMilliseconds; /*Set the query Time parameter*/
            return stats;
        }

        public static Dictionary<Int32, QueryStats> QueryOneSongMinHashFast(
            string pathToSong,
            IStride queryStride,
            IAudioService proxy,
            DaoGateway dalManager,
            int seconds,
            int lshHashTables,
            int lshGroupsPerKey,
            int thresholdTables,
            int topWavelets,
            ref long queryTime)
        {
            ///*Fingerprint service*/
            //fingerprintService service = new fingerprintService {TopWavelets = topWavelets};
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            //int startIndex = -1;
            //Dictionary<Int32, QueryStats> stats = new Dictionary<Int32, QueryStats>();
            //int lenOfQuery = service.SampleRate*seconds;
            //double[] samples = service.GetSamplesFromSong(proxy, pathToSong);
            //int startOfQuery =  Random.Next(0, samples.Length - lenOfQuery);
            //double[] querySamples = new double[lenOfQuery];
            //Array.Copy(samples, startOfQuery, querySamples, 0, lenOfQuery);
            //startIndex = startOfQuery/service.SampleRate;
            //MinHash minHash = new MinHash(dalManager);

            //IStride stride = queryStride;
            //int index = stride.FirstStrideSize();
            //while (index + service.SamplesPerFingerprint < querySamples.Length)
            //{
            //    Fingerprint f = service.CreateFingerprintFromSamplesArray(querySamples, index);
            //    if (f == null) continue;
            //    index += service.SamplesPerFingerprint + stride.StrideSize();
            //    int[] bin = minHash.ComputeMinHashSignature(f); /*Compute Min Hash on randomly selected fingerprints*/
            //    Dictionary<int, long> hashes = minHash.GroupMinHashToLSHBuckets(bin, lshHashTables, lshGroupsPerKey); /*Find all candidates by querying the database*/
            //    long[] hashbuckets = hashes.Values.ToArray();
            //    var candidates = dalManager.ReadFingerprintsByHashBucketLSH(hashbuckets, thresholdTables);
            //    if (candidates != null && candidates.Count > 0)
            //    {               
            //        var query = (from candidate in candidates
            //                    select candidate.Value.Item1).Distinct();

            //        foreach (var item in query)
            //        {
            //            stats.Add(item, new QueryStats(0, 0, 0, startIndex, startIndex + seconds, 0));
            //        }

            //        break;
            //    }
            //}
            //stopWatch.Stop();
            //queryTime = stopWatch.ElapsedMilliseconds; /*Set the query Time parameter*/
            return null;
        }

        /// <summary>
        ///   Arrange candidates according to the corresponding calculation between initial fingerprint and actual fingerprint
        /// </summary>
        /// <param name = "f">Actual fingerprint gathered from the song</param>
        /// <param name = "potentialCandidates">Potential fingerprints returned from the database</param>
        /// <param name = "lHashTables">Number of L Hash tables</param>
        /// <param name = "kKeys">Number of keys per table</param>
        /// <param name = "trackIdQueryStats">Result set</param>
        /// <returns>Result set</returns>
        private static Dictionary<Int32, QueryStats> ArrangeCandidatesAccordingToFingerprints(bool[] f, Dictionary<Fingerprint, int> potentialCandidates,
                                                                                              int lHashTables, int kKeys, Dictionary<Int32, QueryStats> trackIdQueryStats)
        {
            /*Most time consuming method while performing the necessary calculation*/
            foreach (KeyValuePair<Fingerprint, int> pair in potentialCandidates)
            {
                Fingerprint fingerprint = pair.Key;
                int tableVotes = pair.Value;
                /*Compute Hamming Distance of actual and read fingerprint*/
                int hammingDistance = MinHash.CalculateHammingDistance(f, fingerprint.Signature)*tableVotes;
                double jaqSimilarity = MinHash.CalculateSimilarity(f, fingerprint.Signature);
                /*Add to sample set*/
                Int32 trackId = fingerprint.TrackId;
                if (!trackIdQueryStats.ContainsKey(trackId))
                    trackIdQueryStats.Add(trackId, new QueryStats(0, 0, 0, -1, -1, 0, Int32.MinValue, 0, Int32.MaxValue, Int32.MinValue, Int32.MinValue, Double.MaxValue));
                QueryStats stats = trackIdQueryStats[trackId];
                stats.HammingDistance += hammingDistance; /*Sum hamming distance of each potential candidate*/
                stats.NumberOfTrackIdOccurences++; /*Increment occurrence count*/
                stats.NumberOfTotalTableVotes += tableVotes; /*Find total table votes*/
                stats.HammingDistanceByTrack += hammingDistance/tableVotes; /*Find hamming distance by track id occurrence*/
                if (stats.MinHammingDistance > hammingDistance/tableVotes) /*Find minimal hamming distance over the entire set*/
                    stats.MinHammingDistance = hammingDistance/tableVotes;
                if (stats.MaxTableVote < tableVotes) /*Find maximal table vote*/
                    stats.MaxTableVote = tableVotes;
                if (stats.Similarity > jaqSimilarity)
                    stats.Similarity = jaqSimilarity;
            }
            return trackIdQueryStats;
        }

        ///// <summary>
        /////   Query the database for one song
        ///// </summary>
        ///// <param name = "ensemble">Network ensemble to generate hash values</param>
        ///// <param name = "pathToSong">Path to song</param>
        ///// <param name = "queryStride">Query stride</param>
        ///// <param name = "proxy">DSP Proxy used to read from file</param>
        ///// <param name = "dalManager">Dal Manager used to query the database</param>
        ///// <param name = "fingerprintsToConsider">Number of fingerprints to consider</param>
        ///// <param name = "queryTime"></param>
        ///// <returns>Dictionary with Track id and it's associated query statistics</returns>
        //public static Dictionary<Int32, QueryStats> QueryOneSongNeuralHasher(NNEnsemble ensemble, string pathToSong, IStride queryStride, IAudioService proxy, DaoGateway dalManager, int fingerprintsToConsider, ref long queryTime)
        //{
        //    fingerprintService service = new fingerprintService();
        //    /*Create Fingerprints from file*/
        //    Stopwatch watch = new Stopwatch();
        //    watch.Start();
        //   List<bool[]> signatures = service.CreateFingerprintsFromSpectrum(proxy, pathToSong, queryStride);
        //   var listOfFingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, Int32.MinValue);
        //    if (listOfFingerprints.Count == 0)
        //            return null;
        //    Dictionary<Int32, QueryStats> allCandidates = new Dictionary<Int32, QueryStats>();
        //    for (int index = 0; index < fingerprintsToConsider; index++)
        //    {
        //        Fingerprint f = listOfFingerprints[index];
        //        ensemble.ComputeHash(ArrayUtils.GetFloatArrayFromByte(ArrayUtils.GetByteArrayFromBool(f.Signature)));
        //        long[] bin = ensemble.ExtractHashBins();
        //        int[] tables = new int[bin.Length];
        //        for (int i = 0; i < bin.Length; i++)
        //            tables[i] = i;
        //        Dictionary<Int32, int> candidates = dalManager.ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasher(bin, tables);
        //        if (candidates == null)
        //            continue;
        //        foreach (KeyValuePair<Int32, int> item in candidates)
        //        {
        //            if (!allCandidates.ContainsKey(item.Key))
        //                allCandidates.Add(item.Key, new QueryStats(0, item.Value, 0, 0, 0, 0));
        //            else
        //                allCandidates[item.Key].NumberOfTrackIdOccurences += item.Value;
        //        }
        //    }
        //    watch.Stop();
        //    return allCandidates;
        //}

        /// <summary>
        ///   Select potential matches out of the entire dataset
        /// </summary>
        /// <param name = "dataset">Dataset to consider</param>
        /// <param name = "thresholdTables">Threshold tables</param>
        /// <returns>Sub dictionary</returns>
        public static Dictionary<Int32, List<HashBinMinHash>>
            SelectPotentialMatchesOutOfEntireDataset(Dictionary<Int32, List<HashBinMinHash>> dataset, int thresholdTables)
        {
            Dictionary<Int32, List<HashBinMinHash>> result = new Dictionary<Int32, List<HashBinMinHash>>();
            if (dataset == null) return result;

            foreach (KeyValuePair<Int32, List<HashBinMinHash>> item in dataset)
            {
                if (item.Value.Count >= thresholdTables)
                {
                    List<int> tables = new List<int>();
                    foreach (HashBinMinHash hashes in item.Value)
                    {
                        if (!tables.Contains(hashes.HashTable))
                            tables.Add(hashes.HashTable);
                    }
                    if (tables.Count >= thresholdTables)
                        result.Add(item.Key, item.Value);
                }
            }
            return result;
        }
    }
}