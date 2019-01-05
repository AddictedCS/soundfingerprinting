namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
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

            var finalResult = new List<ResultEntry>();
            for (int i = 0; i < 10; ++i)
            {
                var aggregated = aggregator.Consume(new ResultEntry[0], 5d, 5d);
                AddAll(aggregated, finalResult);
            }
            
            Assert.IsTrue(!finalResult.Any());

            for (int i = 0; i < 10; ++i)
            {
                var entry = new ResultEntry(new TrackData("1234", "Queen", "Bohemian Rhapsody", string.Empty, 0, 120d, new ModelReference<uint>(1)), 0d, 1.48d, 1.48d, 10d + i*1.48d, -10d -i*1.48d, 0.01233, 0, 1.48d);
                var aggregated = aggregator.Consume(new[] { entry }, 5d, 1.48d);
                AddAll(aggregated, finalResult);
            }
            
            Assert.AreEqual(2, finalResult.Count);
            for (int i = 0; i < 5; ++i)
            {
                var aggregated = aggregator.Consume(new ResultEntry[0], 5d, 1.48d);
                foreach (var resultEntry in aggregated)
                {
                    finalResult.Add(resultEntry);
                }
            }
            
            Assert.AreEqual(3, finalResult.Count);
            Assert.IsTrue(finalResult[0].QueryMatchLength > 5d);
            Assert.IsTrue(finalResult[1].QueryMatchLength > 5d);
            Assert.IsTrue(finalResult[2].QueryMatchLength < 5d);
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