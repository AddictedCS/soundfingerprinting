namespace SoundFingerprinting.Configuration
{
    using System;

    public class RandomDelayStrategy : IDelayStrategy
    {
        private readonly TimeSpan minDelay;
        private readonly TimeSpan maxDelay;
        private readonly Random random;

        internal RandomDelayStrategy(TimeSpan minDelay, TimeSpan maxDelay, Random random)
        {
            this.minDelay = minDelay;
            this.maxDelay = maxDelay;
            this.random = random;
        }

        public RandomDelayStrategy(double minDelay, double maxDelay) : this(TimeSpan.FromSeconds(minDelay), TimeSpan.FromSeconds(maxDelay), new Random())
        {
            // no-op
        }

        public TimeSpan Delay
        {
            get
            {
                double minSeconds = minDelay.TotalSeconds;
                double maxSeconds = maxDelay.TotalSeconds;
                double randomSeconds = random.NextDouble() * (maxSeconds - minSeconds) + minSeconds;
                return TimeSpan.FromSeconds(randomSeconds);
            }
        } 
    }
}