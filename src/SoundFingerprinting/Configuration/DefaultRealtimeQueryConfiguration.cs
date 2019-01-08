namespace SoundFingerprinting.Configuration
{
    using System;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    public class DefaultRealtimeQueryConfiguration : RealtimeQueryConfiguration
    {
        public DefaultRealtimeQueryConfiguration(Action<ResultEntry> successCallback, Action<ResultEntry> didNotPassFilterCallback) :
            base(4, new QueryMatchLengthFilter(5d), successCallback, didNotPassFilterCallback, new IncrementalRandomStride(256, 512), 2d)
        {
        }
    }
}