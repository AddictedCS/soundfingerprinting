namespace Soundfingerprinting.Fingerprinting.Configuration
{
    using Soundfingerprinting.AudioProxies.Strides;

    public class DefaultFingerprintingConfig : IFingerprintingConfig
    {
       
        public DefaultFingerprintingConfig()
        {
            FingerprintLength = 128;
            Overlap = 64;
            SamplesPerFingerprint = FingerprintLength * overlap;
            WdftSize = 2048;
            MinFrequency = 318;
            MaxFrequency = 2000;
            TopWavelets = 200;
            SamplesPerFingerprint = 5512;
            LogBase = 2;
            Stride = new StaticStride(5115);
            LogBins = 32;
        }

        /// <summary>
        ///   Gets number of samples to read in order to create single fingerprint. The granularity is 1.48 seconds
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
        ///   Gets frequency range which is taken into account when creating the fingerprint
        /// </summary>
        /// <remarks>
        ///   Default = 318
        /// </remarks>
        public int MinFrequency { get; private set; }

        /// <summary>
        ///   Gets frequency range which is taken into account when creating the fingerprint
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
        ///   Gets fingerprint's length
        /// </summary>
        public int FingerprintLength { get; private set; }

        /// <summary>
        /// Gets default stride size between 2 consecutive fingerprint
        /// </summary>
        /// <remarks>
        ///  Default = 5115
        /// </remarks>
        public IStride Stride { get; private set; }
    }
}