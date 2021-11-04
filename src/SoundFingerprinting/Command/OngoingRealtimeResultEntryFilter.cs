namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Ongoing realtime result entry filter.
    /// </summary>
    /// <remarks>
    ///  Filters ongoing matches according to min coverage (see <see cref="ResultEntry.TrackRelativeCoverage"/>) or min track coverage length ((see <see cref="ResultEntry.TrackCoverageWithPermittedGapsLength"/>).
    /// </remarks>
    public class OngoingRealtimeResultEntryFilter : IRealtimeResultEntryFilter
    {
        private readonly double minCoverage;
        private readonly double minTrackLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="OngoingRealtimeResultEntryFilter"/> class.
        /// </summary>
        /// <param name="minCoverage">Min coverage.</param>
        /// <param name="minTrackLength">Min track length.</param>
        public OngoingRealtimeResultEntryFilter(double minCoverage, double minTrackLength)
        {
            this.minCoverage = minCoverage;
            this.minTrackLength = minTrackLength;
        }

        public bool Pass(AVResultEntry entry, bool canContinueInTheNextQuery)
        {
            if (!canContinueInTheNextQuery)
            {
                return false;
            }

            if (entry.Audio?.TrackCoverageWithPermittedGapsLength >= minTrackLength || entry.Video?.TrackCoverageWithPermittedGapsLength >= minTrackLength)
            {
                return true;
            }

            if (entry.Audio?.TrackRelativeCoverage >= minCoverage || entry.Video?.TrackRelativeCoverage >= minCoverage)
            {
                return true;
            }

            return false;
        }
    }
}