namespace SoundFingerprinting.Strides
{
    /// <inheritdoc />
    /// <summary>
    ///   Static stride class
    /// </summary>
    public class StaticStride : IStride
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="StaticStride"/> class. 
        /// </summary>
        /// <param name="strideSize">
        ///    Stride size, used each time GetNextStride method is invoked
        /// </param>
        public StaticStride(int strideSize)
        {
            NextStride = strideSize;
            FirstStride = 0;
        }

        internal StaticStride(int strideSize, int firstStride) : this(strideSize)
        {
            FirstStride = firstStride;
        }

        public int FirstStride { get; }

        public int NextStride { get; }

        public override string ToString()
        {
            return $"StaticStride{NextStride}";
        }
    }
}