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
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    public class FingerprintService : IFingerprintService
    {
        private readonly ISpectrumService spectrumService;
        private readonly IWaveletDecomposition waveletDecomposition;
        private readonly IFingerprintDescriptor fingerprintDescriptor;
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;

        internal FingerprintService(
            ISpectrumService spectrumService,
            ILocalitySensitiveHashingAlgorithm lshAlgorithm,
            IWaveletDecomposition waveletDecomposition,
            IFingerprintDescriptor fingerprintDescriptor)
        {
            this.lshAlgorithm = lshAlgorithm;
            this.spectrumService = spectrumService;
            this.waveletDecomposition = waveletDecomposition;
            this.fingerprintDescriptor = fingerprintDescriptor;
        }

        public static FingerprintService Instance { get; } = new FingerprintService(
            new SpectrumService(new LomontFFT(), new LogUtility()),
            LocalitySensitiveHashingAlgorithm.Instance,
            new StandardHaarWaveletDecomposition(),
            new FastFingerprintDescriptor());

        public IEnumerable<HashedFingerprint> CreateFingerprintsFromAudioSamples(AudioSamples samples, FingerprintConfiguration configuration)
        { 
            var spectrumFrames = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            return CreateOriginalFingerprintsFromFrames(spectrumFrames, configuration)
                .AsParallel()
                .ToList()
                .Select(fingerprint => lshAlgorithm.Hash(fingerprint, configuration.HashingConfig, configuration.Clusters));
        }
        
        public IEnumerable<HashedFingerprint> CreateFingerprintsFromImageFrames(IEnumerable<Frame> imageFrames, FingerprintConfiguration configuration)
        {
            var frames = imageFrames.ToList();
            return CreateOriginalFingerprintsFromFrames(frames, configuration)
                .AsParallel()
                .Select(fingerprint => lshAlgorithm.HashImage(fingerprint, configuration.HashingConfig, configuration.Clusters))
                .ToList()
                .Join(frames, hashed => hashed.SequenceNumber, frame => frame.SequenceNumber, (hash, frame) =>
                {
                    byte[] transformed = configuration.OriginalPointSaveTransform(frame);
                    return new HashedFingerprint(hash.HashBins, hash.SequenceNumber, hash.StartsAt, hash.Clusters, transformed);
                });
        }

        internal IEnumerable<Fingerprint> CreateOriginalFingerprintsFromFrames(IEnumerable<Frame> frames, FingerprintConfiguration configuration)
        {
            var fingerprints = new ConcurrentBag<Fingerprint>();
            var images = frames.ToList();
            if (!images.Any())
            {
                return Enumerable.Empty<Fingerprint>();
            }
            
            var length = images.First().Length;
            Parallel.ForEach(images, () => new ushort[length], (frame, loop, cachedIndexes) =>
            {
                 waveletDecomposition.DecomposeImageInPlace(frame.ImageRowCols, frame.Rows, frame.Cols, configuration.HaarWaveletNorm);
                 RangeUtils.PopulateIndexes(length, cachedIndexes);
                 var image = fingerprintDescriptor.ExtractTopWavelets(frame.ImageRowCols, configuration.TopWavelets, cachedIndexes);
                 if (!image.IsSilence())
                 {
                     fingerprints.Add(new Fingerprint(image, frame.StartsAt, frame.SequenceNumber));
                 }

                 return cachedIndexes;
            }, 
            cachedIndexes => { });

            return fingerprints.ToList();
        }
    }
}
