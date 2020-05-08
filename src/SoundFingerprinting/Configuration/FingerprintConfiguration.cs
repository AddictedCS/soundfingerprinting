namespace SoundFingerprinting.Configuration
{
    using System;
    using SoundFingerprinting.Configuration.Frames;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    public abstract class FingerprintConfiguration
    {
        /// <summary>
        ///  Gets or sets stride between 2 consecutive fingerprints
        /// </summary>
        public IStride Stride
        {
            get => SpectrogramConfig.Stride;
            set => SpectrogramConfig.Stride = value;
        }

        /// <summary>
        ///   Gets or sets Haar Wavelet norm. The default value is Math.Sqrt(2)
        /// </summary>
        public double HaarWaveletNorm { get; set; }

        /// <summary>
        ///  Frequency range to analyze when creating the fingerprints
        /// </summary>
        public FrequencyRange FrequencyRange
        {
            get => SpectrogramConfig.FrequencyRange;
            set => SpectrogramConfig.FrequencyRange = value;
        }

        /// <summary>
        ///  Gets or sets spectrogram creation configuration parameters
        /// </summary>
        internal SpectrogramConfig SpectrogramConfig { get; set; }

        /// <summary>
        ///   Gets number of audio samples read for one fingerprint
        /// </summary>
        internal int SamplesPerFingerprint => SpectrogramConfig.ImageLength * SpectrogramConfig.Overlap;

        /// <summary>
        ///  Gets or sets number of top wavelets to consider during wavelet transformation
        /// </summary>
        public int TopWavelets { get; set; }

        /// <summary>
        ///   Gets or sets sample rate used during generation of acoustic fingerprints
        /// </summary>
        internal int SampleRate { get; set; }

        /// <summary>
        ///  Gets or sets hashing configuration parameters
        /// </summary>
        public HashingConfig HashingConfig { get; set; }

        /// <summary>
        ///  Gets or sets fingerprint length in seconds
        /// </summary>
        public double FingerprintLengthInSeconds { get; set; }

        /// <summary>
        ///  Gets or sets the transformation for the original point that needs to be saved for second level cross-check
        /// </summary>
        public Func<Frame, byte[]>? OriginalPointSaveTransform { get; set; }

        /// <summary>
        ///  Gets or sets gaussian blur configuration applied on the frame before generating fingerprints
        /// </summary>
        public GaussianBlurConfiguration GaussianBlurConfiguration { get; set; }

        /// <summary>
        ///  Gets or sets frame normalization applied before generating fingerprints
        /// </summary>
        public IFrameNormalization FrameNormalizationTransform { get; set; }
    }
}