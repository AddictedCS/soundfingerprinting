﻿namespace SoundFingerprinting.Math
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query;

    internal class SimilarityUtility : ISimilarityUtility
    {
        private readonly IHashConverter hashConverter;

        public SimilarityUtility() : this(DependencyResolver.Current.Get<IHashConverter>())
        {
        }

        internal SimilarityUtility(IHashConverter hashConverter)
        {
            this.hashConverter = hashConverter;
        }

        public int CalculateHammingDistance(byte[] a, byte[] b)
        {
            int distance = 0;
            for (int i = 0, n = a.Length; i < n; i++)
            {
                if (a[i] != b[i])
                {
                    distance++;
                }
            }

            return distance;
        }

        public int CalculateHammingSimilarity(byte[] a, byte[] b)
        {
            return a.Length - CalculateHammingDistance(a, b);
        }

        /// <summary>
        ///   Calculate similarity between 2 fingerprints.
        /// </summary>
        /// <param name = "x">Fingerprint x</param>
        /// <param name = "y">Fingerprint y</param>
        /// <returns>Jaccard similarity between array X and array Y</returns>
        /// <remarks>
        ///   Similarity defined as  (A intersection B)/(A union B)
        ///   for types of columns a (1,1), b(1,0), c(0,1) and d(0,0), it will be equal to
        ///   Sim(x,y) = a/(a+b+c)
        ///   +1 = 10
        ///   -1 = 01
        ///   0 = 00
        /// </remarks>
        public double CalculateJaccardSimilarity(bool[] x, bool[] y)
        {
            int a = 0, b = 0;
            for (int i = 0, n = x.Length; i < n; i++)
            {
                if (x[i] && y[i]) 
                {
                    // 1 1
                    a++;
                }
                else if ((x[i] && !y[i]) || (!x[i] && y[i])) 
                {
                    // 1 0 || 0 1 
                    b++;
                }
            }

            if (a + b == 0)
            {
                return 0;
            }

            return (double)a / (a + b);
        }

        public void AccumulateHammingSimilarity(IEnumerable<SubFingerprintData> candidates, HashedFingerprint expected, ConcurrentDictionary<IModelReference, ResultEntryAccumulator> accumulator)
        {
            foreach (var subFingerprint in candidates)
            {
                byte[] signature = hashConverter.ToBytes(subFingerprint.Hashes, expected.SubFingerprint.Length);
                int hammingSimilarity = CalculateHammingSimilarity(expected.SubFingerprint, signature);
                SubFingerprintData fingerprint = subFingerprint;
                accumulator.AddOrUpdate(
                    subFingerprint.TrackReference,
                    reference => new ResultEntryAccumulator(expected, fingerprint, hammingSimilarity),
                    (reference, entryAccumulator) => entryAccumulator.Add(expected, fingerprint, hammingSimilarity));
            }
        }
    }
}
