namespace SoundFingerprinting.Strides
{
    using System;

    /// <summary>
    ///   Random stride class
    /// </summary>
    public class RandomStride : IStride
    {
        private readonly object lockObject = new object();
        private const int DefaultSamplesPerFingerprint = 128 * 64;
        private readonly Random random;

        /// <summary>
        ///   Initializes a new instance of the <see cref="RandomStride"/> class. 
        /// </summary>
        /// <param name="minStride">
        ///   Inclusive min value used for generating a random stride
        /// </param>
        /// <param name="maxStride">
        ///   Exclusive max value used for generating a random stride
        /// </param>
        /// <param name="seed">
        ///   Seed used when generating next random stride. Don't specify if you want to use program generated seed.
        /// </param>
        public RandomStride(int minStride, int maxStride, int seed)
        {
            if (minStride > maxStride)
            {
                throw new ArgumentException("Bad arguments. Max stride should be bigger than Min stride");
            }

            Min = minStride;
            Max = maxStride;
            random = seed == 0 ? new Random() : new Random(seed);
            FirstStride = 0;
        }

        private int Min { get; }

        private int Max { get; }

        public int FirstStride { get; }

        public virtual int NextStride
        {
            get
            {
                lock (lockObject)
                {
                    return DefaultSamplesPerFingerprint + random.Next(Min, Max);
                }
            }
        }

        public override string ToString()
        {
            return $"RandomStride{Min}-{Max}";
        }
    }
}