namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System.Collections.Generic;
    using System.Linq;

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
            var lists = new List<uint>[tablesCount];
            for (int i = 0; i < lists.Length; i++)
            {
                lists[i] = Enumerable.Range(0, count).ToList().Select(entry => (uint)entry).ToList();
            }

            var groupingResult = SubFingerprintGroupingCounter.GroupByAndCount(lists, 5).ToList();

            Assert.AreEqual(count, groupingResult.Count);
        }
    }
}
