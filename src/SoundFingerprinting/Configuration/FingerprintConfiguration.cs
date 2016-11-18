namespace SoundFingerprinting.Configuration
{
    using System;

    using SoundFingerprinting.Strides;

    public abstract class FingerprintConfiguration
    {
        private int topWavelets;
        private int sampleRate;

        /// <summary>
        ///    Gets or sets stride between 2 consecutive fingerprints
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
        /// Gets or sets a value indicating whether the algorithm has to normalize the audio signal
        /// </summary>
        internal bool NormalizeSignal { get; set; }

        /// <summary>
        /// Gets or sets spectrogram creation configuration parameters
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
        ///   Gets or sets number of Top wavelets to consider
        /// </summary>
        internal int TopWavelets
        {
            get
            {
                return topWavelets;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("TopWavelets can't be negative or equal to 0", "value");
                }

                topWavelets = value;
            }
        }

        /// <summary>
        ///   Gets or sets sample rate
        /// </summary>
        internal int SampleRate
        {
            get
            {
                return sampleRate;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("SampleRate can't be negative or equal to 0", "value");
                }

                sampleRate = value;
            }
        }

        /// <summary>
        /// Gets or sets hashing configuration parameters
        /// </summary>
        internal HashingConfig HashingConfig { get; set; }

        internal double FingerprintLengthInSeconds
        {
            get
            {
                return (double)SamplesPerFingerprint / SampleRate;
            }
        }
    }
}