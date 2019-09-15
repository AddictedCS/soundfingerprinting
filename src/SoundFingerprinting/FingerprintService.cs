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

        public IEnumerable<HashedFingerprint> CreateFingerprintsFromAudioSamples(AudioSamples samples, FingerprintConfiguration configuration)
        { 
            var spectrumFrames = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            return CreateOriginalFingerprintsFromFrames(spectrumFrames, configuration)
                .AsParallel()
                .Select(fingerprint => lshAlgorithm.Hash(fingerprint, configuration.HashingConfig, configuration.Clusters));
        }
        
        public IEnumerable<HashedFingerprint> CreateFingerprintsFromImageFrames(IEnumerable<Frame> imageFrames, FingerprintConfiguration configuration)
        {
            return CreateOriginalFingerprintsFromFrames(imageFrames, configuration)
                .AsParallel()
                .Select(fingerprint => lshAlgorithm.HashImage(fingerprint, configuration.HashingConfig, configuration.Clusters));
        }

        internal IEnumerable<Fingerprint> CreateOriginalFingerprintsFromFrames(IEnumerable<Frame> frames, FingerprintConfiguration configuration)
        {
            var fingerprints = new ConcurrentBag<Fingerprint>();
            var images = frames.ToList();
            var length = images.First().Length;
            Parallel.ForEach(images, () => new ushort[length], (spectralImage, loop, cachedIndexes) =>
            {
                 waveletDecomposition.DecomposeImageInPlace(spectralImage.ImageRowCols, spectralImage.Rows, spectralImage.Cols, configuration.HaarWaveletNorm);
                 RangeUtils.PopulateIndexes(length, cachedIndexes);
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
    }
}
