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
            int[] ind = new int[sequence.Count];
            for (int i = 0; i < sequence.Count; i++)
            {
                maxs[i] = 1;
            }

            int maxLen = 1;

            for (int i = 1; i < sequence.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (sequence[i].SequenceAt > sequence[j].SequenceAt 
                        && sequence[i].SequenceAt - sequence[j].SequenceAt <= Delta)
                    {
                        if (maxs[j] + 1 > maxs[i])
                        {
                            maxs[i] = maxs[j] + 1;
                            ind[i] = j;
                        }

                        if (maxLen < maxs[i])
                        {
                            maxLen = maxs[i];
                        }
                    }
                }
            }

            List<List<SubFingerprintData>> allCandidates = new List<List<SubFingerprintData>>();
            for (int i = 0; i < maxs.Length; i++)
            {
                if (maxs[i] != maxLen)
                {
                    continue;
                }

                Stack<SubFingerprintData> candidate = new Stack<SubFingerprintData>();
                int last = i;
                for (int k = 0; k < maxLen; k++)
                {
                    candidate.Push(sequence[last]);
                    last = ind[last];
                }

                allCandidates.Add(candidate.ToList());
            }
            
            return allCandidates.First();
        }
    }
}
