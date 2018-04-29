namespace SoundFingerprinting.Math
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    internal interface ISimilarityUtility
    {
        int CalculateHammingDistance(byte[] a, byte[] b);

        int CalculateHammingSimilarity(byte[] a, byte[] b);

        int CalculateHammingSimilarity(int[] expected, int[] actual, int setBytesPerLong);

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
        double CalculateJaccardSimilarity(bool[] x, bool[] y);
    }
}