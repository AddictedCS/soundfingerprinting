namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    public class QueryMatchLengthFilter : IRealtimeResultEntryFilter
    {
        private readonly double secondsThreshold;

        public QueryMatchLengthFilter(double secondsThreshold)
        {
            this.secondsThreshold = secondsThreshold;
        }

        public bool Pass(ResultEntry entry)
        {
            return entry.CoverageWithPermittedGapsLength > secondsThreshold;
        }
    }
}