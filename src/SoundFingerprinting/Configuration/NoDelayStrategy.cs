namespace SoundFingerprinting.Configuration
{
    using System;

    public class NoDelayStrategy : IDelayStrategy
    {
        public TimeSpan Delay => TimeSpan.Zero;
    }
}