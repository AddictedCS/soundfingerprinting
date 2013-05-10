namespace Soundfingerprinting.Query
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Query.Configuration;

    public interface IQueryFingerprintService
    {
        Task<QueryResult> Query(IEnumerable<bool[]> fingerprints, IQueryConfiguration queryConfiguration);
    }
}