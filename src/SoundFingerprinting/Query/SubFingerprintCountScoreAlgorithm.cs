namespace SoundFingerprinting.Query;

using SoundFingerprinting.Configuration;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Data;

internal class SubFingerprintCountScoreAlgorithm : IScoreAlgorithm
{
    public static IScoreAlgorithm Instance { get; } = new SubFingerprintCountScoreAlgorithm();
    
    public double GetScore(HashedFingerprint queryPoint, SubFingerprintData databasePoint, QueryConfiguration queryConfiguration)
    {
        // all sub-fingerprints are equal, this socialist approach makes any candidate worthy to be selected (disregarding quality issues with the query fingerprints)
        // sub-fingerprint count will just sum up the number of sub-fingerprints and will consider best candidate the one that contains the most of them
        return 1;
    }
}