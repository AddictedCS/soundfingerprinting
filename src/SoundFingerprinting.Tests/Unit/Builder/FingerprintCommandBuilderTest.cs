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
    }
}
