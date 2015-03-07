namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Audio;
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

        public List<SpectralImage> CreateSpectralImages(AudioSamples samples, FingerprintConfiguration configuration)
        {
            return spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
        }

        public List<bool[]> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration)
        {
            var spectrum = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            return CreateFingerprintsFromLogSpectrum(spectrum, configuration);
        }

        private List<bool[]> CreateFingerprintsFromLogSpectrum(List<SpectralImage> spectralImages, FingerprintConfiguration configuration)
        {
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
