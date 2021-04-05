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
            Action<ResultEntry> didNotPassFilterCallback, Action<Hashes> queryFingerprintsCallback,
            Action<Exception, Hashes> onError, Action restoredAfterErrorCallback) :
            base(4, new TrackMatchLengthEntryFilter(5d), successCallback, didNotPassFilterCallback,
                queryFingerprintsCallback, onError, restoredAfterErrorCallback, Enumerable.Empty<Hashes>(),
                new IncrementalRandomStride(256, 512), 2d, 0d,
                (int) (10240d / 5512) * 1000, new Dictionary<string, string>())
        {
        }
    }
}