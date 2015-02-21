namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    public class FingerprintService : IFingerprintService
    {
        private readonly ISpectrumService spectrumService;

        private readonly IWaveletDecomposition waveletDecomposition;

        private readonly IFingerprintDescriptor fingerprintDescriptor;

        public FingerprintService()
            : this(
                DependencyResolver.Current.Get<ISpectrumService>(),
                DependencyResolver.Current.Get<IWaveletDecomposition>(),
                DependencyResolver.Current.Get<IFingerprintDescriptor>())
        {
        }

        internal FingerprintService(
            ISpectrumService spectrumService,
            IWaveletDecomposition waveletDecomposition,
            IFingerprintDescriptor fingerprintDescriptor)
        {
            this.spectrumService = spectrumService;
            this.waveletDecomposition = waveletDecomposition;
            this.fingerprintDescriptor = fingerprintDescriptor;
        }

        public List<SpectralImage> CreateSpectralImages(float[] samples, IFingerprintConfiguration configuration)
        {
            float[][] spectrum = spectrumService.CreateLogSpectrogram(samples, configuration);
            return spectrumService.CutLogarithmizedSpectrum(spectrum, configuration);
        }

        public List<bool[]> CreateFingerprints(float[] samples, IFingerprintConfiguration configuration)
        {
            float[][] spectrum = spectrumService.CreateLogSpectrogram(samples, configuration);
            return CreateFingerprintsFromLogSpectrum(spectrum, configuration);
        }

        private List<bool[]> CreateFingerprintsFromLogSpectrum(float[][] logarithmizedSpectrum, IFingerprintConfiguration configuration)
        {
            List<SpectralImage> spectralImages = spectrumService.CutLogarithmizedSpectrum(logarithmizedSpectrum, configuration);
            waveletDecomposition.DecomposeImagesInPlace(spectralImages.Select(image => image.Image));
            var fingerprints = new List<bool[]>();
            foreach (var spectralImage in spectralImages)
            {
                bool[] image = fingerprintDescriptor.ExtractTopWavelets(spectralImage.Image, configuration.TopWavelets);
                if (!IsSilence(image))
                {
                    fingerprints.Add(image);
                }
            }

            return fingerprints;
        }

        private bool IsSilence(IEnumerable<bool> image)
        {
            return image.All(b => b == false);
        }
    }
}
