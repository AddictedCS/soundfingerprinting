using System.Collections.Generic;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.Data;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Math;
using SoundFingerprinting.Query;

namespace SoundFingerprinting
{
    public class QueryFactory
    {
        private readonly QueryConfiguration _configuration;
        private readonly IQueryFingerprintService _fingerprintService;
        private readonly IModelService _modelService;

        public QueryFactory(IModelService modelService)
        {
            _fingerprintService = new QueryFingerprintService(new SimilarityUtility(),
                new QueryMath(new QueryResultCoverageCalculator(), new ConfidenceCalculator()));

            _configuration = new DefaultQueryConfiguration();
            _modelService = modelService;
        }

        public QueryResult Query(List<HashedFingerprint> queryFingerprints)
        {
            return _fingerprintService.Query(queryFingerprints, _configuration, _modelService);
        }
    }
}