namespace SoundFingerprinting.Utils
{
    internal class FastFingerprintDescriptor : FingerprintDescriptor
    {
        private readonly QuickSelectAlgorithm quickSelect = new QuickSelectAlgorithm();

        public override bool[] ExtractTopWavelets(float[] frames, int topWavelets)
        {
            int[] indexes = GetRange(frames.Length); 
            quickSelect.Find(topWavelets - 1, frames, indexes, 0, frames.Length - 1);
            bool[] result = EncodeFingerprint(frames, indexes, topWavelets);
            return result;
        }

        private int[] GetRange(int till)
        {
            int[] indexes = new int[till];
            for (int i = 0; i < till; ++i)
            {
                indexes[i] = i;
            }

            return indexes;
        }
    }
}
