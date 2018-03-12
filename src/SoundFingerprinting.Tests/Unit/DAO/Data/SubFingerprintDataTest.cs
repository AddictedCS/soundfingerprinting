namespace SoundFingerprinting.Tests.Unit.DAO.Data
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    public class SubFingerprintDataTest
    {
        [Test]
        public void ShouldIdentifyAsEqual()
        {
            var dto0 = new SubFingerprintData(new int[0], 0, 0, new ModelReference<int>(1), new ModelReference<int>(0));

            var dto1 = new SubFingerprintData(new int[0], 0, 0, new ModelReference<int>(1), new ModelReference<int>(0));

            Assert.AreEqual(dto0, dto1);
        }

        [Test]
        public void ShouldNotBeEqualToNull()
        {
            var dto = new SubFingerprintData(new int[0], 0, 0, new ModelReference<int>(1), new ModelReference<int>(0));

            Assert.IsFalse(dto.Equals(null));
        }
    }
}
