namespace Soundfingerprinting.Query
{
    using System.Collections.Generic;

    using Soundfingerprinting.Query.Configuration;

    public interface IQueryFingerprintService
    {
        QueryResult Query(IEnumerable<bool[]> fingerprints, IQueryConfiguration queryConfiguration);
    }
}