namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Track relative coverage result entry filter.
    /// </summary>
    /// <remarks>
    ///  Filters all entries that have a shorter <see cref="Coverage.TrackRelativeCoverage"/> than the configured threshold.
    /// </remarks>
    public class TrackRelativeCoverageEntryFilter : IRealtimeResultEntryFilter
    {
        private readonly double coverage;
        private readonly bool waitTillCompletion;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackRelativeCoverageEntryFilter"/> class.
        /// </summary>
        /// <param name="coverage">Coverage used as a minimum threshold for the <see cref="Coverage.TrackRelativeCoverage"/>.</param>
        /// <param name="waitTillCompletion">A flag indicating whether to emit the result without waiting until the track finishes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Coverage has to be defined within [0, 1] interval.</exception>
        public TrackRelativeCoverageEntryFilter(double coverage, bool waitTillCompletion)
        {
            if (coverage is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(coverage), "Coverage should be defined within interval [0,1]");
            }

            this.coverage = coverage;
            this.waitTillCompletion = waitTillCompletion;
        }

        /// <inheritdoc cref="IRealtimeResultEntryFilter.Pass"/>
        public bool Pass(AVResultEntry entry, bool canContinueInTheNextQuery)
        {
            if (canContinueInTheNextQuery && waitTillCompletion)
            {
                return false;
            }

            return entry.Audio?.Coverage.TrackRelativeCoverage > coverage || entry.Video?.Coverage.TrackRelativeCoverage > coverage;
        }
    }
}