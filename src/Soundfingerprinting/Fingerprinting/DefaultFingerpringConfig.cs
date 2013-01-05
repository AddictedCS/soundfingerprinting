namespace Soundfingerprinting.Fingerprinting
{
    using Soundfingerprinting.AudioProxies.Strides;

    public class DefaultFingerpringConfig : IFingerprintConfig
    {
        public DefaultFingerpringConfig()
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
            Stride = new StaticStride(5115);
        }

        /// <summary>
        ///   Gets or sets number of samples to read in order to create single fingerprint. The granularity is 1.48 seconds
        /// </summary>
        /// <remarks>
        ///   Default = 8192
        /// </remarks>
        public int SamplesPerFingerprint { get;  set; }

        /// <summary>
        ///   Gets or sets overlap between the sub fingerprints, 11.6 ms
        /// </summary>
        /// <remarks>
        ///   Default = 64
        /// </remarks>
        public int Overlap { get;  set; }

        /// <summary>
        ///   Gets or sets size of the WDFT block, 371 ms
        /// </summary>
        /// <remarks>
        ///   Default = 2048
        /// </remarks>
        public int WdftSize { get;  set; }

        /// <summary>
        ///   Gets or sets frequency range which is taken into account when creating the fingerprint
        /// </summary>
        /// <remarks>
        ///   Default = 318
        /// </remarks>
        public int MinFrequency { get;  set; }

        /// <summary>
        ///   Gets or sets frequency range which is taken into account when creating the fingerprint
        /// </summary>
        /// <remarks>
        ///   Default = 2000
        /// </remarks>
        public int MaxFrequency { get; set; }

        /// <summary>
        ///   Gets or sets number of Top wavelets to consider
        /// </summary>
        /// <remarks>
        ///   Default = 200
        /// </remarks>
        public int TopWavelets { get; set; }

        /// <summary>
        ///   Gets or sets sample rate at which the audio file will be pre-processed
        /// </summary>
        /// <remarks>
        ///   Default = 5512
        /// </remarks>
        public int SampleRate { get; set; }

        /// <summary>
        ///   Gets or sets log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        /// <remarks>
        ///   Default = 10
        /// </remarks>
        public double LogBase { get; set; }

        /// <summary>
        ///   Gets or sets fingerprint's length
        /// </summary>
        public int FingerprintLength { get; set; }

        /// <summary>
        /// Gets or sets default stride size between 2 consecutive fingerprint
        /// </summary>
        /// <remarks>
        ///  Default = 5115
        /// </remarks>
        public IStride Stride { get; set; }
    }
}