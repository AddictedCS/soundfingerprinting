using System.Collections.Generic;
using System.Linq;

namespace SoundFingerprinting.Tests.Unit.Utils
{
    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class SubFingerprintGroupingCounterTest
    {
        [Test]
        public void ShouldGroupAndCountCorrectly()
        {
            var tablesCount = 25;
            var count = 100;
            var lists = new List<ulong>[tablesCount];
            for (int i = 0; i < lists.Length; i++)
            {
                lists[i] = Enumerable.Range(0, count).ToList().Select(entry => (ulong)entry).ToList();
            }

            var groupingResult = SubFingerprintGroupingCounter.GroupByAndCount(lists);

            Assert.AreEqual(count, groupingResult.Count);
            foreach (var pair in groupingResult)
            {
                Assert.AreEqual(tablesCount, pair.Value);
            }
        }
    }
}
