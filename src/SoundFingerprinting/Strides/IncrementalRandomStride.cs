namespace SoundFingerprinting.Strides
{
    /// <summary>
    ///   Incremental random stride used in providing step length (measured in number of audio samples) between 2 consecutive fingerprints
    /// </summary>
    public class IncrementalRandomStride : RandomStride
    {
        private const int SamplesPerFingerprint = 128 * 64; // 8192 samples per one fingerprint
 
        /// <summary>
        ///   Initializes a new instance of the <see cref="IncrementalRandomStride"/> class. 
        /// </summary>
        /// <example>
        ///   new IncrementalRandomStride(256, 512)
        /// </example>
        /// <param name="min">
        ///   Inclusive min value used for generating a random stride
        /// </param>
        /// <param name="max">
        ///   Exclusive max value used for generating a random stride
        /// </param>
        public IncrementalRandomStride(int min, int max) : this(min, max, 0)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="IncrementalRandomStride"/> class. 
        /// </summary>
        /// <example>
        ///   new IncrementalRandomStride(256, 512)
        /// </example>
        /// <param name="min">
        ///   Inclusive min value used for generating a random stride
        /// </param>
        /// <param name="max">
        ///   Exclusive max value used for generating a random stride
        /// </param>
        /// <param name="seed">
        ///   Seed used when generating next random stride. Leave unset if you want to use environment ticks as the seed.
        /// </param>
        public IncrementalRandomStride(int min, int max, int seed) : base(min, max, seed)
        {
        }

        internal IncrementalRandomStride(int minStride, int maxStride, int firstStride, int seed): base(minStride, maxStride, firstStride, seed)
        {
        }

        public override int NextStride
        {
            get
            {
                return -SamplesPerFingerprint + Random.Next(Min, Max);
            }
        }

        public override string ToString()
        {
            return $"IncrementalRandomStride[{Min}-{Max})";
        }
    }
}