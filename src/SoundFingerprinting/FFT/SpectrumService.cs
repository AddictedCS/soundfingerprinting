namespace SoundFingerprinting.FFT
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    public class SpectrumService : ISpectrumService
    {
        private readonly IFFTService fftService;

        private readonly ILogUtility logUtility;

        private readonly IAudioSamplesNormalizer audioSamplesNormalizer;

        public SpectrumService()
            : this(DependencyResolver.Current.Get<IFFTService>())
        {
        }

        public SpectrumService(IFFTService fftService)
            : this(fftService, DependencyResolver.Current.Get<ILogUtility>(), DependencyResolver.Current.Get<IAudioSamplesNormalizer>())
        {
        }

        private SpectrumService(IFFTService fftService, ILogUtility logUtility, IAudioSamplesNormalizer audioSamplesNormalizer)
        {
            this.fftService = fftService;
            this.logUtility = logUtility;
            this.audioSamplesNormalizer = audioSamplesNormalizer;
        }

        public float[][] CreateSpectrogram(float[] samples, int overlap, int wdftSize)
        {
            audioSamplesNormalizer.NormalizeInPlace(samples);
            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            for (int i = 0; i < width; i++)
            {
                float[] complexSignal = fftService.FFTForward(samples, i * overlap, wdftSize);
                float[] band = new float[(wdftSize / 2) + 1];
                for (int j = 0; j < (wdftSize / 2) + 1; j++)
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

        public float[][] CreateLogSpectrogram(float[] samples, IFingerprintConfiguration configuration)
        {
            if (configuration.NormalizeSignal)
            {
                audioSamplesNormalizer.NormalizeInPlace(samples);
            }

            int width = (samples.Length - configuration.WdftSize) / configuration.Overlap; /*width of the image*/
            float[][] frames = new float[width][];
            int[] logFrequenciesIndexes = logUtility.GenerateLogFrequenciesRanges(configuration);
            for (int i = 0; i < width; i++)
            {
                float[] complexSignal = fftService.FFTForward(samples, i * configuration.Overlap, configuration.WdftSize);
                frames[i] = ExtractLogBins(complexSignal, logFrequenciesIndexes, configuration.LogBins);
            }

            return frames;
        }

        public List<float[][]> CutLogarithmizedSpectrum(float[][] logarithmizedSpectrum, IStride strideBetweenConsecutiveImages, int fingerprintImageLength, int overlap)
        {
            int index = (int)((float)strideBetweenConsecutiveImages.FirstStride / overlap);
            int numberOfLogBins = logarithmizedSpectrum[0].Length;
            List<float[][]> spectralImages = new List<float[][]>();

            int width = logarithmizedSpectrum.GetLength(0);
            
            while (index + fingerprintImageLength <= width)
            {
                float[][] spectralImage = AllocateMemoryForFingerprintImage(fingerprintImageLength, numberOfLogBins);
                for (int i = 0; i < fingerprintImageLength; i++)
                {
                    Array.Copy(logarithmizedSpectrum[index + i], spectralImage[i], numberOfLogBins);
                }

                index += fingerprintImageLength + (int)((float)strideBetweenConsecutiveImages.GetNextStride() / overlap);
                spectralImages.Add(spectralImage);
            }

            return spectralImages;
        }

        private float[] ExtractLogBins(float[] spectrum, int[] logFrequenciesIndex, int logBins)
        {
            int width = spectrum.Length / 2;
            float[] sumFreq = new float[logBins]; /*32*/
            for (int i = 0; i < logBins; i++)
            {
                int lowBound = logFrequenciesIndex[i];
                int higherBound = logFrequenciesIndex[i + 1];

                for (int k = lowBound; k < higherBound; k++)
                {
                    double re = spectrum[2 * k] / ((float)width / 2);
                    double img = spectrum[(2 * k) + 1] / ((float)width / 2);
                    sumFreq[i] += (float)((re * re) + (img * img));
                }

                sumFreq[i] /= higherBound - lowBound;
            }

            return sumFreq;
        }

        private float[][] AllocateMemoryForFingerprintImage(int fingerprintLength, int logBins)
        {
            float[][] frames = new float[fingerprintLength][];
            for (int i = 0; i < fingerprintLength; i++)
            {
                frames[i] = new float[logBins];
            }

            return frames;
        }
    }
}