namespace SoundFingerprinting
{
    using System.Collections.Generic;
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
        void RegisterMatches(IEnumerable<AVQueryMatch> avQueryMatches, bool? publishToDownstreamSubscribers = true);
    }
}