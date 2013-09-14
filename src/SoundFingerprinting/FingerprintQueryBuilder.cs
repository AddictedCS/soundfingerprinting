namespace SoundFingerprinting
{
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query;

    public class FingerprintQueryBuilder : IFingerprintQueryBuilder
    {
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        public FingerprintQueryBuilder()
            : this(DependencyResolver.Current.Get<IFingerprintUnitBuilder>(), DependencyResolver.Current.Get<IQueryFingerprintService>())
        {
            // no op
        }

        public FingerprintQueryBuilder(IFingerprintUnitBuilder fingerprintUnitBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        public IQuerySource BuildQuery()
        {
            return new FingerprintingQueryUnit(fingerprintUnitBuilder, queryFingerprintService);
        }
    }
}