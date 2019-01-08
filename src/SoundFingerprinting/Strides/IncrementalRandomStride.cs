namespace SoundFingerprinting.Strides
{
    using System;

    /// <inheritdoc />
    /// <summary>
    ///   Incremental random stride used in providing step length (measured in number of audio samples) between 2 consecutive fingerprints
    /// </summary>
    public class IncrementalRandomStride : IStride
    {
        private readonly object lockObject = new object();
        private readonly Random random;
        
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

        public int FirstStride { get; } = 0;

        public int NextStride
        {
            get
            {
                lock (lockObject)
                {
                    return random.Next(Min, Max);
                }
            }
        }

        public override string ToString()
        {
            return $"IncrementalRandomStride[{Min}-{Max})";
        }
    }
}