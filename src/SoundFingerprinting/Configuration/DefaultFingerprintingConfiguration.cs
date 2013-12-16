namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;
    
    public class DefaultFingerprintingConfiguration : IFingerprintingConfiguration
    {
        public DefaultFingerprintingConfiguration()
        {
            FingerprintLength = 128;
            Overlap = 64;
            SamplesPerFingerprint = FingerprintLength * Overlap;
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
        /// <remarks>
        ///   Default = 8192
        /// </remarks>
        public int SamplesPerFingerprint { get; private set; }

        /// <summary>
        ///   Gets overlap between the sub fingerprints, 11.6 ms
        /// </summary>
        /// <remarks>
        ///   Default = 64
        /// </remarks>
        public int Overlap { get; private set; }

        /// <summary>
        ///   Gets size of the WDFT block, 371 ms
        /// </summary>
        /// <remarks>
        ///   Default = 2048
        /// </remarks>
        public int WdftSize { get; private set; }

        /// <summary>
        ///   Gets frequency range which is taken into account when creating the signature
        /// </summary>
        /// <remarks>
        ///   Default = 318
        /// </remarks>
        public int MinFrequency { get; private set; }

        /// <summary>
        ///   Gets frequency range which is taken into account when creating the signature
        /// </summary>
        /// <remarks>
        ///   Default = 2000
        /// </remarks>
        public int MaxFrequency { get; private set; }

        /// <summary>
        ///   Gets number of Top wavelets to consider
        /// </summary>
        /// <remarks>
        ///   Default = 200
        /// </remarks>
        public int TopWavelets { get; private set; }

        /// <summary>
        ///   Gets sample rate at which the audio file will be pre-processed
        /// </summary>
        /// <remarks>
        ///   Default = 5512
        /// </remarks>
        public int SampleRate { get; private set; }

        /// <summary>
        ///   Gets log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        /// <remarks>
        ///   Default = 10
        /// </remarks>
        public double LogBase { get; private set; }

        /// <summary>
        /// Gets number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
        /// </summary>
        public int LogBins { get; private set; }

        /// <summary>
        ///   Gets signature's length
        /// </summary>
        public int FingerprintLength { get; private set; }

        /// <summary>
        /// Gets default stride size between 2 consecutive signature
        /// </summary>
        /// <remarks>
        ///  Default = 5115
        /// </remarks>
        public IStride Stride { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the algorithm has to normalize the signal
        /// </summary>
        public bool NormalizeSignal { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the algorithm should use dynamic logarithmic base, instead of static
        /// </summary>
        public bool UseDynamicLogBase { get; private set; }

        /// <summary>
        /// Gets the number of LSH tables to split
        /// </summary>
        public int NumberOfLSHTables { get; private set; }

        /// <summary>
        /// Gets the number of Min Hashes per table
        /// </summary>
        public int NumberOfMinHashesPerTable { get; private set; }
    }
}
