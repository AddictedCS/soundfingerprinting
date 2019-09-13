namespace SoundFingerprinting
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;
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
            new LocalitySensitiveHashingAlgorithm(new MinHashService(new MaxEntropyPermutations())),
            new StandardHaarWaveletDecomposition(),
            new FastFingerprintDescriptor());

        public List<HashedFingerprint> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration)
        { 
            var spectrum = spectrumService.CreateLogSpectrogram(samples, configuration.SpectrogramConfig);
            var fingerprints = CreateFingerprintsFromLogSpectrum(spectrum, configuration).ToList();
            return HashFingerprints(fingerprints, configuration);
        }

        public IEnumerable<Fingerprint> CreateFingerprintsFromLogSpectrum(IEnumerable<SpectralImage> spectralImages, FingerprintConfiguration configuration)
        {
            var fingerprints = new ConcurrentBag<Fingerprint>();
            var spectrumLength = configuration.SpectrogramConfig.ImageLength * configuration.SpectrogramConfig.LogBins;

            Parallel.ForEach(spectralImages, () => new ushort[spectrumLength], (spectralImage, loop, cachedIndexes) =>
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

        public HashedFingerprint HashImage(float[][] frame, int sequenceNumber, FingerprintConfiguration configuration)
        {
            var frames = EncodeRowCols(frame);
            int width = frame[0].Length, height = frame.Length;
            waveletDecomposition.DecomposeImageInPlace(frames, height, width, configuration.HaarWaveletNorm);
            ushort[] indexes = RangeUtils.GetRange(frames.Length);
            var schema = fingerprintDescriptor.ExtractTopWavelets(frames, configuration.TopWavelets, indexes);
            var fingerprint = new Fingerprint(schema, 0f, (uint)sequenceNumber);
            return lshAlgorithm.HashImage(fingerprint, configuration.HashingConfig, Enumerable.Empty<string>());
        }

        private float[] EncodeRowCols(float[][] image)
        {
            int width = image[0].Length;
            int height = image.Length;
            float[] floats = new float[width * height];
            for (int row = 0; row < height /*rows*/; ++row)
            {
                for (int col = 0; col < width /*cols*/; ++col)
                {
                    floats[row * width + col] = image[row][col];
                }
            }

            return floats;
        }

    }
}
