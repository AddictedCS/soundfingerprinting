namespace Soundfingerprinting.Query
{
    using Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Infrastructure;
    using Soundfingerprinting.Query.Internal;

    public class FingerprintQueryBuilder : IFingerprintQueryBuilder
    {
        private readonly IFingerprintingUnitsBuilder fingerprintingUnitsBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly IMinHashService minHashService;

        public FingerprintQueryBuilder()
            : this(
                DependencyResolver.Current.Get<IFingerprintingUnitsBuilder>(),
                DependencyResolver.Current.Get<IQueryFingerprintService>(),
                DependencyResolver.Current.Get<IMinHashService>())
        {
        }

        public FingerprintQueryBuilder(IFingerprintingUnitsBuilder fingerprintingUnitsBuilder, IQueryFingerprintService queryFingerprintService, IMinHashService minHashService)
        {
            this.fingerprintingUnitsBuilder = fingerprintingUnitsBuilder;
            this.queryFingerprintService = queryFingerprintService;
            this.minHashService = minHashService;
        }

        public IOngoingQuery BuildQuery()
        {
            return new FingerprintingQueryUnit(fingerprintingUnitsBuilder, queryFingerprintService, minHashService);
        }
    }
}