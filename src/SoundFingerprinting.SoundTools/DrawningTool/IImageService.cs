namespace SoundFingerprinting.SoundTools.DrawningTool
{
    using System.Collections.Generic;
    using System.Drawing;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Wavelets;

    public interface IImageService
    {
        Image GetImageForFingerprint(bool[] data, int width, int height);

        Image GetImageForFingerprints(List<bool[]> fingerprints, int width, int height, int fingerprintsPerRow);

        Image GetSignalImage(float[] data, int width, int height);

        Image GetSpectrogramImage(float[][] spectrum, int width, int height);

        Image GetLogSpectralImages(
            List<SpectralImage> spectrum,
            int sampleRate,
            IStride strideBetweenConsecutiveImages,
            SpectrogramConfig config,
            int imagesPerRow);

        Image GetWaveletsImages(
            List<SpectralImage> spectrum,
            int sampleRate,
            IStride strideBetweenConsecutiveImages,
            SpectrogramConfig config, 
            int imagesPerRow);

        Image GetWaveletTransformedImage(float[][] image, IWaveletDecomposition wavelet);
    }
}
