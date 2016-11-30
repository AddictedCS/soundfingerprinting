﻿namespace SoundFingerprinting.Strides
{
    using System;

    /// <summary>
    ///   Incremental random stride used in providing step length (measured in number of audio samples) between 2 consecutive fingerprints
    /// </summary>
    public class IncrementalRandomStride : RandomStride
    {
        private const int SamplesPerFingerprint = 128 * 64; // 8192 samples per one fingerprint

        /// <summary>
        ///   Initializes a new instance of the <see cref="IncrementalRandomStride"/> class. 
        /// </summary>
        /// <example>
        ///   new IncrementalRandomStride(256, 512)
        /// </example>
        /// <param name="min">
        ///   Inclusive min value used for generating a random stride
        /// </param>
        /// <param name="max">
        ///   Exclusive max value used for generating a random stride
        /// </param>
        public IncrementalRandomStride(int min, int max) : base(min, max)
        {
        }

        internal IncrementalRandomStride(int minStride, int maxStride, int firstStride)
            : base(minStride, maxStride, firstStride)
        {
        }

        public override int NextStride
        {
            get
            {
                return -SamplesPerFingerprint + Random.Next(Min, Max);
            }
        }

        public override string ToString()
        {
            return string.Format("IncrementalRandomStride[{0}-{1})", Min, Max);
        }
    }
}