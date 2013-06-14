namespace SoundFingerprinting.Image
{
    using System.Collections.Generic;
    using System.Drawing;

    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Wavelets;

    public interface IImageService
    {
        Image GetImageForFingerprint(bool[] data, int width, int height);

        Image GetImageForFingerprints(List<bool[]> fingerprints, int width, int height, int fingerprintsPerRow);

        Image GetSignalImage(float[] data, int width, int height);

        Image GetSpectrogramImage(float[][] spectrum, int width, int height);

        Image GetLogSpectralImages(
            float[][] spectrum,
            IStride strideBetweenConsecutiveImages,
            int fingerprintLength,
            int overlap,
            int imagesPerRow);

        Image GetWaveletsImages(
            float[][] spectrum,
            IStride strideBetweenConsecutiveImages,
            int fingerprintLength,
            int overlap,
            int imagesPerRow);

        Image GetWaveletTransformedImage(float[][] image, IWaveletDecomposition wavelet);
    }
}
