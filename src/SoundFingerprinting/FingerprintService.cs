namespace SoundFingerprinting
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    /// <summary>
    ///  Fingerprint service.
    /// </summary>
    /// <remarks>
    ///  Class responsible for fingerprint generation. It is advised though to create fingerprints via <see cref="FingerprintCommandBuilder"/>.
    /// </remarks>
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

        /// <summary>
        ///  Gets an instance of the <see cref="FingerprintService"/> class.
        /// </summary>
        public static FingerprintService Instance { get; } = new (
            new SpectrumService(new LomontFFT(), new LogUtility()),
            LocalitySensitiveHashingAlgorithm.Instance,
            new StandardHaarWaveletDecomposition(),
            new FastFingerprintDescriptor());

        /// <inheritdoc cref="IFingerprintService.CreateFingerprintsFromAudioSamples"/>
        public FingerprintsAndHashes CreateFingerprintsFromAudioSamples(AudioSamples samples, FingerprintConfiguration configuration)
        {
            if (samples.SampleRate != configuration.SampleRate)
            {
                throw new ArgumentException($"Provided samples are not sampled in the required sample rate {configuration.SampleRate}", nameof(samples));
            }
            
            var spectrumFrames = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            var fingerprints = CreateOriginalFingerprintsFromFrames(spectrumFrames, configuration).ToList();
            var hashes = fingerprints
                .AsParallel()
                .ToList()
                .Select(fingerprint => lshAlgorithm.Hash(fingerprint, configuration.HashingConfig))
                .ToList();

            return new FingerprintsAndHashes(fingerprints, new Hashes(hashes, samples.Duration, MediaType.Audio, samples.RelativeTo, new[] {samples.Origin}, string.Empty, new Dictionary<string, string>(), samples.TimeOffset));
        }

        /// <inheritdoc cref="IFingerprintService.CreateFingerprintsFromImageFrames"/>
        public FingerprintsAndHashes CreateFingerprintsFromImageFrames(Frames imageFrames, FingerprintConfiguration configuration)
        {
            var frames = imageFrames.ToList();
            var fingerprints = CreateOriginalFingerprintsFromFrames(frames, configuration).ToList();
            var hashes = fingerprints
                .AsParallel()
                .Select(fingerprint => lshAlgorithm.HashImage(fingerprint, configuration.HashingConfig))
                .ToList();

            return new FingerprintsAndHashes(fingerprints, new Hashes(hashes, imageFrames.Duration, MediaType.Video, imageFrames.RelativeTo, new[] {imageFrames.Origin}));
        }

        internal IEnumerable<Fingerprint> CreateOriginalFingerprintsFromFrames(IEnumerable<Frame> frames, FingerprintConfiguration configuration)
        {
            var normalized = configuration.FrameNormalizationTransform.Normalize(frames);
            var images = normalized.ToList();
            if (!images.Any())
            {
                return Enumerable.Empty<Fingerprint>();
            }

            var fingerprints = new ConcurrentBag<Fingerprint>();
            var length = images.First().Length;
            var saveTransform = configuration.OriginalPointSaveTransform;
            Parallel.ForEach(images, () => new ushort[length], (frame, _, cachedIndexes) =>
                {
                    byte[] originalPoint = saveTransform(frame);
                    float[] rowCols = frame.ImageRowCols;
                    waveletDecomposition.DecomposeImageInPlace(rowCols, frame.Rows, frame.Cols, configuration.HaarWaveletNorm);
                    RangeUtils.PopulateIndexes(length, cachedIndexes);
                    var image = fingerprintDescriptor.ExtractTopWavelets(rowCols, configuration.TopWavelets, cachedIndexes);
                    if (!image.IsSilence || configuration.TreatSilenceAsSignal)
                    {
                        fingerprints.Add(new Fingerprint(image, frame.StartsAt, frame.SequenceNumber, originalPoint));
                    }

                    return cachedIndexes;
                },
                _ => { });

            return fingerprints.ToList();
        }
    }
}