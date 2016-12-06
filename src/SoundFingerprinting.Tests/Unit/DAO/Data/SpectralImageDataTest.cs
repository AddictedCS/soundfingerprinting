namespace SoundFingerprinting.Tests.Unit.DAO.Data
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    public class SpectralImageDataTest
    {
        [Test]
        public void ShouldIdentifyAsEqual()
        {
            var dto0 = new SpectralImageData(new float[0], 0, new ModelReference<int>(0), new ModelReference<int>(0));
            var dto1 = new SpectralImageData(new float[0], 0, new ModelReference<int>(0), new ModelReference<int>(0));

            Assert.AreEqual(dto0, dto1);
        }

        [Test]
        public void ShouldNotBeEqualToNull()
        {
            var dto0 = new SpectralImageData(new float[0], 0, new ModelReference<int>(0), new ModelReference<int>(0));

            Assert.IsFalse(dto0.Equals(null));
        }
    }
}
