namespace SoundFingerprinting.Query
{
    using System.Linq;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public class ThresholdVotesScoreAlgorithm : IScoreAlgorithm
    {
        public double GetScore(HashedFingerprint queryPoint, SubFingerprintData databasePoint, QueryConfiguration queryConfiguration)
        {
            return queryPoint.HashBins.Where((t, i) => t == databasePoint.Hashes[i]).Count();
        }
    }
}