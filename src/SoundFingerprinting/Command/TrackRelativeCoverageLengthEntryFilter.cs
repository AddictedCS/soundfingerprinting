namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Track relative coverage result entry filter.
    /// </summary>
    /// <remarks>
    ///  Filters all entries that have a shorter <see cref="ResultEntry.TrackRelativeCoverage"/> than the configured threshold.
    /// </remarks>
    public class TrackRelativeCoverageLengthEntryFilter : IRealtimeResultEntryFilter
    {
        private readonly double coverage;
        private readonly bool waitTillCompletion;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackRelativeCoverageLengthEntryFilter"/> class.
        /// </summary>
        /// <param name="coverage">Coverage used as a minimum threshold for the <see cref="ResultEntry.TrackRelativeCoverage"/>.</param>
        /// <param name="waitTillCompletion">A flag indicating whether to emit the result without waiting until the track finishes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Coverage has to be defined within [0, 1] interval.</exception>
        public TrackRelativeCoverageLengthEntryFilter(double coverage, bool waitTillCompletion = false)
        {
            if (coverage < 0 || coverage > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(coverage), "Coverage should be defined within interval [0,1]");
            }

            this.coverage = coverage;
            this.waitTillCompletion = waitTillCompletion;
        }

        /// <inheritdoc cref="IRealtimeResultEntryFilter.Pass"/>
        public bool Pass(AVResultEntry entry, bool canContinueInTheNextQuery)
        {
            return !waitTillCompletion ? (entry.Audio?.TrackRelativeCoverage > coverage || entry.Video?.TrackRelativeCoverage > coverage) : (entry.Audio?.TrackRelativeCoverage > coverage || entry.Video?.TrackRelativeCoverage > coverage) && !canContinueInTheNextQuery;
        }
    }
}