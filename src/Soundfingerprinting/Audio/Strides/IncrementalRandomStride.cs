namespace Soundfingerprinting.Audio.Strides
{
    using System;

    /// <summary>
    ///   Incremental random stride
    /// </summary>
    public class IncrementalRandomStride : IStride
    {
        /// <summary>
        ///   Random stride
        /// </summary>
        private static readonly Random Random = new Random(unchecked((int)DateTime.Now.Ticks));

        private readonly int firstStride;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalRandomStride"/> class. 
        /// </summary>
        /// <param name="min">
        /// Min step in samples
        /// </param>
        /// <param name="max">
        /// Max step in samples
        /// </param>
        /// <param name="samplesPerFingerprint">
        /// Samples per signature
        /// </param>
        public IncrementalRandomStride(int min, int max, int samplesPerFingerprint)
        {
            Min = min;
            Max = max;
            SamplesPerFingerprint = samplesPerFingerprint;
            firstStride = Random.Next(min, max);
        }

        public IncrementalRandomStride(int min, int max, int samplesPerFingerprint, int firstStride)
            : this(min, max, samplesPerFingerprint)
        {
            this.firstStride = firstStride;
        }

        /// <summary>
        ///  Gets or sets minimal step between consecutive strides
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        ///   Gets or sets maximal step between consecutive strides
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        ///   Gets or sets number of samples per signature
        /// </summary>
        public int SamplesPerFingerprint { get; set; }

        public int StrideSize
        {
            get
            {
                return -SamplesPerFingerprint + Random.Next(Min, Max);
            }
        }

        public int FirstStrideSize
        {
            get
            {
                return firstStride;
            }
        }
    }
}