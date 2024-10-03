namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;

    internal class HammingSimilarityScoreAlgorithm(ISimilarityUtility similarityUtility) : IScoreAlgorithm
    {
        public static IScoreAlgorithm Instance { get; } = new HammingSimilarityScoreAlgorithm(SimilarityUtility.Instance);

        public double GetScore(HashedFingerprint queryPoint, SubFingerprintData databasePoint, QueryConfiguration queryConfiguration)
        {
            int hashesPerTable = queryConfiguration.FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable;
            return similarityUtility.CalculateHammingSimilarity(queryPoint.HashBins, databasePoint.Hashes, hashesPerTable);
        }
    }
}