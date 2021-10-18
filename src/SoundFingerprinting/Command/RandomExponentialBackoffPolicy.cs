namespace SoundFingerprinting.Command
{
    using System;

    /// <summary>
    ///  Random exponential backoff policy class.
    /// </summary>
    public class RandomExponentialBackoffPolicy : IBackoffPolicy
    {
        private readonly double @base;
        private readonly TimeSpan minDelay;
        private readonly TimeSpan maxDelay;
        private readonly TimeSpan delayCap;
        private readonly Random random;

        private long exponent;
        private DateTime timeToRetry = DateTime.MinValue;

        /// <summary>
        ///  Initializes a new instance of the <see cref="RandomExponentialBackoffPolicy"/> class.
        /// </summary>
        /// <param name="base">Exponent base.</param>
        /// <param name="minDelay">Random min delay.</param>
        /// <param name="maxDelay">Random max delay.</param>
        /// <param name="delayCap">Delay cap.</param>
        /// <param name="random">Instance of the <see cref="Random"/> class.</param>
        /// <exception cref="ArgumentException">Min delay should always be less than max delay.</exception>
        public RandomExponentialBackoffPolicy(double @base, TimeSpan minDelay, TimeSpan maxDelay, TimeSpan delayCap, Random random)
        {
            if (minDelay >= maxDelay)
            {
                throw new ArgumentException($"{nameof(minDelay)}={minDelay} must be < {nameof(maxDelay)}={maxDelay}");
            }
            
            this.@base = @base;
            this.minDelay = minDelay;
            this.maxDelay = maxDelay;
            this.delayCap = delayCap;
            this.random = random;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomExponentialBackoffPolicy"/> class.
        /// </summary>
        /// <remarks>
        ///  Default parameters: exponent 2, time between 5-10 seconds, max cap 15 minutes.
        /// </remarks>
        public RandomExponentialBackoffPolicy() : this(
            @base: 2, minDelay: TimeSpan.FromSeconds(5), maxDelay: TimeSpan.FromSeconds(10), delayCap: TimeSpan.FromMinutes(15), new Random())
        {
            // no-op
        }

        /// <inheritdoc cref="IBackoffPolicy.CanRetry"/>
        public bool CanRetry => DateTime.UtcNow >= timeToRetry;

        /// <inheritdoc cref="IBackoffPolicy.RemainingDelay"/>
        public TimeSpan RemainingDelay => CanRetry ? TimeSpan.Zero : timeToRetry - DateTime.UtcNow;

        /// <inheritdoc cref="IBackoffPolicy.Failure"/>
        public void Failure()
        {
            double factor = Math.Pow(@base, exponent);
            double minSeconds = minDelay.TotalSeconds * factor;
            double maxSeconds = Math.Min(maxDelay.TotalSeconds * factor, delayCap.TotalSeconds);
            double randomSeconds = random.NextDouble() * (maxSeconds - minSeconds) + minSeconds;
            var delay = TimeSpan.FromSeconds(randomSeconds);
            timeToRetry = DateTime.UtcNow.Add(delay);
            double nextMin = minDelay.TotalSeconds * Math.Pow(@base, exponent + 1);
            if (nextMin < delayCap.TotalSeconds)
            {
                exponent++;
            }
        }

        /// <inheritdoc cref="IBackoffPolicy.Success"/>
        public void Success()
        {
            exponent = 0;
        }
    }
}