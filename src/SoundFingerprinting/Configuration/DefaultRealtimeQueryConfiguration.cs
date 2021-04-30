namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    /// <summary>
    ///  Default realtime query configuration used to configure query options for realtime queries.
    /// </summary>
    public class DefaultRealtimeQueryConfiguration : RealtimeQueryConfiguration
    {
        /// <summary>
        ///  Creates new instance of DefaultRealtimeQueryConfiguration
        /// </summary>
        /// <param name="successCallback">Success callback invoked when result entry filter is passed.</param>
        /// <param name="didNotPassFilterCallback">Callback for items that did not pass result entry filter.</param>
        /// <param name="onError">Error callback.</param>
        /// <param name="restoredAfterErrorCallback">When connection to storage is restored, this callback is invoked.</param>
        public DefaultRealtimeQueryConfiguration(Action<ResultEntry> successCallback,
            Action<ResultEntry> didNotPassFilterCallback,
            Action<Exception, Hashes> onError, Action restoredAfterErrorCallback) :
            base(thresholdVotes: 4,
                new TrackMatchLengthEntryFilter(5d),
                successCallback,
                didNotPassFilterCallback,
                onError,
                restoredAfterErrorCallback,
                Enumerable.Empty<Hashes>(),
                new IncrementalRandomStride(256, 512),
                permittedGap: 2d,
                downtimeCapturePeriod: 0d,
                new Dictionary<string, string>(),
                new Dictionary<string, string>())
        {
        }
    }
}