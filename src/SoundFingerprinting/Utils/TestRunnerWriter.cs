namespace SoundFingerprinting.Utils
{
    using System;
    using System.IO;
    using System.Text;

    using Math;
    using Strides;

    internal class TestRunnerWriter
    {
        private const string Header = "Query Track,Result Track,Match,Hamming Distance,Confidence,Coverage,Query Match Length, Starts At";
        private const string HeaderFinalResult = "Inserted As,Query Stride,Query Seconds,Start At,Precision,Recall,F1, TP, TP Percetile, FN, FN Percentile, FP, FP Percentile,Elapsed Time (sec)";
        private const string InsertHeader = "Inserted Tracks, Time (sec)";

        public static StringBuilder StartSuite()
        {
            var sb = new StringBuilder();
            sb.AppendLine(HeaderFinalResult);
            return sb;
        }

        public static StringBuilder StartTestIteration()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            return sb;
        }

        public static StringBuilder StartInsert()
        {
            var sb = new StringBuilder();
            sb.AppendLine(InsertHeader);
            return sb;
        }

        public static void AppendLine(StringBuilder sb, object[] line)
        {
            string[] cells = new string[line.Length];
            for (int i = 0; i < line.Length; i++)
            {
                var cell = line[i];
                if (cell is string)
                {
                    cell = string.Format("\"{0}\"", cell);
                }

                cells[i] = cell.ToString();
            }

            sb.AppendLine(string.Join(",", cells));
        }

        public static void FinishTestIteration(StringBuilder sb, FScore score, HammingDistanceResultStatistics statistics, long elapsedMiliseconds)
        {
            sb.AppendLine();
            sb.AppendLine(string.Format("Results: {0}. Elapsed Seconds: {1}", score, (double)elapsedMiliseconds / 1000));
            sb.AppendLine();
            sb.AppendLine(string.Format("True Positives: {0}", statistics.TruePositiveInfo));
            sb.AppendLine(string.Format("True Positives Percentile: {0}", statistics.TruePositivePercentileInfo));


            sb.AppendLine(string.Format("False Negatives: {0}", statistics.FalseNegativesInfo));
            sb.AppendLine(string.Format("False Negatives Percentile: {0}", statistics.FalseNegativesPercentileInfo));


            sb.AppendLine(string.Format("False Positives: {0}", statistics.FalsePositivesInfo));
            sb.AppendLine(string.Format("False Positives Percentile: {0}", statistics.FalsePositivesPercentileInfo));
        }

        public static void SaveTestIterationToFolder(StringBuilder sb, string resultsFolder, IStride queryStride, string insertMetadata, int queryLength, int startAt)
        {
            string filename = string.Format("results_{0}_{1}_q{2}s_at{3}s.csv", insertMetadata, queryStride, queryLength, startAt);
            string absolutePath = Path.Combine(resultsFolder, filename);
            Write(sb, absolutePath);
        }
        
        public static void SaveSuiteResultsToFolder(StringBuilder sb, string resultsFolder)
        {
            string finalname = string.Format("suite_{0}.csv", DateTime.Now.Ticks);
            string absolutePath = Path.Combine(resultsFolder, finalname);
            Write(sb, absolutePath);
        }

        public static void SaveInsertDataToFolder(StringBuilder sb, string resultsFolder, IStride insertStride)
        {
            string filename = string.Format("insert_{0}.csv", insertStride);
            string absolutePath = Path.Combine(resultsFolder, filename);
            Write(sb, absolutePath);
        }

        private static void Write(StringBuilder sb, string absolutePath)
        {
            using (var writer = new StreamWriter(absolutePath))
            {
                writer.Write(sb.ToString());
            }
        }
    }
}
