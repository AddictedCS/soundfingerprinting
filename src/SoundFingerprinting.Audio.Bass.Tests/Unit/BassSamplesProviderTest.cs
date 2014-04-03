namespace SoundFingerprinting.Tests.Unit.Audio
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio.Bass;

    [TestClass]
    public class BassSamplesProviderTest : AbstractTest
    {
        private const int SourceId = 100;

        private BassSamplesProvider samplesProvider;

        private Mock<IBassServiceProxy> proxy;

        [TestInitialize]
        public void SetUp()
        {
            proxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);

            samplesProvider = new BassSamplesProvider(proxy.Object, SourceId);
        }

        [TestMethod]
        public void TestGetSamplesProvider()
        {
            const int LengthInBytes = 1024 * 4;
            proxy.Setup(p => p.ChannelGetData(SourceId, It.IsAny<float[]>(), LengthInBytes)).Returns(LengthInBytes);

            var result = samplesProvider.GetNextSamples(new float[LengthInBytes / 4]);

            Assert.AreEqual(LengthInBytes, result);
        }
    }
}
