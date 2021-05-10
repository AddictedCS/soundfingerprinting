namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using SoundFingerprinting.Command;

    public class RealtimeQueryResult
    {
        public RealtimeQueryResult(IEnumerable<ResultEntry> successEntries, IEnumerable<ResultEntry> didNotPassThresholdEntries)
        {
            SuccessEntries = successEntries;
            DidNotPassThresholdEntries = didNotPassThresholdEntries;
        }
        
        /// <summary>
        ///  Gets list of aggregated successful matches.
        /// </summary>
        public IEnumerable<ResultEntry> SuccessEntries { get; }
        
        /// <summary>
        ///  Gets list of matches that did not pass the matches filter.
        /// </summary>
        /// <remarks>
        ///  See implementations of <see cref="IRealtimeResultEntryFilter"/> interface.
        /// </remarks>
        public IEnumerable<ResultEntry> DidNotPassThresholdEntries { get; }
    }
}