namespace SoundFingerprinting
{
    using System;
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
            var fingerprints = CreateOriginalFingerprintsFromFrames(spectrumFrames, configuration);
            var hashes = fingerprints
                .AsParallel()
                .Select(fingerprint => lshAlgorithm.Hash(fingerprint, configuration.HashingConfig))
                .ToList();

            return new FingerprintsAndHashes(fingerprints, new Hashes(hashes, samples.Duration, MediaType.Audio, samples.RelativeTo, [samples.Origin], string.Empty, new Dictionary<string, string>(), samples.TimeOffset));
        }

        /// <inheritdoc cref="IFingerprintService.CreateFingerprintsFromImageFrames"/>
        public FingerprintsAndHashes CreateFingerprintsFromImageFrames(Frames imageFrames, FingerprintConfiguration configuration)
        {
            var fingerprints = CreateOriginalFingerprintsFromFrames(imageFrames, configuration);
            var hashes = fingerprints
                .AsParallel()
                .Select(fingerprint => lshAlgorithm.HashImage(fingerprint, configuration.HashingConfig))
                .ToList();

            return new FingerprintsAndHashes(fingerprints, new Hashes(hashes, imageFrames.Duration, MediaType.Video, imageFrames.RelativeTo, [imageFrames.Origin]));
        }

        private List<Fingerprint> CreateOriginalFingerprintsFromFrames(IEnumerable<Frame> frames, FingerprintConfiguration configuration)
        {
            var normalized = configuration.FrameNormalizationTransform.Normalize(frames);
            var images = normalized.ToList();
            if (images.Count == 0)
            {
                return [];
            }

            // pre-sized array for zero-contention parallel writes - each thread writes to its own index
            var fingerprints = new Fingerprint[images.Count];
            var length = images[0].Length;
            var saveTransform = configuration.OriginalPointSaveTransform;
            Parallel.For(0, images.Count, () => new ushort[length], (index, _, cachedIndexes) =>
            {
                var frame = images[index];
                byte[] originalPoint = saveTransform(frame);
                float[] rowCols = frame.ImageRowCols;
                waveletDecomposition.DecomposeImageInPlace(rowCols, frame.Rows, frame.Cols, configuration.HaarWaveletNorm);
                RangeUtils.PopulateIndexes(length, cachedIndexes);
                var image = fingerprintDescriptor.ExtractTopWavelets(rowCols, configuration.TopWavelets, cachedIndexes);
                fingerprints[index] = new Fingerprint(image, frame.StartsAt, frame.SequenceNumber, originalPoint);
                return cachedIndexes;
            },
            _ => { });

            return configuration.TreatSilenceAsSignal ? fingerprints.ToList() : fingerprints.Where(fingerprint => !fingerprint.Schema.IsSilence).ToList();
        }
    }
}