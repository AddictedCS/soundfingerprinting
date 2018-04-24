namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.LCS;

    [TestFixture]
    public class LongestIncreasingTrackSequenceTest
    {
        private readonly ILongestIncreasingTrackSequence increasingTrackSequence = new LongestIncreasingTrackSequence();

        [Test]
        public void ShouldFindLongestIncreasingSequenceWithOneElement()
        {
            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(TestUtilities.GetMatchedWith(new float[] { 0 }, new float [] { 0 }));

            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new float[] { 0 }, result[0].Select(with => with.ResultAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceWithReversedMatches()
        {
            var matches = TestUtilities.GetMatchedWith(new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new float[] { 0, 1, 2, 3, 5, 4, 6, 7, 9, 8 });

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches);

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 0, 1, 2, 3, 4, 6, 7, 9 }, result[0].Select(with => with.ResultAt));
            CollectionAssert.AreEqual(new float[] { 5, 8 }, result[1].Select(with => with.ResultAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceSimple()
        {
            var matches = TestUtilities.GetMatchedWith(new float[] { 10, 11, 12, 13, 14, 15, 16 }, new float[] { 1, 2, 3, 8, 9, 10, 11 });

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches);

            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 8, 9, 10, 11 }, result[0].Select(with => with.ResultAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence()
        {
            var matches = TestUtilities.GetMatchedWith(new float[] { 0, 1, 2, 10, 11, 12, 13, 14, 15, 16 }, new float[] { 1, 2, 3, 1, 2, 3, 4, 5, 6, 7 });

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches);

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5, 6, 7 }, result[0].Select(pair => pair.ResultAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[1].Select(pair => pair.ResultAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex()
        {
            var matches = TestUtilities.GetMatchedWith(new float[] { 0, 1, 2, 10, 11, 12, 13, 14, 15, 16 }, new float[] { 1, 2, 3, 1, 2, 3, 4, 1, 2, 3 });

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches);

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result[0].Select(pair => pair.ResultAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[1].Select(pair => pair.ResultAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[2].Select(pair => pair.ResultAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex2()
        {
            var matches = TestUtilities.GetMatchedWith(new float[] { 0, 1, 2, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, new float[] { 1, 2, 3, 4, 1, 2, 3, 4, 5, 1, 2, 3 });

            var result = increasingTrackSequence.FindAllIncreasingTrackSequences(matches);

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5 }, result[0].Select(pair => pair.ResultAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result[1].Select(pair => pair.ResultAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[2].Select(pair => pair.ResultAt));
        }
    }
}
