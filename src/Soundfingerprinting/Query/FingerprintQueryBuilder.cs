namespace Soundfingerprinting.Query
{
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.Query.Internal;

    public class FingerprintQueryBuilder : IFingerprintQueryBuilder
    {
        private readonly IFingerprintingUnitsBuilder fingerprintingUnitsBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        public FingerprintQueryBuilder(IFingerprintingUnitsBuilder fingerprintingUnitsBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintingUnitsBuilder = fingerprintingUnitsBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        public IOngoingQuery BuildQuery()
        {
            return new FingerprintingQueryUnit(fingerprintingUnitsBuilder, queryFingerprintService);
        }
    }
}