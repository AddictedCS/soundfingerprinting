namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IScoreAlgorithm
    {
        double GetScore(HashedFingerprint queryPoint, SubFingerprintData databasePoint, QueryConfiguration queryConfiguration);
    }
}