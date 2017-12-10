namespace SoundFingerprinting.Tests.Unit.Query
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class MatchedPairTest
    {
        [Test]
        public void ShouldCompare2MatchedPairsBySequenceAt()
        {
            var match0 = new MatchedPair(null, new SubFingerprintData(new int[0], 0, 1, null, null), 100);
            var match1 = new MatchedPair(null, new SubFingerprintData(new int[0], 0, 2, null, null), 100);

            Assert.AreEqual(-1, match0.CompareTo(match1));
        }
    }
}
