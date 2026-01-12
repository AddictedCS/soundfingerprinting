namespace SoundFingerprinting.Math
{
    using System;

    internal class HashConverter : IHashConverter
    {
        public static IHashConverter Instance { get; } = new HashConverter();

        public byte[] ToBytes(long[] array, int count)
        {
            int bytesPerLong = GetBytesPerLong(count, array.Length);
            byte[] bytes = new byte[count];
            
            for (int i = 0; i < array.Length; i++)
            {
                long value = array[i];
                int destIndex = i * bytesPerLong;
                
                // Write bytes directly in little-endian order without BitConverter allocation
                for (int j = 0; j < bytesPerLong; j++)
                {
                    bytes[destIndex + j] = (byte)(value >> (j * 8));
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
                grouped[i] = ReadLongLittleEndian(array, startIndex, bytesPerLong);
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
                
                if (bytesPerLong <= 4)
                {
                    // Direct read for 1-4 bytes
                    grouped[i] = ReadIntLittleEndian(array, startIndex, bytesPerLong);
                }
                else
                {
                    // For 5-8 bytes, XOR the low and high parts to fit into int
                    int value1 = ReadIntLittleEndian(array, startIndex, 4);
                    int value2 = ReadIntLittleEndian(array, startIndex + 4, bytesPerLong - 4);
                    grouped[i] = value1 ^ value2;
                }
            }

            return grouped;
        }

        /// <summary>
        /// Reads 1-8 bytes from array as a little-endian long.
        /// </summary>
        private static long ReadLongLittleEndian(byte[] array, int startIndex, int byteCount)
        {
            // Unrolled for common cases to avoid branching in hot path
            switch (byteCount)
            {
                case 1:
                    return array[startIndex];
                case 2:
                    return (uint)(array[startIndex] | (array[startIndex + 1] << 8));
                case 3:
                    return (uint)(array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16));
                case 4:
                    return (uint)(array[startIndex] | (array[startIndex + 1] << 8) | 
                           (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24));
                case 5:
                    return (uint)(array[startIndex] | (array[startIndex + 1] << 8) | 
                           (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24)) |
                           ((long)array[startIndex + 4] << 32);
                case 6:
                    return (uint)(array[startIndex] | (array[startIndex + 1] << 8) | 
                           (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24)) |
                           ((long)(array[startIndex + 4] | (array[startIndex + 5] << 8)) << 32);
                case 7:
                    return (uint)(array[startIndex] | (array[startIndex + 1] << 8) | 
                           (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24)) |
                           ((long)(array[startIndex + 4] | (array[startIndex + 5] << 8) | 
                           (array[startIndex + 6] << 16)) << 32);
                case 8:
                    return (uint)(array[startIndex] | (array[startIndex + 1] << 8) | 
                           (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24)) |
                           ((long)(uint)(array[startIndex + 4] | (array[startIndex + 5] << 8) | 
                           (array[startIndex + 6] << 16) | (array[startIndex + 7] << 24)) << 32);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Reads 1-4 bytes from array as a little-endian int.
        /// </summary>
        private static int ReadIntLittleEndian(byte[] array, int startIndex, int byteCount)
        {
            switch (byteCount)
            {
                case 1:
                    return array[startIndex];
                case 2:
                    return array[startIndex] | (array[startIndex + 1] << 8);
                case 3:
                    return array[startIndex] | (array[startIndex + 1] << 8) | (array[startIndex + 2] << 16);
                case 4:
                    return array[startIndex] | (array[startIndex + 1] << 8) | 
                           (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24);
                default:
                    return 0;
            }
        }

        private static int GetBytesPerLong(int bytesArrayCount, int longsArrayCount)
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
