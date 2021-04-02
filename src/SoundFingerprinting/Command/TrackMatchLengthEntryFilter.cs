namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    public class TrackMatchLengthEntryFilter : IRealtimeResultEntryFilter
    {
        private readonly double secondsThreshold;

        public TrackMatchLengthEntryFilter(double secondsThreshold)
        {
            this.secondsThreshold = secondsThreshold;
        }

        public bool Pass(ResultEntry entry, bool canContinueInTheNextQuery)
        {
            return entry.TrackCoverageWithPermittedGapsLength > secondsThreshold;
        }
    }
}