namespace SoundFingerprinting.Strides
{
    /// <summary>
    ///   Static stride class
    /// </summary>
    public class StaticStride : IStride
    {
        private readonly int nextStride;

        /// <summary>
        ///   Initializes a new instance of the <see cref="StaticStride"/> class. 
        /// </summary>
        /// <param name="strideSize">
        ///    Stride size, used each time GetNextStride method is invoked
        /// </param>
        public StaticStride(int strideSize)
        {
            nextStride = strideSize;
            FirstStride = 0;
        }

        internal StaticStride(int strideSize, int firstStride) : this(strideSize)
        {
            FirstStride = firstStride;
        }

        public int FirstStride { get; }

        public int NextStride
        {
            get
            {
                return nextStride;
            }
        }

        public override string ToString()
        {
            return $"StaticStride{nextStride}";
        }
    }
}