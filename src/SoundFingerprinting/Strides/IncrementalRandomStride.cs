namespace SoundFingerprinting.Strides
{
    /// <summary>
    /// Incremental random stride used in providing step length (measured in number of audio samples) between 2 consecutive fingerprints
    /// </summary>
    public class IncrementalRandomStride : RandomStride
    {
        private readonly int samplesPerFingerprint;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalRandomStride"/> class. 
        /// </summary>
        /// <example>
        /// new IncrementalRandomStride(256, 521)
        /// </example>
        /// <param name="min">
        /// Inclusive minimal value used for generating a random stride
        /// </param>
        /// <param name="max">
        /// Exclusive maximum value used for generating a random stride
        /// </param>
        public IncrementalRandomStride(int min, int max) : this(min, max, 128 * 64)
        {
            // default number of samples per fingerprint is 8192
        }

        internal IncrementalRandomStride(int minStride, int maxStride, int samplesPerFingerprint)
            : base(minStride, maxStride)
        {
            this.samplesPerFingerprint = samplesPerFingerprint;
            FirstStride = Random.Next(minStride, maxStride);
        }

        internal IncrementalRandomStride(int minStride, int maxStride, int samplesPerFingerprint, int firstStride)
            : this(minStride, maxStride, samplesPerFingerprint)
        {
            FirstStride = firstStride;
        }

        public override int GetNextStride()
        {
            return -samplesPerFingerprint + Random.Next(Min, Max);
        }

        public override string ToString()
        {
            return string.Format("Incremental-Random-Stride-[{0}-{1})", Min, Max);
        }
    }
}