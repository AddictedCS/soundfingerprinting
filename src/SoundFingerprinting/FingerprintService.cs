namespace SoundFingerprinting
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Configuration.Frames;
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

        public Hashes CreateFingerprintsFromAudioSamples(AudioSamples samples, FingerprintConfiguration configuration)
        {
            var spectrumFrames = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            var hashes = CreateOriginalFingerprintsFromFrames(spectrumFrames, configuration)
                .AsParallel()
                .ToList()
                .Select(fingerprint => lshAlgorithm.Hash(fingerprint, configuration.HashingConfig))
                .ToList();

            return new Hashes(hashes, samples.Duration, samples.RelativeTo, new[] {samples.Origin});
        }

        public Hashes CreateFingerprintsFromImageFrames(Frames imageFrames, FingerprintConfiguration configuration)
        {
            var frames = imageFrames.ToList();
            var hashes = CreateOriginalFingerprintsFromFrames(frames, configuration)
                .AsParallel()
                .Select(fingerprint => lshAlgorithm.HashImage(fingerprint, configuration.HashingConfig))
                .ToList()
                .Join(frames, hashed => hashed.SequenceNumber, frame => frame.SequenceNumber, (hash, frame) =>
                {
                    byte[] transformed = configuration.OriginalPointSaveTransform != null ? configuration.OriginalPointSaveTransform(frame) : Array.Empty<byte>();
                    return new HashedFingerprint(hash.HashBins, hash.SequenceNumber, hash.StartsAt, transformed);
                })
                .ToList();

            return new Hashes(hashes, imageFrames.Duration, imageFrames.RelativeTo, new[] {imageFrames.Origin});
        }

        internal IEnumerable<Fingerprint> CreateOriginalFingerprintsFromFrames(IEnumerable<Frame> frames, FingerprintConfiguration configuration)
        {
            var normalized = configuration.FrameNormalizationTransform.Normalize(frames);
            var blurred = configuration.GaussianBlurConfiguration.GaussianFilter switch
            {
                GaussianFilter.None => normalized,
                _ => BlurFrames(normalized, configuration.GaussianBlurConfiguration)
            };


            var images = blurred.ToList();
            if (!images.Any())
            {
                return Enumerable.Empty<Fingerprint>();
            }

            var fingerprints = new ConcurrentBag<Fingerprint>();
            var length = images.First().Length;
            Parallel.ForEach(images, () => new ushort[length], (frame, loop, cachedIndexes) =>
                {
                    float[] rowCols = configuration.OriginalPointSaveTransform != null ? frame.GetImageRowColsCopy() : frame.ImageRowCols;
                    waveletDecomposition.DecomposeImageInPlace(rowCols, frame.Rows, frame.Cols, configuration.HaarWaveletNorm);
                    RangeUtils.PopulateIndexes(length, cachedIndexes);
                    var image = fingerprintDescriptor.ExtractTopWavelets(rowCols, configuration.TopWavelets, cachedIndexes);
                    if (!image.IsSilence())
                    {
                        fingerprints.Add(new Fingerprint(image, frame.StartsAt, frame.SequenceNumber));
                    }

                    return cachedIndexes;
                },
                cachedIndexes => { });

            return fingerprints.ToList();
        }

        private static IEnumerable<Frame> BlurFrames(IEnumerable<Frame> frames, GaussianBlurConfiguration blurConfiguration)
        {
            double[,] kernel = GaussianBlurKernel.Kernel2D(blurConfiguration.Kernel, blurConfiguration.Sigma);
            return frames
                .AsParallel()
                .Select(frame =>
                {
                    float[][] image = ImageService.Instance.RowCols2Image(frame.ImageRowCols, frame.Rows, frame.Cols);
                    float[][] blurred = GrayImage.Convolve(image, kernel);
                    return new Frame(blurred, frame.StartsAt, frame.SequenceNumber);
                })
                .ToList();
        }
    }
}