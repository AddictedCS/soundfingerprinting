namespace SoundFingerprinting.Strides
{
    using System;

    /// <summary>
    ///   Random stride class
    /// </summary>
    public class RandomStride : IStride
    {
        protected static readonly Random Random = new Random(unchecked((int)DateTime.Now.Ticks));

        /// <summary>
        ///   Initializes a new instance of the <see cref="RandomStride"/> class. 
        /// </summary>
        /// <param name="minStride">
        ///   Inclusive min value used for generating a random stride
        /// </param>
        /// <param name="maxStride">
        ///   Exclusive max value used for generating a random stride
        /// </param>
        public RandomStride(int minStride, int maxStride)
        {
            if (minStride > maxStride)
            {
                throw new ArgumentException("Bad arguments. Max stride should be bigger than Min stride");
            }

            Min = minStride;
            Max = maxStride;
            FirstStride = Random.Next(minStride, maxStride);
        }

        public RandomStride(int minStride, int maxStride, int firstStride)
            : this(minStride, maxStride)
        {
            FirstStride = firstStride;
        }

        public int Min { get; }

        public int Max { get; }

        public int FirstStride { get; }

        public virtual int GetNextStride()
        {
            return Random.Next(Min, Max);
        }

        public override string ToString()
        {
            return $"RandomStride{Min}-{Max}";
        }
    }
}