namespace SoundFingerprinting.MinHash
{
    internal class CachedPermutations : IPermutations
    {
        private readonly IPermutations permutations;

        private int[][] cachedPermutations;

        public CachedPermutations(IPermutations permutations)
        {
            this.permutations = permutations;
        }

        public int[][] GetPermutations()
        {
            if (cachedPermutations != null)
            {
                return cachedPermutations;
            }

            return cachedPermutations = permutations.GetPermutations();
        }
    }
}