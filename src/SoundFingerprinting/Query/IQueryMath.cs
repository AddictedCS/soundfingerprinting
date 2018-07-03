namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal interface IQueryMath
    {
        List<ResultEntry> GetBestCandidates(GroupedQueryResults groupedQueryResults, int maxNumberOfMatchesToReturn, IModelService modelService, QueryConfiguration queryConfiguration);

        bool IsCandidatePassingThresholdVotes(HashedFingerprint queryFingerprint, SubFingerprintData candidate, int thresholdVotes);
    }
}