namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal interface IQueryMath
    {
        List<ResultEntry> GetBestCandidates(List<HashedFingerprint> hashedFingerprints, GroupedQueryResults groupedQueryResults, int maxNumberOfMatchesToReturn, IModelService modelService, FingerprintConfiguration fingerprintConfiguration);

        bool IsCandidatePassingThresholdVotes(HashedFingerprint queryFingerprint, SubFingerprintData candidate, int thresholdVotes);

        double CalculateExactQueryLength(IEnumerable<HashedFingerprint> hashedFingerprints, FingerprintConfiguration fingerprintConfiguration);
    }
}