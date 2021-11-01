namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    public class OngoingRealtimeResultEntryFilter : IRealtimeResultEntryFilter<ResultEntry>
    {
        private readonly double minCoverage;
        private readonly double minTrackLength;

        public OngoingRealtimeResultEntryFilter(double minCoverage, double minTrackLength)
        {
            this.minCoverage = minCoverage;
            this.minTrackLength = minTrackLength;
        }
        
        public bool Pass(ResultEntry entry, bool canContinueInTheNextQuery)
        {
            if (!canContinueInTheNextQuery)
            {
                return false;
            }

            if (entry.TrackCoverageWithPermittedGapsLength >= minTrackLength)
            {
                return true;
            }

            if (entry.TrackRelativeCoverage >= minCoverage)
            {
                return true;
            }

            return false;
        }
    }
}