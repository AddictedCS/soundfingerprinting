namespace SoundFingerprinting.Strides
{
    /// <summary>
    ///   Stride interface
    /// </summary>
    public interface IStride
    {
        /// <summary>
        ///   Gets stride size in terms of number of samples, which need to be skipped
        /// </summary>
        /// <returns>Number samples to skip, between 2 consecutive overlapping fingerprints</returns>
        int StrideSize { get; }

        /// <summary>
        ///   Gets size of the first stride
        /// </summary>
        /// <returns>Called at the very beginning just once</returns>
        int FirstStrideSize { get; }
    }
}