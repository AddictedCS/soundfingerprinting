namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.LCS;

    [TestFixture]
    public class LongestIncreasingTrackSequenceTest
    {
        private const double PermittedGap = 8192d / 5512;
        private readonly ILongestIncreasingTrackSequence increasingTrackSequence = new LongestIncreasingTrackSequence();

        [Test]
        public void ShouldFindLongestIncreasingSequenceWithOneElement()
        {
            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(TestUtilities.GetMatchedWith(new[] { 0 }, new [] { 0 }), PermittedGap);

            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new float[] { 0 }, result[0].Select(with => with.TrackMatchAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence()
        {
            var matches = TestUtilities.GetMatchedWith(new[] { 0, 1, 2, 10, 11, 12, 13, 14, 15, 16 }, new[] { 1, 2, 3, 1, 2, 3, 4, 5, 6, 7 });

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches, PermittedGap);

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5, 6, 7 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[1].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex()
        {
            var matches = TestUtilities.GetMatchedWith(
                new[] { 0, 1, 2,   10, 11, 12, 13,  24, 25, 26 }, 
                new[] { 1, 2, 3,   1,  2,  3,  4,   1, 2, 3 });

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches, 5);

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[1].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[2].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex2()
        {
            var matches = TestUtilities.GetMatchedWith(
                new[] { 7, 8, 9, 10, 21, 22, 23, 24, 25, 36, 37, 38 }, 
                new[] { 1, 2, 3, 4,  1,  2,  3,  4,  5,  1,  2,  3 });

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches, 5);

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result[1].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[2].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldGrabTheFirstMatch()
        {
            var matches = TestUtilities.GetMatchedWith(new[] {1, 2, 3, 10, 12, 13, 14}, new[] {1, 2, 3, 10, 12, 13, 14});

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches, 5);
            
            Assert.AreEqual(2, result.Count);
        }
    }
}
