namespace SoundFingerprinting.Image
{
    using System;
    public class ImageService : IImageService
    {
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
    }
}
