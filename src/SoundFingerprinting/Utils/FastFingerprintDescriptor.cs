namespace SoundFingerprinting.Utils
{
    internal class FastFingerprintDescriptor : FingerprintDescriptor
    {
        public override IEncodedFingerprintSchema ExtractTopWavelets(float[] frames, int topWavelets, ushort[] indexes)
        {
            QuickSelectAlgorithm.Find(topWavelets - 1, frames, indexes, 0, frames.Length - 1);
            return EncodeFingerprint(frames, topWavelets, indexes);
        }
    }
}
