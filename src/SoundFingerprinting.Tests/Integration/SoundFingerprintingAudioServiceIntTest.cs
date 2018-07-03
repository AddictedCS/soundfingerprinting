namespace SoundFingerprinting.Tests.Integration
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class SoundFingerprintingAudioServiceIntTest : IntegrationWithSampleFilesTest
    {
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

        [Test]
        public void ShouldEstimateLengthCorrectly()
        {
            float duration = audioService.GetLengthInSeconds(PathToWav);

            Assert.AreEqual(10.0f, duration, 0.1);
        }
    }

}
