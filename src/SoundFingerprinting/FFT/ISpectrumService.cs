namespace SoundFingerprinting.FFT
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;

    public interface ISpectrumService
    {
        /// <summary>
        /// Cut logarithmized spetrum to spectral images
        /// </summary>
        /// <param name="logarithmizedSpectrum">Logarithmized spectrum of the initial signal</param>
        /// <param name="configuration">Fingerprint configuration</param>
        /// <returns>List of logarithmic images</returns>
        List<SpectralImage> CutLogarithmizedSpectrum(float[][] logarithmizedSpectrum, IFingerprintConfiguration configuration);
        
        float[][] CreateSpectrogram(float[] samples, int overlap, int wdftSize);

        float[][] CreateLogSpectrogram(float[] samples, IFingerprintConfiguration configuration);
    }
}