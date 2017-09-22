namespace SoundFingerprinting.Utils
{
    using System.Linq;

    internal class FastFingerprintDescriptor : FingerprintDescriptor
    {
        private readonly QuickSelectAlgorithm quickSelect = new QuickSelectAlgorithm();

        public override bool[] ExtractTopWavelets(float[][] frames, int topWavelets)
        {
            float[] concatenated = ConcatenateFrames(frames);
            int[] indexes = Enumerable.Range(0, concatenated.Length).ToArray();
            quickSelect.Find(topWavelets - 1, concatenated, indexes, 0, concatenated.Length - 1);
            bool[] result = EncodeFingerprint(concatenated, indexes, topWavelets);
            return result;
        }
    }
}
