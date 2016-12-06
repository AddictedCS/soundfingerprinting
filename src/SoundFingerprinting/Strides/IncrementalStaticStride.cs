namespace SoundFingerprinting.Strides
{
    using System;

    /// <summary>
    ///  Incremental static stride used in providing an exact step length (measured in number of audio samples) between 2 consecutive fingerprints
    /// </summary>
    public class IncrementalStaticStride : StaticStride
    {
        private const int SamplesPerFingerprint = 128 * 64;
        private readonly int incrementBy;

        /// <summary>
        ///   Initializes a new instance of the <see cref="IncrementalStaticStride"/> class. 
        /// </summary>
        /// <param name="incrementBy">
        ///    Number of audio samples to use between 2 consecutive fingerprints
        /// </param>
        public IncrementalStaticStride(int incrementBy) : base(-SamplesPerFingerprint + incrementBy) 
        {
            this.incrementBy = incrementBy;
        }

        internal IncrementalStaticStride(int incrementBy, int firstStride)
            : base(-SamplesPerFingerprint + incrementBy, firstStride)
        {
        }

        public override string ToString()
        {
            return string.Format("IncrementalStaticStride{0}", incrementBy);
        }
    }
}