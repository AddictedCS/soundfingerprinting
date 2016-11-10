namespace SoundFingerprinting.Math
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    public class SimilarityUtility : ISimilarityUtility
    {
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

        public void AccumulateHammingSimilarity(IEnumerable<SubFingerprintData> candidates, byte[] expected, Dictionary<IModelReference, int> accumulator)
        {
            foreach (var subFingerprint in candidates)
            {
                int hammingSimilarity = CalculateHammingSimilarity(expected, subFingerprint.Signature);
                if (!accumulator.ContainsKey(subFingerprint.TrackReference))
                {
                    accumulator.Add(subFingerprint.TrackReference, 0);
                }

                accumulator[subFingerprint.TrackReference] += hammingSimilarity;
            }
        }
    }
}
