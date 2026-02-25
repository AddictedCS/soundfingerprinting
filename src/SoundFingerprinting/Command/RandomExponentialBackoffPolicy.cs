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
        private readonly TimeSpan maxTotalDelay;
        private readonly Random random;

        private long exponent;
        private DateTime timeToRetry = DateTime.MinValue;
        private DateTime firstFailureTime = DateTime.MaxValue;

        /// <summary>
        ///  Initializes a new instance of the <see cref="RandomExponentialBackoffPolicy"/> class.
        /// </summary>
        /// <param name="base">Exponent base.</param>
        /// <param name="minDelay">Random min delay.</param>
        /// <param name="maxDelay">Random max delay.</param>
        /// <param name="delayCap">Individual delay cap.</param>
        /// <param name="maxTotalDelay">
        ///     Maximum total retry duration since the first failure. When set to a positive value,
        ///     <see cref="CanRetry"/> returns <c>false</c> once the total elapsed time since the first failure
        ///     exceeds this value (retry budget exhausted). When set to <see cref="TimeSpan.Zero"/> (default),
        ///     <see cref="CanRetry"/> uses temporal check (whether enough time has passed since the last failure).
        /// </param>
        /// <param name="random">Instance of the <see cref="Random"/> class.</param>
        /// <exception cref="ArgumentException">Min delay should always be less than max delay.</exception>
        public RandomExponentialBackoffPolicy(double @base, TimeSpan minDelay, TimeSpan maxDelay, TimeSpan delayCap, TimeSpan maxTotalDelay, Random random)
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
            this.maxTotalDelay = maxTotalDelay;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomExponentialBackoffPolicy"/> class.
        /// </summary>
        /// <remarks>
        ///  Default parameters: exponent 2, time between 5-10 seconds, max cap 15 minutes.
        /// </remarks>
        public RandomExponentialBackoffPolicy() : this(@base: 2, minDelay: TimeSpan.FromSeconds(5), maxDelay: TimeSpan.FromSeconds(10), delayCap: TimeSpan.FromMinutes(15), maxTotalDelay: TimeSpan.Zero, random: new Random())
        {
            // no-op
        }

        /// <inheritdoc cref="IBackoffPolicy.CanRetry"/>
        public bool CanRetry
        {
            get
            {
                if (maxTotalDelay > TimeSpan.Zero && firstFailureTime != DateTime.MaxValue)
                {
                    return DateTime.UtcNow - firstFailureTime < maxTotalDelay;
                }

                return DateTime.UtcNow >= timeToRetry;
            }
        }

        /// <inheritdoc cref="IBackoffPolicy.RemainingDelay"/>
        public TimeSpan RemainingDelay => timeToRetry > DateTime.UtcNow ? timeToRetry - DateTime.UtcNow : TimeSpan.Zero;

        /// <inheritdoc cref="IBackoffPolicy.Failure"/>
        public void Failure()
        {
            if (firstFailureTime == DateTime.MaxValue)
            {
                firstFailureTime = DateTime.UtcNow;
            }

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
            firstFailureTime = DateTime.MaxValue;
        }
    }
}