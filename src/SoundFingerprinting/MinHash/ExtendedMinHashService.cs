namespace SoundFingerprinting.MinHash
{
    using System;
    using SoundFingerprinting.Utils;

    /// <summary>
    /// Represents an entry in the inverted index mapping a bit position
    /// to a permutation and its rank within that permutation.
    /// Uses int for rank to support permutations with more than 255 elements.
    /// </summary>
    internal readonly struct ExtendedPermutationEntry(ushort permutationIndex, int rank)
    {
        /// <summary>
        /// Gets index of the permutation (0 to n-1).
        /// </summary>
        public ushort PermutationIndex { get; } = permutationIndex;

        /// <summary>
        /// Gets rank of the bit within this permutation.
        /// Lower rank means higher priority in min-hash computation.
        /// </summary>
        public int Rank { get; } = rank;
    }

    internal class ExtendedMinHashService : IMinHashService<int>
    {
        // inverted index: for each bit position, stores permutation entries
        // this allows single-pass scanning through set bits instead of nested loops
        private readonly ExtendedPermutationEntry[][] invertedIndex;
        private readonly int maxBitIndex;
        private readonly int defaultRank;
        private readonly int permutationCount;

        public ExtendedMinHashService(IPermutations permutations)
        {
            permutationCount = permutations.Count;
            
            // build inverted index for fast min-hash computation
            int[][] perms = permutations.GetPermutations();

            // store default rank (length of permutation = "not found")
            defaultRank = perms.Length > 0 ? perms[0].Length : 0;

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
            invertedIndex = new ExtendedPermutationEntry[maxBitIndex][];
            for (int i = 0; i < maxBitIndex; i++)
            {
                invertedIndex[i] = counts[i] > 0 ? new ExtendedPermutationEntry[counts[i]] : [];
            }

            // fill inverted index: for each bit position, store which permutations reference it and at what rank
            int[] currentIndex = new int[maxBitIndex];
            for (int permIdx = 0; permIdx < perms.Length; permIdx++)
            {
                for (int rank = 0; rank < perms[permIdx].Length; rank++)
                {
                    int bitPos = perms[permIdx][rank];
                    int idx = currentIndex[bitPos]++;
                    invertedIndex[bitPos][idx] = new ExtendedPermutationEntry((ushort)permIdx, rank);
                }
            }
        }

        public int[] Hash(IEncodedFingerprintSchema schema, int n)
        {
            if (n > permutationCount)
            {
                throw new ArgumentException("n should not exceed number of available hash functions: " + permutationCount);
            }

            int[] minHash = new int[n];

            // initialize all to default rank (not found)
            for (int i = 0; i < n; i++)
            {
                minHash[i] = defaultRank;
            }

            // iterate through all bit positions and check if set
            // for each set bit, update the minimum rank for all permutations that reference it
            for (int bitPos = 0; bitPos < maxBitIndex; bitPos++)
            {
                if (schema[bitPos])
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
