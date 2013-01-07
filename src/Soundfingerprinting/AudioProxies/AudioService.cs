namespace Soundfingerprinting.AudioProxies
{
    using System;

    using Soundfingerprinting.Fingerprinting.FFT;

    public abstract class AudioService : IAudioService
    {
        // normalize power (volume) of a wave file.
        // minimum and maximum rms to normalize from.
        private const float MinRms = 0.1f;

        private const float MaxRms = 3;

        public abstract void Dispose();

        public abstract float[] ReadMonoFromFile(
            string fileName, int sampleRate, int milliSeconds, int startMilliSeconds);

        public float[][] CreateSpectrogram(string pathToFilename, int sampleRate, int overlap, int wdftSize, double[] window)
        {
            // read 5512 Hz, Mono, PCM, with a specific proxy
            float[] samples = ReadMonoFromFile(pathToFilename, sampleRate, 0, 0);

            NormalizeInPlace(samples);

            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * wdftSize]; /*even - Re, odd - Img*/
            for (int i = 0; i < width; i++)
            {
                // take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < wdftSize; j++)
                {
                    complexSignal[2 * j] = (float)(window[j] * samples[(i * overlap) + j]);
                    /*Weight by Hann Window*/
                    complexSignal[(2 * j) + 1] = 0;
                }

                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);

                float[] band = new float[(wdftSize / 2) + 1];
                for (int j = 0; j < (wdftSize / 2) + 1; j++)
                {
                    double re = complexSignal[2 * j];
                    double img = complexSignal[(2 * j) + 1];
                    re /= (float)wdftSize / 2;
                    img /= (float)wdftSize / 2;
                    band[j] = (float)Math.Sqrt((re * re) + (img * img));
                }

                frames[i] = band;
            }

            return frames;
        }

        public float[][] CreateLogSpectrogram(
            float[] samples, int overlap, int wdftSize, int[] logFrequenciesIndexes, double[] window, int logBins)
        {
            NormalizeInPlace(samples);
            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * wdftSize]; /*even - Re, odd - Img*/
            for (int i = 0; i < width; i++)
            {
                // take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < wdftSize /*2048*/; j++)
                {
                    complexSignal[(2 * j)] = (float)(window[j] * samples[(i * overlap) + j]);
                    /*Weight by Hann Window*/
                    complexSignal[(2 * j) + 1] = 0;
                }

                // FFT transform for gathering the spectrum
                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);
                frames[i] = ExtractLogBins(complexSignal, logFrequenciesIndexes, logBins);
            }

            return frames;
        }

        private void NormalizeInPlace(float[] samples)
        {
            double squares = 0;
            int nsamples = samples.Length;
            for (int i = 0; i < nsamples; i++)
            {
                squares += samples[i] * samples[i];
            }

            // we don't want to normalize by the real RMS, because excessive clipping will occur
            float rms = (float)Math.Sqrt(squares / nsamples) * 10;

            if (rms < MinRms)
            {
                rms = MinRms;
            }

            if (rms > MaxRms)
            {
                rms = MaxRms;
            }

            for (int i = 0; i < nsamples; i++)
            {
                samples[i] /= rms;
                samples[i] = Math.Min(samples[i], 1);
                samples[i] = Math.Max(samples[i], -1);
            }
        }

        private float[] ExtractLogBins(float[] spectrum, int[] logFrequenciesIndex, int logBins)
        {
            float[] sumFreq = new float[logBins]; /*32*/
            for (int i = 0; i < logBins; i++)
            {
                int lowBound = logFrequenciesIndex[i];
                int higherBound = logFrequenciesIndex[i + 1];

                for (int k = lowBound; k < higherBound; k++)
                {
                    double re = spectrum[2 * k];
                    double img = spectrum[(2 * k) + 1];
                    sumFreq[i] += (float)Math.Sqrt((re * re) + (img * img));
                }

                sumFreq[i] = sumFreq[i] / (higherBound - lowBound);
            }

            return sumFreq;
        }
    }
}