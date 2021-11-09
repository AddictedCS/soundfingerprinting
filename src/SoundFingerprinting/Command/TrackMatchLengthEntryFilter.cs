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
    public class TrackMatchLengthEntryFilter : IRealtimeResultEntryFilter
    {
        private readonly double secondsThreshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackMatchLengthEntryFilter"/> class.
        /// </summary>
        /// <param name="secondsThreshold">Minimal threshold measured in seconds that will be used to filter incoming query results.</param>
        public TrackMatchLengthEntryFilter(double secondsThreshold)
        {
            if (secondsThreshold < 0)
            {
                throw new ArgumentException("Threshold has to be a positive integer", nameof(secondsThreshold));
            }
            
            this.secondsThreshold = secondsThreshold;
        }

        /// <inheritdoc cref="IRealtimeResultEntryFilter.Pass"/>
        public bool Pass(AVResultEntry entry, bool canContinueInTheNextQuery)
        {
            return entry.Audio?.TrackCoverageWithPermittedGapsLength > secondsThreshold || entry.Video?.TrackCoverageWithPermittedGapsLength > secondsThreshold;
        }
    }
}