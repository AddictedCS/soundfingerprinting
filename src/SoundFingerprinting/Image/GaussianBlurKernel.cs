namespace SoundFingerprinting.Image
{
    using System;

    public class GaussianBlurKernel
    {
        public static double[,] Kernel2D(int size, double sigma)
        {
            if (size % 2 == 0 || size < 3 || size > 101)
                throw new ArgumentException("Wrong kernel size.");
            int half = size / 2;
            double[,] numArray = new double[size, size];
            int y = -half;
            for (int i = 0; i < size; ++i)
            {
                int x = -half;
                for (int j = 0; j < size; ++j)
                {
                    numArray[i, j] = Function2D(x, y, sigma);
                    ++x;
                }
                ++y;
            }

            return numArray;
        }

        private static double Function2D(double x, double y,  double sigma)
        {
            return Math.Exp((x * x + y * y) / (-2.0 * sigma * sigma)) / (2.0 * Math.PI * sigma * sigma);
        }
    }
}