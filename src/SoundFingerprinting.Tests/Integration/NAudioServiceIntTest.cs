namespace SoundFingerprinting.Tests.Integration
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;

    [TestFixture]
    [Category("RequiresWindowsDLL")]
    public class NAudioServiceIntTest : IntegrationWithSampleFilesTest
    {
        private readonly IAudioService audioService = new NAudioService();

        [Test]
        public void ShouldEstimateLengthCorrectly()
        {
            float duration = audioService.GetLengthInSeconds(PathToMp3);

            Assert.AreEqual(193.123f, duration, 0.5);
        }
    }
}
