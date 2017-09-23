namespace SoundFingerprinting.FFT
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class SpectrumService : ISpectrumService
    {
        private readonly IFFTService fftService;
        private readonly ILogUtility logUtility;

        public SpectrumService() : this(DependencyResolver.Current.Get<IFFTService>())
        {
        }

        public SpectrumService(IFFTService fftService) : this(fftService, DependencyResolver.Current.Get<ILogUtility>())
        {
        }

        internal SpectrumService(IFFTService fftService, ILogUtility logUtility)
        {
            this.fftService = fftService;
            this.logUtility = logUtility;
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

        public List<SpectralImage> CreateLogSpectrogram(AudioSamples audioSamples,  SpectrogramConfig configuration)
        {
            int width = (audioSamples.Samples.Length - configuration.WdftSize) / configuration.Overlap;
            if (width < 1)
            {
                return new List<SpectralImage>();
            }

            float[][] frames = new float[width][];
            int[] logFrequenciesIndexes = logUtility.GenerateLogFrequenciesRanges(audioSamples.SampleRate, configuration);
            float[] window = configuration.Window.GetWindow(configuration.WdftSize);
            Parallel.For(0, width, i => 
            {
                float[] complexSignal = fftService.FFTForward(audioSamples.Samples, i * configuration.Overlap, configuration.WdftSize, window);
                frames[i] = ExtractLogBins(complexSignal, logFrequenciesIndexes, configuration.LogBins, configuration.WdftSize);
            });

            return CutLogarithmizedSpectrum(frames, audioSamples.SampleRate, configuration);
        }

        public List<SpectralImage> CutLogarithmizedSpectrum(float[][] logarithmizedSpectrum, int sampleRate, SpectrogramConfig configuration)
        {
            var strideBetweenConsecutiveImages = configuration.Stride;
            int overlap = configuration.Overlap;
            int index = GetFrequencyIndexLocationOfAudioSamples(strideBetweenConsecutiveImages.FirstStride, overlap);
            int numberOfLogBins = configuration.LogBins;
            var spectralImages = new List<SpectralImage>();

            int width = logarithmizedSpectrum.GetLength(0);
            int fingerprintImageLength = configuration.ImageLength;
            int sequenceNumber = 0;
            while (index + fingerprintImageLength <= width)
            {
                float[] spectralImage = AllocateMemoryForFingerprintImage(fingerprintImageLength, numberOfLogBins);
                for (int i = 0; i < fingerprintImageLength; i++)
                {
                    Buffer.BlockCopy(logarithmizedSpectrum[index + i], 0, spectralImage, (i * numberOfLogBins) * sizeof(float), numberOfLogBins * sizeof(float));
                }

                var startsAt = index * ((double)overlap / sampleRate);
                spectralImages.Add(new SpectralImage(spectralImage, fingerprintImageLength, numberOfLogBins, startsAt, sequenceNumber));
                index += fingerprintImageLength + GetFrequencyIndexLocationOfAudioSamples(strideBetweenConsecutiveImages.NextStride, overlap);
                sequenceNumber++;
            }

            return spectralImages;
        }

        public float[] ExtractLogBins(float[] spectrum, int[] logFrequenciesIndex, int logBins, int wdftSize)
        {
            int width = wdftSize / 2; /* 1024 */
            float[] sumFreq = new float[logBins]; /*32*/
            for (int i = 0; i < logBins; i++)
            {
                int lowBound = logFrequenciesIndex[i];
                int higherBound = logFrequenciesIndex[i + 1];

                for (int k = lowBound; k < higherBound; k++)
                {
                    double re = spectrum[2 * k] / width;
                    double img = spectrum[(2 * k) + 1] / width;
                    sumFreq[i] += (float)((re * re) + (img * img));
                }

                sumFreq[i] = sumFreq[i] / (higherBound - lowBound);
            }

            return sumFreq;
        }

        private int GetFrequencyIndexLocationOfAudioSamples(int audioSamples, int overlap)
        {
            // There are 64 audio samples in 1 unit of spectrum due to FFT window overlap (which is 64)
            return (int)((float)audioSamples / overlap);
        }

        private float[] AllocateMemoryForFingerprintImage(int fingerprintLength, int logBins)
        {
            return new float[fingerprintLength * logBins];
        }
    }
}