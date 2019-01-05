namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class RealtimeResultEntryAggregatorTest
    {
        [Test]
        public void ShouldMergeResults()
        {
            var aggregator = new StatefulRealtimeResultEntryAggregator();

            var filter = new QueryMatchLengthFilter(5d);
            var success = new List<ResultEntry>();
            var filtered = new List<ResultEntry>();
            
            for (int i = 0; i < 10; ++i)
            {
                var aggregated = aggregator.Consume(new ResultEntry[0], filter, 5d);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
            
            Assert.IsTrue(!success.Any());

            for (int i = 0; i < 10; ++i)
            {
                var entry = new ResultEntry(new TrackData("1234", "Queen", "Bohemian Rhapsody", string.Empty, 0, 120d, new ModelReference<uint>(1)), 0d, 1.48d, 1.48d, 10d + i*1.48d, -10d -i*1.48d, 0.01233, 0, 1.48d);
                var aggregated = aggregator.Consume(new[] { entry }, filter, 1.48d);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
            
            for (int i = 0; i < 10; ++i)
            {
                var aggregated = aggregator.Consume(new ResultEntry[0], filter, 5d);
                AddAll(aggregated.SuccessEntries, success);
                AddAll(aggregated.DidNotPassThresholdEntries, filtered);
            }
            
            Assert.AreEqual(2, success.Count);
            Assert.AreEqual(1, filtered.Count);
            Assert.IsTrue(success[0].QueryMatchLength > 5d);
            Assert.IsTrue(success[1].QueryMatchLength > 5d);
            Assert.IsTrue(filtered[0].QueryMatchLength < 5d);
        }

        private static void AddAll(IEnumerable<ResultEntry> aggregated, ICollection<ResultEntry> finalResult)
        {
            foreach (var resultEntry in aggregated)
            {
                finalResult.Add(resultEntry);
            }
        }
    }
}