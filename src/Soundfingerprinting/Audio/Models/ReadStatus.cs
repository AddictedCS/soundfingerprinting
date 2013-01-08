namespace Soundfingerprinting.Audio.Models
{
    /// <summary>
    ///   Status of read operation
    /// </summary>
    public enum ReadStatus
    {
        /// <summary>
        ///   The Stream is still being read
        /// </summary>
        Running,

        /// <summary>
        ///   End of stream reached
        /// </summary>
        EndOfStream,

        /// <summary>
        ///   Exception occured while reading the stream
        /// </summary>
        Exception,

        /// <summary>
        ///   The operation has been aborted by the clien call
        /// </summary>
        Aborted
    }
}