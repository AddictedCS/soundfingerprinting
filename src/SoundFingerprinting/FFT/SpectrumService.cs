namespace SoundFingerprinting.FFT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    internal class SpectrumService : ISpectrumService
    {
        private readonly IFFTServiceUnsafe fftServiceUnsafe;
        private readonly ILogUtility logUtility;

        internal SpectrumService(IFFTServiceUnsafe fftServiceUnsafe, ILogUtility logUtility)
        {
            this.fftServiceUnsafe = fftServiceUnsafe;
            this.logUtility = logUtility;
        }

        public List<Frame> CreateLogSpectrogram(AudioSamples audioSamples, SpectrogramConfig configuration)
        {
            int wdftSize = configuration.WdftSize;
            int width = (audioSamples.Samples.Length - wdftSize) / configuration.Overlap + 1;
            if (width < configuration.ImageLength)
            {
                return new List<Frame>();
            }

            float[] frames = new float[width * configuration.LogBins];
            ushort[] logFrequenciesIndexes = logUtility.GenerateLogFrequenciesRanges(audioSamples.SampleRate, configuration);
            float[] window = configuration.Window.GetWindow(wdftSize);
            float[] samples = audioSamples.Samples;

            unsafe
            {
                Parallel.For(0, width, index =>
                {
                    float* fftArray = stackalloc float[wdftSize];
                    CopyAndWindow(fftArray, samples, index * configuration.Overlap, window);
                    fftServiceUnsafe.FFTForwardInPlace(fftArray, wdftSize);
                    ExtractLogBins(fftArray, logFrequenciesIndexes, configuration.LogBins, wdftSize, frames, index);
                });
            }

            var images = CutLogarithmizedSpectrum(frames, audioSamples.SampleRate, configuration);
            ScaleFullSpectrum(images, configuration);
            return images;
        }

        private void ScaleFullSpectrum(IEnumerable<Frame> spectralImages, SpectrogramConfig configuration)
        {
            Parallel.ForEach(spectralImages, image =>
            {
                ScaleSpectrum(image, configuration.ScalingFunction);
            });
        }

        private void ScaleSpectrum(Frame spectralFrame, Func<float, float, float> scalingFunction)
        {
            float max = spectralFrame.ImageRowCols.Max(f => Math.Abs(f));

            for (int i = 0; i < spectralFrame.ImageRowCols.Length; ++i)
            {
                spectralFrame.ImageRowCols[i] = scalingFunction(spectralFrame.ImageRowCols[i], max);
            }
        }

        public List<Frame> CutLogarithmizedSpectrum(float[] logarithmizedSpectrum, int sampleRate, SpectrogramConfig configuration)
        {
            var strideBetweenConsecutiveImages = configuration.Stride;
            int overlap = configuration.Overlap;
            int index = GetFrequencyIndexLocationOfAudioSamples(strideBetweenConsecutiveImages.FirstStride, overlap);
            int numberOfLogBins = configuration.LogBins;
            var spectralImages = new List<Frame>();

            int width = logarithmizedSpectrum.Length / numberOfLogBins;
            ushort fingerprintImageLength = configuration.ImageLength;
            int fullLength = configuration.ImageLength * numberOfLogBins;
            uint sequenceNumber = 0;
            while (index + fingerprintImageLength <= width)
            {
                float[] spectralImage = new float[fingerprintImageLength * numberOfLogBins]; 
                Buffer.BlockCopy(logarithmizedSpectrum, sizeof(float) * index * numberOfLogBins, spectralImage,  0, fullLength * sizeof(float));
                float startsAt = index * ((float)overlap / sampleRate);
                spectralImages.Add(new Frame(spectralImage, fingerprintImageLength, (ushort)numberOfLogBins, startsAt, sequenceNumber));
                index += GetFrequencyIndexLocationOfAudioSamples(strideBetweenConsecutiveImages.NextStride, overlap);
                sequenceNumber++;
            }

            return spectralImages;
        }

        private unsafe void ExtractLogBins(float* spectrum, ushort[] logFrequenciesIndex, int logBins, int wdftSize, float[] targetArray, int targetIndex)
        {
            int width = wdftSize / 2; /* 1024 */
            for (int i = 0; i < logBins; i++)
            {
                int lowBound = logFrequenciesIndex[i];
                int higherBound = logFrequenciesIndex[i + 1];

                for (int k = lowBound; k < higherBound; k++)
                {
                    double re = spectrum[2 * k] / width;
                    double img = spectrum[(2 * k) + 1] / width;
                    targetArray[(targetIndex * logBins) + i] += (float)((re * re) + (img * img));
                }

                targetArray[(targetIndex * logBins) + i] /= (higherBound - lowBound);
            }
        }

        private int GetFrequencyIndexLocationOfAudioSamples(int audioSamples, int overlap)
        {
            // There are 64 audio samples in 1 unit of spectrum due to FFT window overlap (which is 64)
            return (int)((float)audioSamples / overlap);
        }

        private unsafe void CopyAndWindow(float* fftArray, float[] samples, int prefix, float[] window)
        {
            for (int j = 0; j < window.Length; ++j)
            {
                fftArray[j] = samples[prefix + j] * window[j];
            }
        }
    }
}
