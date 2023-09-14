namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using SoundFingerprinting.Command;

    internal class RealtimeQueryResult
    {
        public RealtimeQueryResult(
            IEnumerable<AVResultEntry> ongoingEntries,
            IEnumerable<AVQueryResult> successEntries, 
            IEnumerable<AVQueryResult> didNotPassThresholdEntries)
        {
            OngoingEntries = ongoingEntries;
            SuccessEntries = successEntries;
            DidNotPassThresholdEntries = didNotPassThresholdEntries;
        }

        /// <summary>
        /// Gets list of ongoing matches.
        /// </summary>
        public IEnumerable<AVResultEntry> OngoingEntries { get; }

        /// <summary>
        ///  Gets list of aggregated successful matches.
        /// </summary>
        public IEnumerable<AVQueryResult> SuccessEntries { get; }
        
        /// <summary>
        ///  Gets list of matches that did not pass the matches filter.
        /// </summary>
        /// <remarks>
        ///  See implementations of <see cref="IRealtimeResultEntryFilter"/> interface.
        /// </remarks>
        public IEnumerable<AVQueryResult> DidNotPassThresholdEntries { get; }
        
        public void Deconstruct(out IEnumerable<AVResultEntry> ongoingEntries, out IEnumerable<AVQueryResult> successEntries, out IEnumerable<AVQueryResult> didNotPassThresholdEntries)
        {
            ongoingEntries = OngoingEntries;
            successEntries = SuccessEntries;
            didNotPassThresholdEntries = DidNotPassThresholdEntries;
        }
    }
}