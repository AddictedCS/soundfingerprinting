namespace SoundFingerprinting.Hashing.Utils
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    ///   Hash class used in hashing purposes (projection hash with random permutations used)
    /// </summary>
    public class Hash
    {
        /// <summary>
        ///   Lock object for multithreading purposes
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        ///   Random number generator used for seed generation
        /// </summary>
        private readonly RandomNumberGenerator rng = new RNGCryptoServiceProvider();

        /// <summary>
        ///   Matrix of random projection values
        /// </summary>
        private int[][] randomMatrix;

        /// <summary>
        ///   Generates a random matrix for projection purposes
        /// </summary>
        /// <param name = "cols">Number of columns</param>
        /// <param name = "rows">Number of rows</param>
        /// <param name = "min">Min value in the matrix</param>
        /// <param name = "max">Max value in the matrix</param>
        private void GenerateRandomMatrixOfComponents(int cols, int rows, int min, int max)
        {
            lock (this.lockObject)
            {
                if (this.randomMatrix == null)
                {
                    byte[] seed = new byte[32];
                    this.rng.GetBytes(seed);
                    Random random = new Random(BitConverter.ToInt32(seed, 0));
                    int[][] randomArray = new int[rows][];

                    for (int i = 0; i < rows; i++)
                    {
                        randomArray[i] = new int[cols];
                        for (int j = 0; j < cols; j++)
                            randomArray[i][j] = random.Next(min, max);
                    }
                    this.randomMatrix = randomArray;
                }
            }
        }

        /// <summary>
        ///   Get a random matrix for projection purposes
        /// </summary>
        /// <param name = "cols">Number of columns</param>
        /// <param name = "rows">Number of rows</param>
        /// <param name = "min">Min value in the matrix</param>
        /// <param name = "max">Max value in the matrix</param>
        /// <returns>Random matrix</returns>
        public int[][] GetRandomMatrix(int cols, int rows, int min, int max)
        {
            if (this.randomMatrix == null)
                this.GenerateRandomMatrixOfComponents(cols, rows, min, max);
            return this.randomMatrix;
        }

        /// <summary>
        ///   Set random projection matrix
        /// </summary>
        /// <param name = "randomVector"></param>
        public void SetRandomMatrix(int[][] randomVector)
        {
            this.randomMatrix = randomVector;
        }

        /// <summary>
        ///   Get hashed buckets of the signature
        /// </summary>
        /// <param name = "signature">The signature hash</param>
        /// <param name = "hashFunctions">Number of hash functions (bands)</param>
        /// <param name = "hashKeys">Number of hash keys (rows)</param>
        /// <param name = "b">B random integer</param>
        /// <param name = "w">Width of the bin</param>
        /// <returns>Array of buckets</returns>
        /// <remarks>
        ///   h(p) = (p * v + b)/w, where p,v are vectors b, w are integers
        /// </remarks>
        public long[] GetBuckets(int[] signature, int hashFunctions, int hashKeys, int b, int w)
        {
            long[] hashes = new long[hashFunctions];
            for (int i = 0; i < hashFunctions /*hash functions*/; i++)
            {
                for (int j = 0; j < hashKeys /*r min hash signatures*/; j++)
                {
                    hashes[i] += (signature[i*hashKeys + j]*this.randomMatrix[i][j] + b);
                }
                hashes[i] /= w;
            }
            return hashes;
        }
    }
}