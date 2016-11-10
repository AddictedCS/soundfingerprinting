namespace SoundFingerprinting.Math
{
    using System;

    internal class HashConverter : IHashConverter
    {
        public byte[] ToBytes(long[] array, int count)
        {
            int bytesPerLong = this.GetBytesPerLong(count, array.Length);
            byte[] bytes = new byte[count];
            for (int i = 0; i < array.Length; i++)
            {
                byte[] converted = BitConverter.GetBytes(array[i]);
                for (int j = 0, k = 0; j < bytesPerLong; ++j, ++k)
                {
                    bytes[j + (i * bytesPerLong)] = converted[k];
                }
            }

            return bytes; 
        }

        public long[] ToLongs(byte[] array, int count)
        {
            int bytesPerLong = this.GetBytesPerLong(array.Length, count);
            long[] grouped = new long[count];
            for (int i = 0; i < count; i++)
            {
                if (bytesPerLong == 2)
                {
                    grouped[i] = BitConverter.ToInt16(array, i * bytesPerLong);
                }
                else if (bytesPerLong == 4)
                {
                    grouped[i] = BitConverter.ToInt32(array, i * bytesPerLong);
                }
                else
                {
                    grouped[i] = BitConverter.ToInt64(array, i * bytesPerLong);
                }
            }

            return grouped;
        }

        private int GetBytesPerLong(int bytesArrayCount, int longsArrayCount)
        {
            int bytesPerLong = bytesArrayCount / longsArrayCount;
            if (bytesPerLong != 2 && bytesPerLong != 4 && bytesPerLong != 8)
            {
                throw new ArgumentException("count for longs array is not in accepted range, since longs cannot be grouped evenly {2, 4, 8}");
            }

            return bytesPerLong;
        }
    }
}
