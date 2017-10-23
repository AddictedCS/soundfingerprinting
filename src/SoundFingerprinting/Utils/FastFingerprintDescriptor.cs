namespace SoundFingerprinting.Utils
{
    internal class FastFingerprintDescriptor : FingerprintDescriptor
    {
        private readonly QuickSelectAlgorithm quickSelect = new QuickSelectAlgorithm();

        public override IEncodedFingerprintSchema ExtractTopWavelets(float[] frames, int topWavelets, ushort[] indexes)
        {
            quickSelect.Find(topWavelets - 1, frames, indexes, 0, frames.Length - 1);
            return EncodeFingerprint(frames, topWavelets, indexes);
        }
    }
}
