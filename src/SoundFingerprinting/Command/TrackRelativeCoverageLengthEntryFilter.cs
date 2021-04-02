namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Query;

    public class TrackRelativeCoverageLengthEntryFilter : IRealtimeResultEntryFilter
    {
        private readonly double coverage;
        private readonly bool waitTillCompletion;

        public TrackRelativeCoverageLengthEntryFilter(double coverage, bool waitTillCompletion = false)
        {
            if (coverage < 0 || coverage > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(coverage), "Coverage should be defined within interval [0,1]");
            }
            
            this.coverage = coverage;
            this.waitTillCompletion = waitTillCompletion;
        }
        
        public bool Pass(ResultEntry entry, bool canContinueInTheNextQuery)
        {
            return !waitTillCompletion ? entry.TrackRelativeCoverage > coverage : entry.TrackRelativeCoverage > coverage && !canContinueInTheNextQuery;
        }
    }
}