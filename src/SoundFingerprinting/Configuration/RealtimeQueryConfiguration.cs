namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    /// <summary>
    ///   Configuration options used when querying the data source in realtime.
    /// </summary>
    public abstract class RealtimeQueryConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealtimeQueryConfiguration"/> class.
        /// </summary>
        /// <param name="queryConfiguration">Audio/Video query configuration.</param>
        /// <param name="resultEntryFilter">An instance of the <see cref="IRealtimeResultEntryFilter"/> filter to filter matched entries.</param>
        /// <param name="successCallback">Success callback.</param>
        /// <param name="didNotPassFilterCallback">Callback invoked when filter is not passed.</param>
        /// <param name="ongoingResultEntryFilter">What is playing right now filter.</param>
        /// <param name="ongoingSuccessCallback">What is playing right now success callback.</param>
        /// <param name="errorCallback">Error callback.</param>
        /// <param name="restoredAfterErrorCallback">Restore after error callback.</param>
        /// <param name="offlineStorage">An instance of the <see cref="IOfflineStorage"/> interface.</param>
        /// <param name="errorBackoffPolicy">Error backoff policy.</param>
        /// <param name="delayStrategy">Delay strategy.</param>
        /// <param name="automaticSkipDetection">A flag indicating whether to automatically detect skip in the query source.</param>
        public RealtimeQueryConfiguration(
            AVQueryConfiguration queryConfiguration,
            IRealtimeResultEntryFilter resultEntryFilter,
            Action<AVQueryResult> successCallback,
            Action<AVQueryResult> didNotPassFilterCallback,
            IRealtimeResultEntryFilter ongoingResultEntryFilter,
            Action<AVResultEntry> ongoingSuccessCallback,
            Action<Exception, AVHashes?> errorCallback,
            Action restoredAfterErrorCallback,
            IOfflineStorage offlineStorage,
            IBackoffPolicy errorBackoffPolicy,
            IDelayStrategy delayStrategy, 
            bool automaticSkipDetection)
        {
            QueryConfiguration = queryConfiguration;
            ResultEntryFilter = resultEntryFilter;
            SuccessCallback = successCallback;
            DidNotPassFilterCallback = didNotPassFilterCallback;
            OngoingResultEntryFilter = ongoingResultEntryFilter;
            OngoingSuccessCallback = ongoingSuccessCallback;
            ErrorCallback = errorCallback;
            RestoredAfterErrorCallback = restoredAfterErrorCallback;
            OfflineStorage = offlineStorage;
            ErrorBackoffPolicy = errorBackoffPolicy;
            DelayStrategy = delayStrategy;
            AutomaticSkipDetection = automaticSkipDetection;
        }

        /// <summary>
        ///  Gets or sets result entry filter.
        /// </summary>
        /// <remarks>
        ///  The following implementations are recommended for use: <br/>
        ///  <see cref="CompletedRealtimeMatchResultEntryFilter"/>  keeps the match from getting emitted until it can't continue in the next query.
        ///  Since realtime queries come in chunks that can partition a match into multiple parts (i.e., a 3-minute song will match 3 times if the length of the query is 1 minute), this filter prevents partitioning, emitting only 1 success entry at the end of the last match. <br/>
        ///  <see cref="TrackRelativeCoverageLengthEntryFilter"/> filters all entries those <see cref="ResultEntry.TrackRelativeCoverage"/> is shorter than the threshold. An example: <b>0.4</b> - all tracks that matched less than 40% of their length will be disregarded. Also allows specifying <i>waitTillCompletion</i> flag indicating whether to wait till completion before emitting the result (default <i>true</i>). <br/>
        ///  <see cref="TrackMatchLengthEntryFilter"/> filters all entries those <see cref="ResultEntry.TrackCoverageWithPermittedGapsLength" /> is shorter than the threshold.
        ///  <see cref="PassThroughRealtimeResultEntryFilter"/> the matches will be emitted immediately once occured. <br/>
        ///  <see cref="NoPassRealtimeResultEntryFilter"/> block all matches from getting emitted.
        /// </remarks>
        public IRealtimeResultEntryFilter ResultEntryFilter { get; set; }

        /// <summary>
        ///   Gets or sets success callback invoked when a candidate passes result entry filter.
        /// </summary>
        public Action<AVQueryResult> SuccessCallback { get; set; }

        /// <summary>
        ///  Gets or sets callback invoked when a candidate did not pass result entry filter, but has been considered a candidate.
        /// </summary>
        public Action<AVQueryResult> DidNotPassFilterCallback { get; set; }

        /// <summary>
        ///  Gets or sets ongoing result entry filter that will be invoked for every result entry filter that is captured by the aggregator.
        /// </summary>
        /// <remarks>
        ///  The following implementations are recommended for use: <br />
        ///  <see cref="OngoingRealtimeResultEntryFilter"/> will emit the result without waiting it to complete.
        ///  As an example for initialization values <i>minCoverage = 0.2</i> and <i>minTrackLength = 10</i>, a 1-minute long track will be emitted 6 times in the <see cref="OngoingSuccessCallback"/>.
        /// </remarks>
        public IRealtimeResultEntryFilter OngoingResultEntryFilter { get; set; }

        /// <summary>
        ///  Gets or sets ongoing success callback that will be invoked on entries that pass <see cref="OngoingResultEntryFilter"/>.
        /// </summary>
        public Action<AVResultEntry> OngoingSuccessCallback { get; set; }

        /// <summary>
        ///  Gets or sets error callback which will be invoked in case if an error occurs during query time.
        /// </summary>
        /// <remarks>
        ///  Instance of the <see cref="AVHashes"/> will be null in case if the error occured before hashes were generated, typically connection error to the realtime source. <br/>
        ///  If you need to stop querying immediately after an error occured, invoke token cancellation in the callback.
        /// </remarks>
        public Action<Exception, AVHashes?> ErrorCallback { get; set; }

        /// <summary>
        ///  Gets or sets on error backoff policy that will be invoked before retrying to read from the realtime source.
        /// </summary>
        /// <remarks>
        ///  Default retry policy on streaming error is default <see cref="RandomExponentialBackoffPolicy"/>.
        /// </remarks>
        public IBackoffPolicy ErrorBackoffPolicy { get; set; }

        /// <summary>
        ///  Gets or sets delay strategy used when querying from the offline storage.
        /// </summary>
        /// <remarks>
        ///  This will control the delay between consecutive calls with fingerprints read from the offline storage.
        ///  It is meaningful to set a delay between consecutive calls, not to overwhelm the server with a queue for query requests after it comes back online.
        /// </remarks>
        public IDelayStrategy DelayStrategy { get; set; }

        /// <summary>
        ///  Gets or sets error restore callback.
        /// </summary>
        public Action RestoredAfterErrorCallback { get; set; }

        /// <summary>
        ///  Gets or sets offline storage for hashes.
        /// </summary>
        /// <remarks>
        ///  Experimental, can be used to store hashes during the time the storage is not available. Will be consumed, after RestoredAfterErrorCallback invocation.
        /// </remarks>
        public IOfflineStorage OfflineStorage { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether to enable automatic skip detection (default false).
        /// </summary>
        /// <remarks>
        ///  Experimental automatic skip detection is used to identify fast-forwards or backwards skip in the realtime query source (i.e., skip through audio/video player). <br />
        ///  This helps in correctly identifying query/track gaps that result when the query source is matching across track regions larger than the initial query length. <br />
        ///  A by-product of skip detection is a synthetically increased query length in the resulting <see cref="ResultEntry"/> when skip is detected.
        /// </remarks>
        public bool AutomaticSkipDetection { get; set; }

        /// <summary>
        ///  Gets or sets list of positive meta fields to consider when querying the data source for potential candidates.
        /// </summary>
        public IDictionary<string, string> YesMetaFieldsFilter
        {
            get => QueryConfiguration.Audio.YesMetaFieldsFilters;
            set
            {
                QueryConfiguration.Audio.YesMetaFieldsFilters = value;
                QueryConfiguration.Video.YesMetaFieldsFilters = value;
            }
        }

        /// <summary>
        ///  Gets or sets list of negative meta fields to consider when querying the data source for potential candidates.
        /// </summary>
        public IDictionary<string, string> NoMetaFieldsFilter
        {
            get => QueryConfiguration.Audio.NoMetaFieldsFilters;
            set
            {
                QueryConfiguration.Audio.NoMetaFieldsFilters = value;
                QueryConfiguration.Video.NoMetaFieldsFilters = value;
            }
        }

        /// <summary>
        ///  Gets or sets query configuration.
        /// </summary>
        public AVQueryConfiguration QueryConfiguration { get; set; }
    }
}