namespace SoundFingerprinting.Utils
{
    using System.IO;
    using System.Text;

    using SoundFingerprinting.Math;
    using SoundFingerprinting.Strides;

    internal class TestRunnerWriter
    {
        private const string Header = "Query Track,Result Track,Match,Hamming Distance,Track Candidates,Result ISRC,Match Length, Match Start";

        public static StringBuilder Start()
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

        public static void Finish(StringBuilder sb, FScore score, long elapsedMiliseconds)
        {
            sb.AppendLine();
            sb.AppendLine(string.Format("Results: {0}. Elapsed Seconds: {1}", score, (double)elapsedMiliseconds / 1000));
        }

        public static void SaveToFolder(StringBuilder sb, string resultsFolder, IStride queryStride, string insertMetadata, int queryLength, int startAt)
        {
            string filename = string.Format("results_{0}_{1}_q{2}s_at{3}s.csv", insertMetadata, queryStride.ToString(), queryLength, startAt);
            string absolutePath = Path.Combine(resultsFolder, filename);
            using (var writer = new StreamWriter(absolutePath))
            {
                writer.Write(sb.ToString());
                writer.Close();
            }
        }
    }
}
