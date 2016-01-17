namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;

    public interface IAudioSequencesAnalyzer
    {
        IEnumerable<IEnumerable<SubFingerprintData>> GetLongestIncreasingSubSequence(List<SubFingerprintData> sequence);
    }
}