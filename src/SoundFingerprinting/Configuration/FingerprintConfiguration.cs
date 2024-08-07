namespace SoundFingerprinting.Configuration
{
    using System;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration.Frames;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    /// <summary>
    ///  Fingerprint configuration class used to define the configuration parameters used to create fingerprints.
    /// </summary>
    public abstract class FingerprintConfiguration
    {
        /// <summary>
        ///  Gets or sets stride between two consecutive fingerprints.
        /// </summary>
        /// <remarks>
        ///   The smaller the stride, the better the recognition rate, as more fingerprints are generated per one second of audio. <br />
        ///   Default value <see cref="IncrementalStaticStride"/> with 512 samples stride. <br/>
        ///   Ignored by video fingerprinting algorithm.
        /// </remarks>
        public IStride Stride
        {
            get => SpectrogramConfig.Stride;
            set => SpectrogramConfig.Stride = value;
        }

        /// <summary>
        ///   Gets or sets Haar Wavelet norm. The default value is Math.Sqrt(2).
        /// </summary>
        public double HaarWaveletNorm { get; set; }

        /// <summary>
        ///  Gets or sets frequency range to analyze when creating the fingerprints.
        /// </summary>
        /// <exception cref="ArgumentException"><see cref="SoundFingerprinting.Configuration.FrequencyRange.Max"/> has to be less than <see cref="SampleRate"/> divided by 2 to satisfy <a href="https://en.wikipedia.org/wiki/Nyquist_frequency">Nyquist frequency</a>.</exception>
        /// <remarks>
        ///  Default: [318-2000], since it worked well for <a href="http://ismir2002.ircam.fr/proceedings/02-FP04-2.pdf">others</a>. <br />
        ///  Setting Min below 300Hz does not make a lot of sense since you will start picking up low frequencies which are normally regarded as noise (i.e., airplane noise). <br/>
        ///  Setting Max above 2,000Hz makes sense if the audio which you are fingerprinting contains a lot of unique harmonics (i.e., classical music). <br />
        ///  Measured in hertz. Ignored by video fingerprinting.
        /// </remarks>
        public FrequencyRange FrequencyRange
        {
            get => SpectrogramConfig.FrequencyRange;
            set
            {
                if (value.Max > SampleRate / 2)
                {
                    throw new ArgumentException($"Max frequency can't exceed Nyquist frequency {SampleRate / 2}", nameof(FrequencyRange.Max));
                }
                
                SpectrogramConfig.FrequencyRange = value;
            }
        }

        /// <summary>
        ///  Gets or sets spectrogram creation configuration parameters.
        /// </summary>
        internal SpectrogramConfig SpectrogramConfig { get; set; } = null!;

        /// <summary>
        ///   Gets number of audio samples read for one fingerprint.
        /// </summary>
        internal int SamplesPerFingerprint => SpectrogramConfig.ImageLength * SpectrogramConfig.Overlap;

        /// <summary>
        ///  Gets or sets number of top wavelets to keep after wavelet transformation.
        /// </summary>
        /// <remarks>
        ///   Each fingerprint is generated by min-hashing an image transformed through standard wavelet decomposition.
        ///   The size of the resulting image is 128 * 32 pixels. <br/>
        ///   In layman's terms, <see cref="TopWavelets"/> define how many of those pixels (sorted by magnitude) have to be taken into account by the Min-Hash algorithm to hash the image.
        ///   Audio default = 200, which is ~4.9%, Video default = 4%.
        ///   Lowering this value will instruct the algorithm to consider fewer top wavelets, potentially making it more robust to degraded audio or noisy video.
        ///   The same number of top wavelets have to be used by both <see cref="FingerprintCommand"/> and <see cref="QueryCommand"/>.
        /// </remarks>
        public int TopWavelets { get; set; }

        /// <summary>
        ///   Gets or sets sample rate used during generation of acoustic fingerprints.
        /// </summary>
        /// <remarks>
        ///  Sample rate to down sample the input audio, before extracting log-spectrum of a specific <see cref="FrequencyRange"/>. <br/>
        ///  Ignored by video fingerprinting.
        /// </remarks>
        public int SampleRate { get; set; }

        /// <summary>
        ///  Gets or sets hashing configuration parameters.
        /// </summary>
        public HashingConfig HashingConfig { get; set; } = null!;

        /// <summary>
        ///  Gets fingerprint length in seconds.
        /// </summary>
        public virtual double FingerprintLengthInSeconds => (double)SamplesPerFingerprint / SampleRate;

        /// <summary>
        ///  Gets or sets the transformation for the original point that needs to be saved for second level cross-check.
        /// </summary>
        public Func<Frame, byte[]> OriginalPointSaveTransform { get; set; } = null!;

        /// <summary>
        ///  Gets or sets frame normalization applied before generating fingerprints.
        /// </summary>
        /// <remarks>
        ///  Frame normalization allows to apply
        /// </remarks>
        public IFrameNormalization FrameNormalizationTransform { get; set; } = null!;

        /// <summary>
        ///  Gets or sets a value indicating whether to include silence fingerprints into the fingerprinted result set.
        /// </summary>
        /// <remarks>
        ///   Keep in mind that silence fingerprints will always cross-match with any other silence fingerprints. <br />
        ///   May be useful in scenarios when the dataset is small, and the content you are fingerprinting contains a lot of speech. <br />
        ///   Default value is false. <br/>
        /// </remarks>
        public bool TreatSilenceAsSignal { get; set; }
    }
}