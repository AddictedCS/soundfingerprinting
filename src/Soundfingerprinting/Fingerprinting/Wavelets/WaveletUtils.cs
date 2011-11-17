// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Linq;

namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    public static class WaveletUtils
    {
        public static void WaveletNoiseHardThresholding(float[][] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                int len = array[0].Length;
                double median = (len%2 == 1) ? array[i][len/2 + 1] : array[i][len/2];
                double[] copy = new double[len];
                Array.Copy(array[i], copy, len);
                for (int j = 0; j < len; j++)
                {
                    copy[j] = Math.Abs(copy[j] - median);
                }
                double mad = (len%2 == 1) ? copy[len/2 + 1] : copy[len/2];
                double t = mad*Math.Sqrt(Math.Log(len))/0.6745;
                for (int j = 0; j < len; j++)
                {
                    if (array[i][j] < t)
                        array[i][j] = 0;
                }
            }
        }

        public static void WaveletNoiseHardThresholding2(double[][] array)
        {
            int len = array[0].Length;
            int elements = array.GetLength(0);
            double average = array.Sum((a) => a.Sum())/(len*elements);
            double sum = array.Sum((a) => a.Sum((var) => Math.Abs(var - average)));
            double mean = sum/(len*elements);
            double t = mean*Math.Sqrt(2*Math.Log(len));

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < len; j++)
                {
                    if (array[i][j] < t)
                        array[i][j] = 0;
                }
            }
        }
    }
}