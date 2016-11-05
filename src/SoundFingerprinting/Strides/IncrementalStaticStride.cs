namespace SoundFingerprinting.Strides
{
    using System;

    public class IncrementalStaticStride : StaticStride
    {
        private readonly int incrementBy;

        public IncrementalStaticStride(int incrementBy, int samplesPerFingerprint)
            : base(-samplesPerFingerprint + incrementBy) /*Negative stride will guarantee that the signal is incremented by the parameter specified*/
        {
            if (incrementBy <= 0)
            {
                throw new ArgumentException("Bad parameter. IncrementBy should be strictly bigger than 0");
            }

            this.incrementBy = incrementBy;
        }

        public IncrementalStaticStride(int incrementBy, int samplesPerFingerprint, int firstStride)
            : this(incrementBy, samplesPerFingerprint)
        {
            FirstStride = firstStride;
        }

        public override string ToString()
        {
            return string.Format("IncrementalStatic-{0}", incrementBy);
        }
    }
}