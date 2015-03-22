namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public class AudioSequencesAnalyzer : IAudioSequencesAnalyzer
    {
        private const double Delta = 2 * 1.48;

        public IEnumerable<SubFingerprintData> GetLongestIncreasingSubSequence(List<SubFingerprintData> sequence)
        {
            int[] maxs = new int[sequence.Count];
            maxs[0] = 1;
            int maxLen = 0;
            var longestSubSequence = new List<SubFingerprintData> { sequence.First() };
            for (int i = 1; i < sequence.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (sequence[i].SequenceAt > sequence[j].SequenceAt
                        && sequence[i].SequenceAt - sequence[j].SequenceAt <= Delta)
                    {
                        maxs[i] = System.Math.Max(maxs[j] + 1, maxs[i]);
                        if (maxLen < maxs[i])
                        {
                            longestSubSequence.Add(sequence[i]);
                            maxLen = maxs[i];
                        }
                    }
                }
            }

            return longestSubSequence;
        } 
        
        public double ComputeDynamicTimeWarpingSimilarity(List<HashedFingerprint> hashedFingerprints, List<SubFingerprintData> subFingerprints)
        {
            double offset = hashedFingerprints.First().Timestamp - subFingerprints.First().SequenceAt;
            int n = hashedFingerprints.Count;
            int m = subFingerprints.Count;
            double[][] dtw = InitializeDTW(n, m);
            int window = System.Math.Max(5, System.Math.Abs(n - m));
            for (int i = 1; i <= n; i++)
            {
                for (int j = System.Math.Max(1, i - window); j <= System.Math.Min(m, i + window); j++)
                {
                    double cost = Cost(hashedFingerprints[i - 1].Timestamp, subFingerprints[j - 1].SequenceAt, offset);
                    dtw[i][j] = cost + Min(dtw[i - 1][j], dtw[i][j - 1], dtw[i - 1][j - 1]);
                }
            }

            return dtw[n][m];
        }

        private double Cost(double x, double y, double offset)
        {
            return System.Math.Abs(offset + y - x);
        }

        private double Min(double a, double b, double c)
        {
            return System.Math.Min(a, System.Math.Min(b, c));
        }

        private double[][] InitializeDTW(int n, int m)
        {
            double[][] dtw = new double[n + 1][];
            for (int i = 0; i <= n; i++)
            {
                dtw[i] = new double[m + 1];
            }

            for (int i = 0; i <= n; i++)
            {
                for (int j = 0; j <= m; j++)
                {
                    dtw[i][j] = double.MaxValue;
                }
            }

            dtw[0][0] = 0;
            return dtw;
        }
    }
}
