namespace Soundfingerprinting.Strides
{
    using System;

    /// <summary>
    ///   StaticStride class
    /// </summary>
    [Serializable]
    public class StaticStride : IStride
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticStride"/> class. 
        /// </summary>
        /// <param name="strideSize">
        /// Stride size, used each time StrideSize method is invoked
        /// </param>
        public StaticStride(int strideSize)
        {
            this.StrideSize = strideSize;
            FirstStrideSize = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticStride"/> class. 
        /// </summary>
        /// <param name="strideSize">
        /// Stride size
        /// </param>
        /// <param name="firstStride">
        /// First stride
        /// </param>
        public StaticStride(int strideSize, int firstStride) : this(strideSize)
        {
            FirstStrideSize = firstStride;
        }

        #region IStride Members

        /// <summary>
        ///   Gets stride size in terms of bit samples, which need to be skipped
        /// </summary>
        /// <returns>Bit samples to skip, between 2 consecutive overlapping fingerprints</returns>
        public int StrideSize { get; private set; }

        /// <summary>
        ///   Gets very first stride
        /// </summary>
        public int FirstStrideSize { get; private set; }

        #endregion
    }
}