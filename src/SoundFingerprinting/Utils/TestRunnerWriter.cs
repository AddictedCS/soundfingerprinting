namespace SoundFingerprinting.Utils
{
    using System;
    using System.IO;
    using System.Text;

    using SoundFingerprinting.Math;
    using SoundFingerprinting.Strides;

    internal class TestRunnerWriter
    {
        private const string Header = "Query Track,Result Track,Match,Hamming Distance,Track Candidates,Result ISRC,Match Length, Match Start";

        private const string HeaderFinalResult =
            "Inserted As,Query Stride,Query Seconds,Start At,Precision,Recall,F1,Elapsed Time (sec)";

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

        public static void FinishTestIteration(StringBuilder sb, FScore score, long elapsedMiliseconds)
        {
            sb.AppendLine();
            sb.AppendLine(string.Format("Results: {0}. Elapsed Seconds: {1}", score, (double)elapsedMiliseconds / 1000));
        }

        public static void SaveTestIterationToFolder(StringBuilder sb, string resultsFolder, IStride queryStride, string insertMetadata, int queryLength, int startAt)
        {
            string filename = string.Format("results_{0}_{1}_q{2}s_at{3}s.csv", insertMetadata, queryStride.ToString(), queryLength, startAt);
            string absolutePath = Path.Combine(resultsFolder, filename);
            Write(sb, absolutePath);
        }
        
        public static void SaveSuiteResultsToFolder(StringBuilder sb, string resultsFolder)
        {
            string finalname = string.Format("suite_{0}.csv", DateTime.Now.Ticks);
            string absolutePath = Path.Combine(resultsFolder, finalname);
            Write(sb, absolutePath);
        }

        private static void Write(StringBuilder sb, string absolutePath)
        {
            using (var writer = new StreamWriter(absolutePath))
            {
                writer.Write(sb.ToString());
                writer.Close();
            }
        }
    }
}
