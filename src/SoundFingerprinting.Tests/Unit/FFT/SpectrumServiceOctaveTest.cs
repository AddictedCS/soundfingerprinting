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
        private readonly LogUtility logUtility = new LogUtility();
        private readonly SpectrumService spectrumService;

        public SpectrumServiceOctaveTest()
        {
            spectrumService = new SpectrumService(new LomontFFT(), logUtility);
        }

        [Test]
        public void ShouldGenerateLogSpectrumFromAudioSamples()
        {
            int Fs = 5512;
            int seconds = 10;
            float[] samples = new float[Fs * seconds];
            float f1 = 410;
            float f2 = 1400;
            for (int t = 0; t < samples.Length; ++t)
            {
                samples[t] = (float)System.Math.Sin(2 * System.Math.PI * f1 / Fs * t)
                             + (float)System.Math.Sin(2 * System.Math.PI * f2 / Fs * t);
            }

            var audio = new AudioSamples(samples, "410Hz", 5512);

            var config = new DefaultSpectrogramConfig
                                           {
                                               Stride = new IncrementalStaticStride(5512)
                                           };

            var spectralImages = spectrumService.CreateLogSpectrogram(audio, config);

            Assert.AreEqual((seconds * Fs - config.WdftSize) / Fs, spectralImages.Count);

            // check with logspace(log10(318), log10(2000), 33), 410Hz are located in 4th bin,  1400Hz at 25th (0 indexed)
            int tf1 = 4;
            int tf2 = 25;

            foreach (var image in spectralImages)
            {
                float[] spectrum = image.Image;
                for (int row = 0; row < image.Rows; ++row)
                {
                    for (int col = 0; col < image.Cols; ++col)
                    {
                        int index = row * image.Cols + col;
                        if (col == tf1 || col == tf2)
                            Assert.AreEqual(col == tf1 ? 1 : 0.78, spectrum[index], 0.01);
                        else
                            Assert.AreEqual(0, spectrum[index], 0.001);
                    }
                }
            }
        }

        [Test]
        public void ShouldGenerateLogSpectrumWithBiggerOverlap()
        {
            int Fs = 5512;
            int seconds = 10;
            float[] samples = new float[Fs * seconds];

            for (int i = 0; i < samples.Length; ++i)
            {
                float value = (float)(1.3 * System.Math.Sin(2 * System.Math.PI * 15 * i / Fs));
                samples[i] = value;
            }

            var audio = new AudioSamples(samples, "test", Fs);

            var config = new DefaultSpectrogramConfig
            {
                Stride = new IncrementalStaticStride(Fs, -32 * 128 + Fs, 32 * 128),
                Overlap = 32,
                ImageLength = 128
            };

            var spectralImages = spectrumService.CreateLogSpectrogram(audio, config);

            Assert.AreEqual((10 * Fs - config.WdftSize) / Fs, spectralImages.Count);
        }
    }
}
