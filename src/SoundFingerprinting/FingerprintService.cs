namespace SoundFingerprinting
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;
    using SoundFingerprinting.LSH;

    internal class FingerprintService : IFingerprintService
    {
        private readonly ISpectrumService spectrumService;
        private readonly IWaveletDecomposition waveletDecomposition;
        private readonly IFingerprintDescriptor fingerprintDescriptor;
        private readonly IAudioSamplesNormalizer audioSamplesNormalizer;
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;

        public FingerprintService()
            : this(
                DependencyResolver.Current.Get<ISpectrumService>(),
                DependencyResolver.Current.Get<ILocalitySensitiveHashingAlgorithm>(),
                DependencyResolver.Current.Get<IWaveletDecomposition>(),
                DependencyResolver.Current.Get<IFingerprintDescriptor>(),
                DependencyResolver.Current.Get<IAudioSamplesNormalizer>())
        {
        }

        internal FingerprintService(
            ISpectrumService spectrumService,
            ILocalitySensitiveHashingAlgorithm lshAlgorithm,
            IWaveletDecomposition waveletDecomposition,
            IFingerprintDescriptor fingerprintDescriptor,
            IAudioSamplesNormalizer audioSamplesNormalizer)
        {
            this.lshAlgorithm = lshAlgorithm;
            this.spectrumService = spectrumService;
            this.waveletDecomposition = waveletDecomposition;
            this.fingerprintDescriptor = fingerprintDescriptor;
            this.audioSamplesNormalizer = audioSamplesNormalizer;
        }

        public List<HashedFingerprint> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration)
        { 
            NormalizeAudioIfNecessary(samples, configuration);
            var spectrum = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            var fingerprints = CreateFingerprintsFromLogSpectrum(spectrum, configuration);
            return HashFingerprints(fingerprints, configuration);
        }

        public List<Fingerprint> CreateFingerprintsFromLogSpectrum(IEnumerable<SpectralImage> spectralImages, FingerprintConfiguration configuration)
        {
            var fingerprints = new ConcurrentBag<Fingerprint>();
            var spectrumLength = configuration.SpectrogramConfig.ImageLength * configuration.SpectrogramConfig.LogBins;

            Parallel.ForEach(spectralImages, 
                () => new ushort[spectrumLength],
                (spectralImage, loop, cachedIndexes) =>
                {
                    waveletDecomposition.DecomposeImageInPlace(spectralImage.Image, spectralImage.Rows, spectralImage.Cols, configuration.HaarWaveletNorm);
                    RangeUtils.PopulateIndexes(spectrumLength, cachedIndexes);
                    var image = fingerprintDescriptor.ExtractTopWavelets(spectralImage.Image, configuration.TopWavelets, cachedIndexes);
                    if (!image.IsSilence())
                    {
                        fingerprints.Add(new Fingerprint(image, spectralImage.StartsAt, spectralImage.SequenceNumber));
                    }

                    return cachedIndexes;
                }, 
                cachedIndexes => { });

            return fingerprints.ToList();
        }

        private void NormalizeAudioIfNecessary(AudioSamples samples, FingerprintConfiguration configuration)
        {
            if (configuration.NormalizeSignal)
            {
                audioSamplesNormalizer.NormalizeInPlace(samples.Samples);
            }
        }

        private List<HashedFingerprint> HashFingerprints(IEnumerable<Fingerprint> fingerprints, FingerprintConfiguration configuration)
        {
            var hashedFingerprints = new ConcurrentBag<HashedFingerprint>();
            Parallel.ForEach(fingerprints,
                (fingerprint, state, index) =>
                {
                    var hashedFingerprint = lshAlgorithm.Hash(
                        fingerprint,
                        configuration.HashingConfig.NumberOfLSHTables,
                        configuration.HashingConfig.NumberOfMinHashesPerTable,
                        configuration.Clusters);
                    hashedFingerprints.Add(hashedFingerprint);
                });

            return hashedFingerprints.ToList();
        }
    }
}
