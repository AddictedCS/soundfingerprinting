namespace Soundfingerprinting.Audio.Strides
{
    /// <summary>
    ///   Incremental stride
    /// </summary>
    public class IncrementalStaticStride : IStride
    {
        private readonly int incrementBy;

        private readonly int firstStride;

        public IncrementalStaticStride(int incrementBy, int samplesInFingerprint)
        {
            this.incrementBy = -samplesInFingerprint + incrementBy; /*Negative stride will guarantee that the signal is incremented by the parameter specified*/
            firstStride = 0;
        }

        public IncrementalStaticStride(int incrementBy, int samplesInFingerprint, int firstStride) : this(incrementBy, samplesInFingerprint)
        {
            this.firstStride = firstStride;
        }

        #region IStride Members
        
        public int StrideSize
        {
            get
            {
                return incrementBy;
            }
        }

        public int FirstStrideSize
        {
            get
            {
                return firstStride;
            }
        }

        #endregion
    }
}