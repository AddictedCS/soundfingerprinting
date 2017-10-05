namespace SoundFingerprinting.Utils
{
    using System;

        internal class FingerprintDescriptor : IFingerprintDescriptor
    {
        private readonly AbsComparator absComparator;
        private readonly IFingerprintEncoder fingerprintEncoder;

        public FingerprintDescriptor()
        {
            absComparator = new AbsComparator();
            fingerprintEncoder = new FingerprintEncoder();
        }

        public virtual IEncodedFingerprintSchema ExtractTopWavelets(float[] frames, int topWavelets, ushort[] indexes)
        {
            Array.Sort(frames, indexes, absComparator);
            return EncodeFingerprint(frames, topWavelets, indexes);
        }

        protected IEncodedFingerprintSchema EncodeFingerprint(float[] frames, int topWavelets, ushort[] indexes)
        {
            return fingerprintEncoder.EncodeFingerprint(frames, indexes, topWavelets);
        }
    }
}