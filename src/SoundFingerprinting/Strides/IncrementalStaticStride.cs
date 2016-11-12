namespace SoundFingerprinting.Strides
{
    using System;

    /// <summary>
    /// Incremental static stride used in providing an exact step length (measured in number of audio samples) between 2 consecutive fingerprints
    /// </summary>
    public class IncrementalStaticStride : StaticStride
    {
        private readonly int incrementBy;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalStaticStride"/> class. 
        /// </summary>
        /// <param name="incrementBy">
        ///    Number of audio samples to use between 2 consecutive fingerprints
        /// </param>
        public IncrementalStaticStride(int incrementBy) : this(incrementBy, 128 * 64)
        {
            // default number of samples per fingerprint is 8192
        }

        // negative stride will guarantee that the signal is incremented by the parameter specified
        internal IncrementalStaticStride(int incrementBy, int samplesPerFingerprint)
            : base(-samplesPerFingerprint + incrementBy) 
        {
            if (incrementBy <= 0)
            {
                throw new ArgumentException("Bad parameter. IncrementBy should be strictly bigger than 0");
            }

            this.incrementBy = incrementBy;
        }

        internal IncrementalStaticStride(int incrementBy, int samplesPerFingerprint, int firstStride)
            : this(incrementBy, samplesPerFingerprint)
        {
            FirstStride = firstStride;
        }

        public override string ToString()
        {
            return string.Format("Incremental-Static-Stride-{0}", incrementBy);
        }
    }
}