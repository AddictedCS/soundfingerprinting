namespace SoundFingerprinting.Image
{
    using System;
    
    public class ImageService : IImageService
    {
        public static ImageService Instance { get; } = new ImageService();

        public float[] Image2RowCols(float[][] image)
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

        public float[][] RowCols2Image(float[] image, int rows, int cols)
        {
            float[][] transformed = new float[rows][];
            for (int i = 0; i < rows; ++i)
            {
                transformed[i] = new float[cols];
                Buffer.BlockCopy(image, i * cols * sizeof(float), transformed[i], 0, cols * sizeof(float));
            }

            return transformed;
        }
        
        public byte[][] FloatsToByteImage(float[][] image, int max)
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
        
        public float[][] BytesToFloatImage(byte[][] array, int max)
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
    }
}
