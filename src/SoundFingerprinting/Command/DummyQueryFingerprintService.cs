namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    internal class DummyQueryFingerprintService : IQueryFingerprintService
    {
        public QueryResult Query(IEnumerable<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            return QueryResult.Empty;
        }
    }
}