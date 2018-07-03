namespace SoundFingerprinting.Configuration
{
    using System;

    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Windows;

    internal abstract class SpectrogramConfig
    {
        private FrequencyRange frequencyRange;
        private double logBase;

        /// <summary>
        ///   Gets or sets overlap between the consecutively computed spectrum images 
        /// </summary>
        /// <remarks>64 at 5512 sample rate is aproximatelly 11.6ms</remarks>
        public ushort Overlap { get; set; }

        /// <summary>
        ///   Gets or sets size of the WDFT block, 371 ms
        /// </summary>
        public ushort WdftSize { get; set; }

        /// <summary>
        ///  Gets or sets the frequency range to be taken into account 
        /// </summary>
        public FrequencyRange FrequencyRange
        {
            get
            {
                return frequencyRange;
            }

            set
            {
                if (value.Min <= 0)
                {
                    throw new ArgumentException("Min frequency can't be negative", "value");
                }

                if (value.Max <= 0)
                {
                    throw new ArgumentException("Max frequency can't be negative", "value");
                }

                if (value.Min > value.Max)
                {
                    throw new ArgumentException("Min boundary cannot be bigger than Max boundary", "value");
                }

                frequencyRange = value;
            }
        }

        /// <summary>
        ///   Gets or sets log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        public double LogBase
        {
            get
            {
                return logBase;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("LogBase can't be negative or equal to 0", "value");
                }

                logBase = value;
            }
        }

        /// <summary>
        ///   Gets or sets number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
        /// </summary>
        public byte LogBins { get; set; }

        /// <summary>
        ///   Gets or sets signature's length
        /// </summary>
        public ushort ImageLength { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether the algorithm should use dynamic logarithmic base, instead of static
        /// </summary>
        public bool UseDynamicLogBase { get; set; }

        /// <summary>
        ///  Gets or sets stride between 2 consecutive spectrogram images
        /// </summary>
        public IStride Stride { get; set; }

        /// <summary>
        ///  Gets or sets window function to apply before FFT-ing
        /// </summary>
        public IWindowFunction Window { get; set; }

        /// <summary>
        ///  Gets or sets scaling function for Spectrogram Image generation
        /// </summary>
        public Func<float, float, float> ScalingFunction { get; set; }
    }
}