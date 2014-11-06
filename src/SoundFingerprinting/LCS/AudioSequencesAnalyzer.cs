namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public class AudioSequencesAnalyzer : IAudioSequencesAnalyzer
    {
        public IEnumerable<SubFingerprintData> GetLongestIncreasingSubSequence(
            IEnumerable<SubFingerprintData> sequence)
        {
            return null;
        }
    }

    public interface IAudioSequencesAnalyzer
    {
        IEnumerable<SubFingerprintData> GetLongestIncreasingSubSequence(IEnumerable<SubFingerprintData> sequence);
    }
}
