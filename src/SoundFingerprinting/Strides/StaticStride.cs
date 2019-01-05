namespace SoundFingerprinting.Strides
{
    /// <inheritdoc />
    /// <summary>
    ///   Static stride class
    /// </summary>
    public class StaticStride : IStride
    {
        private const int DefaultSamplesPerFingerprint = 128 * 64;

        public StaticStride(int strideSize, int firstStride = 0)
        {
            NextStride =  DefaultSamplesPerFingerprint + strideSize;
            FirstStride = firstStride;
        }

        public int FirstStride { get; }

        public int NextStride { get; }

        public override string ToString()
        {
            return $"StaticStride Stride={NextStride}, FirstStride={FirstStride}";
        }
    }
}