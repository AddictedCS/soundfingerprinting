namespace SoundFingerprinting.Query
{
    /// <summary>
    ///  A strategy that decides whether a result entry can continue in the next streaming query <see cref="StatefulRealtimeResultEntryAggregator"/>.
    /// </summary>
    internal sealed class ResultEntryCompletionStrategy : ICompletionStrategy<ResultEntry>
    {
        /// <inheritdoc cref="ICompletionStrategy{T}.CanContinueInNextQuery" />
        public bool CanContinueInNextQuery(ResultEntry? entry)
        {
            if (entry == null)
            {
                return false;
            }
            
            double totalPossibleCoverageResultingFromQuery = entry.QueryLength - entry.QueryMatchStartsAt;
            double totalPossibleTrackCoverage = entry.Track.Length - entry.TrackMatchStartsAt;
            return totalPossibleCoverageResultingFromQuery < totalPossibleTrackCoverage;
        }
    }
}