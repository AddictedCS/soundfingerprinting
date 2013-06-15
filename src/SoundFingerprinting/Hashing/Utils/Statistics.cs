namespace SoundFingerprinting.Hashing.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///   Extension methods to return basic statistics on set of data.
    /// </summary>
    public static class Statistics
    {
        /// <summary>
        ///   Calculates the sample mean.
        /// </summary>
        /// <param name = "data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double> data)
        {
            double mean = 0;
            ulong m = 0;
            foreach (double item in data)
            {
                mean += (item - mean) / ++m;
            }

            return mean;
        }

        /// <summary>
        ///   Calculates the sample mean.
        /// </summary>
        /// <param name = "data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double?> data)
        {
            double mean = 0;
            ulong m = 0;
            foreach (double? item in data)
            {
                if (item.HasValue)
                {
                    mean += (item.Value - mean) / ++m;
                }
            }

            return mean;
        }

        /// <summary>
        ///   Calculates the unbiased population variance estimator (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name = "data">The data to calculate the variance of.</param>
        /// <returns>The unbiased population variance of the sample.</returns>
        public static double Variance(this IEnumerable<double> data)
        {
            double variance = 0;
            double t = 0;
            ulong j = 0;

            IEnumerator<double> iterator = data.GetEnumerator();
            if (iterator.MoveNext())
            {
                j++;
                t = iterator.Current;
            }

            while (iterator.MoveNext())
            {
                j++;
                double xi = iterator.Current;
                t += xi;
                double diff = (j * xi) - t;
                variance += (diff * diff) / (j * (j - 1));
            }

            return variance / (j - 1);
        }

        /// <summary>
        ///   Computes the unbiased population variance estimator (on a dataset of size N will use an N-1 normalizer) for nullable data.
        /// </summary>
        /// <param name = "data">The data to calculate the variance of.</param>
        /// <returns>The population variance of the sample.</returns>
        public static double Variance(this IEnumerable<double?> data)
        {
            double variance = 0;
            double t = 0;
            ulong j = 0;

            IEnumerator<double?> iterator = data.GetEnumerator();

            while (true)
            {
                bool hasNext = iterator.MoveNext();
                if (!hasNext)
                {
                    break;
                }

                if (iterator.Current.HasValue)
                {
                    j++;
                    t = iterator.Current.Value;
                    break;
                }
            }

            while (iterator.MoveNext())
            {
                if (iterator.Current.HasValue)
                {
                    j++;
                    double xi = iterator.Current.Value;
                    t += xi;
                    double diff = (j * xi) - t;
                    variance += (diff * diff) / (j * (j - 1));
                }
            }

            return variance / (j - 1);
        }

        /// <summary>
        ///   Calculates the biased population variance estimator (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name = "data">The data to calculate the variance of.</param>
        /// <returns>The biased population variance of the sample.</returns>
        public static double PopulationVariance(this IEnumerable<double> data)
        {
            double variance = 0;
            double t = 0;
            ulong j = 0;

            IEnumerator<double> iterator = data.GetEnumerator();
            if (iterator.MoveNext())
            {
                j++;
                t = iterator.Current;
            }

            while (iterator.MoveNext())
            {
                j++;
                double xi = iterator.Current;
                t += xi;
                double diff = (j * xi) - t;
                variance += (diff * diff) / (j * (j - 1));
            }

            return variance / j;
        }

        /// <summary>
        ///   Computes the biased population variance estimator (on a dataset of size N will use an N normalizer) for nullable data.
        /// </summary>
        /// <param name = "data">The data to calculate the variance of.</param>
        /// <returns>The population variance of the sample.</returns>
        public static double PopulationVariance(this IEnumerable<double?> data)
        {
            double variance = 0;
            double t = 0;
            ulong j = 0;

            IEnumerator<double?> iterator = data.GetEnumerator();

            while (true)
            {
                bool hasNext = iterator.MoveNext();
                if (!hasNext)
                {
                    break;
                }

                if (iterator.Current.HasValue)
                {
                    j++;
                    t = iterator.Current.Value;
                    break;
                }
            }

            while (iterator.MoveNext())
            {
                if (iterator.Current.HasValue)
                {
                    j++;
                    double xi = iterator.Current.Value;
                    t += xi;
                    double diff = (j * xi) - t;
                    variance += (diff * diff) / (j * (j - 1));
                }
            }

            return variance / j;
        }

        /// <summary>
        ///   Calculates the unbiased sample standard deviation (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name = "data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double StandardDeviation(this IEnumerable<double> data)
        {
            return Math.Sqrt(Variance(data));
        }

        /// <summary>
        ///   Calculates the unbiased sample standard deviation (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name = "data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double StandardDeviation(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Math.Sqrt(Variance(data));
        }

        /// <summary>
        ///   Calculates the biased sample standard deviation (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name = "data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double PopulationStandardDeviation(this IEnumerable<double> data)
        {
            return Math.Sqrt(PopulationVariance(data));
        }

        /// <summary>
        ///   Calculates the biased sample standard deviation (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name = "data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double PopulationStandardDeviation(this IEnumerable<double?> data)
        {
            return Math.Sqrt(PopulationVariance(data));
        }

        /// <summary>
        ///   Returns the minimum value in the sample data.
        /// </summary>
        /// <param name = "data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double Minimum(this IEnumerable<double?> data)
        {
            double min = double.MaxValue;
            ulong count = 0;
            foreach (double? d in data)
            {
                if (d.HasValue)
                {
                    min = Math.Min(min, d.Value);
                    count++;
                }
            }

            if (count == 0)
            {
                throw new ArgumentException("CollectionEmpty");
            }

            return min;
        }

        /// <summary>
        ///   Returns the maximum value in the sample data.
        /// </summary>
        /// <param name = "data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double Maximum(this IEnumerable<double?> data)
        {
            double max = double.MinValue;
            ulong count = 0;
            foreach (double? d in data)
            {
                if (d.HasValue)
                {
                    max = Math.Max(max, d.Value);
                    count++;
                }
            }

            if (count == 0)
            {
                throw new ArgumentException("CollectionEmpty");
            }

            return max;
        }

        /// <summary>
        ///   Returns the minimum value in the sample data.
        /// </summary>
        /// <param name = "data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double Minimum(this IEnumerable<double> data)
        {
            double min = double.MaxValue;
            ulong count = 0;
            foreach (double d in data)
            {
                min = Math.Min(min, d);
                count++;
            }

            if (count == 0)
            {
                throw new ArgumentException("CollectionEmpty");
            }

            return min;
        }

        /// <summary>
        ///   Returns the maximum value in the sample data.
        /// </summary>
        /// <param name = "data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double Maximum(this IEnumerable<double> data)
        {
            double max = double.MinValue;
            ulong count = 0;
            foreach (double d in data)
            {
                max = Math.Max(max, d);
                count++;
            }

            if (count == 0)
            {
                throw new ArgumentException("CollectionEmpty");
            }

            return max;
        }

        /// <summary>
        ///   Calculates the sample median.
        /// </summary>
        /// <param name = "data">The data to calculate the median of.</param>
        /// <returns>The median of the sample.</returns>
        public static double Median(this IEnumerable<double> data)
        {
            var order = data.OrderBy(item => item);
            float result;
            int length = order.Count();
            if (length % 2 == 0)
            {
                int middle = length / 2;
                result = (float)(order.ElementAt(middle) + order.ElementAt(middle - 1)) / 2;
            }
            else
            {
                result = (float)order.ElementAt(length / 2);
            }

            return result;
        }

        /// <summary>
        ///   Calculates the sample median.
        /// </summary>
        /// <param name = "data">The data to calculate the median of.</param>
        /// <returns>The median of the sample.</returns>
        public static double Median(this IEnumerable<double?> data)
        {
            List<double> nonNull = new List<double>();
            foreach (double? value in data)
            {
                if (value.HasValue)
                {
                    nonNull.Add(value.Value);
                }
            }

            if (nonNull.Count == 0)
            {
                throw new ArgumentException("CollectionEmpty");
            }

            return nonNull.Median();
        }
    }
}