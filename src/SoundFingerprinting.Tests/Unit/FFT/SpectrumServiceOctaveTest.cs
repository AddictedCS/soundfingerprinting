namespace SoundFingerprinting.Tests.Unit.FFT
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class SpectrumServiceOctaveTest
    {
        private readonly SpectrumService spectrumService = new SpectrumService(new LomontFFT(), new LogUtility());

        [Test]
        public void ShouldGenerateLogSpectrumFromAudioSamples()
        {
            int sampleRate = 5512;
            int seconds = 10;
            float[] samples = new float[sampleRate * seconds];

            for (int i = 0; i < samples.Length; ++i)
            {
                float value = (float)(1.3 * System.Math.Sin(2 * System.Math.PI * 15 * i / 5512));
                samples[i] = value;
            }

            var audio = new AudioSamples(samples, "test", 5512);

            var config = new DefaultSpectrogramConfig
                                           {
                                               Stride = new IncrementalStaticStride(5512)
                                           };
            var spectralImages = spectrumService.CreateLogSpectrogram(audio, config);

            Assert.AreEqual((10 * 5512 - config.WdftSize) / 5512, spectralImages.Count);
        }

        [Test]
        public void ShouldGenerateLogSpectrumWithBiggerOverlap()
        {
            int sampleRate = 5512;
            int seconds = 10;
            float[] samples = new float[sampleRate * seconds];

            for (int i = 0; i < samples.Length; ++i)
            {
                float value = (float)(1.3 * System.Math.Sin(2 * System.Math.PI * 15 * i / 5512));
                samples[i] = value;
            }

            var audio = new AudioSamples(samples, "test", 5512);

            var config = new DefaultSpectrogramConfig
                         {
                             Stride = new IncrementalStaticStride(5512),
                             Overlap = 32,
                             ImageLength = 256
                         };

            var spectralImages = spectrumService.CreateLogSpectrogram(audio, config);

            Assert.AreEqual((10 * 5512 - config.WdftSize) / 5512, spectralImages.Count);
        }
    }
}
