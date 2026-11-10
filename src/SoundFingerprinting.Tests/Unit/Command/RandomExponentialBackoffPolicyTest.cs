namespace SoundFingerprinting.Tests.Unit.Command
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using SoundFingerprinting.Command;

    [TestFixture]
    public class RandomExponentialBackoffPolicyTest
    {
        private static readonly DateTime Start = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

        [Test]
        public void RemainingDelayIsNeverNegativeWhenTheDeadlinePassesBetweenTwoClockReads()
        {
            var clock = new ScriptedClock(Start);
            var policy = new RandomExponentialBackoffPolicy(2, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), TimeSpan.Zero, new Random(0), clock.Now);
            policy.Failure();
            var deadline = Start + policy.RemainingDelay;

            // the first read lands one tick before the deadline, a second read would land after it
            clock.Script(deadline.AddTicks(-1), deadline.AddMilliseconds(1.5));
            var remaining = policy.RemainingDelay;

            Assert.That(remaining, Is.EqualTo(TimeSpan.FromTicks(1)));
            Assert.That(clock.Reads, Is.EqualTo(1), "the property read the clock more than once");
        }

        [Test]
        public void RemainingDelayIsZeroOnceTheDeadlinePassed()
        {
            var clock = new ScriptedClock(Start);
            var policy = new RandomExponentialBackoffPolicy(2, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), TimeSpan.Zero, new Random(0), clock.Now);
            policy.Failure();
            clock.Script(Start.AddMinutes(1));

            Assert.That(policy.RemainingDelay, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void RemainingDelayStaysWithinTheConfiguredWindowAfterAFailure()
        {
            var clock = new ScriptedClock(Start);
            var policy = new RandomExponentialBackoffPolicy(2, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), TimeSpan.Zero, new Random(0), clock.Now);
            policy.Failure();

            Assert.That(policy.RemainingDelay, Is.InRange(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
        }

        private sealed class ScriptedClock
        {
            private readonly Queue<DateTime> reads = new();
            private DateTime last;

            public ScriptedClock(DateTime now)
            {
                last = now;
            }

            public int Reads { get; private set; }

            public void Script(params DateTime[] values)
            {
                reads.Clear();
                foreach (var value in values)
                {
                    reads.Enqueue(value);
                }

                Reads = 0;
            }

            public DateTime Now()
            {
                Reads++;
                if (reads.Count > 0)
                {
                    last = reads.Dequeue();
                }

                return last;
            }
        }
    }
}
