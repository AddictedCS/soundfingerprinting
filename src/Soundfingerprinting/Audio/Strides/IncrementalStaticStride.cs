namespace Soundfingerprinting.Audio.Strides
{
    /// <summary>
    ///   Incremental stride
    /// </summary>
    public class IncrementalStaticStride : IStride
    {
        /// <summary>
        ///   Increment by parameter (usually negative)
        /// </summary>
        private readonly int incrementBy;

        private readonly int firstStride;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalStaticStride"/> class. 
        /// </summary>
        /// <param name="incrementBy">
        /// Increment by parameter in audio samples
        /// </param>
        /// <param name="samplesInFingerprint">
        /// Number of samples in one signature [normally 8192]
        /// </param>
        public IncrementalStaticStride(int incrementBy, int samplesInFingerprint)
        {
            this.incrementBy = -samplesInFingerprint + incrementBy; /*Negative stride will guarantee that the signal is incremented by the parameter specified*/
            firstStride = 0;
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