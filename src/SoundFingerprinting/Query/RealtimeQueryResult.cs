namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using SoundFingerprinting.Command;

    public class RealtimeQueryResult<T>
    {
        public RealtimeQueryResult(IReadOnlyCollection<T> successEntries, IReadOnlyCollection<T> didNotPassThresholdEntries)
        {
            SuccessEntries = successEntries;
            DidNotPassThresholdEntries = didNotPassThresholdEntries;
        }
        
        /// <summary>
        ///  Gets list of aggregated successful matches.
        /// </summary>
        public IReadOnlyCollection<T> SuccessEntries { get; }
        
        /// <summary>
        ///  Gets list of matches that did not pass the matches filter.
        /// </summary>
        /// <remarks>
        ///  See implementations of <see cref="IRealtimeResultEntryFilter{T}"/> interface.
        /// </remarks>
        public IReadOnlyCollection<T> DidNotPassThresholdEntries { get; }
    }
}