namespace SoundFingerprinting.Tests.Unit.Command
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class SynchronizationContextTest
    {
        private readonly AVHashes hashes = new(TestUtilities.GetRandomHashes(10f), null);

        [Test]
        public void QueryCommandNeverResumesOnTheCallerSynchronizationContext()
        {
            var context = new RecordingSynchronizationContext();
            var queryService = NewAsyncQueryService(out _);
            AVQueryResult result = null;
            RunOn(context, () =>
            {
                result = QueryCommandBuilder.Instance
                    .BuildQueryCommand()
                    .From(hashes)
                    .UsingServices(queryService.Object)
                    .Query()
                    .GetAwaiter()
                    .GetResult();
            });

            Assert.That(result, Is.Not.Null);
            Assert.That(context.Posts, Is.Zero, "a continuation inside the query command captured the caller's synchronization context");
        }

        [Test]
        public void RealtimeQueryCommandRunsStorageCallsAndCallbacksOffTheCallerSynchronizationContext()
        {
            var context = new RecordingSynchronizationContext();
            var queryService = NewAsyncQueryService(out var contextsSeenByStorage);
            var contextsSeenByCallbacks = new List<SynchronizationContext>();
            double length = 0;
            RunOn(context, () =>
            {
                length = QueryCommandBuilder.Instance
                    .BuildRealtimeQueryCommand()
                    .From(Once(hashes))
                    .WithRealtimeQueryConfig(config =>
                    {
                        config.OngoingCallback = (_, _) =>
                        {
                            contextsSeenByCallbacks.Add(SynchronizationContext.Current);
                            return Task.CompletedTask;
                        };

                        return config;
                    })
                    .UsingServices(queryService.Object)
                    .Query(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            });

            Assert.That(length, Is.GreaterThan(0));
            Assert.That(context.Posts, Is.Zero, "a continuation inside the realtime command captured the caller's synchronization context");
            Assert.That(contextsSeenByStorage, Has.Count.EqualTo(1).And.All.Null, "the storage was called on the caller's synchronization context");
            Assert.That(contextsSeenByCallbacks, Has.Count.EqualTo(1).And.All.Null, "a callback ran on the caller's synchronization context");
        }

        [Test]
        public void HarnessCountsAnUnconfiguredAwait()
        {
            var context = new RecordingSynchronizationContext();
            RunOn(context, () => Hop().GetAwaiter().GetResult());
            Assert.That(context.Posts, Is.EqualTo(1));
        }

        private static async Task Hop()
        {
            await Task.Delay(20);
        }

        private static Mock<IQueryService> NewAsyncQueryService(out List<SynchronizationContext> contextsSeen)
        {
            var seen = new List<SynchronizationContext>();
            var queryService = new Mock<IQueryService>(MockBehavior.Strict);
            queryService
                .Setup(_ => _.QueryEfficientlyAsync(It.IsAny<Hashes>(), It.IsAny<QueryConfiguration>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    lock (seen)
                    {
                        seen.Add(SynchronizationContext.Current);
                    }

                    return CompleteLater(new Candidates());
                });
            contextsSeen = seen;
            return queryService;
        }

        // completes on a pool thread after the caller suspended, so a continuation that captured the context has to post back to it
        private static async Task<T> CompleteLater<T>(T value)
        {
            await Task.Delay(30).ConfigureAwait(false);
            return value;
        }

        private static async IAsyncEnumerable<AVHashes> Once(AVHashes avHashes)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield return avHashes;
        }

        private static void RunOn(SynchronizationContext context, Action action)
        {
            var previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                action();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        /// <summary>
        ///  Counts every continuation posted to it and runs it on the thread pool, so a captured context is observed instead of deadlocking the blocked test thread.
        /// </summary>
        private sealed class RecordingSynchronizationContext : SynchronizationContext
        {
            private int posts;

            public int Posts => Volatile.Read(ref posts);

            public override void Post(SendOrPostCallback d, object state)
            {
                Interlocked.Increment(ref posts);
                ThreadPool.QueueUserWorkItem(_ => d(state));
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                Interlocked.Increment(ref posts);
                d(state);
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
