namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public interface IQueryFingerprintService
    {
        QueryResult Query(IModelService modelService, IEnumerable<HashedFingerprint> hashes, IQueryConfiguration queryConfiguration);
    }
}