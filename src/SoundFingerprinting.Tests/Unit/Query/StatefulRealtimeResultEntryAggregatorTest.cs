namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

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
                new AVResultEntryCompletionStrategy(new ResultEntryCompletionStrategy(2d), new ResultEntryCompletionStrategy(2d)),
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
                new AVResultEntryCompletionStrategy(new ResultEntryCompletionStrategy(permittedGap), new ResultEntryCompletionStrategy(permittedGap)),
                new ResultEntryConcatenator(),
                new StatefulQueryHashesConcatenator());
            
            const int firstQueryLength = 5;
            const int trackLength = 5;
            var randomHashes = TestUtilities.GetRandomHashes(firstQueryLength);
            var audioResult = new ResultEntry(GetTrack(trackLength), 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] { 0, 1, 2, 3, 4 }, new[] { 0, 1, 2, 3, 4 }).EstimateCoverage(firstQueryLength, trackLength, 1, permittedGap));
            var first = aggregator.Consume(new AVQueryResult(new []{ new AVResultEntry(audioResult, null)}, new AVHashes(randomHashes, null, AVFingerprintingStats.Zero()), new AVQueryCommandStats(QueryCommandStats.Zero(), null)));

            Assert.IsFalse(first.SuccessEntries.Any());
            Assert.IsFalse(first.DidNotPassThresholdEntries.Any());
            
            for (int i = 0; i < 5; ++i)
            {
                var second = aggregator.Consume(AVQueryResult.Empty(new AVHashes(TestUtilities.GetRandomHashes(1), null, AVFingerprintingStats.Zero())));
                Assert.IsFalse(second.SuccessEntries.Any(), $"Iteration {i}");
                Assert.IsFalse(second.DidNotPassThresholdEntries.Any(), $"Iteration {i}");
            }

            var third = aggregator.Consume(AVQueryResult.Empty(new AVHashes(TestUtilities.GetRandomHashes(1), null, AVFingerprintingStats.Zero())));
            
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
                new AVResultEntryCompletionStrategy(new ResultEntryCompletionStrategy(permittedGap), new ResultEntryCompletionStrategy(permittedGap)),
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
            for (int i = 0; i < 10; ++i)
            {
                var entry = new ResultEntry(GetTrack(trackLength), 0, DateTime.Now, TestUtilities.GetMatchedWith(new[] { 0 }, new[] { i }).EstimateCoverage(queryLength, trackLength, fingerprintLength, permittedGap));
                var avEntry = new AVQueryResult(new[] { new AVResultEntry(entry, null) }, new AVHashes(TestUtilities.GetRandomHashes(1), null, AVFingerprintingStats.Zero()), new AVQueryCommandStats(QueryCommandStats.Zero(), null));
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

        private static void SimulateEmptyResults(StatefulRealtimeResultEntryAggregator aggregator, ICollection<AVResultEntry> success, ICollection<AVResultEntry> filtered)
        {
            for (int i = 0; i < 10; ++i)
            {
                var aggregated = aggregator.Consume(new AVQueryResult(Enumerable.Empty<AVResultEntry>(), new AVHashes(TestUtilities.GetRandomHashes(1), null, AVFingerprintingStats.Zero()), new AVQueryCommandStats(QueryCommandStats.Zero(), null)));
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