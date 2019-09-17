namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IScoreAlgorithm
    {
        int GetScore(HashedFingerprint queryPoint, SubFingerprintData databasePoint, QueryConfiguration queryConfiguration);
    }
}