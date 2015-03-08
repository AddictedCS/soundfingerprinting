namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    public class FingerprintService : IFingerprintService
    {
        private readonly ISpectrumService spectrumService;
        private readonly IWaveletDecomposition waveletDecomposition;
        private readonly IFingerprintDescriptor fingerprintDescriptor;
        private readonly IAudioSamplesNormalizer audioSamplesNormalizer;

        public FingerprintService()
            : this(
                DependencyResolver.Current.Get<ISpectrumService>(),
                DependencyResolver.Current.Get<IWaveletDecomposition>(),
                DependencyResolver.Current.Get<IFingerprintDescriptor>(),
                DependencyResolver.Current.Get<IAudioSamplesNormalizer>())
        {
        }

        internal FingerprintService(
            ISpectrumService spectrumService,
            IWaveletDecomposition waveletDecomposition,
            IFingerprintDescriptor fingerprintDescriptor,
            IAudioSamplesNormalizer audioSamplesNormalizer)
        {
            this.spectrumService = spectrumService;
            this.waveletDecomposition = waveletDecomposition;
            this.fingerprintDescriptor = fingerprintDescriptor;
            this.audioSamplesNormalizer = audioSamplesNormalizer;
        }

        public List<Fingerprint> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration)
        { 
            NormalizeAudioIfNecessary(samples, configuration);
            var spectrum = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            return CreateFingerprintsFromLogSpectrum(spectrum, configuration);
        }

        private List<Fingerprint> CreateFingerprintsFromLogSpectrum(IEnumerable<SpectralImage> spectralImages, FingerprintConfiguration configuration)
        {
            var fingerprints = new List<Fingerprint>();
            foreach (var spectralImage in spectralImages)
            {
                waveletDecomposition.DecomposeImageInPlace(spectralImage.Image); 
                bool[] image = fingerprintDescriptor.ExtractTopWavelets(spectralImage.Image, configuration.TopWavelets);
                if (!IsSilence(image))
                {
                    fingerprints.Add(new Fingerprint { Signature = image, Timestamp = spectralImage.Timestamp, SequenceNumber = spectralImage.SequenceNumber });
                }
            }

            return fingerprints;
        }

        private bool IsSilence(IEnumerable<bool> image)
        {
            return image.All(b => b == false);
        }
 
        private void NormalizeAudioIfNecessary(AudioSamples samples, FingerprintConfiguration configuration)
        {
            if (configuration.NormalizeSignal)
            {
                audioSamplesNormalizer.NormalizeInPlace(samples.Samples);
            }
        }
    }
}
