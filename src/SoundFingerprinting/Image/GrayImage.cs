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
            float[][] result = NewImage(width, height);
            int foff = (kernel.GetLength(0) - 1) / 2;
            
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    double rgb = 0.0;
                    for (int fy = -foff; fy <= foff; fy++)
                    {
                        for (int fx = -foff; fx <= foff; fx++)
                        {
                            (int kx, int ky) = (x + fx, y + fy);
                            kx = kx < 0 ? 0 : kx > width - 1 ? width - 1 : kx;
                            ky = ky < 0 ? 0 : ky > height - 1 ? height - 1 : ky;
                            rgb += image[ky][kx] * kernel[fy + foff, fx + foff];
                        }
                    }

                    result[y][x] = (float) rgb;
                }
            });

            return result;
        }
    }
}