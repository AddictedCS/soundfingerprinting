namespace Soundfingerprinting.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.FFT.Exocortex;

    [DeploymentItem(@"bass.dll")]
    [DeploymentItem(@"bass_fx.dll")]
    [DeploymentItem(@"bassmix.dll")]
    [DeploymentItem(@"bassflac.dll")]
    [DeploymentItem(@"Kryptonite.mp3")]
    [DeploymentItem(@"Kryptonite.wav")]
    public class BaseTest
    {
        protected const int BitsPerSample = 32;
        
        protected const int SampleRate = 5512;

        protected const int WaveHeader = 58;

        protected const string PathToWav = @"Kryptonite.wav";

        protected const string PathToMp3 = @"Kryptonite.mp3";

        protected const int SamplesToRead = 128 * 64;

        protected const int MinYear = 1501;

        protected readonly bool[] GenericFingerprint = new[]
            {
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true, true, false, true, false, true, false, true, false,
                true, false, true, false, false, true, false, true, false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true, true, false, true, false, true, false, true, false,
                true, false, true, false, false, true, false, true, false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true
            };

        protected IFFTService FFTService = new ExocortexFFTService();
    }
}