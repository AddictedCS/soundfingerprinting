namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    public class DefaultRealtimeQueryConfiguration : RealtimeQueryConfiguration
    {
        public DefaultRealtimeQueryConfiguration(Action<ResultEntry> successCallback,
            Action<ResultEntry> didNotPassFilterCallback, Action<List<HashedFingerprint>> queryFingerprintsCallback,
            Action<Exception, TimedHashes> onError, Action restoredAfterErrorCallback) :
            base(4, new QueryMatchLengthFilter(5d), successCallback, didNotPassFilterCallback,
                queryFingerprintsCallback, onError, restoredAfterErrorCallback, Enumerable.Empty<TimedHashes>(),
                new IncrementalRandomStride(256, 512), 2d, 0d, new List<string>())
        {
        }
    }
}