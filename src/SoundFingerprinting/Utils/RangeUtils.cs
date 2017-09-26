namespace SoundFingerprinting.Utils
{
    internal static class RangeUtils
    {
        public static ushort[] GetRange(int till)
        {
            ushort[] indexes = new ushort[till];
            PopulateIndexes(till, indexes);
            return indexes;
        }

        public static void PopulateIndexes(int till, ushort[] indexes)
        {
            for (ushort i = 0; i < till; ++i)
            {
                indexes[i] = i;
            }
        }
    }
}
