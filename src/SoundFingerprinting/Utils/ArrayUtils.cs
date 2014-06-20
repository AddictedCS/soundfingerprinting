namespace SoundFingerprinting.Utils
{
    using System;

    public static class ArrayUtils
    {
        public static float[] ConcatenateDoubleDimensionalArray(float[][] array)
        {
            int rows = array.GetLength(0);
            int cols = array[0].Length;
            float[] concatenated = new float[rows * cols];
            for (int row = 0; row < rows; row++)
            {
                Buffer.BlockCopy(array[row], 0, concatenated, row * array[row].Length * 4, array[row].Length * 4);
            }

            return concatenated;
        }
    }
}
