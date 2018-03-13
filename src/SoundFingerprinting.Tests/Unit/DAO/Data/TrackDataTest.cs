namespace SoundFingerprinting.Tests.Unit.DAO.Data
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    public class TrackDataTest
    {
        [Test]
        public void ShouldIdentifyAsEqual()
        {
            var dto0 = new TrackData(string.Empty, string.Empty, string.Empty, string.Empty, 1990, 0d, new ModelReference<int>(0));
            var dto1 = new TrackData(string.Empty, string.Empty, string.Empty, string.Empty, 1990, 0d, new ModelReference<int>(0));

            Assert.AreEqual(dto0, dto1);
        }

        [Test]
        public void ShouldNotBeEqualToNull()
        {
            var dto = new TrackData(string.Empty, string.Empty, string.Empty, string.Empty, 1990, 0d, new ModelReference<int>(0));

            Assert.IsFalse(dto.Equals(null));
        }
    }
}
