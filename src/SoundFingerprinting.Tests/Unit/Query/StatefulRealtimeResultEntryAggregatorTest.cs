namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class StatefulRealtimeResultEntryAggregatorTest
    {
        [Test]
        public void ShouldNotFailWithNullObjectPass()
        {
            var aggregator = new StatefulRealtimeResultEntryAggregator(new QueryMatchLengthFilter(5d), 2d);

            var result = aggregator.Consume(null, 0d);
            
            Assert.IsFalse(result.SuccessEntries.Any());
            Assert.IsFalse(result.DidNotPassThresholdEntries.Any());
        }

        [Test]
        public void ShouldWaitAsGapPermits()
        {
            var aggregator = new StatefulRealtimeResultEntryAggregator(new QueryMatchLengthFilter(10d), 2d);
            var first = aggregator.Consume(new[] { new ResultEntry(GetTrack(), 0d, 2d, 2d, 10d, -10d, .95d, 120, 5d, DateTime.Now) }, 5d);

            Assert.IsFalse(first.SuccessEntries.Any());
            Assert.IsFalse(first.DidNotPassThresholdEntries.Any());
            
            for (int i = 0; i < 10; ++i)
            {
                var second = aggregator.Consume(new ResultEntry[0], 0.2d);
                
                Assert.IsFalse(second.SuccessEntries.Any());
                Assert.IsFalse(second.DidNotPassThresholdEntries.Any());
            }
            
            var third = aggregator.Consume(new ResultEntry[0], 0.2d);
            
            Assert.IsFalse(third.SuccessEntries.Any());
            Assert.IsTrue(third.DidNotPassThresholdEntries.Any());
        }

        [Test]
        public void ShouldMergeResults()
        {
            var aggregator = new StatefulRealtimeResultEntryAggregator(new QueryMatchLengthFilter(5d), 1.48d);

            var success = new List<ResultEntry>();
            var filtered = new List<ResultEntry>();
            
            SimulateEmptyResults(aggregator, success, filtered);
            
            Assert.IsTrue(!success.Any());

            for (int i = 0; i < 10; ++i)
            {
                var entry = new ResultEntry(GetTrack(), 0d, 1.48d, 1.48d, 10d + i*1.48d, -10d -i*1.48d, 0.01233, 0, 1.48d, DateTime.Now);
                var aggregated = aggregator.Consume(new[] { entry }, 1.48d);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
            
            SimulateEmptyResults(aggregator, success, filtered);
            
            Assert.AreEqual(2, success.Count);
            Assert.AreEqual(1, filtered.Count);
            Assert.IsTrue(success[0].QueryCoverageSeconds > 5d);
            Assert.IsTrue(success[1].QueryCoverageSeconds > 5d);
            Assert.IsTrue(filtered[0].QueryCoverageSeconds < 5d);
        }

        private static void SimulateEmptyResults(StatefulRealtimeResultEntryAggregator aggregator, ICollection<ResultEntry> success, ICollection<ResultEntry> filtered)
        {
            for (int i = 0; i < 10; ++i)
            {
                var aggregated = aggregator.Consume(new ResultEntry[0], 1.48d);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
        }

        private static void AddAll(IEnumerable<ResultEntry> aggregated, ICollection<ResultEntry> finalResult)
        {
            foreach (var resultEntry in aggregated)
            {
                finalResult.Add(resultEntry);
            }
        }
        
        private static TrackData GetTrack()
        {
            return new TrackData("1234", "Queen", "Bohemian Rhapsody", string.Empty, 0, 120d, new ModelReference<int>(1));
        }
    }
}