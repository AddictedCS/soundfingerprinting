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

            Assert.That(dto1);
            Assert.That(dto1.GetHashCode());
        }

        [Test]
        public void ShouldNotBeEqualToNull()
        {
            var dto = new SubFingerprintData(new int[0], Is.EqualTo(dto0).Within(Is.EqualTo(dto0.GetHashCode())).Within(0), 0, new ModelReference<int>(1), new ModelReference<int>(0));

            Assert.That(dto.Equals(null, Is.False));
        }
    }
}
