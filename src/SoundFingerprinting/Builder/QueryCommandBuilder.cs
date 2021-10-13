namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    /// <summary>
    ///  Query command builder.
    /// </summary>
    public class QueryCommandBuilder : IQueryCommandBuilder
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        /// <summary>
        ///  Gets an instance of <see cref="QueryCommandBuilder"/>.
        /// </summary>
        public static IQueryCommandBuilder Instance { get; } = new QueryCommandBuilder();
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="QueryCommandBuilder"/> class.
        /// </summary>
        public QueryCommandBuilder() : this(FingerprintCommandBuilder.Instance, QueryFingerprintService.Instance)
        {
            // no op
        }

        internal QueryCommandBuilder(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        /// <inheritdoc cref="IQueryCommandBuilder.BuildQueryCommand"/>
        public IQuerySource BuildQueryCommand()
        {
            return new QueryCommand(fingerprintCommandBuilder, queryFingerprintService);
        }

        /// <inheritdoc cref="IQueryCommandBuilder.BuildRealtimeQueryCommand"/>
        public IRealtimeSource BuildRealtimeQueryCommand()
        {
            return new RealtimeQueryCommand(fingerprintCommandBuilder, queryFingerprintService);
        }
    }
}