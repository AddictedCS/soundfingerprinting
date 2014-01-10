namespace SoundFingerprinting
{
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query;

    public class QueryCommandBuilder : IQueryCommandBuilder
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        public QueryCommandBuilder()
            : this(DependencyResolver.Current.Get<IFingerprintCommandBuilder>(), DependencyResolver.Current.Get<IQueryFingerprintService>())
        {
            // no op
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