namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    public class FingerprintService : IFingerprintService
    {
        private readonly ISpectrumService spectrumService;

        private readonly IWaveletDecomposition waveletDecomposition;

        private readonly IFingerprintDescriptor fingerprintDescriptor;

        public FingerprintService()
            : this(
                DependencyResolver.Current.Get<IFingerprintDescriptor>(),
                DependencyResolver.Current.Get<ISpectrumService>(),
                DependencyResolver.Current.Get<IWaveletDecomposition>())
        {
        }

        private FingerprintService(IFingerprintDescriptor fingerprintDescriptor, ISpectrumService spectrumService, IWaveletDecomposition waveletDecomposition)
        {
            this.spectrumService = spectrumService;
            this.waveletDecomposition = waveletDecomposition;
            this.fingerprintDescriptor = fingerprintDescriptor;
        }

        public List<bool[]> CreateFingerprints(float[] samples, IFingerprintConfiguration fingerprintConfiguration)
        {
            float[][] spectrum = spectrumService.CreateLogSpectrogram(samples, fingerprintConfiguration);
            return CreateFingerprintsFromLogSpectrum(
                spectrum,
                fingerprintConfiguration.Stride,
                fingerprintConfiguration.FingerprintLength,
                fingerprintConfiguration.Overlap,
                fingerprintConfiguration.TopWavelets);
        }

        private List<bool[]> CreateFingerprintsFromLogSpectrum(float[][] logarithmizedSpectrum, IStride stride, int fingerprintLength, int overlap, int topWavelets)
        {
            List<float[][]> spectralImages = spectrumService.CutLogarithmizedSpectrum(logarithmizedSpectrum, stride, fingerprintLength, overlap);

            waveletDecomposition.DecomposeImagesInPlace(spectralImages);

            var fingerprints = new List<bool[]>();
            foreach (var spectralImage in spectralImages)
            {
                bool[] image = fingerprintDescriptor.ExtractTopWavelets(spectralImage, topWavelets);
                if (IsSilence(image))
                {
                    continue;
                }

                fingerprints.Add(image);
            }

            return fingerprints;
        }

        private bool IsSilence(IEnumerable<bool> image)
        {
            return image.All(b => b == false);
        }
    }
}
