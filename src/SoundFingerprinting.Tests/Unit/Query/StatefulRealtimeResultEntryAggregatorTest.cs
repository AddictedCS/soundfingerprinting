namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class StatefulRealtimeResultEntryAggregatorTest
    {
        [Test]
        public void ShouldNotFailWithNullObjectPass()
        {
            var aggregator = new StatefulRealtimeResultEntryAggregator(new QueryMatchLengthFilter(5d), GetConfigWithPermittedGap(2d));

            var result = aggregator.Consume(null, 0, 0);
            
            Assert.IsFalse(result.SuccessEntries.Any());
            Assert.IsFalse(result.DidNotPassThresholdEntries.Any());
        }

        [Test]
        public void ShouldWaitAsGapPermits()
        {
            double permittedGap = 5d;
            var aggregator = new StatefulRealtimeResultEntryAggregator(new QueryMatchLengthFilter(10d), GetConfigWithPermittedGap(permittedGap));
            int firstQueryLength = 5;
            int trackLength = 5;
            var first = aggregator.Consume(new[]
            {
                new ResultEntry(GetTrack(trackLength), 100, DateTime.Now,  TestUtilities.GetMatchedWith(new[] {0, 1, 2, 3, 4}, new[] {0, 1, 2, 3, 4}).EstimateCoverage(firstQueryLength, trackLength, 1, permittedGap))
            }, firstQueryLength, 0);

            Assert.IsFalse(first.SuccessEntries.Any());
            Assert.IsFalse(first.DidNotPassThresholdEntries.Any());
            
            for (int i = 0; i < 5; ++i)
            {
                var second = aggregator.Consume(new ResultEntry[0], 1, 0);
                
                Assert.IsFalse(second.SuccessEntries.Any(), $"Iteration {i}");
                Assert.IsFalse(second.DidNotPassThresholdEntries.Any(), $"Iteration {i}");
            }

            var third = aggregator.Consume(new ResultEntry[0], 1, 0);
            
            Assert.IsFalse(third.SuccessEntries.Any());
            Assert.IsTrue(third.DidNotPassThresholdEntries.Any());
        }

        [Test]
        public void ShouldMergeResults()
        {
            double permittedGap = 2d;
            var aggregator = new StatefulRealtimeResultEntryAggregator(new QueryMatchLengthFilter(5d), GetConfigWithPermittedGap(permittedGap));

            var success = new List<ResultEntry>();
            var filtered = new List<ResultEntry>();
            
            SimulateEmptyResults(aggregator, success, filtered);
            
            Assert.IsEmpty(success);
            Assert.IsEmpty(filtered);

            const int queryLength = 1;
            const int trackLength = 10;
            const int fingerprintLength = 1;
            for (int i = 0; i < 10; ++i)
            {
                var entry = new ResultEntry(GetTrack(trackLength), 0, DateTime.Now, TestUtilities.GetMatchedWith(new[] { 0 }, new[] { i }).EstimateCoverage(queryLength, trackLength, fingerprintLength, permittedGap));
                var aggregated = aggregator.Consume(new[] { entry }, queryLength, 0);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
            
            SimulateEmptyResults(aggregator, success, filtered);
            
            Assert.AreEqual(1, success.Count);
            Assert.AreEqual(1, filtered.Count);
            Assert.IsTrue(success[0].TrackCoverageWithPermittedGapsLength > 5d);
            Assert.IsTrue(filtered[0].TrackCoverageWithPermittedGapsLength < 5d);
        }

        private static QueryConfiguration GetConfigWithPermittedGap(double permittedGap)
        {
            return new DefaultQueryConfiguration
            {
                PermittedGap = permittedGap,
                FingerprintConfiguration = new DefaultFingerprintConfiguration
                {
                    FingerprintLengthInSeconds = 1d
                }
            };
        }

        private static void SimulateEmptyResults(StatefulRealtimeResultEntryAggregator aggregator, ICollection<ResultEntry> success, ICollection<ResultEntry> filtered)
        {
            for (int i = 0; i < 10; ++i)
            {
                var aggregated = aggregator.Consume(new ResultEntry[0], 1, 0);
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
        
        private static TrackData GetTrack(double trackLength)
        {
            return new TrackData("1234", "Queen", "Bohemian Rhapsody", trackLength, new ModelReference<int>(1));
        }
    }
}