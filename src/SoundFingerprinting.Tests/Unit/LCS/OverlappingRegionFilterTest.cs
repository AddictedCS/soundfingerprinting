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
                    new Matches(TestUtilities.GetMatchedWith(new[] { 0, 1, 3, 4 }, new[] { 0, 1, 2, 3 }).ToList()),
                    new Matches(TestUtilities.GetMatchedWith(new[] { 2 }, new[] { 1 }).ToList())
                }, PermittedGap)
                .ToList();

            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new float[] { 0, 1, 2, 3, 4 }, result[0].Select(with => with.QueryMatchAt));
        }

        [Test]
        public void ShouldFilterOverlappingRegionsWithLongestInFront2()
        {
            var result = OverlappingRegionFilter.MergeOverlappingSequences(
                    new List<Matches>
                    {
                        new Matches(TestUtilities.GetMatchedWith(new[] { 0, 1, 2 }, new[] { 0, 1, 2 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new[] { 4, 5 }, new[] { 4, 5 }).ToList())
                    }, PermittedGap)
                .ToList();

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 0, 1, 2 }, result[0].Select(with => with.QueryMatchAt));
            CollectionAssert.AreEqual(new float[] { 4, 5 }, result[1].Select(with => with.QueryMatchAt));
        }

        [Test]
        public void ShouldFilterOverlappingRegionsWithLongestInTheBack()
        {
            var result = OverlappingRegionFilter.MergeOverlappingSequences(
                    new List<Matches>
                    {
                        new Matches(TestUtilities.GetMatchedWith(new[] { 4, 6, 7, 9, 10 }, new[] { 4, 5, 6, 7, 9 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new[] { 10, 11, 17}, new[] { 10, 11, 12 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new[] { 0, 1, 2 }, new[] { 0, 1, 2 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new[] { 5 }, new[] { 5 }).ToList()),
                        new Matches(TestUtilities.GetMatchedWith(new[] { 6 }, new[] { 6 }).ToList())
                    }, PermittedGap)
                .ToList();

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 4, 5, 6, 7, 9, 10, 11, 17 }, result[0].Select(with => with.QueryMatchAt));
            CollectionAssert.AreEqual(new float[] { 0, 1, 2 }, result[1].Select(with => with.QueryMatchAt));
        }

        [Test]
        public void ShouldNotFailWithJustOneEntryAtTheInput()
        {
            var matches = new List<Matches> {new Matches(TestUtilities.GetMatchedWith(new[] {1}, new[] {1}))};
            
            var result = OverlappingRegionFilter.MergeOverlappingSequences(matches, PermittedGap);
            
            Assert.AreEqual(1, result.Count());
        }
    }
}
