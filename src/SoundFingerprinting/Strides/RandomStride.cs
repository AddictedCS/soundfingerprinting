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

        public int Min { get; private set; }

        public int Max { get; private set; }

        public int FirstStride { get; private set; }

        public virtual int NextStride
        {
            get
            {
                return Random.Next(Min, Max);
            }
        }

        public override string ToString()
        {
            return string.Format("RandomStride{0}-{1}", Min, Max);
        }
    }
}