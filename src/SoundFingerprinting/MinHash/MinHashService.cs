namespace SoundFingerprinting.MinHash
{
    using System;
    using SoundFingerprinting.Utils;

    /// <summary>
    /// Represents an entry in the inverted index mapping a bit position
    /// to a permutation and its rank within that permutation.
    /// </summary>
    internal readonly struct PermutationEntry(byte permutationIndex, byte rank)
    {
        /// <summary>
        /// Gets index of the permutation (0 to 99 typically).
        /// </summary>
        public byte PermutationIndex { get; } = permutationIndex;

        /// <summary>
        /// Gets rank of the bit within this permutation (0 to 255).
        /// Lower rank means higher priority in min-hash computation.
        /// </summary>
        public byte Rank { get; } = rank;
    }

    internal class MinHashService : IMinHashService<byte>
    {
        // inverted index: for each bit position, stores permutation entries, this allows single-pass scanning through set bits instead of nested loops
        private readonly PermutationEntry[][] invertedIndex;
        private readonly int maxBitIndex;

        internal MinHashService(IPermutations permutations)
        {
            // build inverted index for fast min-hash computation
            int[][] perms = permutations.GetPermutations();
            
            // find max bit index to size the inverted index
            maxBitIndex = 0;
            for (int i = 0; i < perms.Length; i++)
            {
                for (int j = 0; j < perms[i].Length; j++)
                {
                    if (perms[i][j] > maxBitIndex)
                    {
                        maxBitIndex = perms[i][j];
                    }
                }
            }

            maxBitIndex++; // convert to size

            // count how many permutations reference each bit position
            int[] counts = new int[maxBitIndex];
            for (int i = 0; i < perms.Length; i++)
            {
                for (int j = 0; j < perms[i].Length; j++)
                {
                    counts[perms[i][j]]++;
                }
            }

            // allocate inverted index arrays
            invertedIndex = new PermutationEntry[maxBitIndex][];
            for (int i = 0; i < maxBitIndex; i++)
            {
                invertedIndex[i] = counts[i] > 0 ? new PermutationEntry[counts[i]] : [];
            }

            // fill inverted index: for each bit position, store which permutations reference it and at what rank
            int[] currentIndex = new int[maxBitIndex];
            for (int permIdx = 0; permIdx < perms.Length; permIdx++)
            {
                for (int rank = 0; rank < perms[permIdx].Length; rank++)
                {
                    int bitPos = perms[permIdx][rank];
                    int idx = currentIndex[bitPos]++;
                    invertedIndex[bitPos][idx] = new PermutationEntry((byte)permIdx, (byte)rank);
                }
            }
        }

        /// <summary>
        ///  Gets old permutations, not used anymore.
        /// </summary>
        public static MinHashService Default { get; } = new MinHashService(new DefaultPermutations());

        /// <summary>
        ///  Gets max entropy permutations.
        /// </summary>
        public static MinHashService MaxEntropy { get; } = new MinHashService(new MaxEntropyPermutations());

        public byte[] Hash(IEncodedFingerprintSchema fingerprint, int n)
        {
            return ComputeMinHashSignatureFast(fingerprint, n);
        }

        /// <summary>
        /// Compute Min Hash signature of a fingerprint (original implementation as fallback)
        /// </summary>
        /// <param name="fingerprint">
        /// Fingerprint signature
        /// </param>
        /// <param name="n">
        /// The number of hash functions to use
        /// </param>
        /// <returns>
        /// N-sized sub-fingerprint (length of the permutations number)
        /// </returns>
        /// <remarks>
        /// The basic idea in the Min Hashing scheme is to randomly permute the rows and for each 
        /// column c(i) compute its hash value h(c(i)) as the index of the first row under the permutation that has a 1 in that column.
        /// I.e. http://infolab.stanford.edu/~ullman/mmds/book.pdf s.3.3.4
        /// Optimized Min Hash computation using inverted index.
        /// Instead of iterating through each permutation and checking bits sequentially, we iterate through set bits once and update affected permutations.
        /// </remarks>
        private byte[] ComputeMinHashSignatureFast(IEncodedFingerprintSchema fingerprint, int n)
        {
            byte[] minHash = new byte[n];
            
            // initialize all to 255 (not found)
            for (int i = 0; i < n; i++)
            {
                minHash[i] = 255;
            }
            
            // iterate through all bit positions and check if set
            // for each set bit, update the minimum rank for all permutations that reference it
            int limit = Math.Min(maxBitIndex, 8192); // fingerprint is 8192 bits (128 * 32 * 2)
            for (int bitPos = 0; bitPos < limit; bitPos++)
            {
                if (fingerprint[bitPos])
                {
                    var entries = invertedIndex[bitPos];
                    for (int j = 0; j < entries.Length; j++)
                    {
                        var entry = entries[j];
                        if (entry.PermutationIndex < n && entry.Rank < minHash[entry.PermutationIndex])
                        {
                            minHash[entry.PermutationIndex] = entry.Rank;
                        }
                    }
                }
            }
            
            return minHash;
        }
    }
}
