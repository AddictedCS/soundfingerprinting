namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;

    internal interface IQueryMath
    {
        double CalculateExactSnippetLength(IEnumerable<HashedFingerprint> hashedFingerprints, FingerprintConfiguration fingerprintConfiguration);

        List<ResultEntry> GetBestCandidates(IDictionary<IModelReference, int> hammingSimilarites, int numberOfCandidatesToReturn, IModelService modelService);
    }
}