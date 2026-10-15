namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Register query matches with associated metadata.
    /// </summary>
    public interface IAVQueryMatchRegistry
    {
        /// <summary>
        ///  Register query matches with associated metadata.
        /// </summary>
        /// <param name="avQueryMatches">AV query matches to register.</param>
        /// <param name="publishToDownstreamSubscribers">A flag indicating whether we should publish the query matches to downstream components.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task that completes when the registry accepted the matches; whether that means persisted or published is defined by the implementation.</returns>
        Task RegisterMatchesAsync(IEnumerable<AVQueryMatch> avQueryMatches, bool? publishToDownstreamSubscribers = true, CancellationToken cancellationToken = default);
    }
}