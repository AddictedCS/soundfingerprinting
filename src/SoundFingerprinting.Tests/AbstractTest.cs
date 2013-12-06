namespace SoundFingerprinting.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [DeploymentItem(@"x86", @"x86")]
    [DeploymentItem(@"x64", @"x64")]
    [DeploymentItem(@"libfftw3-3.dll")]
    [DeploymentItem(@"libfftw3f-3.dll")]
    [DeploymentItem(@"libfftw3l-3.dll")]
    [TestClass]
    public abstract class AbstractTest
    {
        protected const double Epsilon = 0.0001;

        protected const int BitsPerSample = 32;
        
        protected const int SampleRate = 5512;

        protected const int SamplesPerFingerprint = 128 * 64;

        protected const int WaveHeader = 58;

        protected const string PathToMp3 = @"Kryptonite.mp3";

        protected const string PathToSamples = @"floatsamples.bin";

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
    }
}
