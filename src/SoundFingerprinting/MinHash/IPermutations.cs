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
    }
}