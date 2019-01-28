namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class OverlappingRegionFilterTest
    {
        private const double PermittedGap = 8192d/5512;
        
        [Test]
        public void ShouldMergeOverlappingRegionsWithLongestInFront()
        {
            var result = OverlappingRegionFilter.MergeOverlappingSequences(
                new List<Matches>
                {
                    new Matches(TestUtilities.GetMatchedWith(new float[] { 0, 1, 3, 4 }, new float[] { 0, 1, 2, 3 }).ToList()),
                    new Matches(TestUtilities.GetMatchedWith(new float[] { 2 }, new float[] { 1 }).ToList())
                }, PermittedGap)
                .ToList();

            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new float[] { 0, 1, 2, 3, 4 }, result[0].Select(with => with.QueryAt));
        }

        [Test]
        public void ShouldFilterOverlappingRegionsWithLongestInFront2()
        {
            var result = OverlappingRegionFilter.MergeOverlappingSequences(
                    new List<Matches>
                    {
                        new Matches(TestUtilities.GetMatchedWith(new float[] { 0, 1, 2 }, new float[] { 0, 1, 2 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new float[] { 4, 5 }, new float[] { 4, 5 }).ToList())
                    }, PermittedGap)
                .ToList();

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 0, 1, 2 }, result[0].Select(with => with.QueryAt));
            CollectionAssert.AreEqual(new float[] { 4, 5 }, result[1].Select(with => with.QueryAt));
        }

        [Test]
        public void ShouldFilterOverlappingRegionsWithLongestInTheBack()
        {
            var result = OverlappingRegionFilter.MergeOverlappingSequences(
                    new List<Matches>
                    {
                        new Matches(TestUtilities.GetMatchedWith(new float[] { 4, 6, 7, 9, 10 }, new float[] { 4, 5, 6, 7, 9 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new float[] { 10, 11, 17}, new float[] { 10, 11, 12 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new float[] { 0, 1, 2 }, new float[] { 0, 1, 2 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new float[] { 5 }, new float[] { 5 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new float[] { 6 }, new float[] { 6 }).ToList())
                    }, PermittedGap)
                .ToList();

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 4, 5, 6, 7, 9, 10, 11, 17 }, result[0].Select(with => with.QueryAt));
            CollectionAssert.AreEqual(new float[] { 0, 1, 2 }, result[1].Select(with => with.QueryAt));
        }
    }
}
