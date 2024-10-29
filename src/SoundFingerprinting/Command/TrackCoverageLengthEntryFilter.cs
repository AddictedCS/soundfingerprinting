namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Track match length entry filter class.
    /// </summary>
    /// <remarks>
    ///  Filters all entries that have a shorter <see cref="ResultEntry.TrackCoverageWithPermittedGapsLength"/> than the configured threshold.
    /// </remarks>
    public class TrackCoverageLengthEntryFilter : IRealtimeResultEntryFilter
    {
        private readonly double secondsThreshold;
        private readonly bool waitTillCompletion;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackCoverageLengthEntryFilter"/> class.
        /// </summary>
        /// <param name="secondsThreshold">Minimal threshold measured in seconds that will be used to filter incoming query results.</param>
        /// <param name="waitTillCompletion">A flag indicating whether to wait till completion or not.</param>
        public TrackCoverageLengthEntryFilter(double secondsThreshold, bool waitTillCompletion)
        {
            if (secondsThreshold < 0)
            {
                throw new ArgumentException("Threshold has to be a positive integer", nameof(secondsThreshold));
            }
            
            this.secondsThreshold = secondsThreshold;
            this.waitTillCompletion = waitTillCompletion;
        }

        /// <inheritdoc cref="IRealtimeResultEntryFilter.Pass"/>
        public bool Pass(AVResultEntry entry, bool canContinueInTheNextQuery)
        {
            if (canContinueInTheNextQuery && waitTillCompletion)
            {
                // if we can continue in the next query, but we are waiting for the track to finish, we should not emit the result
                return false;
            }
            
            return entry.Audio?.TrackCoverageWithPermittedGapsLength > secondsThreshold || entry.Video?.TrackCoverageWithPermittedGapsLength > secondsThreshold;
        }
    }
}