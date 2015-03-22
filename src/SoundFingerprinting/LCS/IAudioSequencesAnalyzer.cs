namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IAudioSequencesAnalyzer
    {
        IEnumerable<SubFingerprintData> GetLongestIncreasingSubSequence(List<SubFingerprintData> sequence);

        double ComputeDynamicTimeWarpingSimilarity(List<HashedFingerprint> hashedFingerprints, List<SubFingerprintData> subFingerprints);
    }
}