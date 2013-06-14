namespace SoundFingerprinting.Hashing.Utils
{
    using System;
    using System.Linq;

    public static class SignalUtils
    {
        private const double Epsilon = 0.001;

        public static double CalculateEntropy(int[] array)
        {
            int n = array.Length;
            int total = 0;
            double entropy = 0;
            double p;

            // calculate total amount of hits
            for (int i = 0; i < n; i++)
            {
                total += array[i];
            }

            // for all values
            for (int i = 0; i < n; i++)
            {
                // get item's probability
                p = (double)array[i] / total;

                // calculate entropy
                if (Math.Abs(p - 0) > Epsilon)
                {
                    entropy += -p * Math.Log(p, 2);
                }
            }

            return entropy;
        }

        /// <summary>
        ///   Finds the median of an input vector
        /// </summary>
        /// <param name = "input">Input vector</param>
        /// <returns>Median of the input vector</returns>
        public static double Median(double[] input)
        {
            if (input == null)
                throw new ArgumentException("Input can not be null");
            Array.Sort(input);
            double result;
            int length = input.Length;
            if (length%2 == 0)
            {
                int middle = length/2;
                result = (input[middle] + input[middle - 1])/2;
            }
            else
            {
                result = input[length/2];
            }
            return result;
        }

        /// <summary>
        ///   Find minimal mutual information between two vectors
        /// </summary>
        /// <param name = "a">Vector a</param>
        /// <param name = "b">Vector b</param>
        /// <returns>Minimal mutual information between 2 vectors</returns>
        public static double MutualInformation(double[] a, double[] b)
        {
            const int BucketCount = 256;
            int arrayLength = a.Length;

            Histogram aHistogram = new Histogram(a, BucketCount);
            Histogram bHistogram = new Histogram(b, BucketCount);

            double aSum = aHistogram.DataCount;
            double bSum = bHistogram.DataCount;

            double aMinValue = a.Min();
            double bMinValue = b.Min();
            double aMaxValue = a.Max();
            double bMaxValue = b.Max();

            double[] aArray = new double[BucketCount]; /*Items from histogram A*/
            double[] bArray = new double[BucketCount]; /*Items from histogram B*/

            /*Refine the values of the histogram*/
            for (int i = 0; i < BucketCount; i++)
            {
                aArray[i] = aHistogram[i].Count/aSum;
                bArray[i] = bHistogram[i].Count/bSum;
            }

            /*Scale and round to fit 0, BucketsCount - 1*/
            for (int i = 0; i < arrayLength; i++)
            {
                a[i] = Math.Round((a[i] - aMinValue)*(BucketCount - 1)/(aMaxValue - aMinValue));
                b[i] = Math.Round((b[i] - bMinValue)*(BucketCount - 1)/(bMaxValue - bMinValue));
            }

            double[][] histograms = new double[BucketCount][];

            double sum = 0;
            for (int i = 0; i < BucketCount; i++)
            {
                histograms[i] = new double[BucketCount];
                var indexes = a.Select((value, indexAt) => new {Value = value, IndexAt = indexAt}).Where((item) => item.Value == i);
                int count = indexes.Count();
                double[] items = new double[count];
                int k = 0;
                foreach (var index in indexes)
                    items[k++] = b[index.IndexAt];

                if (count != 0)
                {
                    /*Fuck! I so fucking hate this buggy class [Histogram]*/
                    Histogram hist = new Histogram(items, BucketCount, 0, BucketCount - 1);
                    sum += hist.DataCount;
                    for (int j = 0; j < BucketCount; j++)
                        histograms[i][j] = hist[j].Count;
                }
            }

            /*Refine the values*/
            for (int i = 0; i < BucketCount; i++)
                for (int j = 0; j < BucketCount; j++)
                    histograms[i][j] /= sum;

            /*Outer multiplication*/
            double[][] result = new double[BucketCount][];
            for (int i = 0; i < BucketCount; i++)
                result[i] = new double[BucketCount];

            /*Histogram muiltiplication*/
            for (int i = 0; i < BucketCount; i++)
                for (int j = 0; j < BucketCount; j++)
                    result[i][j] = aArray[i]*bArray[j];

            /*Probability calculation*/
            sum = 0;
            for (int i = 0; i < BucketCount; i++)
                for (int j = 0; j < BucketCount; j++)
                    if (result[i][j] > 0)
                        if (histograms[i][j] != 0)
                            sum += histograms[i][j]*Math.Log(histograms[i][j]/result[i][j], 2);
            return sum;
        }

        public static double MutualInformation(int[] p, int[] groupMember)
        {
            return MutualInformation(Array.ConvertAll(p, (item) => (double) item),
                Array.ConvertAll(groupMember, (item) => (double) item));
        }
    }
}