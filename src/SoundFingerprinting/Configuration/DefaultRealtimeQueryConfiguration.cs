namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Threading.Tasks;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Default realtime query configuration used to configure query options for realtime queries.
    /// </summary>
    public class DefaultRealtimeQueryConfiguration : RealtimeQueryConfiguration
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="DefaultRealtimeQueryConfiguration"/> class.
        /// </summary>
        /// <param name="successCallback">Success callback invoked when result entry filter is passed.</param>
        /// <param name="didNotPassFilterCallback">Callback for items that did not pass result entry filter.</param>
        /// <param name="onError">Error callback.</param>
        /// <param name="restoredAfterErrorCallback">When connection to storage is restored, this callback is invoked.</param>
        public DefaultRealtimeQueryConfiguration(
            Func<AVQueryResult, Task> successCallback,
            Func<AVQueryResult, Task> didNotPassFilterCallback,
            Action<Exception, AVHashes?> onError,
            Action restoredAfterErrorCallback) : base(
            new DefaultAVQueryConfiguration(),
            new TrackCoverageLengthEntryFilter(secondsThreshold: 5d, waitTillCompletion: false),
            successCallback,
            didNotPassFilterCallback,
            new PassThroughRealtimeResultEntryFilter(),
            ongoingCallback: (_, _) => Task.CompletedTask,
            errorCallback: onError,
            restoredAfterErrorCallback: restoredAfterErrorCallback,
            offlineStorage: new EmptyOfflineStorage(),
            errorBackoffPolicy: new RandomExponentialBackoffPolicy(),
            delayStrategy: new RandomDelayStrategy(1, 5),
            automaticSkipDetection: false, 
            includeQueryHashesInResponse: true)
        {
        }
    }
}