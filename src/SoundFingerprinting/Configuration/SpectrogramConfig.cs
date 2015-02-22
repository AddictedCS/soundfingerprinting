namespace SoundFingerprinting.Configuration
{
    using System;

    public abstract class SpectrogramConfig
    {
        public static readonly SpectrogramConfig Default = new DefaultSpectrogramConfig();
        private int overlap;
        private int wdftSize;
        private FrequencyRange frequencyRange;
        private double logBase;
        private int logBins;
        private int imageLength;

        /// <summary>
        ///   Gets or sets overlap between the consecutively computed spectrum images 
        /// </summary>
        /// <remarks>64 at 5512 sample rate is aproximatelly 11.6ms</remarks>
        public int Overlap
        {
            get
            {
                return overlap;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Overlap can't be negative", "value");
                }

                overlap = value;
            }
        }

        /// <summary>
        ///   Gets or sets size of the WDFT block, 371 ms
        /// </summary>
        public int WdftSize
        {
            get
            {
                return wdftSize;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("WdftSize can't be negative", "value");
                }

                wdftSize = value;
            }
        }

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

                if (value.Min < value.Max)
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
        /// Gets or sets number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
        /// </summary>
        public int LogBins
        {
            get
            {
                return logBins;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("LogBins can't be negative or equal to 0", "value");
                }

                logBins = value;
            }
        }

        /// <summary>
        ///   Gets or sets signature's length
        /// </summary>
        public int ImageLength
        {
            get
            {
                return imageLength;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("ImageLength can't be negative or equal to 0", "value");
                }

                imageLength = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the algorithm should use dynamic logarithmic base, instead of static
        /// </summary>
        public bool UseDynamicLogBase { get; set; }
    }
}