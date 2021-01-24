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
            double permittedGap = 2d;
            var aggregator = new StatefulRealtimeResultEntryAggregator(new QueryMatchLengthFilter(10d), permittedGap);
            var first = aggregator.Consume(new[] { new ResultEntry(GetTrack(), 100, DateTime.Now,  TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {1, 2, 3}).EstimateCoverage(5, 3, 1, permittedGap)) }, 5d);

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
            double permittedGap = 1d;
            var aggregator = new StatefulRealtimeResultEntryAggregator(new QueryMatchLengthFilter(5d), permittedGap);

            var success = new List<ResultEntry>();
            var filtered = new List<ResultEntry>();
            
            SimulateEmptyResults(aggregator, success, filtered);
            
            Assert.IsTrue(!success.Any());

            for (int i = 0; i < 10; ++i)
            {
                var entry = new ResultEntry(GetTrack(), 0, DateTime.Now, TestUtilities.GetMatchedWith(new[] { i }, new[] { i }).EstimateCoverage(1, 10, 1, permittedGap));
                var aggregated = aggregator.Consume(new[] { entry }, 1);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
            
            SimulateEmptyResults(aggregator, success, filtered);
            
            Assert.AreEqual(1, success.Count);
            Assert.AreEqual(1, filtered.Count);
            Assert.IsTrue(success[0].TrackCoverageWithPermittedGapsLength > 5d);
            Assert.IsTrue(filtered[0].TrackCoverageWithPermittedGapsLength < 5d);
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
            return new TrackData("1234", "Queen", "Bohemian Rhapsody", 120d, new ModelReference<int>(1));
        }
    }
}