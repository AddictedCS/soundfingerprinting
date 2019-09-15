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
    using SoundFingerprinting.Image;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    public class FingerprintService : IFingerprintService
    {
        private readonly ISpectrumService spectrumService;
        private readonly IWaveletDecomposition waveletDecomposition;
        private readonly IFingerprintDescriptor fingerprintDescriptor;
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;
        private readonly IImageService imageService;

        internal FingerprintService(
            ISpectrumService spectrumService,
            ILocalitySensitiveHashingAlgorithm lshAlgorithm,
            IWaveletDecomposition waveletDecomposition,
            IFingerprintDescriptor fingerprintDescriptor,
            IImageService imageService)
        {
            this.lshAlgorithm = lshAlgorithm;
            this.spectrumService = spectrumService;
            this.waveletDecomposition = waveletDecomposition;
            this.fingerprintDescriptor = fingerprintDescriptor;
            this.imageService = imageService;
        }

        public static FingerprintService Instance { get; } = new FingerprintService(
            new SpectrumService(new LomontFFT(), new LogUtility()),
            LocalitySensitiveHashingAlgorithm.Instance,
            new StandardHaarWaveletDecomposition(),
            new FastFingerprintDescriptor(), 
            new ImageService());

        public List<HashedFingerprint> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration)
        { 
            var spectrum = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            var fingerprints = CreateFingerprintsFromLogSpectrum(spectrum, configuration).ToList();
            return HashFingerprints(fingerprints, configuration);
        }

        internal IEnumerable<Fingerprint> CreateFingerprintsFromLogSpectrum(IEnumerable<Data.Frame> spectralImages, FingerprintConfiguration configuration)
        {
            var fingerprints = new ConcurrentBag<Fingerprint>();
            var spectrumLength = configuration.SpectrogramConfig.ImageLength * configuration.SpectrogramConfig.LogBins;

            Parallel.ForEach(spectralImages, () => new ushort[spectrumLength], (spectralImage, loop, cachedIndexes) =>
            {
                 waveletDecomposition.DecomposeImageInPlace(spectralImage.ImageRowCols, spectralImage.Rows, spectralImage.Cols, configuration.HaarWaveletNorm);
                 RangeUtils.PopulateIndexes(spectrumLength, cachedIndexes);
                 var image = fingerprintDescriptor.ExtractTopWavelets(spectralImage.ImageRowCols, configuration.TopWavelets, cachedIndexes);
                 if (!image.IsSilence())
                 {
                     fingerprints.Add(new Fingerprint(image, spectralImage.StartsAt, spectralImage.SequenceNumber));
                 }

                 return cachedIndexes;
            }, 
            cachedIndexes => { });

            return fingerprints.ToList();
        }

        private List<HashedFingerprint> HashFingerprints(IEnumerable<Fingerprint> fingerprints, FingerprintConfiguration configuration)
        {
            var hashedFingerprints = new ConcurrentBag<HashedFingerprint>();
            Parallel.ForEach(fingerprints, (fingerprint, state, index) =>
            { 
                var hashedFingerprint = lshAlgorithm.Hash(fingerprint, configuration.HashingConfig, configuration.Clusters);
                hashedFingerprints.Add(hashedFingerprint);
            });

            return hashedFingerprints.ToList();
        }

        public HashedFingerprint CreateFingerprintFromImage(float[][] image, int sequenceNumber, FingerprintConfiguration configuration)
        {
            var frames =  imageService.Image2RowCols(image);
            int width = image[0].Length, height = image.Length;
            waveletDecomposition.DecomposeImageInPlace(frames, height, width, configuration.HaarWaveletNorm);
            ushort[] indexes = RangeUtils.GetRange(frames.Length);
            var schema = fingerprintDescriptor.ExtractTopWavelets(frames, configuration.TopWavelets, indexes);
            var fingerprint = new Fingerprint(schema, 0f, (uint)sequenceNumber);
            return lshAlgorithm.HashImage(fingerprint, configuration.HashingConfig, Enumerable.Empty<string>());
        }
    }
}
