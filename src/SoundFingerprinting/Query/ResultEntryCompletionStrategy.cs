namespace SoundFingerprinting.Query
{
    /// <summary>
    ///  A strategy that decides whether a result entry can continue in the next streaming query <see cref="StatefulRealtimeResultEntryAggregator"/>.
    /// </summary>
    internal sealed class ResultEntryCompletionStrategy : ICompletionStrategy<ResultEntry>
    {
        private readonly double permittedGap;
        private readonly double maxStalenessSeconds;

        /// <param name="permittedGap">Permitted gap between consecutive matches.</param>
        /// <param name="maxStalenessSeconds">
        ///  Longest non-matching stretch (in seconds) an entry may bridge while waiting for the track to resume matching.
        ///  Defaults to <see cref="double.MaxValue"/>, preserving the historical behavior of waiting for the remaining
        ///  track length (see <see cref="Configuration.RealtimeQueryConfiguration.OngoingEntryStalenessCeiling"/>).
        /// </param>
        public ResultEntryCompletionStrategy(double permittedGap, double maxStalenessSeconds = double.MaxValue)
        {
            this.permittedGap = permittedGap;
            this.maxStalenessSeconds = maxStalenessSeconds;
        }

        /// <inheritdoc cref="ICompletionStrategy{T}.CanContinueInNextQuery" />
        public bool CanContinueInNextQuery(ResultEntry? entry)
        {
            if (entry == null)
            {
                return false;
            }

            double totalPossibleCoverageResultingFromQuery = entry.QueryLength - entry.QueryMatchStartsAt;
            double totalPossibleTrackCoverage = entry.Track.Length - entry.TrackMatchStartsAt;
            if (totalPossibleCoverageResultingFromQuery >= totalPossibleTrackCoverage + permittedGap)
            {
                // the query stream advanced past the point where the track could still be matching
                return false;
            }

            // for tracks shorter than the staleness ceiling the bound above is always tighter, so their behavior
            // is unchanged; only entries on longer tracks complete early once they stop extending (no-op with the
            // double.MaxValue default)
            double stalenessInSeconds = entry.QueryLength - entry.Coverage.QueryMatchEndsAt;
            return stalenessInSeconds <= maxStalenessSeconds + permittedGap;
        }
    }
}