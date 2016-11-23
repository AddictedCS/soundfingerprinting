namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class AbsComparatorTest
    {
        private readonly AbsComparator comparator = new AbsComparator();

        [Test]
        public void ArraysIsSortedCorrectlyDescending()
        {
            float[] expected = new[] { -13, -8, 7, 5, 2, -1, -0.5f, 0 };
            int[] expectedIndexes = new[] { 7, 4, 5, 6, 2, 1, 3, 0 };
            float[] arrayToSort = new[] { 0, -1, 2, -0.5f, -8, 7, 5, -13 };
            int[] indexes = Enumerable.Range(0, 8).ToArray();
            Array.Sort(arrayToSort, indexes, comparator);

            for (int i = 0; i < arrayToSort.Length; i++)
            {
                Assert.AreEqual(expected[i], arrayToSort[i]);
                Assert.AreEqual(expectedIndexes[i], indexes[i]);
            }
        }
    }
}
