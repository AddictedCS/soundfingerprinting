namespace SoundFingerprinting
{
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query;

    public class FingerprintQueryBuilder : IFingerprintQueryBuilder
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        public FingerprintQueryBuilder()
            : this(DependencyResolver.Current.Get<IFingerprintCommandBuilder>(), DependencyResolver.Current.Get<IQueryFingerprintService>())
        {
            // no op
        }

        public FingerprintQueryBuilder(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        public IQuerySource BuildQuery()
        {
            return new FingerprintingQueryCommand(fingerprintCommandBuilder, queryFingerprintService);
        }
    }
}