namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class StatefulRealtimeResultEntryAggregatorTest
    {
        [Test]
        public void ShouldNotFailWithNullObjectPass()
        {
            var aggregator = new StatefulRealtimeResultEntryAggregator(
                new TrackMatchLengthEntryFilter(5d), 
                new NoPassRealtimeResultEntryFilter(),
                _ => { },
                new AVResultEntryCompletionStrategy(new ResultEntryCompletionStrategy(), new ResultEntryCompletionStrategy()),
                new ResultEntryConcatenator(),
                new StatefulQueryHashesConcatenator());

            var result = aggregator.Consume(null);
            
            Assert.IsFalse(result.SuccessEntries.Any());
            Assert.IsFalse(result.DidNotPassThresholdEntries.Any());
        }

        [Test]
        public void ShouldWaitAsGapPermits()
        {
            double permittedGap = 5d;
            var aggregator = new StatefulRealtimeResultEntryAggregator(
                new TrackMatchLengthEntryFilter(10d),
                new NoPassRealtimeResultEntryFilter(),
                _ => { },
                new AVResultEntryCompletionStrategy(new ResultEntryCompletionStrategy(), new ResultEntryCompletionStrategy()),
                new ResultEntryConcatenator(),
                new StatefulQueryHashesConcatenator());
            
            const int firstQueryLength = 5;
            const int trackLength = 5;
            var randomHashes = TestUtilities.GetRandomHashes(firstQueryLength);
            var audioResultEntry = new ResultEntry(GetTrack(trackLength), 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] { 0, 1, 2, 3, 4 }, new[] { 0, 1, 2, 3, 4 }).EstimateCoverage(firstQueryLength, trackLength, 1, permittedGap));
            var audioResult = new QueryResult(new[] { audioResultEntry }, randomHashes, QueryCommandStats.Zero());
            var first = aggregator.Consume(new AVQueryResult(audioResult, null, new AVHashes(randomHashes, null), new AVQueryCommandStats(QueryCommandStats.Zero(), null)));

            Assert.IsFalse(first.SuccessEntries.Any());
            Assert.IsFalse(first.DidNotPassThresholdEntries.Any());
            
            for (int i = 0; i < 5; ++i)
            {
                var second = aggregator.Consume(AVQueryResult.Empty(new AVHashes(TestUtilities.GetRandomHashes(1), null)));
                Assert.IsFalse(second.SuccessEntries.Any(), $"Iteration {i}");
                Assert.IsFalse(second.DidNotPassThresholdEntries.Any(), $"Iteration {i}");
            }

            var third = aggregator.Consume(AVQueryResult.Empty(new AVHashes(TestUtilities.GetRandomHashes(1), null)));
            
            Assert.IsFalse(third.SuccessEntries.Any());
            Assert.IsTrue(third.DidNotPassThresholdEntries.Any());
        }

        [Test]
        public void ShouldMergeResults()
        {
            double permittedGap = 2d;
            var aggregator = new StatefulRealtimeResultEntryAggregator(new TrackMatchLengthEntryFilter(5d),
                new NoPassRealtimeResultEntryFilter(),
                _ => { },
                new AVResultEntryCompletionStrategy(new ResultEntryCompletionStrategy(), new ResultEntryCompletionStrategy()),
                new ResultEntryConcatenator(),
                new StatefulQueryHashesConcatenator());

            var success = new List<AVResultEntry>();
            var filtered = new List<AVResultEntry>();
            
            SimulateEmptyResults(aggregator, success, filtered);
            
            Assert.IsEmpty(success);
            Assert.IsEmpty(filtered);

            const int queryLength = 1;
            const int trackLength = 10;
            const int fingerprintLength = 1;
            var randomHashes = TestUtilities.GetRandomHashes(1);
            for (int i = 0; i < 10; ++i)
            {
                var entry = new ResultEntry(GetTrack(trackLength), 0, DateTime.Now, TestUtilities.GetMatchedWith(new[] { 0 }, new[] { i }).EstimateCoverage(queryLength, trackLength, fingerprintLength, permittedGap));
                var audioResult = new QueryResult(new[] { entry }, randomHashes, QueryCommandStats.Zero());
                var avEntry = new AVQueryResult(audioResult, null, new AVHashes(randomHashes, null), new AVQueryCommandStats(QueryCommandStats.Zero(), null));
                var aggregated = aggregator.Consume(avEntry);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
            
            SimulateEmptyResults(aggregator, success, filtered);
            
            Assert.AreEqual(1, success.Count);
            Assert.AreEqual(1, filtered.Count);
            Assert.IsNotNull(success[0].Audio);
            Assert.IsTrue(success[0].Audio.TrackCoverageWithPermittedGapsLength > 5d);
            Assert.IsNotNull(filtered[0].Audio);
            Assert.IsTrue(filtered[0].Audio.TrackCoverageWithPermittedGapsLength < 5d);
        }

        [Test]
        public async Task ShouldAggregateCorrectly()
        {
            const int seconds = 10, sampleRate = 5512;
            var stride = new IncrementalStaticStride(512);
            var samples = TestUtilities.GenerateRandomFloatArray(seconds * sampleRate, 1234);
            var modelService = new InMemoryModelService();
            var configuration = new DefaultAVFingerprintConfiguration
            {
                Audio =
                {
                    Stride = stride
                }
            };
            
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(new AudioSamples(samples, string.Empty, sampleRate))
                .WithFingerprintConfig(configuration)
                .Hash();

            Assert.NotNull(hashes.Audio);
            
            var orderedHashes = hashes.Audio.OrderBy(_ => _.SequenceNumber).ToList();
            
            var track = new TrackInfo("1", string.Empty, string.Empty);
            modelService.Insert(track, hashes);

            var splits = new List<float[]>
            {
                samples.Take(seconds / 2 * sampleRate).ToArray(),
                samples.Skip(seconds / 2 * sampleRate).ToArray()
            };

            var blocking = new BlockingCollection<AVTrack>();
            var relativeTo = DateTime.UnixEpoch;
            foreach (float[] split in splits)
            {
                blocking.Add(new AVTrack(new AudioTrack(new AudioSamples(split, string.Empty, 5512, relativeTo)), null));
                relativeTo = relativeTo.AddSeconds(seconds / 2d);
            }
            
            blocking.CompleteAdding();
            var realtimeCollection = new BlockingRealtimeCollection<AVTrack>(blocking);

            var results = new List<ResultEntry>();
            await QueryCommandBuilder.Instance
                .BuildRealtimeQueryCommand()
                .From(realtimeCollection)
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryConfiguration.Audio.Stride = stride;
                    config.ResultEntryFilter = new NoPassRealtimeResultEntryFilter();
                    config.OngoingResultEntryFilter = new NoPassRealtimeResultEntryFilter();
                    config.DidNotPassFilterCallback = (avQueryResult) =>
                    {
                        results.AddRange(avQueryResult.ResultEntries.Select(_ => _.Audio));
                    };
                    
                    return config;
                })
                .UsingServices(modelService)
                .Query(CancellationToken.None);

            var resultEntry = results.FirstOrDefault();
            Assert.IsNotNull(resultEntry);
            Assert.IsEmpty(resultEntry.Coverage.QueryGaps);
            Assert.IsEmpty(resultEntry.Coverage.TrackGaps);

            var averageScore = resultEntry.Coverage.BestPath.Average(_ => _.Score);
            Assert.AreEqual(100, averageScore, "Did not match exactly!");
            CollectionAssert.AreEqual(orderedHashes.Select(_ => _.SequenceNumber).ToList(), resultEntry.Coverage.BestPath.Select(_ => _.TrackSequenceNumber));
            CollectionAssert.AreEqual(orderedHashes.Select(_ => _.StartsAt), resultEntry.Coverage.BestPath.Select(_ => _.TrackMatchAt));
            var zipped = orderedHashes.Select(_ => _.StartsAt).Zip(resultEntry.Coverage.BestPath.Select(_ => _.QueryMatchAt), (e, a) => new { Expected = e, Actual = a });
            foreach (var p in zipped)
            {
                Assert.AreEqual(p.Expected, p.Actual, 0.00001);
            }
        }
        
        

        private static void SimulateEmptyResults(IRealtimeAggregator aggregator, ICollection<AVResultEntry> success, ICollection<AVResultEntry> filtered)
        {
            for (int i = 0; i < 10; ++i)
            {
                var randomHashes = TestUtilities.GetRandomHashes(1);
                var avResults = new AVQueryResult(QueryResult.Empty(randomHashes, 100), null, new AVHashes(randomHashes, null), new AVQueryCommandStats(QueryCommandStats.Zero(), null));
                var aggregated = aggregator.Consume(avResults);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
        }

        private static void AddAll(IEnumerable<AVQueryResult> aggregated, ICollection<AVResultEntry> finalResult)
        {
            foreach (var resultEntry in aggregated.SelectMany(_ => _.ResultEntries))
            {
                finalResult.Add(resultEntry);
            }
        }
        
        private static TrackData GetTrack(double trackLength)
        {
            return new TrackData("1234", "Queen", "Bohemian Rhapsody", trackLength, new ModelReference<int>(1));
        }
    }
}