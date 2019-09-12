namespace SoundFingerprinting.MinHash
{
    /// <summary>
    ///   Permutations storage
    /// </summary>
    internal interface IPermutations
    {
        /// <summary>
        ///   Get Min Hash random permutations
        /// </summary>
        /// <returns>Permutation indexes</returns>
        int[][] GetPermutations();

        /// <summary>
        ///  Gets number of permutations
        /// </summary>
        int Count { get; }

        /// <summary>
        ///  Gets indexes per permutation
        /// </summary>
        int IndexesPerPermutation { get; }
    }
}