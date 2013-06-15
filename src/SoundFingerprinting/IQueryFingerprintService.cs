namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Query;
    using SoundFingerprinting.Query.Configuration;

    using IQueryConfiguration = SoundFingerprinting.Query.Configuration.IQueryConfiguration;

    public interface IQueryFingerprintService
    {
        QueryResult Query(IEnumerable<bool[]> fingerprints, IQueryConfiguration queryConfiguration);
    }
}