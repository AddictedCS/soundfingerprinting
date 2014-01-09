namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Query.Configuration;

    public interface IQueryFingerprintService
    {
        QueryResult Query(IEnumerable<HashData> hashes, IQueryConfiguration queryConfiguration);
    }
}