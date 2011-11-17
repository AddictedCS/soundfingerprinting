// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
namespace Soundfingerprinting.AudioProxies
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