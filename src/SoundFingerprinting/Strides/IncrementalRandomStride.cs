namespace SoundFingerprinting.Strides
{
    using System;

    /// <summary>
    ///   Incremental random stride used in providing step length (measured in number of audio samples) between 2 consecutive fingerprints.
    /// </summary>
    public class IncrementalRandomStride : IStride
    {
        private readonly Random random;
        
        /// <summary>
        ///  Creates new instance of <see cref="IncrementalRandomStride"/>.
        /// </summary>
        /// <param name="minStride">Size of the minimum stride measured in audio samples, used between two consecutive fingerprints.</param>
        /// <param name="maxStride">Size of the maximum stride measured in audio samples, used between two consecutive fingerprints.</param>
        /// <param name="seed">Seed used stride the track. If set to zero, random seed will be used.</param>
        /// <exception cref="ArgumentException">MinStride should always be less or equal to MaxStride.</exception>
        public IncrementalRandomStride(int minStride, int maxStride, int seed = 0) 
        {
            if (minStride > maxStride)
            {
                throw new ArgumentException("Bad arguments. Max stride should be bigger than Min stride");
            }

            Min = minStride;
            Max = maxStride;
            random = seed == 0 ? new Random() : new Random(seed);
        }

        private int Min { get; }

        private int Max { get; }

        /// <inheritdoc cref="Strides.IStride.FirstStride"/>.
        public int FirstStride { get; } = 0;

        /// <inheritdoc cref="Strides.IStride.NextStride"/>.
        public int NextStride => random.Next(Min, Max);

        /// <inheritdoc cref="object.ToString"/>.
        public override string ToString()
        {
            return $"IncrementalRandomStride[{Min}-{Max})";
        }
    }
}