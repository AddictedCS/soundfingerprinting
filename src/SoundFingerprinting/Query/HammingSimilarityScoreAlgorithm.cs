namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;

    public class HammingSimilarityScoreAlgorithm : IScoreAlgorithm
    {
        private readonly ISimilarityUtility similarityUtility;

        public HammingSimilarityScoreAlgorithm(ISimilarityUtility similarityUtility)
        {
            this.similarityUtility = similarityUtility;
        }

        public int GetScore(HashedFingerprint queryPoint, SubFingerprintData databasePoint, QueryConfiguration queryConfiguration)
        {
            int hashesPerTable = queryConfiguration.FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable;
            return similarityUtility.CalculateHammingSimilarity(queryPoint.HashBins, databasePoint.Hashes, hashesPerTable);
        }
    }
}

    