namespace SoundFingerprinting.Query
{
    using System;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;

    [Obsolete("Scores will be removed in v9 as they do not mean a lot, counting bytes is not a right approach")]
    internal class HammingSimilarityScoreAlgorithm : IScoreAlgorithm
    {
        private readonly ISimilarityUtility similarityUtility;

        public HammingSimilarityScoreAlgorithm(ISimilarityUtility similarityUtility)
        {
            this.similarityUtility = similarityUtility;
        }

        public double GetScore(HashedFingerprint queryPoint, SubFingerprintData databasePoint, QueryConfiguration queryConfiguration)
        {
            int hashesPerTable = queryConfiguration.FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable;
            return similarityUtility.CalculateHammingSimilarity(queryPoint.HashBins, databasePoint.Hashes, hashesPerTable);
        }
    }
}