namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Infrastructure;

    public class QueryCommandBuilder : IQueryCommandBuilder
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        public QueryCommandBuilder()
            : this(DependencyResolver.Current.Get<IFingerprintCommandBuilder>(), DependencyResolver.Current.Get<IQueryFingerprintService>())
        {
        }

        public QueryCommandBuilder(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        public IQuerySource BuildQueryCommand()
        {
            return new QueryCommand(fingerprintCommandBuilder, queryFingerprintService);
        }
    }
}