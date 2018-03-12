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
            int bytesPerLong = GetBytesPerLong(array.Length, count);
            long[] grouped = new long[count];
            for (int i = 0; i < count; i++)
            {
                int startIndex = i * bytesPerLong;
                if (bytesPerLong == 1)
                {
                    grouped[i] = array[startIndex];
                }
                if (bytesPerLong == 2)
                {
                    grouped[i] = BitConverter.ToInt16(array, startIndex);
                }
                else if (bytesPerLong == 3)
                {
                    grouped[i] = array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16); 
                }
                else if (bytesPerLong == 4)
                {
                    grouped[i] = BitConverter.ToInt32(array, startIndex);
                }
                else if (bytesPerLong == 5)
                {
                    int value1 = array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16)
                                 | (array[startIndex + 3] << 24);
                    int value2 = array[startIndex + 4];
                    grouped[i] = (uint)value1 | ((long)value2 << 32);
                }
                else if (bytesPerLong == 6)
                {
                    int value1 = array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16)
                                 | (array[startIndex + 3] << 24);
                    int value2 = array[startIndex + 4] | (array[startIndex + 5] << 8);
                    grouped[i] = (uint)value1 | ((long)value2 << 32);
                }
                else if (bytesPerLong == 7)
                {
                    int value1 = array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16)
                                 | (array[startIndex + 3] << 24);
                    int value2 = array[startIndex + 4] | (array[startIndex + 5] << 8) | (array[startIndex + 6] << 16);
                    grouped[i] = (uint)value1 | ((long)value2 << 32);
                }
                else if (bytesPerLong == 8)
                {
                    grouped[i] = BitConverter.ToInt64(array, startIndex);
                }
            }

            return grouped;
        }

        public int[] ToInts(byte[] array, int count)
        {
            int bytesPerLong = GetBytesPerLong(array.Length, count);
            int[] grouped = new int[count];
            for (int i = 0; i < count; i++)
            {
                int startIndex = i * bytesPerLong;
                if (bytesPerLong == 1)
                {
                    grouped[i] = array[startIndex];
                }
                if (bytesPerLong == 2)
                {
                    grouped[i] = BitConverter.ToInt16(array, startIndex);
                }
                else if (bytesPerLong == 3)
                {
                    grouped[i] = array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16); 
                }
                else if (bytesPerLong == 4)
                {
                    grouped[i] = BitConverter.ToInt32(array, startIndex);
                }
                else if (bytesPerLong == 5)
                {
                    int value1 = array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16)
                                 | (array[startIndex + 3] << 24);
                    int value2 = array[startIndex + 4];
                    grouped[i] = value1 ^ value2;
                }
                else if (bytesPerLong == 6)
                {
                    int value1 = array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16)
                                 | (array[startIndex + 3] << 24);
                    int value2 = array[startIndex + 4] | (array[startIndex + 5] << 8);
                    grouped[i] = value1 ^ value2;
                }
                else if (bytesPerLong == 7)
                {
                    int value1 = array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16)
                                 | (array[startIndex + 3] << 24);
                    int value2 = array[startIndex + 4] | (array[startIndex + 5] << 8) | (array[startIndex + 6] << 16);
                    grouped[i] = value1 ^ value2;
                }
                else if (bytesPerLong == 8)
                {
                    grouped[i] = (int)BitConverter.ToInt64(array, startIndex);
                }
            }

            return grouped;
        }


        private int GetBytesPerLong(int bytesArrayCount, int longsArrayCount)
        {
            int bytesPerLong = bytesArrayCount / longsArrayCount;
            if (bytesPerLong > 8)
            {
                throw new ArgumentException("count for longs array is not in accepted range. Max number of bytes per one long is 8");
            }

            return bytesPerLong;
        }
    }
}
