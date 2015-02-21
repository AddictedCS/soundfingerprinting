namespace SoundFingerprinting.Configuration
{
    public abstract class SpectrogramConfig
    {
        public static readonly SpectrogramConfig Default = new DefaultSpectrogramConfig();

        /// <summary>
        ///   Gets or sets overlap between the consecutively computed spectrum images 
        /// </summary>
        /// <remarks>64 at 5512 sample rate is aproximatelly 11.6ms</remarks>
        public int Overlap { get; set; }

        /// <summary>
        ///   Gets or sets size of the WDFT block, 371 ms
        /// </summary>
        public int WdftSize { get; set; }

        /// <summary>
        ///  Gets or sets the frequency range to be taken into account 
        /// </summary>
        public FrequencyRange FrequencyRange { get; set; }

        /// <summary>
        ///   Gets or sets log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        public double LogBase { get; set; }

        /// <summary>
        /// Gets or sets number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
        /// </summary>
        public int LogBins { get; set; }

        /// <summary>
        ///   Gets or sets signature's length
        /// </summary>
        public int ImageLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the algorithm should use dynamic logarithmic base, instead of static
        /// </summary>
        public bool UseDynamicLogBase { get; set; }
    }
}