namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO.Data;

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
                    if (sequence[i].SequenceAt > sequence[j].SequenceAt && sequence[i].SequenceAt - sequence[j].SequenceAt <= Delta)
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
    }
}
