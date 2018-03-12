namespace SoundFingerprinting.Tests.Unit.Builder
{
    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;

    [TestFixture]
    public class FingerprintCommandBuilderTest : AbstractTest
    {
        private FingerprintCommandBuilder fingerprintCommandBuilder;

        private Mock<IFingerprintService> fingerprintService;
        private Mock<IAudioService> audioService;

        [SetUp]
        public void SetUp()
        {
            fingerprintService = new Mock<IFingerprintService>(MockBehavior.Strict);
            audioService = new Mock<IAudioService>(MockBehavior.Strict);
            fingerprintCommandBuilder = new FingerprintCommandBuilder(fingerprintService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            fingerprintService.VerifyAll();
            audioService.VerifyAll();
        }

        [Test]
        public void ShoudUseCorrectFingerprintConfigurationIsUsedWithInstanceConfig()
        {
            const string PathToAudioFile = "path-to-audio-file";

            var configuration = new DefaultFingerprintConfiguration { SpectrogramConfig = new DefaultSpectrogramConfig { ImageLength = 1234 } };
             
            var fingerprintCommand = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                              .From(PathToAudioFile)
                                                              .WithFingerprintConfig(configuration)
                                                              .UsingServices(audioService.Object);

            Assert.AreSame(configuration, fingerprintCommand.FingerprintConfiguration);
            Assert.AreEqual(configuration.SpectrogramConfig.ImageLength, fingerprintCommand.FingerprintConfiguration.SpectrogramConfig.ImageLength);
        }
    }
}
