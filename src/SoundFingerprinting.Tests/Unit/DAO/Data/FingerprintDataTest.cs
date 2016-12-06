namespace SoundFingerprinting.Tests.Unit.DAO.Data
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    public class FingerprintDataTest
    {
        [Test]
        public void ShouldIdentifyAsEqual()
        {
            var dto0 = new FingerprintData(new bool[0], new ModelReference<int>(0), new ModelReference<int>(0));
            var dto1 = new FingerprintData(new bool[0], new ModelReference<int>(0), new ModelReference<int>(0));

            Assert.AreEqual(dto0, dto1);
        }

        [Test]
        public void ShouldNotBeEqualToNull()
        {
            var dto0 = new FingerprintData(new bool[0], new ModelReference<int>(0), new ModelReference<int>(0));

            Assert.IsFalse(dto0.Equals(null));
        }
    }
}
