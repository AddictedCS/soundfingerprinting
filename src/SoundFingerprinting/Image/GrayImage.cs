namespace SoundFingerprinting.Image
{
    using System;
    using System.Threading.Tasks;

    public class GrayImage
    {
        private readonly float[][] image;

        public GrayImage(byte[][] image)
        {
            this.image = new float[image.Length][];
            for (int i = 0; i < image.Length; ++i)
            {
                this.image[i] = new float[image[0].Length];
                for(int j = 0; j < image[0].Length; ++j)
                {
                    this.image[i][j] = image[i][j];
                }
            }
        }

        private GrayImage(float[][] image)
        {
            this.image = image;
        }

        public int Width => image[0].Length;

        public int Height => image.Length;

        public float this[int i, int j] => image[i][j];

        public GrayImage Multiply(GrayImage other)
        {
            AssertSize(other);
            var result = NewImage(Width, Height);
            Parallel.For(0, Height, i =>
            {
                for (int j = 0; j < Width; ++j)
                {
                    result[i][j] = this[i, j] * other[i, j];
                }
            });

            return new GrayImage(result);
        }

        public GrayImage Divide(GrayImage other)
        {
            AssertSize(other);
            var result = NewImage(Width, Height);
            Parallel.For(0, Height, i =>
            {
                for (int j = 0; j < Width; ++j)
                {
                    result[i][j] = this[i, j] / other[i, j];
                }
            });

            return new GrayImage(result);
        }

        public GrayImage Subtract(GrayImage other)
        {
            AssertSize(other);
            var result = NewImage(Width, Height);
            Parallel.For(0, Height, i =>
            {
                for (int j = 0; j < Width; ++j)
                {
                    result[i][j] = this[i, j] - other[i, j];
                }
            });

            return new GrayImage(result);
        }

        public GrayImage Add(GrayImage other)
        {
            AssertSize(other);
            var result = NewImage(Width, Height);
            Parallel.For(0, Height, i =>
            {
                for (int j = 0; j < Width; ++j)
                {
                    result[i][j] = this[i, j] + other[i, j];
                }
            });

            return new GrayImage(result);
        }

        public GrayImage Convert(Func<float, float> functor)
        {
            var result = NewImage(Width, Height);
            Parallel.For(0, Height, i =>
            {
                for (int j = 0; j < Width; ++j)
                {
                    result[i][j] = functor(this[i, j]);
                }
            });

            return new GrayImage(result);
        }

        public T[][] ConvertAndUnwrap<T>(Func<float, T> functor)
        {
            var result = new T[Height][];
            Parallel.For(0, Height, i =>
            {
                result[i] = new T[Width];
                for (int j = 0; j < Width; ++j)
                {
                    result[i][j] = functor(this[i, j]);
                }
            });

            return result;
        }

        public GrayImage GaussianBlur(int size, double sigma)
        {
            var kernel = GaussianBlurKernel.Kernel2D(size, sigma);
            return new GrayImage(Convolve(image, kernel));
        }

        public GrayImage GaussianBlur(double[,] kernel2d)
        {
            return new GrayImage(Convolve(image, kernel2d));
        }

        private static float[][] NewImage(int width, int height)
        {
            var x = new float[height][];
            for (int i = 0; i < height; ++i)
            {
                x[i] = new float[width];
            }

            return x;
        }

        private void AssertSize(GrayImage other)
        {
            if (other.Width != Width)
                throw new ArgumentException(nameof(other.Width));
            if (other.Height != Height)
                throw new ArgumentException(nameof(other.Height));
        }

        private static float[][] Convolve(float[][] image, double[,] kernel)
        {
            int width = image[0].Length;
            int height = image.Length;
            float[] buffer = new float[width * height];
            float[][] result = NewImage(width, height);
            for (int i = 0; i < height; ++i)
            {
                Buffer.BlockCopy(image[i], 0, buffer, i * width * sizeof(float), width * sizeof(float));
            }

            int foff = (kernel.GetLength(0) - 1) / 2;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double rgb = 0.0;

                    int kcenter = y * width + x;
                    for (int fy = -foff; fy <= foff; fy++)
                    {
                        for (int fx = -foff; fx <= foff; fx++)
                        {
                            int kpixel = kcenter + fy * width + fx;
                            if (kpixel < 0)
                                kpixel = 0;
                            if (kpixel >= width * height)
                                kpixel = width * height - 1;
                            rgb += buffer[kpixel] * kernel[fy + foff, fx + foff];
                        }
                    }

                    result[y][x] = (float)rgb;
                }
            }

            return result;
        }
    }
}