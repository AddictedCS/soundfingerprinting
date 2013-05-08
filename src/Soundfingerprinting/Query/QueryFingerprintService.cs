namespace Soundfingerprinting.Query
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Hashing;

    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly ICombinedHashingAlgoritm hashingAlgorithm;
        private readonly IModelService modelService;

        public QueryFingerprintService(ICombinedHashingAlgoritm hashingAlgorithm, IModelService modelService)
        {
            this.hashingAlgorithm = hashingAlgorithm;
            this.modelService = modelService;
        }

        public Task<QueryResult> Query(IEnumerable<bool[]> fingerprints, IQueryConfiguration queryConfiguration)
        {
            // no op
            return null;
        }
    }
}
