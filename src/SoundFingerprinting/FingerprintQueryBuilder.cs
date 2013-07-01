namespace SoundFingerprinting
{
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query;

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
            // no op
        }

        public FingerprintQueryBuilder(IFingerprintUnitBuilder fingerprintUnitBuilder, IQueryFingerprintService queryFingerprintService, IMinHashService minHashService)
        {
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
            this.queryFingerprintService = queryFingerprintService;
            this.minHashService = minHashService;
        }

        public IQuerySource BuildQuery()
        {
            return new FingerprintingQueryUnit(fingerprintUnitBuilder, queryFingerprintService, minHashService);
        }
    }
}