namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using SoundFingerprinting.Command;

    internal class RealtimeQueryResult
    {
        public RealtimeQueryResult(IReadOnlyCollection<ResultEntry> successEntries, IReadOnlyCollection<ResultEntry> didNotPassThresholdEntries)
        {
            SuccessEntries = successEntries;
            DidNotPassThresholdEntries = didNotPassThresholdEntries;
        }
        
        /// <summary>
        ///  Gets list of aggregated successful matches.
        /// </summary>
        public IReadOnlyCollection<ResultEntry> SuccessEntries { get; }
        
        /// <summary>
        ///  Gets list of matches that did not pass the matches filter.
        /// </summary>
        /// <remarks>
        ///  See implementations of <see cref="IRealtimeResultEntryFilter"/> interface.
        /// </remarks>
        public IReadOnlyCollection<ResultEntry> DidNotPassThresholdEntries { get; }
    }
}