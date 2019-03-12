namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    public class DefaultRealtimeQueryConfiguration : RealtimeQueryConfiguration
    {
        public DefaultRealtimeQueryConfiguration(Action<ResultEntry> successCallback,
            Action<ResultEntry> didNotPassFilterCallback, Action<List<HashedFingerprint>> queryFingerprintsCallback,
            Action<Exception> onError, Action<double> restoredAfterErrorCallback) :
            base(4, new QueryMatchLengthFilter(5d), successCallback, didNotPassFilterCallback,
                queryFingerprintsCallback, onError, restoredAfterErrorCallback, new IncrementalRandomStride(256, 512), 2d, new List<string>())
        {
        }
    }
}