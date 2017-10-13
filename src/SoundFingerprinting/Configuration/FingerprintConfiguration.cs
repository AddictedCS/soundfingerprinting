namespace SoundFingerprinting.Configuration
{
    using System.Collections.Generic;

    using SoundFingerprinting.Strides;

    public abstract class FingerprintConfiguration
    {
        /// <summary>
        ///  Gets or sets stride between 2 consecutive fingerprints
        /// </summary>
        public IStride Stride
        {
            get
            {
                return SpectrogramConfig.Stride;
            }

            set
            {
                SpectrogramConfig.Stride = value;
            }
        }

        /// <summary>
        ///   Gets or sets the list of assigned clusters to all generated fingerprints
        /// </summary>
        public IEnumerable<string> Clusters { get; set; }

        /// <summary>
        ///   Gets or sets Haar Wavelet norm. The universaly recognized norm is sqrt(2), though for acoustic fingerprinting 1 works very well for noisy scenarious
        /// </summary>
        public double HaarWaveletNorm { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether the algorithm has to normalize the audio signal
        /// </summary>
        internal bool NormalizeSignal { get; set; }

        /// <summary>
        ///  Gets or sets spectrogram creation configuration parameters
        /// </summary>
        internal SpectrogramConfig SpectrogramConfig { get; set; }

        /// <summary>
        ///   Gets number of audio samples read for one fingerprint
        /// </summary>
        internal int SamplesPerFingerprint
        {
            get
            {
                return SpectrogramConfig.ImageLength * SpectrogramConfig.Overlap;
            }
        }

        /// <summary>
        ///  Gets or sets number of top wavelets to consider during wavelet transformation
        /// </summary>
        internal int TopWavelets { get; set; }

        /// <summary>
        ///   Gets or sets sample rate used during generation of acoustic fingerprints
        /// </summary>
        internal int SampleRate { get; set; }

        /// <summary>
        ///  Gets or sets hashing configuration parameters
        /// </summary>
        internal HashingConfig HashingConfig { get; set; }

        /// <summary>
        ///  Gets fingerprint length in seconds
        /// </summary>
        internal double FingerprintLengthInSeconds
        {
            get
            {
                return (double)SamplesPerFingerprint / SampleRate;
            }
        }
    }
}