namespace SoundFingerprinting.FFT
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Strides;

    public interface ISpectrumService
    {
        /// <summary>
        /// Cut logarithmized spetrum to spectral images
        /// </summary>
        /// <param name="logarithmizedSpectrum">Logarithmized spectrum of the initial signal</param>
        /// <param name="stride">Stride between 2 consecutive spectral image</param>
        /// <param name="configuration">Spectrum configuration</param>
        /// <returns>List of logarithmic images</returns>
        List<SpectralImage> CutLogarithmizedSpectrum(float[][] logarithmizedSpectrum, IStride stride, SpectrogramConfig configuration);
        
        float[][] CreateSpectrogram(float[] samples, int overlap, int wdftSize);

        float[][] CreateLogSpectrogram(float[] samples, SpectrogramConfig configuration);
    }
}