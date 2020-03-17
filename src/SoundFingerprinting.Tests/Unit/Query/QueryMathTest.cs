namespace SoundFingerprinting.Tests.Unit.Query
{
    using NUnit.Framework;

    using SoundFingerprinting.Query;

    [TestFixture]
    public class QueryMathTest
    {
        [Test]
        public void ShouldFilterExactMatches0()
        {
            bool result = QueryMath.IsCandidatePassingThresholdVotes(new[] { 1, 2, 3, 4, 5 }, new[] { 1, 2, 3, 7, 8 }, 3);

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldFilterExactMatches1()
        {
            bool result = QueryMath.IsCandidatePassingThresholdVotes(new[] { 1, 2, 3, 4, 5 }, new[] { 1, 2, 4, 7, 8 }, 3);

            Assert.IsFalse(result);
        }
    }
}
