namespace SoundFingerprinting.Utils
{
    public static class RangeUtils
    {
        public static ushort[] GetRange(int till)
        {
            ushort[] indexes = new ushort[till];
            for (ushort i = 0; i < till; ++i)
            {
                indexes[i] = i;
            }

            return indexes;
        }
    }
}
