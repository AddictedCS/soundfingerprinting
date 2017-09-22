namespace SoundFingerprinting.Utils
{
    internal class FastFingerprintDescriptor : FingerprintDescriptor
    {
        private readonly QuickSelectAlgorithm quickSelect = new QuickSelectAlgorithm();

        public override bool[] ExtractTopWavelets(float[][] frames, int topWavelets)
        {
            float[] concatenated = ConcatenateFrames(frames);
            int[] indexes = GetRange(concatenated.Length); 
            quickSelect.Find(topWavelets - 1, concatenated, indexes, 0, concatenated.Length - 1);
            bool[] result = EncodeFingerprint(concatenated, indexes, topWavelets);
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
