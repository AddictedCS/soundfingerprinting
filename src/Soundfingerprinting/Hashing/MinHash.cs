// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;

namespace Soundfingerprinting.Hashing
{
    /// <summary>
    ///   Class for Min Hash algorithm implementation
    /// </summary>
    public class MinHash
    {
        /// <summary>
        ///   Maximum number of hash buckets in the database
        /// </summary>
        private const int HASH_BUCKET_SIZE = 100000;

        /// <summary>
        ///   The smallest Prime number that exceeds (D / 2)
        ///   <remarks>
        ///     D - domain of values. D = 2^(8*nrKeys)
        ///     5 Keys - 549755813911
        ///     4 Keys - 2147483659
        ///     3 Keys - 8388617
        ///     2 Keys - 32771
        ///   </remarks>
        /// </summary>
        private const long PRIME_P = 2147483659;

        /// <summary>
        ///   A Constant used in computation of  hash bucket value
        /// </summary>
        private const int A = 1;

        /// <summary>
        ///   B Constant used in computation of hash bucket value
        /// </summary>
        private const int B = 0;

        /// <summary>
        ///   Permutations dictionary
        /// </summary>
        private readonly int[][] _permutations;

        /// <summary>
        ///   Number of permutation read from the database
        /// </summary>
        /// <remarks>
        ///   Default = 100
        /// </remarks>
        private readonly int _permutationsCount;

        /// <summary>
        ///   Public constructor
        /// </summary>
        /// <param name = "permutations">Storage from which to read the permutations</param>
        public MinHash(IPermutations permutations)
        {
            _permutations = permutations.GetPermutations(); /*Read the permutation from the database*/

            if (_permutations == null || _permutations.Length == 0)
                throw new Exception("Permutations are null or not enough to create the Min Hash signature");

            _permutationsCount = _permutations.Length;
        }

        /// <summary>
        ///   Number of random permutations
        /// </summary>
        public int PermutationsCount
        {
            get { return _permutationsCount; }
        }


        /// <summary>
        ///   Compute Min Hash signature of a fingerprint
        /// </summary>
        /// <param name = "fingerprint">Fingerprint</param>
        /// <returns>MinHashes [concatenated of size PERMUTATION SIZE]</returns>
        /// <remarks>
        ///   The basic idea in the Min Hashing scheme is to randomly permute the rows and for each 
        ///   column c(i) compute its hash value h(c(i)) as the index of the first row under the permutation that has a 1 in that column.
        /// </remarks>
        public int[] ComputeMinHashSignature(bool[] fingerprint)
        {
            bool[] signature = fingerprint;
            int[] minHash = new int[_permutations.Length]; /*100*/
            for (int i = 0; i < _permutations.Length /*100*/; i++)
            {
                minHash[i] = 255; /*The probability of occurrence of 1 after position 255 is very insignificant*/
                int len = _permutations[i].Length;
                for (int j = 0; j < len /*256*/; j++)
                {
                    if (signature[_permutations[i][j]])
                    {
                        minHash[i] = j; /*Looking for first occurrence of '1'*/
                        break;
                    }
                }
            }
            return minHash; /*Array of 100 elements with bit turned ON if permutation captured successfully a TRUE bit*/
        }

        /// <summary>
        ///   Compute LSH hash buckets which will be inserted into hash tables.
        ///   Each fingerprint will have a candidate in each of the hash tables.
        /// </summary>
        /// <param name = "minHashes">Min Hashes gathered from every fingerprint [N = 100]</param>
        /// <param name = "numberOfHashTables">Number of hash tables [L = 25]</param>
        /// <param name = "numberOfMinHashesPerKey">Number of min hashes per key [N = 4]</param>
        /// <returns>Collection of Pairs with Key = Hash table index, Value = Hash bucket</returns>
        public Dictionary<int, long> GroupMinHashToLSHBuckets(int[] minHashes, int numberOfHashTables, int numberOfMinHashesPerKey)
        {
            Dictionary<int, long> result = new Dictionary<int, long>();
            const int maxNumber = 8; /*Int64 biggest value for MinHash*/
            if (numberOfMinHashesPerKey > maxNumber)
                throw new ArgumentException("numberOfMinHashesPerKey cannot be bigger than 8");
            for (int i = 0; i < numberOfHashTables /*hash functions*/; i++)
            {
                byte[] array = new byte[maxNumber];
                for (int j = 0; j < numberOfMinHashesPerKey /*r min hash signatures*/; j++)
                {
                    array[j] = (byte) minHashes[i*numberOfMinHashesPerKey + j];
                }
                long hashbucket = BitConverter.ToInt64(array, 0); //actual value of the signature
                hashbucket = ((A*hashbucket + B)%PRIME_P)%HASH_BUCKET_SIZE;
                result.Add(i, hashbucket);
            }
            return result;
        }

        /// <summary>
        ///   Calculate Hamming Distance between two fingerprints
        /// </summary>
        /// <param name = "a">Fingerprint 'A'</param>
        /// <param name = "b">Fingerprint 'B'</param>
        /// <returns>Hamming distance</returns>
        public static int CalculateHammingDistance(bool[] a, bool[] b)
        {
            int distance = 0;
            for (int i = 0, n = a.Length; i < n; i++)
                if (a[i] != b[i])
                    distance++;
            return distance;
        }

        /// <summary>
        ///   Calculate hamming distance between 2 longs
        /// </summary>
        /// <param name = "a">First item</param>
        /// <param name = "b">Second item</param>
        /// <returns>Hamming distance</returns>
        public static int CalculateHammingDistance(long a, long b)
        {
            long dist = 0, val = a ^ b;
            // Count the number of set bits
            while (val != 0)
            {
                ++dist;
                val &= val - 1;
            }
            return (int) dist;
        }

        /// <summary>
        ///   Calculate similarity between 2 fingerprints.
        /// </summary>
        /// <param name = "x">Fingerprint x</param>
        /// <param name = "y">Fingerprint y</param>
        /// <returns></returns>
        /// <remarks>
        ///   Similarity defined as  (A intersection B)/(A union B)
        ///   for types of columns a (1,1), b(1,0), c(0,1) and d(0,0), it will be equal to
        ///   Sim(x,y) = a/(a+b+c)
        /// 
        ///   +1 = 10
        ///   -1 = 01
        ///   0 = 00
        /// </remarks>
        public static double CalculateSimilarity(bool[] x, bool[] y)
        {
            int a = 0, b = 0;
            for (int i = 0, n = x.Length; i < n; i++)
            {
                if (x[i] == y[i] && x[i]) /*1 1*/
                    a++;
                else if (x[i] != y[i]) /*1 0 0 1*/
                    b++;
            }
            if (a + b == 0)
                return 0;
            return (double) a/(a + b);
        }

        /// <summary>
        ///   Calculates Jacquard similarity between 2 buckets
        /// </summary>
        /// <param name = "aBucket">First bucket</param>
        /// <param name = "bBucket">Second bucket</param>
        /// <returns></returns>
        public static double CalculateSimilarity(long aBucket, long bBucket)
        {
            byte[] aarray = BitConverter.GetBytes(aBucket);
            byte[] barray = BitConverter.GetBytes(bBucket);
            int count = aarray.Length;
            int a = 0, b = 0, d = 0;
            for (int i = 0; i < count; i++)
            {
                if (aarray[i] == barray[i] && aarray[i] != 0)
                    a++;
                else if (aarray[i] == barray[i])
                    d++;
                else
                    b++;
            }
            if (a + b == 0)
                return 0;
            return (double) a/(a + b);
        }
    }
}