namespace Soundfingerprinting
{
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Infrastructure;
    using Soundfingerprinting.Query;
    using Soundfingerprinting.Query.Internal;

    public class FingerprintQueryBuilder : IFingerprintQueryBuilder
    {
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly IMinHashService minHashService;

        public FingerprintQueryBuilder()
            : this(
                DependencyResolver.Current.Get<IFingerprintUnitBuilder>(),
                DependencyResolver.Current.Get<IQueryFingerprintService>(),
                DependencyResolver.Current.Get<IMinHashService>())
        {
        }

        public FingerprintQueryBuilder(IFingerprintUnitBuilder fingerprintUnitBuilder, IQueryFingerprintService queryFingerprintService, IMinHashService minHashService)
        {
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
            this.queryFingerprintService = queryFingerprintService;
            this.minHashService = minHashService;
        }

        public IOngoingQuery BuildQuery()
        {
            return new FingerprintingQueryUnit(fingerprintUnitBuilder, queryFingerprintService, minHashService);
        }
    }
}