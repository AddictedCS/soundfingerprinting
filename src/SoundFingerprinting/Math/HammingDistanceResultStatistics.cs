namespace SoundFingerprinting.Math
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class HammingDistanceResultStatistics
    {
        private const double Epsilon = 0.001;

        public double TruePositivesAvg { get; set; }

        public int TruePositiveMin { get; set; }

        public int TruePositiveMax { get; set; }

        public double FalseNegativesAvg { get; set; }

        public int FalseNegativesMin { get; set; }

        public int FalseNegativesMax { get; set; }

        public double FalsePositiveAvg { get; set; }

        public int FalsePositiveMin { get; set; }

        public int FalsePositiveMax { get; set; }

        public double[] TruePositivePercentile { get; set; }

        public double[] FalseNegativePercentile { get; set; }

        public double[] FalsePositivePercentile { get; set; }

        public string TruePositiveInfo
        {
            get
            {
                return string.Format("Avg {0:0.00}  Min {1} Max {2}", TruePositivesAvg, TruePositiveMin, TruePositiveMax);
            }
        }

        public string FalseNegativesInfo
        {
            get
            {
                return string.Format("Avg {0:0.00} Min {1} Max {2}", FalseNegativesAvg, FalseNegativesMin, FalseNegativesMax);
            }
        }

        public string FalsePositivesInfo
        {
            get
            {
                return string.Format("Avg {0:0.00} Min {1} Max {2}", FalsePositiveAvg, FalsePositiveMin, FalsePositiveMax);
            }
        }

        public string TruePositivePercentileInfo
        {
            get
            {
                return string.Join(" ", TruePositivePercentile.Select(p => string.Format("{0:0.00}", p)).ToList());
            }
        }

        public string FalseNegativesPercentileInfo
        {
            get
            {
                return string.Join(" ", FalseNegativePercentile.Select(p => string.Format("{0:0.00}", p)).ToList());
            }
        }

        public string FalsePositivesPercentileInfo
        {
            get
            {
                return string.Join(" ", FalsePositivePercentile.Select(p => string.Format("{0:0.00}", p)).ToList());
            }
        }

        public override string ToString()
        {
            return
                string.Format(
                    "True Positives: [{0}], False Negatives: [{1}], False Positives: [{2}], True Positives(0.8, 0.9, 0.95, 0.98): [{3}], "
                    + "False Negatives(0.8, 0.9, 0.95, 0.98): [{4}], False Positives(0.8, 0.9, 0.95, 0.98): [{5}]",
                    TruePositiveInfo,
                    FalseNegativesInfo,
                    FalsePositivesInfo,
                    TruePositivePercentileInfo,
                    FalseNegativesPercentileInfo,
                    FalsePositivesPercentileInfo);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static HammingDistanceResultStatistics From(ConcurrentBag<int> truePositivesBag, ConcurrentBag<int> falseNegativesBag, ConcurrentBag<int> falsePositivesBag, double[] percentiles)
        {
            var truePositives = truePositivesBag.ToList();
            truePositives.Sort();
            var falseNegatives = falseNegativesBag.ToList();
            falseNegatives.Sort();
            var falsePositives = falsePositivesBag.ToList();
            falsePositives.Sort();

            var instance = new HammingDistanceResultStatistics()
                {
                    TruePositivesAvg = truePositives.DefaultIfEmpty(0).Average(),
                    TruePositiveMin = truePositives.DefaultIfEmpty(0).Min(),
                    TruePositiveMax = truePositives.DefaultIfEmpty(0).Max(),
                    FalseNegativesAvg = falseNegatives.DefaultIfEmpty(0).Average(),
                    FalseNegativesMin = falseNegatives.DefaultIfEmpty(0).Min(),
                    FalseNegativesMax = falseNegatives.DefaultIfEmpty(0).Max(),
                    FalsePositiveAvg = falsePositives.DefaultIfEmpty(0).Average(),
                    FalsePositiveMin = falsePositives.DefaultIfEmpty(0).Min(),
                    FalsePositiveMax = falsePositives.DefaultIfEmpty(0).Max(),
                    TruePositivePercentile = Percentiles(truePositives, percentiles),
                    FalseNegativePercentile = Percentiles(falseNegatives, percentiles),
                    FalsePositivePercentile = Percentiles(falsePositives, percentiles)
                };

            return instance;
        }

        private static double[] Percentiles(List<int> sortedSequence, IEnumerable<double> percentiles)
        {
            return percentiles.Select(p => Percentile(sortedSequence, p)).ToArray();
        }

        private static double Percentile(IReadOnlyList<int> sortedSequence, double excelPercentile)
        {
            if (sortedSequence.Count == 0)
            {
                return 0d;
            }

            int length = sortedSequence.Count;
            double n = ((length - 1) * excelPercentile) + 1;
            if (Math.Abs(n - 1d) < Epsilon)
            {
                return sortedSequence[0];
            }

            if (Math.Abs(n - length) < Epsilon)
            {
                return sortedSequence[length - 1];
            }

            int k = (int)n;
            double d = n - k;
            return sortedSequence[k - 1] + (d * (sortedSequence[k] - sortedSequence[k - 1]));
        }
    }
}
