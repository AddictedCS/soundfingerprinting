namespace SoundFingerprinting.Configuration
{
    using System;

    public interface IDelayStrategy
    {
        TimeSpan Delay { get; }
    }
}