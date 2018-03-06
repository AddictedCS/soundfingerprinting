namespace SoundFingerprinting.Tests.Integration
{
    using System.IO;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;

    [TestFixture]
    public class SoundFingeprintingAudioServiceTest : IntegrationWithSampleFilesTest
    {
        private IAudioService service = new SoundFingerprintingAudioService();

        private IAudioService audioService = new NAudioService();
        private NAudioWaveFileUtility nAudioWaveFileUtility = new NAudioWaveFileUtility();

        [Test]
        public void ShouldConvert()
        {
            nAudioWaveFileUtility.WriteSamplesToFile(service.ReadMonoSamplesFromFile(PathToChirp, 5512).Samples, 5512, @"c:\chirp_44_5512.wav");
            nAudioWaveFileUtility.WriteSamplesToFile(service.ReadMonoSamplesFromFile(PathToChirp22, 5512).Samples, 5512, @"c:\chirp_22_5512.wav");
            nAudioWaveFileUtility.WriteSamplesToFile(service.ReadMonoSamplesFromFile(PathToChirp11, 5512).Samples, 5512, @"c:\chirp_11_5512.wav");
        }
    }
}
