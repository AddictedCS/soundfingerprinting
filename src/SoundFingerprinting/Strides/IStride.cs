namespace SoundFingerprinting.Strides
{
    /// <summary>
    ///   Stride interface
    /// </summary>
    public interface IStride
    {
        /// <summary>
        ///   Gets the first stride
        /// </summary>
        /// <returns>Called at the very beginning just once (normally it is 0)</returns>
        int FirstStride { get; }

        /// <summary>
        ///   Gets stride in terms of number of samples, which need to be skipped
        /// </summary>
        /// <returns>Number samples to skip, between 2 consecutive overlapping fingerprints</returns>
        int GetNextStride();
    }
}