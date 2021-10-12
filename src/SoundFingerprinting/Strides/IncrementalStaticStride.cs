namespace SoundFingerprinting.Strides
{
    /// <summary>
    ///  Incremental static stride used in providing an exact step length (measured in number of audio samples) between 2 consecutive fingerprints.
    /// </summary>
    public class IncrementalStaticStride : IStride
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalStaticStride"/> class.
        /// </summary>
        /// <param name="incrementBy">Size is samples to be used between two consecutive fingerprints.</param>
        /// <param name="firstStride">Size in samples of the first stride.</param>
        public IncrementalStaticStride(int incrementBy, int firstStride = 0)
        {
            NextStride = incrementBy;
            FirstStride = firstStride;
        }

        /// <inheritdoc cref="IStride.FirstStride"/>
        public int FirstStride { get; }

        /// <inheritdoc cref="IStride.NextStride"/>
        public int NextStride { get; }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"IncrementalStaticStride, Stride={NextStride}, FirstStride={FirstStride}";
        }
    }
}