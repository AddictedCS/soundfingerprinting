namespace SoundFingerprinting.Query;

using SoundFingerprinting.Configuration;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Data;

internal class SubFingerprintCountScoreAlgorithm : IScoreAlgorithm
{
    public double GetScore(HashedFingerprint queryPoint, SubFingerprintData databasePoint, QueryConfiguration queryConfiguration)
    {
        // sub-fingerprint count will just sum up the number of sub-fingerprints and will consider best candidate the one that contains the most of them
        return 1;
    }
}