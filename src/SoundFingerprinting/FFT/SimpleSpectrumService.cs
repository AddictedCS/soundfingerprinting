namespace SoundFingerprinting.FFT
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;

    internal class SimpleSpectrumService : ISimpleSpectrumService
    {
        private readonly IFFTService fftService;

        public SimpleSpectrumService(IFFTService fftService)
        {
            this.fftService = fftService;
        }

        public float[][] CreateSpectrogram(AudioSamples audioSamples, int overlap, int wdftSize)
        {
            float[] window = new DefaultSpectrogramConfig().Window.GetWindow(wdftSize);
            float[] samples = audioSamples.Samples;
            int width = (samples.Length - wdftSize) / overlap;
            float[][] frames = new float[width][];
            for (int i = 0; i < width; i++)
            {
                float[] complexSignal = fftService.FFTForward(samples, i * overlap, wdftSize, window);
                float[] band = new float[(wdftSize / 2) + 1];
                for (int j = 0; j < wdftSize / 2; j++)
                {
                    double re = complexSignal[2 * j];
                    double img = complexSignal[(2 * j) + 1];

                    re /= (float)wdftSize / 2;
                    img /= (float)wdftSize / 2;

                    band[j] = (float)((re * re) + (img * img));
                }

                frames[i] = band;
            }

            return frames;
        }
    }
}
