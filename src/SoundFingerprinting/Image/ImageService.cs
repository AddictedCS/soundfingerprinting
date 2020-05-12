namespace SoundFingerprinting.Image
{
    using System;
    
    public static class ImageService
    {
        public static float[] Image2RowCols(float[][] image)
        {
            int width = image[0].Length;
            int height = image.Length;
            float[] transformed = new float[width * height];
            for (int row = 0; row < height /*rows*/; ++row)
            {
                Buffer.BlockCopy(image[row], 0, transformed, row * width * sizeof(float), width * sizeof(float));
            }

            return transformed;
        }

        public static float[][] RowCols2Image(float[] image, int rows, int cols)
        {
            float[][] transformed = new float[rows][];
            for (int i = 0; i < rows; ++i)
            {
                transformed[i] = new float[cols];
                Buffer.BlockCopy(image, i * cols * sizeof(float), transformed[i], 0, cols * sizeof(float));
            }

            return transformed;
        }
        
        public static byte[][] FloatsToByteImage(float[][] image, int max)
        {
            byte[][] q = new byte[image.Length][];
            for (int i = 0; i < q.Length; ++i)
            {
                q[i] = new byte[image[0].Length];
                for (int j = 0; j < image[0].Length; ++j)
                {
                    q[i][j] = (byte)Math.Min(max, image[i][j] * max);
                }
            }

            return q;
        }
        
        public static float[][] BytesToFloatImage(byte[][] array, int max)
        {
            float[][] floats = new float[array.Length][];
            for(int i = 0; i < array.Length; ++i)
            {
                floats[i] = new float[array[i].Length];
                for(int j = 0; j < array[i].Length; ++j)
                {
                    floats[i][j] = (float)array[i][j] / max;
                }
            }

            return floats;
        }
        
        /// <summary>
        ///  Threshold image. Pixels that are bigger than threshold (i.e. 235) will be set to 0, otherwise set to max value
        /// </summary>
        /// <param name="image">Image to threshold</param>
        /// <param name="threshold">Threshold value</param>
        /// <param name="maxValue">Max value set on pixels that are less or equal to threshold</param>
        public static void ThresholdInvInPlace(byte[][] image, int threshold, byte maxValue)
        {
            foreach (byte[] row in image)
            {
                for (int j = 0; j < row.Length; ++j)
                {
                    row[j] = row[j] > threshold ? byte.MinValue : maxValue;
                }
            }
        }
    }
}
