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
                return $"Avg {TruePositivesAvg:0.00}  Min {TruePositiveMin} Max {TruePositiveMax}";
            }
        }

        public string FalseNegativesInfo
        {
            get
            {
                return $"Avg {FalseNegativesAvg:0.00} Min {FalseNegativesMin} Max {FalseNegativesMax}";
            }
        }

        public string FalsePositivesInfo
        {
            get
            {
                return $"Avg {FalsePositiveAvg:0.00} Min {FalsePositiveMin} Max {FalsePositiveMax}";
            }
        }

        public string TruePositivePercentileInfo
        {
            get
            {
                return string.Join(" ", TruePositivePercentile.Select(p => $"{p:0.00}").ToList());
            }
        }

        public string FalseNegativesPercentileInfo
        {
            get
            {
                return string.Join(" ", FalseNegativePercentile.Select(p => $"{p:0.00}").ToList());
            }
        }

        public string FalsePositivesPercentileInfo
        {
            get
            {
                return string.Join(" ", FalsePositivePercentile.Select(p => $"{p:0.00}").ToList());
            }
        }

        public override string ToString()
        {
            return
                $"True Positives: [{TruePositiveInfo}], False Negatives: [{FalseNegativesInfo}], False Positives: [{FalsePositivesInfo}], True Positives(0.8, 0.9, 0.95, 0.98): [{TruePositivePercentileInfo}], "
                + $"False Negatives(0.8, 0.9, 0.95, 0.98): [{FalseNegativesPercentileInfo}], False Positives(0.8, 0.9, 0.95, 0.98): [{FalsePositivesPercentileInfo}]";
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
