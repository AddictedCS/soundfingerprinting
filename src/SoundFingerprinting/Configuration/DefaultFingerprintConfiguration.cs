namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;
    
    public class DefaultFingerprintConfiguration : IFingerprintConfiguration
    {
        public DefaultFingerprintConfiguration()
        {
            FingerprintLength = 128;
            Overlap = 64;
            WdftSize = 2048;
            MinFrequency = 318;
            MaxFrequency = 2000;
            TopWavelets = 200;
            SampleRate = 5512;
            LogBase = 2;
            Stride = new IncrementalStaticStride(5115, FingerprintLength * Overlap);
            LogBins = 32;
            NormalizeSignal = false;
            UseDynamicLogBase = false;
            NumberOfLSHTables = 25;
            NumberOfMinHashesPerTable = 4;
        }

        /// <summary>
        ///   Gets number of samples to read in order to create single signature. The granularity is 1.48 seconds
        /// </summary>
        public int SamplesPerFingerprint
        {
            get
            {
                return FingerprintLength * Overlap;
            }
        }

        /// <summary>
        ///   Gets or sets overlap between the sub fingerprints, 11.6 ms
        /// </summary>
        /// <remarks>
        ///   Default = 64
        /// </remarks>
        public int Overlap { get; protected set; }

        /// <summary>
        ///   Gets or sets size of the WDFT block, 371 ms
        /// </summary>
        /// <remarks>
        ///   Default = 2048
        /// </remarks>
        public int WdftSize { get; protected set; }

        /// <summary>
        ///   Gets or sets frequency range which is taken into account when creating the signature
        /// </summary>
        /// <remarks>
        ///   Default = 318
        /// </remarks>
        public int MinFrequency { get; protected set; }

        /// <summary>
        ///   Gets or sets frequency range which is taken into account when creating the signature
        /// </summary>
        /// <remarks>
        ///   Default = 2000
        /// </remarks>
        public int MaxFrequency { get; protected set; }

        /// <summary>
        ///   Gets or sets number of Top wavelets to consider
        /// </summary>
        /// <remarks>
        ///   Default = 200
        /// </remarks>
        public int TopWavelets { get; protected set; }

        /// <summary>
        ///   Gets or sets sample rate at which the audio file will be pre-processed
        /// </summary>
        /// <remarks>
        ///   Default = 5512
        /// </remarks>
        public int SampleRate { get; protected set; }

        /// <summary>
        ///   Gets or sets log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        /// <remarks>
        ///   Default = 10
        /// </remarks>
        public double LogBase { get; protected set; }

        /// <summary>
        /// Gets or sets number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
        /// </summary>
        public int LogBins { get; protected set; }

        /// <summary>
        ///   Gets or sets signature's length
        /// </summary>
        public int FingerprintLength { get; protected set; }

        /// <summary>
        /// Gets or sets default stride size between 2 consecutive signature
        /// </summary>
        public IStride Stride { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the algorithm has to normalize the signal
        /// </summary>
        public bool NormalizeSignal { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the algorithm should use dynamic logarithmic base, instead of static
        /// </summary>
        public bool UseDynamicLogBase { get; protected set; }

        /// <summary>
        /// Gets or sets the number of LSH tables to split
        /// </summary>
        public int NumberOfLSHTables { get; protected set; }

        /// <summary>
        /// Gets or sets the number of Min Hashes per table
        /// </summary>
        public int NumberOfMinHashesPerTable { get; protected set; }
    }
}
