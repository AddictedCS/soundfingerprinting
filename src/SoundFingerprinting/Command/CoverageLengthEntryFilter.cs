namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    public class CoverageLengthEntryFilter : IRealtimeResultEntryFilter
    {
        private readonly double coverage;

        public CoverageLengthEntryFilter(double coverage)
        {
            this.coverage = coverage;
        }
        
        public bool Pass(ResultEntry entry)
        {
            return entry.RelativeCoverage > coverage;
        }
    }
}