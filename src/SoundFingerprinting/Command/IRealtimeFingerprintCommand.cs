namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using System.Threading;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Contract for realtime fingerprint command.
    /// </summary>
    public interface IRealtimeFingerprintCommand
    {
        /// <summary>
        ///  Creates hashes from the realtime source.
        /// </summary>
        /// <param name="cancellationToken">Token to stop reading from the source.</param>
        /// <returns>Async enumerable of <see cref="AVHashes"/>, one for each chunk of the source that is long enough to fingerprint.</returns>
        /// <remarks>
        ///  The returned enumerable owns one samples aggregator for its lifetime. Every enumeration re-enumerates the source and continues with
        ///  the buffered tail, so a consumer can resume after a transient error without losing content.
        /// </remarks>
        IAsyncEnumerable<AVHashes> Hash(CancellationToken cancellationToken);
    }
}
