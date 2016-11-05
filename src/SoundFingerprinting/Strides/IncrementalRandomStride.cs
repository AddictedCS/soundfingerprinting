namespace SoundFingerprinting.Strides
{
    public class IncrementalRandomStride : RandomStride
    {
        private readonly int samplesPerFingerprint;
         
        public IncrementalRandomStride(int minStride, int maxStride, int samplesPerFingerprint)
            : base(minStride, maxStride)
        {
            this.samplesPerFingerprint = samplesPerFingerprint;
            FirstStride = Random.Next(minStride, maxStride);
        }

        public IncrementalRandomStride(int minStride, int maxStride, int samplesPerFingerprint, int firstStride)
            : this(minStride, maxStride, samplesPerFingerprint)
        {
            FirstStride = firstStride;
        }

        public override int GetNextStride()
        {
            return -samplesPerFingerprint + Random.Next(Min, Max);
        }

        public override string ToString()
        {
            return string.Format("IncrementalRandom-{0}-{1}", Min, Max);
        }
    }
}