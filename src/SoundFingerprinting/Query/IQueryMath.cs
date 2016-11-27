namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal interface IQueryMath
    {
        List<ResultEntry> GetBestCandidates(
            List<HashedFingerprint> hashedFingerprints,
            IDictionary<IModelReference, ResultEntryAccumulator> hammingSimilarites,
            int maxNumberOfMatchesToReturn,
            IModelService modelService,
            FingerprintConfiguration fingerprintConfiguration);

        bool IsCandidatePassingThresholdVotes(HashedFingerprint queryFingerprint, SubFingerprintData candidate, int thresholdVotes);
    }
}