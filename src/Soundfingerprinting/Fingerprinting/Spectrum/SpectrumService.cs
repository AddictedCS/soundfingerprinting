namespace Soundfingerprinting.Fingerprinting.Spectrum
{
    using System;
    using System.Collections.Generic;

    using Soundfingerprinting.Audio.Strides;

    public class SpectrumService : ISpectrumService
    {
        public List<float[][]> CutLogarithmizedSpectrum(
            float[][] logarithmizedSpectrum, IStride strideBetweenConsecutiveImages, int fingerprintImageLength, int overlap)
        {
            int start = strideBetweenConsecutiveImages.FirstStrideSize / overlap;
            int logarithmicBins = logarithmizedSpectrum[0].Length;
            List<float[][]> spectralImages = new List<float[][]>();

            int width = logarithmizedSpectrum.GetLength(0);
            
            while (start + fingerprintImageLength < width)
            {
                float[][] spectralImage = this.AllocateMemoryForFingerprintImage(fingerprintImageLength, logarithmicBins);
                for (int i = 0; i < fingerprintImageLength; i++)
                {
                    Array.Copy(logarithmizedSpectrum[start + i], spectralImage[i], logarithmicBins);
                }

                start += fingerprintImageLength + (strideBetweenConsecutiveImages.StrideSize / overlap);
                spectralImages.Add(spectralImage);
            }

            return spectralImages;
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