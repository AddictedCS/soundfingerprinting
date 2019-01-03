namespace SoundFingerprinting.Strides
{
    /// <inheritdoc />
    /// <summary>
    ///  Incremental static stride used in providing an exact step length (measured in number of audio samples) between 2 consecutive fingerprints
    /// </summary>
    public class IncrementalStaticStride : IStride
    {
        public IncrementalStaticStride(int incrementBy, int firstStride = 0)
        {
            NextStride = incrementBy;
            FirstStride = firstStride;
        }
        
        public int FirstStride { get; }

        public int NextStride { get; }

        public override string ToString()
        {
            return $"IncrementalStaticStride, Stride={NextStride}, FirstStride={FirstStride}";
        }
    }
}